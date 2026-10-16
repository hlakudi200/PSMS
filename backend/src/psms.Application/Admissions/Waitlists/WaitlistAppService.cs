using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Admissions.Shared;
using psms.Admissions.Waitlists.Dto;
using psms.Authorization;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Admissions.Waitlists;

/// <summary>
/// Service for managing admission waitlists.
/// Implements ADM-021 to ADM-024.
/// </summary>
[AbpAuthorize(PermissionNames.Admissions_Waitlist)]
public class WaitlistAppService : ApplicationService, IWaitlistAppService
{
    private readonly IRepository<Waitlist, Guid> _waitlistRepository;
    private readonly IRepository<Application, Guid> _applicationRepository;
    private readonly ICurrentApplicantResolver _currentApplicant;

    public WaitlistAppService(
        IRepository<Waitlist, Guid> waitlistRepository,
        IRepository<Application, Guid> applicationRepository,
        ICurrentApplicantResolver currentApplicant)
    {
        _waitlistRepository = waitlistRepository;
        _applicationRepository = applicationRepository;
        _currentApplicant = currentApplicant;
    }

    /// <summary>
    /// Refuses a family anything but their own.
    /// <para>
    /// An applicant holds the waitlist view, accept and decline permissions so
    /// they can answer about their own child. Nothing checked that the entry
    /// they named was theirs: <c>GetByApplication</c> and <c>GetPosition</c>
    /// took any application id and answered with another family's child, their
    /// grade and their place in the queue — and <c>AcceptOffer</c> and
    /// <c>DeclineOffer</c> took any waitlist id and <b>acted on it</b>. One
    /// family could take or throw away a place offered to another.
    /// </para>
    /// <para>
    /// Staff are told apart by Applications.ViewAll, which no applicant holds.
    /// </para>
    /// </summary>
    private async Task EnsureTheirsAsync(Guid applicationId)
    {
        var ownerOnly = await _currentApplicant.GetOwnApplicationsOnlyForAsync();
        if (!ownerOnly.HasValue) return;

        var application = await _applicationRepository
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        // Same answer for "does not exist" and "is not yours", so the ids of
        // other families' applications cannot be discovered by asking.
        if (application == null || application.CreatorUserId != ownerOnly.Value)
            throw new UserFriendlyException(AdmissionsExceptionCodes.ApplicationNotFound,
                "Application not found.");
    }

    /// <summary>The same check, for the calls that name a waitlist entry rather than an application.</summary>
    private async Task EnsureEntryIsTheirsAsync(Guid waitlistId)
    {
        var ownerOnly = await _currentApplicant.GetOwnApplicationsOnlyForAsync();
        if (!ownerOnly.HasValue) return;

        var entry = await _waitlistRepository.FirstOrDefaultAsync(w => w.Id == waitlistId);
        if (entry == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.WaitlistNotFound,
                "Waitlist entry not found.");

        await EnsureTheirsAsync(entry.ApplicationId);
    }

    [AbpAuthorize(PermissionNames.Admissions_Waitlist_View)]
    public async Task<WaitlistDto> GetAsync(Guid id)
    {
        await EnsureEntryIsTheirsAsync(id);

        var waitlist = await _waitlistRepository
            .GetAll()
            .Include(w => w.Application)
                .ThenInclude(a => a.AppliedGrade)
            .Include(w => w.Grade)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (waitlist == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.WaitlistNotFound, "Waitlist entry not found.");

        return MapToDto(waitlist);
    }

    [AbpAuthorize(PermissionNames.Admissions_Waitlist_View)]
    public async Task<WaitlistDto> GetByApplicationAsync(Guid applicationId)
    {
        await EnsureTheirsAsync(applicationId);

        var waitlist = await _waitlistRepository
            .GetAll()
            .Include(w => w.Application)
                .ThenInclude(a => a.AppliedGrade)
            .Include(w => w.Grade)
            .FirstOrDefaultAsync(w => w.ApplicationId == applicationId);

        if (waitlist == null)
            return null;

        return MapToDto(waitlist);
    }

    [AbpAuthorize(PermissionNames.Admissions_Waitlist_ViewAll)]
    public async Task<List<WaitlistDto>> GetByGradeAsync(Guid gradeId)
    {
        var waitlists = await _waitlistRepository
            .GetAll()
            .Include(w => w.Application)
                .ThenInclude(a => a.AppliedGrade)
            .Include(w => w.Grade)
            .Where(w => w.GradeId == gradeId && w.Status == WaitlistStatus.Active)
            .OrderBy(w => w.Position)
            .ToListAsync();

        return waitlists.Select(MapToDto).ToList();
    }

    [AbpAuthorize(PermissionNames.Admissions_Waitlist_ViewAll)]
    public async Task<PagedResultDto<WaitlistDto>> GetAllAsync(GetWaitlistsInput input)
    {
        var query = _waitlistRepository
            .GetAll()
            .Include(w => w.Application)
                .ThenInclude(a => a.AppliedGrade)
            .Include(w => w.Grade)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                w => w.Application.ProspectiveStudentFirstName.ToLower().Contains(input.Keyword.ToLower())
                    || w.Application.ProspectiveStudentLastName.ToLower().Contains(input.Keyword.ToLower())
                    || w.Application.ApplicationNumber.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var waitlists = await query
            .OrderBy(input.Sorting ?? "Position ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<WaitlistDto>(
            totalCount,
            waitlists.Select(MapToDto).ToList());
    }

    [AbpAuthorize(PermissionNames.Admissions_Decision_Waitlist)]
    public async Task<WaitlistDto> AddToWaitlistAsync(Guid applicationId, string notes = null)
    {
        var application = await _applicationRepository
            .GetAll()
            .Include(a => a.AppliedGrade)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.ApplicationNotFound, "Application not found.");

        if (application.Status != ApplicationStatus.UnderConsideration)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidStatusTransition,
                "Only applications under consideration can be placed on the waitlist.");

        // Check if already on waitlist
        var existingWaitlist = await _waitlistRepository
            .FirstOrDefaultAsync(w => w.ApplicationId == applicationId);

        if (existingWaitlist != null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.WaitlistAlreadyExists,
                "Application is already on the waitlist.");

        // Get next position in waitlist for this grade
        var nextPosition = await _waitlistRepository
            .GetAll()
            .Where(w => w.GradeId == application.AppliedGradeId && w.Status == WaitlistStatus.Active)
            .CountAsync() + 1;

        var waitlist = new Waitlist(
            Guid.NewGuid(),
            AbpSession.TenantId,
            applicationId,
            application.AppliedGradeId,
            nextPosition)
        {
            Notes = notes
        };

        await _waitlistRepository.InsertAsync(waitlist);

        // Update application status
        application.PlaceOnWaitlist(AbpSession.UserId ?? 0, notes);
        await _applicationRepository.UpdateAsync(application);

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(waitlist.Id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Waitlist_OfferPosition)]
    public async Task<WaitlistDto> OfferPositionAsync(Guid id, int expiryDays = 7)
    {
        var waitlist = await _waitlistRepository.GetAsync(id);

        if (waitlist.Status != WaitlistStatus.Active)
            throw new UserFriendlyException(AdmissionsExceptionCodes.CannotOfferPosition,
                "Only active waitlist entries can be offered a position.");

        // Use entity method to offer position
        waitlist.OfferPosition(expiryDays);

        await _waitlistRepository.UpdateAsync(waitlist);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Waitlist_AcceptOffer)]
    public async Task<WaitlistDto> AcceptOfferAsync(Guid id)
    {
        await EnsureEntryIsTheirsAsync(id);

        var waitlist = await _waitlistRepository.GetAsync(id);

        if (waitlist.Status != WaitlistStatus.Offered)
            throw new UserFriendlyException(AdmissionsExceptionCodes.CannotAcceptOffer,
                "Only offered positions can be accepted.");

        if (waitlist.OfferExpiryDate.HasValue && waitlist.OfferExpiryDate.Value < DateTime.UtcNow)
            throw new UserFriendlyException(AdmissionsExceptionCodes.WaitlistOfferExpired,
                "The waitlist offer has expired.");

        // Use entity method to accept offer
        waitlist.AcceptOffer();
        await _waitlistRepository.UpdateAsync(waitlist);

        /* A place taken off the waiting list is a place taken. ApproveFromWaitlist
           moves the application to Approved with a fresh offer window on it, and
           without this the family would be shown that offer and asked to accept
           the very thing they just accepted. Recorded as their own act, because
           it is. */
        var application = await _applicationRepository.GetAsync(waitlist.ApplicationId);
        application.ApproveFromWaitlist();
        application.AcceptOffer(AbpSession.UserId);
        await _applicationRepository.UpdateAsync(application);

        await CurrentUnitOfWork.SaveChangesAsync();

        // Reorder waitlist positions
        await ReorderWaitlistPositionsAsync(waitlist.GradeId);

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Waitlist_DeclineOffer)]
    public async Task<WaitlistDto> DeclineOfferAsync(Guid id)
    {
        await EnsureEntryIsTheirsAsync(id);

        var waitlist = await _waitlistRepository.GetAsync(id);

        if (waitlist.Status != WaitlistStatus.Offered)
            throw new UserFriendlyException(AdmissionsExceptionCodes.CannotDeclineOffer,
                "Only offered positions can be declined.");

        // Use entity method to decline offer
        waitlist.DeclineOffer();

        await _waitlistRepository.UpdateAsync(waitlist);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Reorder waitlist positions
        await ReorderWaitlistPositionsAsync(waitlist.GradeId);

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Waitlist_Withdraw)]
    public async Task WithdrawAsync(Guid id)
    {
        await EnsureEntryIsTheirsAsync(id);

        var waitlist = await _waitlistRepository.GetAsync(id);

        if (waitlist.Status == WaitlistStatus.Accepted || waitlist.Status == WaitlistStatus.Withdrawn)
            throw new UserFriendlyException(AdmissionsExceptionCodes.CannotWithdrawFromWaitlist,
                "Cannot withdraw from an accepted or already withdrawn waitlist entry.");

        var gradeId = waitlist.GradeId;

        // Use entity method to withdraw
        waitlist.Withdraw();

        await _waitlistRepository.UpdateAsync(waitlist);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Reorder waitlist positions
        await ReorderWaitlistPositionsAsync(gradeId);
    }

    [AbpAuthorize(PermissionNames.Admissions_Waitlist_View)]
    public async Task<WaitlistPositionDto> GetPositionAsync(Guid applicationId)
    {
        await EnsureTheirsAsync(applicationId);

        var waitlist = await _waitlistRepository
            .GetAll()
            .Include(w => w.Application)
            .Include(w => w.Grade)
            .FirstOrDefaultAsync(w => w.ApplicationId == applicationId);

        if (waitlist == null)
            return null;

        var totalInWaitlist = await _waitlistRepository
            .GetAll()
            .Where(w => w.GradeId == waitlist.GradeId && w.Status == WaitlistStatus.Active)
            .CountAsync();

        return new WaitlistPositionDto
        {
            ApplicationId = waitlist.ApplicationId,
            ApplicationNumber = waitlist.Application?.ApplicationNumber,
            GradeId = waitlist.GradeId,
            GradeName = waitlist.Grade?.GradeName,
            Position = waitlist.Position,
            AddedDate = waitlist.AddedDate,
            TotalInWaitlist = totalInWaitlist
        };
    }

    #region Private Methods

    private WaitlistDto MapToDto(Waitlist waitlist)
    {
        var dto = ObjectMapper.Map<WaitlistDto>(waitlist);

        // Map additional properties
        dto.ApplicationNumber = waitlist.Application?.ApplicationNumber;
        dto.ApplicantName = waitlist.Application?.GetProspectiveStudentFullName();
        dto.GradeName = waitlist.Grade?.GradeName;

        return dto;
    }

    private async Task ReorderWaitlistPositionsAsync(Guid gradeId)
    {
        var activeWaitlists = await _waitlistRepository
            .GetAll()
            .Where(w => w.GradeId == gradeId && w.Status == WaitlistStatus.Active)
            .OrderBy(w => w.AddedDate)
            .ToListAsync();

        for (int i = 0; i < activeWaitlists.Count; i++)
        {
            activeWaitlists[i].Position = i + 1;
            await _waitlistRepository.UpdateAsync(activeWaitlists[i]);
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    #endregion
}
