using System;
using System.Collections.Generic;
using psms.Domain.Shared.Enums;

namespace psms.Financial.Statements.Dto;

/// <summary>
/// A fee statement: what was owed at the start of the period, what was charged
/// and paid during it, and what is owed at the end — per learner and, for a
/// family, consolidated.
/// </summary>
public class FeeStatementDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    /// <summary>"Term 1 · 2026", "2026", or the date range.</summary>
    public string PeriodLabel { get; set; }

    /// <summary>The day the balances are stated at: the period end, or today if that is earlier.</summary>
    public DateTime StatementDate { get; set; }

    public string Currency { get; set; }

    /// <summary>Owed before the period started.</summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>Fees charged during the period.</summary>
    public decimal TotalCharged { get; set; }

    /// <summary>Discounts and waivers during the period.</summary>
    public decimal TotalCredits { get; set; }

    /// <summary>Payments received during the period.</summary>
    public decimal TotalPaid { get; set; }

    /// <summary>Payments refunded during the period.</summary>
    public decimal TotalRefunded { get; set; }

    /// <summary>Owed at the end of the period. Negative means in credit.</summary>
    public decimal ClosingBalance { get; set; }

    /// <summary>The part of the closing balance that is past its due date.</summary>
    public decimal AmountOverdue { get; set; }

    public List<FeeStatementChildDto> Children { get; set; } = new();

    /// <summary>Every transaction in the period, oldest first, with a running balance.</summary>
    public List<FeeStatementLineDto> Lines { get; set; } = new();
}

/// <summary>One learner's share of the statement.</summary>
public class FeeStatementChildDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public string AdmissionNumber { get; set; }
    public string GradeName { get; set; }

    public decimal OpeningBalance { get; set; }
    public decimal Charged { get; set; }
    public decimal Credits { get; set; }
    public decimal Paid { get; set; }
    public decimal Refunded { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal AmountOverdue { get; set; }

    /// <summary>
    /// Fees with something still to pay, as of today regardless of the period —
    /// what a parent would be paying against.
    /// </summary>
    public List<OutstandingFeeDto> OutstandingFees { get; set; } = new();
}

/// <summary>One transaction on the statement.</summary>
public class FeeStatementLineDto
{
    public DateTime Date { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public FeeStatementLineType Type { get; set; }
    public string Description { get; set; }

    /// <summary>The receipt number for a payment or refund.</summary>
    public string Reference { get; set; }

    /// <summary>Increases what is owed (a fee, a refund).</summary>
    public decimal Debit { get; set; }

    /// <summary>Reduces what is owed (a payment, a discount, a waiver).</summary>
    public decimal Credit { get; set; }

    /// <summary>The running balance after this line.</summary>
    public decimal Balance { get; set; }

    /// <summary>For a payment: which fees it was put towards.</summary>
    public List<FeeStatementAllocationDto> Allocations { get; set; } = new();
}

public class FeeStatementAllocationDto
{
    public Guid StudentFeeId { get; set; }
    public string FeeName { get; set; }
    public decimal Amount { get; set; }
}

public class OutstandingFeeDto
{
    public Guid StudentFeeId { get; set; }
    public string FeeName { get; set; }
    public DateTime DueDate { get; set; }
    public decimal AmountDue { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Outstanding { get; set; }
    public FeeStatus Status { get; set; }
    public bool IsOverdue { get; set; }
}

public enum FeeStatementLineType
{
    Charge = 1,
    Discount = 2,
    Waiver = 3,
    Payment = 4,
    Refund = 5
}
