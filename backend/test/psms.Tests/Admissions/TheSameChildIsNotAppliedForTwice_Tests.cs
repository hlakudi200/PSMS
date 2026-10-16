using Abp.UI;
using psms.Admissions.Applications;
using psms.Admissions.Applications.Dto;
using psms.Admissions.Shared;
using psms.Domain.Academic.Entities;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// One child, one application per grade per year.
/// <para>
/// <c>ADM_DUPLICATE_APPLICATION</c> was defined and never thrown, so the same
/// family could apply for the same learner as often as they liked. Mostly that
/// is an accident — a parent who is not sure the first one saved — and it
/// leaves the school deciding the same child twice, each decision counting
/// separately against the grade's places.
/// </para>
/// </summary>
public class TheSameChildIsNotAppliedForTwice_Tests : psmsTestBase
{
    private readonly IApplicationAppService _applications;

    private Guid _yearId;
    private Guid _gradeId;
    private Guid _otherGradeId;

    public TheSameChildIsNotAppliedForTwice_Tests()
    {
        _applications = Resolve<IApplicationAppService>();
        LoginAsDefaultTenantAdmin();
        Seed();
    }

    private void Seed()
    {
        _yearId = Guid.NewGuid();
        _gradeId = Guid.NewGuid();
        _otherGradeId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            context.AcademicYears.Add(new AcademicYear(_yearId, 1, 2027,
                new DateTime(2027, 1, 1), new DateTime(2027, 12, 31)) { YearName = "2027 Academic Year" });
            context.Grades.Add(new Grade(_gradeId, 1, SouthAfricanGradeLevel.Grade8, "Grade 8",
                SouthAfricanSchoolPhase.Senior));
            context.Grades.Add(new Grade(_otherGradeId, 1, SouthAfricanGradeLevel.Grade9, "Grade 9",
                SouthAfricanSchoolPhase.Senior));

            foreach (var gradeId in new[] { _gradeId, _otherGradeId })
                context.AdmissionSettings.Add(new AdmissionSettings(Guid.NewGuid(), 1, _yearId, 0m)
                {
                    GradeId = gradeId,
                    IsAcceptingApplications = true,
                    MinimumAge = 10,
                    MaximumAge = 20,
                });
        });
    }

    /// <summary>A Grade 8 applicant born in 2013, old enough for the seeded window.</summary>
    private CreateApplicationDto Child(
        string first = "Ann", string last = "Adams", string idNumber = null, Guid? gradeId = null) =>
        new CreateApplicationDto
        {
            AcademicYearId = _yearId,
            ApplyingForGradeId = gradeId ?? _gradeId,
            FirstName = first,
            LastName = last,
            DateOfBirth = new DateTime(2013, 4, 1),
            Gender = Gender.Female,
            IsSACitizen = idNumber != null,
            IdNumber = idNumber,
            // A passport identifies one person, so each child gets their own —
            // sharing one would make two different children look like a duplicate,
            // which is the check working, not failing.
            PassportNumber = idNumber == null ? $"A{first}{last}" : null,
            CreatorEmailAddress = "parent@example.test",
        };

    private void SetStatus(Guid id, ApplicationStatus status) =>
        UsingDbContext(1, context =>
        {
            var application = context.Applications.Single(a => a.Id == id);
            context.Entry(application).Property(nameof(Application.Status)).CurrentValue = status;
            context.SaveChanges();
        });

    [Fact]
    public async Task The_first_application_is_fine()
    {
        var created = await _applications.CreateAsync(Child());

        created.ApplicationNumber.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task A_second_one_for_the_same_child_is_refused()
    {
        var first = await _applications.CreateAsync(Child());

        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => _applications.CreateAsync(Child()));

        refusal.Message.ShouldBe(AdmissionsExceptionCodes.DuplicateApplication);

        // Naming the one they already have is the difference between a refusal
        // they can act on and one they can only be annoyed by.
        refusal.Details.ShouldContain(first.ApplicationNumber);
    }

    [Fact]
    public async Task The_identity_document_matches_the_child_even_when_the_name_is_typed_differently()
    {
        const string id = "1004020500089";
        await _applications.CreateAsync(Child(idNumber: id));

        // Same child, name spelled another way. The document is what settles it.
        await Should.ThrowAsync<UserFriendlyException>(
            () => _applications.CreateAsync(Child(first: "Anne", last: "Adams", idNumber: id)));
    }

    [Fact]
    public async Task A_different_child_is_not_a_duplicate()
    {
        await _applications.CreateAsync(Child());

        var sibling = await _applications.CreateAsync(Child(first: "Ben", last: "Adams"));

        sibling.ApplicationNumber.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task The_same_child_may_apply_for_a_different_grade()
    {
        // A family unsure which year group their child belongs in is applying
        // for two different things, not the same thing twice.
        await _applications.CreateAsync(Child());

        var other = await _applications.CreateAsync(Child(gradeId: _otherGradeId));

        other.ApplicationNumber.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task A_withdrawn_application_does_not_block_a_new_one()
    {
        // Withdrawing is an ending. A family that withdrew in March may well
        // mean it in June, and refusing them then would be absurd.
        var first = await _applications.CreateAsync(Child());
        SetStatus(first.Id, ApplicationStatus.Withdrawn);

        var again = await _applications.CreateAsync(Child());

        again.Id.ShouldNotBe(first.Id);
    }

    [Fact]
    public async Task A_rejected_application_does_not_block_a_new_one()
    {
        var first = await _applications.CreateAsync(Child());
        SetStatus(first.Id, ApplicationStatus.Rejected);

        var again = await _applications.CreateAsync(Child());

        again.Id.ShouldNotBe(first.Id);
    }

    [Fact]
    public async Task A_decided_application_still_blocks_one()
    {
        // An offer is not an ending. Applying again on top of a place already
        // offered is the duplicate this is about.
        var first = await _applications.CreateAsync(Child());
        SetStatus(first.Id, ApplicationStatus.Approved);

        await Should.ThrowAsync<UserFriendlyException>(
            () => _applications.CreateAsync(Child()));
    }
}
