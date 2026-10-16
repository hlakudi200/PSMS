using psms.Admissions.ApplicationDocuments;
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
/// The list an application is measured against: the school's own where they
/// have set one, and otherwise the sensible default for the grade.
/// <para>
/// Two things were wrong. The school's list was read into a variable and
/// dropped — every application got the hardcoded list whatever the school had
/// said. And the hardcoded list picked its grade branch with
/// <c>!gradeName.ToUpperInvariant().Contains("R")</c>, which is true of no
/// grade at all, because the word GRADE contains an R. So a previous school
/// report has never been asked of anybody.
/// </para>
/// </summary>
public class WhichDocumentsThisGradeAsksFor_Tests : psmsTestBase
{
    private readonly IApplicationDocumentAppService _documents;

    public WhichDocumentsThisGradeAsksFor_Tests()
    {
        _documents = Resolve<IApplicationDocumentAppService>();
        LoginAsDefaultTenantAdmin();
    }

    /// <summary>
    /// An application in <paramref name="level"/>, with the school asking for
    /// <paramref name="schoolAsksFor"/> (null meaning they have not chosen).
    /// </summary>
    private Guid Application(
        SouthAfricanGradeLevel level,
        string schoolAsksFor = null,
        bool isSACitizen = true)
    {
        var yearId = Guid.NewGuid();
        var gradeId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            context.AcademicYears.Add(new AcademicYear(yearId, 1, 2027,
                new DateTime(2027, 1, 1), new DateTime(2027, 12, 31)));
            context.Grades.Add(new Grade(gradeId, 1, level, level.ToString(),
                SouthAfricanSchoolPhase.Senior));
            context.AdmissionSettings.Add(new AdmissionSettings(Guid.NewGuid(), 1, yearId, 0m)
            {
                GradeId = gradeId,
                RequiredDocuments = schoolAsksFor,
            });

            context.Applications.Add(new Application(
                applicationId, 1, $"APP-{applicationId:N}".Substring(0, 16),
                "Ann", "Adams", new DateTime(2013, 4, 1), Gender.Female,
                gradeId, yearId, "parent@example.test")
            {
                IsSACitizen = isSACitizen,
            });
        });

        return applicationId;
    }

    private async Task<DocumentCategory[]> AskedForAsync(Guid applicationId) =>
        (await _documents.GetRequiredDocumentsStatusAsync(applicationId))
            .Documents.Select(d => d.Category).ToArray();

    // ---- the school has chosen ----

    [Fact]
    public async Task The_schools_own_list_is_what_is_asked_for()
    {
        var application = Application(SouthAfricanGradeLevel.Grade8,
            schoolAsksFor: "BirthCertificate,ProofOfResidence");

        (await AskedForAsync(application)).ShouldBe(new[]
        {
            DocumentCategory.BirthCertificate,
            DocumentCategory.ProofOfResidence,
        });
    }

    [Fact]
    public async Task A_study_permit_is_added_to_a_list_that_forgot_it()
    {
        // Not the school's to waive: a non-citizen learner needs one whatever
        // the settings screen says.
        var application = Application(SouthAfricanGradeLevel.Grade8,
            schoolAsksFor: "BirthCertificate", isSACitizen: false);

        (await AskedForAsync(application)).ShouldContain(DocumentCategory.StudyPermit);
    }

    [Fact]
    public async Task Prose_left_over_from_the_old_free_text_box_falls_back_to_the_default()
    {
        /* The real row from the live tenant. It must not be read as "this
           school requires nothing" — which is what an empty list would mean. */
        var application = Application(SouthAfricanGradeLevel.Grade8,
            schoolAsksFor: "Parents ID \nLearners birth certificate \nPrevious school report ");

        var asked = await AskedForAsync(application);

        asked.Length.ShouldBeGreaterThan(0);
        asked.ShouldContain(DocumentCategory.BirthCertificate);
    }

    // ---- the school has not chosen ----

    [Fact]
    public async Task Every_applicant_is_asked_for_the_four_basics()
    {
        var asked = await AskedForAsync(Application(SouthAfricanGradeLevel.Grade8));

        asked.ShouldContain(DocumentCategory.BirthCertificate);
        asked.ShouldContain(DocumentCategory.ParentIdDocument);
        asked.ShouldContain(DocumentCategory.PassportPhoto);
        asked.ShouldContain(DocumentCategory.ProofOfResidence);
    }

    [Theory]
    [InlineData(SouthAfricanGradeLevel.Grade1)]
    [InlineData(SouthAfricanGradeLevel.Grade4)]
    [InlineData(SouthAfricanGradeLevel.Grade8)]
    [InlineData(SouthAfricanGradeLevel.Grade12)]
    public async Task A_learner_coming_from_another_school_is_asked_for_their_last_report(
        SouthAfricanGradeLevel level)
    {
        // Never asked of anybody until now: "GRADE 4" contains an R.
        (await AskedForAsync(Application(level)))
            .ShouldContain(DocumentCategory.PreviousSchoolReport);
    }

    [Fact]
    public async Task Grade_R_is_asked_for_a_clinic_card_and_not_a_school_report()
    {
        // There is no previous school to report on.
        var asked = await AskedForAsync(Application(SouthAfricanGradeLevel.GradeR));

        asked.ShouldContain(DocumentCategory.ImmunizationRecord);
        asked.ShouldNotContain(DocumentCategory.PreviousSchoolReport);
    }

    [Fact]
    public async Task A_non_citizen_is_asked_for_a_study_permit()
    {
        (await AskedForAsync(Application(SouthAfricanGradeLevel.Grade8, isSACitizen: false)))
            .ShouldContain(DocumentCategory.StudyPermit);
    }
}
