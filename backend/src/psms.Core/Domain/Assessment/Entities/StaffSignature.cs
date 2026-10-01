using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Domain.Assessment.Entities
{
    /// <summary>
    /// A staff member's handwritten signature, drawn once and reused when they
    /// sign a report card.
    /// <para>
    /// Held as SVG rather than a bitmap: it is what a signature pad produces
    /// natively, it stays sharp at print resolution, and it is a few kilobytes
    /// of text instead of an image file — small enough to live here rather than
    /// in object storage, which keeps it off any URL a browser could reach.
    /// </para>
    /// <para>
    /// This is the picture, not the act. The signing itself is recorded on the
    /// report card — who signed and when — and the picture is copied onto that
    /// card at the moment of signing, so changing the one stored here never
    /// alters a card that is already signed.
    /// </para>
    /// </summary>
    public class StaffSignature : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        public int TenantId { get; set; }

        /// <summary>The staff member this belongs to. One each.</summary>
        public long UserId { get; set; }

        /// <summary>
        /// The drawing, as an SVG document. Capped well above what a signature
        /// pad emits (a few KB) and well below anything that could be used to
        /// smuggle a large payload through.
        /// </summary>
        [Required]
        [MaxLength(MaxSvgLength)]
        public string SvgContent { get; set; }

        public const int MaxSvgLength = 64 * 1024;

        protected StaffSignature() { }

        public StaffSignature(Guid id, int tenantId, long userId, string svgContent)
        {
            Id = id;
            TenantId = tenantId;
            UserId = userId;
            Replace(svgContent);
        }

        /// <summary>Redraws the signature.</summary>
        public void Replace(string svgContent)
        {
            if (string.IsNullOrWhiteSpace(svgContent))
                throw new ArgumentException("A signature cannot be blank.", nameof(svgContent));

            if (svgContent.Length > MaxSvgLength)
                throw new ArgumentException(
                    $"A signature must be under {MaxSvgLength / 1024}KB.", nameof(svgContent));

            SvgContent = svgContent.Trim();
            LastModificationTime = Clock.Now;
        }
    }
}
