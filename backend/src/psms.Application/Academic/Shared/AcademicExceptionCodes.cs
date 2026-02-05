namespace psms.Academic.Shared;

/// <summary>
/// Exception codes for the Academic module.
/// Maps to business rules AR-001 through AR-007, ER-001 through ER-005.
/// </summary>
public static class AcademicExceptionCodes
{
    // Grade (AR-005)
    public const string GradeNotFound = "ACD_GRADE_NOT_FOUND";
    public const string DuplicateGradeLevel = "ACD005_DUPLICATE_GRADE_LEVEL";
    public const string InvalidGradeLevel = "ACD005_INVALID_GRADE_LEVEL";
    public const string InvalidSchoolPhaseForGrade = "ACD005_INVALID_SCHOOL_PHASE_FOR_GRADE";
    public const string CannotDeleteGradeWithStudents = "ACD_CANNOT_DELETE_GRADE_WITH_STUDENTS";
    public const string CannotDeleteGradeWithClasses = "ACD_CANNOT_DELETE_GRADE_WITH_CLASSES";
    public const string GradeNotActive = "ACD_GRADE_NOT_ACTIVE";

    // Academic Year (AR-001, AR-002)
    public const string AcademicYearNotFound = "ACD_ACADEMIC_YEAR_NOT_FOUND";
    public const string DuplicateAcademicYear = "ACD001_DUPLICATE_ACADEMIC_YEAR";
    public const string AcademicYearAlreadyCurrent = "ACD001_ACADEMIC_YEAR_ALREADY_CURRENT";
    public const string InvalidAcademicYearDates = "ACD002_INVALID_ACADEMIC_YEAR_DATES";
    public const string InvalidAcademicYearStartMonth = "ACD002_INVALID_START_MONTH";
    public const string InvalidAcademicYearEndMonth = "ACD002_INVALID_END_MONTH";
    public const string InvalidAcademicYearDuration = "ACD002_INVALID_DURATION";
    public const string CannotDeleteCurrentAcademicYear = "ACD_CANNOT_DELETE_CURRENT_ACADEMIC_YEAR";
    public const string CannotDeleteAcademicYearWithClasses = "ACD_CANNOT_DELETE_YEAR_WITH_CLASSES";

    // Term (AR-003, AR-004)
    public const string TermNotFound = "ACD_TERM_NOT_FOUND";
    public const string InvalidTermCount = "ACD003_INVALID_TERM_COUNT";
    public const string OverlappingTerms = "ACD003_OVERLAPPING_TERMS";
    public const string TermAlreadyCurrent = "ACD004_TERM_ALREADY_CURRENT";
    public const string AcademicYearNotCurrent = "ACD004_ACADEMIC_YEAR_NOT_CURRENT";
    public const string TermOutsideAcademicYear = "ACD_TERM_OUTSIDE_ACADEMIC_YEAR";
    public const string DuplicateTermNumber = "ACD_DUPLICATE_TERM_NUMBER";
    public const string CannotDeleteCurrentTerm = "ACD_CANNOT_DELETE_CURRENT_TERM";

    // Subject
    public const string SubjectNotFound = "ACD_SUBJECT_NOT_FOUND";
    public const string DuplicateSubjectCode = "ACD_DUPLICATE_SUBJECT_CODE";
    public const string SubjectNotActive = "ACD_SUBJECT_NOT_ACTIVE";
    public const string CannotDeleteSubjectWithGrades = "ACD_CANNOT_DELETE_SUBJECT_WITH_GRADES";
    public const string CannotDeleteSubjectWithTeachers = "ACD_CANNOT_DELETE_SUBJECT_WITH_TEACHERS";
    public const string CannotDeleteSubjectWithStudents = "ACD_CANNOT_DELETE_SUBJECT_WITH_STUDENTS";

    // GradeSubject
    public const string GradeSubjectNotFound = "ACD_GRADE_SUBJECT_NOT_FOUND";
    public const string DuplicateGradeSubject = "ACD_DUPLICATE_GRADE_SUBJECT";

    // Teacher
    public const string TeacherNotFound = "ACD_TEACHER_NOT_FOUND";
    public const string DuplicateEmployeeNumber = "ACD_DUPLICATE_EMPLOYEE_NUMBER";
    public const string DuplicateTeacherEmail = "ACD_DUPLICATE_TEACHER_EMAIL";
    public const string CannotDeleteTeacherWithSubjects = "ACD_CANNOT_DELETE_TEACHER_WITH_SUBJECTS";
    public const string CannotDeleteTeacherWithClasses = "ACD_CANNOT_DELETE_TEACHER_WITH_CLASSES";

    // Parent
    public const string ParentNotFound = "ACD_PARENT_NOT_FOUND";
    public const string DuplicateParentEmail = "ACD_DUPLICATE_PARENT_EMAIL";
    public const string InvalidSAIdNumber = "ACD_INVALID_SA_ID_NUMBER";
    public const string CannotDeleteParentWithStudents = "ACD_CANNOT_DELETE_PARENT_WITH_STUDENTS";

    // Class
    public const string ClassNotFound = "ACD_CLASS_NOT_FOUND";
    public const string DuplicateClassName = "ACD_DUPLICATE_CLASS_NAME";
    public const string ClassCapacityTooLow = "ACD_CLASS_CAPACITY_TOO_LOW";
    public const string ClassTeacherNotActive = "ACD_CLASS_TEACHER_NOT_ACTIVE";
    public const string CannotDeleteClassWithStudents = "ACD_CANNOT_DELETE_CLASS_WITH_STUDENTS";
    public const string CannotDeleteClassWithTeachers = "ACD_CANNOT_DELETE_CLASS_WITH_TEACHERS";
    public const string ClassNotActive = "ACD_CLASS_NOT_ACTIVE";
    public const string ClassGradeMismatch = "ACD_CLASS_GRADE_MISMATCH";
    public const string ClassAtCapacity = "ACD006_CLASS_AT_CAPACITY";

    // Student
    public const string StudentNotFound = "ACD_STUDENT_NOT_FOUND";
    public const string DuplicateAdmissionNumber = "ACD_DUPLICATE_ADMISSION_NUMBER";
    public const string InvalidDateOfBirth = "ACD_INVALID_DATE_OF_BIRTH";
    public const string InvalidAdmissionDate = "ACD_INVALID_ADMISSION_DATE";
    public const string CannotDeleteStudentWithParents = "ACD_CANNOT_DELETE_STUDENT_WITH_PARENTS";
    public const string CannotDeleteStudentWithSubjects = "ACD_CANNOT_DELETE_STUDENT_WITH_SUBJECTS";
}
