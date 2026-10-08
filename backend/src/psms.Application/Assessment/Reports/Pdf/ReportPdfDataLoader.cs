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
    private readonly IRepository<psms.Authorization.Users.User, long> _userRepository;
    private readonly TenantManager _tenantManager;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
    public Castle.Core.Logging.ILogger Logger { get; set; } = Castle.Core.Logging.NullLogger.Instance;

    public ReportPdfDataLoader(
        IRepository<Report, Guid> reportRepository,
        IRepository<SchoolBranding, Guid> brandingRepository,
        IRepository<psms.Authorization.Users.User, long> userRepository,
        TenantManager tenantManager,
        IUnitOfWorkManager unitOfWorkManager,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _reportRepository = reportRepository;
        _brandingRepository = brandingRepository;
        _userRepository = userRepository;
        _tenantManager = tenantManager;
        _unitOfWorkManager = unitOfWorkManager;
        _configuration = configuration;
    }

    public async Task<ReportPdfData> LoadAsync(Guid reportId, int? tenantId)
    {
        // Disable tenant filter since this runs in background jobs where session TenantId may be null
        using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
        {
        var report = await _reportRepository
            .GetAll()
            .Include(r => r.Student)
            .Include(r => r.Class).ThenInclude(c => c.Grade)
            .Include(r => r.Term)
            .Include(r => r.AcademicYear)
            .Include(r => r.PromotedToGrade)
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
        string stampUrl = null;

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
                stampUrl = branding.StampUrl;
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
            LogoBytes = await TryFetchImageAsync(logoUrl, "logo"),
            // §25(8)(b): the school stamp.
            StampBytes = await TryFetchImageAsync(stampUrl, "stamp"),
            StudentName = report.Student?.GetFullName() ?? "Unknown Student",
            AdmissionNumber = report.Student?.AdmissionNumber,
            ClassName = report.Class?.ClassName ?? "N/A",
            // RC-21, §25(8)(a) and (b): the grade, the learner's date of birth
            // and the term's own opening and closing dates.
            GradeName = report.Class?.Grade?.GradeName,
            DateOfBirth = report.Student?.DateOfBirth.ToString("dd MMM yyyy"),
            SchoolOpensOn = report.Term?.StartDate.ToString("dd MMM yyyy"),
            SchoolClosesOn = report.Term?.EndDate.ToString("dd MMM yyyy"),
            ReportsPercentages = psms.Domain.Assessment.ReportingScale
                .ReportsPercentages(report.Class?.Grade?.GradeLevel),
            TermName = report.Term?.TermName,
            AcademicYearName = report.AcademicYear?.YearName ?? "N/A",
            ReportType = GetReportTypeLabel(report.ReportType),
            GeneratedDate = report.GeneratedDate?.ToString("dd MMM yyyy"),
            ReportCardNumber = report.ReportCardNumber,
            DaysInTerm = report.DaysInTerm,
            ConductRating = GetConductLabel(report.ConductRating),
            DiligenceRating = GetConductLabel(report.DiligenceRating),
            BehaviourComments = report.BehaviourComments,
            TeacherSignedBy = await ResolveSignerAsync(report.TeacherSignedByUserId),
            TeacherSignedDate = report.TeacherSignedDate?.ToString("dd MMM yyyy"),
            TeacherSignatureSvg = report.TeacherSignatureSvg,
            PrincipalSignedBy = await ResolveSignerAsync(report.PrincipalSignedByUserId),
            PrincipalSignedDate = report.PrincipalSignedDate?.ToString("dd MMM yyyy"),
            PrincipalSignatureSvg = report.PrincipalSignatureSvg,
            VerificationUrl = BuildVerificationUrl(report.VerificationToken),
            OverallPercentage = report.OverallPercentage,
            OverallAchievementLevel = report.OverallAchievementLevel,
            ClassPosition = report.ClassPosition,
            TotalStudentsInClass = report.TotalStudentsInClass,
            DaysPresent = report.DaysPresent,
            DaysAbsent = report.DaysAbsent,
            DaysLate = report.DaysLate,
            PromotionDecision = GetPromotionLabel(report.PromotionDecision),
            PromotedToGradeName = report.PromotedToGrade?.GradeName,
            PromotionReason = report.PromotionReason,
            TeacherComment = report.TeacherComment,
            PrincipalComment = report.PrincipalComment,
            ParentComment = report.ParentComment,
        };

        // RC-21, §25(8)(d): what the learner did last term, so the reader has
        // something to read this term against.
        data.PreviousPerformance = await PreviousPerformanceAsync(report, data.ReportsPercentages);

        data.Subjects = report.SubjectReports
            .OrderBy(sr => sr.Subject?.SubjectName)
            .Select(sr => new SubjectEntry
            {
                // RC-20, §17(6): a language is reported at the level it is
                // offered at. Most schools already put it in the subject's name,
                // so it is appended only where it would otherwise be missing.
                SubjectName = psms.Domain.Assessment.SubjectLanguageRules.NameWithLevel(
                    sr.Subject?.SubjectName, sr.Subject?.LanguageLevel) ?? "Unknown",
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
                AwaitsExternalExamination = sr.AwaitsExternalExamination,
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
    private async Task<byte[]> TryFetchImageAsync(string url, string what)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            Logger.Warn($"School {what} URL is not a usable http(s) address, skipping it: {url}");
            return null;
        }

        try
        {
            using var cts = new CancellationTokenSource(LogoFetchTimeout);
            using var response = await LogoClient.GetAsync(uri, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                Logger.Warn($"School {what} fetch returned {(int)response.StatusCode}, printing without it.");
                return null;
            }

            if (response.Content.Headers.ContentLength > MaxLogoBytes)
            {
                Logger.Warn($"School {what} is larger than the embed limit, printing without it.");
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return bytes.Length == 0 || bytes.Length > MaxLogoBytes ? null : bytes;
        }
        catch (Exception ex)
        {
            Logger.Warn($"Could not read the school {what} for the report card: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// RC-16. How a promotion decision is worded on the card. The Protocol
    /// distinguishes progression from promotion — Grades R-8 progress, Grades
    /// 9-11 are promoted (National Protocol §26(7)(k)-(m)) — so the wording says
    /// which one this is.
    /// </summary>
    private static string GetPromotionLabel(PromotionDecision? decision) => decision switch
    {
        psms.Domain.Shared.Enums.PromotionDecision.Promoted => "Promoted",
        psms.Domain.Shared.Enums.PromotionDecision.Retained => "Not promoted — retained",
        psms.Domain.Shared.Enums.PromotionDecision.ConditionalPromotion => "Conditionally promoted",
        psms.Domain.Shared.Enums.PromotionDecision.ProgressedWithSupport => "Progressed with support",
        _ => null,
    };

    /// <summary>
    /// Where the QR on the card points. Null when the card has no code yet —
    /// anything generated before verification existed — which simply leaves the
    /// strip off rather than printing a link that resolves to nothing.
    /// <para>
    /// Also null when the address configured is one only this machine can
    /// reach. A report card is issued once and, under National Protocol §25(3),
    /// is not withdrawn afterwards; a QR pointing at localhost would sit on a
    /// parent's copy forever, scanning to nothing and looking for all the world
    /// like the card is a forgery. A card with no QR can be reissued with one.
    /// A card with the wrong QR cannot be taken back, so a deployment that has
    /// not been told its own public address prints no code and says why.
    /// </para>
    /// </summary>
    private string BuildVerificationUrl(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        var root = _configuration["App:ClientRootAddress"];
        if (string.IsNullOrWhiteSpace(root)) return null;

        if (!Uri.TryCreate(root, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            Logger.Warn($"App:ClientRootAddress is not an absolute http(s) address ('{root}'), "
                + "so report cards are printing without a verification QR code.");
            return null;
        }

        if (uri.IsLoopback || uri.Host.Equals("0.0.0.0", StringComparison.Ordinal))
        {
            Logger.Warn($"App:ClientRootAddress points at this machine ('{root}'), which nobody "
                + "scanning a printed report card can reach, so cards are printing without a "
                + "verification QR code. Set it to the address parents use.");
            return null;
        }

        return $"{root.TrimEnd('/')}/verify/{token}";
    }

    /// <summary>
    /// RC-17. The name to print over a signature line. Null when nobody has
    /// signed, or when the account that did has since been removed — the card
    /// then prints the blank line it used to, rather than failing over a name.
    /// </summary>

    /// <summary>
    /// RC-21. The learner's result from the term before this one, phrased for
    /// printing, or null when there is nothing earlier to compare with.
    /// <para>
    /// §25(8)(d) asks for feedback "in relation to his or her previous
    /// performance". The card cannot write the teacher's commentary, but it can
    /// put last term's result next to this one so the comparison is on the page
    /// rather than in the reader's memory.
    /// </para>
    /// <para>
    /// Foundation Phase cards get the level and its description, not a
    /// percentage — RC-19 applies to the comparison as much as to the marks.
    /// </para>
    /// </summary>
    private async Task<string> PreviousPerformanceAsync(Report report, bool reportsPercentages)
    {
        if (report.TermId == null) return null;

        var previous = await _reportRepository
            .GetAll()
            .Include(r => r.Term)
            .Where(r => r.TenantId == report.TenantId
                && r.StudentId == report.StudentId
                && r.AcademicYearId == report.AcademicYearId
                && r.Id != report.Id
                && r.Term != null
                && report.Term != null
                && r.Term.TermNumber < report.Term.TermNumber)
            .OrderByDescending(r => r.Term.TermNumber)
            .FirstOrDefaultAsync();

        if (previous == null) return null;

        return psms.Domain.Assessment.PreviousPerformance.Describe(
            previous.Term?.TermName,
            previous.OverallPercentage,
            previous.OverallAchievementLevel,
            report.OverallPercentage,
            reportsPercentages);
    }

    /// <summary>
    /// RC-17. The name to print over a signature line. Null when nobody has
    /// signed, or when the account that did has since been removed — the card
    /// then prints the blank line it used to, rather than failing over a name.
    /// </summary>
    private async Task<string> ResolveSignerAsync(long? userId)
    {
        if (!userId.HasValue || userId.Value == 0)
            return null;

        var user = await _userRepository
            .GetAll()
            .Where(u => u.Id == userId.Value)
            .Select(u => new { u.Name, u.Surname, u.UserName })
            .FirstOrDefaultAsync();

        if (user == null)
            return null;

        var full = $"{user.Name} {user.Surname}".Trim();

        return string.IsNullOrWhiteSpace(full) ? user.UserName : full;
    }

    /// <summary>
    /// RC-17. A conduct or diligence rating in words. Seven points, matching the
    /// shape of the achievement scale so a reader is not switching between two
    /// different-sized rulers on one page.
    /// </summary>
    private static string GetConductLabel(ConductDiligenceRating? rating) => rating switch
    {
        ConductDiligenceRating.Excellent => "Excellent",
        ConductDiligenceRating.VeryGood => "Very good",
        ConductDiligenceRating.Good => "Good",
        ConductDiligenceRating.Satisfactory => "Satisfactory",
        ConductDiligenceRating.NeedsImprovement => "Needs improvement",
        ConductDiligenceRating.Poor => "Poor",
        ConductDiligenceRating.Unsatisfactory => "Unsatisfactory",
        _ => null,
    };

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
