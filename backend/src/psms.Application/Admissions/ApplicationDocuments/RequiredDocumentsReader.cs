using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Admissions.ApplicationDocuments;

/// <summary>One document an application has to produce.</summary>
public class RequiredDocument
{
    public DocumentCategory Category { get; set; }

    /// <summary>What it is called where a parent will read it.</summary>
    public string Description { get; set; }

    /// <summary>False for the ones that only apply sometimes, like a transfer letter.</summary>
    public bool IsRequired { get; set; }
}

/// <summary>
/// What an application has to produce, by the one answer everything must agree on.
/// <para>
/// This existed twice and the two copies disagreed. The status endpoint fell
/// back to a sensible default list for the grade when the school had not set
/// one; the documents-verified guard did not, and reported "this grade has no
/// document list set — so nothing can be checked against it" for the very same
/// application the status endpoint said needed six documents. #441 pointed the
/// guard at the school's list and stopped there, which closed half the gap and
/// left the halves still able to drift.
/// </para>
/// <para>
/// Anything that asks what an application owes now asks here.
/// </para>
/// </summary>
public class RequiredDocumentsReader : ITransientDependency
{
    private readonly IRepository<Application, Guid> _applications;
    private readonly IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> _settings;

    public RequiredDocumentsReader(
        IRepository<Application, Guid> applications,
        IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> settings)
    {
        _applications = applications;
        _settings = settings;
    }

    /// <summary>What each category is called when asked for.</summary>
    public static string DescriptionOf(DocumentCategory category, bool isSACitizen) => category switch
    {
        DocumentCategory.BirthCertificate => isSACitizen ? "Birth Certificate" : "Passport Copy",
        DocumentCategory.ParentIdDocument => "Parent/Guardian ID Document",
        DocumentCategory.PassportPhoto => "Passport-size Photo of Student",
        DocumentCategory.ProofOfResidence => "Proof of Residence (not older than 3 months)",
        DocumentCategory.ImmunizationRecord => "Immunization Record (Road to Health Card)",
        DocumentCategory.PreviousSchoolReport => "Previous School Report Card",
        DocumentCategory.TransferLetter => "Transfer Letter (if mid-year)",
        DocumentCategory.StudyPermit => "Valid Study Permit",
        DocumentCategory.MedicalReport => "Medical Report",
        DocumentCategory.IdDocument => "Learner ID Document",
        DocumentCategory.Passport => "Passport",
        DocumentCategory.Photo => "Photograph",
        _ => category.ToString(),
    };

    /// <summary>
    /// The school's own list where they have set one, and otherwise the
    /// sensible default for the grade.
    /// </summary>
    public static List<RequiredDocument> For(
        bool isSACitizen, SouthAfricanGradeLevel? gradeLevel, Domain.Admissions.Entities.AdmissionSettings settings)
    {
        RequiredDocument One(DocumentCategory category, bool required = true) => new RequiredDocument
        {
            Category = category,
            Description = DescriptionOf(category, isSACitizen),
            IsRequired = required,
        };

        var chosen = settings?.RequiredDocumentCategories;
        if (chosen != null && chosen.Count > 0)
        {
            var fromSchool = chosen.Select(c => One(c)).ToList();

            // A study permit is the law's requirement, not the school's, so it
            // is added even to a list that forgot it.
            if (!isSACitizen && !chosen.Contains(DocumentCategory.StudyPermit))
                fromSchool.Add(One(DocumentCategory.StudyPermit));

            return fromSchool;
        }

        var documents = new List<RequiredDocument>
        {
            // Required for ALL applications (ADM-008)
            One(DocumentCategory.BirthCertificate),
            One(DocumentCategory.ParentIdDocument),
            One(DocumentCategory.PassportPhoto),
            One(DocumentCategory.ProofOfResidence),
        };

        /* The grade used to be matched on its name, and "GRADE 4" contains an R
           because the word GRADE does — so the "not Grade R" branch never ran
           for any grade, and a previous school report was never asked of
           anybody. The grade level is an enum; ask it. */
        if (gradeLevel == SouthAfricanGradeLevel.GradeR)
        {
            documents.Add(One(DocumentCategory.ImmunizationRecord));
        }
        else if (gradeLevel.HasValue)
        {
            documents.Add(One(DocumentCategory.PreviousSchoolReport));
            documents.Add(One(DocumentCategory.TransferLetter, required: false));
        }

        if (!isSACitizen) documents.Add(One(DocumentCategory.StudyPermit));

        return documents;
    }

    /// <summary>
    /// What this application owes, loading the grade and the school's settings
    /// for it. Empty only when the application itself has gone.
    /// </summary>
    public async Task<List<RequiredDocument>> ForApplicationAsync(Guid applicationId)
    {
        var application = await _applications
            .GetAll()
            .Where(a => a.Id == applicationId)
            .Select(a => new
            {
                a.AcademicYearId,
                a.AppliedGradeId,
                a.IsSACitizen,
                GradeLevel = (SouthAfricanGradeLevel?)a.AppliedGrade.GradeLevel,
            })
            .FirstOrDefaultAsync();

        if (application == null) return new List<RequiredDocument>();

        var settings = await SettingsForAsync(application.AcademicYearId, application.AppliedGradeId);

        return For(application.IsSACitizen, application.GradeLevel, settings);
    }

    /// <summary>
    /// The grade's own settings, falling back to the year's default row.
    /// </summary>
    public async Task<Domain.Admissions.Entities.AdmissionSettings> SettingsForAsync(Guid academicYearId, Guid gradeId)
    {
        var forGrade = await _settings
            .FirstOrDefaultAsync(s => s.AcademicYearId == academicYearId && s.GradeId == gradeId);

        return forGrade ?? await _settings
            .FirstOrDefaultAsync(s => s.AcademicYearId == academicYearId && s.GradeId == null);
    }
}
