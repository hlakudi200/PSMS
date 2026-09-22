namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// The kind of assessment an applicant sits during admissions.
    /// <para>
    /// Was called AssessmentType, which read as though it covered academic
    /// assessments too — and the academic Assessment entity duly used it, so a
    /// teacher creating an "Exam" stored "Readiness" (RC-13). Academic
    /// assessments now use <see cref="AcademicAssessmentType"/>.
    /// </para>
    /// </summary>
    public enum AdmissionAssessmentType
    {
        Placement = 1,
        Diagnostic = 2,
        Readiness = 3,
        LanguageProficiency = 4,
        Mathematics = 5,
        General = 6
    }
}
