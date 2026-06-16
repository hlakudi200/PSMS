namespace psms.Domain.Shared.Storage;

/// <summary>
/// Metadata for a stored object, used to verify a direct-uploaded file's
/// real size/type server-side (the client's reported values are untrusted).
/// </summary>
public class FileObjectInfo
{
    public long SizeBytes { get; set; }
    public string ContentType { get; set; }
}
