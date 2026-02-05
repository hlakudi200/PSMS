# PSMS Admissions Module Business Rules
## South African Private School Management System

**Version**: 1.0
**Last Updated**: 2026-01-27
**Framework**: ASP.NET Boilerplate (ABP)
**Context**: South African Private Schools - School Application Process

---

## Table of Contents

1. [Application Submission Rules](#1-application-submission-rules)
2. [Application Fee Rules](#2-application-fee-rules)
3. [Document Verification Rules](#3-document-verification-rules)
4. [Interview Management Rules](#4-interview-management-rules)
5. [Assessment Rules](#5-assessment-rules)
6. [Admission Decision Rules](#6-admission-decision-rules)
7. [Waitlist Management Rules](#7-waitlist-management-rules)
8. [Application-to-Student Conversion Rules](#8-application-to-student-conversion-rules)
9. [Capacity Management Rules](#9-capacity-management-rules)
10. [Application Expiry Rules](#10-application-expiry-rules)

---

## 1. Application Submission Rules

### ADM-001: Application Number Format
**Rule**: Each application must have a unique sequential number per tenant per year.

**Validations**:
- Format: `APP-{TenantId}-{Year}-{SequentialNumber}`
- Example: `APP-001-2026-00123`
- Sequential number resets each calendar year
- Must be unique within tenant
- Auto-generated on application creation
- Immutable after generation

**Exception Code**: `INVALID_APPLICATION_NUMBER`

---

### ADM-002: Prospective Student Basic Information
**Rule**: Minimum required information must be provided for the prospective student.

**Validations**:
- First name required (2-50 characters)
- Last name required (2-50 characters)
- Date of birth required (student must be between 4-19 years old for Grades R-12)
- SA citizens must provide valid SA ID number (Luhn algorithm validation)
- Non-SA citizens must provide passport number
- Cannot apply for grade inappropriate for age (±2 years tolerance)

**Exception Code**: `INVALID_PROSPECTIVE_STUDENT_INFO`

---

### ADM-003: At Least One Parent Required
**Rule**: Application must have at least one parent/guardian with complete information.

**Validations**:
- Minimum 1 parent, maximum 4 parents/guardians
- At least one parent must be marked as primary contact
- At least one parent must be marked as financially responsible
- Parent must have: First name, Last name, Email, Phone, Physical address
- SA parents must have valid SA ID number
- Primary contact email must be unique across applications (one parent, one active application)

**Exception Code**: `PARENT_INFORMATION_REQUIRED`

---

### ADM-004: Grade Selection Validation
**Rule**: Prospective student must apply for appropriate grade based on age and previous schooling.

**Validations**:
- Age-grade appropriateness (SA standard):
  - Grade R: 5-6 years old
  - Grade 1: 6-7 years old
  - Grade 2-12: Add 1 year per grade
  - Tolerance: ±2 years (special cases require approval)
- Previous school information required for Grades 1-12
- Cannot apply for grade if already enrolled in same grade at another school (unless transfer)
- Grade must be offered by the school (TenantId)

**Exception Code**: `INVALID_GRADE_SELECTION`

---

### ADM-005: Application Status Workflow
**Rule**: Application status must follow defined workflow transitions.

**Valid Transitions**:
```
Draft → Submitted
Submitted → PaymentPending
PaymentPending → UnderReview (after payment)
UnderReview → DocumentsRequired | InterviewScheduled | AssessmentScheduled | UnderConsideration
DocumentsRequired → UnderReview (after documents uploaded)
InterviewScheduled → UnderConsideration (after interview)
AssessmentScheduled → UnderConsideration (after assessment)
UnderConsideration → Approved | Rejected | Waitlisted
Approved → Enrolled (after registration completed)
Waitlisted → Approved (if position becomes available)
Any status → Withdrawn (parent action)
Any status → Expired (system action after 12 months)
```

**Invalid Transitions**:
- Cannot go from Rejected to Approved directly (must create new application)
- Cannot go from Enrolled back to any other status
- Cannot change status if application is Expired or Withdrawn

**Exception Code**: `INVALID_STATUS_TRANSITION`

---

## 2. Application Fee Rules

### ADM-006: Application Fee Requirement
**Rule**: Application fee must be paid before application review begins.

**Validations**:
- Application fee amount configured per tenant (typical: R500-R2000)
- Fee must be paid within 7 days of submission
- Payment methods: All SA payment methods supported (EFT, Card, Capitec, Ozow, etc.)
- Receipt generated automatically upon payment
- Status changes from PaymentPending → UnderReview only after successful payment
- Fee is non-refundable (even if application rejected)
- One application fee per application (no duplicates)

**Exception Code**: `APPLICATION_FEE_NOT_PAID`

---

### ADM-007: Application Fee Non-Refundable
**Rule**: Application fees are non-refundable under all circumstances.

**Validations**:
- ApplicationFee.IsRefundable = false (always)
- No refund if application rejected
- No refund if application withdrawn
- No refund if student does not accept admission offer
- Refund only in exceptional circumstances (system error, duplicate payment) with admin approval
- Refund logged in audit trail

**Exception Code**: `REFUND_NOT_ALLOWED`

---

## 3. Document Verification Rules

### ADM-008: Required Documents
**Rule**: Specific documents are required based on grade and student citizenship.

**Required for ALL Applications**:
- Birth certificate (SA students) OR Passport (non-SA students)
- ID document copies for both parents/guardians
- Passport-size photo of prospective student
- Proof of residence (utility bill, lease agreement) - not older than 3 months

**Required for Grade R ONLY**:
- Immunization record (SA Road to Health card or equivalent)

**Required for Grades 1-12**:
- Previous school report card (latest term/annual report)
- Transfer letter from previous school (if mid-year)

**Required for NON-SA Students**:
- Valid passport copy
- Study permit (if applicable)
- Proof of guardian in SA

**Exception Code**: `REQUIRED_DOCUMENTS_MISSING`

---

### ADM-009: Document Upload Validation
**Rule**: Uploaded documents must meet technical and quality requirements.

**Validations**:
- Allowed formats: PDF, JPG, JPEG, PNG
- Maximum file size: 10MB per document
- Document must be legible (manual verification by admin)
- File must pass virus scan
- Document name must include: Document type, Student name
- Multiple documents can be uploaded per category (e.g., multiple report cards)
- Documents cannot be deleted once verified (can be replaced with new version)

**Exception Code**: `INVALID_DOCUMENT_UPLOAD`

---

### ADM-010: Document Verification Process
**Rule**: All required documents must be verified by admin before admission decision.

**Validations**:
- Only users with "Admissions.VerifyDocuments" permission can verify
- Verification includes: Document matches student info, Document is authentic, Document is current/valid
- Admin can mark document as: Verified, Rejected (with reason)
- If document rejected, application status changes to DocumentsRequired
- Parent receives notification to upload correct/missing documents
- All documents must be verified before status can change to UnderConsideration
- Verification logged with admin user ID and timestamp

**Exception Code**: `DOCUMENTS_NOT_VERIFIED`

---

## 4. Interview Management Rules

### ADM-011: Interview Requirement Configuration
**Rule**: Interviews may be required or optional based on school policy and grade.

**Validations**:
- Configurable per tenant and per grade
- Grade R-3: Interview usually optional (parent interview only)
- Grades 4-12: Interview may be required (student + parent)
- If required and not completed, application cannot proceed to decision
- If optional, admin can skip interview step
- Interview requirement checked before status changes to UnderConsideration

**Exception Code**: `INTERVIEW_REQUIRED`

---

### ADM-012: Interview Scheduling
**Rule**: Interviews must be scheduled with adequate notice and appropriate interviewers.

**Validations**:
- Interview must be scheduled at least 48 hours in advance
- Interview duration: 30-60 minutes (configurable)
- Interviewer must have "Admissions.ConductInterviews" permission (Principal, Admissions Officer, Head of Department)
- Interview can be: In-person (at school) or Online (via meeting link)
- For online interviews, meeting link must be provided
- Parent receives email/SMS notification with interview details
- Reminder sent 24 hours before interview
- Interview can be rescheduled up to 2 times (after that, counts as NoShow)

**Exception Code**: `INVALID_INTERVIEW_SCHEDULE`

---

### ADM-013: Interview Completion
**Rule**: Interview must be completed and rated before application proceeds.

**Validations**:
- Interviewer must mark interview as Completed or NoShow
- If Completed, rating required (1-5 scale)
- If Completed, recommendation required (Yes/No)
- Optional: Interview notes (up to 2000 characters)
- If NoShow: Application status remains InterviewScheduled; parent contacted to reschedule
- If Completed with rating < 3 and NOT recommended: Application likely to be rejected (admin review)
- Interview completion date recorded
- Interview results considered in admission decision

**Exception Code**: `INTERVIEW_NOT_COMPLETED`

---

## 5. Assessment Rules

### ADM-014: Assessment Requirement Configuration
**Rule**: Placement assessments may be required based on grade and school policy.

**Validations**:
- Configurable per tenant and per grade
- Grade R-1: Usually no assessment
- Grades 2-5: Optional placement test (literacy, numeracy)
- Grades 6-12: Usually required (literacy, numeracy, plus other subjects)
- If required and not completed, application cannot proceed to decision
- If optional, admin can skip assessment step
- Assessment requirement checked before status changes to UnderConsideration

**Exception Code**: `ASSESSMENT_REQUIRED`

---

### ADM-015: Assessment Scheduling
**Rule**: Assessments must be scheduled with adequate preparation time.

**Validations**:
- Assessment must be scheduled at least 7 days in advance
- Assessment date: Within school hours, Monday-Friday
- Subjects to be assessed defined (JSON array: Math, English, Afrikaans, etc.)
- Duration: 1-3 hours depending on subjects
- Assessor must have "Admissions.ConductAssessments" permission (Teachers, Head of Department)
- Parent receives notification with assessment details and preparation guidelines
- Reminder sent 48 hours before assessment

**Exception Code**: `INVALID_ASSESSMENT_SCHEDULE`

---

### ADM-016: Assessment Scoring and Pass Requirements
**Rule**: Assessment must be scored and pass criteria applied.

**Validations**:
- Total score and max score required
- Percentage auto-calculated: (TotalScore / MaxScore) × 100
- Pass mark: Configurable per grade (typical: 50% for Grades 2-7, 40% for Grades 8-12)
- If student scores below pass mark, admin reviews for conditional admission or rejection
- Assessor must provide feedback (up to 1000 characters)
- Assessment results factor into admission decision
- Assessment completion date recorded
- Assessment can be retaken once (with 14 days gap) if failed

**Exception Code**: `ASSESSMENT_BELOW_PASS_MARK`

---

## 6. Admission Decision Rules

### ADM-017: Decision Authorization
**Rule**: Only authorized personnel can make admission decisions.

**Validations**:
- Only users with "Admissions.MakeDecision" permission (Principal, Admissions Director)
- Decision cannot be made if:
  - Application fee not paid
  - Required documents not verified
  - Required interview not completed
  - Required assessment not completed
- Application must be in UnderConsideration status
- Decision options: Accepted, AcceptedWithConditions, Rejected, Waitlisted

**Exception Code**: `UNAUTHORIZED_ADMISSION_DECISION`

---

### ADM-018: Acceptance Offer Expiry
**Rule**: Acceptance offers expire after defined period if not accepted.

**Validations**:
- Acceptance offer valid for 14 days (configurable per tenant)
- Expiry date set automatically upon Approval
- Parent must accept offer (complete enrollment and pay registration fee) before expiry
- Reminder sent at: 7 days remaining, 3 days remaining, 1 day remaining
- If offer expires:
  - Application status changes to Expired
  - Position may be offered to waitlisted applicant
  - Parent can re-apply (new application, new fee) if still interested
- Admin can extend expiry date (with reason) - maximum 1 extension of 7 days

**Exception Code**: `OFFER_EXPIRED`

---

### ADM-019: Conditional Acceptance
**Rule**: Acceptance with conditions must have clear conditions documented.

**Validations**:
- Conditions must be documented in Application.DecisionReason field
- Common conditions:
  - Additional tutoring/support required
  - Probationary period (first term)
  - Re-assessment after 3 months
  - Behavioral contract required
- Conditions communicated to parent in acceptance letter
- Conditions tracked and reviewed after specified period
- Failure to meet conditions may result in enrollment review

**Exception Code**: N/A

---

### ADM-020: Rejection Reasons
**Rule**: Rejections must have documented reasons for audit and compliance.

**Validations**:
- Rejection reason required (minimum 50 characters)
- Common reasons:
  - Admission capacity full
  - Assessment scores below school standards
  - Incomplete documentation (after multiple requests)
  - Previous school record concerns
  - School unable to meet student's special needs
- Reasons must be non-discriminatory (PAIA/POPIA compliance)
- Rejection reasons logged in audit trail
- Parent receives rejection letter (reasons may be summarized, not full detail)

**Exception Code**: `REJECTION_REASON_REQUIRED`

---

## 7. Waitlist Management Rules

### ADM-021: Waitlist Automatic Addition
**Rule**: When admission capacity full, applications automatically added to waitlist.

**Validations**:
- Waitlist position assigned based on application submission date (FIFO - First In First Out)
- Position is sequential per grade (1, 2, 3, ...)
- Waitlist position cannot be manually changed (automatic based on date)
- Parent notified when added to waitlist with current position
- Waitlist position updated automatically when positions become available
- Waitlist status: Active

**Exception Code**: N/A

---

### ADM-022: Waitlist Position Offer
**Rule**: When position becomes available, waitlist applicants offered in order.

**Validations**:
- Position becomes available when:
  - Accepted applicant declines offer
  - Accepted applicant's offer expires
  - Enrolled student withdraws before term starts
  - School increases capacity for grade
- Next applicant in waitlist automatically offered position
- Waitlist status changes to Offered
- Parent receives notification via email + SMS (urgent)
- Offer expiry set: 7 days (shorter than regular acceptance offer)
- Only 1 applicant offered at a time per position (not multiple simultaneously)

**Exception Code**: N/A

---

### ADM-023: Waitlist Offer Response
**Rule**: Parents must respond to waitlist offer within defined timeframe.

**Validations**:
- Parent must accept or decline offer within 7 days
- If Accepted:
  - Waitlist status changes to Accepted
  - Application status changes to Approved
  - Standard enrollment process begins (registration fee payment)
  - Other waitlisted applicants move up one position
- If Declined:
  - Waitlist status changes to Declined
  - Next applicant in waitlist automatically offered
  - Application remains in system for records
- If no response within 7 days:
  - Offer expires automatically
  - Waitlist status changes to Expired
  - Next applicant offered
- Parent can withdraw from waitlist anytime (status: Withdrawn)

**Exception Code**: `WAITLIST_OFFER_EXPIRED`

---

### ADM-024: Waitlist Annual Expiry
**Rule**: Waitlists expire at end of each enrollment period.

**Validations**:
- Waitlists for upcoming academic year expire on December 31
- Parent notified 30 days before expiry
- Parent can:
  - Re-apply for next academic year (new application)
  - Withdraw from waitlist
  - Request to remain on waitlist (if school policy allows multi-year waitlists)
- Expired waitlist applications archived
- Waitlist positions reset for new academic year

**Exception Code**: N/A

---

## 8. Application-to-Student Conversion Rules

### ADM-025: Enrollment Completion Required
**Rule**: Accepted applicant must complete enrollment process to become student.

**Validations**:
- Parent must accept admission offer
- Registration fee must be paid (separate from application fee)
- Enrollment form must be completed (additional student details)
- Medical forms must be submitted
- Uniform order placed (if applicable)
- Transport/aftercare registration (if applicable)
- Parent signs enrollment contract
- POPIA consent forms signed
- Once all completed, Application status changes to Enrolled

**Exception Code**: `ENROLLMENT_NOT_COMPLETE`

---

### ADM-026: Student Record Creation
**Rule**: Student record created only after enrollment completion.

**Validations**:
- Application.CreatedStudentId populated with new Student.Id
- Student record created with:
  - Personal details from application
  - Parent relationships linked (ApplicantParent → Parent entity created)
  - Grade and class assignment
  - Admission number generated
  - Enrollment date set
  - Student status: Active
- Fee structure assigned to student
- Student account created (if portal access enabled)
- Parent account linked to student
- Welcome email sent to parent and student

**Exception Code**: `STUDENT_CREATION_FAILED`

---

### ADM-027: Application Record Retention
**Rule**: Application records retained even after conversion to student.

**Validations**:
- Application record is NOT deleted after student creation
- Application serves as audit trail for admission process
- Application.CreatedStudentId links application to student
- Application documents retained and linked to student file
- Application fee payment retained in financial records
- Interview and assessment records retained for reference
- Retention period: Duration of student enrollment + 7 years

**Exception Code**: N/A

---

## 9. Capacity Management Rules

### ADM-028: Grade Capacity Limits
**Rule**: Each grade has maximum enrollment capacity that cannot be exceeded.

**Validations**:
- Capacity configured per grade per tenant (e.g., Grade 1: 120 students = 4 classes × 30 students)
- Capacity checked before approving application
- If capacity reached:
  - Application automatically placed on waitlist
  - Parent notified
  - Admin can override (increase capacity) with approval and reason
- Capacity includes:
  - Currently enrolled students
  - Accepted applications not yet enrolled
  - Does NOT include rejected, expired, withdrawn applications
- Capacity report available to admin

**Exception Code**: `GRADE_CAPACITY_FULL`

---

### ADM-029: Class Assignment Upon Enrollment
**Rule**: Student must be assigned to specific class within grade upon enrollment.

**Validations**:
- Class assignment based on:
  - Class capacity (typically 25-30 students per class)
  - Student needs (e.g., language of instruction)
  - Sibling in same class (if parent requests)
  - Gender balance (if school policy)
- Class assignment can be done:
  - Automatically by system (balanced distribution)
  - Manually by admin
- Class assignment can be changed before term starts
- Class assignment locked once term starts (changes require approval)

**Exception Code**: `CLASS_ASSIGNMENT_REQUIRED`

---

## 10. Application Expiry Rules

### ADM-030: Application Auto-Expiry
**Rule**: Applications expire automatically if no decision made within defined period.

**Validations**:
- Application expires after 12 months from submission date if:
  - Status is not: Approved, Rejected, Enrolled, Withdrawn, Expired
  - No decision made
  - Documents not submitted despite reminders
  - Interview/assessment not scheduled despite reminders
- Parent notified:
  - 30 days before expiry (final reminder)
  - On expiry date
- Expired applications status changes to Expired
- Application fee NOT refunded
- Parent can submit new application for next academic year
- Expired applications archived

**Exception Code**: N/A

---

### ADM-031: Withdrawal by Parent
**Rule**: Parents can withdraw application at any stage before enrollment.

**Validations**:
- Parent can request withdrawal via portal or contact admin
- Withdrawal allowed at any status except Enrolled
- Withdrawal reason optional but encouraged
- Application status changes to Withdrawn
- Application fee NOT refunded
- Waitlist position freed (if on waitlist)
- Scheduled interview/assessment cancelled
- Confirmation email sent to parent
- Withdrawn applications retained for audit

**Exception Code**: N/A

---

## Business Rule Summary Statistics

- **Total Admissions Rules**: 31
- **Critical Rules (MUST)**: 25
- **Important Rules (SHOULD)**: 6
- **Rule Categories**: 10
- **Exception Codes**: 20

---

## Integration with Core Business Rules

These admissions rules extend the core PSMS business rules and integrate with:

- **Academic Management Rules (AR-001 to AR-007)**: Grade structure, academic year
- **Enrollment Rules (ER-001 to ER-005)**: Student-Grade-Class assignment
- **Financial Rules (FM-001 to FM-005)**: Application fee, registration fee
- **POPIA Compliance (PC-001 to PC-003)**: Consent forms, data retention
- **Security & Authorization (SA-001 to SA-002)**: Permission-based decision making
- **Multi-Tenancy Rules (MT-001 to MT-003)**: Tenant isolation, capacity per school

---

## Domain Events Triggered

### Application Lifecycle Events:
- `ApplicationSubmittedEvent` - When parent submits application
- `ApplicationFeePaidEvent` - When payment received
- `DocumentsVerifiedEvent` - When all required documents verified
- `InterviewScheduledEvent` - When interview booked
- `InterviewCompletedEvent` - When interview finished
- `AssessmentScheduledEvent` - When assessment booked
- `AssessmentCompletedEvent` - When assessment finished
- `ApplicationApprovedEvent` - When admission granted
- `ApplicationRejectedEvent` - When admission denied
- `ApplicationWaitlistedEvent` - When placed on waitlist
- `WaitlistPositionOfferedEvent` - When position becomes available
- `WaitlistOfferAcceptedEvent` - When parent accepts offer
- `ApplicationConvertedToStudentEvent` - When student record created
- `ApplicationWithdrawnEvent` - When parent withdraws
- `ApplicationExpiredEvent` - When application expires

---

**End of Admissions Business Rules**
