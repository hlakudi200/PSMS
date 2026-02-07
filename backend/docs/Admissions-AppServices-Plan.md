# Admissions Module - Application Services Plan

## Overview
This document outlines the Application Services required for the Admissions module of the PSMS (Private School Management System) for South African schools. All services are designed to enforce the business rules defined in `PSMS-Business-Rules-ADMISSIONS.md`.

**Framework**: ASP.NET Boilerplate (ABP)
**Business Rules Reference**: ADM-001 through ADM-031
**Permissions Reference**: [PSMS-Permissions-Matrix.md](./PSMS-Permissions-Matrix.md)

---

## Module Structure

```
psms.Application/
└── Admissions/
    ├── Applications/
    │   ├── IApplicationAppService.cs
    │   ├── ApplicationAppService.cs
    │   └── Dto/
    │       ├── CreateApplicationDto.cs
    │       ├── UpdateApplicationDto.cs
    │       ├── ApplicationDto.cs
    │       ├── ApplicationListDto.cs
    │       ├── SubmitApplicationDto.cs
    │       ├── ApplicationDecisionDto.cs
    │       ├── ApplicationStatisticsDto.cs
    │       └── GetApplicationsInput.cs
    ├── ApplicantParents/
    │   ├── IApplicantParentAppService.cs
    │   ├── ApplicantParentAppService.cs
    │   └── Dto/
    │       ├── CreateApplicantParentDto.cs
    │       ├── UpdateApplicantParentDto.cs
    │       └── ApplicantParentDto.cs
    ├── ApplicationDocuments/
    │   ├── IApplicationDocumentAppService.cs
    │   ├── ApplicationDocumentAppService.cs
    │   └── Dto/
    │       ├── UploadDocumentDto.cs
    │       ├── ApplicationDocumentDto.cs
    │       ├── VerifyDocumentDto.cs
    │       └── RequiredDocumentsStatusDto.cs
    ├── ApplicationFees/
    │   ├── IApplicationFeeAppService.cs
    │   ├── ApplicationFeeAppService.cs
    │   └── Dto/
    │       ├── ApplicationFeeDto.cs
    │       ├── RecordPaymentDto.cs
    │       ├── PaymentResultDto.cs
    │       └── PaymentCallbackDto.cs
    ├── AdmissionInterviews/
    │   ├── IAdmissionInterviewAppService.cs
    │   ├── AdmissionInterviewAppService.cs
    │   └── Dto/
    │       ├── ScheduleInterviewDto.cs
    │       ├── AdmissionInterviewDto.cs
    │       ├── CompleteInterviewDto.cs
    │       ├── RescheduleInterviewDto.cs
    │       └── TimeSlotDto.cs
    ├── AdmissionAssessments/
    │   ├── IAdmissionAssessmentAppService.cs
    │   ├── AdmissionAssessmentAppService.cs
    │   └── Dto/
    │       ├── ScheduleAssessmentDto.cs
    │       ├── AdmissionAssessmentDto.cs
    │       └── RecordAssessmentResultsDto.cs
    ├── Waitlists/
    │   ├── IWaitlistAppService.cs
    │   ├── WaitlistAppService.cs
    │   └── Dto/
    │       ├── WaitlistDto.cs
    │       ├── WaitlistPositionDto.cs
    │       └── OfferWaitlistPositionDto.cs
    ├── AdmissionSettings/
    │   ├── IAdmissionSettingsAppService.cs
    │   ├── AdmissionSettingsAppService.cs
    │   └── Dto/
    │       ├── CreateAdmissionSettingsDto.cs
    │       ├── UpdateAdmissionSettingsDto.cs
    │       ├── AdmissionSettingsDto.cs
    │       └── CapacityStatusDto.cs
    ├── Enrollment/
    │   ├── IEnrollmentAppService.cs
    │   ├── EnrollmentAppService.cs
    │   └── Dto/
    │       ├── CompleteEnrollmentDto.cs
    │       ├── EnrollmentStatusDto.cs
    │       └── ClassAssignmentDto.cs
    └── Shared/
        ├── AdmissionsMapper.cs (AutoMapper Profile)
        ├── AdmissionsExceptionCodes.cs
        └── DomainEvents/
            ├── ApplicationSubmittedEvent.cs
            ├── ApplicationFeePaidEvent.cs
            ├── DocumentsVerifiedEvent.cs
            ├── InterviewScheduledEvent.cs
            ├── InterviewCompletedEvent.cs
            ├── AssessmentScheduledEvent.cs
            ├── AssessmentCompletedEvent.cs
            ├── ApplicationApprovedEvent.cs
            ├── ApplicationRejectedEvent.cs
            ├── ApplicationWaitlistedEvent.cs
            ├── WaitlistPositionOfferedEvent.cs
            ├── WaitlistOfferAcceptedEvent.cs
            ├── ApplicationConvertedToStudentEvent.cs
            ├── ApplicationWithdrawnEvent.cs
            └── ApplicationExpiredEvent.cs
```

> **Note**: SA ID validation uses the shared `psms.Domain.Shared.Validators.SAIdNumberValidator` class.

---

## Exception Codes (ADM-XXX)

```csharp
public static class AdmissionsExceptionCodes
{
    public const string InvalidApplicationNumber = "INVALID_APPLICATION_NUMBER";           // ADM-001
    public const string InvalidProspectiveStudentInfo = "INVALID_PROSPECTIVE_STUDENT_INFO"; // ADM-002
    public const string ParentInformationRequired = "PARENT_INFORMATION_REQUIRED";         // ADM-003
    public const string InvalidGradeSelection = "INVALID_GRADE_SELECTION";                 // ADM-004
    public const string InvalidStatusTransition = "INVALID_STATUS_TRANSITION";             // ADM-005
    public const string ApplicationFeeNotPaid = "APPLICATION_FEE_NOT_PAID";               // ADM-006
    public const string RefundNotAllowed = "REFUND_NOT_ALLOWED";                          // ADM-007
    public const string RequiredDocumentsMissing = "REQUIRED_DOCUMENTS_MISSING";          // ADM-008
    public const string InvalidDocumentUpload = "INVALID_DOCUMENT_UPLOAD";                // ADM-009
    public const string DocumentsNotVerified = "DOCUMENTS_NOT_VERIFIED";                  // ADM-010
    public const string InterviewRequired = "INTERVIEW_REQUIRED";                         // ADM-011
    public const string InvalidInterviewSchedule = "INVALID_INTERVIEW_SCHEDULE";          // ADM-012
    public const string InterviewNotCompleted = "INTERVIEW_NOT_COMPLETED";                // ADM-013
    public const string AssessmentRequired = "ASSESSMENT_REQUIRED";                       // ADM-014
    public const string InvalidAssessmentSchedule = "INVALID_ASSESSMENT_SCHEDULE";        // ADM-015
    public const string AssessmentBelowPassMark = "ASSESSMENT_BELOW_PASS_MARK";          // ADM-016
    public const string UnauthorizedAdmissionDecision = "UNAUTHORIZED_ADMISSION_DECISION"; // ADM-017
    public const string OfferExpired = "OFFER_EXPIRED";                                   // ADM-018
    public const string RejectionReasonRequired = "REJECTION_REASON_REQUIRED";            // ADM-020
    public const string WaitlistOfferExpired = "WAITLIST_OFFER_EXPIRED";                 // ADM-023
    public const string EnrollmentNotComplete = "ENROLLMENT_NOT_COMPLETE";               // ADM-025
    public const string StudentCreationFailed = "STUDENT_CREATION_FAILED";               // ADM-026
    public const string GradeCapacityFull = "GRADE_CAPACITY_FULL";                       // ADM-028
    public const string ClassAssignmentRequired = "CLASS_ASSIGNMENT_REQUIRED";            // ADM-029
}
```

---

## 1. ApplicationAppService

### Purpose
Main service for managing admission applications through the entire workflow. Enforces ADM-001 through ADM-005, ADM-017 through ADM-020.

### Role Access Matrix
| Method Category | Admin | Principal | VP | Admissions Officer | Applicant |
|-----------------|:-----:|:---------:|:--:|:------------------:|:---------:|
| CRUD Operations | ✅ | ✅ | ✅ | ✅ | 🔒 Own |
| Review Workflow | ✅ | ✅ | ✅ | ✅ | ❌ |
| Decision Operations | ⚙️ | ✅ | ⚙️ | ❌ | ❌ |
| Offer Management | ✅ | ✅ | ✅ | ❌ | ❌ |
| Withdrawal | ✅ | ✅ | ✅ | ✅ | 🔒 Own |

### Interface: IApplicationAppService

```csharp
public interface IApplicationAppService : IApplicationService
{
    // CRUD Operations
    // Permission: Admissions.Applications.Create (Applicant can create own)
    [AbpAuthorize(PermissionNames.Admissions_Applications_Create)]
    Task<ApplicationDto> CreateAsync(CreateApplicationDto input);  // ADM-001, ADM-002

    // Permission: Admissions.Applications.View (Applicant can view own)
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<ApplicationDto> GetAsync(Guid id);

    // Permission: Admissions.Applications.ViewAll (Staff only)
    [AbpAuthorize(PermissionNames.Admissions_Applications_ViewAll)]
    Task<PagedResultDto<ApplicationListDto>> GetListAsync(GetApplicationsInput input);

    // Permission: Admissions.Applications.Edit (Applicant can edit own Draft only)
    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    Task<ApplicationDto> UpdateAsync(Guid id, UpdateApplicationDto input);  // ADM-002

    // Permission: Admissions.Applications.Delete (Admin/Principal only, Draft status only)
    [AbpAuthorize(PermissionNames.Admissions_Applications_Delete)]
    Task DeleteAsync(Guid id);

    // Workflow Operations (ADM-005)
    // Permission: Admissions.Applications.Submit (Applicant can submit own)
    [AbpAuthorize(PermissionNames.Admissions_Applications_Submit)]
    Task<ApplicationDto> SubmitAsync(Guid id);  // Draft → PaymentPending

    // Permission: Admissions.Review.StartReview
    [AbpAuthorize(PermissionNames.Admissions_Review_StartReview)]
    Task<ApplicationDto> StartReviewAsync(Guid id);  // PaymentPending → UnderReview (after fee paid)

    // Permission: Admissions.Review.RequestDocuments
    [AbpAuthorize(PermissionNames.Admissions_Review_RequestDocuments)]
    Task<ApplicationDto> RequestDocumentsAsync(Guid id, List<DocumentCategory> requiredDocuments);  // → DocumentsRequired

    // Permission: Admissions.Review.StartReview (same as start review)
    [AbpAuthorize(PermissionNames.Admissions_Review_StartReview)]
    Task<ApplicationDto> MarkDocumentsCompleteAsync(Guid id);  // DocumentsRequired → UnderReview

    // Permission: Admissions.Interviews.Schedule
    [AbpAuthorize(PermissionNames.Admissions_Interviews_Schedule)]
    Task<ApplicationDto> ScheduleInterviewAsync(Guid id);  // → InterviewScheduled

    // Permission: Admissions.Interviews.Conduct
    [AbpAuthorize(PermissionNames.Admissions_Interviews_Conduct)]
    Task<ApplicationDto> CompleteInterviewAsync(Guid id);  // InterviewScheduled → UnderConsideration

    // Permission: Admissions.Assessments.Schedule
    [AbpAuthorize(PermissionNames.Admissions_Assessments_Schedule)]
    Task<ApplicationDto> ScheduleAssessmentAsync(Guid id);  // → AssessmentScheduled

    // Permission: Admissions.Assessments.Conduct
    [AbpAuthorize(PermissionNames.Admissions_Assessments_Conduct)]
    Task<ApplicationDto> CompleteAssessmentAsync(Guid id);  // AssessmentScheduled → UnderConsideration

    // Permission: Admissions.Review.StartReview
    [AbpAuthorize(PermissionNames.Admissions_Review_StartReview)]
    Task<ApplicationDto> MoveToConsiderationAsync(Guid id);  // UnderReview → UnderConsideration (no interview/assessment)

    // Decision Operations (ADM-017 - ADM-020)
    // Permission: Admissions.Decision.Approve (Principal, or delegated VP/Admin)
    [AbpAuthorize(PermissionNames.Admissions_Decision_Approve)]
    Task<ApplicationDto> ApproveAsync(Guid id, ApplicationDecisionDto input);  // → Approved

    // Permission: Admissions.Decision.Approve (same permission, different outcome)
    [AbpAuthorize(PermissionNames.Admissions_Decision_Approve)]
    Task<ApplicationDto> ApproveWithConditionsAsync(Guid id, ApplicationDecisionDto input);  // ADM-019

    // Permission: Admissions.Decision.Reject (Principal, or delegated VP/Admin)
    [AbpAuthorize(PermissionNames.Admissions_Decision_Reject)]
    Task<ApplicationDto> RejectAsync(Guid id, ApplicationDecisionDto input);  // ADM-020: reason min 50 chars

    // Permission: Admissions.Decision.Waitlist (Principal, or delegated VP/Admin)
    [AbpAuthorize(PermissionNames.Admissions_Decision_Waitlist)]
    Task<ApplicationDto> WaitlistAsync(Guid id, ApplicationDecisionDto input);  // ADM-021: → Waitlisted

    // Offer Management (ADM-018)
    // Permission: Admissions.Decision.ExtendOffer
    [AbpAuthorize(PermissionNames.Admissions_Decision_ExtendOffer)]
    Task<ApplicationDto> ExtendOfferAsync(Guid id, int additionalDays);  // Max 7 days, once only

    // Withdrawal & Expiry (ADM-030, ADM-031)
    // Permission: Admissions.Applications.Withdraw (Applicant can withdraw own)
    [AbpAuthorize(PermissionNames.Admissions_Applications_Withdraw)]
    Task<ApplicationDto> WithdrawAsync(Guid id, string reason);

    // No permission required - background job
    Task ExpireApplicationsAsync();  // Background job for auto-expiry

    // Enrollment (ADM-025, ADM-026)
    // Permission: Admissions.Enrollment.Complete
    [AbpAuthorize(PermissionNames.Admissions_Enrollment_Complete)]
    Task<ApplicationDto> MarkAsEnrolledAsync(Guid id, Guid studentId);

    // Queries (ViewAll permission for staff queries)
    [AbpAuthorize(PermissionNames.Admissions_Applications_ViewAll)]
    Task<ApplicationDto> GetByApplicationNumberAsync(string applicationNumber);

    [AbpAuthorize(PermissionNames.Admissions_Applications_ViewAll)]
    Task<List<ApplicationListDto>> GetByStatusAsync(ApplicationStatus status);

    [AbpAuthorize(PermissionNames.Admissions_Applications_ViewAll)]
    Task<int> GetCountByStatusAsync(ApplicationStatus status);

    [AbpAuthorize(PermissionNames.Admissions_Applications_ViewAll)]
    Task<ApplicationStatisticsDto> GetStatisticsAsync(Guid academicYearId);

    // Public - checks if school is accepting applications
    Task<bool> CanApplyAsync(Guid gradeId, Guid academicYearId);

    // For Applicants (Public Portal) - Own data only, verified by business logic
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<ApplicationDto> GetMyApplicationAsync(Guid id);

    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<List<ApplicationListDto>> GetMyApplicationsAsync();
}
```

### Key Business Rules Enforcement
1. **ADM-001**: Application number auto-generated: `APP-{TenantId}-{Year}-{Sequence}`
2. **ADM-002**: Validate prospective student info (age 4-19, SA ID validation using `SAIdNumberValidator`)
3. **ADM-003**: At least one parent before submission
4. **ADM-004**: Age-grade validation (±2 years tolerance)
5. **ADM-005**: Status transitions must follow defined workflow
6. **ADM-017**: Only users with `Admissions.Decision.*` permissions can make decisions
7. **ADM-018**: Set expiry date on approval (14 days default)
8. **ADM-020**: Rejection requires reason ≥50 characters
9. **ADM-028**: Check grade capacity before approval

---

## 2. ApplicantParentAppService

### Purpose
Manage parent/guardian information on applications. Enforces ADM-003.

### Role Access Matrix
| Method | Admin | Principal | Admissions Officer | Applicant |
|--------|:-----:|:---------:|:------------------:|:---------:|
| Create | ✅ | ✅ | ✅ | 🔒 Own |
| Get | ✅ | ✅ | ✅ | 🔒 Own |
| Update | ✅ | ✅ | ✅ | 🔒 Own Draft |
| Delete | ✅ | ✅ | ✅ | 🔒 Own Draft |

### Interface: IApplicantParentAppService

```csharp
public interface IApplicantParentAppService : IApplicationService
{
    // Permission: Admissions.Applications.Edit (creating parent is editing application)
    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    Task<ApplicantParentDto> CreateAsync(CreateApplicantParentDto input);  // ADM-003: validate parent info

    // Permission: Admissions.Applications.View
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<ApplicantParentDto> GetAsync(Guid id);

    // Permission: Admissions.Applications.View
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<List<ApplicantParentDto>> GetByApplicationAsync(Guid applicationId);

    // Permission: Admissions.Applications.Edit
    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    Task<ApplicantParentDto> UpdateAsync(Guid id, UpdateApplicantParentDto input);

    // Permission: Admissions.Applications.Edit (ADM-003: cannot delete if only one parent)
    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    Task DeleteAsync(Guid id);

    // Permission: Admissions.Applications.Edit
    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    Task SetAsPrimaryContactAsync(Guid id);  // ADM-003: only one primary

    // Permission: Admissions.Applications.Edit
    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    Task SetAsFinanciallyResponsibleAsync(Guid id);  // ADM-003: at least one required

    // Permission: Admissions.Applications.View
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<bool> ValidateParentEmailUniqueAsync(string email, Guid? excludeApplicationId);  // ADM-003
}
```

### Key Business Rules Enforcement
1. **ADM-003**: Minimum 1 parent, maximum 4 parents/guardians
2. **ADM-003**: At least one primary contact
3. **ADM-003**: At least one financially responsible parent
4. **ADM-003**: Required fields: First name, Last name, Email, Phone, Physical address
5. **ADM-003**: SA parents must have valid SA ID number (use `SAIdNumberValidator.IsValid()`)
6. **ADM-003**: Primary contact email unique across active applications

---

## 3. ApplicationDocumentAppService

### Purpose
Handle document uploads, verification, and management. Enforces ADM-008 through ADM-010.

### Role Access Matrix
| Method | Admin | Principal | Admissions Officer | Applicant |
|--------|:-----:|:---------:|:------------------:|:---------:|
| Upload | ✅ | ✅ | ✅ | 🔒 Own |
| View | ✅ | ✅ | ✅ | 🔒 Own |
| Download | ✅ | ✅ | ✅ | 🔒 Own |
| Verify | ✅ | ✅ | ✅ | ❌ |
| Reject | ✅ | ✅ | ✅ | ❌ |
| Delete | ✅ | ✅ | ✅ | ❌ |

### Interface: IApplicationDocumentAppService

```csharp
public interface IApplicationDocumentAppService : IApplicationService
{
    // Permission: Admissions.Documents.Upload (Applicant can upload to own application)
    [AbpAuthorize(PermissionNames.Admissions_Documents_Upload)]
    Task<ApplicationDocumentDto> UploadAsync(UploadDocumentDto input);  // ADM-009

    // Permission: Admissions.Applications.View (view documents with application)
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<ApplicationDocumentDto> GetAsync(Guid id);

    // Permission: Admissions.Applications.View
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<List<ApplicationDocumentDto>> GetByApplicationAsync(Guid applicationId);

    // Permission: Admissions.Applications.View
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<RequiredDocumentsStatusDto> GetRequiredDocumentsStatusAsync(Guid applicationId);  // ADM-008

    // Permission: Admissions.Documents.Upload (cannot delete if verified)
    [AbpAuthorize(PermissionNames.Admissions_Documents_Upload)]
    Task DeleteAsync(Guid id);  // ADM-009

    // Permission: Admissions.Documents.Verify (ADM-010: staff only)
    [AbpAuthorize(PermissionNames.Admissions_Documents_Verify)]
    Task<ApplicationDocumentDto> VerifyAsync(Guid id, VerifyDocumentDto input);

    // Permission: Admissions.Documents.Verify
    [AbpAuthorize(PermissionNames.Admissions_Documents_Verify)]
    Task<ApplicationDocumentDto> RejectAsync(Guid id, string reason);  // ADM-010

    // Permission: Admissions.Documents.Download
    [AbpAuthorize(PermissionNames.Admissions_Documents_Download)]
    Task<byte[]> DownloadAsync(Guid id);

    // Permission: Admissions.Applications.View
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<bool> AreAllRequiredDocumentsVerifiedAsync(Guid applicationId);  // ADM-010
}
```

### Key Business Rules Enforcement (ADM-008, ADM-009, ADM-010)
1. **ADM-008**: Required documents based on grade and citizenship:
   - All: Birth certificate/Passport, Parent IDs, Photo, Proof of residence
   - Grade R: Immunization record
   - Grades 1-12: Previous school report, Transfer letter (if mid-year)
   - Non-SA: Passport, Study permit, Proof of guardian in SA
2. **ADM-009**: File validation:
   - Formats: PDF, JPG, JPEG, PNG only
   - Max size: 10MB per document
   - Virus scan required
3. **ADM-010**: Verification process:
   - Only `Admissions.Documents.Verify` permission can verify
   - Mark as Verified or Rejected (with reason)
   - All documents must be verified before UnderConsideration

---

## 4. ApplicationFeeAppService

### Purpose
Manage application fee payments. Enforces ADM-006 and ADM-007.

### Role Access Matrix
| Method | Admin | Principal | Finance | Admissions Officer | Applicant |
|--------|:-----:|:---------:|:-------:|:------------------:|:---------:|
| Get Fee | ✅ | ✅ | ✅ | ✅ | 🔒 Own |
| Create Fee | ✅ | ✅ | ✅ | ✅ | ❌ |
| Record Manual | ✅ | ❌ | ✅ | ✅ | ❌ |
| Initiate Online | ✅ | ✅ | ✅ | ✅ | 🔒 Own |
| Refund | ✅ | ⚙️ | ⚙️ | ❌ | ❌ |

### Interface: IApplicationFeeAppService

```csharp
public interface IApplicationFeeAppService : IApplicationService
{
    // Permission: Admissions.Applications.View (fee visible with application)
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<ApplicationFeeDto> GetByApplicationAsync(Guid applicationId);

    // Permission: Admissions.Review.StartReview (creates fee when review starts)
    [AbpAuthorize(PermissionNames.Admissions_Review_StartReview)]
    Task<ApplicationFeeDto> CreateFeeAsync(Guid applicationId);  // Uses AdmissionSettings.ApplicationFeeAmount

    // Permission: Financial.Payments.RecordManual (EFT, Cash)
    [AbpAuthorize(PermissionNames.Financial_Payments_Process)]
    Task<PaymentResultDto> RecordManualPaymentAsync(Guid applicationId, RecordPaymentDto input);

    // Permission: Financial.Payments.Process (or Applicant paying own)
    [AbpAuthorize(PermissionNames.Financial_Payments_Process)]
    Task<PaymentResultDto> InitiateOnlinePaymentAsync(Guid applicationId, SouthAfricanPaymentMethod method);

    // No permission - webhook handler (validated by payment gateway signature)
    Task<PaymentResultDto> ProcessPaymentCallbackAsync(PaymentCallbackDto callback);

    // Permission: Admissions.Applications.View
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<bool> IsFeePaidAsync(Guid applicationId);  // ADM-006

    // Permission: Financial.Payments.Void (ADM-007: exceptional only, requires admin)
    [AbpAuthorize(PermissionNames.Financial_Payments_View)]  // Note: Needs special handling
    Task<ApplicationFeeDto> ProcessRefundAsync(Guid applicationId, string adminApprovalReason);  // ADM-007
}
```

### Key Business Rules Enforcement (ADM-006, ADM-007)
1. **ADM-006**: Fee must be paid within 7 days of submission
2. **ADM-006**: Payment methods: EFT, Card, Capitec, Ozow, SnapScan, etc.
3. **ADM-006**: Receipt auto-generated on payment
4. **ADM-006**: Status changes PaymentPending → UnderReview after payment
5. **ADM-007**: Fee is non-refundable (except admin approval for system errors)
6. **ADM-007**: Refund logged in audit trail

---

## 5. AdmissionInterviewAppService

### Purpose
Schedule and manage admission interviews. Enforces ADM-011 through ADM-013.

### Role Access Matrix
| Method | Admin | Principal | VP | HOD | Admissions Officer |
|--------|:-----:|:---------:|:--:|:---:|:------------------:|
| View | ✅ | ✅ | ✅ | ✅ | ✅ |
| Schedule | ✅ | ✅ | ✅ | ❌ | ✅ |
| Reschedule | ✅ | ✅ | ✅ | ❌ | ✅ |
| Conduct | ✅ | ✅ | ✅ | ✅ | ✅ |
| Record Outcome | ✅ | ✅ | ✅ | ✅ | ✅ |
| Cancel | ✅ | ✅ | ✅ | ❌ | ✅ |

### Interface: IAdmissionInterviewAppService

```csharp
public interface IAdmissionInterviewAppService : IApplicationService
{
    // Permission: Admissions.Interviews.Schedule
    [AbpAuthorize(PermissionNames.Admissions_Interviews_Schedule)]
    Task<AdmissionInterviewDto> ScheduleAsync(ScheduleInterviewDto input);  // ADM-012

    // Permission: Admissions.Applications.View (interview visible with application)
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<AdmissionInterviewDto> GetAsync(Guid id);

    // Permission: Admissions.Applications.View
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<AdmissionInterviewDto> GetByApplicationAsync(Guid applicationId);

    // Permission: Admissions.Interviews.Schedule (view upcoming for scheduling)
    [AbpAuthorize(PermissionNames.Admissions_Interviews_Schedule)]
    Task<List<AdmissionInterviewDto>> GetUpcomingAsync(DateTime fromDate, DateTime toDate);

    // Permission: Admissions.Interviews.Schedule (ADM-012: max 2 reschedules)
    [AbpAuthorize(PermissionNames.Admissions_Interviews_Schedule)]
    Task<AdmissionInterviewDto> RescheduleAsync(Guid id, RescheduleInterviewDto input);

    // Permission: Admissions.Interviews.Conduct
    [AbpAuthorize(PermissionNames.Admissions_Interviews_Conduct)]
    Task<AdmissionInterviewDto> CompleteAsync(Guid id, CompleteInterviewDto input);  // ADM-013

    // Permission: Admissions.Interviews.Conduct
    [AbpAuthorize(PermissionNames.Admissions_Interviews_Conduct)]
    Task<AdmissionInterviewDto> MarkNoShowAsync(Guid id);  // ADM-013

    // Permission: Admissions.Interviews.Schedule
    [AbpAuthorize(PermissionNames.Admissions_Interviews_Schedule)]
    Task<AdmissionInterviewDto> CancelAsync(Guid id, string reason);

    // Permission: Admissions.Interviews.Schedule
    [AbpAuthorize(PermissionNames.Admissions_Interviews_Schedule)]
    Task<List<TimeSlotDto>> GetAvailableTimeSlotsAsync(DateTime date, long interviewerId);

    // Permission: Admissions.Applications.View
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<bool> IsInterviewRequiredAsync(Guid applicationId);  // ADM-011
}
```

### Key Business Rules Enforcement (ADM-011, ADM-012, ADM-013)
1. **ADM-011**: Interview required based on AdmissionSettings (grade-specific)
2. **ADM-012**: Schedule at least 48 hours in advance
3. **ADM-012**: Duration: 30-60 minutes
4. **ADM-012**: Interviewer must have `Admissions.Interviews.Conduct` permission
5. **ADM-012**: Can be In-person or Online (meeting link required)
6. **ADM-012**: Maximum 2 reschedules, then counts as NoShow
7. **ADM-013**: If Completed: rating (1-5) and recommendation (Yes/No) required
8. **ADM-013**: Notes optional (max 2000 characters)

---

## 6. AdmissionAssessmentAppService

### Purpose
Manage placement assessments/tests. Enforces ADM-014 through ADM-016.

### Role Access Matrix
| Method | Admin | Principal | VP | HOD | Teacher | Admissions Officer |
|--------|:-----:|:---------:|:--:|:---:|:-------:|:------------------:|
| View | ❌ | ✅ | ✅ | ✅ | 📚 | ✅ |
| Schedule | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ |
| Conduct | ❌ | ✅ | ✅ | ✅ | ✅ | ❌ |
| Record Results | ❌ | ✅ | ✅ | ✅ | ✅ | ❌ |
| Cancel | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ |

### Interface: IAdmissionAssessmentAppService

```csharp
public interface IAdmissionAssessmentAppService : IApplicationService
{
    // Permission: Admissions.Assessments.Schedule
    [AbpAuthorize(PermissionNames.Admissions_Assessments_Schedule)]
    Task<AdmissionAssessmentDto> ScheduleAsync(ScheduleAssessmentDto input);  // ADM-015

    // Permission: Admissions.Applications.View
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<AdmissionAssessmentDto> GetAsync(Guid id);

    // Permission: Admissions.Applications.View
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<AdmissionAssessmentDto> GetByApplicationAsync(Guid applicationId);

    // Permission: Admissions.Assessments.Schedule
    [AbpAuthorize(PermissionNames.Admissions_Assessments_Schedule)]
    Task<List<AdmissionAssessmentDto>> GetScheduledAsync(DateTime date);

    // Permission: Admissions.Assessments.RecordResults
    [AbpAuthorize(PermissionNames.Admissions_Assessments_RecordResults)]
    Task<AdmissionAssessmentDto> RecordResultsAsync(Guid id, RecordAssessmentResultsDto input);  // ADM-016

    // Permission: Admissions.Assessments.Schedule
    [AbpAuthorize(PermissionNames.Admissions_Assessments_Schedule)]
    Task<AdmissionAssessmentDto> CancelAsync(Guid id, string reason);

    // Permission: Admissions.Applications.View
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<bool> IsAssessmentRequiredAsync(Guid applicationId);  // ADM-014

    // Permission: Admissions.Assessments.Schedule
    [AbpAuthorize(PermissionNames.Admissions_Assessments_Schedule)]
    Task<bool> CanRetakeAssessmentAsync(Guid applicationId);  // ADM-016: once, after 14 days
}
```

### Key Business Rules Enforcement (ADM-014, ADM-015, ADM-016)
1. **ADM-014**: Assessment required based on AdmissionSettings (grade-specific)
2. **ADM-015**: Schedule at least 7 days in advance
3. **ADM-015**: School hours only, Monday-Friday
4. **ADM-015**: Subjects defined as JSON array
5. **ADM-015**: Assessor must have `Admissions.Assessments.Conduct` permission
6. **ADM-016**: Total score and max score required
7. **ADM-016**: Pass mark configurable per grade (50% Grades 2-7, 40% Grades 8-12)
8. **ADM-016**: Feedback required (max 1000 characters)
9. **ADM-016**: Retake allowed once, 14 days gap

---

## 7. WaitlistAppService

### Purpose
Manage waitlist queue and offers. Enforces ADM-021 through ADM-024.

### Role Access Matrix
| Method | Admin | Principal | VP | Admissions Officer | Applicant |
|--------|:-----:|:---------:|:--:|:------------------:|:---------:|
| View Own | ✅ | ✅ | ✅ | ✅ | 🔒 Own |
| View All | ✅ | ✅ | ✅ | ✅ | ❌ |
| Offer Position | ✅ | ✅ | ✅ | ✅ | ❌ |
| Accept Offer | ❌ | ❌ | ❌ | ❌ | 🔒 Own |
| Decline Offer | ❌ | ❌ | ❌ | ❌ | 🔒 Own |
| Withdraw | ✅ | ✅ | ✅ | ✅ | 🔒 Own |

### Interface: IWaitlistAppService

```csharp
public interface IWaitlistAppService : IApplicationService
{
    // Permission: Admissions.Waitlist.View
    [AbpAuthorize(PermissionNames.Admissions_Waitlist_View)]
    Task<WaitlistDto> GetAsync(Guid id);

    // Permission: Admissions.Waitlist.View (Applicant can view own)
    [AbpAuthorize(PermissionNames.Admissions_Waitlist_View)]
    Task<WaitlistDto> GetByApplicationAsync(Guid applicationId);

    // Permission: Admissions.Waitlist.View (ViewAll for full grade list)
    [AbpAuthorize(PermissionNames.Admissions_Waitlist_View)]
    Task<List<WaitlistDto>> GetByGradeAsync(Guid gradeId, Guid academicYearId);

    // Permission: Admissions.Waitlist.View (Applicant can view own position)
    [AbpAuthorize(PermissionNames.Admissions_Waitlist_View)]
    Task<WaitlistPositionDto> GetPositionAsync(Guid applicationId);  // ADM-021

    // Permission: Admissions.Waitlist.OfferPosition
    [AbpAuthorize(PermissionNames.Admissions_Waitlist_OfferPosition)]
    Task<WaitlistDto> OfferPositionAsync(Guid id);  // ADM-022

    // Permission: Admissions.Applications.Edit (Applicant accepting own offer)
    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    Task<WaitlistDto> AcceptOfferAsync(Guid id);  // ADM-023

    // Permission: Admissions.Applications.Edit (Applicant declining own offer)
    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    Task<WaitlistDto> DeclineOfferAsync(Guid id);  // ADM-023

    // Permission: Admissions.Applications.Withdraw
    [AbpAuthorize(PermissionNames.Admissions_Applications_Withdraw)]
    Task<WaitlistDto> WithdrawAsync(Guid id);  // ADM-023

    // No permission - background job
    Task ProcessExpiredOffersAsync();  // ADM-023: Background job, 7 day expiry

    // No permission - background job
    Task ProcessAnnualExpiryAsync();  // ADM-024: December 31 expiry

    // Permission: Admissions.Waitlist.OfferPosition
    [AbpAuthorize(PermissionNames.Admissions_Waitlist_OfferPosition)]
    Task PromoteNextInQueueAsync(Guid gradeId, Guid academicYearId);  // ADM-022
}
```

### Key Business Rules Enforcement (ADM-021, ADM-022, ADM-023, ADM-024)
1. **ADM-021**: Position based on submission date (FIFO)
2. **ADM-021**: Sequential per grade (1, 2, 3...)
3. **ADM-021**: Cannot manually reorder
4. **ADM-022**: Auto-offer when position becomes available
5. **ADM-022**: Only 1 applicant offered at a time per position
6. **ADM-022**: Offer expiry: 7 days (shorter than regular)
7. **ADM-023**: Accept → Application status to Approved
8. **ADM-023**: Decline/Expire → Next applicant offered
9. **ADM-024**: Annual expiry on December 31

---

## 8. AdmissionSettingsAppService

### Purpose
Configure admission settings per academic year and grade. Used by all other services.

### Role Access Matrix
| Method | Admin | Principal | VP | Admissions Officer |
|--------|:-----:|:---------:|:--:|:------------------:|
| View | ✅ | ✅ | ✅ | ✅ |
| Create | ✅ | ✅ | ❌ | ❌ |
| Update | ✅ | ✅ | ❌ | ❌ |
| Delete | ✅ | ✅ | ❌ | ❌ |
| Open/Close | ✅ | ✅ | ❌ | ❌ |
| Update Capacity | ⚙️ | ✅ | ❌ | ❌ |

### Interface: IAdmissionSettingsAppService

```csharp
public interface IAdmissionSettingsAppService : IApplicationService
{
    // Permission: Admissions.Settings.Manage
    [AbpAuthorize(PermissionNames.Admissions_Settings_Manage)]
    Task<AdmissionSettingsDto> CreateAsync(CreateAdmissionSettingsDto input);

    // Permission: Admissions.Applications.View (settings visible for application context)
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<AdmissionSettingsDto> GetAsync(Guid id);

    // Permission: Admissions.Applications.View
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<AdmissionSettingsDto> GetByGradeAndYearAsync(Guid? gradeId, Guid academicYearId);

    // Permission: Admissions.Applications.ViewAll
    [AbpAuthorize(PermissionNames.Admissions_Applications_ViewAll)]
    Task<List<AdmissionSettingsDto>> GetByAcademicYearAsync(Guid academicYearId);

    // Permission: Admissions.Settings.Manage
    [AbpAuthorize(PermissionNames.Admissions_Settings_Manage)]
    Task<AdmissionSettingsDto> UpdateAsync(Guid id, UpdateAdmissionSettingsDto input);

    // Permission: Admissions.Settings.Manage
    [AbpAuthorize(PermissionNames.Admissions_Settings_Manage)]
    Task DeleteAsync(Guid id);

    // Permission: Admissions.Settings.Manage
    [AbpAuthorize(PermissionNames.Admissions_Settings_Manage)]
    Task<AdmissionSettingsDto> OpenApplicationsAsync(Guid id);

    // Permission: Admissions.Settings.Manage
    [AbpAuthorize(PermissionNames.Admissions_Settings_Manage)]
    Task<AdmissionSettingsDto> CloseApplicationsAsync(Guid id);

    // Permission: Admissions.Applications.ViewAll
    [AbpAuthorize(PermissionNames.Admissions_Applications_ViewAll)]
    Task<CapacityStatusDto> GetCapacityStatusAsync(Guid gradeId, Guid academicYearId);  // ADM-028

    // Permission: Admissions.Enrollment.Complete (incremented during enrollment)
    [AbpAuthorize(PermissionNames.Admissions_Enrollment_Complete)]
    Task IncrementEnrolledCountAsync(Guid gradeId, Guid academicYearId);  // ADM-028

    // Permission: Admissions.Settings.Manage (decrement for withdrawals)
    [AbpAuthorize(PermissionNames.Admissions_Settings_Manage)]
    Task DecrementEnrolledCountAsync(Guid gradeId, Guid academicYearId);  // ADM-028
}
```

### Key Business Rules Enforcement
1. Default settings for all grades if GradeId is null
2. Grade-specific settings override defaults
3. Only users with `Admissions.Settings.Manage` can modify settings
4. **ADM-028**: Capacity tracking and enforcement

---

## 9. EnrollmentAppService

### Purpose
Handle the enrollment process after application approval. Enforces ADM-025 through ADM-027, ADM-029.

### Role Access Matrix
| Method | Admin | Principal | VP | Admissions Officer | Applicant |
|--------|:-----:|:---------:|:--:|:------------------:|:---------:|
| View Status | ✅ | ✅ | ✅ | ✅ | 🔒 Own |
| Accept Offer | ❌ | ❌ | ❌ | ❌ | 🔒 Own |
| Submit Forms | ✅ | ✅ | ✅ | ✅ | 🔒 Own |
| Assign Class | ✅ | ✅ | ✅ | ✅ | ❌ |
| Complete Enrollment | ✅ | ✅ | ✅ | ✅ | ❌ |

### Interface: IEnrollmentAppService

```csharp
public interface IEnrollmentAppService : IApplicationService
{
    // Permission: Admissions.Applications.View (Applicant can view own status)
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<EnrollmentStatusDto> GetEnrollmentStatusAsync(Guid applicationId);  // ADM-025

    // Permission: Admissions.Applications.Edit (Applicant accepting own offer)
    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    Task<EnrollmentStatusDto> AcceptOfferAsync(Guid applicationId);  // Parent accepts admission

    // Permission: Admissions.Applications.Edit (Applicant submitting own forms)
    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    Task<EnrollmentStatusDto> SubmitEnrollmentFormAsync(Guid applicationId, CompleteEnrollmentDto input);  // ADM-025

    // Permission: Financial.Payments.View (staff confirms payment)
    [AbpAuthorize(PermissionNames.Financial_Payments_View)]
    Task<EnrollmentStatusDto> ConfirmRegistrationFeePaidAsync(Guid applicationId);  // ADM-025

    // Permission: Admissions.Applications.Edit
    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    Task<EnrollmentStatusDto> SubmitMedicalFormsAsync(Guid applicationId);  // ADM-025

    // Permission: Admissions.Applications.Edit
    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    Task<EnrollmentStatusDto> SignEnrollmentContractAsync(Guid applicationId);  // ADM-025

    // Permission: Admissions.Applications.Edit
    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    Task<EnrollmentStatusDto> SignPOPIAConsentAsync(Guid applicationId);  // ADM-025

    // Permission: Admissions.Enrollment.AssignClass
    [AbpAuthorize(PermissionNames.Admissions_Enrollment_AssignClass)]
    Task<ClassAssignmentDto> AssignClassAsync(Guid applicationId, Guid classId);  // ADM-029

    // Permission: Admissions.Enrollment.AssignClass
    [AbpAuthorize(PermissionNames.Admissions_Enrollment_AssignClass)]
    Task<ClassAssignmentDto> AutoAssignClassAsync(Guid applicationId);  // ADM-029: balanced distribution

    // Permission: Admissions.Enrollment.Complete
    [AbpAuthorize(PermissionNames.Admissions_Enrollment_Complete)]
    Task<ApplicationDto> CompleteEnrollmentAsync(Guid applicationId);  // ADM-026: creates Student

    // Permission: Admissions.Applications.View
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    Task<bool> CanCompleteEnrollmentAsync(Guid applicationId);  // ADM-025: all steps done?
}
```

### Key Business Rules Enforcement (ADM-025, ADM-026, ADM-027, ADM-029)
1. **ADM-025**: Enrollment checklist:
   - Offer accepted
   - Registration fee paid
   - Enrollment form completed
   - Medical forms submitted
   - Enrollment contract signed
   - POPIA consent signed
2. **ADM-026**: Student record creation:
   - Personal details from application
   - Parent relationships linked
   - Grade and class assignment
   - Admission number generated
   - Fee structure assigned
   - Welcome email sent
3. **ADM-027**: Application record retained (not deleted)
4. **ADM-029**: Class assignment based on capacity, needs, sibling, gender balance

---

## Permissions Reference

All permissions are defined in `PermissionNames` class. See [PSMS-Permissions-Matrix.md](./PSMS-Permissions-Matrix.md) for the complete matrix.

### Admissions Module Permissions Summary

| Permission | Description | Roles |
|------------|-------------|-------|
| `Admissions.Applications.View` | View applications | Admin, Principal, VP, AdmissionsOfficer, Applicant (own) |
| `Admissions.Applications.ViewAll` | View all applications | Admin, Principal, VP, AdmissionsOfficer |
| `Admissions.Applications.Create` | Create applications | Admin, Principal, AdmissionsOfficer, Applicant |
| `Admissions.Applications.Edit` | Edit applications | Admin, Principal, AdmissionsOfficer, Applicant (own draft) |
| `Admissions.Applications.Delete` | Delete applications | Admin, Principal |
| `Admissions.Applications.Submit` | Submit applications | Admin, Principal, AdmissionsOfficer, Applicant (own) |
| `Admissions.Applications.Withdraw` | Withdraw applications | Admin, Principal, AdmissionsOfficer, Applicant (own) |
| `Admissions.Review.StartReview` | Start application review | Admin, Principal, VP, AdmissionsOfficer |
| `Admissions.Review.RequestDocuments` | Request additional documents | Admin, Principal, VP, AdmissionsOfficer |
| `Admissions.Decision.Approve` | Approve applications | Principal, VP (delegated), Admin (delegated) |
| `Admissions.Decision.Reject` | Reject applications | Principal, VP (delegated), Admin (delegated) |
| `Admissions.Decision.Waitlist` | Waitlist applications | Principal, VP (delegated), Admin (delegated) |
| `Admissions.Decision.ExtendOffer` | Extend offer expiry | Admin, Principal, VP |
| `Admissions.Documents.Upload` | Upload documents | Admin, Principal, AdmissionsOfficer, Applicant (own) |
| `Admissions.Documents.Verify` | Verify documents | Admin, Principal, AdmissionsOfficer |
| `Admissions.Documents.Download` | Download documents | Admin, Principal, AdmissionsOfficer, Applicant (own) |
| `Admissions.Interviews.Schedule` | Schedule interviews | Admin, Principal, VP, AdmissionsOfficer |
| `Admissions.Interviews.Conduct` | Conduct interviews | Admin, Principal, VP, HOD, AdmissionsOfficer |
| `Admissions.Assessments.Schedule` | Schedule assessments | Admin, Principal, VP, AdmissionsOfficer |
| `Admissions.Assessments.Conduct` | Conduct assessments | Principal, VP, HOD, Teacher |
| `Admissions.Assessments.RecordResults` | Record assessment results | Principal, VP, HOD, Teacher |
| `Admissions.Waitlist.View` | View waitlist | Admin, Principal, VP, AdmissionsOfficer, Applicant (own) |
| `Admissions.Waitlist.OfferPosition` | Offer waitlist position | Admin, Principal, VP, AdmissionsOfficer |
| `Admissions.Enrollment.AssignClass` | Assign class to student | Admin, Principal, VP, AdmissionsOfficer |
| `Admissions.Enrollment.Complete` | Complete enrollment | Admin, Principal, VP, AdmissionsOfficer |
| `Admissions.Settings.Manage` | Manage admission settings | Admin, Principal |

---

## Domain Events (for cross-module communication)

```csharp
// Application Lifecycle Events
public class ApplicationSubmittedEvent : EventData { public Guid ApplicationId; }
public class ApplicationFeePaidEvent : EventData { public Guid ApplicationId; public decimal Amount; }
public class DocumentsVerifiedEvent : EventData { public Guid ApplicationId; }
public class InterviewScheduledEvent : EventData { public Guid ApplicationId; public DateTime ScheduledDate; }
public class InterviewCompletedEvent : EventData { public Guid ApplicationId; public int Rating; public bool Recommended; }
public class AssessmentScheduledEvent : EventData { public Guid ApplicationId; public DateTime ScheduledDate; }
public class AssessmentCompletedEvent : EventData { public Guid ApplicationId; public decimal Score; public bool Passed; }
public class ApplicationApprovedEvent : EventData { public Guid ApplicationId; public DateTime ExpiryDate; }
public class ApplicationRejectedEvent : EventData { public Guid ApplicationId; public string Reason; }
public class ApplicationWaitlistedEvent : EventData { public Guid ApplicationId; public int Position; }
public class WaitlistPositionOfferedEvent : EventData { public Guid WaitlistId; public DateTime OfferExpiry; }
public class WaitlistOfferAcceptedEvent : EventData { public Guid WaitlistId; public Guid ApplicationId; }
public class ApplicationConvertedToStudentEvent : EventData { public Guid ApplicationId; public Guid StudentId; }
public class ApplicationWithdrawnEvent : EventData { public Guid ApplicationId; public string Reason; }
public class ApplicationExpiredEvent : EventData { public Guid ApplicationId; }
```

---

## Application Workflow State Machine (ADM-005)

```
                                    ┌─────────────────┐
                                    │     Draft       │
                                    └────────┬────────┘
                                             │ Submit() [ADM-003: min 1 parent]
                                             ▼
                                    ┌─────────────────┐
                                    │ PaymentPending  │
                                    └────────┬────────┘
                                             │ MarkPaymentReceived() [ADM-006]
                                             ▼
                                    ┌─────────────────┐
                                    │  Under Review   │◄────────────────────────┐
                                    └────────┬────────┘                         │
                                             │                                  │
                    ┌────────────────────────┼────────────────────────┐         │
                    │                        │                        │         │
                    ▼                        ▼                        ▼         │
           ┌────────────────┐     ┌────────────────┐     ┌────────────────┐     │
           │ Documents      │     │ Interview      │     │ Assessment     │     │
           │ Required       │     │ Scheduled      │     │ Scheduled      │     │
           │ [ADM-008]      │     │ [ADM-012]      │     │ [ADM-015]      │     │
           └───────┬────────┘     └───────┬────────┘     └────────┬───────┘     │
                   │                      │                       │             │
                   │                      │                       │             │
                   └──────────────────────┼───────────────────────┘             │
                            MarkDocumentsComplete() │ CompleteInterview()        │
                                          │ CompleteAssessment()                │
                                          ▼                                     │
                                 ┌─────────────────┐                            │
                                 │Under            │                            │
                                 │Consideration    │                            │
                                 │[ADM-017]        │                            │
                                 └────────┬────────┘                            │
                        ┌─────────────────┼─────────────────┐                   │
                        ▼                 ▼                 ▼                   │
               ┌─────────────┐   ┌─────────────┐   ┌─────────────┐              │
               │  Approved   │   │  Rejected   │   │ Waitlisted  │──────────────┘
               │  [ADM-018]  │   │  [ADM-020]  │   │  [ADM-021]  │   (when position
               └──────┬──────┘   └─────────────┘   └─────────────┘    becomes available)
                      │
                      │ MarkAsEnrolled() [ADM-025, ADM-026]
                      ▼
               ┌─────────────┐
               │  Enrolled   │ ──► Creates Student [ADM-026]
               └─────────────┘

    Any State (except Enrolled) ──► Withdrawn [ADM-031]
    Any State (except Enrolled, Withdrawn, Expired) ──► Expired [ADM-030: 12 months]
    Approved ──► Expired [ADM-018: offer expiry]
```

---

## Implementation Order

### Phase 1: Core Setup (Foundation)
1. Exception Codes (`AdmissionsExceptionCodes.cs`)
2. Register permissions in `psmsAuthorizationProvider.cs`
3. AutoMapper Profile (`AdmissionsMapper.cs`)
4. Base DTOs (all Dto classes)

### Phase 2: Settings & Foundation
5. `AdmissionSettingsAppService` - Required by all other services

### Phase 3: Main Application Flow
6. `ApplicationAppService` (CRUD + Submit only)
7. `ApplicantParentAppService`
8. `ApplicationDocumentAppService`
9. `ApplicationFeeAppService`

### Phase 4: Review Process
10. `ApplicationAppService` (Review workflow methods)
11. `AdmissionInterviewAppService`
12. `AdmissionAssessmentAppService`

### Phase 5: Decision & Waitlist
13. `ApplicationAppService` (Approve/Reject/Waitlist methods)
14. `WaitlistAppService`

### Phase 6: Enrollment
15. `EnrollmentAppService`
16. `ApplicationAppService` (MarkAsEnrolled method)

### Phase 7: Background Jobs & Notifications
17. Expired offers handler (ADM-018, ADM-023)
18. Application auto-expiry handler (ADM-030)
19. Waitlist annual expiry handler (ADM-024)
20. Email notifications (interview reminders, offer expiry warnings, etc.)
21. Domain event handlers

---

## Notes

- All services should inherit from `ApplicationService` or `AsyncCrudAppService<>`
- Use `IRepository<TEntity, TKey>` for data access
- Implement proper authorization checks using `[AbpAuthorize(PermissionNames.xxx)]`
- Use `IUnitOfWorkManager` for transaction management
- Emit domain events for cross-module communication
- Use specification pattern for complex queries
- All monetary values in ZAR (South African Rand)
- Date handling: Use UTC internally, display in SAST (UTC+2)
- SA ID validation using `SAIdNumberValidator` from `psms.Domain.Shared.Validators`

---

## Testing Considerations

- Unit tests for each business rule (ADM-001 through ADM-031)
- Integration tests for workflow transitions
- Permission-based authorization tests
- SA ID validation edge cases (use `SAIdNumberValidator` tests)
- Age-grade validation matrix
- Capacity limit enforcement tests
- Waitlist FIFO ordering tests
- Expiry handling tests (offer, waitlist, application)

---

**Document Version**: 2.0
**Last Updated**: 2026-01-29
**Related Documents**:
- [PSMS-Permissions-Matrix.md](./PSMS-Permissions-Matrix.md)
- [PSMS-Business-Rules-ADMISSIONS.md](./PSMS-Business-Rules-ADMISSIONS.md)
