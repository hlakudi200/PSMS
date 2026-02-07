namespace psms.SASpecific.Shared;

/// <summary>
/// Exception codes for the SASpecific module.
/// </summary>
public static class SASpecificExceptionCodes
{
    // AfterCare
    public const string AfterCareNotFound = "SA_AFTER_CARE_NOT_FOUND";
    public const string DuplicateAfterCare = "SA_DUPLICATE_AFTER_CARE";
    public const string AcademicYearNotFound = "SA_ACADEMIC_YEAR_NOT_FOUND";
    public const string AfterCareAtCapacity = "SA_AFTER_CARE_AT_CAPACITY";
    public const string CannotDeleteAfterCareWithEnrollments = "SA_CANNOT_DELETE_AFTER_CARE_WITH_ENROLLMENTS";
    public const string AfterCareInactive = "SA_AFTER_CARE_INACTIVE";
    public const string InvalidAfterCareTimes = "SA_INVALID_AFTER_CARE_TIMES";

    // StudentAfterCare
    public const string StudentAfterCareNotFound = "SA_STUDENT_AFTER_CARE_NOT_FOUND";
    public const string DuplicateStudentAfterCare = "SA_DUPLICATE_STUDENT_AFTER_CARE";
    public const string StudentNotFound = "SA_STUDENT_NOT_FOUND";
    public const string InvalidEnrollmentStatusTransition = "SA_INVALID_ENROLLMENT_STATUS_TRANSITION";
    public const string EnrollmentAlreadyTerminated = "SA_ENROLLMENT_ALREADY_TERMINATED";

    // ExtramuralActivity
    public const string ExtramuralActivityNotFound = "SA_EXTRAMURAL_ACTIVITY_NOT_FOUND";
    public const string DuplicateExtramuralActivity = "SA_DUPLICATE_EXTRAMURAL_ACTIVITY";
    public const string ExtramuralAtCapacity = "SA_EXTRAMURAL_AT_CAPACITY";
    public const string CannotDeleteExtramuralWithEnrollments = "SA_CANNOT_DELETE_EXTRAMURAL_WITH_ENROLLMENTS";
    public const string ExtramuralInactive = "SA_EXTRAMURAL_INACTIVE";
    public const string RegistrationClosed = "SA_REGISTRATION_CLOSED";
    public const string GradeNotEligible = "SA_GRADE_NOT_ELIGIBLE";
    public const string InvalidGradeRange = "SA_INVALID_GRADE_RANGE";

    // StudentExtramural
    public const string StudentExtramuralNotFound = "SA_STUDENT_EXTRAMURAL_NOT_FOUND";
    public const string DuplicateStudentExtramural = "SA_DUPLICATE_STUDENT_EXTRAMURAL";

    // SchoolTransport
    public const string SchoolTransportNotFound = "SA_SCHOOL_TRANSPORT_NOT_FOUND";
    public const string DuplicateSchoolTransport = "SA_DUPLICATE_SCHOOL_TRANSPORT";
    public const string TransportAtCapacity = "SA_TRANSPORT_AT_CAPACITY";
    public const string CannotDeleteTransportWithEnrollments = "SA_CANNOT_DELETE_TRANSPORT_WITH_ENROLLMENTS";
    public const string TransportInactive = "SA_TRANSPORT_INACTIVE";

    // StudentTransport
    public const string StudentTransportNotFound = "SA_STUDENT_TRANSPORT_NOT_FOUND";
    public const string DuplicateStudentTransport = "SA_DUPLICATE_STUDENT_TRANSPORT";
}
