using psms.Domain.Shared.Enums;
using System.Collections.Generic;

namespace psms.Domain.Assessment
{
    /// <summary>
    /// The national School-Based Assessment to examination split, per grade band.
    /// <para>
    /// Source: <b>DBE Circular S8 of 2023</b>, "Release of revised subject
    /// weightings relating to the Programme of Assessment", 23 March 2023, which
    /// amends the weightings issued in Circular S33 of 2022:
    /// </para>
    /// <list type="table">
    ///   <item><description>Foundation (Grade R-3) — 100% SBA</description></item>
    ///   <item><description>Intermediate (Grade 4-6) — 80% SBA : 20% examination</description></item>
    ///   <item><description>Senior Phase (Grade 7-9) — 60% SBA : 40% examination</description></item>
    ///   <item><description>Grade 10 &amp; 11 — 40% SBA : 60% examination</description></item>
    ///   <item><description>Grade 12 — 25% SBA : 75% examination</description></item>
    /// </list>
    /// <para>
    /// These are the defaults a school starts with. A school may set its own
    /// (see AssessmentWeighting), which is why nothing in the codebase should
    /// read these constants directly when computing a mark — read the tenant's
    /// configured row instead.
    /// </para>
    /// <para>
    /// Note the earlier NPPPPR (Feb 2021) figures — Intermediate 75:25, Senior
    /// 40:60, Grade 10-11 25:75 — are superseded by S8 and must not be used.
    /// </para>
    /// </summary>
    public static class AssessmentWeightingDefaults
    {
        /// <summary>SBA percentage per band. The examination is the remainder.</summary>
        public static readonly IReadOnlyDictionary<AssessmentWeightingBand, int> SbaPercentage =
            new Dictionary<AssessmentWeightingBand, int>
            {
                [AssessmentWeightingBand.Foundation] = 100,
                [AssessmentWeightingBand.Intermediate] = 80,
                [AssessmentWeightingBand.Senior] = 60,
                [AssessmentWeightingBand.Grade10And11] = 40,
                [AssessmentWeightingBand.Grade12] = 25
            };

        /// <summary>The policy SBA percentage for a band.</summary>
        public static int SbaFor(AssessmentWeightingBand band) =>
            SbaPercentage.TryGetValue(band, out var value) ? value : 100;

        /// <summary>The policy examination percentage for a band.</summary>
        public static int ExamFor(AssessmentWeightingBand band) => 100 - SbaFor(band);

        /// <summary>
        /// The band a grade falls in. Grade R through 3 have no examination
        /// component; Grade 12's examination is the external NSC paper.
        /// </summary>
        public static AssessmentWeightingBand BandFor(SouthAfricanGradeLevel grade)
        {
            switch (grade)
            {
                case SouthAfricanGradeLevel.GradeR:
                case SouthAfricanGradeLevel.Grade1:
                case SouthAfricanGradeLevel.Grade2:
                case SouthAfricanGradeLevel.Grade3:
                    return AssessmentWeightingBand.Foundation;

                case SouthAfricanGradeLevel.Grade4:
                case SouthAfricanGradeLevel.Grade5:
                case SouthAfricanGradeLevel.Grade6:
                    return AssessmentWeightingBand.Intermediate;

                case SouthAfricanGradeLevel.Grade7:
                case SouthAfricanGradeLevel.Grade8:
                case SouthAfricanGradeLevel.Grade9:
                    return AssessmentWeightingBand.Senior;

                case SouthAfricanGradeLevel.Grade10:
                case SouthAfricanGradeLevel.Grade11:
                    return AssessmentWeightingBand.Grade10And11;

                default:
                    return AssessmentWeightingBand.Grade12;
            }
        }
    }
}
