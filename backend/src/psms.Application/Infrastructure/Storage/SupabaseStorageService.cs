using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using psms.Domain.Shared.Storage;

namespace psms.Infrastructure.Storage;

/// <summary>
/// Supabase Storage implementation using S3-compatible API.
/// </summary>
public class SupabaseStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;

    public SupabaseStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public string DefaultBucketName => GetConfig().BucketName;

    public Task<string> UploadAsync(string path, byte[] data, string contentType)
        => UploadAsync(GetConfig().BucketName, path, data, contentType);

    /// <summary>
    /// Uploads bytes and returns the object key, NOT a URL.
    /// <para>
    /// These objects are private. They used to be written with
    /// <c>S3CannedACL.PublicRead</c> and the caller stored the resulting
    /// permanent public link — which, for report cards, meant a named child's
    /// full academic record was readable by anyone with the URL, forever, with
    /// no authentication. Callers now keep the key and mint a short-lived
    /// signed URL per request via <see cref="CreateSignedDownloadUrlAsync"/>.
    /// </para>
    /// </summary>
    public async Task<string> UploadAsync(string bucket, string path, byte[] data, string contentType)
    {
        var config = GetConfig();

        using var client = CreateClient(config);
        using var stream = new MemoryStream(data);

        var request = new PutObjectRequest
        {
            BucketName = bucket,
            Key = path,
            InputStream = stream,
            ContentType = contentType,
        };

        await client.PutObjectAsync(request);

        return path;
    }

    public Task DeleteAsync(string path)
        => DeleteAsync(GetConfig().BucketName, path);

    public async Task DeleteAsync(string bucket, string path)
    {
        using var client = CreateClient(GetConfig());

        var request = new DeleteObjectRequest
        {
            BucketName = bucket,
            Key = path,
        };

        await client.DeleteObjectAsync(request);
    }

    // Single shared client — minting tickets is request-frequent, so a new
    // HttpClient per call would risk socket exhaustion. Per-request headers
    // go on the HttpRequestMessage, not DefaultRequestHeaders.
    private static readonly HttpClient Http = new HttpClient();

    private async Task<JObject> PostSignAsync(string url, string jsonBody)
    {
        var config = GetConfig();
        if (string.IsNullOrEmpty(config.ServiceKey))
            throw new InvalidOperationException(
                "SupabaseStorage:ServiceKey is not configured — set env var SupabaseStorage__ServiceKey to mint signed URLs.");

        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.TryAddWithoutValidation("apikey", config.ServiceKey);
        req.Headers.TryAddWithoutValidation("Authorization", "Bearer " + config.ServiceKey);
        req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var resp = await Http.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        return JObject.Parse(await resp.Content.ReadAsStringAsync());
    }

    public async Task<FileUploadTicket> CreateUploadTicketAsync(string bucket, string objectKey)
    {
        var config = GetConfig();
        // Supabase mints a one-time signed upload URL; the client then PUTs the
        // bytes straight to storage (they never transit this server).
        var result = await PostSignAsync(
            $"{config.ProjectUrl}/storage/v1/object/upload/sign/{bucket}/{objectKey}", "{}");
        var relative = result["url"]?.ToString();

        return new FileUploadTicket
        {
            UploadUrl = $"{config.ProjectUrl}/storage/v1{relative}",
            PublicUrl = GetPublicUrl(bucket, objectKey),
            ObjectKey = objectKey,
        };
    }

    public async Task<string> CreateSignedDownloadUrlAsync(string bucket, string objectKey, int expirySeconds = 3600)
    {
        var config = GetConfig();
        var result = await PostSignAsync(
            $"{config.ProjectUrl}/storage/v1/object/sign/{bucket}/{objectKey}",
            $"{{\"expiresIn\":{expirySeconds}}}");
        return $"{config.ProjectUrl}/storage/v1{result["signedURL"]?.ToString()}";
    }

    public string GetPublicUrl(string bucket, string objectKey)
        => $"{GetConfig().PublicUrl}/{bucket}/{objectKey}";

    public async Task<FileObjectInfo> GetObjectInfoAsync(string bucket, string objectKey)
    {
        using var client = CreateClient(GetConfig());
        try
        {
            var meta = await client.GetObjectMetadataAsync(bucket, objectKey);
            return new FileObjectInfo
            {
                SizeBytes = meta.ContentLength,
                ContentType = meta.Headers.ContentType,
            };
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    private AmazonS3Client CreateClient(SupabaseStorageConfig config)
    {
        var s3Config = new AmazonS3Config
        {
            ServiceURL = config.Endpoint,
            ForcePathStyle = true,
        };

        return new AmazonS3Client(config.AccessKey, config.SecretKey, s3Config);
    }

    private SupabaseStorageConfig GetConfig()
    {
        var section = _configuration.GetSection("SupabaseStorage");
        var publicUrl = section["PublicUrl"] ?? throw new InvalidOperationException("SupabaseStorage:PublicUrl not configured");
        return new SupabaseStorageConfig
        {
            Endpoint = section["Endpoint"] ?? throw new InvalidOperationException("SupabaseStorage:Endpoint not configured"),
            AccessKey = section["AccessKey"] ?? throw new InvalidOperationException("SupabaseStorage:AccessKey not configured"),
            SecretKey = section["SecretKey"] ?? throw new InvalidOperationException("SupabaseStorage:SecretKey not configured"),
            BucketName = section["BucketName"] ?? throw new InvalidOperationException("SupabaseStorage:BucketName not configured"),
            PublicUrl = publicUrl,
            // Project base URL (e.g. https://<ref>.supabase.co). Derived from
            // PublicUrl when not set explicitly.
            ProjectUrl = section["ProjectUrl"]
                ?? publicUrl.Replace("/storage/v1/object/public", string.Empty).TrimEnd('/'),
            // Service-role key — required only for signed upload/download URLs.
            // Supplied via env (SupabaseStorage__ServiceKey); never committed.
            ServiceKey = section["ServiceKey"],
        };
    }

    private class SupabaseStorageConfig
    {
        public string Endpoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }
        public string PublicUrl { get; set; }
        public string ProjectUrl { get; set; }
        public string ServiceKey { get; set; }
    }
}
