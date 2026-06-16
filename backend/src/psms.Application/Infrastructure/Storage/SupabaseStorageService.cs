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

    public Task<string> UploadAsync(string path, byte[] data, string contentType)
        => UploadAsync(GetConfig().BucketName, path, data, contentType);

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
            CannedACL = S3CannedACL.PublicRead,
        };

        await client.PutObjectAsync(request);

        return $"{config.PublicUrl}/{bucket}/{path}";
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

    public async Task<FileUploadTicket> CreateUploadTicketAsync(string bucket, string objectKey)
    {
        var config = GetConfig();
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("apikey", config.ServiceKey);
        http.DefaultRequestHeaders.Add("Authorization", "Bearer " + config.ServiceKey);

        // Supabase mints a one-time signed upload URL; the client then PUTs the
        // bytes straight to storage (they never transit this server).
        var resp = await http.PostAsync(
            $"{config.ProjectUrl}/storage/v1/object/upload/sign/{bucket}/{objectKey}",
            new StringContent("{}", Encoding.UTF8, "application/json"));
        resp.EnsureSuccessStatusCode();
        var relative = JObject.Parse(await resp.Content.ReadAsStringAsync())["url"]?.ToString();

        return new FileUploadTicket
        {
            UploadUrl = $"{config.ProjectUrl}/storage/v1{relative}",
            PublicUrl = $"{config.PublicUrl}/{bucket}/{objectKey}",
            ObjectKey = objectKey,
        };
    }

    public async Task<string> CreateSignedDownloadUrlAsync(string bucket, string objectKey, int expirySeconds = 3600)
    {
        var config = GetConfig();
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("apikey", config.ServiceKey);
        http.DefaultRequestHeaders.Add("Authorization", "Bearer " + config.ServiceKey);

        var resp = await http.PostAsync(
            $"{config.ProjectUrl}/storage/v1/object/sign/{bucket}/{objectKey}",
            new StringContent($"{{\"expiresIn\":{expirySeconds}}}", Encoding.UTF8, "application/json"));
        resp.EnsureSuccessStatusCode();
        var relative = JObject.Parse(await resp.Content.ReadAsStringAsync())["signedURL"]?.ToString();

        return $"{config.ProjectUrl}/storage/v1{relative}";
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
