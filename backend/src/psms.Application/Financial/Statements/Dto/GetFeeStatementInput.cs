using System;

namespace psms.Financial.Statements.Dto;

/// <summary>
/// Which statement to produce: whose, and for what period.
/// <para>
/// The period is one of a term, an academic year, or a custom date range —
/// never more than one. With none of them the statement covers the current
/// academic year.
/// </para>
/// </summary>
public class GetFeeStatementInput
{
    /// <summary>
    /// One learner's statement. A parent may leave this empty for a single
    /// statement across all their children; staff must name the learner.
    /// </summary>
    public Guid? StudentId { get; set; }

    public Guid? TermId { get; set; }

    public Guid? AcademicYearId { get; set; }

    /// <summary>Custom range start, inclusive. Needs <see cref="ToDate"/>.</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>Custom range end, inclusive. Needs <see cref="FromDate"/>.</summary>
    public DateTime? ToDate { get; set; }
}
