using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization.Users;
using psms.Communication.Contacts.Dto;
using psms.Domain.Communication.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Communication.Contacts;

/// <summary>
/// COMM-04: the signed-in user's contact details + push devices. Authenticated
/// users manage only their own record (scoped by AbpSession.UserId).
/// </summary>
[AbpAuthorize]
public class UserContactAppService : ApplicationService, IUserContactAppService
{
    private readonly IRepository<User, long> _userRepository;
    private readonly IRepository<UserDeviceToken, Guid> _deviceTokenRepository;
    private readonly IContactResolver _contactResolver;

    public UserContactAppService(
        IRepository<User, long> userRepository,
        IRepository<UserDeviceToken, Guid> deviceTokenRepository,
        IContactResolver contactResolver)
    {
        _userRepository = userRepository;
        _deviceTokenRepository = deviceTokenRepository;
        _contactResolver = contactResolver;
    }

    public async Task<MyContactDto> GetMyContactAsync()
    {
        var userId = GetCurrentUserId();
        var contacts = await _contactResolver.ResolveAsync(userId);

        var devices = await _deviceTokenRepository.GetAll()
            .Where(t => t.UserId == userId && t.IsActive)
            .OrderByDescending(t => t.LastSeenDate)
            .Select(t => new DeviceTokenDto
            {
                Id = t.Id,
                Platform = t.Platform,
                DeviceName = t.DeviceName,
                LastSeenDate = t.LastSeenDate
            })
            .ToListAsync();

        return new MyContactDto
        {
            Email = contacts.Email,
            MobileNumber = contacts.MobileNumber,
            WhatsAppNumber = contacts.WhatsAppNumber,
            Devices = devices
        };
    }

    public async Task<MyContactDto> UpdateMyContactAsync(UpdateMyContactDto input)
    {
        var userId = GetCurrentUserId();
        var user = await _userRepository.GetAsync(userId);

        var newMobile = string.IsNullOrWhiteSpace(input.MobileNumber) ? null : input.MobileNumber.Trim();
        if (user.PhoneNumber != newMobile)
        {
            user.PhoneNumber = newMobile;
            // Number changed via self-service → it's no longer a verified number.
            // (SMS verification is a later ticket; keep the confirmed flag honest.)
            user.IsPhoneNumberConfirmed = false;
        }
        user.WhatsAppNumber = string.IsNullOrWhiteSpace(input.WhatsAppNumber) ? null : input.WhatsAppNumber.Trim();
        await _userRepository.UpdateAsync(user);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetMyContactAsync();
    }

    public async Task RegisterDeviceAsync(RegisterDeviceDto input)
    {
        var userId = GetCurrentUserId();
        var token = input.Token?.Trim();
        if (string.IsNullOrWhiteSpace(token))
            throw new UserFriendlyException("A device token is required.");
        if (!Enum.IsDefined(typeof(DevicePlatform), input.Platform))
            throw new UserFriendlyException("Unknown device platform.");

        // A push token is globally unique to one physical device (possession of the
        // opaque token = control of that device's push routing), so if it already
        // exists we re-home it to the caller — this is the "device changed hands /
        // user logged in on a shared device" case, and it MUST move so the previous
        // owner stops receiving that device's pushes. Move TenantId too, or the row
        // would stay under the old tenant and be invisible to the new owner. Log a
        // cross-user transfer for an audit trail (the threat requires already
        // possessing the victim's high-entropy token).
        var existing = await _deviceTokenRepository.FirstOrDefaultAsync(t => t.Token == token);
        if (existing != null)
        {
            if (existing.UserId != userId)
                Logger.Warn($"Push device token re-homed from user {existing.UserId} to user {userId}.");
            existing.UserId = userId;
            existing.TenantId = AbpSession.TenantId;
            existing.Touch(input.Platform, input.DeviceName?.Trim());
            await _deviceTokenRepository.UpdateAsync(existing);
        }
        else
        {
            await _deviceTokenRepository.InsertAsync(new UserDeviceToken(
                Guid.NewGuid(), AbpSession.TenantId, userId, token, input.Platform, input.DeviceName?.Trim()));
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    public async Task UnregisterDeviceAsync(string token)
    {
        var userId = GetCurrentUserId();
        var trimmed = token?.Trim();
        var device = await _deviceTokenRepository
            .FirstOrDefaultAsync(t => t.Token == trimmed && t.UserId == userId);

        if (device == null)
            return; // already gone — idempotent

        device.Deactivate();
        await _deviceTokenRepository.UpdateAsync(device);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    private long GetCurrentUserId()
    {
        if (!AbpSession.UserId.HasValue)
            throw new UserFriendlyException("You must be signed in.");
        return AbpSession.UserId.Value;
    }
}
