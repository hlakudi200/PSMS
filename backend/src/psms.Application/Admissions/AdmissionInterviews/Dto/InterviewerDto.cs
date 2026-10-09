using System.Collections.Generic;

namespace psms.Admissions.AdmissionInterviews.Dto;

/// <summary>
/// Somebody who may be put down to conduct an admission interview.
/// <para>
/// Scheduling used to take whatever <c>InterviewerUserId</c> and
/// <c>InterviewerName</c> the caller sent, unchecked and unrelated to each
/// other. An interview could be booked against a person who did not exist, or
/// against a name that belonged to somebody else, or against a colleague with
/// no permission to open it — and that only showed up at the end, when they
/// came to record the outcome and could not.
/// </para>
/// </summary>
public class InterviewerDto
{
    public long UserId { get; set; }

    /// <summary>The name the school knows them by; also what is stored on the interview.</summary>
    public string Name { get; set; }

    public string EmailAddress { get; set; }

    /// <summary>Which roles make them an interviewer — a teacher, the principal, admissions.</summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>How many interviews they are already down for and have not yet completed.</summary>
    public int UpcomingInterviews { get; set; }
}
