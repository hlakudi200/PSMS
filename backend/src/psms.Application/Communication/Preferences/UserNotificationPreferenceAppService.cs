using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Communication.Preferences.Dto;
using psms.Domain.Communication.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Communication.Preferences;

/// <summary>
/// COMM-05: self-service notification consents + category preferences. The
/// dispatcher reads these to gate external channels (POPIA opt-in + per-category
/// opt-outs); in-app is exempt. Every operation is scoped to AbpSession.UserId.
/// </summary>
[AbpAuthorize]
public class UserNotificationPreferenceAppService : ApplicationService, IUserNotificationPreferenceAppService
{
    private const string PreferenceSource = "preferences-screen";

    private readonly IRepository<NotificationConsent, Guid> _consentRepository;
    private readonly IRepository<NotificationPreference, Guid> _preferenceRepository;

    public UserNotificationPreferenceAppService(
        IRepository<NotificationConsent, Guid> consentRepository,
        IRepository<NotificationPreference, Guid> preferenceRepository)
    {
        _consentRepository = consentRepository;
        _preferenceRepository = preferenceRepository;
    }

    public async Task<MyPreferencesDto> GetMyPreferencesAsync()
    {
        var userId = GetCurrentUserId();

        var consents = await _consentRepository.GetAll()
            .Where(c => c.UserId == userId)
            .Select(c => new ChannelConsentDto
            {
                Channel = c.Channel,
                IsGranted = c.IsGranted,
                GrantedDate = c.GrantedDate,
                RevokedDate = c.RevokedDate
            })
            .ToListAsync();

        var preferences = await _preferenceRepository.GetAll()
            .Where(p => p.UserId == userId)
            .Select(p => new CategoryPreferenceDto
            {
                Channel = p.Channel,
                Category = p.Category,
                IsEnabled = p.IsEnabled
            })
            .ToListAsync();

        return new MyPreferencesDto { Consents = consents, Preferences = preferences };
    }

    public async Task SetConsentAsync(SetConsentDto input)
    {
        var userId = GetCurrentUserId();

        if (!Enum.IsDefined(typeof(NotificationChannel), input.Channel))
            throw new UserFriendlyException("Unknown channel.");
        if (input.Channel == NotificationChannel.InApp)
            throw new UserFriendlyException("In-app notifications don't require consent.");

        var consent = await _consentRepository
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Channel == input.Channel);

        if (consent == null)
        {
            consent = new NotificationConsent(Guid.NewGuid(), AbpSession.TenantId, userId, input.Channel);
            if (input.IsGranted) consent.Grant(PreferenceSource); else consent.Revoke(PreferenceSource);
            await _consentRepository.InsertAsync(consent);
        }
        else
        {
            if (input.IsGranted) consent.Grant(PreferenceSource); else consent.Revoke(PreferenceSource);
            await _consentRepository.UpdateAsync(consent);
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    public async Task SetPreferenceAsync(SetPreferenceDto input)
    {
        var userId = GetCurrentUserId();

        if (!Enum.IsDefined(typeof(NotificationChannel), input.Channel))
            throw new UserFriendlyException("Unknown channel.");
        if (!Enum.IsDefined(typeof(NotificationType), input.Category))
            throw new UserFriendlyException("Unknown category.");
        // In-app is the inbox of record and is never gated by preferences, so don't
        // accept an in-app preference that the dispatcher would silently ignore.
        if (input.Channel == NotificationChannel.InApp)
            throw new UserFriendlyException("In-app notifications can't be muted per category.");

        var preference = await _preferenceRepository
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Channel == input.Channel && p.Category == input.Category);

        if (preference == null)
        {
            preference = new NotificationPreference(
                Guid.NewGuid(), AbpSession.TenantId, userId, input.Channel, input.Category, input.IsEnabled);
            await _preferenceRepository.InsertAsync(preference);
        }
        else
        {
            preference.SetEnabled(input.IsEnabled);
            await _preferenceRepository.UpdateAsync(preference);
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    private long GetCurrentUserId()
    {
        if (!AbpSession.UserId.HasValue)
            throw new UserFriendlyException("You must be signed in.");
        return AbpSession.UserId.Value;
    }
}
