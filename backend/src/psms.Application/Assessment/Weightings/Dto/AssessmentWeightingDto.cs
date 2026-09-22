using psms.Domain.Shared.Enums;

namespace psms.Assessment.Weightings.Dto;

/// <summary>
/// One grade band's SBA / examination split, alongside the national default it
/// is measured against so the UI can show where a school has departed from
/// policy without having to hold the policy table itself.
/// </summary>
public class AssessmentWeightingDto
{
    public AssessmentWeightingBand Band { get; set; }

    /// <summary>Human label for the band, e.g. "Senior Phase (Grade 7-9)".</summary>
    public string BandName { get; set; }

    public int SbaPercentage { get; set; }

    public int ExamPercentage { get; set; }

    /// <summary>The DBE Circular S8 of 2023 value for this band.</summary>
    public int PolicySbaPercentage { get; set; }

    /// <summary>The DBE Circular S8 of 2023 value for this band.</summary>
    public int PolicyExamPercentage { get; set; }

    /// <summary>False when the school has set its own split for this band.</summary>
    public bool MatchesPolicyDefault { get; set; }
}
