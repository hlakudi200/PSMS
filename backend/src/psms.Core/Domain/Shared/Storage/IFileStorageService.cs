using System.Threading.Tasks;

namespace psms.Domain.Shared.Storage;

/// <summary>
/// Abstraction for cloud file storage operations.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to the default bucket and returns the public URL.
    /// </summary>
    Task<string> UploadAsync(string path, byte[] data, string contentType);

    /// <summary>
    /// Uploads a file to the given bucket and returns the public URL.
    /// </summary>
    Task<string> UploadAsync(string bucket, string path, byte[] data, string contentType);

    /// <summary>
    /// Deletes a file by its storage path in the default bucket.
    /// </summary>
    Task DeleteAsync(string path);

    /// <summary>
    /// Deletes a file by its storage path in the given bucket.
    /// </summary>
    Task DeleteAsync(string bucket, string path);

    /// <summary>
    /// Mints a short-lived ticket for a direct browser-to-storage upload, so
    /// the file bytes never pass through this server (scalable for large
    /// files). The client PUTs the bytes to the ticket's UploadUrl.
    /// </summary>
    Task<FileUploadTicket> CreateUploadTicketAsync(string bucket, string objectKey);

    /// <summary>
    /// Returns a time-limited signed URL to download a private object.
    /// </summary>
    Task<string> CreateSignedDownloadUrlAsync(string bucket, string objectKey, int expirySeconds = 3600);

    /// <summary>The public URL an object is served from.</summary>
    string GetPublicUrl(string bucket, string objectKey);

    /// <summary>
    /// Real object metadata (size/type) from storage, or null if the object
    /// doesn't exist. Used to verify a direct-uploaded file server-side.
    /// </summary>
    Task<FileObjectInfo> GetObjectInfoAsync(string bucket, string objectKey);
}
