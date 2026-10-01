using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using psms.MultiTenancy;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace psms.Assessment.Reports;

/// <summary>What the public verification page is told about a card.</summary>
public class ReportVerificationDto
{
    /// <summary>Whether a card was found for this code at all.</summary>
    public bool Found { get; set; }

    /// <summary>
    /// Whether the school has actually issued it. A card that exists but has
    /// not been published is not a document anybody should be holding.
    /// </summary>
    public bool Issued { get; set; }

    public string SchoolName { get; set; }

    /// <summary>
    /// The learner's initials rather than their name. Enough for the reader to
    /// match the card in their hand; not enough to publish a child's name to
    /// anyone who scans a code.
    /// </summary>
    public string LearnerInitials { get; set; }

    public string ClassName { get; set; }
    public string GradeName { get; set; }
    public string TermName { get; set; }
    public string ReportType { get; set; }
    public string AcademicYear { get; set; }

    /// <summary>The report card number printed on the document, to compare.</summary>
    public string ReportCardNumber { get; set; }

    public DateTime? IssuedOn { get; set; }

    /// <summary>Whether the card carries both signatures RE-003 requires.</summary>
    public bool SignedOff { get; set; }

    /// <summary>A sentence for the reader, rather than a status code.</summary>
    public string Message { get; set; }
}

public interface IReportVerificationAppService : IApplicationService
{
    Task<ReportVerificationDto> VerifyAsync(string token);
}

/// <summary>
/// Confirms that a report card in somebody's hand was issued by the school.
/// <para>
/// This is what actually proves a card is genuine. The signatures on it are a
/// picture — anyone holding one PDF can lift them — and without a signing
/// certificate the PDF itself is not tamper-evident, so a mark can be edited.
/// The only thing an outsider can trust is checking the code against the
/// school's own records, which is what this does.
/// </para>
/// <para>
/// Anonymous on purpose: the people who need it — another school, a bursary
/// office, an employer — have no account here. It therefore returns the least
/// that makes the check meaningful, and no marks at all.
/// </para>
/// </summary>
[AbpAllowAnonymous]
public class ReportVerificationAppService : ApplicationService, IReportVerificationAppService
{
    private readonly IRepository<Report, Guid> _reportRepository;
    private readonly IRepository<Tenant, int> _tenantRepository;

    public ReportVerificationAppService(
        IRepository<Report, Guid> reportRepository,
        IRepository<Tenant, int> tenantRepository)
    {
        _reportRepository = reportRepository;
        _tenantRepository = tenantRepository;
    }

    /// <summary>
    /// A fresh, unguessable code for a card. 16 random bytes is far beyond
    /// enumerating, which matters because this endpoint has no rate limit and
    /// no sign-in in front of it.
    /// </summary>
    public static string NewToken()
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public async Task<ReportVerificationDto> VerifyAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return NotFound();

        var trimmed = token.Trim();

        // The caller is not signed in and belongs to no tenant, so this has to
        // run host-side across every school's cards. SetTenantId(null) also
        // means an Abp-TenantId header a caller supplies cannot steer it.
        using (CurrentUnitOfWork.SetTenantId(null))
        {
            var card = await _reportRepository
                .GetAll()
                .Include(r => r.Student)
                .Include(r => r.Class).ThenInclude(c => c.Grade)
                .Include(r => r.Term)
                .Include(r => r.AcademicYear)
                .FirstOrDefaultAsync(r => r.VerificationToken == trimmed);

            if (card == null)
                return NotFound();

            var schoolName = await _tenantRepository
                .GetAll()
                .Where(t => t.Id == card.TenantId)
                .Select(t => t.Name ?? t.TenancyName)
                .FirstOrDefaultAsync();

            // A card that exists but was never published is not a document the
            // school stands behind, and saying so is the honest answer.
            if (card.Status != ReportStatus.Published)
            {
                return new ReportVerificationDto
                {
                    Found = true,
                    Issued = false,
                    SchoolName = schoolName,
                    Message = "This code matches a report card that the school has not issued. "
                        + "A card is only official once it has been published.",
                };
            }

            return new ReportVerificationDto
            {
                Found = true,
                Issued = true,
                SchoolName = schoolName,
                LearnerInitials = Initials(card.Student?.FirstName, card.Student?.LastName),
                ClassName = card.Class?.ClassName,
                GradeName = card.Class?.Grade?.GradeName,
                TermName = card.Term?.TermName,
                ReportType = Label(card.ReportType),
                AcademicYear = card.AcademicYear?.YearName,
                ReportCardNumber = card.ReportCardNumber,
                IssuedOn = card.PublishedDate,
                SignedOff = card.IsSignedOff(),
                Message = $"This is a genuine report card issued by {schoolName}.",
            };
        }
    }

    private static ReportVerificationDto NotFound() => new()
    {
        Found = false,
        Issued = false,
        Message = "No report card matches this code. Check the code on the document, "
            + "or ask the school to confirm it.",
    };

    /// <summary>"Thabo Hlakudi" becomes "T.H." — enough to match the card, no more.</summary>
    private static string Initials(string first, string last)
    {
        var a = string.IsNullOrWhiteSpace(first) ? "" : $"{char.ToUpperInvariant(first.Trim()[0])}.";
        var b = string.IsNullOrWhiteSpace(last) ? "" : $"{char.ToUpperInvariant(last.Trim()[0])}.";
        return (a + b).Length == 0 ? null : a + b;
    }

    private static string Label(ReportType type) => type switch
    {
        ReportType.Term1 => "Term 1 report",
        ReportType.Term2 => "Term 2 report",
        ReportType.Term3 => "Term 3 report",
        ReportType.Term4 => "Term 4 report",
        ReportType.MidYear => "Mid-year report",
        ReportType.YearEnd => "Year-end report",
        ReportType.Progress => "Progress report",
        _ => "Report card",
    };
}
