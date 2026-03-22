using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
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

    public async Task<string> UploadAsync(string path, byte[] data, string contentType)
    {
        var config = GetConfig();

        using var client = CreateClient(config);
        using var stream = new MemoryStream(data);

        var request = new PutObjectRequest
        {
            BucketName = config.BucketName,
            Key = path,
            InputStream = stream,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead,
        };

        await client.PutObjectAsync(request);

        return $"{config.PublicUrl}/{config.BucketName}/{path}";
    }

    public async Task DeleteAsync(string path)
    {
        var config = GetConfig();

        using var client = CreateClient(config);

        var request = new DeleteObjectRequest
        {
            BucketName = config.BucketName,
            Key = path,
        };

        await client.DeleteObjectAsync(request);
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
        return new SupabaseStorageConfig
        {
            Endpoint = section["Endpoint"] ?? throw new InvalidOperationException("SupabaseStorage:Endpoint not configured"),
            AccessKey = section["AccessKey"] ?? throw new InvalidOperationException("SupabaseStorage:AccessKey not configured"),
            SecretKey = section["SecretKey"] ?? throw new InvalidOperationException("SupabaseStorage:SecretKey not configured"),
            BucketName = section["BucketName"] ?? throw new InvalidOperationException("SupabaseStorage:BucketName not configured"),
            PublicUrl = section["PublicUrl"] ?? throw new InvalidOperationException("SupabaseStorage:PublicUrl not configured"),
        };
    }

    private class SupabaseStorageConfig
    {
        public string Endpoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }
        public string PublicUrl { get; set; }
    }
}
