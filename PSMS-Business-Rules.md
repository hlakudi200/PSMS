# PSMS Business Rules & Validation
## South African Private School Management System

**Version**: 1.0
**Last Updated**: 2026-01-27
**Framework**: ASP.NET Boilerplate (ABP)
**Context**: South African Private Schools

---

## Table of Contents

1. [Academic Management Rules](#academic-management-rules)
2. [Enrollment & Registration Rules](#enrollment--registration-rules)
3. [Grading & Assessment Rules](#grading--assessment-rules)
4. [Attendance Rules](#attendance-rules)
5. [Reporting Rules](#reporting-rules)
6. [Financial Management Rules](#financial-management-rules)
7. [Communication Rules](#communication-rules)
8. [Multi-Tenancy Rules](#multi-tenancy-rules)
9. [Security & Authorization Rules](#security--authorization-rules)
10. [POPIA Compliance Rules](#popia-compliance-rules)
11. [SA-Specific Rules](#sa-specific-rules)
12. [Data Integrity Rules](#data-integrity-rules)
13. [Business Process Rules](#business-process-rules)

---

## Academic Management Rules

### AR-001: Academic Year Rules

**Rule**: Only ONE academic year can be current per tenant at any time.

**Implementation**:
```csharp
public class AcademicYear : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void SetAsCurrent()
    {
        // Business Rule: Only one current academic year per tenant
        var currentYear = _academicYearRepository.FirstOrDefault(
            y => y.TenantId == this.TenantId && y.IsCurrent && y.Id != this.Id
        );

        if (currentYear != null)
        {
            throw new BusinessException(
                "ACADEMIC_YEAR_ALREADY_CURRENT",
                $"Academic year '{currentYear.Name}' is already set as current."
            );
        }

        this.IsCurrent = true;
    }
}
```

**Validation**:
- ✅ Before setting `IsCurrent = true`, check no other academic year is current
- ✅ Enforce via domain method, not direct property access

---

### AR-002: SA Academic Year Dates

**Rule**: Academic year MUST start in January and end in December (SA standard).

**Implementation**:
```csharp
public void ValidateSouthAfricanAcademicYear()
{
    // Business Rule: SA academic year starts in January
    if (StartDate.Month < 1 || StartDate.Month > 2)
    {
        throw new BusinessException(
            "INVALID_SA_ACADEMIC_YEAR_START",
            "South African academic year must start in January or early February."
        );
    }

    // Business Rule: SA academic year ends in December
    if (EndDate.Month != 12)
    {
        throw new BusinessException(
            "INVALID_SA_ACADEMIC_YEAR_END",
            "South African academic year must end in December."
        );
    }

    // Business Rule: Academic year must span approximately 12 months
    var duration = (EndDate - StartDate).Days;
    if (duration < 300 || duration > 400)
    {
        throw new BusinessException(
            "INVALID_ACADEMIC_YEAR_DURATION",
            "Academic year must be approximately 12 months long."
        );
    }
}
```

**Validation**:
- ✅ StartDate: January or early February
- ✅ EndDate: December
- ✅ Duration: ~12 months (300-400 days)

---

### AR-003: SA Four-Term Structure

**Rule**: Academic year MUST have exactly 4 terms (SA standard).

**Implementation**:
```csharp
public void ValidateTermStructure()
{
    // Business Rule: SA schools must have 4 terms
    if (Terms.Count != 4)
    {
        throw new BusinessException(
            "INVALID_TERM_COUNT",
            "South African academic year must have exactly 4 terms."
        );
    }

    // Business Rule: Terms must be sequential and non-overlapping
    var orderedTerms = Terms.OrderBy(t => t.TermNumber).ToList();
    for (int i = 0; i < orderedTerms.Count - 1; i++)
    {
        if (orderedTerms[i].EndDate >= orderedTerms[i + 1].StartDate)
        {
            throw new BusinessException(
                "OVERLAPPING_TERMS",
                $"Term {i + 1} and Term {i + 2} dates overlap."
            );
        }
    }
}
```

**Validation**:
- ✅ Exactly 4 terms per academic year
- ✅ Term numbers: 1, 2, 3, 4
- ✅ No overlapping terms
- ✅ Sequential order maintained

---

### AR-004: Current Term

**Rule**: Only ONE term can be current per academic year.

**Implementation**:
```csharp
public class Term : Entity<Guid>
{
    public void SetAsCurrent()
    {
        // Business Rule: Only one current term per academic year
        var currentTerm = _termRepository.FirstOrDefault(
            t => t.AcademicYearId == this.AcademicYearId &&
                 t.IsCurrent &&
                 t.Id != this.Id
        );

        if (currentTerm != null)
        {
            throw new BusinessException(
                "TERM_ALREADY_CURRENT",
                $"Term '{currentTerm.Name}' is already set as current."
            );
        }

        // Business Rule: Can only set term as current if academic year is current
        if (!AcademicYear.IsCurrent)
        {
            throw new BusinessException(
                "ACADEMIC_YEAR_NOT_CURRENT",
                "Cannot set term as current when academic year is not current."
            );
        }

        this.IsCurrent = true;
    }
}
```

**Validation**:
- ✅ Only one current term per academic year
- ✅ Can only be current if academic year is current
- ✅ Automatically set based on current date (optional)

---

### AR-005: Grade Level Progression

**Rule**: Grades must follow SA education structure (R, 1-12).

**Implementation**:
```csharp
public class Grade : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void ValidateSAGradeLevel()
    {
        // Business Rule: Grade level must be valid SA level (R-12)
        if (GradeLevel < SouthAfricanGradeLevel.GradeR ||
            GradeLevel > SouthAfricanGradeLevel.Grade12)
        {
            throw new BusinessException(
                "INVALID_GRADE_LEVEL",
                "Grade level must be between Grade R and Grade 12."
            );
        }

        // Business Rule: School phase must match grade level
        var expectedPhase = CalculateExpectedPhase(GradeLevel);
        if (SchoolPhase != expectedPhase)
        {
            throw new BusinessException(
                "INVALID_SCHOOL_PHASE",
                $"Grade {GradeLevel} should be in {expectedPhase} phase."
            );
        }
    }

    private SouthAfricanSchoolPhase CalculateExpectedPhase(SouthAfricanGradeLevel level)
    {
        return level switch
        {
            <= SouthAfricanGradeLevel.Grade3 => SouthAfricanSchoolPhase.Foundation,
            <= SouthAfricanGradeLevel.Grade6 => SouthAfricanSchoolPhase.Intermediate,
            <= SouthAfricanGradeLevel.Grade9 => SouthAfricanSchoolPhase.Senior,
            _ => SouthAfricanSchoolPhase.FET
        };
    }
}
```

**Validation**:
- ✅ Grade level: R, 1, 2, ..., 12
- ✅ School phase matches grade level
- ✅ Foundation (R-3), Intermediate (4-6), Senior (7-9), FET (10-12)

---

### AR-006: Class Capacity

**Rule**: Class cannot exceed maximum capacity.

**Implementation**:
```csharp
public class Class : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void ValidateCapacity()
    {
        // Business Rule: Current enrollment cannot exceed capacity
        var currentEnrollment = Students.Count(s => s.IsActive);
        if (currentEnrollment > Capacity)
        {
            throw new BusinessException(
                "CLASS_OVER_CAPACITY",
                $"Class has {currentEnrollment} students but capacity is {Capacity}."
            );
        }
    }

    public void AddStudent(Student student)
    {
        // Business Rule: Cannot add student if class is full
        var currentEnrollment = Students.Count(s => s.IsActive);
        if (currentEnrollment >= Capacity)
        {
            throw new BusinessException(
                "CLASS_FULL",
                $"Cannot add student. Class is at full capacity ({Capacity})."
            );
        }

        Students.Add(student);
    }
}
```

**Validation**:
- ✅ Current enrollment ≤ Capacity
- ✅ Cannot add students when class is full
- ✅ Only count active students

---

### AR-007: Subject CAPS Compliance

**Rule**: Subjects must align with SA CAPS curriculum per phase.

**Implementation**:
```csharp
public class Subject : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void ValidateCAPSCompliance()
    {
        // Business Rule: Core subjects must be CAPS compliant
        if (IsCore && !IsCAPSCompliant)
        {
            throw new BusinessException(
                "CORE_SUBJECT_NOT_CAPS_COMPLIANT",
                "Core subjects must be CAPS compliant."
            );
        }

        // Business Rule: Language subjects must specify type
        if (IsLanguage && string.IsNullOrWhiteSpace(LanguageType))
        {
            throw new BusinessException(
                "LANGUAGE_TYPE_REQUIRED",
                "Language subjects must specify type: Home Language or First Additional Language."
            );
        }

        // Business Rule: Validate language type values
        if (IsLanguage && LanguageType != "Home Language" && LanguageType != "First Additional Language")
        {
            throw new BusinessException(
                "INVALID_LANGUAGE_TYPE",
                "Language type must be 'Home Language' or 'First Additional Language'."
            );
        }
    }
}
```

**Validation**:
- ✅ Core subjects are CAPS compliant
- ✅ Language subjects specify Home/First Additional
- ✅ Subject aligns with applicable phase

---

## Enrollment & Registration Rules

### ER-001: Student Must Have Grade and Class

**Rule**: Active students MUST be assigned to both a Grade and a Class.

**Implementation**:
```csharp
public class Student : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void Activate()
    {
        // Business Rule: Cannot activate student without grade and class
        if (CurrentGradeId == Guid.Empty)
        {
            throw new BusinessException(
                "GRADE_REQUIRED",
                "Student must be assigned to a grade before activation."
            );
        }

        if (CurrentClassId == Guid.Empty)
        {
            throw new BusinessException(
                "CLASS_REQUIRED",
                "Student must be assigned to a class before activation."
            );
        }

        IsActive = true;
    }
}
```

**Validation**:
- ✅ `CurrentGradeId` must be set
- ✅ `CurrentClassId` must be set
- ✅ Both must exist in same tenant
- ✅ Class must belong to the assigned grade

---

### ER-002: SA ID Number Validation

**Rule**: SA citizens must have valid SA ID number (Luhn algorithm).

**Implementation**:
```csharp
public class Student : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void SetSAIdNumber(string idNumber)
    {
        // Business Rule: SA ID must pass Luhn validation
        try
        {
            IdNumber = new SouthAfricanIdNumber(idNumber);

            // Auto-populate fields from ID
            DateOfBirth = IdNumber.GetDateOfBirth();
            Gender = IdNumber.GetGender();
            IsSACitizen = IdNumber.IsCitizen();
        }
        catch (BusinessException ex)
        {
            throw new BusinessException(
                "INVALID_SA_ID_NUMBER",
                "Invalid South African ID number format or checksum.",
                ex
            );
        }

        // Business Rule: DateOfBirth from ID must make student age-appropriate
        var age = (Clock.Now - DateOfBirth).Days / 365;
        if (age < 3 || age > 25)
        {
            throw new BusinessException(
                "INVALID_STUDENT_AGE",
                $"Student age ({age}) is outside acceptable range (3-25 years)."
            );
        }
    }

    public void SetPassport(string passportNumber)
    {
        // Business Rule: Non-SA citizens must have passport
        if (string.IsNullOrWhiteSpace(passportNumber))
        {
            throw new BusinessException(
                "PASSPORT_REQUIRED",
                "Non-SA citizens must provide passport number."
            );
        }

        PassportNumber = passportNumber;
        IsSACitizen = false;
    }
}
```

**Validation**:
- ✅ SA ID passes Luhn checksum validation
- ✅ Date of birth extracted from ID is valid
- ✅ Gender extracted from ID
- ✅ Student age is appropriate (3-25 years)
- ✅ Non-SA citizens must provide passport

---

### ER-003: Admission Number Uniqueness

**Rule**: Admission number must be unique per tenant.

**Implementation**:
```csharp
public class StudentAppService : psmsAppServiceBase
{
    public async Task<StudentDto> CreateAsync(CreateStudentDto input)
    {
        // Business Rule: Admission number must be unique per tenant
        var existing = await _studentRepository.FirstOrDefaultAsync(
            s => s.TenantId == AbpSession.TenantId &&
                 s.AdmissionNumber == input.AdmissionNumber
        );

        if (existing != null)
        {
            throw new UserFriendlyException(
                "ADMISSION_NUMBER_ALREADY_EXISTS",
                $"Admission number '{input.AdmissionNumber}' is already in use."
            );
        }

        // Create student...
    }
}
```

**Validation**:
- ✅ Unique within tenant
- ✅ Cannot be empty or null
- ✅ Format validation (e.g., "2024-001")

---

### ER-004: Parent-Student Link Required

**Rule**: Every student must have at least ONE parent/guardian linked.

**Implementation**:
```csharp
public class Student : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void ValidateParentLinks()
    {
        // Business Rule: Student must have at least one parent
        if (!ParentLinks.Any(p => !p.IsDeleted))
        {
            throw new BusinessException(
                "NO_PARENT_LINKED",
                "Student must have at least one parent or guardian linked."
            );
        }

        // Business Rule: Must have at least one primary contact
        if (!ParentLinks.Any(p => p.IsPrimaryContact && !p.IsDeleted))
        {
            throw new BusinessException(
                "NO_PRIMARY_CONTACT",
                "Student must have at least one parent designated as primary contact."
            );
        }

        // Business Rule: Only one primary contact allowed
        if (ParentLinks.Count(p => p.IsPrimaryContact && !p.IsDeleted) > 1)
        {
            throw new BusinessException(
                "MULTIPLE_PRIMARY_CONTACTS",
                "Student can only have one primary contact parent."
            );
        }
    }
}
```

**Validation**:
- ✅ At least 1 parent linked
- ✅ Exactly 1 primary contact
- ✅ Primary contact must have `CanMakePayments = true`

---

### ER-005: Subject Enrollment Rules

**Rule**: Students must be enrolled in subjects appropriate for their grade.

**Implementation**:
```csharp
public class StudentEnrollmentManager : DomainService
{
    public async Task EnrollInSubject(Student student, Subject subject, AcademicYear academicYear)
    {
        // Business Rule: Subject must be available for student's grade
        var gradeSubject = await _gradeSubjectRepository.FirstOrDefaultAsync(
            gs => gs.GradeId == student.CurrentGradeId && gs.SubjectId == subject.Id
        );

        if (gradeSubject == null)
        {
            throw new BusinessException(
                "SUBJECT_NOT_AVAILABLE_FOR_GRADE",
                $"Subject '{subject.Name}' is not available for {student.CurrentGrade.Name}."
            );
        }

        // Business Rule: Cannot enroll in subject twice in same academic year
        var existing = await _studentSubjectRepository.FirstOrDefaultAsync(
            ss => ss.StudentId == student.Id &&
                  ss.SubjectId == subject.Id &&
                  ss.AcademicYearId == academicYear.Id &&
                  ss.IsActive
        );

        if (existing != null)
        {
            throw new BusinessException(
                "ALREADY_ENROLLED_IN_SUBJECT",
                $"Student is already enrolled in '{subject.Name}' for this academic year."
            );
        }

        // Business Rule: Foundation phase students must take all subjects
        if (student.CurrentGrade.SchoolPhase == SouthAfricanSchoolPhase.Foundation)
        {
            if (!gradeSubject.IsCompulsory)
            {
                throw new BusinessException(
                    "FOUNDATION_PHASE_NO_ELECTIVES",
                    "Foundation phase students must take all subjects (no electives)."
                );
            }
        }

        // Create enrollment
        var enrollment = new StudentSubject
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            AcademicYearId = academicYear.Id,
            EnrollmentDate = Clock.Now,
            IsActive = true
        };

        await _studentSubjectRepository.InsertAsync(enrollment);
    }
}
```

**Validation**:
- ✅ Subject available for student's grade
- ✅ No duplicate enrollments in same academic year
- ✅ Foundation phase: All subjects compulsory
- ✅ FET phase: Minimum subject requirements met

---

## Grading & Assessment Rules

### GA-001: SA 7-Level Achievement Grading

**Rule**: All marks must be converted to SA 7-level achievement scale.

**Implementation**:
```csharp
public class Mark : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void CalculateAchievement(SouthAfricanGradingService gradingService)
    {
        // Business Rule: Percentage must be calculated
        if (TotalMarks == 0)
        {
            throw new BusinessException(
                "INVALID_TOTAL_MARKS",
                "Total marks cannot be zero."
            );
        }

        Percentage = (MarkValue / TotalMarks) * 100;

        // Business Rule: Percentage must be 0-100
        if (Percentage < 0 || Percentage > 100)
        {
            throw new BusinessException(
                "INVALID_PERCENTAGE",
                $"Percentage ({Percentage:F2}%) must be between 0 and 100."
            );
        }

        // Business Rule: Calculate SA achievement level
        AchievementLevel = gradingService.CalculateAchievementLevel(Percentage);
        AchievementDescription = gradingService.GetAchievementDescription(AchievementLevel);
    }
}
```

**SA Achievement Scale**:
- Level 7 (Outstanding): 80-100%
- Level 6 (Meritorious): 70-79%
- Level 5 (Substantial): 60-69%
- Level 4 (Adequate): 50-59% ✅ **Pass**
- Level 3 (Moderate): 40-49%
- Level 2 (Elementary): 30-39%
- Level 1 (Not Achieved): 0-29%

**Validation**:
- ✅ Achievement level calculated automatically
- ✅ Percentage within 0-100%
- ✅ Level 4+ is passing grade

---

### GA-002: Locked Marks Cannot Be Edited

**Rule**: Once marks are locked, they CANNOT be modified.

**Implementation**:
```csharp
public class Mark : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void UpdateMark(decimal newMarkValue, decimal newTotalMarks)
    {
        // Business Rule: Cannot update locked marks
        if (IsLocked)
        {
            throw new BusinessException(
                "MARK_IS_LOCKED",
                $"Mark is locked since {LockedAt:yyyy-MM-dd} and cannot be modified."
            );
        }

        MarkValue = newMarkValue;
        TotalMarks = newTotalMarks;
        // Recalculate percentage and achievement level...
    }

    public void Lock(long userId)
    {
        // Business Rule: Cannot lock already locked mark
        if (IsLocked)
        {
            throw new BusinessException(
                "MARK_ALREADY_LOCKED",
                "Mark is already locked."
            );
        }

        // Business Rule: Must have valid mark value
        if (MarkValue < 0 || MarkValue > TotalMarks)
        {
            throw new BusinessException(
                "INVALID_MARK_VALUE",
                "Cannot lock mark with invalid mark value."
            );
        }

        IsLocked = true;
        LockedAt = Clock.Now;
        LockedBy = userId;
    }

    public void Unlock(long userId)
    {
        // Business Rule: Only specific roles can unlock (e.g., Principal)
        // This should be checked in application service with authorization

        IsLocked = false;
        LockedAt = null;
        LockedBy = null;
    }
}
```

**Validation**:
- ✅ Locked marks throw exception on edit attempt
- ✅ Only authorized users can unlock (Principal, Admin)
- ✅ Audit trail maintained (LockedAt, LockedBy)
- ✅ Must validate mark before locking

---

### GA-003: Assessment Mark Constraints

**Rule**: Student marks for an assessment must be within assessment total marks.

**Implementation**:
```csharp
public class Mark : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void ValidateMarkValue()
    {
        // Business Rule: Mark value cannot be negative
        if (MarkValue < 0)
        {
            throw new BusinessException(
                "NEGATIVE_MARK_VALUE",
                "Mark value cannot be negative."
            );
        }

        // Business Rule: Mark value cannot exceed total marks
        if (MarkValue > TotalMarks)
        {
            throw new BusinessException(
                "MARK_EXCEEDS_TOTAL",
                $"Mark value ({MarkValue}) cannot exceed total marks ({TotalMarks})."
            );
        }

        // Business Rule: If linked to assessment, total marks must match
        if (AssessmentId.HasValue && Assessment != null)
        {
            if (TotalMarks != Assessment.TotalMarks)
            {
                throw new BusinessException(
                    "TOTAL_MARKS_MISMATCH",
                    $"Total marks ({TotalMarks}) must match assessment total marks ({Assessment.TotalMarks})."
                );
            }
        }
    }
}
```

**Validation**:
- ✅ 0 ≤ MarkValue ≤ TotalMarks
- ✅ TotalMarks matches Assessment.TotalMarks
- ✅ Marks can only be captured by assigned teacher

---

### GA-004: Teacher Assignment to Mark

**Rule**: Only the assigned subject teacher can capture/modify marks.

**Implementation**:
```csharp
public class MarkAppService : psmsAppServiceBase
{
    public async Task<MarkDto> CreateAsync(CreateMarkDto input)
    {
        // Get current user teacher profile
        var teacher = await _teacherRepository.FirstOrDefaultAsync(
            t => t.UserId == AbpSession.UserId && t.TenantId == AbpSession.TenantId
        );

        if (teacher == null)
        {
            throw new UserFriendlyException("Only teachers can capture marks.");
        }

        // Business Rule: Teacher must be assigned to the subject
        var teacherSubject = await _teacherSubjectRepository.FirstOrDefaultAsync(
            ts => ts.TeacherId == teacher.Id &&
                  ts.SubjectId == input.SubjectId &&
                  !ts.IsDeleted
        );

        if (teacherSubject == null)
        {
            throw new BusinessException(
                "TEACHER_NOT_ASSIGNED_TO_SUBJECT",
                "You are not assigned to teach this subject."
            );
        }

        // Business Rule: Teacher must be assigned to student's class
        var student = await _studentRepository.GetAsync(input.StudentId);
        var teacherClass = await _teacherClassRepository.FirstOrDefaultAsync(
            tc => tc.TeacherId == teacher.Id &&
                  tc.ClassId == student.CurrentClassId &&
                  tc.SubjectId == input.SubjectId &&
                  !tc.IsDeleted
        );

        if (teacherClass == null)
        {
            throw new BusinessException(
                "TEACHER_NOT_ASSIGNED_TO_CLASS",
                "You are not assigned to teach this class."
            );
        }

        // Create mark...
    }
}
```

**Validation**:
- ✅ User is a Teacher
- ✅ Teacher assigned to subject
- ✅ Teacher assigned to student's class for that subject
- ✅ Authorization via ABP permissions

---

### GA-005: Term-Specific Marks

**Rule**: Marks must belong to a valid, non-future term.

**Implementation**:
```csharp
public class Mark : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void ValidateTerm()
    {
        // Business Rule: Term must exist
        if (Term == null)
        {
            throw new BusinessException(
                "INVALID_TERM",
                "Mark must be associated with a valid term."
            );
        }

        // Business Rule: Cannot capture marks for future terms
        if (Term.StartDate > Clock.Now)
        {
            throw new BusinessException(
                "FUTURE_TERM_MARKS",
                $"Cannot capture marks for future term (starts {Term.StartDate:yyyy-MM-dd})."
            );
        }

        // Business Rule: Warning for past term marks (after term end + 30 days)
        var cutoffDate = Term.EndDate.AddDays(30);
        if (Clock.Now > cutoffDate && !IsLocked)
        {
            // Log warning but allow (teacher might have been late)
            Logger.Warn($"Mark being captured for past term {Term.Name} after cutoff date.");
        }
    }
}
```

**Validation**:
- ✅ Term must be valid and exist
- ✅ Cannot capture for future terms
- ✅ Warning for very late captures (30+ days after term end)

---

### GA-006: Duplicate Mark Prevention

**Rule**: Student cannot have multiple unlocked marks for same subject/assessment/term.

**Implementation**:
```csharp
public class MarkAppService : psmsAppServiceBase
{
    public async Task<MarkDto> CreateAsync(CreateMarkDto input)
    {
        // Business Rule: Check for duplicate unlocked marks
        var existingMark = await _markRepository.FirstOrDefaultAsync(
            m => m.StudentId == input.StudentId &&
                 m.SubjectId == input.SubjectId &&
                 m.TermId == input.TermId &&
                 m.AssessmentId == input.AssessmentId &&
                 !m.IsLocked &&
                 !m.IsDeleted
        );

        if (existingMark != null)
        {
            throw new BusinessException(
                "DUPLICATE_MARK",
                "Student already has an unlocked mark for this assessment. Update existing mark instead."
            );
        }

        // Create mark...
    }
}
```

**Validation**:
- ✅ No duplicate unlocked marks per student/subject/assessment/term
- ✅ Locked marks can coexist (historical record)

---

## Attendance Rules

### AT-001: Daily Attendance Capture

**Rule**: Attendance can only be captured for current or past dates, not future.

**Implementation**:
```csharp
public class Attendance : CreationAuditedEntity<Guid>, IMultiTenant
{
    public void ValidateAttendanceDate()
    {
        // Business Rule: Cannot capture attendance for future dates
        if (AttendanceDate.Date > Clock.Now.Date)
        {
            throw new BusinessException(
                "FUTURE_ATTENDANCE",
                "Cannot capture attendance for future dates."
            );
        }

        // Business Rule: Cannot capture attendance for dates too far in past (30 days)
        var cutoffDate = Clock.Now.AddDays(-30);
        if (AttendanceDate.Date < cutoffDate.Date)
        {
            throw new BusinessException(
                "ATTENDANCE_TOO_OLD",
                "Cannot capture attendance for dates more than 30 days in the past."
            );
        }

        // Business Rule: Attendance must be on school day (Mon-Fri)
        if (AttendanceDate.DayOfWeek == DayOfWeek.Saturday ||
            AttendanceDate.DayOfWeek == DayOfWeek.Sunday)
        {
            throw new BusinessException(
                "ATTENDANCE_ON_WEEKEND",
                "Cannot capture attendance for weekends."
            );
        }

        // Business Rule: Check if date is SA public holiday
        var isPublicHoliday = _publicHolidayRepository.Any(
            h => h.Date.Date == AttendanceDate.Date
        );

        if (isPublicHoliday)
        {
            throw new BusinessException(
                "ATTENDANCE_ON_HOLIDAY",
                "Cannot capture attendance on public holiday."
            );
        }
    }
}
```

**Validation**:
- ✅ Today or past date only
- ✅ Maximum 30 days in past
- ✅ Not on weekends
- ✅ Not on SA public holidays

---

### AT-002: Teacher Authorization for Attendance

**Rule**: Only assigned class/subject teacher can capture attendance.

**Implementation**:
```csharp
public class AttendanceAppService : psmsAppServiceBase
{
    public async Task CaptureClassAttendance(CaptureAttendanceDto input)
    {
        var teacher = await GetCurrentTeacher();

        // Business Rule: Teacher must be assigned to class
        var teacherClass = await _teacherClassRepository.FirstOrDefaultAsync(
            tc => tc.TeacherId == teacher.Id &&
                  tc.ClassId == input.ClassId &&
                  !tc.IsDeleted
        );

        if (teacherClass == null && !await IsClassTeacher(teacher.Id, input.ClassId))
        {
            throw new BusinessException(
                "NOT_AUTHORIZED_FOR_CLASS",
                "You are not authorized to capture attendance for this class."
            );
        }

        // Capture attendance for all students in class...
    }

    private async Task<bool> IsClassTeacher(Guid teacherId, Guid classId)
    {
        var @class = await _classRepository.GetAsync(classId);
        return @class.ClassTeacherId == teacherId;
    }
}
```

**Validation**:
- ✅ User is a Teacher
- ✅ Teacher assigned to class OR is class teacher
- ✅ Teacher assigned to subject (for subject-specific attendance)

---

### AT-003: Duplicate Attendance Prevention

**Rule**: Student cannot have duplicate attendance records for same date/class/subject.

**Implementation**:
```csharp
public async Task<AttendanceDto> CreateAsync(CreateAttendanceDto input)
{
    // Business Rule: Check for duplicate attendance
    var existing = await _attendanceRepository.FirstOrDefaultAsync(
        a => a.StudentId == input.StudentId &&
             a.ClassId == input.ClassId &&
             a.SubjectId == input.SubjectId &&
             a.AttendanceDate.Date == input.AttendanceDate.Date &&
             !a.IsDeleted
    );

    if (existing != null)
    {
        throw new BusinessException(
            "DUPLICATE_ATTENDANCE",
            "Attendance already captured for this student on this date. Update existing record."
        );
    }

    // Create attendance...
}
```

**Validation**:
- ✅ No duplicate attendance per student/date/class/subject
- ✅ Update existing record instead

---

## Reporting Rules

### RE-001: Report Generation Prerequisites

**Rule**: Reports can only be generated when ALL marks for the term are locked.

**Implementation**:
```csharp
public class ReportGenerationManager : DomainService
{
    public async Task<Report> GenerateTermReport(Student student, Term term)
    {
        // Business Rule: Check if all marks are locked
        var unlockedMarks = await _markRepository.GetAllListAsync(
            m => m.StudentId == student.Id &&
                 m.TermId == term.Id &&
                 !m.IsLocked &&
                 !m.IsDeleted
        );

        if (unlockedMarks.Any())
        {
            throw new BusinessException(
                "MARKS_NOT_LOCKED",
                $"Cannot generate report. {unlockedMarks.Count} marks are still unlocked."
            );
        }

        // Business Rule: Student must have marks for the term
        var marks = await _markRepository.GetMarksByStudentAndTermAsync(
            student.Id, term.Id
        );

        if (!marks.Any())
        {
            throw new BusinessException(
                "NO_MARKS_FOUND",
                "Cannot generate report. No marks found for this term."
            );
        }

        // Business Rule: Term must be completed or current
        if (term.StartDate > Clock.Now)
        {
            throw new BusinessException(
                "TERM_NOT_STARTED",
                "Cannot generate report for future term."
            );
        }

        // Generate report...
    }
}
```

**Validation**:
- ✅ All marks locked
- ✅ At least some marks exist
- ✅ Term has started
- ✅ Student has attendance records

---

### RE-002: SA Report Card Requirements

**Rule**: Reports must include ALL SA-mandated fields.

**Implementation**:
```csharp
public class Report : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void ValidateSAReportCard()
    {
        // Business Rule: Must have report card number
        if (string.IsNullOrWhiteSpace(ReportCardNumber))
        {
            throw new BusinessException(
                "REPORT_CARD_NUMBER_REQUIRED",
                "SA report cards must have a unique report card number."
            );
        }

        // Business Rule: Must have attendance data
        if (DaysInTerm <= 0)
        {
            throw new BusinessException(
                "DAYS_IN_TERM_REQUIRED",
                "Report must specify total days in term."
            );
        }

        if (DaysAttended < 0 || DaysAbsent < 0)
        {
            throw new BusinessException(
                "INVALID_ATTENDANCE_DATA",
                "Attendance data cannot be negative."
            );
        }

        if (DaysAttended + DaysAbsent != DaysInTerm)
        {
            throw new BusinessException(
                "ATTENDANCE_MISMATCH",
                "Days attended + days absent must equal total days in term."
            );
        }

        // Business Rule: Must have conduct rating
        if (string.IsNullOrWhiteSpace(ConductRating))
        {
            throw new BusinessException(
                "CONDUCT_RATING_REQUIRED",
                "SA report cards must include conduct rating."
            );
        }

        // Business Rule: Must have at least one subject
        if (!ReportSubjects.Any())
        {
            throw new BusinessException(
                "NO_SUBJECTS_IN_REPORT",
                "Report must include at least one subject."
            );
        }

        // Business Rule: All subjects must have achievement levels
        foreach (var reportSubject in ReportSubjects)
        {
            if (reportSubject.AchievementLevel == 0)
            {
                throw new BusinessException(
                    "ACHIEVEMENT_LEVEL_REQUIRED",
                    $"Subject '{reportSubject.Subject?.Name}' must have achievement level."
                );
            }
        }
    }
}
```

**Validation**:
- ✅ Unique report card number
- ✅ Attendance data (days in term, attended, absent)
- ✅ Conduct and diligence ratings
- ✅ Overall achievement level
- ✅ Subject-specific achievement levels
- ✅ Teacher and principal comments

---

### RE-003: Report Publishing Rules

**Rule**: Reports can only be published once validated and signed.

**Implementation**:
```csharp
public class Report : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void Publish()
    {
        // Business Rule: Cannot publish already published report
        if (IsPublished)
        {
            throw new BusinessException(
                "REPORT_ALREADY_PUBLISHED",
                $"Report was already published on {PublishedDate:yyyy-MM-dd}."
            );
        }

        // Business Rule: Must be validated first
        ValidateSAReportCard();

        // Business Rule: Teacher must have signed
        if (!TeacherSigned)
        {
            throw new BusinessException(
                "TEACHER_SIGNATURE_REQUIRED",
                "Teacher must sign report before publishing."
            );
        }

        // Business Rule: Principal must have signed
        if (!PrincipalSigned)
        {
            throw new BusinessException(
                "PRINCIPAL_SIGNATURE_REQUIRED",
                "Principal must sign report before publishing."
            );
        }

        // Business Rule: Must have report file (PDF)
        if (string.IsNullOrWhiteSpace(ReportFileUrl))
        {
            throw new BusinessException(
                "REPORT_FILE_REQUIRED",
                "Report PDF must be generated before publishing."
            );
        }

        IsPublished = true;
        PublishedDate = Clock.Now;

        // Trigger domain event for notification
        DomainEvents.Raise(new ReportPublishedEvent
        {
            ReportId = this.Id,
            StudentId = this.StudentId,
            TermId = this.TermId
        });
    }
}
```

**Validation**:
- ✅ All validations pass
- ✅ Teacher signed
- ✅ Principal signed
- ✅ PDF generated
- ✅ Notification sent to parents on publish

---

## Financial Management Rules

### FI-001: Fee Structure Validation

**Rule**: Fee structures must be valid and complete before assignment.

**Implementation**:
```csharp
public class FeeStructure : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void Validate()
    {
        // Business Rule: Amount must be positive
        if (Amount.Amount <= 0)
        {
            throw new BusinessException(
                "INVALID_FEE_AMOUNT",
                "Fee amount must be greater than zero."
            );
        }

        // Business Rule: Currency must be ZAR for SA schools
        if (Amount.Currency != "ZAR")
        {
            throw new BusinessException(
                "INVALID_CURRENCY",
                "South African schools must use ZAR currency."
            );
        }

        // Business Rule: VAT percentage must be 15% if applicable
        if (IsVATApplicable && VATPercentage != 15m)
        {
            throw new BusinessException(
                "INVALID_VAT_PERCENTAGE",
                "South African VAT rate is 15%."
            );
        }

        // Business Rule: Academic year must be valid
        if (AcademicYear == null || !AcademicYear.IsActive)
        {
            throw new BusinessException(
                "INVALID_ACADEMIC_YEAR",
                "Fee structure must be associated with active academic year."
            );
        }

        // Business Rule: For recurring fees, due day must be specified
        if (PaymentFrequency != PaymentPlan.AnnualUpfront && !DueDay.HasValue)
        {
            throw new BusinessException(
                "DUE_DAY_REQUIRED",
                "Recurring fees must specify due day of month (1-31)."
            );
        }

        if (DueDay.HasValue && (DueDay < 1 || DueDay > 31))
        {
            throw new BusinessException(
                "INVALID_DUE_DAY",
                "Due day must be between 1 and 31."
            );
        }
    }
}
```

**Validation**:
- ✅ Amount > 0
- ✅ Currency = ZAR
- ✅ VAT = 15% if applicable
- ✅ Valid academic year
- ✅ Due day specified for recurring fees

---

### FI-002: Student Fee Assignment

**Rule**: Student fees must be derived from valid fee structures.

**Implementation**:
```csharp
public class FeeCalculationManager : DomainService
{
    public async Task<StudentFee> AssignFeeToStudent(
        Student student,
        FeeStructure feeStructure,
        decimal? discountPercentage = null)
    {
        // Business Rule: Fee structure must be active
        if (!feeStructure.IsActive)
        {
            throw new BusinessException(
                "INACTIVE_FEE_STRUCTURE",
                "Cannot assign inactive fee structure."
            );
        }

        // Business Rule: Fee structure must be for student's grade (if grade-specific)
        if (feeStructure.GradeId.HasValue &&
            feeStructure.GradeId != student.CurrentGradeId)
        {
            throw new BusinessException(
                "GRADE_MISMATCH",
                "Fee structure is for different grade."
            );
        }

        // Business Rule: Check for duplicate assignment
        var existing = await _studentFeeRepository.FirstOrDefaultAsync(
            sf => sf.StudentId == student.Id &&
                  sf.FeeStructureId == feeStructure.Id &&
                  sf.Status != FeeStatus.Paid &&
                  !sf.IsDeleted
        );

        if (existing != null)
        {
            throw new BusinessException(
                "FEE_ALREADY_ASSIGNED",
                "Student already has this fee assigned."
            );
        }

        // Calculate amount with discount
        var amount = feeStructure.Amount;
        Money? discountAmount = null;

        if (discountPercentage.HasValue)
        {
            if (discountPercentage < 0 || discountPercentage > 100)
            {
                throw new BusinessException(
                    "INVALID_DISCOUNT_PERCENTAGE",
                    "Discount percentage must be between 0 and 100."
                );
            }

            discountAmount = new Money(
                amount.Amount * (discountPercentage.Value / 100),
                "ZAR"
            );
        }

        // Create student fee
        var studentFee = new StudentFee
        {
            StudentId = student.Id,
            FeeStructureId = feeStructure.Id,
            Amount = amount,
            DiscountAmount = discountAmount,
            DueDate = CalculateDueDate(feeStructure),
            Status = FeeStatus.Pending
        };

        return studentFee;
    }

    private DateTime CalculateDueDate(FeeStructure feeStructure)
    {
        var now = Clock.Now;

        return feeStructure.PaymentFrequency switch
        {
            PaymentPlan.AnnualUpfront => feeStructure.AcademicYear.StartDate.AddDays(30),
            PaymentPlan.Quarterly => new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, feeStructure.DueDay ?? 15),
            PaymentPlan.Monthly or PaymentPlan.MonthlyDebitOrder => new DateTime(now.Year, now.Month, feeStructure.DueDay ?? 15),
            _ => now.AddDays(30)
        };
    }
}
```

**Validation**:
- ✅ Fee structure active
- ✅ Grade matches (if grade-specific)
- ✅ No duplicate assignments
- ✅ Discount percentage valid (0-100%)
- ✅ Due date calculated correctly

---

### FI-003: Payment Processing Rules

**Rule**: Payments must be validated and allocated correctly.

**Implementation**:
```csharp
public class Payment : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void Validate()
    {
        // Business Rule: Amount must be positive
        if (Amount.Amount <= 0)
        {
            throw new BusinessException(
                "INVALID_PAYMENT_AMOUNT",
                "Payment amount must be greater than zero."
            );
        }

        // Business Rule: Currency must be ZAR
        if (Amount.Currency != "ZAR")
        {
            throw new BusinessException(
                "INVALID_PAYMENT_CURRENCY",
                "Payments must be in ZAR."
            );
        }

        // Business Rule: Payment date cannot be in future
        if (PaymentDate > Clock.Now)
        {
            throw new BusinessException(
                "FUTURE_PAYMENT_DATE",
                "Payment date cannot be in the future."
            );
        }

        // Business Rule: Receipt number must be unique per tenant
        if (string.IsNullOrWhiteSpace(ReceiptNumber))
        {
            throw new BusinessException(
                "RECEIPT_NUMBER_REQUIRED",
                "Receipt number is required."
            );
        }

        // Business Rule: For EFT payments, bank reference required
        if (PaymentMethod == SouthAfricanPaymentMethod.EFT &&
            string.IsNullOrWhiteSpace(BankReference))
        {
            throw new BusinessException(
                "BANK_REFERENCE_REQUIRED",
                "EFT payments must include bank reference."
            );
        }

        // Business Rule: For debit orders, debit day required
        if (IsRecurring && !DebitDay.HasValue)
        {
            throw new BusinessException(
                "DEBIT_DAY_REQUIRED",
                "Recurring payments must specify debit day (1-31)."
            );
        }
    }

    public void Complete()
    {
        // Business Rule: Can only complete pending payment
        if (Status != PaymentStatus.Pending)
        {
            throw new BusinessException(
                "PAYMENT_NOT_PENDING",
                $"Cannot complete payment with status {Status}."
            );
        }

        Status = PaymentStatus.Completed;

        // Trigger domain event
        DomainEvents.Raise(new PaymentReceivedEvent
        {
            PaymentId = this.Id,
            StudentId = this.StudentId,
            Amount = this.Amount
        });
    }
}
```

**Validation**:
- ✅ Amount > 0
- ✅ Currency = ZAR
- ✅ Payment date not in future
- ✅ Unique receipt number per tenant
- ✅ Bank reference for EFT
- ✅ Debit day for recurring payments

---

### FI-004: Payment Allocation Rules

**Rule**: Payments must be fully allocated to student fees.

**Implementation**:
```csharp
public class PaymentAllocationManager : DomainService
{
    public async Task AllocatePayment(Payment payment, List<StudentFee> feesToAllocate)
    {
        // Business Rule: Payment must be completed
        if (payment.Status != PaymentStatus.Completed)
        {
            throw new BusinessException(
                "PAYMENT_NOT_COMPLETED",
                "Can only allocate completed payments."
            );
        }

        // Business Rule: All fees must belong to same student
        if (feesToAllocate.Any(f => f.StudentId != payment.StudentId))
        {
            throw new BusinessException(
                "FEE_STUDENT_MISMATCH",
                "All fees must belong to the payment's student."
            );
        }

        // Business Rule: Calculate total allocation
        decimal totalAllocated = 0;
        var allocations = new List<PaymentAllocation>();

        foreach (var fee in feesToAllocate.OrderBy(f => f.DueDate))
        {
            var outstanding = CalculateOutstanding(fee);
            if (outstanding <= 0) continue; // Fee fully paid

            var toAllocate = Math.Min(
                outstanding,
                payment.Amount.Amount - totalAllocated
            );

            if (toAllocate <= 0) break; // Payment fully allocated

            allocations.Add(new PaymentAllocation
            {
                PaymentId = payment.Id,
                StudentFeeId = fee.Id,
                AllocatedAmount = new Money(toAllocate, "ZAR")
            });

            totalAllocated += toAllocate;

            // Update fee status
            UpdateFeeStatus(fee, toAllocate);
        }

        // Business Rule: Payment must be fully allocated
        if (totalAllocated < payment.Amount.Amount)
        {
            Logger.Warn($"Payment {payment.ReceiptNumber} not fully allocated. " +
                       $"Allocated: R{totalAllocated:F2}, Total: R{payment.Amount.Amount:F2}");
        }

        // Save allocations
        await _paymentAllocationRepository.InsertRangeAsync(allocations);
    }

    private decimal CalculateOutstanding(StudentFee fee)
    {
        var totalAllocated = fee.PaymentAllocations.Sum(pa => pa.AllocatedAmount.Amount);
        var totalFee = fee.Amount.Amount - (fee.DiscountAmount?.Amount ?? 0);
        return totalFee - totalAllocated;
    }

    private void UpdateFeeStatus(StudentFee fee, decimal allocation)
    {
        var outstanding = CalculateOutstanding(fee) - allocation;

        if (outstanding <= 0)
            fee.Status = FeeStatus.Paid;
        else if (outstanding < fee.Amount.Amount)
            fee.Status = FeeStatus.PartiallyPaid;
        else
            fee.Status = FeeStatus.Pending;
    }
}
```

**Validation**:
- ✅ Payment completed before allocation
- ✅ Fees belong to payment student
- ✅ Allocate in due date order (oldest first)
- ✅ Update fee status correctly
- ✅ Handle over-payments (credit)

---

### FI-005: Fee Status Rules

**Rule**: Fee status must reflect payment state accurately.

**Fee Status Flow**:
```
Pending → PartiallyPaid → Paid
   ↓            ↓           ↓
Overdue      Overdue     [Final]
   ↓
Waived
```

**Implementation**:
```csharp
public class StudentFee : CreationAuditedEntity<Guid>
{
    public void UpdateStatus()
    {
        var outstanding = CalculateOutstanding();

        // Business Rule: Update status based on outstanding amount
        if (outstanding <= 0)
        {
            Status = FeeStatus.Paid;
        }
        else if (outstanding < Amount.Amount)
        {
            Status = FeeStatus.PartiallyPaid;
        }
        else
        {
            Status = FeeStatus.Pending;
        }

        // Business Rule: Mark as overdue if past due date
        if (Clock.Now.Date > DueDate.Date && Status != FeeStatus.Paid && Status != FeeStatus.Waived)
        {
            Status = FeeStatus.Overdue;
        }
    }

    public void Waive(string reason)
    {
        // Business Rule: Cannot waive paid fees
        if (Status == FeeStatus.Paid)
        {
            throw new BusinessException(
                "CANNOT_WAIVE_PAID_FEE",
                "Cannot waive fee that is already paid."
            );
        }

        // Business Rule: Reason required for waiver
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessException(
                "WAIVER_REASON_REQUIRED",
                "Reason required to waive fee."
            );
        }

        Status = FeeStatus.Waived;
        DiscountReason = reason;
        DiscountAmount = Amount; // Full waiver
    }
}
```

**Validation**:
- ✅ Status reflects actual payment state
- ✅ Overdue status applied automatically
- ✅ Waiver requires reason
- ✅ Cannot waive paid fees

---

## Communication Rules

### CO-001: Announcement Targeting

**Rule**: Announcements must have valid target audience.

**Implementation**:
```csharp
public class Announcement : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void ValidateTargeting()
    {
        // Business Rule: If targeting specific grade, grade must be specified
        if (TargetAudience == TargetAudience.Grade && !GradeId.HasValue)
        {
            throw new BusinessException(
                "GRADE_REQUIRED",
                "Grade must be specified when targeting specific grade."
            );
        }

        // Business Rule: If targeting specific class, class must be specified
        if (TargetAudience == TargetAudience.Class && !ClassId.HasValue)
        {
            throw new BusinessException(
                "CLASS_REQUIRED",
                "Class must be specified when targeting specific class."
            );
        }

        // Business Rule: School-wide announcements should not have grade/class
        if (TargetAudience == TargetAudience.School && (GradeId.HasValue || ClassId.HasValue))
        {
            Logger.Warn("School-wide announcement has grade/class specified. These will be ignored.");
            GradeId = null;
            ClassId = null;
        }

        // Business Rule: Expiry date must be after publish date
        if (ExpiryDate.HasValue && ExpiryDate < PublishDate)
        {
            throw new BusinessException(
                "INVALID_EXPIRY_DATE",
                "Expiry date must be after publish date."
            );
        }
    }

    public void Publish()
    {
        // Business Rule: Cannot publish empty announcement
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Content))
        {
            throw new BusinessException(
                "EMPTY_ANNOUNCEMENT",
                "Announcement must have title and content."
            );
        }

        ValidateTargeting();

        IsPublished = true;
        PublishDate = Clock.Now;
    }
}
```

**Validation**:
- ✅ Target audience specified
- ✅ Grade/Class specified for targeted announcements
- ✅ Expiry date after publish date
- ✅ Not empty when published

---

### CO-002: Message Permissions

**Rule**: Users can only send messages to authorized recipients.

**Implementation**:
```csharp
public class MessageAppService : psmsAppServiceBase
{
    public async Task<MessageDto> SendAsync(SendMessageDto input)
    {
        var sender = await GetCurrentUser();
        var recipient = await GetUser(input.RecipientUserId);

        // Business Rule: Sender and recipient must be in same tenant
        if (sender.TenantId != recipient.TenantId)
        {
            throw new BusinessException(
                "CROSS_TENANT_MESSAGE",
                "Cannot send messages to users in different schools."
            );
        }

        // Business Rule: Validate sender-recipient relationship
        await ValidateMessagePermission(sender, recipient);

        // Create message...
    }

    private async Task ValidateMessagePermission(User sender, User recipient)
    {
        var senderRole = sender.Roles.FirstOrDefault()?.RoleId;
        var recipientRole = recipient.Roles.FirstOrDefault()?.RoleId;

        // Parents can message:
        // - Teachers of their children
        // - Admin
        if (await IsParent(sender))
        {
            if (await IsTeacher(recipient))
            {
                // Check if recipient teaches sender's child
                var parent = await _parentRepository.FirstOrDefaultAsync(p => p.UserId == sender.Id);
                var teacher = await _teacherRepository.FirstOrDefaultAsync(t => t.UserId == recipient.Id);

                var hasSharedStudent = await _studentParentRepository
                    .GetAll()
                    .Where(sp => sp.ParentId == parent.Id)
                    .Join(_studentRepository.GetAll().Where(s => s.CurrentClassId == teacher.ClassId),
                          sp => sp.StudentId,
                          s => s.Id,
                          (sp, s) => s)
                    .AnyAsync();

                if (!hasSharedStudent && !await IsAdmin(recipient))
                {
                    throw new BusinessException(
                        "UNAUTHORIZED_MESSAGE",
                        "You can only message teachers of your children or school administrators."
                    );
                }
            }
        }

        // Teachers can message:
        // - Parents of their students
        // - Other teachers
        // - Admin
        if (await IsTeacher(sender))
        {
            if (await IsParent(recipient))
            {
                var teacher = await _teacherRepository.FirstOrDefaultAsync(t => t.UserId == sender.Id);
                var parent = await _parentRepository.FirstOrDefaultAsync(p => p.UserId == recipient.Id);

                var hasSharedStudent = await _studentParentRepository
                    .GetAll()
                    .Where(sp => sp.ParentId == parent.Id)
                    .Join(_studentRepository.GetAll().Where(s => s.CurrentClassId == teacher.ClassId),
                          sp => sp.StudentId,
                          s => s.Id,
                          (sp, s) => s)
                    .AnyAsync();

                if (!hasSharedStudent)
                {
                    throw new BusinessException(
                        "UNAUTHORIZED_MESSAGE",
                        "You can only message parents of your students."
                    );
                }
            }
        }

        // Admin can message anyone in tenant
        // Students cannot send messages (optional business rule)
    }
}
```

**Validation**:
- ✅ Same tenant only
- ✅ Parents → Teachers of their children, Admin
- ✅ Teachers → Parents of their students, Other teachers, Admin
- ✅ Admin → Anyone in tenant
- ✅ Students → (Restricted or allowed based on school policy)

---

## Multi-Tenancy Rules

### MT-001: Tenant Data Isolation

**Rule**: Users can ONLY access data from their own tenant.

**Implementation**:
```csharp
// ABP handles this automatically via IMultiTenant interface
// But for extra validation:

public class StudentAppService : psmsAppServiceBase
{
    public async Task<StudentDto> GetAsync(Guid id)
    {
        var student = await _studentRepository.GetAsync(id);

        // Business Rule: Extra tenant check (ABP already filters, but for safety)
        if (student.TenantId != AbpSession.TenantId)
        {
            throw new BusinessException(
                "CROSS_TENANT_ACCESS",
                "Access denied. Student belongs to different school."
            );
        }

        return ObjectMapper.Map<StudentDto>(student);
    }
}
```

**Validation**:
- ✅ ABP automatic filtering via `IMultiTenant`
- ✅ No cross-tenant data access
- ✅ Explicit checks in sensitive operations

---

### MT-002: Junction Table Tenant Validation

**Rule**: Junction table entities must reference entities from same tenant.

**Implementation**:
```csharp
public class StudentParent : Entity<Guid>
{
    public void ValidateTenancy()
    {
        // Business Rule: Student and Parent must belong to same tenant
        if (Student.TenantId != Parent.TenantId)
        {
            throw new BusinessException(
                "CROSS_TENANT_RELATIONSHIP",
                "Student and Parent must belong to the same school."
            );
        }
    }
}

public class TeacherSubject : Entity<Guid>
{
    public void ValidateTenancy()
    {
        // Business Rule: Teacher and Subject must belong to same tenant
        if (Teacher.TenantId != Subject.TenantId)
        {
            throw new BusinessException(
                "CROSS_TENANT_RELATIONSHIP",
                "Teacher and Subject must belong to the same school."
            );
        }
    }
}
```

**Validation**:
- ✅ All related entities in same tenant
- ✅ Validate on create and update
- ✅ Apply to all junction tables

---

### MT-003: User-Tenant Matching

**Rule**: User.TenantId must match domain entity.TenantId.

**Implementation**:
```csharp
public class Teacher : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void LinkUser(User user)
    {
        // Business Rule: User tenant must match Teacher tenant
        if (user.TenantId != this.TenantId)
        {
            throw new BusinessException(
                "USER_TENANT_MISMATCH",
                "User and Teacher must belong to the same tenant (school)."
            );
        }

        UserId = user.Id;
    }
}

public class Parent : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void LinkUser(User user)
    {
        // Business Rule: User tenant must match Parent tenant
        if (user.TenantId != this.TenantId)
        {
            throw new BusinessException(
                "USER_TENANT_MISMATCH",
                "User and Parent must belong to the same tenant (school)."
            );
        }

        UserId = user.Id;
    }
}
```

**Validation**:
- ✅ User.TenantId == Entity.TenantId
- ✅ Validate on user linking
- ✅ Apply to Teacher, Parent entities

---

## Security & Authorization Rules

### SE-001: Role-Based Access Control

**Rules**:

| Role | Can Access |
|------|-----------|
| **Host Admin** | All tenants, platform settings |
| **School Admin** | All data in their tenant |
| **Principal** | All academic data, can unlock marks |
| **Teacher** | Own classes, students, subjects, marks |
| **Parent** | Own children's data only |
| **Student** | Own data only (read-only) |
| **Finance** | Financial data, fee structures, payments |

**Implementation**:
```csharp
// Define permissions in *PermissionNames.cs
public static class AppPermissions
{
    // Academic
    public const string Pages_Students = "Pages.Students";
    public const string Pages_Students_Create = "Pages.Students.Create";
    public const string Pages_Students_Edit = "Pages.Students.Edit";
    public const string Pages_Students_Delete = "Pages.Students.Delete";

    // Marks
    public const string Pages_Marks = "Pages.Marks";
    public const string Pages_Marks_Create = "Pages.Marks.Create";
    public const string Pages_Marks_Edit = "Pages.Marks.Edit";
    public const string Pages_Marks_Lock = "Pages.Marks.Lock";
    public const string Pages_Marks_Unlock = "Pages.Marks.Unlock"; // Principal only

    // Financial
    public const string Pages_Payments = "Pages.Payments";
    public const string Pages_Payments_Process = "Pages.Payments.Process";
    public const string Pages_FeeStructures = "Pages.FeeStructures";
}

// Use in Application Services
[AbpAuthorize(AppPermissions.Pages_Marks_Create)]
public async Task<MarkDto> CreateAsync(CreateMarkDto input)
{
    // Only users with Pages_Marks_Create permission can execute
    // ...
}

[AbpAuthorize(AppPermissions.Pages_Marks_Unlock)]
public async Task UnlockMarkAsync(Guid markId)
{
    // Only Principals/Admins can unlock marks
    // ...
}
```

**Validation**:
- ✅ ABP `[AbpAuthorize]` attribute on all app services
- ✅ Granular permissions per operation
- ✅ Role-based permission assignments
- ✅ Custom authorization rules for complex scenarios

---

### SE-002: Data Ownership Rules

**Rule**: Users can only access/modify data they own or are authorized for.

**Implementation**:
```csharp
public class MarkAppService : psmsAppServiceBase
{
    [AbpAuthorize(AppPermissions.Pages_Marks_Edit)]
    public async Task<MarkDto> UpdateAsync(Guid id, UpdateMarkDto input)
    {
        var mark = await _markRepository.GetAsync(id);
        var currentUser = await GetCurrentUserAsync();

        // Business Rule: Only the teacher who captured the mark can edit it
        var teacher = await _teacherRepository.FirstOrDefaultAsync(
            t => t.UserId == currentUser.Id
        );

        if (teacher == null || mark.TeacherId != teacher.Id)
        {
            // Unless user is Principal/Admin
            if (!await IsInRoleAsync("Principal") && !await IsInRoleAsync("Admin"))
            {
                throw new BusinessException(
                    "UNAUTHORIZED_MARK_EDIT",
                    "You can only edit marks you captured."
                );
            }
        }

        // Update mark...
    }
}

public class PaymentAppService : psmsAppServiceBase
{
    [AbpAuthorize(AppPermissions.Pages_Payments)]
    public async Task<List<PaymentDto>> GetMyPayments()
    {
        // Parents can only see payments they made
        var parent = await _parentRepository.FirstOrDefaultAsync(
            p => p.UserId == AbpSession.UserId
        );

        if (parent == null)
        {
            throw new BusinessException(
                "NOT_A_PARENT",
                "Only parents can access this endpoint."
            );
        }

        var payments = await _paymentRepository.GetAllListAsync(
            p => p.ParentId == parent.Id && !p.IsDeleted
        );

        return ObjectMapper.Map<List<PaymentDto>>(payments);
    }
}
```

**Validation**:
- ✅ Teachers: Own classes/students/marks
- ✅ Parents: Own children's data
- ✅ Students: Own data only
- ✅ Admins/Principals: All tenant data

---

## POPIA Compliance Rules

### PO-001: Consent Required

**Rule**: POPIA consent required before storing/processing student data.

**Implementation**:
```csharp
public class Student : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void ValidatePOPIAConsent()
    {
        // Business Rule: POPIA consent required for active students
        if (IsActive && !POPIAConsentGiven)
        {
            throw new BusinessException(
                "POPIA_CONSENT_REQUIRED",
                "POPIA consent must be given before student can be activated."
            );
        }

        // Business Rule: Consent must not be expired (review annually)
        if (POPIAConsentDate.HasValue)
        {
            var consentAge = (Clock.Now - POPIAConsentDate.Value).Days;
            if (consentAge > 365)
            {
                Logger.Warn($"Student {Id}: POPIA consent is over 1 year old. Consider renewal.");
            }
        }
    }

    public void GivePOPIAConsent()
    {
        POPIAConsentGiven = true;
        POPIAConsentDate = Clock.Now;

        // Create consent record
        var consent = new POPIAConsent
        {
            StudentId = this.Id,
            TenantId = this.TenantId,
            ConsentDate = Clock.Now,
            IsActive = true,
            ConsentVersion = "1.0" // Track policy version
        };

        POPIAConsents.Add(consent);
    }

    public void WithdrawPOPIAConsent()
    {
        POPIAConsentGiven = false;
        IsActive = false; // Must deactivate student

        // Trigger data minimization process
        DomainEvents.Raise(new POPIAConsentWithdrawnEvent
        {
            StudentId = this.Id,
            WithdrawalDate = Clock.Now
        });
    }
}
```

**Validation**:
- ✅ Consent given before activation
- ✅ Consent tracked with date
- ✅ Consent version tracked
- ✅ Withdrawal triggers data minimization
- ✅ Annual consent review reminder

---

### PO-002: Right to Access

**Rule**: Parents must be able to access all data held about their children.

**Implementation**:
```csharp
public class POPIAComplianceAppService : psmsAppServiceBase
{
    [AbpAuthorize]
    public async Task<StudentDataExportDto> ExportStudentData(Guid studentId)
    {
        var parent = await _parentRepository.FirstOrDefaultAsync(
            p => p.UserId == AbpSession.UserId
        );

        // Business Rule: Parent must be linked to student
        var link = await _studentParentRepository.FirstOrDefaultAsync(
            sp => sp.StudentId == studentId && sp.ParentId == parent.Id
        );

        if (link == null)
        {
            throw new BusinessException(
                "UNAUTHORIZED_ACCESS",
                "You can only export data for your own children."
            );
        }

        // Export all student data
        var student = await _studentRepository.GetAsync(studentId);
        var marks = await _markRepository.GetAllListAsync(m => m.StudentId == studentId);
        var attendance = await _attendanceRepository.GetAllListAsync(a => a.StudentId == studentId);
        var reports = await _reportRepository.GetAllListAsync(r => r.StudentId == studentId);
        var fees = await _studentFeeRepository.GetAllListAsync(f => f.StudentId == studentId);
        var payments = await _paymentRepository.GetAllListAsync(p => p.StudentId == studentId);

        return new StudentDataExportDto
        {
            Student = ObjectMapper.Map<StudentDto>(student),
            Marks = ObjectMapper.Map<List<MarkDto>>(marks),
            Attendance = ObjectMapper.Map<List<AttendanceDto>>(attendance),
            Reports = ObjectMapper.Map<List<ReportDto>>(reports),
            Fees = ObjectMapper.Map<List<StudentFeeDto>>(fees),
            Payments = ObjectMapper.Map<List<PaymentDto>>(payments),
            ExportDate = Clock.Now
        };
    }
}
```

**Validation**:
- ✅ Parent can export all child data
- ✅ Data export includes all modules
- ✅ Export audit logged

---

### PO-003: Right to Erasure

**Rule**: Parents can request deletion of student data (with limitations).

**Implementation**:
```csharp
public async Task RequestDataDeletion(Guid studentId, string reason)
{
    var student = await _studentRepository.GetAsync(studentId);

    // Business Rule: Cannot delete if student has financial obligations
    var outstandingFees = await _studentFeeRepository.GetAllListAsync(
        f => f.StudentId == studentId &&
             f.Status != FeeStatus.Paid &&
             f.Status != FeeStatus.Waived
    );

    if (outstandingFees.Any())
    {
        throw new BusinessException(
            "OUTSTANDING_FEES",
            "Cannot delete student data while fees are outstanding. Please settle all fees first."
        );
    }

    // Business Rule: Cannot delete if student is currently enrolled
    if (student.IsActive)
    {
        throw new BusinessException(
            "STUDENT_ACTIVE",
            "Cannot delete data for active student. Please unenroll first."
        );
    }

    // Soft delete (retain for legal/audit purposes)
    student.IsDeleted = true;
    student.DeletionTime = Clock.Now;

    // Anonymize personal data (POPIA compliance)
    student.FirstName = "DELETED";
    student.LastName = "DELETED";
    student.IdNumber = null;
    student.Address = null;
    student.EmergencyContactName = null;
    student.EmergencyContactPhone = null;
    student.MedicalConditions = null;
    student.ProfilePhotoUrl = null;

    // Keep academic records for audit (but anonymized)
    // Marks, attendance, reports remain but linked to anonymized student

    await _studentRepository.UpdateAsync(student);
}
```

**Validation**:
- ✅ No outstanding fees
- ✅ Student not currently enrolled
- ✅ Soft delete + anonymization
- ✅ Retain audit trail
- ✅ Academic records preserved (anonymized)

---

## SA-Specific Rules

### SA-001: Public Holiday Validation

**Rule**: Cannot capture attendance or schedule assessments on SA public holidays.

**Implementation**:
```csharp
public class SouthAfricanPublicHolidayService : DomainService
{
    public bool IsPublicHoliday(DateTime date)
    {
        var publicHoliday = _publicHolidayRepository.FirstOrDefault(
            h => h.Date.Date == date.Date
        );

        return publicHoliday != null;
    }

    public void ValidateNotPublicHoliday(DateTime date, string operation)
    {
        if (IsPublicHoliday(date))
        {
            var holiday = _publicHolidayRepository.First(h => h.Date.Date == date.Date);
            throw new BusinessException(
                "PUBLIC_HOLIDAY",
                $"Cannot {operation} on public holiday: {holiday.Name}."
            );
        }
    }
}

// Usage:
public async Task CaptureAttendance(CaptureAttendanceDto input)
{
    _holidayService.ValidateNotPublicHoliday(input.AttendanceDate, "capture attendance");
    // ...
}
```

**SA Public Holidays** (Fixed + Movable):
- New Year's Day (1 Jan)
- Human Rights Day (21 Mar)
- Good Friday (Movable - Easter)
- Family Day (Movable - Easter Monday)
- Freedom Day (27 Apr)
- Workers' Day (1 May)
- Youth Day (16 Jun)
- National Women's Day (9 Aug)
- Heritage Day (24 Sep)
- Day of Reconciliation (16 Dec)
- Christmas Day (25 Dec)
- Day of Goodwill (26 Dec)

**Validation**:
- ✅ No attendance on holidays
- ✅ No assessments due on holidays
- ✅ No classes scheduled on holidays

---

### SA-002: Language Subject Requirements

**Rule**: SA schools must offer at least 2 languages (Home + Additional).

**Implementation**:
```csharp
public class SubjectValidationManager : DomainService
{
    public async Task ValidateSALanguageRequirements(Grade grade)
    {
        var gradeSubjects = await _gradeSubjectRepository.GetAllListAsync(
            gs => gs.GradeId == grade.Id
        );

        var subjects = gradeSubjects.Select(gs => gs.Subject).ToList();

        // Business Rule: Must have Home Language
        var homeLanguage = subjects.FirstOrDefault(
            s => s.IsLanguage && s.LanguageType == "Home Language"
        );

        if (homeLanguage == null)
        {
            throw new BusinessException(
                "HOME_LANGUAGE_REQUIRED",
                "Grade must have at least one Home Language subject (SA requirement)."
            );
        }

        // Business Rule: Must have First Additional Language (from Grade 1 onwards)
        if (grade.GradeLevel >= SouthAfricanGradeLevel.Grade1)
        {
            var additionalLanguage = subjects.FirstOrDefault(
                s => s.IsLanguage && s.LanguageType == "First Additional Language"
            );

            if (additionalLanguage == null)
            {
                throw new BusinessException(
                    "FIRST_ADDITIONAL_LANGUAGE_REQUIRED",
                    "Grade 1 and above must have First Additional Language (SA requirement)."
                );
            }
        }
    }
}
```

**Validation**:
- ✅ Home Language required (all grades)
- ✅ First Additional Language required (Grade 1+)
- ✅ Can offer 11 SA official languages

---

### SA-003: Matric Requirements (Grade 12)

**Rule**: Grade 12 students must meet NSC (National Senior Certificate) requirements.

**Implementation**:
```csharp
public class MatricRequirementsValidator : DomainService
{
    public async Task<bool> ValidateNSCRequirements(Student student)
    {
        if (student.CurrentGrade?.GradeLevel != SouthAfricanGradeLevel.Grade12)
            return true; // Not applicable

        var enrolledSubjects = await _studentSubjectRepository.GetAllListAsync(
            ss => ss.StudentId == student.Id && ss.IsActive
        );

        // Business Rule: Minimum 7 subjects required
        if (enrolledSubjects.Count < 7)
        {
            throw new BusinessException(
                "INSUFFICIENT_SUBJECTS",
                "Grade 12 students must be enrolled in at least 7 subjects (NSC requirement)."
            );
        }

        var subjects = enrolledSubjects.Select(ss => ss.Subject).ToList();

        // Business Rule: Must include Home Language
        if (!subjects.Any(s => s.IsLanguage && s.LanguageType == "Home Language"))
        {
            throw new BusinessException(
                "HOME_LANGUAGE_REQUIRED",
                "Grade 12 must include Home Language (NSC requirement)."
            );
        }

        // Business Rule: Must include First Additional Language
        if (!subjects.Any(s => s.IsLanguage && s.LanguageType == "First Additional Language"))
        {
            throw new BusinessException(
                "FIRST_ADDITIONAL_LANGUAGE_REQUIRED",
                "Grade 12 must include First Additional Language (NSC requirement)."
            );
        }

        // Business Rule: Must include Mathematics OR Mathematical Literacy
        if (!subjects.Any(s => s.Code == "MATH" || s.Code == "MATHLIT"))
        {
            throw new BusinessException(
                "MATHEMATICS_REQUIRED",
                "Grade 12 must include Mathematics or Mathematical Literacy (NSC requirement)."
            );
        }

        // Business Rule: Must include Life Orientation
        if (!subjects.Any(s => s.Code == "LO"))
        {
            throw new BusinessException(
                "LIFE_ORIENTATION_REQUIRED",
                "Grade 12 must include Life Orientation (NSC requirement)."
            );
        }

        return true;
    }
}
```

**NSC Requirements**:
- ✅ Minimum 7 subjects
- ✅ Home Language (compulsory)
- ✅ First Additional Language (compulsory)
- ✅ Mathematics OR Mathematical Literacy (compulsory)
- ✅ Life Orientation (compulsory)
- ✅ 3 additional subjects (electives)

---

## Data Integrity Rules

### DI-001: Referential Integrity

**Rule**: Foreign key references must be valid and exist.

**Implementation**:
```csharp
// Enforced by EF Core configuration:

modelBuilder.Entity<Student>()
    .HasOne(s => s.Grade)
    .WithMany(g => g.Students)
    .HasForeignKey(s => s.CurrentGradeId)
    .OnDelete(DeleteBehavior.Restrict); // Prevent orphans

modelBuilder.Entity<Mark>()
    .HasOne(m => m.Student)
    .WithMany(s => s.Marks)
    .HasForeignKey(m => m.StudentId)
    .OnDelete(DeleteBehavior.Restrict); // Keep marks if student soft-deleted

// Business validation:
public async Task ValidateReferences()
{
    // Business Rule: Grade must exist
    if (!await _gradeRepository.AnyAsync(g => g.Id == CurrentGradeId))
    {
        throw new BusinessException("GRADE_NOT_FOUND", "Grade does not exist.");
    }

    // Business Rule: Class must exist
    if (!await _classRepository.AnyAsync(c => c.Id == CurrentClassId))
    {
        throw new BusinessException("CLASS_NOT_FOUND", "Class does not exist.");
    }
}
```

**Validation**:
- ✅ EF Core foreign key constraints
- ✅ `Restrict` delete behavior (prevent orphans)
- ✅ Application-level validation
- ✅ Soft delete preserves relationships

---

### DI-002: Unique Constraints

**Rule**: Business keys must be unique per tenant.

**Unique Constraints**:
```csharp
// Student
- (TenantId, AdmissionNumber) - Unique
- (TenantId, IdNumber) - Unique (if SA citizen)

// Teacher
- (TenantId, EmployeeNumber) - Unique

// Payment
- (TenantId, ReceiptNumber) - Unique

// Parent
- (TenantId, IdNumber) - Unique (if provided)

// FeeStructure
- (TenantId, Name, AcademicYearId) - Unique

// Class
- (TenantId, GradeId, Name) - Unique (e.g., "Grade 1A" unique per school)
```

**Implementation**:
```csharp
// EF Core configuration:
modelBuilder.Entity<Student>()
    .HasIndex(s => new { s.TenantId, s.AdmissionNumber })
    .IsUnique();

modelBuilder.Entity<Payment>()
    .HasIndex(p => new { p.TenantId, p.ReceiptNumber })
    .IsUnique();

// Application validation:
public async Task ValidateUniqueness()
{
    var existing = await _studentRepository.FirstOrDefaultAsync(
        s => s.TenantId == TenantId && s.AdmissionNumber == AdmissionNumber
    );

    if (existing != null && existing.Id != this.Id)
    {
        throw new BusinessException("ADMISSION_NUMBER_EXISTS",
            "Admission number already in use.");
    }
}
```

**Validation**:
- ✅ Database unique indexes
- ✅ Application-level checks
- ✅ Consider tenant in uniqueness
- ✅ Handle update vs create scenarios

---

## Business Process Rules

### BP-001: Student Lifecycle

**Student States**:
```
Registered → Active → Inactive → Archived
```

**State Transitions**:
```csharp
public class Student : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public void Activate()
    {
        // Prerequisites
        ValidatePOPIAConsent();
        ValidateParentLinks();

        if (CurrentGradeId == Guid.Empty || CurrentClassId == Guid.Empty)
            throw new BusinessException("GRADE_CLASS_REQUIRED",
                "Grade and class required for activation.");

        IsActive = true;
        DomainEvents.Raise(new StudentEnrolledEvent { StudentId = this.Id });
    }

    public void Deactivate(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessException("REASON_REQUIRED",
                "Reason required for deactivation.");

        IsActive = false;
        // Keep historical data
    }
}
```

---

### BP-002: Academic Year Rollover

**Rule**: Process for transitioning to new academic year.

**Process**:
1. Lock all marks for previous year
2. Generate all final reports
3. Promote students to next grade
4. Create new classes for new academic year
5. Enroll students in new classes
6. Set new academic year as current

**Implementation**:
```csharp
public class AcademicYearRolloverManager : DomainService
{
    public async Task RolloverToNewYear(AcademicYear oldYear, AcademicYear newYear)
    {
        // Step 1: Validate all marks locked
        var unlockedMarks = await _markRepository.GetAllListAsync(
            m => m.TenantId == oldYear.TenantId &&
                 !m.IsLocked &&
                 oldYear.Terms.Select(t => t.Id).Contains(m.TermId)
        );

        if (unlockedMarks.Any())
        {
            throw new BusinessException("MARKS_NOT_LOCKED",
                $"{unlockedMarks.Count} marks are still unlocked. Lock all marks before rollover.");
        }

        // Step 2: Generate final reports
        // ...

        // Step 3: Promote students
        await PromoteStudents(oldYear, newYear);

        // Step 4: Create new classes
        // ...

        // Step 5: Set new year as current
        newYear.SetAsCurrent();
        oldYear.IsCurrent = false;
    }

    private async Task PromoteStudents(AcademicYear oldYear, AcademicYear newYear)
    {
        var students = await _studentRepository.GetAllListAsync(
            s => s.TenantId == oldYear.TenantId && s.IsActive
        );

        foreach (var student in students)
        {
            // Promote to next grade (if not Grade 12)
            if (student.CurrentGrade.GradeLevel < SouthAfricanGradeLevel.Grade12)
            {
                var nextGradeLevel = student.CurrentGrade.GradeLevel + 1;
                var nextGrade = await _gradeRepository.FirstOrDefaultAsync(
                    g => g.TenantId == student.TenantId &&
                         g.GradeLevel == nextGradeLevel
                );

                if (nextGrade != null)
                {
                    student.CurrentGradeId = nextGrade.Id;
                    // Assign to new class (business logic here)
                }
            }
        }
    }
}
```

---

## Summary

**Total Business Rules**: 50+

**Categories**:
- Academic Management: 7 rules
- Enrollment & Registration: 5 rules
- Grading & Assessment: 6 rules
- Attendance: 3 rules
- Reporting: 3 rules
- Financial Management: 5 rules
- Communication: 2 rules
- Multi-Tenancy: 3 rules
- Security & Authorization: 2 rules
- POPIA Compliance: 3 rules
- SA-Specific: 3 rules
- Data Integrity: 2 rules
- Business Processes: 2 rules

**Implementation Checklist**:
- ✅ Domain validation in entity constructors/methods
- ✅ Application service authorization
- ✅ ABP permissions system
- ✅ Multi-tenancy data isolation
- ✅ POPIA compliance tracking
- ✅ SA-specific validations
- ✅ Business exception handling
- ✅ Domain events for cross-aggregate communication
- ✅ Audit logging via ABP
- ✅ Soft delete for data retention

---

**Document Version**: 1.0
**Last Updated**: 2026-01-27
**Status**: ✅ Production Ready
