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
using psms.Domain.Tenancy;
using psms.Domain.Tenancy.Entities;
using psms.MultiTenancy;
using System.Net.Http;
using System.Threading;

namespace psms.Assessment.Reports.Pdf;

/// <summary>
/// Loads report data and maps it to the flat PDF data model.
/// </summary>
public class ReportPdfDataLoader : ITransientDependency
{
    /// <summary>
    /// How long to wait for the school logo before printing without it. A report
    /// card must not fail, or hang a background job, over a decoration.
    /// </summary>
    private static readonly TimeSpan LogoFetchTimeout = TimeSpan.FromSeconds(5);

    /// <summary>Beyond this a "logo" is not a logo, and we skip it.</summary>
    private const int MaxLogoBytes = 2 * 1024 * 1024;

    private static readonly HttpClient LogoClient = new HttpClient
    {
        Timeout = LogoFetchTimeout
    };

    private readonly IRepository<Report, Guid> _reportRepository;
    private readonly IRepository<SchoolBranding, Guid> _brandingRepository;
    private readonly TenantManager _tenantManager;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    public Castle.Core.Logging.ILogger Logger { get; set; } = Castle.Core.Logging.NullLogger.Instance;

    public ReportPdfDataLoader(
        IRepository<Report, Guid> reportRepository,
        IRepository<SchoolBranding, Guid> brandingRepository,
        TenantManager tenantManager,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _reportRepository = reportRepository;
        _brandingRepository = brandingRepository;
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

        // The school's own identity, so a printed card looks like it came from
        // the school rather than from PSMS (issue #56). The branding row carries
        // the display name; the tenancy name is only a fallback for a tenant that
        // has never been branded.
        string schoolName = null;
        var primaryColor = BrandingDefaults.PrimaryColor;
        var secondaryColor = BrandingDefaults.SecondaryColor;
        string logoUrl = null;

        if (tenantId.HasValue)
        {
            var branding = await _brandingRepository
                .GetAll()
                .FirstOrDefaultAsync(b => b.TenantId == tenantId.Value);

            if (branding != null)
            {
                if (!string.IsNullOrWhiteSpace(branding.SchoolName))
                    schoolName = branding.SchoolName;
                if (SchoolBranding.IsValidHexColor(branding.PrimaryColor))
                    primaryColor = branding.PrimaryColor;
                if (SchoolBranding.IsValidHexColor(branding.SecondaryColor))
                    secondaryColor = branding.SecondaryColor;
                logoUrl = branding.LogoUrl;
            }

            if (string.IsNullOrWhiteSpace(schoolName))
            {
                var tenant = await _tenantManager.FindByIdAsync(tenantId.Value);
                if (tenant != null)
                    schoolName = tenant.TenancyName;
            }
        }

        var data = new ReportPdfData
        {
            SchoolName = string.IsNullOrWhiteSpace(schoolName) ? "School" : schoolName,
            PrimaryColor = primaryColor,
            SecondaryColor = secondaryColor,
            LogoBytes = await TryFetchLogoAsync(logoUrl),
            StudentName = report.Student?.GetFullName() ?? "Unknown Student",
            AdmissionNumber = report.Student?.AdmissionNumber,
            ClassName = report.Class?.ClassName ?? "N/A",
            TermName = report.Term?.TermName,
            AcademicYearName = report.AcademicYear?.YearName ?? "N/A",
            ReportType = GetReportTypeLabel(report.ReportType),
            GeneratedDate = report.GeneratedDate?.ToString("dd MMM yyyy"),
            OverallPercentage = report.OverallPercentage,
            OverallAchievementLevel = report.OverallAchievementLevel,
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
                AchievementLevel = sr.AchievementLevel,
                TeacherName = sr.Teacher?.GetFullName(),
                TeacherComment = sr.TeacherComment,
                ClassAverage = sr.ClassAverage,
                SubjectPosition = sr.SubjectPosition,
                HighestInClass = sr.HighestInClass,
                LowestInClass = sr.LowestInClass,
            })
            .ToList();

        return data;
        } // end DisableFilter
    }

    /// <summary>
    /// Reads the logo bytes so QuestPDF can embed them. Returns null on anything
    /// going wrong — a slow host, a 404, a file that is not an image — because a
    /// report card has to print with or without the badge on it.
    /// </summary>
    private async Task<byte[]> TryFetchLogoAsync(string logoUrl)
    {
        if (string.IsNullOrWhiteSpace(logoUrl))
            return null;

        if (!Uri.TryCreate(logoUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            Logger.Warn($"School logo URL is not a usable http(s) address, skipping it: {logoUrl}");
            return null;
        }

        try
        {
            using var cts = new CancellationTokenSource(LogoFetchTimeout);
            using var response = await LogoClient.GetAsync(uri, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                Logger.Warn($"School logo fetch returned {(int)response.StatusCode}, printing without it.");
                return null;
            }

            if (response.Content.Headers.ContentLength > MaxLogoBytes)
            {
                Logger.Warn("School logo is larger than the embed limit, printing without it.");
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return bytes.Length == 0 || bytes.Length > MaxLogoBytes ? null : bytes;
        }
        catch (Exception ex)
        {
            Logger.Warn($"Could not read the school logo for the report card: {ex.Message}");
            return null;
        }
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

}
