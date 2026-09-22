using System;
using System.Collections.Generic;
using System.Linq;

namespace psms.Domain.Assessment
{
    /// <summary>
    /// One assessment's contribution to a subject mark.
    /// </summary>
    public readonly struct AssessmentContribution
    {
        public AssessmentContribution(decimal percentage, decimal weight, bool isExamination)
        {
            Percentage = percentage;
            Weight = weight;
            IsExamination = isExamination;
        }

        /// <summary>What the learner scored, 0-100.</summary>
        public decimal Percentage { get; }

        /// <summary>
        /// How much this task counts within its component. Zero across a whole
        /// component falls back to an unweighted mean rather than producing
        /// nothing.
        /// </summary>
        public decimal Weight { get; }

        /// <summary>
        /// Whether this is the end-of-year examination rather than a
        /// School-Based Assessment task. Everything that is not an examination
        /// — tests, assignments, projects, orals, practicals — is SBA.
        /// </summary>
        public bool IsExamination { get; }
    }

    /// <summary>What a subject's marks came to, and how.</summary>
    public readonly struct SubjectMarkResult
    {
        public SubjectMarkResult(
            decimal? schoolBasedMark,
            decimal? examinationMark,
            decimal? finalMark,
            decimal appliedSbaWeight,
            decimal appliedExamWeight,
            bool awaitsExternalExamination)
        {
            SchoolBasedMark = schoolBasedMark;
            ExaminationMark = examinationMark;
            FinalMark = finalMark;
            AppliedSbaWeight = appliedSbaWeight;
            AppliedExamWeight = appliedExamWeight;
            AwaitsExternalExamination = awaitsExternalExamination;
        }

        /// <summary>The School-Based Assessment component, or null if there is none.</summary>
        public decimal? SchoolBasedMark { get; }

        /// <summary>
        /// The examination component, or null — including when the examination
        /// is external and therefore not the school's to report.
        /// </summary>
        public decimal? ExaminationMark { get; }

        public decimal? FinalMark { get; }

        /// <summary>
        /// The split actually used, which is not always the configured one: a
        /// missing component is dropped and the other carries 100%.
        /// </summary>
        public decimal AppliedSbaWeight { get; }

        public decimal AppliedExamWeight { get; }

        /// <summary>
        /// The final mark is the school-based component only, because the
        /// examination is set and marked outside the school and has not
        /// happened here. Grade 12's National Senior Certificate paper.
        /// </summary>
        public bool AwaitsExternalExamination { get; }

        public static SubjectMarkResult None =>
            new SubjectMarkResult(null, null, null, 100m, 0m, false);
    }

    /// <summary>
    /// RC-05 / RC-15. Turns a learner's assessment marks in one subject into the
    /// mark that goes on the report card.
    /// <para>
    /// <b>What counts.</b> Everything that is not an examination is School-Based
    /// Assessment (National Protocol §5(1)). The two components are averaged
    /// separately, each weighted by its tasks' weights, and then combined with
    /// the school's SBA:examination split for the grade band (DBE Circular S8 of
    /// 2023, configurable per tenant).
    /// </para>
    /// <para>
    /// <b>A missing component is dropped, not scored zero.</b> National Protocol
    /// §12(2) — a learner who cannot sit the examination is awarded "a mark based
    /// on the School-Based Assessment … obtained by the learner" — and §8(5)(b),
    /// where outstanding SBA means "the final mark for the relevant subject will
    /// be adjusted for promotion purposes in terms of the completed tasks". So a
    /// subject with SBA and no examination is marked out of the SBA alone. The
    /// one case that genuinely is a zero, §8(4)'s missing Grades 10-12 SBA with
    /// no valid reason, is a decision a person makes by entering that zero.
    /// </para>
    /// <para>
    /// <b>Grade 12 is not an ordinary band.</b> NPPPPR §31(1): the examination is
    /// the external National Senior Certificate paper, "initiated, directed and
    /// coordinated by provincial education departments and the Department of
    /// Basic Education". The school holds only the 25% SBA. Combining a school's
    /// own trial paper at 75% would produce a number that looks like an NSC
    /// result and is not one, so for an external band the internal examination
    /// marks are left out of the card entirely and the result is flagged
    /// <see cref="SubjectMarkResult.AwaitsExternalExamination"/>. A predicted
    /// mark is a separate, separately-labelled thing, and is not this.
    /// </para>
    /// </summary>
    public static class SubjectMarkAggregator
    {
        public static SubjectMarkResult Aggregate(
            IEnumerable<AssessmentContribution> contributions,
            int sbaPercentage,
            int examPercentage,
            bool examinationIsExternal)
        {
            var all = (contributions ?? Enumerable.Empty<AssessmentContribution>()).ToList();

            var schoolBased = Average(all.Where(c => !c.IsExamination));

            // An external examination did not happen at this school, so whatever
            // internal paper was written is not the examination component and is
            // not reported as one.
            var examination = examinationIsExternal
                ? null
                : Average(all.Where(c => c.IsExamination));

            if (!schoolBased.HasValue && !examination.HasValue)
                return SubjectMarkResult.None;

            if (examinationIsExternal || !examination.HasValue)
            {
                // The school-based component carries the whole card.
                return new SubjectMarkResult(
                    schoolBased,
                    null,
                    schoolBased,
                    100m,
                    0m,
                    examinationIsExternal && schoolBased.HasValue);
            }

            if (!schoolBased.HasValue)
            {
                // No SBA at all — unusual, but the examination is a real mark and
                // dropping it would leave a blank subject on the card.
                return new SubjectMarkResult(null, examination, examination, 0m, 100m, false);
            }

            var sba = Clamp(sbaPercentage);
            var exam = Clamp(examPercentage);

            if (sba + exam != 100)
            {
                // A split that does not total 100 would silently under- or
                // over-state the mark. Fall back to the unweighted mean of the
                // two components rather than inventing a number from it.
                sba = 50;
                exam = 50;
            }

            var final = (schoolBased.Value * sba / 100m) + (examination.Value * exam / 100m);

            return new SubjectMarkResult(schoolBased, examination, final, sba, exam, false);
        }

        /// <summary>
        /// The weighted mean of a component's tasks, or null when it has none.
        /// Falls back to an unweighted mean when no task carries a weight, which
        /// is how a school that never configured weights still gets a mark.
        /// </summary>
        private static decimal? Average(IEnumerable<AssessmentContribution> contributions)
        {
            var items = contributions.ToList();

            if (items.Count == 0)
                return null;

            var totalWeight = items.Sum(c => c.Weight);

            return totalWeight > 0
                ? items.Sum(c => c.Percentage * c.Weight) / totalWeight
                : items.Average(c => c.Percentage);
        }

        private static int Clamp(int percentage) =>
            percentage < 0 ? 0 : percentage > 100 ? 100 : percentage;
    }
}
