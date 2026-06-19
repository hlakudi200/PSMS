using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using psms.Authorization.Users;
using psms.Domain.Communication.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Communication.Contacts;

/// <summary>
/// COMM-04: reads the recipient's contact fields off the user and their active
/// device tokens. Tenant scoping comes from the ambient ABP filters; a future
/// session-less caller (background job) must establish the tenant context.
/// </summary>
public class ContactResolver : IContactResolver, ITransientDependency
{
    private readonly IRepository<User, long> _userRepository;
    private readonly IRepository<UserDeviceToken, Guid> _deviceTokenRepository;

    public ContactResolver(
        IRepository<User, long> userRepository,
        IRepository<UserDeviceToken, Guid> deviceTokenRepository)
    {
        _userRepository = userRepository;
        _deviceTokenRepository = deviceTokenRepository;
    }

    public async Task<RecipientContacts> ResolveAsync(long userId)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return new RecipientContacts { UserId = userId };

        var contacts = new RecipientContacts
        {
            UserId = userId,
            Email = user.EmailAddress,
            MobileNumber = user.PhoneNumber,
            WhatsAppNumber = user.WhatsAppNumber
        };

        contacts.DeviceTokens = await _deviceTokenRepository.GetAll()
            .Where(t => t.UserId == userId && t.IsActive)
            .Select(t => new RecipientDeviceToken { Token = t.Token, Platform = t.Platform })
            .ToListAsync();

        return contacts;
    }
}
