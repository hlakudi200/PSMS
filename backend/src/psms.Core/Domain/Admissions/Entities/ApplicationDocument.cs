using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Admissions.Entities
{
    /// <summary>
    /// Represents a document uploaded for an application
    /// </summary>
    [Table("ApplicationDocuments")]
    public class ApplicationDocument : CreationAuditedEntity<Guid>
    {
        public const int MaxDocumentNameLength = 200;
        public const int MaxFileNameLength = 256;
        public const int MaxFileUrlLength = 500;
        public const int MaxContentTypeLength = 100;

        [Required]
        public Guid ApplicationId { get; set; }

        [Required]
        public DocumentCategory Category { get; set; }

        [Required]
        [StringLength(MaxDocumentNameLength)]
        public string DocumentName { get; set; }

        [Required]
        [StringLength(MaxFileNameLength)]
        public string FileName { get; set; }

        [Required]
        [StringLength(MaxFileUrlLength)]
        public string FileUrl { get; set; }

        public long FileSizeBytes { get; set; }

        [StringLength(MaxContentTypeLength)]
        public string ContentType { get; set; }

        [Required]
        public DateTime UploadedDate { get; set; }

        public bool IsRequired { get; set; }

        public bool IsVerified { get; set; }

        public long? VerifiedByUserId { get; set; }

        public DateTime? VerifiedDate { get; set; }

        [ForeignKey(nameof(ApplicationId))]
        public virtual Application Application { get; set; }

        protected ApplicationDocument() { }

        public ApplicationDocument(Guid id, Guid applicationId, DocumentCategory category,
            string documentName, string fileName, string fileUrl, long fileSizeBytes)
        {
            Id = id;
            ApplicationId = applicationId;
            Category = category;
            DocumentName = documentName;
            FileName = fileName;
            FileUrl = fileUrl;
            FileSizeBytes = fileSizeBytes;
            UploadedDate = DateTime.UtcNow;
        }

        public void Verify(long userId)
        {
            IsVerified = true;
            VerifiedByUserId = userId;
            VerifiedDate = DateTime.UtcNow;
        }
    }
}
