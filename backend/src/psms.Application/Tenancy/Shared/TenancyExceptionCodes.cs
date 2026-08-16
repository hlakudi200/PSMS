namespace psms.Tenancy.Shared;

public static class TenancyExceptionCodes
{
    // SchoolBranding
    public const string BrandingTenantRequired = "TEN_BRANDING_TENANT_REQUIRED";
    public const string InvalidBrandingColor = "TEN_BRANDING_INVALID_COLOR";
    public const string InvalidBrandingUpload = "TEN_BRANDING_INVALID_UPLOAD";
    public const string BrandingImageTooLarge = "TEN_BRANDING_IMAGE_TOO_LARGE";
    public const string BrandingImageTypeNotAllowed = "TEN_BRANDING_IMAGE_TYPE_NOT_ALLOWED";
    public const string TenantNotFound = "TEN_TENANT_NOT_FOUND";
}
