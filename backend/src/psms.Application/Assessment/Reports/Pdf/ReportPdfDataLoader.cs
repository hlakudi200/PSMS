using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using psms.MultiTenancy;

namespace psms.Assessment.Reports.Pdf;

/// <summary>
/// Loads report data and maps it to the flat PDF data model.
/// </summary>
public class ReportPdfDataLoader : ITransientDependency
{
    private readonly IRepository<Report, Guid> _reportRepository;
    private readonly TenantManager _tenantManager;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public ReportPdfDataLoader(
        IRepository<Report, Guid> reportRepository,
        TenantManager tenantManager,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _reportRepository = reportRepository;
        _tenantManager = tenantManager;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public async Task<ReportPdfData> LoadAsync(Guid reportId, int? tenantId)
    {
        // Disable tenant filter since this runs in background jobs where session TenantId may be null
        using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
        {
        var report = await _reportRepository
            .GetAll()
            .Include(r => r.Student)
            .Include(r => r.Class)
            .Include(r => r.Term)
            .Include(r => r.AcademicYear)
            .Include(r => r.SubjectReports).ThenInclude(sr => sr.Subject)
            .Include(r => r.SubjectReports).ThenInclude(sr => sr.Teacher)
            .FirstOrDefaultAsync(r => r.Id == reportId && r.TenantId == tenantId);

        if (report == null)
            return null;

        // Get school name from tenant
        string schoolName = "School";
        if (tenantId.HasValue)
        {
            var tenant = await _tenantManager.FindByIdAsync(tenantId.Value);
            if (tenant != null)
                schoolName = tenant.TenancyName;
        }

        var data = new ReportPdfData
        {
            SchoolName = schoolName,
            StudentName = report.Student?.GetFullName() ?? "Unknown Student",
            AdmissionNumber = report.Student?.AdmissionNumber,
            ClassName = report.Class?.ClassName ?? "N/A",
            TermName = report.Term?.TermName,
            AcademicYearName = report.AcademicYear?.YearName ?? "N/A",
            ReportType = GetReportTypeLabel(report.ReportType),
            GeneratedDate = report.GeneratedDate?.ToString("dd MMM yyyy"),
            OverallPercentage = report.OverallPercentage,
            OverallAchievementLevel = GetAchievementLabel(report.OverallAchievementLevel),
            ClassPosition = report.ClassPosition,
            TotalStudentsInClass = report.TotalStudentsInClass,
            DaysPresent = report.DaysPresent,
            DaysAbsent = report.DaysAbsent,
            DaysLate = report.DaysLate,
            TeacherComment = report.TeacherComment,
            PrincipalComment = report.PrincipalComment,
            ParentComment = report.ParentComment,
        };

        data.Subjects = report.SubjectReports
            .OrderBy(sr => sr.Subject?.SubjectName)
            .Select(sr => new SubjectEntry
            {
                SubjectName = sr.Subject?.SubjectName ?? "Unknown",
                SubjectCode = sr.Subject?.SubjectCode,
                TermMark = sr.TermMark,
                ExamMark = sr.ExamMark,
                FinalMark = sr.FinalMark,
                AchievementLevel = GetAchievementLabel(sr.AchievementLevel),
                TeacherName = sr.Teacher?.GetFullName(),
                TeacherComment = sr.TeacherComment,
            })
            .ToList();

        return data;
        } // end DisableFilter
    }

    private static string GetReportTypeLabel(ReportType type) => type switch
    {
        ReportType.Term1 => "Term 1 Report",
        ReportType.Term2 => "Term 2 Report",
        ReportType.Term3 => "Term 3 Report",
        ReportType.Term4 => "Term 4 Report",
        ReportType.MidYear => "Mid-Year Report",
        ReportType.YearEnd => "Year-End Report",
        ReportType.Progress => "Progress Report",
        _ => "Report",
    };

    private static string GetAchievementLabel(CapsAchievementLevel? level) => level switch
    {
        CapsAchievementLevel.Level7 => "7 - Outstanding",
        CapsAchievementLevel.Level6 => "6 - Meritorious",
        CapsAchievementLevel.Level5 => "5 - Substantial",
        CapsAchievementLevel.Level4 => "4 - Adequate",
        CapsAchievementLevel.Level3 => "3 - Moderate",
        CapsAchievementLevel.Level2 => "2 - Elementary",
        CapsAchievementLevel.Level1 => "1 - Not Achieved",
        _ => null,
    };
}
