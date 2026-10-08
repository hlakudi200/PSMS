using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Domain.Assessment.Entities
{
    /// <summary>
    /// RC-26. What had to happen around a decision to retain a learner.
    /// <para>
    /// <b>NPPPPR §(2b)</b>: "(a) the school must convene a <b>special meeting of
    /// relevant subject staff</b> to evaluate each learner holistically that has
    /// not met the promotion requirements more than once in Grade 10 or 11;
    /// (b) if there is consensus … a meeting must be held with the
    /// parent/guardian so that the advice is carefully and clearly explained …
    /// <b>before the learner's school report is handed to them</b>; (c) the
    /// decision reached at the meeting contemplated above must be reflected on
    /// the learner's report card. <b>If the learner is retained, this must be
    /// confirmed in writing by the parent.</b>"
    /// </para>
    /// <para>
    /// <b>§(2c)</b>: "a parent/guardian may appeal … the parent/guardian must
    /// submit a written request, <b>not later than three (3) days</b> after the
    /// official opening of schools, to the school principal … within
    /// <b>fourteen (14) working days</b> of receiving a request to appeal, the
    /// head of department or his/her designee shall make a final
    /// determination."
    /// </para>
    /// <para>
    /// One per report, created only when a retention is recorded. It holds dates
    /// and references rather than the documents themselves: the minutes and the
    /// parent's written confirmation live in the school's own file, and this is
    /// the record that they exist and when.
    /// </para>
    /// </summary>
    public class RetentionProcedure : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxNoteLength = 1000;

        public int? TenantId { get; set; }

        /// <summary>The year-end report whose retention this is about.</summary>
        public Guid ReportId { get; set; }

        /// <summary>
        /// §(2b)(a). When the special meeting of subject staff was held.
        /// </summary>
        public DateTime? StaffMeetingDate { get; set; }

        /// <summary>Who was there, and what they concluded.</summary>
        [StringLength(MaxNoteLength)]
        public string StaffMeetingNote { get; set; }

        /// <summary>
        /// §(2b)(b). When the advice was explained to the parent or guardian.
        /// This has to happen before the report is handed over, which is why a
        /// retained learner's card cannot be published without it.
        /// </summary>
        public DateTime? ParentMeetingDate { get; set; }

        [StringLength(MaxNoteLength)]
        public string ParentMeetingNote { get; set; }

        /// <summary>
        /// §(2b)(c). When the parent confirmed the retention in writing.
        /// <para>
        /// Recorded rather than required before publishing: the Protocol orders
        /// the <i>meeting</i> before the report is handed over, not the
        /// confirmation, and a card withheld waiting for a signature would run
        /// into §25(13), which forbids withholding a report card for any reason.
        /// </para>
        /// </summary>
        public DateTime? ParentConfirmedInWritingDate { get; set; }

        /// <summary>Where the school's copy of that confirmation is held.</summary>
        [StringLength(MaxNoteLength)]
        public string ParentConfirmationReference { get; set; }

        /// <summary>§(2c). When the parent lodged a written appeal.</summary>
        public DateTime? AppealLodgedDate { get; set; }

        /// <summary>
        /// The last day an appeal could be lodged — three days after the
        /// official opening of schools, recorded when the procedure is opened so
        /// it does not move if the term dates are later edited.
        /// </summary>
        public DateTime? AppealDeadline { get; set; }

        /// <summary>
        /// §(2c). The last working day the head of department has to determine
        /// it, counted from the day the appeal was lodged.
        /// </summary>
        public DateTime? AppealDeterminationDeadline { get; set; }

        /// <summary>When the determination was made.</summary>
        public DateTime? AppealDeterminedDate { get; set; }

        /// <summary>What was determined, in the determiner's own words.</summary>
        [StringLength(MaxNoteLength)]
        public string AppealOutcome { get; set; }

        public virtual Report Report { get; set; }

        protected RetentionProcedure() { }

        public RetentionProcedure(Guid id, int? tenantId, Guid reportId)
        {
            Id = id;
            TenantId = tenantId;
            ReportId = reportId;
        }

        /// <summary>
        /// §(2b)(b). Whether the parent has been seen, which is the one step the
        /// Protocol puts before the report reaches them.
        /// </summary>
        public bool ParentHasBeenMet() => ParentMeetingDate.HasValue;

        /// <summary>
        /// Whether an appeal was lodged after the deadline had passed. Recorded
        /// rather than refused — a late appeal is still a parent asking, and
        /// whether to hear it is not this system's call.
        /// </summary>
        public bool AppealWasLate() =>
            AppealLodgedDate.HasValue
            && AppealDeadline.HasValue
            && AppealLodgedDate.Value.Date > AppealDeadline.Value.Date;

        /// <summary>
        /// Whether the determination is overdue: an appeal lodged, its fourteen
        /// working days passed, and nothing determined.
        /// </summary>
        public bool DeterminationIsOverdue(DateTime asAt) =>
            AppealLodgedDate.HasValue
            && !AppealDeterminedDate.HasValue
            && AppealDeterminationDeadline.HasValue
            && asAt.Date > AppealDeterminationDeadline.Value.Date;
    }
}
