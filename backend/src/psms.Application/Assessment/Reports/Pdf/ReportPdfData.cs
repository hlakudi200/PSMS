using System;
using System.Collections.Generic;

namespace psms.Assessment.Reports.Pdf;

/// <summary>
/// Flat data model for PDF report card rendering.
/// </summary>
public class ReportPdfData
{
    // School info
    public string SchoolName { get; set; }

    /// <summary>
    /// The school's action colour, "#RRGGBB". Falls back to the stock PSMS
    /// palette when the tenant has not branded itself (issue #56).
    /// </summary>
    public string PrimaryColor { get; set; } = psms.Domain.Tenancy.BrandingDefaults.PrimaryColor;

    /// <summary>The school's chrome colour, "#RRGGBB".</summary>
    public string SecondaryColor { get; set; } = psms.Domain.Tenancy.BrandingDefaults.SecondaryColor;

    /// <summary>
    /// The school's logo, already fetched. Null when the tenant has no logo, or
    /// when it could not be read — a report card must still print without it.
    /// </summary>
    public byte[] LogoBytes { get; set; }

    // Student info
    public string StudentName { get; set; }
    public string AdmissionNumber { get; set; }
    public string ClassName { get; set; }
    public string TermName { get; set; }
    public string AcademicYearName { get; set; }
    public string ReportType { get; set; }
    public string GeneratedDate { get; set; }

    // Overall performance
    public decimal? OverallPercentage { get; set; }
    public string OverallAchievementLevel { get; set; }
    public int? ClassPosition { get; set; }
    public int? TotalStudentsInClass { get; set; }

    // Attendance
    public int DaysPresent { get; set; }
    public int DaysAbsent { get; set; }
    public int DaysLate { get; set; }

    // Comments
    public string TeacherComment { get; set; }
    public string PrincipalComment { get; set; }
    public string ParentComment { get; set; }

    // Subject entries
    public List<SubjectEntry> Subjects { get; set; } = new();
}

/// <summary>
/// Subject line item for the report card table.
/// </summary>
public class SubjectEntry
{
    public string SubjectName { get; set; }
    public string SubjectCode { get; set; }
    public decimal? TermMark { get; set; }
    public decimal? ExamMark { get; set; }
    public decimal? FinalMark { get; set; }
    public string AchievementLevel { get; set; }
    public string TeacherName { get; set; }
    public string TeacherComment { get; set; }
}
