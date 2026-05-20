using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace psms.Domain.Learning.Entities
{
    /// <summary>
    /// A single historical version of a LearningMaterial's file. Each
    /// "Upload new version" creates a new row (incrementing
    /// <see cref="VersionNumber"/> within the scope of the parent material).
    /// Restoring an old version creates a NEW row that copies the file
    /// pointer from the chosen historical row — the original history is
    /// never overwritten.
    ///
    /// Retention follows business rule LM-003 (up to 10 versions per
    /// material).
    /// </summary>
    [Table("LearningMaterialVersions")]
    public class LearningMaterialVersion : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxFileNameLength = 256;
        public const int MaxFileUrlLength = 500;
        public const int MaxContentTypeLength = 100;
        public const int MaxChangeDescriptionLength = 500;

        public int? TenantId { get; set; }

        [Required]
        public Guid LearningMaterialId { get; set; }

        /// <summary>1-based sequential number, unique per LearningMaterialId.</summary>
        [Required]
        public int VersionNumber { get; set; }

        [Required]
        [StringLength(MaxChangeDescriptionLength)]
        public string ChangeDescription { get; set; }

        [StringLength(MaxFileNameLength)]
        public string FileName { get; set; }

        [StringLength(MaxFileUrlLength)]
        public string FileUrl { get; set; }

        public long? FileSizeBytes { get; set; }

        [StringLength(MaxContentTypeLength)]
        public string ContentType { get; set; }

        /// <summary>
        /// User who created this version. Already captured via FullAudited's
        /// CreatorUserId; kept here explicitly so callers don't have to fall
        /// back to the audit columns when displaying author info.
        /// </summary>
        public long UploadedByUserId { get; set; }

        // Navigation
        [ForeignKey(nameof(LearningMaterialId))]
        public virtual LearningMaterial LearningMaterial { get; set; }

        protected LearningMaterialVersion() { }

        public LearningMaterialVersion(
            Guid id,
            int? tenantId,
            Guid learningMaterialId,
            int versionNumber,
            string changeDescription,
            long uploadedByUserId)
        {
            Id = id;
            TenantId = tenantId;
            LearningMaterialId = learningMaterialId;
            VersionNumber = versionNumber;
            ChangeDescription = changeDescription;
            UploadedByUserId = uploadedByUserId;
        }

        public void SetFile(string fileName, string fileUrl, long? fileSizeBytes, string contentType)
        {
            FileName = fileName;
            FileUrl = fileUrl;
            FileSizeBytes = fileSizeBytes;
            ContentType = contentType;
        }
    }
}
