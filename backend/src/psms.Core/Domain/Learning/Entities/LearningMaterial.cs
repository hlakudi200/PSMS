using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Learning.Entities
{
    /// <summary>
    /// Represents learning materials shared with students
    /// </summary>
    [Table("LearningMaterials")]
    public class LearningMaterial : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxTitleLength = 200;
        public const int MaxDescriptionLength = 2000;
        public const int MaxFileNameLength = 256;
        public const int MaxFileUrlLength = 500;
        public const int MaxContentTypeLength = 100;
        public const int MaxExternalLinkLength = 500;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Reference to the class-subject
        /// </summary>
        [Required]
        public Guid ClassSubjectId { get; set; }

        /// <summary>
        /// Reference to the term (optional)
        /// </summary>
        public Guid? TermId { get; set; }

        /// <summary>
        /// Title of the learning material
        /// </summary>
        [Required]
        [StringLength(MaxTitleLength)]
        public string Title { get; set; }

        /// <summary>
        /// Description of the material
        /// </summary>
        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        /// <summary>
        /// Type of learning material
        /// </summary>
        [Required]
        public LearningMaterialType MaterialType { get; set; }

        /// <summary>
        /// Original file name (if uploaded)
        /// </summary>
        [StringLength(MaxFileNameLength)]
        public string FileName { get; set; }

        /// <summary>
        /// URL to the file (if uploaded)
        /// </summary>
        [StringLength(MaxFileUrlLength)]
        public string FileUrl { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long? FileSizeBytes { get; set; }

        /// <summary>
        /// MIME type of the file
        /// </summary>
        [StringLength(MaxContentTypeLength)]
        public string ContentType { get; set; }

        /// <summary>
        /// External link (for videos, websites, etc.)
        /// </summary>
        [StringLength(MaxExternalLinkLength)]
        public string ExternalLink { get; set; }

        /// <summary>
        /// Order for display
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Whether the material is published to students
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// Date when the material was published
        /// </summary>
        public DateTime? PublishedDate { get; set; }

        /// <summary>
        /// Teacher who uploaded the material
        /// </summary>
        [Required]
        public long UploadedByUserId { get; set; }

        /// <summary>
        /// Number of downloads/views
        /// </summary>
        public int ViewCount { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(ClassSubjectId))]
        public virtual ClassSubject ClassSubject { get; set; }

        [ForeignKey(nameof(TermId))]
        public virtual Term Term { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected LearningMaterial()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public LearningMaterial(
            Guid id,
            int? tenantId,
            Guid classSubjectId,
            string title,
            LearningMaterialType materialType,
            long uploadedByUserId) : this()
        {
            Id = id;
            TenantId = tenantId;
            ClassSubjectId = classSubjectId;
            Title = title;
            MaterialType = materialType;
            UploadedByUserId = uploadedByUserId;
            IsPublished = false;
            ViewCount = 0;
            IsDeleted = false;
        }

        /// <summary>
        /// Sets file information
        /// </summary>
        public void SetFile(string fileName, string fileUrl, long fileSizeBytes, string contentType)
        {
            FileName = fileName;
            FileUrl = fileUrl;
            FileSizeBytes = fileSizeBytes;
            ContentType = contentType;
        }

        /// <summary>
        /// Publishes the material to students
        /// </summary>
        public void Publish()
        {
            IsPublished = true;
            PublishedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Unpublishes the material
        /// </summary>
        public void Unpublish()
        {
            IsPublished = false;
        }

        /// <summary>
        /// Increments the view count
        /// </summary>
        public void IncrementViewCount()
        {
            ViewCount++;
        }
    }
}
