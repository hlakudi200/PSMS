using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace psms.Domain.Assessment.Entities
{
    /// <summary>
    /// A school's School-Based Assessment to examination split for one grade
    /// band — what a year-end subject mark is made of.
    /// <para>
    /// Seeded from <see cref="AssessmentWeightingDefaults"/> (DBE Circular S8 of
    /// 2023) and editable per school, because an independent school may run an
    /// approved variation. One row per (tenant, band).
    /// </para>
    /// <para>
    /// <see cref="IMustHaveTenant"/> like SchoolBranding: a weighting is never
    /// host-owned, and the filter behaviour suits a per-tenant settings row.
    /// </para>
    /// </summary>
    [Table("AssessmentWeightings")]
    public class AssessmentWeighting : FullAuditedEntity<Guid>, IMustHaveTenant, ISoftDelete
    {
        /// <summary>The school this weighting belongs to.</summary>
        public int TenantId { get; set; }

        /// <summary>The grade band it applies to.</summary>
        [Required]
        public AssessmentWeightingBand Band { get; set; }

        /// <summary>
        /// Percentage of the final mark that comes from the year's School-Based
        /// Assessment. Together with <see cref="ExamPercentage"/> this totals 100.
        /// </summary>
        [Range(0, 100)]
        public int SbaPercentage { get; set; }

        /// <summary>
        /// Percentage of the final mark that comes from the end-of-year
        /// examination. Zero in the Foundation Phase, which sits no examination.
        /// </summary>
        [Range(0, 100)]
        public int ExamPercentage { get; set; }

        public bool IsDeleted { get; set; }

        protected AssessmentWeighting()
        {
        }

        /// <summary>Creates a band's weighting at the national default.</summary>
        public AssessmentWeighting(Guid id, int tenantId, AssessmentWeightingBand band)
        {
            Id = id;
            TenantId = tenantId;
            Band = band;
            SbaPercentage = AssessmentWeightingDefaults.SbaFor(band);
            ExamPercentage = AssessmentWeightingDefaults.ExamFor(band);
        }

        /// <summary>
        /// Whether a split is usable. The two halves must account for the whole
        /// mark — anything else silently under- or over-states every learner's
        /// result, so it is rejected rather than normalised.
        /// </summary>
        public static bool IsValidSplit(int sba, int exam) =>
            sba >= 0 && sba <= 100 && exam >= 0 && exam <= 100 && sba + exam == 100;

        /// <summary>True when this band still sits at the S8 policy default.</summary>
        public bool MatchesPolicyDefault() =>
            SbaPercentage == AssessmentWeightingDefaults.SbaFor(Band)
            && ExamPercentage == AssessmentWeightingDefaults.ExamFor(Band);

        /// <summary>
        /// Sets the split. Callers validate with <see cref="IsValidSplit"/> first
        /// so they can raise a friendly error; this guards the invariant.
        /// </summary>
        public void SetSplit(int sba, int exam)
        {
            if (!IsValidSplit(sba, exam))
                throw new InvalidOperationException("SBA and examination percentages must each be 0-100 and total 100.");

            SbaPercentage = sba;
            ExamPercentage = exam;
        }

        /// <summary>Returns the band to the national default.</summary>
        public void ResetToPolicyDefault()
        {
            SbaPercentage = AssessmentWeightingDefaults.SbaFor(Band);
            ExamPercentage = AssessmentWeightingDefaults.ExamFor(Band);
        }
    }
}
