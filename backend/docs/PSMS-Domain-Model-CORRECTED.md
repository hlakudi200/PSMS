# Private School Management System (PSMS) - Domain Model
## CORRECTED & ALIGNED - South African Context

**ABP Framework | Multi-Tenancy | DDD | POPIA Compliant**

---

## Overview

This document defines the complete domain model for PSMS following ABP Framework patterns, Domain-Driven Design (DDD) principles, and South African educational requirements.

## Multi-Tenancy Architecture

- **Tenant**: Each school is a separate tenant (ABP Tenant entity)
- **Host**: Platform administrator managing multiple schools
- **TenantId Type**: `int?` (nullable int) - ABP Standard
- **UserId Type**: `long` - ABP Standard (User.Id)
- All entities are tenant-scoped except platform-level configurations

---

## Core Domain Entities

### MODULE 1: Academic Management

#### 1.1 School (Tenant)
**Type**: Tenant Entity (ABP Multi-tenancy)
**Description**: Represents a private school in the system

**C# Definition**:
```csharp
public class Tenant : AbpTenant<User>
{
    // ABP properties
    public string Name { get; set; }
    public string TenancyName { get; set; }

    // Extended properties for PSMS
    public string Code { get; set; }
    public Address Address { get; set; }  // Value Object
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
    public string LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime? SubscriptionExpiryDate { get; set; }
}
```

**ABP Features**:
- Inherits from ABP `Tenant`
- Multi-tenancy enabled automatically

---

#### 1.2 Student (Learner)
**Type**: Aggregate Root
**Description**: Represents a learner enrolled in the school with SA-specific fields

**C# Definition**:
```csharp
public class Student : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }  // CRITICAL: int? not Guid!

    // Basic Information
    public string FirstName { get; set; }              // Max 100
    public string LastName { get; set; }               // Max 100
    public string MiddleName { get; set; }             // Max 100
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }

    // SA-Specific Identity
    public SouthAfricanIdNumber IdNumber { get; set; }  // Value Object with validation
    public string PassportNumber { get; set; }          // For non-SA students
    public bool IsSACitizen { get; set; }              // Auto-set from ID

    // School Information
    public string AdmissionNumber { get; set; }         // Unique per tenant
    public DateTime AdmissionDate { get; set; }
    public Guid CurrentGradeId { get; set; }
    public Guid CurrentClassId { get; set; }

    // Contact & Emergency
    public Address Address { get; set; }                // Value Object
    public string ProfilePhotoUrl { get; set; }
    public string EmergencyContactName { get; set; }
    public string EmergencyContactPhone { get; set; }
    public string MedicalConditions { get; set; }

    // POPIA Compliance (SA Data Protection)
    public bool POPIAConsentGiven { get; set; }
    public DateTime? POPIAConsentDate { get; set; }
    public bool AllowPhotography { get; set; }
    public bool AllowNameInPublications { get; set; }

    // Status
    public bool IsActive { get; set; }

    // Concurrency
    public string ConcurrencyStamp { get; set; }

    // Navigation Properties (Collections)
    public virtual ICollection<StudentParent> ParentLinks { get; set; }
    public virtual ICollection<StudentSubject> SubjectEnrollments { get; set; }
    public virtual ICollection<Mark> Marks { get; set; }
    public virtual ICollection<Attendance> Attendances { get; set; }
    public virtual ICollection<Report> Reports { get; set; }
    public virtual ICollection<POPIAConsent> POPIAConsents { get; set; }

    // Domain Methods
    public void SetSAIdNumber(string idNumber)
    {
        IdNumber = new SouthAfricanIdNumber(idNumber);
        DateOfBirth = IdNumber.GetDateOfBirth();
        Gender = IdNumber.GetGender();
        IsSACitizen = IdNumber.IsCitizen();
    }

    public void GivePOPIAConsent()
    {
        POPIAConsentGiven = true;
        POPIAConsentDate = Clock.Now;
    }
}
```

**Indexes**:
```sql
CREATE INDEX IX_Students_TenantId ON Students(TenantId);
CREATE UNIQUE INDEX IX_Students_AdmissionNumber ON Students(TenantId, AdmissionNumber);
CREATE INDEX IX_Students_CurrentGradeId ON Students(CurrentGradeId);
```

---

#### 1.3 Parent (Guardian)
**Type**: Aggregate Root
**Description**: Represents a parent or guardian of a student

**C# Definition**:
```csharp
public class Parent : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }

    // User Link (ABP Identity)
    public long UserId { get; set; }  // CRITICAL: long not Guid!

    // Basic Information
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string IdNumber { get; set; }  // SA ID
    public RelationshipType Relationship { get; set; }
    public Address Address { get; set; }  // Value Object
    public string Occupation { get; set; }
    public string WorkPhone { get; set; }
    public bool IsPrimaryContact { get; set; }
    public string ProfilePhotoUrl { get; set; }

    // Navigation Properties
    public virtual ICollection<StudentParent> StudentLinks { get; set; }
    public virtual ICollection<Payment> Payments { get; set; }

    // Validation
    public void ValidateUserTenant(User user)
    {
        if (user.TenantId != this.TenantId)
            throw new BusinessException("User and Parent must belong to same tenant");
    }
}
```

---

#### 1.4 StudentParent (Junction Table)
**Type**: Entity
**Description**: Links students to their parents/guardians with permissions

**C# Definition**:
```csharp
public class StudentParent : Entity<Guid>
{
    public Guid StudentId { get; set; }
    public Guid ParentId { get; set; }
    public RelationshipType RelationshipType { get; set; }
    public bool IsPrimaryContact { get; set; }
    public bool CanMakePayments { get; set; }
    public bool CanViewAcademicRecords { get; set; }

    // Navigation Properties
    public Student Student { get; set; }
    public Parent Parent { get; set; }

    // Validation
    public void ValidateSameTenant()
    {
        if (Student.TenantId != Parent.TenantId)
            throw new BusinessException("Student and Parent must belong to same school");
    }
}
```

---

#### 1.5 Teacher
**Type**: Aggregate Root
**Description**: Represents a teacher in the school

**C# Definition**:
```csharp
public class Teacher : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }

    // User Link
    public long UserId { get; set; }  // ABP User.Id is long

    // Basic Information
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string IdNumber { get; set; }  // SA ID
    public string EmployeeNumber { get; set; }  // Unique per tenant
    public DateTime DateOfJoining { get; set; }
    public string Qualifications { get; set; }
    public string Specialization { get; set; }
    public Address Address { get; set; }  // Value Object
    public string ProfilePhotoUrl { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public virtual ICollection<TeacherSubject> SubjectAssignments { get; set; }
    public virtual ICollection<TeacherClass> ClassAssignments { get; set; }
    public virtual ICollection<LearningMaterial> LearningMaterials { get; set; }
    public virtual ICollection<OnlineLesson> OnlineLessons { get; set; }
    public virtual ICollection<Assessment> Assessments { get; set; }
}
```

**Indexes**:
```sql
CREATE INDEX IX_Teachers_TenantId ON Teachers(TenantId);
CREATE UNIQUE INDEX IX_Teachers_EmployeeNumber ON Teachers(TenantId, EmployeeNumber);
```

---

#### 1.6 Grade
**Type**: Aggregate Root
**Description**: Represents a grade/year level with SA-specific fields

**C# Definition**:
```csharp
public class Grade : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }

    // Basic Information
    public string Name { get; set; }        // e.g., "Grade 1"
    public string Code { get; set; }        // e.g., "G1"
    public string Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }

    // SA-Specific
    public SouthAfricanGradeLevel GradeLevel { get; set; }  // R, 1-12
    public SouthAfricanSchoolPhase SchoolPhase { get; set; } // Foundation/Intermediate/Senior/FET

    // Navigation Properties
    public virtual ICollection<Class> Classes { get; set; }
    public virtual ICollection<GradeSubject> GradeSubjects { get; set; }
    public virtual ICollection<Student> Students { get; set; }

    // Domain Methods
    public void SetGradeLevel(SouthAfricanGradeLevel level)
    {
        GradeLevel = level;
        SchoolPhase = level switch
        {
            <= SouthAfricanGradeLevel.Grade3 => SouthAfricanSchoolPhase.Foundation,
            <= SouthAfricanGradeLevel.Grade6 => SouthAfricanSchoolPhase.Intermediate,
            <= SouthAfricanGradeLevel.Grade9 => SouthAfricanSchoolPhase.Senior,
            _ => SouthAfricanSchoolPhase.FET
        };
    }
}
```

---

#### 1.7 Class (Section)
**Type**: Aggregate Root
**Description**: Represents a class/section within a grade

**C# Definition**:
```csharp
public class Class : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }

    // Basic Information
    public Guid GradeId { get; set; }
    public string Name { get; set; }              // e.g., "1A"
    public string ClassName { get; set; }         // Full: "Grade 1A"
    public int Capacity { get; set; }
    public Guid? ClassTeacherId { get; set; }     // Form teacher
    public Guid AcademicYearId { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public Grade Grade { get; set; }
    public Teacher ClassTeacher { get; set; }
    public AcademicYear AcademicYear { get; set; }
    public virtual ICollection<Student> Students { get; set; }
    public virtual ICollection<TeacherClass> TeacherAssignments { get; set; }
    public virtual ICollection<Timetable> Timetables { get; set; }
}
```

---

#### 1.8 Subject
**Type**: Aggregate Root
**Description**: Represents an academic subject with SA CAPS curriculum support

**C# Definition**:
```csharp
public class Subject : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }

    // Basic Information
    public string Name { get; set; }         // e.g., "Mathematics"
    public string Code { get; set; }         // e.g., "MATH"
    public string Description { get; set; }
    public bool IsCore { get; set; }         // Core vs Elective
    public bool IsActive { get; set; }

    // SA-Specific (CAPS Curriculum)
    public bool IsLanguage { get; set; }
    public string LanguageType { get; set; }  // "Home Language", "First Additional Language"
    public SouthAfricanSchoolPhase ApplicablePhase { get; set; }
    public bool IsCAPSCompliant { get; set; }

    // Navigation Properties
    public virtual ICollection<GradeSubject> GradeSubjects { get; set; }
    public virtual ICollection<TeacherSubject> TeacherSubjects { get; set; }
    public virtual ICollection<StudentSubject> StudentSubjects { get; set; }
    public virtual ICollection<LearningMaterial> LearningMaterials { get; set; }
    public virtual ICollection<Assessment> Assessments { get; set; }
}
```

---

#### 1.9-1.12 Junction Tables

```csharp
// GradeSubject
public class GradeSubject : Entity<Guid>
{
    public Guid GradeId { get; set; }
    public Guid SubjectId { get; set; }
    public bool IsCompulsory { get; set; }

    public Grade Grade { get; set; }
    public Subject Subject { get; set; }
}

// TeacherSubject
public class TeacherSubject : Entity<Guid>
{
    public Guid TeacherId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid GradeId { get; set; }
    public bool IsPrimary { get; set; }

    public Teacher Teacher { get; set; }
    public Subject Subject { get; set; }
    public Grade Grade { get; set; }
}

// TeacherClass
public class TeacherClass : Entity<Guid>
{
    public Guid TeacherId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public bool IsClassTeacher { get; set; }

    public Teacher Teacher { get; set; }
    public Class Class { get; set; }
    public Subject Subject { get; set; }
}

// StudentSubject
public class StudentSubject : Entity<Guid>
{
    public Guid StudentId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid AcademicYearId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public bool IsActive { get; set; }

    public Student Student { get; set; }
    public Subject Subject { get; set; }
    public AcademicYear AcademicYear { get; set; }
}
```

---

#### 1.13 AcademicYear
**Type**: Aggregate Root
**Description**: Represents an academic year with SA 4-term structure

**C# Definition**:
```csharp
public class AcademicYear : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }

    // Basic Information
    public string Name { get; set; }         // e.g., "2024"
    public int Year { get; set; }
    public DateTime StartDate { get; set; }  // SA: Mid-January
    public DateTime EndDate { get; set; }    // SA: December
    public bool IsCurrent { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public virtual ICollection<Term> Terms { get; set; }
    public virtual ICollection<Class> Classes { get; set; }

    // SA-Specific Validation
    public void ValidateSouthAfricanAcademicYear()
    {
        if (StartDate.Month < 1 || StartDate.Month > 2)
            throw new BusinessException("SA academic year should start in January");

        if (EndDate.Month != 12)
            throw new BusinessException("SA academic year should end in December");
    }

    // Create default SA 4-term structure
    public void CreateDefaultSATerms()
    {
        Terms = new List<Term>
        {
            new Term(GuidGenerator.Create(), Id, "Term 1", SouthAfricanTermNumber.Term1,
                new DateTime(Year, 1, 15), new DateTime(Year, 3, 22)),
            new Term(GuidGenerator.Create(), Id, "Term 2", SouthAfricanTermNumber.Term2,
                new DateTime(Year, 4, 10), new DateTime(Year, 6, 23)),
            new Term(GuidGenerator.Create(), Id, "Term 3", SouthAfricanTermNumber.Term3,
                new DateTime(Year, 7, 17), new DateTime(Year, 9, 22)),
            new Term(GuidGenerator.Create(), Id, "Term 4", SouthAfricanTermNumber.Term4,
                new DateTime(Year, 10, 9), new DateTime(Year, 12, 7))
        };
    }
}
```

---

#### 1.14 Term
**Type**: Entity
**Description**: Represents a school term (SA: 4 terms per year)

**C# Definition**:
```csharp
public class Term : Entity<Guid>
{
    public Guid AcademicYearId { get; set; }
    public string Name { get; set; }         // "Term 1", "Term 2", etc.
    public SouthAfricanTermNumber TermNumber { get; set; }  // 1-4
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }

    // Navigation Properties
    public AcademicYear AcademicYear { get; set; }
    public virtual ICollection<TermEvent> TermEvents { get; set; }
    public virtual ICollection<Mark> Marks { get; set; }
    public virtual ICollection<Report> Reports { get; set; }
}
```

---

#### 1.15 TermEvent
**Type**: Entity
**Description**: Represents holidays, exams, and other calendar events

**C# Definition**:
```csharp
public class TermEvent : Entity<Guid>
{
    public Guid TermId { get; set; }
    public EventType EventType { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsAllDay { get; set; }

    public Term Term { get; set; }
}
```

---

### MODULE 2: Learning Content & Assessment

#### 2.1 LearningMaterial
**Type**: Aggregate Root

**C# Definition**:
```csharp
public class LearningMaterial : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public int? TenantId { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    public Guid SubjectId { get; set; }
    public Guid GradeId { get; set; }
    public Guid? TermId { get; set; }
    public Guid TeacherId { get; set; }
    public MaterialType MaterialType { get; set; }
    public string FileUrl { get; set; }
    public long FileSize { get; set; }
    public string ExternalUrl { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedDate { get; set; }
    public int ViewCount { get; set; }

    // Navigation Properties
    public Subject Subject { get; set; }
    public Grade Grade { get; set; }
    public Term Term { get; set; }
    public Teacher Teacher { get; set; }
}
```

---

#### 2.2 OnlineLesson
**Type**: Aggregate Root

**C# Definition**:
```csharp
public class OnlineLesson : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public int? TenantId { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    public Guid SubjectId { get; set; }
    public Guid GradeId { get; set; }
    public Guid ClassId { get; set; }
    public Guid TeacherId { get; set; }
    public LessonType LessonType { get; set; }
    public DateTime? ScheduledDateTime { get; set; }
    public int Duration { get; set; }  // minutes
    public string MeetingUrl { get; set; }
    public string RecordingUrl { get; set; }
    public bool IsPublished { get; set; }
    public bool AttendanceRequired { get; set; }

    // Navigation Properties
    public Subject Subject { get; set; }
    public Grade Grade { get; set; }
    public Class Class { get; set; }
    public Teacher Teacher { get; set; }
    public virtual ICollection<Attendance> Attendances { get; set; }
}
```

---

#### 2.3 Assessment
**Type**: Aggregate Root

**C# Definition**:
```csharp
public class Assessment : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public int? TenantId { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    public Guid SubjectId { get; set; }
    public Guid GradeId { get; set; }
    public Guid TeacherId { get; set; }
    public Guid? TermId { get; set; }
    public AssessmentType AssessmentType { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal? PassMark { get; set; }
    public DateTime? DueDate { get; set; }
    public int? Duration { get; set; }  // minutes
    public bool IsOnlinePlatform { get; set; }
    public bool IsPublished { get; set; }
    public bool AllowLateSubmission { get; set; }
    public string Instructions { get; set; }

    // Navigation Properties
    public Subject Subject { get; set; }
    public Grade Grade { get; set; }
    public Teacher Teacher { get; set; }
    public Term Term { get; set; }
    public virtual ICollection<AssessmentQuestion> Questions { get; set; }
    public virtual ICollection<Mark> Marks { get; set; }
}
```

---

#### 2.4 AssessmentQuestion
**Type**: Entity

**C# Definition**:
```csharp
public class AssessmentQuestion : Entity<Guid>
{
    public Guid AssessmentId { get; set; }
    public string QuestionText { get; set; }
    public QuestionType QuestionType { get; set; }
    public decimal Points { get; set; }
    public int SortOrder { get; set; }
    public string CorrectAnswer { get; set; }
    public string Options { get; set; }  // JSON for multiple choice

    public Assessment Assessment { get; set; }
    public virtual ICollection<StudentAnswer> StudentAnswers { get; set; }
}
```

---

#### 2.5 StudentAnswer
**Type**: Entity

**C# Definition**:
```csharp
public class StudentAnswer : CreationAuditedEntity<Guid>
{
    public Guid AssessmentQuestionId { get; set; }
    public Guid StudentId { get; set; }
    public string Answer { get; set; }
    public bool? IsCorrect { get; set; }
    public decimal? PointsAwarded { get; set; }
    public DateTime SubmittedAt { get; set; }

    public AssessmentQuestion AssessmentQuestion { get; set; }
    public Student Student { get; set; }
}
```

---

#### 2.6 Mark
**Type**: Aggregate Root
**Description**: Marks/grades with SA 7-level achievement scale

**C# Definition**:
```csharp
public class Mark : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }

    // References
    public Guid StudentId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid? AssessmentId { get; set; }
    public Guid TeacherId { get; set; }
    public Guid TermId { get; set; }

    // Marks
    public decimal MarkValue { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal Percentage { get; set; }

    // SA 7-Level Achievement
    public SouthAfricanAchievementLevel AchievementLevel { get; set; }
    public string AchievementDescription { get; set; }

    // Feedback
    public string Feedback { get; set; }

    // Locking (business rule)
    public bool IsLocked { get; set; }
    public DateTime? LockedAt { get; set; }
    public long? LockedBy { get; set; }  // UserId

    // Concurrency
    [ConcurrencyCheck]
    public string ConcurrencyStamp { get; set; }

    // Navigation Properties
    public Student Student { get; set; }
    public Subject Subject { get; set; }
    public Assessment Assessment { get; set; }
    public Teacher Teacher { get; set; }
    public Term Term { get; set; }

    // Domain Methods
    public void CalculateAchievementLevel(SouthAfricanGradingService gradingService)
    {
        Percentage = (MarkValue / TotalMarks) * 100;
        AchievementLevel = gradingService.CalculateAchievementLevel(Percentage);
        AchievementDescription = gradingService.GetAchievementDescription(AchievementLevel);
    }

    public void Lock(long userId)
    {
        if (IsLocked)
            throw new BusinessException("Mark is already locked");

        IsLocked = true;
        LockedAt = Clock.Now;
        LockedBy = userId;
    }
}
```

**Indexes**:
```sql
CREATE INDEX IX_Marks_StudentId_TermId ON Marks(StudentId, TermId);
CREATE INDEX IX_Marks_SubjectId_TermId ON Marks(SubjectId, TermId);
```

---

#### 2.7 Attendance
**Type**: Entity

**C# Definition**:
```csharp
public class Attendance : CreationAuditedEntity<Guid>, IMultiTenant
{
    public int? TenantId { get; set; }

    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
    public Guid? SubjectId { get; set; }
    public Guid? OnlineLessonId { get; set; }
    public Guid TeacherId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public AttendanceStatus Status { get; set; }
    public string Remarks { get; set; }

    // Navigation Properties
    public Student Student { get; set; }
    public Class Class { get; set; }
    public Subject Subject { get; set; }
    public OnlineLesson OnlineLesson { get; set; }
    public Teacher Teacher { get; set; }
}
```

---

#### 2.8 Report
**Type**: Aggregate Root
**Description**: Academic reports with SA report card requirements

**C# Definition**:
```csharp
public class Report : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }

    // References
    public Guid StudentId { get; set; }
    public Guid TermId { get; set; }
    public Guid AcademicYearId { get; set; }

    // Report Information
    public string ReportCardNumber { get; set; }  // Unique identifier
    public ReportType ReportType { get; set; }
    public DateTime GeneratedDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public bool IsPublished { get; set; }
    public string ReportFileUrl { get; set; }  // PDF

    // Overall Performance
    public decimal OverallAverage { get; set; }
    public SouthAfricanAchievementLevel OverallAchievementLevel { get; set; }
    public int? ClassPosition { get; set; }
    public int? GradePosition { get; set; }

    // SA Report Card Requirements
    public int DaysInTerm { get; set; }
    public int DaysAttended { get; set; }
    public int DaysAbsent { get; set; }
    public decimal AttendancePercentage => (decimal)DaysAttended / DaysInTerm * 100;

    // Conduct & Behaviour (SA requirement)
    public string ConductRating { get; set; }  // Excellent/Good/Satisfactory/Needs Improvement
    public string DiligenceRating { get; set; }
    public string BehaviourComments { get; set; }

    // Progress
    public bool PromotedToNextGrade { get; set; }
    public bool RequiresSupport { get; set; }
    public string InterventionPlan { get; set; }

    // Comments
    public string TeacherComments { get; set; }
    public string PrincipalComments { get; set; }

    // Signatures (SA requirement)
    public bool TeacherSigned { get; set; }
    public bool PrincipalSigned { get; set; }
    public bool ParentSigned { get; set; }
    public DateTime? ParentSignedDate { get; set; }

    // Navigation Properties
    public Student Student { get; set; }
    public Term Term { get; set; }
    public AcademicYear AcademicYear { get; set; }
    public virtual ICollection<ReportSubject> ReportSubjects { get; set; }
}
```

---

#### 2.9 ReportSubject
**Type**: Entity
**Description**: Subject-specific details in a report with SA grading

**C# Definition**:
```csharp
public class ReportSubject : Entity<Guid>
{
    public Guid ReportId { get; set; }
    public Guid SubjectId { get; set; }

    // Marks (SA: typically tasks + exam)
    public decimal Task1Mark { get; set; }
    public decimal Task2Mark { get; set; }
    public decimal Task3Mark { get; set; }
    public decimal ExaminationMark { get; set; }
    public decimal FinalMark { get; set; }

    // SA 7-Level Achievement
    public SouthAfricanAchievementLevel AchievementLevel { get; set; }

    // Class Performance
    public decimal ClassAverage { get; set; }
    public int? SubjectPosition { get; set; }

    // Comments
    public string TeacherComments { get; set; }

    // Navigation Properties
    public Report Report { get; set; }
    public Subject Subject { get; set; }

    // Domain Method: Calculate final mark (SA weighting: 75% tasks, 25% exam)
    public void CalculateFinalMark()
    {
        var tasksAverage = (Task1Mark + Task2Mark + Task3Mark) / 3;
        FinalMark = (tasksAverage * 0.75m) + (ExaminationMark * 0.25m);
    }
}
```

---

### MODULE 3: Financial Management (SA Context)

#### 3.1 FeeStructure
**Type**: Aggregate Root
**Description**: Fee structures with SA-specific fee types and VAT

**C# Definition**:
```csharp
public class FeeStructure : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }

    // Basic Information
    public string Name { get; set; }
    public string Description { get; set; }
    public SouthAfricanFeeType FeeType { get; set; }  // Tuition/Transport/AfterCare etc
    public Guid? GradeId { get; set; }  // Grade-specific or null for all
    public Guid AcademicYearId { get; set; }

    // Pricing
    public Money Amount { get; set; }  // Value Object (Amount + ZAR)
    public PaymentPlan PaymentFrequency { get; set; }
    public int? DueDay { get; set; }  // Day of month for recurring

    // SA VAT Handling
    public bool IsVATApplicable { get; set; }
    public decimal VATPercentage { get; set; } = 15m;  // SA VAT is 15%
    public Money AmountExcludingVAT => IsVATApplicable
        ? new Money(Amount.Amount / 1.15m, "ZAR")
        : Amount;

    // Status
    public bool IsActive { get; set; }
    public bool AllowsBursaryApplication { get; set; }

    // Navigation Properties
    public Grade Grade { get; set; }
    public AcademicYear AcademicYear { get; set; }
    public virtual ICollection<StudentFee> StudentFees { get; set; }
}
```

---

#### 3.2 StudentFee
**Type**: Entity

**C# Definition**:
```csharp
public class StudentFee : CreationAuditedEntity<Guid>
{
    public Guid StudentId { get; set; }
    public Guid FeeStructureId { get; set; }

    public Money Amount { get; set; }  // Value Object
    public Money? DiscountAmount { get; set; }  // Value Object
    public string DiscountReason { get; set; }
    public DateTime DueDate { get; set; }
    public FeeStatus Status { get; set; }

    // Concurrency
    [ConcurrencyCheck]
    public string ConcurrencyStamp { get; set; }

    // Navigation Properties
    public Student Student { get; set; }
    public FeeStructure FeeStructure { get; set; }
    public virtual ICollection<PaymentAllocation> PaymentAllocations { get; set; }
}
```

---

#### 3.3 Payment
**Type**: Aggregate Root
**Description**: Payment transactions with SA payment methods

**C# Definition**:
```csharp
public class Payment : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }

    // References
    public Guid StudentId { get; set; }
    public Guid ParentId { get; set; }

    // Payment Details
    public Money Amount { get; set; }  // Value Object (Amount + ZAR)
    public DateTime PaymentDate { get; set; }
    public SouthAfricanPaymentMethod PaymentMethod { get; set; }  // EFT/Capitec/Ozow etc
    public string PaymentReference { get; set; }
    public string PaymentGatewayTransactionId { get; set; }
    public PaymentStatus Status { get; set; }

    // SA-Specific
    public string BankReference { get; set; }  // For EFT
    public string BankName { get; set; }  // FNB/Standard Bank/ABSA/Nedbank/Capitec
    public bool IsRecurring { get; set; }  // Debit order
    public int? DebitDay { get; set; }  // Day of month for debit orders

    // Receipt
    public string ReceiptNumber { get; set; }  // Unique
    public string ReceiptFileUrl { get; set; }
    public string Notes { get; set; }

    // Concurrency
    [ConcurrencyCheck]
    public string ConcurrencyStamp { get; set; }

    // Navigation Properties
    public Student Student { get; set; }
    public Parent Parent { get; set; }
    public virtual ICollection<PaymentAllocation> PaymentAllocations { get; set; }
}
```

**Indexes**:
```sql
CREATE UNIQUE INDEX IX_Payments_ReceiptNumber ON Payments(TenantId, ReceiptNumber);
CREATE INDEX IX_Payments_StudentId ON Payments(StudentId);
```

---

#### 3.4 PaymentAllocation
**Type**: Entity

**C# Definition**:
```csharp
public class PaymentAllocation : Entity<Guid>
{
    public Guid PaymentId { get; set; }
    public Guid StudentFeeId { get; set; }
    public Money AllocatedAmount { get; set; }  // Value Object

    public Payment Payment { get; set; }
    public StudentFee StudentFee { get; set; }
}
```

---

### MODULE 4: Communication

#### 4.1 Announcement
**Type**: Aggregate Root

**C# Definition**:
```csharp
public class Announcement : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public int? TenantId { get; set; }

    public string Title { get; set; }
    public string Content { get; set; }
    public AnnouncementType AnnouncementType { get; set; }
    public TargetAudience TargetAudience { get; set; }
    public Guid? GradeId { get; set; }
    public Guid? ClassId { get; set; }
    public DateTime PublishDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsPublished { get; set; }
    public bool IsPinned { get; set; }
    public long CreatedByUserId { get; set; }
    public string AttachmentUrl { get; set; }

    // Navigation Properties
    public Grade Grade { get; set; }
    public Class Class { get; set; }
    public virtual ICollection<AnnouncementRead> AnnouncementReads { get; set; }
}
```

---

#### 4.2 AnnouncementRead
**Type**: Entity

**C# Definition**:
```csharp
public class AnnouncementRead : Entity<Guid>
{
    public Guid AnnouncementId { get; set; }
    public long UserId { get; set; }  // ABP User.Id
    public DateTime ReadAt { get; set; }

    public Announcement Announcement { get; set; }
}
```

---

#### 4.3 Message
**Type**: Aggregate Root

**C# Definition**:
```csharp
public class Message : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public int? TenantId { get; set; }

    public long SenderId { get; set; }  // ABP User.Id
    public long RecipientId { get; set; }  // ABP User.Id
    public string Subject { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public Guid? ParentMessageId { get; set; }  // Threading
    public string AttachmentUrl { get; set; }
}
```

---

#### 4.4 Notification
**Type**: Entity

**C# Definition**:
```csharp
public class Notification : CreationAuditedEntity<Guid>, IMultiTenant
{
    public int? TenantId { get; set; }

    public long UserId { get; set; }  // ABP User.Id
    public NotificationType NotificationType { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ActionUrl { get; set; }  // Deep link
}
```

---

#### 4.5 Document
**Type**: Aggregate Root

**C# Definition**:
```csharp
public class Document : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public int? TenantId { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    public DocumentType DocumentType { get; set; }
    public string FileUrl { get; set; }
    public long FileSize { get; set; }
    public bool IsPublic { get; set; }
    public TargetAudience TargetAudience { get; set; }
    public DateTime PublishedDate { get; set; }
}
```

---

### MODULE 5: Timetable

#### 5.1 Timetable
**Type**: Aggregate Root

**C# Definition**:
```csharp
public class Timetable : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public int? TenantId { get; set; }

    public Guid ClassId { get; set; }
    public Guid AcademicYearId { get; set; }
    public Guid? TermId { get; set; }
    public DateTime EffectiveDate { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public Class Class { get; set; }
    public AcademicYear AcademicYear { get; set; }
    public Term Term { get; set; }
    public virtual ICollection<TimetableSlot> TimetableSlots { get; set; }
}
```

---

#### 5.2 TimetableSlot
**Type**: Entity

**C# Definition**:
```csharp
public class TimetableSlot : Entity<Guid>
{
    public Guid TimetableId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public int PeriodNumber { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public Guid SubjectId { get; set; }
    public Guid TeacherId { get; set; }
    public string Room { get; set; }

    // Navigation Properties
    public Timetable Timetable { get; set; }
    public Subject Subject { get; set; }
    public Teacher Teacher { get; set; }
}
```

---

### MODULE 6: SA-Specific Entities

#### 6.1 SchoolTransport
**Type**: Aggregate Root
**Description**: School transport routes (common in SA schools)

**C# Definition**:
```csharp
public class SchoolTransport : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }

    // Route Information
    public string RouteName { get; set; }
    public string Area { get; set; }
    public string Description { get; set; }

    // Pricing
    public Money MonthlyFee { get; set; }  // Value Object

    // Schedule
    public TimeSpan PickupTime { get; set; }
    public TimeSpan DropoffTime { get; set; }

    // Vehicle Details
    public string VehicleRegistration { get; set; }
    public string DriverName { get; set; }
    public string DriverContact { get; set; }
    public int Capacity { get; set; }

    // Status
    public bool IsActive { get; set; }

    // Navigation Properties
    public virtual ICollection<StudentTransport> StudentEnrollments { get; set; }
}
```

---

#### 6.2 AfterCare
**Type**: Aggregate Root
**Description**: Aftercare programs (very common in SA schools)

**C# Definition**:
```csharp
public class AfterCare : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }

    // Program Information
    public string Name { get; set; }
    public string Description { get; set; }

    // Pricing
    public Money MonthlyFee { get; set; }  // Value Object
    public Money DailyDropInFee { get; set; }  // Value Object

    // Schedule
    public TimeSpan StartTime { get; set; }  // e.g., 14:00
    public TimeSpan EndTime { get; set; }    // e.g., 18:00

    // Features
    public bool IncludesHomeworkSupervision { get; set; }
    public bool IncludesSnacks { get; set; }
    public int Capacity { get; set; }

    // Status
    public bool IsActive { get; set; }

    // Navigation Properties
    public virtual ICollection<StudentAfterCare> StudentEnrollments { get; set; }
}
```

---

#### 6.3 ExtramuralActivity
**Type**: Aggregate Root
**Description**: Sport, cultural, and academic activities

**C# Definition**:
```csharp
public class ExtramuralActivity : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }

    // Activity Information
    public string Name { get; set; }
    public string Description { get; set; }
    public ExtramuralType ActivityType { get; set; }  // Sport/Cultural/Academic

    // Pricing
    public Money TermFee { get; set; }  // Value Object

    // Schedule
    public string Coach { get; set; }
    public List<DayOfWeek> PracticeDays { get; set; }  // JSON
    public TimeSpan PracticeTime { get; set; }
    public string Venue { get; set; }

    // Status
    public bool IsActive { get; set; }
    public int MaxParticipants { get; set; }

    // Navigation Properties
    public virtual ICollection<StudentExtramural> StudentEnrollments { get; set; }
}
```

---

#### 6.4 Junction Tables (SA Entities)

```csharp
// StudentTransport
public class StudentTransport : Entity<Guid>
{
    public Guid StudentId { get; set; }
    public Guid SchoolTransportId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public bool IsActive { get; set; }

    public Student Student { get; set; }
    public SchoolTransport SchoolTransport { get; set; }
}

// StudentAfterCare
public class StudentAfterCare : Entity<Guid>
{
    public Guid StudentId { get; set; }
    public Guid AfterCareId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public bool IsActive { get; set; }

    public Student Student { get; set; }
    public AfterCare AfterCare { get; set; }
}

// StudentExtramural
public class StudentExtramural : Entity<Guid>
{
    public Guid StudentId { get; set; }
    public Guid ExtramuralActivityId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public bool IsActive { get; set; }

    public Student Student { get; set; }
    public ExtramuralActivity ExtramuralActivity { get; set; }
}
```

---

#### 6.5 POPIAConsent
**Type**: Entity
**Description**: POPIA (SA Data Protection) consent tracking

**C# Definition**:
```csharp
public class POPIAConsent : Entity<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }

    // References
    public Guid StudentId { get; set; }
    public long UserId { get; set; }  // Parent/Guardian User.Id

    // Consent Details
    public DateTime ConsentDate { get; set; }
    public bool MarketingConsent { get; set; }
    public bool DataSharingConsent { get; set; }
    public bool PhotographyConsent { get; set; }
    public string ConsentVersion { get; set; }  // Track policy version
    public bool IsActive { get; set; }

    // Navigation Properties
    public Student Student { get; set; }

    // Domain Methods
    public void WithdrawConsent()
    {
        IsActive = false;
        // Trigger data anonymization/retention process
    }
}
```

---

### MODULE 7: Host-Level Entities (No TenantId)

#### 7.1 SouthAfricanPublicHoliday
**Type**: Entity
**Description**: SA public holidays (shared across all schools)

**C# Definition**:
```csharp
public class SouthAfricanPublicHoliday : Entity<Guid>
{
    // NO TenantId - This is host-level data

    public string Name { get; set; }
    public DateTime Date { get; set; }
    public int Year { get; set; }
    public bool IsNationalHoliday { get; set; }
    public bool IsMovable { get; set; }  // e.g., Good Friday, Easter
    public string Description { get; set; }
}
```

---

## Value Objects (DDD Pattern)

### Address
```csharp
public class Address : ValueObject
{
    public string StreetAddress { get; private set; }
    public string Suburb { get; private set; }
    public string City { get; private set; }
    public string Province { get; private set; }  // SA: 9 provinces
    public string PostalCode { get; private set; }
    public string Country { get; private set; } = "South Africa";

    private Address() { }

    public Address(string street, string suburb, string city, string province, string postalCode)
    {
        StreetAddress = street;
        Suburb = suburb;
        City = city;
        Province = province;
        PostalCode = postalCode;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return StreetAddress;
        yield return Suburb;
        yield return City;
        yield return Province;
        yield return PostalCode;
        yield return Country;
    }
}
```

### Money
```csharp
public class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    private Money() { }

    public Money(decimal amount, string currency = "ZAR")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative");

        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }

    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");

        return new Money(left.Amount + right.Amount, left.Currency);
    }
}
```

### SouthAfricanIdNumber
```csharp
public class SouthAfricanIdNumber : ValueObject
{
    public string Value { get; private set; }

    private SouthAfricanIdNumber() { }

    public SouthAfricanIdNumber(string idNumber)
    {
        if (!IsValid(idNumber))
            throw new BusinessException("Invalid South African ID number");

        Value = idNumber;
    }

    private bool IsValid(string idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length != 13)
            return false;

        if (!idNumber.All(char.IsDigit))
            return false;

        // Validate date portion
        var year = int.Parse(idNumber.Substring(0, 2));
        var month = int.Parse(idNumber.Substring(2, 2));
        var day = int.Parse(idNumber.Substring(4, 2));

        if (month < 1 || month > 12 || day < 1 || day > 31)
            return false;

        // Luhn algorithm check
        return ValidateLuhnChecksum(idNumber);
    }

    private bool ValidateLuhnChecksum(string idNumber)
    {
        int sum = 0;
        for (int i = 0; i < 13; i++)
        {
            int digit = int.Parse(idNumber[i].ToString());
            if (i % 2 == 0)
                sum += digit;
            else
            {
                int doubled = digit * 2;
                sum += doubled > 9 ? doubled - 9 : doubled;
            }
        }
        return sum % 10 == 0;
    }

    public DateTime GetDateOfBirth()
    {
        int year = int.Parse(Value.Substring(0, 2));
        int month = int.Parse(Value.Substring(2, 2));
        int day = int.Parse(Value.Substring(4, 2));

        int fullYear = year + (year >= 0 && year <= DateTime.Now.Year % 100 ? 2000 : 1900);
        return new DateTime(fullYear, month, day);
    }

    public Gender GetGender()
    {
        int genderCode = int.Parse(Value.Substring(6, 4));
        return genderCode < 5000 ? Gender.Female : Gender.Male;
    }

    public bool IsCitizen()
    {
        return Value[10] == '0';
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
```

---

## 9. Admissions Module

#### 9.1 Application
**Type**: Aggregate Root
**Description**: Represents an admission application for prospective students with SA-specific requirements

**C# Definition**:
```csharp
public class Application : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }

    // Application Information
    public string ApplicationNumber { get; set; }  // Format: APP-{TenantId}-{Year}-{Sequence}

    // Prospective Student Details
    public string ProspectiveStudentFirstName { get; set; }
    public string ProspectiveStudentLastName { get; set; }
    public string ProspectiveStudentMiddleName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }

    // SA-Specific Identity
    public string IdNumber { get; set; }  // SA ID with validation
    public string PassportNumber { get; set; }  // For non-SA students
    public bool IsSACitizen { get; set; }

    // Application Details
    public Guid AppliedGradeId { get; set; }
    public string PreviousSchool { get; set; }
    public DateTime ApplicationDate { get; set; }
    public DateTime? SubmissionDate { get; set; }

    // Review Information
    public ApplicationStatus Status { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public long? ReviewedByUserId { get; set; }  // Admin User.Id

    // Decision
    public AdmissionDecision Decision { get; set; }
    public string DecisionReason { get; set; }
    public DateTime? DecisionDate { get; set; }
    public DateTime? ExpiryDate { get; set; }  // Offer expiry

    // Conversion
    public Guid? CreatedStudentId { get; set; }  // Links to Student if accepted & enrolled

    // Creator
    public string CreatorUserId { get; set; }  // Prospective parent email/contact

    // Concurrency
    public string ConcurrencyStamp { get; set; }

    // Navigation Properties
    public Grade AppliedGrade { get; set; }
    public Student CreatedStudent { get; set; }
    public virtual ICollection<ApplicantParent> ApplicantParents { get; set; }
    public virtual ICollection<ApplicationDocument> ApplicationDocuments { get; set; }
    public virtual ICollection<ApplicationFee> ApplicationFees { get; set; }
    public virtual ICollection<AdmissionInterview> AdmissionInterviews { get; set; }
    public virtual ICollection<AdmissionAssessment> AdmissionAssessments { get; set; }
    public virtual ICollection<Waitlist> WaitlistEntries { get; set; }

    // Domain Methods
    public void Submit()
    {
        if (Status != ApplicationStatus.Draft)
            throw new BusinessException("Only draft applications can be submitted");

        Status = ApplicationStatus.Submitted;
        SubmissionDate = Clock.Now;
    }

    public void Approve(long userId, string reason = null)
    {
        if (Status == ApplicationStatus.Enrolled)
            throw new BusinessException("Application is already enrolled");

        Status = ApplicationStatus.Approved;
        Decision = AdmissionDecision.Accepted;
        DecisionReason = reason;
        DecisionDate = Clock.Now;
        ReviewedByUserId = userId;
        ReviewedDate = Clock.Now;
        ExpiryDate = Clock.Now.AddDays(14);  // 14-day acceptance window
    }

    public void Reject(long userId, string reason)
    {
        Check.NotNullOrWhiteSpace(reason, nameof(reason));

        Status = ApplicationStatus.Rejected;
        Decision = AdmissionDecision.Rejected;
        DecisionReason = reason;
        DecisionDate = Clock.Now;
        ReviewedByUserId = userId;
        ReviewedDate = Clock.Now;
    }

    public void PlaceOnWaitlist(long userId, string reason = null)
    {
        Status = ApplicationStatus.Waitlisted;
        Decision = AdmissionDecision.Waitlisted;
        DecisionReason = reason;
        DecisionDate = Clock.Now;
        ReviewedByUserId = userId;
        ReviewedDate = Clock.Now;
    }

    public void ConvertToStudent(Guid studentId)
    {
        if (Decision != AdmissionDecision.Accepted)
            throw new BusinessException("Only accepted applications can be converted to students");

        CreatedStudentId = studentId;
        Status = ApplicationStatus.Enrolled;
    }

    public bool HasRequiredDocuments()
    {
        // Must have at least birth certificate or ID, and parent ID
        var hasStudentIdentity = ApplicationDocuments.Any(d =>
            d.Category == DocumentCategory.BirthCertificate ||
            d.Category == DocumentCategory.IDDocument);

        var hasParentId = ApplicationDocuments.Any(d =>
            d.Category == DocumentCategory.ParentID);

        return hasStudentIdentity && hasParentId;
    }
}
```

**Indexes**:
```sql
CREATE INDEX IX_Applications_TenantId ON Applications(TenantId);
CREATE UNIQUE INDEX IX_Applications_ApplicationNumber ON Applications(TenantId, ApplicationNumber);
CREATE INDEX IX_Applications_Status ON Applications(Status);
CREATE INDEX IX_Applications_AppliedGradeId ON Applications(AppliedGradeId);
CREATE INDEX IX_Applications_ApplicationDate ON Applications(ApplicationDate);
CREATE INDEX IX_Applications_DecisionDate ON Applications(DecisionDate);
```

---

#### 9.2 ApplicantParent
**Type**: Entity
**Description**: Represents parent/guardian information on an application

**C# Definition**:
```csharp
public class ApplicantParent : Entity<Guid>
{
    public Guid ApplicationId { get; set; }

    // Personal Information
    public RelationshipType Relationship { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string IdNumber { get; set; }  // SA ID
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string AlternatePhone { get; set; }

    // Address Information
    public Address PhysicalAddress { get; set; }  // Value Object
    public Address PostalAddress { get; set; }  // Value Object
    public bool SameAsPhysicalAddress { get; set; }

    // Employment Information
    public string Occupation { get; set; }
    public string Employer { get; set; }
    public string WorkPhone { get; set; }

    // Contact Preferences
    public bool IsPrimaryContact { get; set; }
    public bool IsFinanciallyResponsible { get; set; }

    // Navigation Properties
    public Application Application { get; set; }

    // Validation
    public void ValidateSAIdNumber()
    {
        if (!string.IsNullOrWhiteSpace(IdNumber) && IdNumber.Length == 13)
        {
            var saId = new SouthAfricanIdNumber(IdNumber);
            // Will throw if invalid
        }
    }
}
```

---

#### 9.3 ApplicationDocument
**Type**: Entity
**Description**: Documents uploaded with an application

**C# Definition**:
```csharp
public class ApplicationDocument : CreationAuditedEntity<Guid>
{
    public Guid ApplicationId { get; set; }

    // Document Information
    public DocumentCategory Category { get; set; }
    public string DocumentName { get; set; }
    public string FileName { get; set; }
    public string FileUrl { get; set; }
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; }
    public DateTime UploadedDate { get; set; }

    // Requirements
    public bool IsRequired { get; set; }
    public bool IsVerified { get; set; }
    public long? VerifiedByUserId { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public string VerificationNotes { get; set; }

    // Navigation Properties
    public Application Application { get; set; }

    // Domain Methods
    public void Verify(long userId, string notes = null)
    {
        IsVerified = true;
        VerifiedByUserId = userId;
        VerifiedDate = Clock.Now;
        VerificationNotes = notes;
    }

    public void Reject(long userId, string notes)
    {
        Check.NotNullOrWhiteSpace(notes, nameof(notes));

        IsVerified = false;
        VerifiedByUserId = userId;
        VerifiedDate = Clock.Now;
        VerificationNotes = notes;
    }
}
```

---

#### 9.4 ApplicationFee
**Type**: Entity
**Description**: Application/admission fee payment tracking

**C# Definition**:
```csharp
public class ApplicationFee : CreationAuditedEntity<Guid>
{
    public Guid ApplicationId { get; set; }

    // Fee Details
    public Money Amount { get; set; }  // Value Object (Amount + ZAR)
    public PaymentStatus Status { get; set; }

    // Payment Information
    public SouthAfricanPaymentMethod PaymentMethod { get; set; }
    public string PaymentReference { get; set; }
    public string PaymentGatewayTransactionId { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string ReceiptNumber { get; set; }

    // Refund Information
    public bool IsRefundable { get; set; }  // Usually NO for application fees
    public bool IsRefunded { get; set; }
    public DateTime? RefundDate { get; set; }
    public Money RefundAmount { get; set; }  // Value Object

    // Navigation Properties
    public Application Application { get; set; }

    // Domain Methods
    public void MarkAsPaid(string receiptNumber, string reference)
    {
        Check.NotNullOrWhiteSpace(receiptNumber, nameof(receiptNumber));

        Status = PaymentStatus.Completed;
        PaymentDate = Clock.Now;
        ReceiptNumber = receiptNumber;
        PaymentReference = reference;
    }

    public void ProcessRefund(decimal refundAmount)
    {
        if (!IsRefundable)
            throw new BusinessException("This application fee is not refundable");

        if (Status != PaymentStatus.Completed)
            throw new BusinessException("Only completed payments can be refunded");

        IsRefunded = true;
        RefundDate = Clock.Now;
        RefundAmount = new Money(refundAmount, "ZAR");
        Status = PaymentStatus.Refunded;
    }
}
```

---

#### 9.5 AdmissionInterview
**Type**: Entity
**Description**: Interview scheduling and tracking for admission process

**C# Definition**:
```csharp
public class AdmissionInterview : CreationAuditedEntity<Guid>
{
    public Guid ApplicationId { get; set; }

    // Scheduling
    public DateTime ScheduledDate { get; set; }
    public TimeSpan ScheduledTime { get; set; }
    public string Location { get; set; }  // e.g., "Principal's Office" or "Online"
    public string MeetingLink { get; set; }  // For online interviews

    // Interviewer
    public long InterviewerUserId { get; set; }  // Principal/Admin User.Id
    public string InterviewerName { get; set; }

    // Status
    public InterviewStatus Status { get; set; }

    // Results
    public string Notes { get; set; }
    public int? Rating { get; set; }  // 1-5 scale
    public bool? Recommended { get; set; }
    public DateTime? CompletedDate { get; set; }

    // Navigation Properties
    public Application Application { get; set; }

    // Domain Methods
    public void Schedule(DateTime date, TimeSpan time, long interviewerId, string interviewerName, string location)
    {
        ScheduledDate = date;
        ScheduledTime = time;
        InterviewerUserId = interviewerId;
        InterviewerName = interviewerName;
        Location = location;
        Status = InterviewStatus.Scheduled;
    }

    public void Reschedule(DateTime newDate, TimeSpan newTime)
    {
        ScheduledDate = newDate;
        ScheduledTime = newTime;
        Status = InterviewStatus.Rescheduled;
    }

    public void Complete(int rating, bool recommended, string notes)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5");

        Status = InterviewStatus.Completed;
        CompletedDate = Clock.Now;
        Rating = rating;
        Recommended = recommended;
        Notes = notes;
    }

    public void MarkAsNoShow()
    {
        Status = InterviewStatus.NoShow;
        CompletedDate = Clock.Now;
    }

    public void Cancel()
    {
        Status = InterviewStatus.Cancelled;
    }
}
```

---

#### 9.6 AdmissionAssessment
**Type**: Entity
**Description**: Placement test/assessment for prospective students

**C# Definition**:
```csharp
public class AdmissionAssessment : CreationAuditedEntity<Guid>
{
    public Guid ApplicationId { get; set; }

    // Assessment Details
    public AssessmentType Type { get; set; }  // Placement/Aptitude/Language
    public DateTime ScheduledDate { get; set; }
    public Guid AssessedGradeId { get; set; }
    public string Subjects { get; set; }  // JSON array: ["Math", "English", "Afrikaans"]

    // Scoring
    public decimal? TotalScore { get; set; }
    public decimal? MaxScore { get; set; }
    public decimal? Percentage { get; set; }
    public bool? Passed { get; set; }

    // Feedback
    public string Feedback { get; set; }
    public string Recommendations { get; set; }

    // Assessor
    public long? AssessorUserId { get; set; }  // Teacher/Admin User.Id
    public DateTime? CompletedDate { get; set; }

    // Status
    public bool IsCompleted { get; set; }

    // Navigation Properties
    public Application Application { get; set; }
    public Grade AssessedGrade { get; set; }

    // Domain Methods
    public void Schedule(DateTime date, Guid gradeId, List<string> subjects)
    {
        ScheduledDate = date;
        AssessedGradeId = gradeId;
        Subjects = JsonConvert.SerializeObject(subjects);
    }

    public void RecordResults(decimal totalScore, decimal maxScore, long assessorId, string feedback)
    {
        if (totalScore < 0 || maxScore <= 0)
            throw new ArgumentException("Invalid score values");

        if (totalScore > maxScore)
            throw new ArgumentException("Total score cannot exceed max score");

        TotalScore = totalScore;
        MaxScore = maxScore;
        Percentage = (totalScore / maxScore) * 100;
        Passed = Percentage >= 50;  // Configurable pass mark
        Feedback = feedback;
        AssessorUserId = assessorId;
        CompletedDate = Clock.Now;
        IsCompleted = true;
    }
}
```

---

#### 9.7 Waitlist
**Type**: Aggregate Root
**Description**: Manages waitlist for grades at capacity

**C# Definition**:
```csharp
public class Waitlist : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // Multi-tenancy
    public int? TenantId { get; set; }

    // References
    public Guid ApplicationId { get; set; }
    public Guid GradeId { get; set; }

    // Waitlist Information
    public int Position { get; set; }  // Queue position (1 = first in line)
    public DateTime AddedDate { get; set; }
    public WaitlistStatus Status { get; set; }

    // Offer Management
    public DateTime? NotifiedDate { get; set; }
    public DateTime? OfferExpiryDate { get; set; }
    public DateTime? ResponseDate { get; set; }

    // Notes
    public string Notes { get; set; }

    // Navigation Properties
    public Application Application { get; set; }
    public Grade Grade { get; set; }

    // Domain Methods
    public void Add(int position)
    {
        Position = position;
        AddedDate = Clock.Now;
        Status = WaitlistStatus.Active;
    }

    public void OfferPosition(int expiryDays = 7)
    {
        if (Status != WaitlistStatus.Active)
            throw new BusinessException("Can only offer position to active waitlist entries");

        Status = WaitlistStatus.Offered;
        NotifiedDate = Clock.Now;
        OfferExpiryDate = Clock.Now.AddDays(expiryDays);
    }

    public void AcceptOffer()
    {
        if (Status != WaitlistStatus.Offered)
            throw new BusinessException("Can only accept offered positions");

        Status = WaitlistStatus.Accepted;
        ResponseDate = Clock.Now;
    }

    public void DeclineOffer()
    {
        if (Status != WaitlistStatus.Offered)
            throw new BusinessException("Can only decline offered positions");

        Status = WaitlistStatus.Declined;
        ResponseDate = Clock.Now;
    }

    public void ExpireOffer()
    {
        if (Status != WaitlistStatus.Offered)
            throw new BusinessException("Can only expire offered positions");

        Status = WaitlistStatus.Expired;
    }

    public void Withdraw(string reason)
    {
        Status = WaitlistStatus.Withdrawn;
        Notes = reason;
    }

    public void UpdatePosition(int newPosition)
    {
        if (Status != WaitlistStatus.Active)
            throw new BusinessException("Can only update position for active waitlist entries");

        Position = newPosition;
    }
}
```

**Indexes**:
```sql
CREATE INDEX IX_Waitlist_TenantId ON Waitlist(TenantId);
CREATE INDEX IX_Waitlist_GradeId_Status ON Waitlist(GradeId, Status);
CREATE INDEX IX_Waitlist_Position ON Waitlist(Position);
CREATE INDEX IX_Waitlist_ApplicationId ON Waitlist(ApplicationId);
```

---

### Admissions Module Enumerations

```csharp
public enum ApplicationStatus
{
    Draft = 1,              // Parent creating application
    Submitted = 2,          // Submitted, awaiting payment
    PaymentPending = 3,     // Awaiting application fee payment
    UnderReview = 4,        // Admin reviewing application
    DocumentsRequired = 5,  // Missing required documents
    InterviewScheduled = 6, // Interview booked
    AssessmentScheduled = 7,// Placement test scheduled
    UnderConsideration = 8, // All done, awaiting decision
    Approved = 9,           // Admission granted
    Rejected = 10,          // Application declined
    Waitlisted = 11,        // Placed on waitlist
    Expired = 12,           // Offer/application expired
    Withdrawn = 13,         // Parent withdrew
    Enrolled = 14           // Converted to student
}

public enum AdmissionDecision
{
    Pending = 1,
    Accepted = 2,
    AcceptedWithConditions = 3,
    Rejected = 4,
    Waitlisted = 5
}

public enum DocumentCategory
{
    BirthCertificate = 1,
    IDDocument = 2,
    Passport = 3,
    PreviousReportCard = 4,
    TransferLetter = 5,
    ImmunizationRecord = 6,
    MedicalCertificate = 7,
    ProofOfResidence = 8,
    ParentID = 9,
    PassportPhoto = 10,
    Other = 99
}

public enum InterviewStatus
{
    Scheduled = 1,
    Rescheduled = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5
}

public enum WaitlistStatus
{
    Active = 1,
    Offered = 2,      // Position available, offer sent
    Accepted = 3,     // Offer accepted
    Declined = 4,     // Offer declined
    Expired = 5,      // Offer expired
    Withdrawn = 6     // Removed from waitlist
}
```

---

## Domain Services

### SouthAfricanGradingService
```csharp
public class SouthAfricanGradingService : DomainService
{
    public SouthAfricanAchievementLevel CalculateAchievementLevel(decimal percentage)
    {
        return percentage switch
        {
            >= 80 => SouthAfricanAchievementLevel.Level7_Outstanding,
            >= 70 => SouthAfricanAchievementLevel.Level6_Meritorious,
            >= 60 => SouthAfricanAchievementLevel.Level5_Substantial,
            >= 50 => SouthAfricanAchievementLevel.Level4_Adequate,
            >= 40 => SouthAfricanAchievementLevel.Level3_Moderate,
            >= 30 => SouthAfricanAchievementLevel.Level2_Elementary,
            _ => SouthAfricanAchievementLevel.Level1_NotAchieved
        };
    }

    public string GetAchievementDescription(SouthAfricanAchievementLevel level)
    {
        return level switch
        {
            SouthAfricanAchievementLevel.Level7_Outstanding => "Outstanding achievement",
            SouthAfricanAchievementLevel.Level6_Meritorious => "Meritorious achievement",
            SouthAfricanAchievementLevel.Level5_Substantial => "Substantial achievement",
            SouthAfricanAchievementLevel.Level4_Adequate => "Adequate achievement",
            SouthAfricanAchievementLevel.Level3_Moderate => "Moderate achievement",
            SouthAfricanAchievementLevel.Level2_Elementary => "Elementary achievement",
            SouthAfricanAchievementLevel.Level1_NotAchieved => "Not achieved",
            _ => "Unknown"
        };
    }

    public bool IsPassingGrade(SouthAfricanAchievementLevel level)
    {
        return level >= SouthAfricanAchievementLevel.Level4_Adequate;
    }
}
```

### StudentEnrollmentManager
```csharp
public class StudentEnrollmentManager : DomainService
{
    public void EnrollStudent(Student student, Grade grade, Class @class, List<Guid> subjectIds)
    {
        // Validation
        Check.NotNull(student, nameof(student));
        Check.NotNull(grade, nameof(grade));
        Check.NotNull(@class, nameof(@class));

        if (grade.TenantId != student.TenantId)
            throw new BusinessException("Grade and Student must belong to same tenant");

        // Check class capacity
        if (@class.Students.Count >= @class.Capacity)
            throw new BusinessException("Class is at full capacity");

        // Enroll
        student.CurrentGradeId = grade.Id;
        student.CurrentClassId = @class.Id;

        // Enroll in subjects
        foreach (var subjectId in subjectIds)
        {
            student.SubjectEnrollments.Add(new StudentSubject
            {
                StudentId = student.Id,
                SubjectId = subjectId,
                EnrollmentDate = Clock.Now,
                IsActive = true
            });
        }
    }
}
```

### MarkCalculationManager
```csharp
public class MarkCalculationManager : DomainService
{
    private readonly IRepository<Mark, Guid> _markRepository;
    private readonly SouthAfricanGradingService _gradingService;

    public async Task<decimal> CalculateTermAverageAsync(Guid studentId, Guid termId)
    {
        var marks = await _markRepository.GetAllListAsync(
            m => m.StudentId == studentId && m.TermId == termId && !m.IsDeleted
        );

        if (!marks.Any()) return 0;

        return marks.Average(m => m.Percentage);
    }

    public async Task<SouthAfricanAchievementLevel> CalculateOverallAchievementAsync(
        Guid studentId, Guid termId)
    {
        var average = await CalculateTermAverageAsync(studentId, termId);
        return _gradingService.CalculateAchievementLevel(average);
    }
}
```

### FeeCalculationManager
```csharp
public class FeeCalculationManager : DomainService
{
    public Money CalculateTotalOutstanding(Student student, List<StudentFee> fees)
    {
        decimal total = 0;

        foreach (var fee in fees.Where(f => f.Status != FeeStatus.Paid))
        {
            var outstanding = fee.Amount.Amount -
                (fee.DiscountAmount?.Amount ?? 0);
            total += outstanding;
        }

        return new Money(total, "ZAR");
    }

    public void ApplyBursary(StudentFee fee, decimal percentage, string reason)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Bursary percentage must be between 0 and 100");

        var discountAmount = fee.Amount.Amount * (percentage / 100);
        fee.DiscountAmount = new Money(discountAmount, "ZAR");
        fee.DiscountReason = reason;
    }
}
```

### ReportGenerationManager
```csharp
public class ReportGenerationManager : DomainService
{
    private readonly IMarkRepository _markRepository;
    private readonly MarkCalculationManager _markCalculationManager;

    public async Task<Report> GenerateTermReport(
        Student student, Term term, AcademicYear academicYear)
    {
        // Get all marks for term
        var marks = await _markRepository.GetMarksByStudentAndTermAsync(
            student.Id, term.Id);

        // Calculate overall average
        var overallAverage = await _markCalculationManager
            .CalculateTermAverageAsync(student.Id, term.Id);

        var overallAchievement = await _markCalculationManager
            .CalculateOverallAchievementAsync(student.Id, term.Id);

        // Create report
        var report = new Report
        {
            StudentId = student.Id,
            TermId = term.Id,
            AcademicYearId = academicYear.Id,
            ReportType = ReportType.TermReport,
            GeneratedDate = Clock.Now,
            OverallAverage = overallAverage,
            OverallAchievementLevel = overallAchievement,
            IsPublished = false
        };

        // Generate PDF (separate service)
        // report.ReportFileUrl = await _pdfGenerator.GenerateReportPdf(report);

        return report;
    }
}
```

---

## Repository Interfaces

```csharp
public interface IStudentRepository : IRepository<Student, Guid>
{
    Task<Student> FindByAdmissionNumberAsync(string admissionNumber);
    Task<List<Student>> GetStudentsByGradeAsync(Guid gradeId);
    Task<List<Student>> GetStudentsByClassAsync(Guid classId);
    Task<List<Student>> GetActiveStudentsAsync();
}

public interface IMarkRepository : IRepository<Mark, Guid>
{
    Task<List<Mark>> GetMarksByStudentAndTermAsync(Guid studentId, Guid termId);
    Task<decimal> GetAverageMarkBySubjectAsync(Guid subjectId, Guid termId);
    Task<List<Mark>> GetLockedMarksAsync(Guid termId);
}

public interface IPaymentRepository : IRepository<Payment, Guid>
{
    Task<List<Payment>> GetUnallocatedPaymentsAsync(Guid studentId);
    Task<decimal> GetTotalPaymentsByStudentAsync(Guid studentId, Guid academicYearId);
    Task<Payment> FindByReceiptNumberAsync(string receiptNumber);
}

public interface IReportRepository : IRepository<Report, Guid>
{
    Task<Report> GetLatestReportAsync(Guid studentId, Guid termId);
    Task<List<Report>> GetPublishedReportsAsync(Guid studentId);
}

public interface IApplicationRepository : IRepository<Application, Guid>
{
    Task<Application> FindByApplicationNumberAsync(string applicationNumber);
    Task<List<Application>> GetApplicationsByStatusAsync(ApplicationStatus status);
    Task<List<Application>> GetApplicationsByGradeAsync(Guid gradeId);
    Task<List<Application>> GetPendingApplicationsAsync();
    Task<int> GetNextApplicationSequenceAsync(int year);
    Task<bool> HasCapacityForGradeAsync(Guid gradeId);
}

public interface IWaitlistRepository : IRepository<Waitlist, Guid>
{
    Task<List<Waitlist>> GetWaitlistByGradeAsync(Guid gradeId, WaitlistStatus status);
    Task<int> GetNextWaitlistPositionAsync(Guid gradeId);
    Task<List<Waitlist>> GetExpiredOffersAsync();
    Task UpdatePositionsAfterRemovalAsync(Guid gradeId, int removedPosition);
}
```

---

## Key Business Rules

1. **Students** must be assigned to a Grade and Class
2. **Marks** cannot be edited once locked (IsLocked = true)
3. **Parents** must be linked to at least one Student
4. **Teachers** must be assigned to Subjects and Classes they teach
5. **Reports** can only be published once all marks are locked
6. **Payments** must be allocated to specific fees
7. **Academic Year** - only one can be current at a time (per tenant)
8. **Term** - only one can be current per academic year
9. **Fee structures** must be defined before assigning to students
10. **Attendance** can only be taken by assigned teacher
11. **SA ID numbers** must pass Luhn validation
12. **POPIA consent** required before storing student data
13. **VAT handling** for registered schools (15%)
14. **4-term academic year** (SA standard)
15. **7-level achievement grading** (SA CAPS)
16. **User.TenantId must match entity.TenantId** for Parent/Teacher
17. **Junction table entities must belong to same tenant**
18. **Application Number** format: APP-{TenantId}-{Year}-{Sequence}
19. **Application fee** must be paid before review begins
20. **At least one parent/guardian** required per application
21. **Birth certificate or passport** required (SA students must have ID number)
22. **Previous school report card** required for Grades 2-12
23. **Interview** is optional (configurable per school/grade)
24. **Assessment** is optional (configurable per school/grade)
25. **Admission capacity limits** per grade enforced
26. **Waitlist position** automatic based on application date (FIFO)
27. **Acceptance offer** expires after 14 days (configurable)
28. **Application** expires after 12 months if no decision made
29. **Once accepted and enrolled**, Application.CreatedStudentId links to Student entity
30. **Only accepted applications** can be converted to students

---

## Domain Events

```csharp
public class StudentEnrolledEvent : DomainEvent
{
    public Guid StudentId { get; set; }
    public Guid GradeId { get; set; }
    public Guid ClassId { get; set; }
    public DateTime EnrollmentDate { get; set; }
}

public class MarkCapturedEvent : DomainEvent
{
    public Guid MarkId { get; set; }
    public Guid StudentId { get; set; }
    public Guid SubjectId { get; set; }
    public decimal Percentage { get; set; }
}

public class PaymentReceivedEvent : DomainEvent
{
    public Guid PaymentId { get; set; }
    public Guid StudentId { get; set; }
    public Money Amount { get; set; }
}

public class ReportPublishedEvent : DomainEvent
{
    public Guid ReportId { get; set; }
    public Guid StudentId { get; set; }
    public Guid TermId { get; set; }
}

public class POPIAConsentWithdrawnEvent : DomainEvent
{
    public Guid StudentId { get; set; }
    public DateTime WithdrawalDate { get; set; }
}

public class ApplicationSubmittedEvent : DomainEvent
{
    public Guid ApplicationId { get; set; }
    public string ApplicationNumber { get; set; }
    public Guid AppliedGradeId { get; set; }
    public DateTime SubmissionDate { get; set; }
}

public class ApplicationApprovedEvent : DomainEvent
{
    public Guid ApplicationId { get; set; }
    public string ApplicationNumber { get; set; }
    public DateTime DecisionDate { get; set; }
    public DateTime ExpiryDate { get; set; }
}

public class ApplicationRejectedEvent : DomainEvent
{
    public Guid ApplicationId { get; set; }
    public string ApplicationNumber { get; set; }
    public string DecisionReason { get; set; }
    public DateTime DecisionDate { get; set; }
}

public class ApplicationWaitlistedEvent : DomainEvent
{
    public Guid ApplicationId { get; set; }
    public Guid GradeId { get; set; }
    public int WaitlistPosition { get; set; }
    public DateTime AddedDate { get; set; }
}

public class ApplicationConvertedToStudentEvent : DomainEvent
{
    public Guid ApplicationId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime ConversionDate { get; set; }
}

public class ApplicationFeePaidEvent : DomainEvent
{
    public Guid ApplicationId { get; set; }
    public Guid ApplicationFeeId { get; set; }
    public decimal Amount { get; set; }
    public string ReceiptNumber { get; set; }
    public DateTime PaymentDate { get; set; }
}

public class InterviewScheduledEvent : DomainEvent
{
    public Guid ApplicationId { get; set; }
    public Guid InterviewId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public TimeSpan ScheduledTime { get; set; }
    public long InterviewerUserId { get; set; }
}

public class InterviewCompletedEvent : DomainEvent
{
    public Guid ApplicationId { get; set; }
    public Guid InterviewId { get; set; }
    public int Rating { get; set; }
    public bool Recommended { get; set; }
    public DateTime CompletedDate { get; set; }
}

public class AssessmentCompletedEvent : DomainEvent
{
    public Guid ApplicationId { get; set; }
    public Guid AssessmentId { get; set; }
    public decimal Percentage { get; set; }
    public bool Passed { get; set; }
    public DateTime CompletedDate { get; set; }
}

public class WaitlistPositionOfferedEvent : DomainEvent
{
    public Guid WaitlistId { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid GradeId { get; set; }
    public DateTime OfferExpiryDate { get; set; }
}

public class WaitlistOfferAcceptedEvent : DomainEvent
{
    public Guid WaitlistId { get; set; }
    public Guid ApplicationId { get; set; }
    public DateTime AcceptedDate { get; set; }
}
```

---

## Summary Statistics

- **Total Entities**: 49
- **Aggregate Roots**: 27 (added: Application, Waitlist)
- **Child Entities**: 16 (added: ApplicantParent, ApplicationDocument, ApplicationFee, AdmissionInterview, AdmissionAssessment)
- **Junction Tables**: 9
- **Value Objects**: 3 (Address, Money, SouthAfricanIdNumber)
- **Domain Services**: 5+
- **Enumerations**: 30+ (added: ApplicationStatus, AdmissionDecision, DocumentCategory, InterviewStatus, WaitlistStatus)
- **SA-Specific Entities**: 7
- **Multi-Tenant Entities**: 41 (added: Application, Waitlist)
- **Host-Level Entities**: 1 (SouthAfricanPublicHoliday)

---

## Critical ABP Implementation Points

1. **TenantId Type**: `int?` (nullable int) - NOT Guid
2. **UserId Type**: `long` - NOT Guid (ABP User.Id is long)
3. **IMultiTenant**: All tenant-scoped entities implement this interface
4. **Data Filtering**: ABP automatically filters queries by TenantId
5. **Concurrency Control**: Use `[ConcurrencyCheck]` on Payment, Mark, StudentFee
6. **Soft Delete**: Automatic via `ISoftDelete` interface
7. **Audit Logging**: Automatic via `FullAuditedAggregateRoot`
8. **Value Objects**: Immutable, no identity, compared by value
9. **Domain Services**: Business logic that doesn't belong to a single entity
10. **Repository Pattern**: ABP provides generic repositories automatically

---

**Document Version**: 2.0 (Corrected & Aligned)
**Last Updated**: 2026-01-27
**Framework**: ASP.NET Boilerplate (ABP)
**Target**: South African Private Schools
**Compliance**: POPIA (SA Data Protection Act)
