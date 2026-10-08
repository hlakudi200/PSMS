using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// Why an issued report card is being taken back.
/// <para>
/// Required, and deliberately so. Withdrawing a card is undoing something a
/// family has already been told about and may already have read; the school
/// should have to say what was wrong with it, and the answer should be on the
/// record rather than in somebody's memory.
/// </para>
/// </summary>
public class WithdrawReportDto
{
    [Required]
    [StringLength(psms.Domain.Assessment.Entities.Report.MaxWithdrawalReasonLength, MinimumLength = 3)]
    public string Reason { get; set; }
}
