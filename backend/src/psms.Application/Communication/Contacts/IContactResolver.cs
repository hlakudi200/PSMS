using System.Threading.Tasks;

namespace psms.Communication.Contacts;

/// <summary>
/// COMM-04: resolves a recipient to the addresses an external channel needs
/// (email, mobile, WhatsApp, active push tokens). The channel providers added in
/// COMM-07+ call this; the dispatcher uses it to decide which channels are even
/// possible for a recipient.
/// </summary>
public interface IContactResolver
{
    Task<RecipientContacts> ResolveAsync(long userId);
}
