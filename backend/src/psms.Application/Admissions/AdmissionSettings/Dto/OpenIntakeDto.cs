using System;

namespace psms.Admissions.AdmissionSettings.Dto;

/// <summary>
/// A year and grade a prospective parent may apply for, with the terms
/// attached to it.
/// <para>
/// The application form needs an academic year and a grade, and an applicant
/// holds neither <c>Academic.Calendar.View</c> nor <c>Academic.Grades.View</c>
/// — nor should they: a parent applying to a school has no business reading
/// its whole timetable of years and grades. This is the admissions-shaped
/// answer to "what can I apply for", and it carries the terms the parent needs
/// to see before they start.
/// </para>
/// </summary>
public class OpenIntakeDto
{
    public Guid AcademicYearId { get; set; }
    public string AcademicYearName { get; set; }

    /// <summary>Null on a school's default row, which covers every grade.</summary>
    public Guid? GradeId { get; set; }
    public string GradeName { get; set; }

    public DateTime? ApplicationCloseDate { get; set; }

    public bool FeeRequired { get; set; }
    public decimal FeeAmount { get; set; }

    public bool IsInterviewRequired { get; set; }
    public bool IsAssessmentRequired { get; set; }

    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }

    /// <summary>What the school asks an applicant to provide, as it wrote it.</summary>
    public string RequiredDocuments { get; set; }

    /// <summary>
    /// Places left, where the school caps the grade. Null means uncapped —
    /// not zero, which would read as full.
    /// </summary>
    public int? AvailableSpots { get; set; }
}
