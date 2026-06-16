namespace psms.Domain.Shared.Storage;

/// <summary>
/// A short-lived ticket for a direct browser-to-storage upload. The client
/// PUTs the file bytes to <see cref="UploadUrl"/> (bytes never pass through
/// our server), then the application records <see cref="PublicUrl"/> /
/// <see cref="ObjectKey"/> on the owning entity.
/// </summary>
public class FileUploadTicket
{
    /// <summary>Pre-authorized URL the client uploads the file to (PUT).</summary>
    public string UploadUrl { get; set; }

    /// <summary>The public (or to-be-signed) URL the file is served from.</summary>
    public string PublicUrl { get; set; }

    /// <summary>Storage object key (bucket-relative path).</summary>
    public string ObjectKey { get; set; }
}
