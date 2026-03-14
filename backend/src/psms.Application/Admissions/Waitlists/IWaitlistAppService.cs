using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Admissions.Waitlists.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Admissions.Waitlists;

/// <summary>
/// Service for managing admission waitlists.
/// Implements ADM-021 to ADM-024.
/// </summary>
public interface IWaitlistAppService : IApplicationService
{
    /// <summary>
    /// Gets a waitlist entry by ID.
    /// </summary>
    Task<WaitlistDto> GetAsync(Guid id);

    /// <summary>
    /// Gets the waitlist entry for an application.
    /// </summary>
    Task<WaitlistDto> GetByApplicationAsync(Guid applicationId);

    /// <summary>
    /// Gets all waitlist entries for a grade.
    /// </summary>
    Task<List<WaitlistDto>> GetByGradeAsync(Guid gradeId);

    /// <summary>
    /// Gets all waitlist entries with filters.
    /// </summary>
    Task<PagedResultDto<WaitlistDto>> GetAllAsync(GetWaitlistsInput input);

    /// <summary>
    /// Adds an application to the waitlist.
    /// </summary>
    Task<WaitlistDto> AddToWaitlistAsync(Guid applicationId, string notes = null);

    /// <summary>
    /// Offers a position to a waitlisted applicant.
    /// </summary>
    Task<WaitlistDto> OfferPositionAsync(Guid id, int expiryDays = 7);

    /// <summary>
    /// Accepts a waitlist offer.
    /// </summary>
    Task<WaitlistDto> AcceptOfferAsync(Guid id);

    /// <summary>
    /// Declines a waitlist offer.
    /// </summary>
    Task<WaitlistDto> DeclineOfferAsync(Guid id);

    /// <summary>
    /// Withdraws from the waitlist.
    /// </summary>
    Task WithdrawAsync(Guid id);

    /// <summary>
    /// Gets the current position for an application on the waitlist.
    /// </summary>
    Task<WaitlistPositionDto> GetPositionAsync(Guid applicationId);
}
