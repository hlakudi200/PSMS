using psms.Admissions.Applications;
using psms.Admissions.AdmissionSettings;
using psms.Admissions.AdmissionSettings.Dto;
using psms.Admissions.Applications.Dto;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// An application carries what its own grade asks of an applicant.
/// <para>
/// A Grade 11 intake with the interview and assessment switches off still
/// showed an Interview tab on the application, and a button to arrange one.
/// The workflow had learned to step those stages over; the screens had not, so
/// they offered work the school had said it does not do.
/// </para>
/// <para>
/// The answer rides on the application rather than being looked up by the
/// screen, because reading admission settings needs a permission that somebody
/// looking at an application may not hold.
/// </para>
/// </summary>
public class WhatThisGradeAsksOfApplicants_Tests : psmsTestBase
{
    private readonly IApplicationAppService _applications;
    private readonly IAdmissionSettingsAppService _settings;

    private readonly Guid _year = Guid.NewGuid();
    private readonly Guid _grade11 = Guid.NewGuid();
    private readonly Guid _grade1 = Guid.NewGuid();

    public WhatThisGradeAsksOfApplicants_Tests()
    {
        LoginAsDefaultTenantAdmin();
        _applications = Resolve<IApplicationAppService>();
        _settings = Resolve<IAdmissionSettingsAppService>();

        UsingDbContext(1, context =>
        {
            context.AcademicYears.Add(new AcademicYear(_year, 1, 2027,
                new DateTime(2027, 1, 15), new DateTime(2027, 12, 15)) { YearName = "2027" });
            context.Grades.Add(new Grade(_grade1, 1,
                SouthAfricanGradeLevel.Grade1, "Grade 1", SouthAfricanSchoolPhase.Foundation));
            context.Grades.Add(new Grade(_grade11, 1,
                SouthAfricanGradeLevel.Grade11, "Grade 11", SouthAfricanSchoolPhase.FET));
            context.SaveChanges();
        });
    }

    private async Task OpenAsync(Guid gradeId, bool interview, bool assessment, int minAge, int maxAge) =>
        await _settings.CreateAsync(new CreateAdmissionSettingsDto
        {
            AcademicYearId = _year,
            GradeId = gradeId,
            ApplicationFeeAmount = 0m,
            IsApplicationFeeRequired = false,
            IsAcceptingApplications = true,
            IsInterviewRequired = interview,
            IsAssessmentRequired = assessment,
            MinimumAge = minAge,
            MaximumAge = maxAge,
            OfferExpiryDays = 14,
        });

    private async Task<ApplicationDto> ApplyAsync(Guid gradeId, DateTime dateOfBirth) =>
        await _applications.CreateAsync(new CreateApplicationDto
        {
            AcademicYearId = _year,
            ApplyingForGradeId = gradeId,
            FirstName = "Mogau",
            LastName = "Tau",
            DateOfBirth = dateOfBirth,
            Gender = Gender.Male,
            IsSACitizen = true,
            IdNumber = "1004020500089",
            CreatorEmailAddress = "parent@example.test",
        });

    [Fact]
    public async Task A_grade_that_does_neither_says_so()
    {
        await OpenAsync(_grade11, interview: false, assessment: false, minAge: 15, maxAge: 19);

        var application = await ApplyAsync(_grade11, DateTime.UtcNow.AddYears(-16));

        application.RequiresInterview.ShouldBe(false);
        application.RequiresAssessment.ShouldBe(false);
    }

    [Fact]
    public async Task And_a_grade_that_does_both_says_that()
    {
        await OpenAsync(_grade1, interview: true, assessment: true, minAge: 5, maxAge: 8);

        var application = await ApplyAsync(_grade1, DateTime.UtcNow.AddYears(-6));

        application.RequiresInterview.ShouldBe(true);
        application.RequiresAssessment.ShouldBe(true);
    }

    [Fact]
    public async Task Each_grade_is_answered_on_its_own_terms()
    {
        // The whole point of settings being per grade: Grade 1 interviews,
        // Grade 11 does not, in the same year.
        await OpenAsync(_grade1, interview: true, assessment: false, minAge: 5, maxAge: 8);
        await OpenAsync(_grade11, interview: false, assessment: true, minAge: 15, maxAge: 19);

        var little = await ApplyAsync(_grade1, DateTime.UtcNow.AddYears(-6));
        var older = await ApplyAsync(_grade11, DateTime.UtcNow.AddYears(-16));

        little.RequiresInterview.ShouldBe(true);
        little.RequiresAssessment.ShouldBe(false);
        older.RequiresInterview.ShouldBe(false);
        older.RequiresAssessment.ShouldBe(true);
    }
}
