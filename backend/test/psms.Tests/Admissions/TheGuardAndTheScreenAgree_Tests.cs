using psms.Admissions.ApplicationDocuments;
using psms.Domain.Academic.Entities;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using psms.Workflow.Engine.Guards;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// The step that blocks on documents and the screen that lists them give the
/// same answer.
/// <para>
/// They did not. On the deployed system the same Grade 5 application produced:
/// </para>
/// <code>
/// STATUS endpoint: 6 required — BirthCertificate, ParentIdDocument, ...
/// GUARD:           "this grade has no document list set — so nothing can be
///                   checked against it."
/// </code>
/// <para>
/// The status fell back to a default list for the grade when the school had
/// set none; the guard did not. #441 pointed the guard at the school's list
/// and stopped there, which closed half the gap and left the halves free to
/// drift. One reader answers both now, and these hold it that way.
/// </para>
/// </summary>
public class TheGuardAndTheScreenAgree_Tests : psmsTestBase
{
    private readonly IApplicationDocumentAppService _documents;
    private readonly ApplicationDocumentsVerifiedGuard _guard;

    public TheGuardAndTheScreenAgree_Tests()
    {
        _documents = Resolve<IApplicationDocumentAppService>();
        _guard = Resolve<ApplicationDocumentsVerifiedGuard>();
        LoginAsDefaultTenantAdmin();
    }

    private Guid Application(
        SouthAfricanGradeLevel level = SouthAfricanGradeLevel.Grade5,
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
                SouthAfricanSchoolPhase.Intermediate));
            context.AdmissionSettings.Add(new AdmissionSettings(Guid.NewGuid(), 1, yearId, 0m)
            {
                GradeId = gradeId,
                RequiredDocuments = schoolAsksFor,
            });
            context.Applications.Add(new Application(
                applicationId, 1, $"APP-{applicationId:N}".Substring(0, 16),
                "Ann", "Adams", new DateTime(2013, 4, 1), Gender.Female,
                gradeId, yearId, "parent@example.test")
            { IsSACitizen = isSACitizen });
        });

        return applicationId;
    }

    private void Upload(Guid applicationId, DocumentCategory category, bool verified)
        => UsingDbContext(1, context =>
        {
            var document = new ApplicationDocument(
                Guid.NewGuid(), applicationId, category,
                RequiredDocumentsReader.DescriptionOf(category, true),
                "scan.pdf", "documents/scan.pdf", 1024)
            {
                TenantId = 1,
                ContentType = "application/pdf",
            };

            // Verified through the domain call, so the record says who did it.
            if (verified) document.Verify(1);

            context.ApplicationDocuments.Add(document);
        });

    /// <summary>
    /// The guard is resolved directly rather than driven through the workflow
    /// service, so nothing has opened a unit of work for its repositories.
    /// </summary>
    private async Task<psms.Workflow.Engine.WorkflowGuardResult> GuardSaysAsync(Guid applicationId)
    {
        using var uow = Resolve<Abp.Domain.Uow.IUnitOfWorkManager>().Begin();
        var result = await _guard.EvaluateAsync(applicationId);
        await uow.CompleteAsync();
        return result;
    }

    /// <summary>Everything the screen would show as outstanding and required.</summary>
    private async Task<string[]> ScreenSaysOutstandingAsync(Guid applicationId)
    {
        var status = await _documents.GetRequiredDocumentsStatusAsync(applicationId);

        return status.Documents
            .Where(d => d.IsRequired && !d.IsVerified)
            .Select(d => d.Category.ToString())
            .ToArray();
    }

    [Fact]
    public async Task A_grade_with_no_list_set_is_not_a_grade_with_nothing_to_check()
    {
        // The exact shape of the live disagreement.
        var application = Application();

        var status = await _documents.GetRequiredDocumentsStatusAsync(application);
        var guard = await GuardSaysAsync(application);

        status.TotalRequired.ShouldBeGreaterThan(0);
        guard.Satisfied.ShouldBeFalse();

        // And it says what is outstanding rather than that it cannot tell.
        guard.Message.ShouldNotContain("no document list set");
        guard.Message.ShouldContain("not uploaded");
    }

    [Fact]
    public async Task Both_count_the_same_documents_outstanding()
    {
        var application = Application();
        Upload(application, DocumentCategory.BirthCertificate, verified: true);
        Upload(application, DocumentCategory.ParentIdDocument, verified: false);

        var outstanding = await ScreenSaysOutstandingAsync(application);
        var guard = await GuardSaysAsync(application);

        // Birth certificate is in and verified; the parent ID is in but not
        // verified; the rest are missing. The screen and the step agree.
        outstanding.ShouldNotContain(nameof(DocumentCategory.BirthCertificate));
        outstanding.ShouldContain(nameof(DocumentCategory.ParentIdDocument));

        guard.Satisfied.ShouldBeFalse();
        guard.Message.ShouldContain("awaiting verification");
    }

    [Fact]
    public async Task The_schools_own_list_decides_for_both()
    {
        var application = Application(schoolAsksFor: "BirthCertificate,ProofOfResidence");

        var status = await _documents.GetRequiredDocumentsStatusAsync(application);
        status.TotalRequired.ShouldBe(2);

        Upload(application, DocumentCategory.BirthCertificate, verified: true);
        Upload(application, DocumentCategory.ProofOfResidence, verified: true);

        (await ScreenSaysOutstandingAsync(application)).ShouldBeEmpty();
        (await GuardSaysAsync(application)).Satisfied.ShouldBeTrue();
    }

    [Fact]
    public async Task An_optional_document_never_holds_the_application_up()
    {
        /* A transfer letter applies "if mid-year". The screen lists it so the
           family knows it may be wanted; the step must not block on it, or no
           application that is not a transfer could ever pass. */
        var application = Application();

        foreach (var category in new[]
        {
            DocumentCategory.BirthCertificate, DocumentCategory.ParentIdDocument,
            DocumentCategory.PassportPhoto, DocumentCategory.ProofOfResidence,
            DocumentCategory.PreviousSchoolReport,
        })
            Upload(application, category, verified: true);

        var status = await _documents.GetRequiredDocumentsStatusAsync(application);
        status.Documents.ShouldContain(d => d.Category == DocumentCategory.TransferLetter && !d.IsRequired);

        (await GuardSaysAsync(application)).Satisfied.ShouldBeTrue();
    }

    [Fact]
    public async Task A_non_citizen_is_held_to_the_study_permit_by_both()
    {
        var application = Application(schoolAsksFor: "BirthCertificate", isSACitizen: false);

        var status = await _documents.GetRequiredDocumentsStatusAsync(application);
        status.Documents.ShouldContain(d => d.Category == DocumentCategory.StudyPermit);

        Upload(application, DocumentCategory.BirthCertificate, verified: true);

        var guard = await GuardSaysAsync(application);
        guard.Satisfied.ShouldBeFalse();
        guard.Message.ShouldContain("Study Permit");
    }
}
