# Remaining 31 PSMS Domain Entities - Implementation Templates

This document provides implementation templates for the remaining 31 entities that need to be created.

## Implementation Pattern

All entities follow ASP.NET Boilerplate (NOT ABP vNext) patterns:
- Use `Abp.Domain.Entities` and `Abp.Domain.Entities.Auditing`
- Use `int? TenantId` for multi-tenancy
- Use `long` for UserId references (NOT Guid)
- Base classes: `Entity<Guid>`, `CreationAuditedEntity<Guid>`, `FullAuditedEntity<Guid>`
- Interfaces: `IMayHaveTenant`, `ISoftDelete`

---

## PHASE 3: Admissions Module (7 entities)

### 1. Application.cs
**Pattern**: FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
**Key Fields**:
- ApplicationNumber (string) - unique per tenant
- ProspectiveStudent fields (FirstName, LastName, DateOfBirth, Gender, etc.)
- Status (ApplicationStatus enum)
- Decision (AdmissionDecision enum)
- AppliedGradeId (Guid)
- ReviewedByUserId (long)
- CreatedStudentId (Guid?) - if converted
- ConcurrencyStamp (string)
**Collections**: ApplicationDocuments, ApplicantParents, Fees, Interviews, Assessments

### 2. Waitlist.cs
**Pattern**: FullAuditedEntity<Guid>, IMayHaveTenant
**Key Fields**:
- ApplicationId (Guid)
- GradeId (Guid)
- Position (int)
- Status (WaitlistStatus enum)
- AddedDate (DateTime)
- NotifiedDate (DateTime?)
- OfferExpiryDate (DateTime?)

### 3. ApplicantParent.cs
**Pattern**: Entity<Guid>
**Key Fields**:
- ApplicationId (Guid)
- FirstName, LastName, Email, Phone, IdNumber
- PhysicalAddress (Address VO)
- PostalAddress (Address VO)
- Relationship (RelationshipType enum)
- IsPrimaryContact (bool)

### 4. ApplicationDocument.cs
**Pattern**: CreationAuditedEntity<Guid>
**Key Fields**:
- ApplicationId (Guid)
- Category (DocumentCategory enum)
- FileName (string)
- FileUrl (string)
- FileSize (long)
- IsVerified (bool)
- VerifiedByUserId (long?)

### 5. ApplicationFee.cs
**Pattern**: CreationAuditedEntity<Guid>
**Key Fields**:
- ApplicationId (Guid)
- Amount (decimal)
- Currency (string, default "ZAR")
- Status (PaymentStatus enum)
- PaymentMethod (SouthAfricanPaymentMethod enum)
- PaymentDate (DateTime?)
- ReceiptNumber (string)
- IsRefundable (bool)

### 6. AdmissionInterview.cs
**Pattern**: CreationAuditedEntity<Guid>
**Key Fields**:
- ApplicationId (Guid)
- ScheduledDate (DateTime)
- InterviewerUserId (long)
- Status (InterviewStatus enum)
- Rating (int?) - e.g., 1-10
- Notes (string)
- Recommended (bool?)

### 7. AdmissionAssessment.cs
**Pattern**: CreationAuditedEntity<Guid>
**Key Fields**:
- ApplicationId (Guid)
- AssessedGradeId (Guid)
- AssessmentDate (DateTime)
- TotalScore (decimal)
- MaxScore (decimal)
- Percentage (decimal)
- Passed (bool)
- AssessorUserId (long)
- Notes (string)

---

## PHASE 4: Financial Module (4 entities)

### 1. FeeStructure.cs
**Pattern**: FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
**Key Fields**:
- GradeId (Guid)
- AcademicYearId (Guid)
- FeeType (SouthAfricanFeeType enum)
- Amount (decimal)
- Currency (string, default "ZAR")
- BillingFrequency (PaymentPlan enum)
- Description (string)
- IsActive (bool)

### 2. StudentFee.cs
**Pattern**: FullAuditedEntity<Guid>, IMayHaveTenant
**Key Fields**:
- StudentId (Guid)
- FeeStructureId (Guid)
- AmountDue (decimal)
- AmountPaid (decimal)
- DueDate (DateTime)
- PaidDate (DateTime?)
- Status (FeeStatus enum)
- ConcurrencyStamp (string)
**Collections**: Payments (through PaymentAllocations)

### 3. Payment.cs
**Pattern**: FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
**Key Fields**:
- StudentId (Guid)
- ParentId (Guid) - who made payment
- Amount (decimal)
- Currency (string, default "ZAR")
- PaymentMethod (SouthAfricanPaymentMethod enum)
- PaymentDate (DateTime)
- ReceiptNumber (string)
- ReferenceNumber (string)
- Status (PaymentStatus enum)
- ConcurrencyStamp (string)
**Collections**: PaymentAllocations

### 4. PaymentAllocation.cs
**Pattern**: Entity<Guid>
**Key Fields**:
- PaymentId (Guid)
- StudentFeeId (Guid)
- Amount (decimal) - portion allocated to this fee

---

## PHASE 5: Assessment Module (6 entities)

### 1. Assessment.cs
**Pattern**: FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
**Key Fields**:
- Title (string)
- SubjectId (Guid)
- GradeId (Guid)
- TermId (Guid)
- TeacherId (Guid)
- AssessmentType (AssessmentType enum)
- MaxMark (decimal)
- DueDate (DateTime)
- IsPublished (bool)
**Collections**: Questions, Marks

### 2. Mark.cs
**Pattern**: FullAuditedEntity<Guid>, IMayHaveTenant
**Key Fields**:
- StudentId (Guid)
- AssessmentId (Guid)
- SubjectId (Guid)
- TermId (Guid)
- MarkValue (decimal)
- MaxMark (decimal)
- Percentage (decimal)
- AchievementLevel (SouthAfricanAchievementLevel enum)
- TeacherId (Guid)
- Feedback (string)
- IsLocked (bool)
- ConcurrencyStamp (string)

### 3. Report.cs
**Pattern**: FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
**Key Fields**:
- StudentId (Guid)
- TermId (Guid)
- AcademicYearId (Guid)
- ReportType (ReportType enum)
- PublishedDate (DateTime?)
- IsPublished (bool)
- OverallAverage (decimal)
- TeacherComment (string)
- PrincipalComment (string)
**Collections**: ReportSubjects

### 4. AssessmentQuestion.cs
**Pattern**: Entity<Guid>
**Key Fields**:
- AssessmentId (Guid)
- QuestionText (string)
- QuestionType (QuestionType enum)
- CorrectAnswer (string) - for auto-marking
- Marks (decimal)
- OrderNumber (int)

### 5. StudentAnswer.cs
**Pattern**: Entity<Guid>
**Key Fields**:
- AssessmentQuestionId (Guid)
- StudentId (Guid)
- AnswerText (string)
- IsCorrect (bool?)
- MarksAwarded (decimal?)
- SubmittedDate (DateTime)

### 6. ReportSubject.cs
**Pattern**: Entity<Guid>
**Key Fields**:
- ReportId (Guid)
- SubjectId (Guid)
- SubjectName (string) - denormalized
- Average (decimal)
- AchievementLevel (SouthAfricanAchievementLevel enum)
- TeacherComment (string)

---

## PHASE 6: Learning Module (2 entities)

### 1. LearningMaterial.cs
**Pattern**: FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
**Key Fields**:
- SubjectId (Guid)
- GradeId (Guid)
- TermId (Guid?)
- TeacherId (Guid)
- Title (string)
- Description (string)
- MaterialType (MaterialType enum)
- FileUrl (string)
- FileSize (long)
- ReleaseDate (DateTime)
- IsPublished (bool)

### 2. OnlineLesson.cs
**Pattern**: FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
**Key Fields**:
- SubjectId (Guid)
- ClassId (Guid)
- TeacherId (Guid)
- Title (string)
- Description (string)
- LessonType (LessonType enum)
- ScheduledDate (DateTime)
- StartTime (TimeSpan)
- Duration (int) - minutes
- MeetingLink (string)
- RecordingUrl (string)

---

## PHASE 7: Communication Module (5 entities)

### 1. Announcement.cs
**Pattern**: FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
**Key Fields**:
- Title (string)
- Message (string)
- AnnouncementType (AnnouncementType enum)
- TargetAudience (TargetAudience enum)
- TargetGradeId (Guid?)
- TargetClassId (Guid?)
- Priority (int) - 1=low, 2=medium, 3=high
- PublishedDate (DateTime?)
- ExpiryDate (DateTime?)
- IsPublished (bool)
**Collections**: AnnouncementReads

### 2. Message.cs
**Pattern**: FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
**Key Fields**:
- SenderUserId (long)
- RecipientUserId (long)
- Subject (string)
- Body (string)
- IsRead (bool)
- ReadDate (DateTime?)
- ParentMessageId (Guid?) - for threading

### 3. Notification.cs
**Pattern**: CreationAuditedEntity<Guid>
**Key Fields**:
- UserId (long)
- TenantId (int?)
- NotificationType (NotificationType enum)
- Title (string)
- Message (string)
- IsRead (bool)
- ReadDate (DateTime?)
- ActionUrl (string)

### 4. Document.cs
**Pattern**: FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
**Key Fields**:
- Title (string)
- Description (string)
- DocumentType (DocumentType enum)
- FileUrl (string)
- FileSize (long)
- TargetAudience (TargetAudience enum)
- TargetGradeId (Guid?)
- PublishedDate (DateTime?)
- ExpiryDate (DateTime?)
- UploadedByUserId (long)

### 5. AnnouncementRead.cs
**Pattern**: Entity<Guid>
**Key Fields**:
- AnnouncementId (Guid)
- UserId (long)
- ReadDate (DateTime)

---

## PHASE 8: SASpecific Module (6 entities)

### 1. SchoolTransport.cs
**Pattern**: FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
**Key Fields**:
- RouteName (string)
- Description (string)
- MonthlyFee (decimal)
- Currency (string, default "ZAR")
- DriverName (string)
- DriverPhone (string)
- VehicleRegistration (string)
- IsActive (bool)
**Collections**: StudentTransports

### 2. AfterCare.cs
**Pattern**: FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
**Key Fields**:
- Name (string)
- Description (string)
- MonthlyFee (decimal)
- DailyDropInFee (decimal)
- Currency (string, default "ZAR")
- StartTime (TimeSpan)
- EndTime (TimeSpan)
- MaxCapacity (int)
- IsActive (bool)
**Collections**: StudentAfterCares

### 3. ExtramuralActivity.cs
**Pattern**: FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
**Key Fields**:
- Name (string)
- Description (string)
- ActivityType (ExtramuralType enum)
- TermFee (decimal)
- Currency (string, default "ZAR")
- Coach (string)
- CoachPhone (string)
- PracticeDays (string) - comma-separated days
- PracticeTime (TimeSpan)
- IsActive (bool)
**Collections**: StudentExtramurals

### 4. StudentTransport.cs (Junction)
**Pattern**: Entity<Guid>
**Key Fields**:
- StudentId (Guid)
- SchoolTransportId (Guid)
- EnrollmentDate (DateTime)
- IsActive (bool)

### 5. StudentAfterCare.cs (Junction)
**Pattern**: Entity<Guid>
**Key Fields**:
- StudentId (Guid)
- AfterCareId (Guid)
- EnrollmentDate (DateTime)
- IsActive (bool)

### 6. StudentExtramural.cs (Junction)
**Pattern**: Entity<Guid>
**Key Fields**:
- StudentId (Guid)
- ExtramuralActivityId (Guid)
- EnrollmentDate (DateTime)
- IsActive (bool)

---

## Implementation Checklist

For each entity, ensure:

1. ✅ Correct using statements (`Abp.Domain.Entities`, not `Volo.Abp`)
2. ✅ Correct namespace (`psms.Domain.[ModuleName].Entities`)
3. ✅ Table attribute with table name
4. ✅ XML documentation for class
5. ✅ Constants for MaxLength values
6. ✅ `int? TenantId` property with `IMayHaveTenant` or `IMustHaveTenant`
7. ✅ All properties with XML comments
8. ✅ [Required] and [StringLength] attributes
9. ✅ [ForeignKey] attributes on navigation properties
10. ✅ Protected parameterless constructor
11. ✅ Public constructor with required parameters
12. ✅ Collection properties initialized in constructor
13. ✅ `ISoftDelete` with `IsDeleted` property where specified
14. ✅ Domain methods if applicable

---

## Quick Start Command

To create all remaining entities at once, use this PowerShell script approach or create each file individually following the patterns shown in the completed Academic module entities.

**Reference Files**:
- Academic module entities: `C:\Users\Rapudi Hlakudi\Desktop\PSMS\backend\src\psms.Core\Domain\Academic\Entities\*.cs`
- Domain model: `C:\Users\Rapudi Hlakudi\Desktop\PSMS\PSMS-Domain-Model-CORRECTED.md`
