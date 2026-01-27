using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Communication.Entities
{
    /// <summary>
    /// Represents a shared document in the system
    /// </summary>
    [Table("Documents")]
    public class Document : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxTitleLength = 200;
        public const int MaxDescriptionLength = 1000;
        public const int MaxFileNameLength = 256;
        public const int MaxFileUrlLength = 500;
        public const int MaxContentTypeLength = 100;
        public const int MaxCategoryLength = 50;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Title of the document
        /// </summary>
        [Required]
        [StringLength(MaxTitleLength)]
        public string Title { get; set; }

        /// <summary>
        /// Description of the document
        /// </summary>
        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        /// <summary>
        /// Original file name
        /// </summary>
        [Required]
        [StringLength(MaxFileNameLength)]
        public string FileName { get; set; }

        /// <summary>
        /// URL to the file
        /// </summary>
        [Required]
        [StringLength(MaxFileUrlLength)]
        public string FileUrl { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// MIME type
        /// </summary>
        [StringLength(MaxContentTypeLength)]
        public string ContentType { get; set; }

        /// <summary>
        /// Category of document
        /// </summary>
        [StringLength(MaxCategoryLength)]
        public string Category { get; set; }

        /// <summary>
        /// Document type
        /// </summary>
        [Required]
        public SharedDocumentType DocumentType { get; set; }

        /// <summary>
        /// Target audience for the document
        /// </summary>
        [Required]
        public DocumentAudience TargetAudience { get; set; }

        /// <summary>
        /// Whether the document is published
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// Date when published
        /// </summary>
        public DateTime? PublishedDate { get; set; }

        /// <summary>
        /// Academic year the document belongs to (optional)
        /// </summary>
        public Guid? AcademicYearId { get; set; }

        /// <summary>
        /// User who uploaded the document
        /// </summary>
        [Required]
        public long UploadedByUserId { get; set; }

        /// <summary>
        /// Number of downloads
        /// </summary>
        public int DownloadCount { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Document()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Document(
            Guid id,
            int? tenantId,
            string title,
            string fileName,
            string fileUrl,
            long fileSizeBytes,
            SharedDocumentType documentType,
            DocumentAudience targetAudience,
            long uploadedByUserId) : this()
        {
            Id = id;
            TenantId = tenantId;
            Title = title;
            FileName = fileName;
            FileUrl = fileUrl;
            FileSizeBytes = fileSizeBytes;
            DocumentType = documentType;
            TargetAudience = targetAudience;
            UploadedByUserId = uploadedByUserId;
            IsPublished = false;
            DownloadCount = 0;
            IsDeleted = false;
        }

        /// <summary>
        /// Publishes the document
        /// </summary>
        public void Publish()
        {
            IsPublished = true;
            PublishedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Unpublishes the document
        /// </summary>
        public void Unpublish()
        {
            IsPublished = false;
        }

        /// <summary>
        /// Increments download count
        /// </summary>
        public void IncrementDownloadCount()
        {
            DownloadCount++;
        }
    }
}
