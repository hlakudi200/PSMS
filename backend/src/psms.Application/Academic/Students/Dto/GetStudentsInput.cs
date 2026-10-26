using psms.Academic.Shared;

namespace psms.Academic.Students.Dto;

public class GetStudentsInput : GetAcademicEntityInput
{
    /// <summary>
    /// #437. True lists only learners with no parent or guardian linked: nobody
    /// the school can contact, and nobody a report card can be shown to. The
    /// count of these is the list's TotalCount.
    /// </summary>
    public bool? WithoutGuardian { get; set; }
}
