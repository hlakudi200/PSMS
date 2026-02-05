# PSMS Supplementary Business Rules
## Addressing Requirements Gaps

**Version**: 1.0
**Last Updated**: 2026-01-27
**Framework**: ASP.NET Boilerplate (ABP)
**Context**: South African Private Schools

---

## Table of Contents

1. [Online Lessons Module Rules](#1-online-lessons-module-rules)
2. [Quiz & Assessment Platform Rules](#2-quiz--assessment-platform-rules)
3. [Learning Materials Management Rules](#3-learning-materials-management-rules)
4. [Timetable Management Rules](#4-timetable-management-rules)
5. [Excel Import Rules](#5-excel-import-rules)
6. [Document Repository Rules](#6-document-repository-rules)
7. [Payment Receipts Rules](#7-payment-receipts-rules)
8. [Notification System Rules](#8-notification-system-rules)
9. [Financial Reporting Rules](#9-financial-reporting-rules)
10. [Teacher Feedback Rules](#10-teacher-feedback-rules)
11. [Enrollment View Rules](#11-enrollment-view-rules)
12. [Dashboard Rules](#12-dashboard-rules)
13. [Fee Statement Rules](#13-fee-statement-rules)
14. [Teacher Assignment Rules](#14-teacher-assignment-rules)
15. [Non-Functional Requirements Rules](#15-non-functional-requirements-rules)

---

## 1. Online Lessons Module Rules

### OL-001: Lesson Scheduling Validation
**Rule**: Online lessons must be scheduled at least 24 hours in advance and cannot overlap with existing lessons.

**Validations**:
- Scheduled start time must be at least 24 hours from current time
- Lesson duration must be between 30 minutes and 3 hours
- Teacher cannot have overlapping scheduled lessons
- Students in the class cannot have overlapping lessons
- Lesson must be scheduled within current academic term dates
- Lesson must be scheduled during school hours (07:00 - 17:00 SA time)

**Exception Code**: `INVALID_ONLINE_LESSON_SCHEDULE`

---

### OL-002: Live Class Capacity Limits
**Rule**: Live online classes are limited by platform capacity and student enrollment.

**Validations**:
- Maximum participants per live class: 100 (configurable per tenant)
- Only enrolled students can join the live class
- Teacher must be assigned to the subject/class
- Live class link only becomes active 15 minutes before scheduled start
- Live class link expires 30 minutes after scheduled end time

**Exception Code**: `LIVE_CLASS_CAPACITY_EXCEEDED`

---

### OL-003: Lesson Recording Upload
**Rule**: Lesson recordings must meet technical specifications and security requirements.

**Validations**:
- Allowed formats: MP4, MOV, AVI, WebM
- Maximum file size: 5GB per recording
- Recording must pass virus scan before storage
- Recording must be linked to a scheduled lesson
- Recording file name must include: Subject, Grade, Date, Topic
- Recording metadata must include: Duration, Upload date, Teacher ID

**Exception Code**: `INVALID_RECORDING_UPLOAD`

---

### OL-004: Recording Access Control
**Rule**: Only authorized users can access lesson recordings based on enrollment and time restrictions.

**Validations**:
- Students can only access recordings for enrolled subjects
- Parents can access recordings for their children's subjects
- Recordings become available immediately after upload
- Recordings remain available for the duration of the academic year
- Archived recordings (previous years) require admin approval for access
- Teachers can access all recordings for subjects they teach

**Exception Code**: `RECORDING_ACCESS_DENIED`

---

### OL-005: Recording Retention Policy
**Rule**: Lesson recordings must be retained according to regulatory and storage policies.

**Validations**:
- Current academic year recordings: Retain online, full access
- Previous academic year recordings: Retain online, restricted access
- Recordings older than 2 years: Archive to cold storage
- Recordings older than 5 years: Delete unless flagged for permanent retention
- Total storage per tenant must not exceed allocated quota

**Exception Code**: `RECORDING_RETENTION_VIOLATION`

---

### OL-006: Live Class Attendance Tracking
**Rule**: Attendance for live online classes must be automatically tracked.

**Validations**:
- Student marked present if joined within 15 minutes of start time
- Student marked late if joined after 15 minutes but before 30 minutes
- Student marked absent if did not join or joined after 30 minutes
- Minimum attendance duration: 60% of lesson duration to be marked present
- Attendance records are immutable once lesson ends

**Exception Code**: `INVALID_ATTENDANCE_RECORD`

---

## 2. Quiz & Assessment Platform Rules

### QA-001: Quiz Creation Validation
**Rule**: Quizzes must meet minimum quality and structural requirements.

**Validations**:
- Minimum 5 questions per quiz, maximum 100 questions
- Each question must have at least 2 options, maximum 6 options
- Each question must have exactly one correct answer (for multiple choice)
- Question text must be between 10 and 1000 characters
- Each option text must be between 1 and 500 characters
- Quiz must be assigned to specific Grade, Subject, and Term
- Quiz must have a unique title within the subject

**Exception Code**: `INVALID_QUIZ_STRUCTURE`

---

### QA-002: Quiz Scheduling Rules
**Rule**: Quizzes must be scheduled with appropriate availability windows.

**Validations**:
- Quiz available date must be at least 24 hours after creation
- Quiz available date must be within current term
- Quiz due date must be after available date
- Availability window (available to due date) must be at least 24 hours
- Maximum availability window: 14 days
- Quiz cannot be edited once it becomes available to students
- Quiz can be scheduled for specific date/time or open-ended within window

**Exception Code**: `INVALID_QUIZ_SCHEDULE`

---

### QA-003: Quiz Attempt Rules
**Rule**: Quiz attempts must be controlled to ensure fair assessment.

**Validations**:
- Default: Students allowed 1 attempt (configurable: 1-3 attempts)
- Minimum time between attempts: 24 hours
- Time limit per attempt: Configurable (default 60 minutes)
- Auto-submit when time limit reached
- Cannot pause quiz once started
- Only highest score counts if multiple attempts allowed
- Quiz cannot be attempted before available date or after due date

**Exception Code**: `INVALID_QUIZ_ATTEMPT`

---

### QA-004: Quiz Submission Validation
**Rule**: Quiz submissions must be validated for completeness and integrity.

**Validations**:
- All questions must be answered (or explicitly skipped)
- Submission timestamp must be within quiz availability window
- Submission must be from enrolled student
- Duplicate submissions prevented (one active submission per attempt)
- Submission cannot be modified after final submit
- Submission automatically saved every 5 minutes (draft state)

**Exception Code**: `INVALID_QUIZ_SUBMISSION`

---

### QA-005: Auto-Grading Rules
**Rule**: Quiz submissions must be automatically graded with consistent scoring.

**Validations**:
- Multiple choice: 1 point per correct answer (configurable)
- All questions weighted equally (unless custom weights defined)
- Total score calculated as: (Correct answers / Total questions) × 100
- Partial credit not allowed for multiple choice
- Grade immediately available to student after submission
- Teacher can review and manually adjust grades within 48 hours

**Exception Code**: `GRADING_ERROR`

---

### QA-006: Quiz Result Release
**Rule**: Quiz results must be released according to teacher preferences and fairness rules.

**Validations**:
- Release mode options: Immediate, After due date, Manual release by teacher
- If immediate: Results shown immediately after submission
- If after due date: Results released automatically at due date + 1 hour
- If manual: Teacher must explicitly release results
- Students can view: Score, correct/incorrect answers, correct answer key (optional)
- Detailed analytics only available to teacher and admin

**Exception Code**: `RESULT_RELEASE_ERROR`

---

### QA-007: Plagiarism Detection
**Rule**: Quiz submissions must be monitored for potential academic dishonesty.

**Validations**:
- Track submission IP address
- Flag if multiple students submit from same IP within 5 minutes
- Flag if submission completed in unusually short time (< 25% of time limit)
- Flag if student switches browser tabs more than 5 times during quiz
- Flagged submissions require teacher review
- Flags are advisory only, teacher makes final decision

**Exception Code**: `PLAGIARISM_DETECTED`

---

## 3. Learning Materials Management Rules

### LM-001: File Upload Validation
**Rule**: Learning materials must meet security and quality standards.

**Validations**:
- Allowed file types: PDF, DOC, DOCX, PPT, PPTX, MP4, MP3, JPG, PNG, ZIP, URL
- Maximum file size per upload:
  - Documents (PDF, DOC, PPT): 50MB
  - Videos: 500MB
  - Images: 10MB
  - Archives (ZIP): 100MB
- All files must pass virus/malware scan
- File name must not contain special characters: `/ \ : * ? " < > |`
- Minimum file name length: 5 characters

**Exception Code**: `INVALID_FILE_UPLOAD`

---

### LM-002: Material Categorization
**Rule**: All learning materials must be properly categorized for discoverability.

**Validations**:
- Material must be assigned to: Grade, Subject, Term (all required)
- Material must have a title (5-200 characters)
- Material must have a description (10-1000 characters)
- Material type must be specified: Lesson Notes, Homework, Reading, Video, Reference
- Optional tags: Maximum 5 tags per material
- Materials can be marked as: Core (required) or Supplementary (optional)

**Exception Code**: `INVALID_MATERIAL_CATEGORIZATION`

---

### LM-003: Material Versioning
**Rule**: Updated materials must maintain version history for audit purposes.

**Validations**:
- Each material update creates a new version
- Version number increments automatically (1.0, 1.1, 1.2, etc.)
- Previous versions retained for current academic year
- Version history includes: Upload date, Uploader, Change description
- Students always see the latest version by default
- Teachers can view and restore previous versions
- Maximum 10 versions retained per material

**Exception Code**: `VERSION_LIMIT_EXCEEDED`

---

### LM-004: Duplicate Detection
**Rule**: Prevent accidental duplicate uploads of identical materials.

**Validations**:
- Check file hash (SHA-256) before upload
- If identical file exists for same Grade/Subject/Term, warn teacher
- Teacher can choose to: Link existing material, Upload as new, Create new version
- Duplicate detection within tenant only (cross-tenant not checked)

**Exception Code**: `DUPLICATE_MATERIAL_DETECTED`

---

### LM-005: Access Control
**Rule**: Learning materials must be accessible only to authorized users.

**Validations**:
- Students can only access materials for enrolled subjects
- Parents can access materials for their children's subjects
- Materials become visible based on release date (default: immediate)
- Teachers can schedule material release (future date/time)
- Materials can be hidden (archived) after term end
- Admin can access all materials across all subjects

**Exception Code**: `MATERIAL_ACCESS_DENIED`

---

### LM-006: Download Tracking
**Rule**: Material downloads must be tracked for usage analytics and compliance.

**Validations**:
- Track: User ID, Material ID, Download timestamp, IP address
- Generate download analytics per material: Views, Downloads, Unique users
- Teachers can view analytics for their materials
- Admin can view analytics across all materials
- Retention: Download logs kept for current + previous academic year

**Exception Code**: N/A (Analytics only)

---

### LM-007: Storage Quota Management
**Rule**: Storage usage must be monitored and limited per tenant and teacher.

**Validations**:
- Default tenant storage quota: 100GB (configurable)
- Default teacher storage quota: 5GB (configurable)
- Warn when teacher reaches 80% of quota
- Block uploads when quota exceeded
- Admin can view storage usage reports
- Archived materials count toward quota
- Delete materials older than 3 years to free space

**Exception Code**: `STORAGE_QUOTA_EXCEEDED`

---

## 4. Timetable Management Rules

### TT-001: Timetable Creation Validation
**Rule**: Timetables must be structurally valid and conflict-free.

**Validations**:
- Timetable must be linked to: Academic Year, Term, Grade, Class
- School day start time: 07:00 - 08:00 (configurable)
- School day end time: 13:00 - 16:00 (configurable)
- Period duration: 30-60 minutes (configurable)
- Break periods: At least 2 breaks per day (15-30 minutes each)
- Minimum periods per day: 4, Maximum: 10

**Exception Code**: `INVALID_TIMETABLE_STRUCTURE`

---

### TT-002: Period Conflict Detection - Teacher
**Rule**: Teachers cannot be double-booked in the same time slot.

**Validations**:
- Check all classes where teacher is assigned
- Validate across all grades/classes for overlapping periods
- Consider travel time between physical locations (if applicable)
- Flag conflicts during timetable creation
- Prevent timetable publication if conflicts exist

**Exception Code**: `TEACHER_PERIOD_CONFLICT`

---

### TT-003: Period Conflict Detection - Room
**Rule**: Classrooms cannot be double-booked in the same time slot.

**Validations**:
- Check all classes assigned to specific room/venue
- Validate across all grades for overlapping periods
- Special rooms (Labs, Gym) require advance booking
- Allow virtual rooms (no conflict checking)

**Exception Code**: `ROOM_PERIOD_CONFLICT`

---

### TT-004: Period Conflict Detection - Student
**Rule**: Students cannot have overlapping classes in their timetable.

**Validations**:
- Check all subjects student is enrolled in
- Validate for overlapping periods across different teachers/rooms
- Ensure lunch/break periods are consistent across all subjects
- Flag conflicts before finalizing enrollment

**Exception Code**: `STUDENT_PERIOD_CONFLICT`

---

### TT-005: Teacher Workload Validation
**Rule**: Teachers must have reasonable and compliant workloads.

**Validations**:
- Maximum teaching periods per day: 8 (SA labor law)
- Maximum teaching periods per week: 35 (SA labor law)
- Minimum 1 free period per day (non-teaching time)
- Maximum consecutive teaching periods: 4 (then break required)
- Minimum 30-minute lunch break per day

**Exception Code**: `TEACHER_WORKLOAD_EXCEEDED`

---

### TT-006: Timetable Publication
**Rule**: Timetables must be reviewed and approved before becoming active.

**Validations**:
- Timetable draft state: Editable, not visible to students/parents
- Timetable pending approval: Locked, visible to admin only
- Timetable published: Locked, visible to all stakeholders
- Published timetable can only be edited with admin approval
- Changes to published timetable trigger notifications

**Exception Code**: `TIMETABLE_NOT_PUBLISHED`

---

### TT-007: Timetable Variations
**Rule**: Temporary timetable changes must be documented and communicated.

**Validations**:
- Temporary changes allowed: Teacher substitution, Room change, Period swap
- Permanent changes require new timetable version
- All changes must include reason/notes
- Changes must be approved by admin
- Affected stakeholders automatically notified
- Changes logged in audit trail

**Exception Code**: `INVALID_TIMETABLE_CHANGE`

---

## 5. Excel Import Rules

### EI-001: Excel File Format Validation
**Rule**: Excel files must meet specific format requirements for successful import.

**Validations**:
- Allowed formats: XLSX, XLS, CSV
- Maximum file size: 10MB
- Maximum rows: 1000 per import
- Required columns must be present (validated per import type)
- Column headers must match expected names exactly (case-insensitive)
- File must not contain macros or formulas

**Exception Code**: `INVALID_EXCEL_FORMAT`

---

### EI-002: Marks Import Validation
**Rule**: Bulk mark imports must validate data integrity before committing.

**Validations**:
- Required columns: Student ID Number, Subject Code, Assessment Type, Mark, Max Mark
- Student ID must exist in tenant database
- Student must be enrolled in specified subject
- Assessment must exist and be open for marking
- Mark must be numeric and between 0 and Max Mark
- Max Mark must match assessment definition
- Duplicate entries not allowed (Student + Assessment unique)

**Exception Code**: `INVALID_MARKS_IMPORT`

---

### EI-003: Import Preview & Validation
**Rule**: All imports must show preview and validation results before commit.

**Validations**:
- Display first 10 rows as preview
- Show validation summary: Total rows, Valid rows, Invalid rows
- List all validation errors with row numbers
- Errors must be corrected before import can proceed
- Allow download of error report
- User must explicitly confirm import after preview

**Exception Code**: `IMPORT_VALIDATION_FAILED`

---

### EI-004: Import Transaction Control
**Rule**: Imports must be atomic - all or nothing.

**Validations**:
- All rows validated before any data saved
- If any row fails validation, entire import rejected
- Successful import commits all rows in single transaction
- Failed import rolls back all changes
- Import status: Pending, In Progress, Completed, Failed
- Failed imports retain error details for 30 days

**Exception Code**: `IMPORT_TRANSACTION_FAILED`

---

### EI-005: Import Audit Trail
**Rule**: All import operations must be fully auditable.

**Validations**:
- Log: User ID, Import timestamp, File name, Import type, Row count
- Store original file for 90 days
- Log all validation errors
- Log success/failure status
- Track which records were created/updated
- Audit log accessible to admin and finance roles

**Exception Code**: N/A (Audit only)

---

## 6. Document Repository Rules

### DR-001: Document Categorization
**Rule**: All documents must be categorized for proper organization and access control.

**Validations**:
- Document categories: Policy, Procedure, Form, Circular, Calendar, Curriculum, Other
- Document must have title (10-200 characters)
- Document must have description (20-500 characters)
- Document must specify target audience: All, Parents, Teachers, Students, Admin
- Optional metadata: Academic Year, Grade, Subject

**Exception Code**: `INVALID_DOCUMENT_CATEGORY`

---

### DR-002: Document Access Control
**Rule**: Documents must only be accessible to intended audience.

**Validations**:
- Public documents: Accessible to all authenticated users
- Role-based documents: Accessible based on user role
- Grade-specific documents: Accessible to students/parents of that grade only
- Subject-specific documents: Accessible to enrolled students/assigned teachers only
- Admin documents: Accessible to admin and principal roles only

**Exception Code**: `DOCUMENT_ACCESS_DENIED`

---

### DR-003: Document Versioning
**Rule**: Document updates must maintain version history.

**Validations**:
- Each document update creates new version (v1, v2, v3, etc.)
- Previous versions retained for 2 academic years
- Version history shows: Upload date, Uploader, Change summary
- Users always see latest version by default
- Admin can view and restore previous versions
- Maximum 20 versions per document

**Exception Code**: `VERSION_LIMIT_EXCEEDED`

---

### DR-004: Document Expiry
**Rule**: Documents can have expiry dates for time-sensitive information.

**Validations**:
- Expiry date optional
- Expired documents automatically hidden from users
- Expired documents retained in archive
- Admin notified 7 days before document expiry
- Admin can extend expiry date or mark as permanent
- Circulars automatically expire after 90 days (unless extended)

**Exception Code**: `DOCUMENT_EXPIRED`

---

### DR-005: Document Approval Workflow
**Rule**: Certain document types require approval before publication.

**Validations**:
- Policy documents require principal approval
- Curriculum documents require head of department approval
- Forms and circulars can be published immediately
- Pending approval documents visible to approvers only
- Approver can: Approve, Reject (with reason), Request changes
- Rejected documents returned to uploader with feedback

**Exception Code**: `DOCUMENT_APPROVAL_REQUIRED`

---

## 7. Payment Receipts Rules

### PR-001: Receipt Generation
**Rule**: Payment receipts must be generated immediately upon successful payment.

**Validations**:
- Receipt generated for all successful payments (online and manual)
- Receipt number format: `RCP-{TenantId}-{Year}-{SequentialNumber}` (e.g., RCP-001-2026-00123)
- Receipt includes: School details, Parent details, Student details, Payment details, Payment method
- Receipt timestamp matches payment timestamp
- Receipt generation failure triggers alert

**Exception Code**: `RECEIPT_GENERATION_FAILED`

---

### PR-002: Receipt Numbering Sequence
**Rule**: Receipt numbers must be unique and sequential per tenant.

**Validations**:
- Sequential numbering per tenant per year
- No gaps in sequence (except voided receipts)
- Sequence resets each academic year (January 1)
- Voided receipts marked but retain original number
- Number assignment must be thread-safe (prevent duplicates)

**Exception Code**: `RECEIPT_NUMBER_CONFLICT`

---

### PR-003: Receipt Immutability
**Rule**: Issued receipts cannot be modified, only voided with audit trail.

**Validations**:
- Receipt details cannot be edited after generation
- Incorrect receipts must be voided and new receipt issued
- Void requires reason and admin approval
- Voided receipts marked with "VOID" watermark
- Void action logged in audit trail with User ID and timestamp
- Original receipt retained in database (soft delete)

**Exception Code**: `RECEIPT_MODIFICATION_DENIED`

---

### PR-004: Receipt Retrieval & Access
**Rule**: Receipts must be accessible to authorized users for specified retention period.

**Validations**:
- Parents can access all receipts for their children
- Admin and Finance can access all receipts
- Receipt accessible immediately after payment
- Receipts retained online for 7 years (SARS requirement)
- Receipts older than 7 years archived to cold storage
- Archived receipts retrievable within 24 hours upon request

**Exception Code**: `RECEIPT_NOT_FOUND`

---

### PR-005: Receipt Template Requirements
**Rule**: Receipt layout must comply with South African invoicing regulations.

**Validations**:
- Must include: School legal name, School address, School tax number (if applicable)
- Must include: Receipt number, Date, Payment amount, Payment method
- Must include: Student name, Grade, Parent name
- Must include: Description of payment (fee type and period)
- Must show: Previous balance, Payment amount, New balance
- Optional: School logo, Bank details, Terms and conditions

**Exception Code**: `INVALID_RECEIPT_TEMPLATE`

---

### PR-006: Receipt Delivery
**Rule**: Receipts must be delivered to parents through multiple channels.

**Validations**:
- Auto-send receipt via email immediately after payment
- Receipt downloadable from parent portal (PDF)
- Receipt accessible via mobile app
- Email delivery failure triggers alert
- Store email delivery status (Sent, Delivered, Failed, Opened)
- Resend option available for failed deliveries

**Exception Code**: `RECEIPT_DELIVERY_FAILED`

---

## 8. Notification System Rules

### NT-001: Notification Trigger Events
**Rule**: System must send notifications for defined events automatically.

**Validations**:
- Trigger events include:
  - New report published
  - Payment received
  - Payment overdue
  - New announcement posted
  - New learning material uploaded
  - Quiz available
  - Attendance alert (absence)
  - Grade updated
  - Timetable changed
- Each event type has default notification template
- Notifications sent in real-time (within 5 minutes of event)

**Exception Code**: N/A (System automatic)

---

### NT-002: Notification Channels
**Rule**: Notifications must be delivered through user-preferred channels.

**Validations**:
- Available channels: Email, SMS, In-app notification, Push notification
- Default channels per event type configurable by admin
- Users can set channel preferences in profile
- Critical notifications (payment overdue) cannot be disabled
- In-app notifications always enabled
- Email and SMS respect opt-out preferences

**Exception Code**: N/A

---

### NT-003: Notification Preferences
**Rule**: Users must be able to control their notification preferences.

**Validations**:
- Per-user preferences for each event type
- Preferences include: Enable/disable, Channel selection, Frequency (immediate/daily digest)
- Parents can set separate preferences per child
- Teachers can set preferences per class/subject
- System notifications (security, account) cannot be disabled
- Default preferences applied for new users

**Exception Code**: N/A

---

### NT-004: Notification Retry Logic
**Rule**: Failed notification deliveries must be retried with exponential backoff.

**Validations**:
- Email delivery failure: Retry 3 times (5 min, 15 min, 60 min intervals)
- SMS delivery failure: Retry 2 times (10 min, 30 min intervals)
- Push notification failure: Retry 1 time (5 min interval)
- After all retries failed, mark as failed and alert admin
- Failed notifications logged for troubleshooting
- Manual resend option available

**Exception Code**: `NOTIFICATION_DELIVERY_FAILED`

---

### NT-005: Notification Rate Limiting
**Rule**: Prevent notification spam by implementing rate limits.

**Validations**:
- Maximum emails per user: 20 per day
- Maximum SMS per user: 10 per day
- Maximum push notifications per user: 50 per day
- Batch similar notifications (e.g., multiple announcements in 1 email)
- Critical notifications exempt from rate limits
- Rate limit reset daily at midnight

**Exception Code**: `NOTIFICATION_RATE_LIMIT_EXCEEDED`

---

### NT-006: Notification Opt-Out
**Rule**: Users must be able to opt out of non-critical notifications.

**Validations**:
- Opt-out link included in all emails
- Opt-out applies to specific notification type or all non-critical
- Cannot opt out of: Payment receipts, Legal notices, Security alerts
- Opt-out status persists across sessions
- Users can re-enable notifications anytime
- Admin can view opt-out statistics

**Exception Code**: N/A

---

### NT-007: Notification History
**Rule**: All sent notifications must be logged and accessible to users.

**Validations**:
- Notification history visible in user profile
- Shows: Date/time, Event type, Subject/title, Channel, Status (sent/failed)
- Resend option for failed notifications
- Mark as read/unread for in-app notifications
- History retained for current + previous academic year
- Notification count badge on user menu

**Exception Code**: N/A

---

## 9. Financial Reporting Rules

### FR-001: Report Generation Schedule
**Rule**: Financial reports must be generated automatically on defined schedules.

**Validations**:
- Daily reports: Outstanding fees, Payments received, Generate at 23:00 daily
- Weekly reports: Payment collections, Outstanding by grade, Generate Sunday 23:00
- Monthly reports: Income statement, Aged debtors, Generate on last day of month
- Term reports: Fee collection summary, Generate on last day of term
- Annual reports: Full financial year summary, Generate Dec 31
- On-demand reports available anytime

**Exception Code**: N/A

---

### FR-002: Report Data Accuracy
**Rule**: Financial reports must reflect accurate, real-time data with clear cut-off times.

**Validations**:
- Data cut-off time clearly stated on report
- All transactions up to cut-off time included
- Pending transactions excluded from reports
- Reconciled payments only
- Currency amounts to 2 decimal places
- Totals must balance (Assets = Liabilities + Equity concept)

**Exception Code**: `REPORT_DATA_INCONSISTENCY`

---

### FR-003: Outstanding Fees Report
**Rule**: Outstanding fees must be accurately tracked and aged.

**Validations**:
- Group by: Grade, Class, Student
- Show: Total fees, Paid amount, Outstanding amount
- Age categories: Current, 30 days, 60 days, 90+ days
- Highlight overdue accounts (>30 days)
- Include: Student name, Parent contact, Last payment date
- Exclude: Withdrawn students (unless outstanding balance)

**Exception Code**: N/A

---

### FR-004: Income Report
**Rule**: Income reports must categorize all revenue streams accurately.

**Validations**:
- Categories: Tuition fees, Registration fees, Extramural fees, Transport fees, Other income
- Show: Per category, Per grade, Per payment method
- Time period: Daily, Weekly, Monthly, Term, Annual
- Include: Payment date, Receipt number, Amount, Payment method
- Exclude: Voided payments
- Show YTD (Year-to-Date) totals

**Exception Code**: N/A

---

### FR-005: Report Access Control
**Rule**: Financial reports must be restricted to authorized personnel only.

**Validations**:
- Access roles: Admin, Principal, Finance Manager, Accountant
- Teachers cannot access financial reports
- Parents can only view their own fee statements
- Audit log for all report access (who viewed what when)
- Reports can be exported to: PDF, Excel, CSV
- Export actions logged in audit trail

**Exception Code**: `REPORT_ACCESS_DENIED`

---

### FR-006: Report Retention
**Rule**: Financial reports must be retained according to regulatory requirements.

**Validations**:
- Retain online for 7 years (SARS requirement)
- Archive reports older than 2 years to cold storage
- Archived reports retrievable within 24 hours
- Permanent retention for annual reports
- Deletion requires legal/compliance approval
- Backup copies in disaster recovery site

**Exception Code**: N/A

---

## 10. Teacher Feedback Rules

### TF-001: Feedback Character Limits
**Rule**: Teacher feedback must be concise yet meaningful.

**Validations**:
- Minimum characters: 20 (ensures meaningful feedback)
- Maximum characters: 2000 (keeps it focused)
- Warn teacher at 1800 characters (90% limit)
- Allow line breaks and basic formatting
- Strip HTML/script tags (security)

**Exception Code**: `INVALID_FEEDBACK_LENGTH`

---

### TF-002: Feedback Professionalism
**Rule**: Feedback must maintain professional standards and appropriate language.

**Validations**:
- Scan for profanity/inappropriate language
- Flag potentially problematic feedback for admin review
- Feedback cannot contain: Personal contact information, External links, Discriminatory language
- Feedback must be constructive (no purely negative comments allowed without suggestions)
- Admin can edit/remove inappropriate feedback

**Exception Code**: `INAPPROPRIATE_FEEDBACK`

---

### TF-003: Feedback Edit & Delete Rules
**Rule**: Feedback can be edited within limits to maintain integrity.

**Validations**:
- Teacher can edit feedback within 48 hours of posting
- After 48 hours, feedback locked (admin can unlock)
- Edit history retained (show "Edited" label with timestamp)
- Feedback can be deleted only if not yet viewed by parent/student
- Once viewed, feedback can only be hidden (soft delete) with admin approval
- Deleted feedback retained in database for audit

**Exception Code**: `FEEDBACK_EDIT_LOCKED`

---

### TF-004: Feedback Visibility Timing
**Rule**: Feedback must be released in conjunction with marks or reports.

**Validations**:
- Feedback visibility options: Immediate, With marks release, With report publish, Manual release
- Default: With marks release (when marks are published)
- Teacher can set visibility rule per feedback entry
- Students and parents see feedback based on visibility setting
- Teacher and admin always see all feedback

**Exception Code**: N/A

---

### TF-005: Feedback Notification
**Rule**: Parents and students must be notified when new feedback is available.

**Validations**:
- Send notification when feedback becomes visible
- Notification includes: Subject, Teacher name, Date
- Parent and student both receive notification
- Notification respects user's channel preferences
- Batch multiple feedback notifications (per subject per day)

**Exception Code**: N/A

---

## 11. Enrollment View Rules

### EV-001: Current Enrollment Visibility
**Rule**: Students must be able to view their current subject enrollments and assigned teachers.

**Validations**:
- Show: Subject name, Subject code, Teacher name, Class name, Period times (from timetable)
- Only show active enrollments (current academic year)
- Sort by: Timetable order or Subject name (user preference)
- Include teacher contact information (school email only)
- Show enrollment date
- Indicate core vs elective subjects

**Exception Code**: N/A

---

### EV-002: Historical Enrollment Access
**Rule**: Students and parents can view past enrollments for reference.

**Validations**:
- Historical enrollments available for completed academic years
- Show: Academic year, Subjects, Teachers, Final marks, Final grade
- Cannot modify historical enrollments
- Historical enrollments retained for duration of student attendance + 5 years
- Include report card downloads for historical years

**Exception Code**: N/A

---

## 12. Dashboard Rules

### DB-001: Dashboard Data Refresh
**Rule**: Dashboard data must be refreshed at appropriate intervals to balance accuracy and performance.

**Validations**:
- Real-time widgets: Refresh every 5 minutes (notifications, recent activity)
- Academic widgets: Refresh every 30 minutes (marks, attendance, performance)
- Financial widgets: Refresh every 60 minutes (fees, payments)
- Manual refresh button available
- Show last refresh timestamp on dashboard
- Cache dashboard data per user to improve load time

**Exception Code**: N/A

---

### DB-002: Dashboard Metrics Definitions
**Rule**: All dashboard metrics must have clear, consistent definitions.

**Validations**:
- Overall Performance: Average of all subject marks (%)
- Attendance Rate: (Days present / Total school days) × 100
- Outstanding Fees: Total fees - Total payments
- Class Rank: Student position based on average mark (1 = highest)
- Improvement: Comparison of current term vs previous term average
- At Risk: Students with average < 40% or attendance < 80%

**Exception Code**: N/A

---

### DB-003: Dashboard Access Control
**Rule**: Dashboard content must be role-specific and data-isolated.

**Validations**:
- Admin dashboard: School-wide metrics, all grades, financial summary
- Teacher dashboard: Assigned classes, student performance in teacher's subjects
- Parent dashboard: Child's academic performance, fees, attendance
- Student dashboard: Own performance, assignments, timetable
- Multi-tenancy enforced (only see data for own school)

**Exception Code**: `DASHBOARD_ACCESS_DENIED`

---

### DB-004: Dashboard Customization
**Rule**: Users should be able to customize their dashboard layout and widgets.

**Validations**:
- Drag-and-drop widget positioning
- Show/hide widgets per user preference
- Reset to default layout option
- Preferences saved per user
- Minimum 3 widgets must be visible
- Maximum 12 widgets on dashboard

**Exception Code**: N/A

---

## 13. Fee Statement Rules

### FS-001: Statement Generation Frequency
**Rule**: Fee statements must be generated regularly and available to parents.

**Validations**:
- Automatic generation: Monthly (1st of each month)
- On-demand generation: Anytime by parent or admin
- Statement reflects all transactions up to generation date
- Statement includes: Opening balance, New fees, Payments, Adjustments, Closing balance
- Statement shows breakdown by fee type
- PDF format with school branding

**Exception Code**: N/A

---

### FS-002: Statement Delivery Method
**Rule**: Fee statements must be delivered through multiple accessible channels.

**Validations**:
- Email to parent's primary email address
- Downloadable from parent portal (PDF)
- Accessible via mobile app
- SMS notification with link to download statement
- Physical mail option for parents who opt-in (additional charge)
- Statement history: Available for current + previous 2 years

**Exception Code**: `STATEMENT_DELIVERY_FAILED`

---

### FS-003: Statement Historical Access
**Rule**: Parents must be able to access historical fee statements for a defined period.

**Validations**:
- Online access: Current + previous 3 years
- Archived access: Older than 3 years (request required, 24-hour turnaround)
- All statements downloadable as PDF
- No modification allowed (read-only)
- Statements retained for 7 years (SARS requirement)

**Exception Code**: `STATEMENT_NOT_AVAILABLE`

---

## 14. Teacher Assignment Rules

### TA-001: Maximum Classes Per Teacher
**Rule**: Teachers must not be assigned to an excessive number of classes.

**Validations**:
- Maximum classes per teacher: 8 (configurable per tenant)
- Count distinct classes (Grade + Class combination)
- Same subject in multiple classes counts toward limit
- Warn at 80% of limit (6 classes)
- Block assignment at limit
- Part-time teachers: Lower limit (configurable)

**Exception Code**: `TEACHER_CLASS_LIMIT_EXCEEDED`

---

### TA-002: Maximum Subjects Per Teacher
**Rule**: Teachers must not be assigned to teach too many different subjects.

**Validations**:
- Maximum subjects per teacher: 4 (to ensure subject expertise)
- Count distinct subjects regardless of grade
- Related subjects count separately (e.g., Maths and Maths Literacy = 2)
- Warn at 3 subjects
- Block assignment at 4 subjects
- Exception for multi-skilled teachers (requires principal approval)

**Exception Code**: `TEACHER_SUBJECT_LIMIT_EXCEEDED`

---

### TA-003: Workload Balancing
**Rule**: Class assignments should be distributed evenly among qualified teachers.

**Validations**:
- Calculate teaching load: Sum of (periods per week × number of classes)
- Target load range: 25-35 periods per week
- Warn if teacher load < 20 or > 35 periods per week
- Admin dashboard shows load distribution across all teachers
- Suggest alternative assignments when imbalance detected
- Consider teacher availability (part-time, leave, etc.)

**Exception Code**: `WORKLOAD_IMBALANCE_DETECTED`

---

### TA-004: Subject Qualification Validation
**Rule**: Teachers must be qualified to teach assigned subjects (as per existing AR-004).

**Validations**:
- Teacher must have qualification for subject
- Qualifications stored in Teacher profile
- Validation on assignment creation
- Warning for provisional/emergency assignments
- Principal can override with documented reason
- Track unqualified assignments for compliance reporting

**Exception Code**: `TEACHER_NOT_QUALIFIED`

---

## 15. Non-Functional Requirements Rules

### NF-001: System Uptime SLA
**Rule**: System must maintain high availability for uninterrupted service.

**Validations**:
- Target uptime: 99.5% (approximately 3.65 hours downtime per month)
- Planned maintenance windows: Weekends 02:00-06:00 SA time
- Notify users 48 hours before planned maintenance
- Maximum planned downtime: 4 hours per month
- Unplanned downtime incidents must be resolved within 4 hours
- Monthly uptime reports generated automatically

**Exception Code**: N/A

---

### NF-002: Backup Schedule
**Rule**: Regular backups must be maintained for disaster recovery.

**Validations**:
- Database backups: Daily at 02:00 SA time
- Full system backup: Weekly (Sunday 01:00 SA time)
- File storage backup: Daily incremental, Weekly full
- Backup retention: Daily (7 days), Weekly (4 weeks), Monthly (12 months)
- Backup verification: Weekly automated restore test
- Off-site backup replication: Within 6 hours of primary backup

**Exception Code**: N/A

---

### NF-003: Backup Recovery Testing
**Rule**: Backup integrity must be verified through regular recovery tests.

**Validations**:
- Automated restore test: Weekly on test environment
- Full disaster recovery drill: Quarterly
- Recovery Time Objective (RTO): 4 hours (system restored within 4 hours)
- Recovery Point Objective (RPO): 24 hours (maximum 24 hours data loss)
- Document recovery procedures
- Update recovery plan quarterly

**Exception Code**: N/A

---

### NF-004: Accessibility Compliance
**Rule**: System must be accessible to users with disabilities.

**Validations**:
- Target compliance: WCAG 2.1 Level AA
- Keyboard navigation support for all functions
- Screen reader compatibility (JAWS, NVDA)
- Color contrast ratio minimum 4.5:1 for text
- Alternative text for all images
- Form labels and error messages accessible
- Quarterly accessibility audits

**Exception Code**: N/A

---

### NF-005: Performance Benchmarks
**Rule**: System must meet defined performance benchmarks for user experience.

**Validations**:
- Page load time: < 3 seconds (90th percentile)
- API response time: < 500ms (95th percentile)
- Report generation: < 10 seconds for standard reports
- File upload: Support up to 100MB files without timeout
- Concurrent users: Support 500 concurrent users per tenant
- Database query optimization: No query > 5 seconds

**Exception Code**: N/A

---

### NF-006: Security Patch Management
**Rule**: Security patches must be applied promptly to minimize vulnerabilities.

**Validations**:
- Critical security patches: Applied within 48 hours
- High priority patches: Applied within 7 days
- Medium priority patches: Applied within 30 days
- Patch testing on staging environment before production
- Automated vulnerability scanning: Weekly
- Security audit: Annual by external auditor

**Exception Code**: N/A

---

## Implementation Priority Summary

| Priority | Rule Categories | Rule Codes |
|----------|----------------|------------|
| **Phase 1 (Critical)** | Online Lessons, Quiz Platform, Learning Materials, Timetable | OL-001 to OL-006, QA-001 to QA-007, LM-001 to LM-007, TT-001 to TT-007 |
| **Phase 2 (High)** | Excel Import, Notifications, Payment Receipts | EI-001 to EI-005, NT-001 to NT-007, PR-001 to PR-006 |
| **Phase 3 (Medium)** | Document Repository, Financial Reporting, Teacher Feedback | DR-001 to DR-005, FR-001 to FR-006, TF-001 to TF-005 |
| **Phase 4 (Lower)** | Enrollment Views, Dashboards, Fee Statements, Teacher Assignment, Non-Functional | EV-001 to EV-002, DB-001 to DB-004, FS-001 to FS-003, TA-001 to TA-004, NF-001 to NF-006 |

---

## Integration Points with Original Business Rules

These supplementary rules complement the original PSMS-Business-Rules.md:
- **AR-001 to AR-007**: Academic Management (extended by OL, QA, TT rules)
- **ER-001 to ER-005**: Enrollment (extended by EV rules)
- **GR-001 to GR-006**: Grading (extended by QA, EI rules)
- **AT-001 to AT-003**: Attendance (extended by OL-006)
- **RP-001 to RP-003**: Reporting (extended by FR rules)
- **FM-001 to FM-005**: Financial (extended by PR, FS rules)
- **CM-001 to CM-002**: Communication (extended by NT rules)
- **MT-001 to MT-003**: Multi-Tenancy (unchanged)
- **SA-001 to SA-002**: Security (extended by NF-006)
- **PC-001 to PC-003**: POPIA Compliance (unchanged)

---

**Total Business Rules**: Original (50+) + Supplementary (106) = **156+ Rules**

**End of Document**
