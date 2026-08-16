using System.ComponentModel.DataAnnotations;
using psms.Domain.Shared.Enums;

namespace psms.Tenancy.Branding.Dto;

/// <summary>
/// Step 1 of a branding image upload: ask for a one-time signed URL. The
/// client PUTs the bytes straight to storage, then calls SetAsset with the
/// returned object key.
/// </summary>
public class RequestBrandingUploadUrlDto
{
    [Required]
    public BrandingAssetType AssetType { get; set; }

    [Required]
    [StringLength(260)]
    public string FileName { get; set; }
}

/// <summary>
/// Step 2: record an image that has already been uploaded to storage. The
/// server re-reads the object's real size and content type from storage and
/// validates those — nothing about the file is trusted from the client.
/// </summary>
public class SetBrandingAssetDto
{
    [Required]
    public BrandingAssetType AssetType { get; set; }

    /// <summary>Object key returned by RequestAssetUploadUrl.</summary>
    [Required]
    [StringLength(500)]
    public string ObjectKey { get; set; }
}

/// <summary>Clears an image, reverting to the text/stock fallback.</summary>
public class RemoveBrandingAssetDto
{
    [Required]
    public BrandingAssetType AssetType { get; set; }
}
