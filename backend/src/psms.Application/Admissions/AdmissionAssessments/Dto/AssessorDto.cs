using System.Collections.Generic;

namespace psms.Admissions.AdmissionAssessments.Dto;

/// <summary>
/// Somebody who may be put down to set and mark an admission assessment.
/// <para>
/// Scheduling used to take whatever <c>AssessorUserId</c> the caller sent, with
/// nothing checking that it belonged to a person who could open the result. The
/// mistake surfaced on the day the paper needed marking.
/// </para>
/// </summary>
public class AssessorDto
{
    public long UserId { get; set; }

    public string Name { get; set; }

    public string EmailAddress { get; set; }

    /// <summary>Which roles make them an assessor — usually a teacher.</summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>How many papers they are already down to mark and have not.</summary>
    public int OutstandingAssessments { get; set; }
}
