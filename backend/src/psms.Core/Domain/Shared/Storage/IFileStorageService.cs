using System.Threading.Tasks;

namespace psms.Domain.Shared.Storage;

/// <summary>
/// Abstraction for cloud file storage operations.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file and returns the public URL.
    /// </summary>
    Task<string> UploadAsync(string path, byte[] data, string contentType);

    /// <summary>
    /// Deletes a file by its storage path.
    /// </summary>
    Task DeleteAsync(string path);
}
