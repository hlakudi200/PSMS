using psms.Domain.Shared.Enums;
using System.Collections.Generic;
using System.Linq;

namespace psms.Domain.Assessment.Promotion
{
    /// <summary>
    /// What the promotion rules make of one learner's year.
    /// <para>
    /// Advice, not a decision. The policy puts the decision in a person's hands
    /// — NPPPPR §(2b) requires a staff meeting and then a meeting with the
    /// parent before a learner is retained — so this says whether the
    /// requirements are met and exactly which ones are not, and a person
    /// records the outcome.
    /// </para>
    /// </summary>
    public sealed class PromotionEvaluation
    {
        public PromotionEvaluation(
            SouthAfricanGradeLevel grade,
            bool meetsRequirements,
            PromotionDecision recommended,
            IReadOnlyList<PromotionRequirement> requirements)
        {
            Grade = grade;
            MeetsRequirements = meetsRequirements;
            Recommended = recommended;
            Requirements = requirements ?? new List<PromotionRequirement>();
        }

        public SouthAfricanGradeLevel Grade { get; }

        /// <summary>Whether every requirement for this grade is satisfied.</summary>
        public bool MeetsRequirements { get; }

        /// <summary>
        /// What the rules point to. <see cref="PromotionDecision.Promoted"/>
        /// when the requirements are met; <see cref="PromotionDecision.Retained"/>
        /// when they are not. Progression — moving a learner on despite not
        /// meeting the requirements, to keep them from spending more than four
        /// years in a phase — is a judgement the rules cannot make, so it is
        /// never recommended, only recorded.
        /// </summary>
        public PromotionDecision Recommended { get; }

        /// <summary>Every requirement for this grade, met or not, in policy order.</summary>
        public IReadOnlyList<PromotionRequirement> Requirements { get; }

        /// <summary>The requirements that are not satisfied.</summary>
        public IEnumerable<PromotionRequirement> Unmet => Requirements.Where(r => !r.IsMet);
    }

    /// <summary>One clause of a grade's promotion requirements, and whether it is met.</summary>
    public sealed class PromotionRequirement
    {
        public PromotionRequirement(string clause, string description, bool isMet, string detail = null)
        {
            Clause = clause;
            Description = description;
            IsMet = isMet;
            Detail = detail;
        }

        /// <summary>The policy reference, e.g. "NPPPPR §21(1)(a)".</summary>
        public string Clause { get; }

        /// <summary>What the clause requires, in the policy's own terms.</summary>
        public string Description { get; }

        public bool IsMet { get; }

        /// <summary>
        /// What was actually achieved against it — the subject and mark that
        /// satisfied it, or what is short.
        /// </summary>
        public string Detail { get; }
    }
}
