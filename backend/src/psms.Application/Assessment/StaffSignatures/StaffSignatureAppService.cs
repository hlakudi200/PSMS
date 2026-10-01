using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Assessment.Shared;
using psms.Authorization;
using psms.Domain.Assessment.Entities;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace psms.Assessment.StaffSignatures;

/// <summary>What a staff member's stored signature looks like to the screen.</summary>
public class StaffSignatureDto
{
    /// <summary>The drawing, as an SVG document.</summary>
    public string SvgContent { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

/// <summary>The signature as drawn, on its way in.</summary>
public class SaveStaffSignatureDto
{
    public string SvgContent { get; set; }
}

public interface IStaffSignatureAppService : IApplicationService
{
    Task<StaffSignatureDto> GetMineAsync();
    Task<StaffSignatureDto> SaveMineAsync(SaveStaffSignatureDto input);
    Task DeleteMineAsync();
}

/// <summary>
/// RC-17. A staff member's handwritten signature, drawn once and reused when
/// they sign a report card.
/// <para>
/// Every method here acts on the <b>caller's own</b> signature and takes no user
/// id, deliberately: a signature somebody else can write or read is not a
/// signature. Only the comment permission is needed, which is what a teacher
/// holds in order to sign the Class Teacher line.
/// </para>
/// </summary>
[AbpAuthorize(PermissionNames.Assessment_ReportCards_Comment)]
public class StaffSignatureAppService : ApplicationService, IStaffSignatureAppService
{
    private readonly IRepository<StaffSignature, Guid> _repository;

    public StaffSignatureAppService(IRepository<StaffSignature, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<StaffSignatureDto> GetMineAsync()
    {
        var mine = await MineAsync();
        return mine == null
            ? null
            : new StaffSignatureDto { SvgContent = mine.SvgContent, UpdatedAt = mine.LastModificationTime ?? mine.CreationTime };
    }

    public async Task<StaffSignatureDto> SaveMineAsync(SaveStaffSignatureDto input)
    {
        var userId = AbpSession.UserId
            ?? throw new UserFriendlyException(AssessmentExceptionCodes.UserNotSignedIn,
                "You must be signed in to save a signature.");

        var svg = Sanitise(input?.SvgContent);

        var mine = await MineAsync();
        if (mine == null)
        {
            mine = new StaffSignature(Guid.NewGuid(), AbpSession.TenantId ?? 0, userId, svg);
            await _repository.InsertAsync(mine);
        }
        else
        {
            mine.Replace(svg);
            await _repository.UpdateAsync(mine);
        }

        await CurrentUnitOfWork.SaveChangesAsync();
        return new StaffSignatureDto { SvgContent = mine.SvgContent, UpdatedAt = mine.LastModificationTime ?? mine.CreationTime };
    }

    public async Task DeleteMineAsync()
    {
        var mine = await MineAsync();
        if (mine == null) return;

        await _repository.DeleteAsync(mine);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    private async Task<StaffSignature> MineAsync()
    {
        var userId = AbpSession.UserId;
        if (userId == null) return null;

        return await _repository
            .GetAll()
            .Where(s => s.TenantId == AbpSession.TenantId && s.UserId == userId.Value)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Accepts a drawing and nothing else.
    /// <para>
    /// This SVG is rendered into a PDF and echoed back to a browser, so it is
    /// untrusted input on both paths. A signature pad emits only a path, so
    /// anything that can execute, fetch or embed is rejected outright rather
    /// than stripped — a signature has no legitimate use for a script, a
    /// stylesheet, a remote reference or an event handler, and refusing is
    /// safer than trying to clean one up.
    /// </para>
    /// </summary>
    private static string Sanitise(string svg)
    {
        if (string.IsNullOrWhiteSpace(svg))
            throw new UserFriendlyException(AssessmentExceptionCodes.SignatureInvalid,
                "Draw your signature before saving it.");

        svg = svg.Trim();

        if (svg.Length > StaffSignature.MaxSvgLength)
            throw new UserFriendlyException(AssessmentExceptionCodes.SignatureInvalid,
                $"That signature is too large. Keep it under {StaffSignature.MaxSvgLength / 1024}KB.");

        if (!svg.StartsWith("<svg", StringComparison.OrdinalIgnoreCase)
            && !svg.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
            throw new UserFriendlyException(AssessmentExceptionCodes.SignatureInvalid,
                "A signature must be an SVG drawing.");

        var lowered = svg.ToLowerInvariant();
        string[] forbidden =
        {
            "<script", "javascript:", "<foreignobject", "<iframe", "<embed", "<object",
            "<use", "<image", "<style", "xlink:href", "data:text/html", "onload=", "onclick=",
            "onerror=", "onmouseover=", "<animate", "<set ", "<handler",
        };

        foreach (var bad in forbidden)
        {
            if (lowered.Contains(bad))
                throw new UserFriendlyException(AssessmentExceptionCodes.SignatureInvalid,
                    "That does not look like a signature. Draw it in the box rather than pasting a file.");
        }

        if (!lowered.Contains("<path"))
            throw new UserFriendlyException(AssessmentExceptionCodes.SignatureInvalid,
                "Draw your signature before saving it.");

        if (!HasDrawableArea(lowered))
            throw new UserFriendlyException(AssessmentExceptionCodes.SignatureInvalid,
                "That signature did not come out. Draw it again — the box may not have finished "
                + "opening the first time.");

        return svg;
    }

    /// <summary>
    /// Whether the drawing has a canvas to be drawn on.
    /// <para>
    /// A signature pad that reads its canvas before the panel has been laid out
    /// writes viewBox="0 0 0 0" around perfectly good strokes. Stored, that
    /// looks like a signature everywhere except the printed card, where it
    /// renders as nothing at all and the line above the name comes out blank.
    /// Refusing it here keeps it off the card and out of the database.
    /// </para>
    /// </summary>
    private static bool HasDrawableArea(string loweredSvg)
    {
        var box = Regex.Match(loweredSvg, "viewbox\\s*=\\s*\"([^\"]*)\"");
        if (!box.Success)
            return true;    // no viewBox at all: the renderer falls back to width/height

        var parts = box.Groups[1].Value
            .Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        return parts.Length == 4
            && double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width)
            && double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height)
            && width > 0
            && height > 0;
    }
}
