# PSMS User Stories
## South African Private School Management System

**Version**: 1.0
**Last Updated**: 2026-01-27
**Framework**: ASP.NET Boilerplate (ABP)
**Methodology**: Agile / Scrum

---

## Table of Contents

1. [School Administration User Stories](#1-school-administration-user-stories)
2. [Teacher User Stories](#2-teacher-user-stories)
3. [Parent & Guardian User Stories](#3-parent--guardian-user-stories)
4. [Student User Stories](#4-student-user-stories)
5. [Finance User Stories](#5-finance-user-stories)
6. [System Administration User Stories](#6-system-administration-user-stories)

---

## Story Format & Conventions

**Format**: As a [role], I want [feature], so that [benefit]

**Priority Levels** (MoSCoW):
- **MUST**: Critical for MVP, system cannot function without it
- **SHOULD**: Important but not critical for initial launch
- **COULD**: Desirable but can be deferred
- **WON'T**: Out of scope for current phase

**Story Points**: Fibonacci scale (1, 2, 3, 5, 8, 13, 21)

**Reference Codes**:
- **REQ**: Requirements Document section (e.g., REQ-4.1.1)
- **BR**: Business Rule (e.g., BR-AR-001)
- **SBR**: Supplementary Business Rule (e.g., SBR-OL-001)

---

## 1. School Administration User Stories

### 1.1 Student & Academic Management

#### US-ADM-001: Register New Student
**As a** school administrator
**I want** to register a new student with their personal and academic information
**So that** the student can be enrolled in the school system

**Acceptance Criteria**:
- [ ] Can enter student personal details (name, DOB, gender, ID number)
- [ ] Can enter parent/guardian information and link to student
- [ ] Can specify grade and class for enrollment
- [ ] System validates SA ID number using Luhn algorithm
- [ ] System checks for duplicate student records
- [ ] System captures POPIA consent
- [ ] Student record is created with unique ID
- [ ] Confirmation notification sent to parent

**Priority**: MUST
**Story Points**: 8
**References**: REQ-4.1.1, BR-ER-001, BR-SA-001

---

#### US-ADM-002: Assign Student to Grade and Class
**As a** school administrator
**I want** to assign students to specific grades and classes
**So that** students can access appropriate curriculum and timetables

**Acceptance Criteria**:
- [ ] Can select student from existing records
- [ ] Can assign to grade level (Grade R-12)
- [ ] Can assign to specific class within grade
- [ ] System validates age-grade appropriateness
- [ ] System prevents duplicate assignments
- [ ] Student can be reassigned if needed (with audit trail)
- [ ] Assignment effective date can be specified

**Priority**: MUST
**Story Points**: 5
**References**: REQ-4.1.1, BR-ER-002

---

#### US-ADM-003: Link Parents to Students
**As a** school administrator
**I want** to link one or more parents/guardians to a student
**So that** parents can access their child's information and receive communications

**Acceptance Criteria**:
- [ ] Can create new parent account or link existing parent
- [ ] Can specify relationship type (Mother, Father, Guardian, etc.)
- [ ] Can mark primary parent for billing and communication
- [ ] Can link multiple parents to one student
- [ ] Can link one parent to multiple students (siblings)
- [ ] System sends account activation email to parent
- [ ] Parent access is activated immediately after linking

**Priority**: MUST
**Story Points**: 5
**References**: REQ-4.1.1, BR-ER-003

---

#### US-ADM-004: Manage Academic Calendar
**As a** school administrator
**I want** to create and manage the academic calendar with terms and holidays
**So that** all stakeholders can view important academic dates

**Acceptance Criteria**:
- [ ] Can create new academic year (SA: January-December)
- [ ] Can define 4 terms with start and end dates
- [ ] Can add SA public holidays automatically
- [ ] Can add school-specific holidays and events
- [ ] Can add exam periods and assessment dates
- [ ] Calendar is visible to all users once published
- [ ] Only one academic year can be marked as current
- [ ] Changes trigger notifications to stakeholders

**Priority**: MUST
**Story Points**: 8
**References**: REQ-4.1.1, BR-AR-001, BR-AR-002

---

#### US-ADM-005: Manage Teacher Profiles
**As a** school administrator
**I want** to create and manage teacher profiles with qualifications
**So that** teachers can be assigned to appropriate subjects and classes

**Acceptance Criteria**:
- [ ] Can create teacher profile with personal details
- [ ] Can add teacher qualifications and certifications
- [ ] Can add subjects teacher is qualified to teach
- [ ] Can specify employment status (Full-time, Part-time, Contract)
- [ ] Can set teacher availability
- [ ] System creates user account for teacher
- [ ] Teacher receives onboarding email with credentials

**Priority**: MUST
**Story Points**: 8
**References**: REQ-4.1.2, BR-AR-004

---

#### US-ADM-006: Assign Teachers to Subjects and Classes
**As a** school administrator
**I want** to assign teachers to specific subjects and classes
**So that** teaching responsibilities are clearly defined

**Acceptance Criteria**:
- [ ] Can select teacher from active teachers list
- [ ] Can assign to subject(s) teacher is qualified for
- [ ] Can assign to specific grade and class
- [ ] System validates teacher qualification for subject
- [ ] System checks maximum classes per teacher (limit: 8)
- [ ] System checks maximum subjects per teacher (limit: 4)
- [ ] System validates workload (25-35 periods/week)
- [ ] Assignment is reflected in timetable

**Priority**: MUST
**Story Points**: 8
**References**: REQ-4.1.2, BR-AR-004, SBR-TA-001 to TA-004

---

#### US-ADM-007: View Academic Performance Dashboard
**As a** school administrator
**I want** to view a dashboard with school-wide academic performance metrics
**So that** I can monitor overall academic health and identify areas needing attention

**Acceptance Criteria**:
- [ ] Dashboard shows school-wide average performance per grade
- [ ] Dashboard shows subject-wise performance comparison
- [ ] Dashboard shows attendance statistics
- [ ] Dashboard shows at-risk students (performance < 40%)
- [ ] Dashboard shows top performing students per grade
- [ ] Data refreshes every 30 minutes
- [ ] Can filter by: Grade, Subject, Term, Class
- [ ] Can export dashboard data to Excel

**Priority**: SHOULD
**Story Points**: 13
**References**: REQ-4.1.3, SBR-DB-001 to DB-004

---

#### US-ADM-008: Generate Academic Reports
**As a** school administrator
**I want** to generate various academic reports
**So that** I can analyze performance and meet regulatory requirements

**Acceptance Criteria**:
- [ ] Can generate term reports for individual students
- [ ] Can generate class performance summary reports
- [ ] Can generate grade performance summary reports
- [ ] Can generate attendance reports
- [ ] Reports follow SA report card format (7-level grading)
- [ ] Can export reports to PDF
- [ ] Can schedule automatic report generation
- [ ] Generated reports are stored in document repository

**Priority**: MUST
**Story Points**: 13
**References**: REQ-4.1.3, BR-RP-001 to RP-003

---

#### US-ADM-009: Publish Reports to Parents
**As a** school administrator
**I want** to publish student reports to parent accounts
**So that** parents can access their child's academic progress

**Acceptance Criteria**:
- [ ] Can select reports to publish (by grade, class, or individual)
- [ ] Can set publication date and time
- [ ] System validates all reports are complete before publishing
- [ ] Parents receive notification when reports are published
- [ ] Published reports appear in parent portal immediately
- [ ] Published reports are locked from editing
- [ ] Audit trail tracks publication action

**Priority**: MUST
**Story Points**: 5
**References**: REQ-4.1.3, BR-RP-002

---

#### US-ADM-010: Send Announcements
**As a** school administrator
**I want** to send announcements to specific audiences
**So that** important information reaches the right stakeholders

**Acceptance Criteria**:
- [ ] Can compose announcement with title and message
- [ ] Can select target audience: All school, Specific grade, Specific class, Teachers only, Parents only
- [ ] Can attach documents or images
- [ ] Can schedule announcement for future delivery
- [ ] Can mark announcement priority (Normal, Important, Urgent)
- [ ] Announcement sent via multiple channels (email, in-app, SMS for urgent)
- [ ] Can view delivery status and read receipts
- [ ] Announcement archived after 90 days

**Priority**: MUST
**Story Points**: 8
**References**: REQ-4.1.4, BR-CM-001, SBR-NT-001 to NT-007

---

### 1.2 Timetable Management

#### US-ADM-011: Create Class Timetable
**As a** school administrator
**I want** to create timetables for classes
**So that** students and teachers know their daily schedules

**Acceptance Criteria**:
- [ ] Can define school day structure (start time, periods, breaks)
- [ ] Can assign subjects to time slots for specific class
- [ ] Can assign teachers to periods
- [ ] Can assign classrooms/venues to periods
- [ ] System detects teacher period conflicts
- [ ] System detects room period conflicts
- [ ] System detects student period conflicts (multi-class students)
- [ ] System validates teacher workload limits
- [ ] Timetable draft can be saved and edited
- [ ] Timetable requires approval before publication

**Priority**: MUST
**Story Points**: 21
**References**: REQ-4.1.1, SBR-TT-001 to TT-007

---

#### US-ADM-012: Publish Timetable
**As a** school administrator
**I want** to publish approved timetables
**So that** students, teachers, and parents can view schedules

**Acceptance Criteria**:
- [ ] Can review timetable for conflicts before publishing
- [ ] Can publish timetable for entire grade or individual classes
- [ ] Published timetables become visible to affected stakeholders
- [ ] System sends notifications about published timetables
- [ ] Published timetables are locked from editing
- [ ] Changes to published timetables require admin approval
- [ ] Timetable changes trigger notifications

**Priority**: MUST
**Story Points**: 5
**References**: SBR-TT-006

---

#### US-ADM-013: Manage Timetable Variations
**As a** school administrator
**I want** to make temporary changes to published timetables
**So that** schedules can accommodate special events or teacher absences

**Acceptance Criteria**:
- [ ] Can substitute teacher for specific period(s)
- [ ] Can change room/venue for specific period(s)
- [ ] Can swap period times for specific day
- [ ] Must provide reason for each change
- [ ] Changes require admin approval
- [ ] Affected stakeholders receive automatic notification
- [ ] Changes are logged in audit trail
- [ ] Can revert temporary changes

**Priority**: SHOULD
**Story Points**: 8
**References**: SBR-TT-007

---

### 1.3 System Configuration

#### US-ADM-014: Configure School Profile
**As a** school administrator
**I want** to configure school profile and settings
**So that** the system reflects our school's identity and policies

**Acceptance Criteria**:
- [ ] Can set school name, address, contact details
- [ ] Can upload school logo
- [ ] Can configure grading system (SA 7-level grading)
- [ ] Can configure fee types and structures
- [ ] Can configure term dates template
- [ ] Can configure notification preferences
- [ ] Can configure storage quotas
- [ ] Changes apply tenant-wide

**Priority**: MUST
**Story Points**: 8
**References**: BR-MT-001

---

#### US-ADM-015: Manage User Roles and Permissions
**As a** school administrator
**I want** to manage user roles and permissions
**So that** users have appropriate access to system functions

**Acceptance Criteria**:
- [ ] Can view all users in tenant
- [ ] Can assign roles: Admin, Principal, Teacher, Parent, Student, Finance
- [ ] Can view permissions per role
- [ ] Can activate/deactivate user accounts
- [ ] Can reset user passwords
- [ ] Role changes take effect immediately
- [ ] All role changes logged in audit trail

**Priority**: MUST
**Story Points**: 8
**References**: REQ-5.1, BR-SA-001, BR-SA-002

---

## 2. Teacher User Stories

### 2.1 Learning Materials Management

#### US-TCH-001: Upload Learning Materials
**As a** teacher
**I want** to upload learning materials for my subjects
**So that** students can access resources for their studies

**Acceptance Criteria**:
- [ ] Can upload files: PDF, DOC, PPT, MP4, MP3, images, ZIP
- [ ] Can upload files up to size limits (documents 50MB, videos 500MB)
- [ ] Must categorize by: Grade, Subject, Term
- [ ] Must provide title (5-200 characters) and description (10-1000 characters)
- [ ] Must specify material type: Lesson Notes, Homework, Reading, Video, Reference
- [ ] Can mark as Core or Supplementary
- [ ] Can add up to 5 tags
- [ ] File passes virus scan before storage
- [ ] Can schedule future release date
- [ ] Students receive notification when material is published

**Priority**: MUST
**Story Points**: 8
**References**: REQ-4.2.1, SBR-LM-001 to LM-007

---

#### US-TCH-002: Organize Learning Materials
**As a** teacher
**I want** to organize my learning materials into categories
**So that** students can easily find relevant resources

**Acceptance Criteria**:
- [ ] Can view all my uploaded materials
- [ ] Can filter by: Grade, Subject, Term, Material Type
- [ ] Can search materials by title or tags
- [ ] Can edit material details (title, description, tags)
- [ ] Can update material file (creates new version)
- [ ] Can hide/archive old materials
- [ ] Can see download statistics per material
- [ ] Can see my storage quota usage

**Priority**: SHOULD
**Story Points**: 5
**References**: REQ-4.2.1, SBR-LM-002, LM-006

---

#### US-TCH-003: Update Learning Material Version
**As a** teacher
**I want** to upload updated versions of learning materials
**So that** students have access to corrected or improved resources

**Acceptance Criteria**:
- [ ] Can upload new version of existing material
- [ ] System auto-increments version number (1.0, 1.1, 1.2, etc.)
- [ ] Must provide change description
- [ ] Previous versions retained (up to 10 versions)
- [ ] Students see latest version by default
- [ ] Can view version history
- [ ] Can restore previous version if needed
- [ ] Students notified of material update

**Priority**: SHOULD
**Story Points**: 5
**References**: SBR-LM-003

---

### 2.2 Online Lessons

#### US-TCH-004: Schedule Online Lesson
**As a** teacher
**I want** to schedule online lessons for my classes
**So that** students know when to join live sessions

**Acceptance Criteria**:
- [ ] Can select subject and class for lesson
- [ ] Can set lesson date and time (must be 24+ hours in advance)
- [ ] Can set lesson duration (30 minutes to 3 hours)
- [ ] Can add lesson title and description
- [ ] Can attach pre-lesson materials
- [ ] System validates no scheduling conflicts
- [ ] System validates lesson is during school hours
- [ ] System validates lesson is within current term
- [ ] Students receive notification 24 hours before lesson
- [ ] Students receive reminder 15 minutes before lesson
- [ ] Lesson link becomes active 15 minutes before start

**Priority**: MUST
**Story Points**: 13
**References**: REQ-4.2.2, SBR-OL-001, OL-002

---

#### US-TCH-005: Host Live Online Class
**As a** teacher
**I want** to host live online classes
**So that** I can teach students remotely in real-time

**Acceptance Criteria**:
- [ ] Can start live class from scheduled lesson
- [ ] System generates unique meeting link
- [ ] Can see list of joined students
- [ ] Can share screen, whiteboard, presentations
- [ ] Can enable/disable student video and audio
- [ ] Can record the session
- [ ] System tracks student attendance automatically
- [ ] Maximum 100 participants per session
- [ ] Only enrolled students can join
- [ ] Meeting link expires 30 minutes after scheduled end

**Priority**: MUST
**Story Points**: 21
**References**: REQ-4.2.2, SBR-OL-002, OL-006

---

#### US-TCH-006: Upload Lesson Recording
**As a** teacher
**I want** to upload recordings of lessons
**So that** students who missed the class can catch up

**Acceptance Criteria**:
- [ ] Can upload video files: MP4, MOV, AVI, WebM
- [ ] Can upload files up to 5GB
- [ ] Must link recording to scheduled lesson
- [ ] File name must include: Subject, Grade, Date, Topic
- [ ] File passes virus scan
- [ ] System stores recording metadata (duration, upload date)
- [ ] Recording immediately available to enrolled students
- [ ] Students receive notification when recording is available
- [ ] Can see recording view statistics

**Priority**: MUST
**Story Points**: 8
**References**: REQ-4.2.2, SBR-OL-003, OL-004

---

### 2.3 Assessment & Marking

#### US-TCH-007: Create Quiz/Assessment
**As a** teacher
**I want** to create online quizzes and assessments
**So that** I can evaluate student understanding digitally

**Acceptance Criteria**:
- [ ] Can create quiz with title and description
- [ ] Can add 5-100 multiple choice questions
- [ ] Each question has 2-6 options with one correct answer
- [ ] Question text: 10-1000 characters, Option text: 1-500 characters
- [ ] Must assign to specific Grade, Subject, Term
- [ ] Can set time limit (default 60 minutes)
- [ ] Can configure number of attempts (1-3)
- [ ] Can set quiz available date (24+ hours after creation)
- [ ] Can set quiz due date
- [ ] Can configure result release: Immediate, After due date, Manual
- [ ] Quiz locked from editing once available to students

**Priority**: MUST
**Story Points**: 21
**References**: REQ-4.2.3, SBR-QA-001 to QA-007

---

#### US-TCH-008: Capture Student Marks
**As a** teacher
**I want** to capture marks for assessments
**So that** student performance is recorded in the system

**Acceptance Criteria**:
- [ ] Can select assessment and class
- [ ] Can enter marks per student (0 to max mark)
- [ ] System validates mark is within valid range
- [ ] Can add individual feedback per student (20-2000 characters)
- [ ] System auto-calculates percentage
- [ ] System converts to SA 7-level achievement grade
- [ ] Can save as draft and complete later
- [ ] Marks locked once published
- [ ] Published marks visible to students and parents
- [ ] Students receive notification when marks published

**Priority**: MUST
**Story Points**: 8
**References**: REQ-4.2.3, BR-GR-001 to GR-006

---

#### US-TCH-009: Import Marks via Excel
**As a** teacher
**I want** to import marks using an Excel spreadsheet
**So that** I can efficiently upload marks for large classes

**Acceptance Criteria**:
- [ ] Can download Excel template with required columns
- [ ] Template includes: Student ID Number, Subject Code, Assessment Type, Mark, Max Mark
- [ ] Can upload completed Excel file (XLSX, XLS, CSV)
- [ ] Maximum file size: 10MB, Maximum rows: 1000
- [ ] System validates all data before import
- [ ] System shows preview of first 10 rows
- [ ] System shows validation summary (total, valid, invalid rows)
- [ ] Can download error report if validation fails
- [ ] Must correct all errors before import proceeds
- [ ] Import is atomic (all or nothing)
- [ ] Success confirmation shows number of records imported
- [ ] Import action logged in audit trail

**Priority**: SHOULD
**Story Points**: 13
**References**: REQ-4.2.3, SBR-EI-001 to EI-005

---

#### US-TCH-010: Provide Student Feedback
**As a** teacher
**I want** to provide written feedback on student work
**So that** students understand their strengths and areas for improvement

**Acceptance Criteria**:
- [ ] Can add feedback when entering marks
- [ ] Feedback: 20-2000 characters
- [ ] Can use basic formatting (line breaks, paragraphs)
- [ ] System scans for inappropriate language
- [ ] Can edit feedback within 48 hours of posting
- [ ] After 48 hours, feedback locked (admin can unlock)
- [ ] Feedback visibility tied to mark publication
- [ ] Students and parents see feedback when marks published
- [ ] Edit history retained with timestamps

**Priority**: SHOULD
**Story Points**: 5
**References**: REQ-4.2.3, SBR-TF-001 to TF-005

---

#### US-TCH-011: Take Class Attendance Register
**As a** teacher
**I want** to record daily attendance for my classes
**So that** student attendance is tracked for each lesson

**Acceptance Criteria**:
- [ ] Can select class and date
- [ ] Can see list of all enrolled students
- [ ] Can mark each student: Present, Absent, Late, Excused
- [ ] Can add notes for absences
- [ ] Cannot mark attendance for future dates
- [ ] Can edit attendance for current day only
- [ ] After midnight, attendance locked (admin can edit)
- [ ] Parents receive notification for absences (same day)
- [ ] Attendance data flows to student attendance reports

**Priority**: MUST
**Story Points**: 8
**References**: REQ-4.2.3, BR-AT-001 to AT-003

---

### 2.4 Communication

#### US-TCH-012: Send Messages to Parents
**As a** teacher
**I want** to send messages to parents of students in my classes
**So that** I can communicate important information directly

**Acceptance Criteria**:
- [ ] Can compose message with subject and body
- [ ] Can select recipients: Individual parent, All parents in class, Parents of specific students
- [ ] Can attach documents (max 10MB)
- [ ] Message sent via email and in-app notification
- [ ] Can see message delivery status
- [ ] Parents can reply to messages
- [ ] Can view message history per parent
- [ ] All communication logged in audit trail

**Priority**: SHOULD
**Story Points**: 8
**References**: REQ-4.2.4, BR-CM-002

---

#### US-TCH-013: View My Teaching Schedule
**As a** teacher
**I want** to view my teaching timetable
**So that** I know my daily schedule and responsibilities

**Acceptance Criteria**:
- [ ] Can view daily, weekly, or term timetable
- [ ] Timetable shows: Period times, Subject, Grade/Class, Room
- [ ] Can see free periods
- [ ] Can see total teaching periods per week
- [ ] Can filter by specific date range
- [ ] Can export timetable to PDF or calendar format (iCal)
- [ ] Timetable updates reflect immediately when changes made
- [ ] Receive notification for any timetable changes

**Priority**: MUST
**Story Points**: 5
**References**: SBR-TT-001

---

## 3. Parent & Guardian User Stories

### 3.1 Academic Monitoring

#### US-PAR-001: View Child's Performance Per Subject
**As a** parent
**I want** to view my child's academic performance for each subject
**So that** I can monitor their progress and provide support

**Acceptance Criteria**:
- [ ] Can select child (if multiple children enrolled)
- [ ] Can view performance summary per subject
- [ ] Can see all marks for each assessment
- [ ] Can see achievement level (SA 7-level grading)
- [ ] Can see class average for comparison
- [ ] Can see term average and overall average
- [ ] Can view performance trends over time
- [ ] Can filter by: Term, Subject
- [ ] Performance data updates when teacher publishes marks

**Priority**: MUST
**Story Points**: 8
**References**: REQ-4.3.1, BR-GR-001

---

#### US-PAR-002: View Teacher Feedback
**As a** parent
**I want** to read teacher feedback on my child's work
**So that** I understand their strengths and areas needing improvement

**Acceptance Criteria**:
- [ ] Can view feedback for each assessment
- [ ] Feedback displayed with corresponding mark
- [ ] Can filter feedback by: Subject, Teacher, Date
- [ ] Can see when feedback was posted
- [ ] New feedback highlighted until viewed
- [ ] Receive notification when new feedback available
- [ ] Can reply to teacher (if communication enabled)

**Priority**: SHOULD
**Story Points**: 3
**References**: REQ-4.3.1, SBR-TF-001

---

#### US-PAR-003: View Child's Attendance
**As a** parent
**I want** to view my child's attendance record
**So that** I can ensure they are attending classes regularly

**Acceptance Criteria**:
- [ ] Can view attendance summary: Present days, Absent days, Late days, Attendance %
- [ ] Can view detailed daily attendance by subject/period
- [ ] Can see absence reasons (if provided)
- [ ] Can filter by date range or term
- [ ] Receive same-day notification for absences
- [ ] Can view attendance trends over time
- [ ] Attendance % calculated: (Present / Total school days) × 100

**Priority**: MUST
**Story Points**: 5
**References**: BR-AT-001

---

### 3.2 Reports & Documents

#### US-PAR-004: Download Child's Academic Reports
**As a** parent
**I want** to download my child's official term/annual reports
**So that** I can keep records of their academic progress

**Acceptance Criteria**:
- [ ] Can view list of available reports (per term/year)
- [ ] Can download reports as PDF
- [ ] Reports follow SA report card format
- [ ] Reports include: All subjects, Marks, Achievement levels, Teacher comments, Overall performance
- [ ] Receive notification when new report published
- [ ] Can access historical reports (previous years)
- [ ] Downloaded reports are read-only

**Priority**: MUST
**Story Points**: 5
**References**: REQ-4.3.2, BR-RP-001

---

#### US-PAR-005: Access School Documents
**As a** parent
**I want** to access school documents and policies
**So that** I stay informed about school procedures and requirements

**Acceptance Criteria**:
- [ ] Can view document categories: Policies, Forms, Circulars, Calendars
- [ ] Can search documents by title or keywords
- [ ] Can download documents as PDF
- [ ] Can see when document was published/updated
- [ ] Receive notification for new important documents
- [ ] Can access grade-specific documents
- [ ] Expired documents not shown

**Priority**: SHOULD
**Story Points**: 5
**References**: REQ-4.3.2, SBR-DR-001 to DR-005

---

### 3.3 Fee & Payment Management

#### US-PAR-006: View Fee Statements
**As a** parent
**I want** to view my child's fee statements
**So that** I know what fees are due and my account balance

**Acceptance Criteria**:
- [ ] Can view current fee statement
- [ ] Statement shows: Opening balance, New fees, Payments, Adjustments, Closing balance
- [ ] Statement shows breakdown by fee type (Tuition, Transport, Extramural, etc.)
- [ ] Can filter by date range or academic term
- [ ] Can download statement as PDF
- [ ] Receive automatic monthly statement via email
- [ ] Can access historical statements (previous 3 years)

**Priority**: MUST
**Story Points**: 8
**References**: REQ-4.3.3, SBR-FS-001 to FS-003

---

#### US-PAR-007: Make Online Payment
**As a** parent
**I want** to pay school fees online
**So that** I can conveniently settle my account

**Acceptance Criteria**:
- [ ] Can view amount due and fee breakdown
- [ ] Can select payment amount (full balance or partial)
- [ ] Can select payment method: Credit Card, EFT, Capitec Pay, Ozow, Instant EFT
- [ ] Payment processed through secure gateway
- [ ] Receive payment confirmation immediately
- [ ] Payment reflects in account within 24 hours (for EFT)
- [ ] Receipt generated automatically
- [ ] Receipt sent via email
- [ ] Receipt downloadable from portal

**Priority**: MUST
**Story Points**: 13
**References**: REQ-4.3.3, BR-FM-003, SBR-PR-001 to PR-006

---

#### US-PAR-008: Download Payment Receipts
**As a** parent
**I want** to download receipts for all payments made
**So that** I can keep financial records

**Acceptance Criteria**:
- [ ] Can view list of all payments with receipt numbers
- [ ] Can download individual receipt as PDF
- [ ] Can download multiple receipts (bulk download)
- [ ] Receipt includes: School details, Parent details, Student details, Payment details, Receipt number, Date
- [ ] Receipts accessible immediately after payment
- [ ] Can access historical receipts (7 years)
- [ ] Receipts are read-only (immutable)

**Priority**: MUST
**Story Points**: 5
**References**: REQ-4.3.3, SBR-PR-001 to PR-006

---

### 3.4 Notifications

#### US-PAR-009: Receive Notifications
**As a** parent
**I want** to receive notifications about important events
**So that** I stay informed about my child's education

**Acceptance Criteria**:
- [ ] Receive notifications for: New reports, New marks, Absences, Payment due, Payment received, School announcements, Timetable changes
- [ ] Notifications delivered via: Email, SMS (for critical), In-app, Push (mobile app)
- [ ] Can view notification history
- [ ] Can mark notifications as read/unread
- [ ] Unread notification count shown in menu
- [ ] Can set notification preferences per type
- [ ] Can enable/disable specific notification types
- [ ] Cannot disable critical notifications (payment overdue, security)

**Priority**: MUST
**Story Points**: 8
**References**: REQ-4.3.4, SBR-NT-001 to NT-007

---

#### US-PAR-010: Manage Notification Preferences
**As a** parent
**I want** to customize my notification preferences
**So that** I receive notifications through my preferred channels

**Acceptance Criteria**:
- [ ] Can access notification settings in profile
- [ ] Can enable/disable notifications per event type
- [ ] Can select delivery channel: Email, SMS, In-app, Push
- [ ] Can set frequency: Immediate or Daily digest
- [ ] Can set separate preferences per child (if multiple)
- [ ] Changes apply immediately
- [ ] Critical notifications cannot be disabled
- [ ] Can opt out of marketing/non-essential notifications

**Priority**: SHOULD
**Story Points**: 5
**References**: SBR-NT-003

---

### 3.5 Communication

#### US-PAR-011: View School Announcements
**As a** parent
**I want** to view school announcements
**So that** I stay informed about school events and news

**Acceptance Criteria**:
- [ ] Can view all announcements relevant to my child's grade/class
- [ ] Can filter by: Date, Priority, Grade, Class
- [ ] Can search announcements by keywords
- [ ] Urgent announcements highlighted
- [ ] Can see attached documents
- [ ] Can mark announcements as read
- [ ] Receive notification for new announcements
- [ ] Announcements sorted by date (newest first)

**Priority**: SHOULD
**Story Points**: 5
**References**: BR-CM-001

---

## 4. Student User Stories

### 4.1 Academic Access

#### US-STU-001: View My Timetable
**As a** student
**I want** to view my class timetable
**So that** I know my daily schedule

**Acceptance Criteria**:
- [ ] Can view daily, weekly, or term timetable
- [ ] Timetable shows: Period times, Subject, Teacher, Room
- [ ] Can see current day highlighted
- [ ] Can see current/next period highlighted
- [ ] Timetable updates when changes made
- [ ] Receive notification for timetable changes
- [ ] Can export timetable to calendar format

**Priority**: MUST
**Story Points**: 5
**References**: REQ-4.4.1, SBR-EV-001

---

#### US-STU-002: View Enrolled Subjects and Teachers
**As a** student
**I want** to view my enrolled subjects and assigned teachers
**So that** I know who teaches each subject

**Acceptance Criteria**:
- [ ] Can view list of all enrolled subjects
- [ ] Each subject shows: Subject name, Teacher name, Class name, Period times
- [ ] Can see teacher contact (school email)
- [ ] Can distinguish core vs elective subjects
- [ ] Can view enrollment date
- [ ] Subjects sorted by timetable order or name
- [ ] Can access historical enrollments (previous years)

**Priority**: MUST
**Story Points**: 3
**References**: REQ-4.4.1, SBR-EV-001 to EV-002

---

### 4.2 Learning Materials

#### US-STU-003: View Learning Materials
**As a** student
**I want** to view learning materials for my subjects
**So that** I can access resources for studying

**Acceptance Criteria**:
- [ ] Can view materials for all enrolled subjects
- [ ] Can filter by: Subject, Term, Material Type (Notes, Homework, Reading, Video)
- [ ] Can search materials by title or tags
- [ ] Materials show: Title, Description, Upload date, File type, File size
- [ ] Core materials highlighted
- [ ] Can see new materials badge
- [ ] Materials organized by upload date (newest first)
- [ ] Only see materials for current and past terms

**Priority**: MUST
**Story Points**: 5
**References**: REQ-4.4.2, SBR-LM-001, LM-005

---

#### US-STU-004: Download Learning Materials
**As a** student
**I want** to download learning materials
**So that** I can study offline

**Acceptance Criteria**:
- [ ] Can download individual files
- [ ] Can download multiple files (batch download as ZIP)
- [ ] Download starts immediately upon click
- [ ] Can view download progress
- [ ] Can pause/resume large downloads
- [ ] Downloaded files named clearly (Subject-Title-Date)
- [ ] Download action tracked for analytics
- [ ] Can download materials unlimited times

**Priority**: MUST
**Story Points**: 5
**References**: REQ-4.4.2, SBR-LM-005, LM-006

---

### 4.3 Online Learning

#### US-STU-005: Join Live Online Class
**As a** student
**I want** to join live online classes
**So that** I can attend lessons remotely

**Acceptance Criteria**:
- [ ] Can see upcoming scheduled online lessons
- [ ] Lesson join link becomes active 15 minutes before start
- [ ] Can click link to join lesson directly
- [ ] Can enable/disable my video and audio
- [ ] Can see teacher and other students
- [ ] Can see shared screen/presentations
- [ ] Can participate via chat
- [ ] Attendance tracked automatically
- [ ] Receive reminder notification 15 minutes before lesson

**Priority**: MUST
**Story Points**: 13
**References**: REQ-4.4.3, SBR-OL-002, OL-006

---

#### US-STU-006: View Recorded Lessons
**As a** student
**I want** to view recordings of past lessons
**So that** I can review material or catch up on missed classes

**Acceptance Criteria**:
- [ ] Can view recordings for all enrolled subjects
- [ ] Recordings show: Title, Date, Duration, Subject, Teacher
- [ ] Can play recording in browser (no download required)
- [ ] Can download recording for offline viewing
- [ ] Video player has: Play/Pause, Seek, Speed control, Fullscreen
- [ ] Can access recordings from current academic year
- [ ] Previous years' recordings require special access
- [ ] Receive notification when new recording available

**Priority**: MUST
**Story Points**: 8
**References**: REQ-4.4.3, SBR-OL-003, OL-004

---

### 4.4 Performance Tracking

#### US-STU-007: View My Marks
**As a** student
**I want** to view my marks for all assessments
**So that** I can track my academic progress

**Acceptance Criteria**:
- [ ] Can view marks for all subjects
- [ ] Can see each assessment: Name, Date, Mark, Max mark, Percentage, Achievement level
- [ ] Can see term average per subject
- [ ] Can see overall average across all subjects
- [ ] Can filter by: Subject, Term, Assessment type
- [ ] Can see class average for comparison
- [ ] Marks update when teacher publishes new marks
- [ ] Receive notification when new marks available

**Priority**: MUST
**Story Points**: 8
**References**: REQ-4.4.4, BR-GR-001

---

#### US-STU-008: View Teacher Feedback
**As a** student
**I want** to read teacher feedback on my work
**So that** I can understand how to improve

**Acceptance Criteria**:
- [ ] Can view feedback for each assessment
- [ ] Feedback displayed with corresponding mark
- [ ] Can filter feedback by: Subject, Date
- [ ] New feedback highlighted
- [ ] Receive notification when new feedback available
- [ ] Feedback visible when teacher publishes marks

**Priority**: SHOULD
**Story Points**: 3
**References**: SBR-TF-004

---

#### US-STU-009: Take Online Quiz
**As a** student
**I want** to take quizzes and assessments online
**So that** I can be evaluated digitally

**Acceptance Criteria**:
- [ ] Can see available quizzes with due dates
- [ ] Can see quiz details: Number of questions, Time limit, Attempts allowed
- [ ] Can start quiz within availability window
- [ ] Quiz timer counts down during attempt
- [ ] Can navigate between questions
- [ ] Draft saved automatically every 5 minutes
- [ ] Must answer all questions or explicitly skip
- [ ] Auto-submit when time expires
- [ ] Submit confirmation required
- [ ] See score immediately (if immediate release enabled)
- [ ] Cannot modify submission after final submit

**Priority**: MUST
**Story Points**: 21
**References**: SBR-QA-001 to QA-007

---

### 4.5 Communication

#### US-STU-010: Receive Announcements
**As a** student
**I want** to receive school and class announcements
**So that** I stay informed about important information

**Acceptance Criteria**:
- [ ] Can view all announcements for my grade/class
- [ ] Announcements show: Title, Message, Date, Priority, Attachments
- [ ] Can filter by: Date, Priority, Subject
- [ ] Urgent announcements highlighted
- [ ] Can mark as read
- [ ] Receive notification for new announcements
- [ ] Can download attachments
- [ ] Announcements sorted by date (newest first)

**Priority**: MUST
**Story Points**: 5
**References**: REQ-4.4.5, BR-CM-001

---

## 5. Finance User Stories

### 5.1 Fee Structure Management

#### US-FIN-001: Create Fee Structure
**As a** finance manager
**I want** to create fee structures for different grades
**So that** students are billed correctly

**Acceptance Criteria**:
- [ ] Can create fee structure per grade
- [ ] Can add multiple fee types: Tuition, Registration, Extramural, Transport, Aftercare, Books, Uniform
- [ ] Can set amount per fee type
- [ ] Can set billing frequency: Annual, Term, Monthly
- [ ] Can set due dates per term/month
- [ ] Can apply discounts by percentage or fixed amount
- [ ] Can apply sibling discounts
- [ ] Can add bursary/scholarship adjustments
- [ ] Fee structure requires approval before activation
- [ ] Active fee structure applies to all students in grade

**Priority**: MUST
**Story Points**: 13
**References**: REQ-4.5.1, BR-FM-001

---

#### US-FIN-002: Configure Discounts and Penalties
**As a** finance manager
**I want** to configure discounts and late payment penalties
**So that** fee policies are automatically applied

**Acceptance Criteria**:
- [ ] Can create discount rules: Sibling discount, Early payment discount, Bursary
- [ ] Can set discount percentage or fixed amount
- [ ] Can create penalty rules: Late payment fee, Returned payment fee
- [ ] Can set penalty amount or percentage
- [ ] Can define payment grace periods
- [ ] Discounts applied automatically when conditions met
- [ ] Penalties applied automatically when payment overdue
- [ ] Can override automatic application (with reason)
- [ ] All adjustments logged in audit trail

**Priority**: SHOULD
**Story Points**: 8
**References**: REQ-4.5.1, BR-FM-002

---

### 5.2 Payments & Reconciliation

#### US-FIN-003: View Payment Transactions
**As a** finance manager
**I want** to view all payment transactions
**So that** I can monitor incoming payments

**Acceptance Criteria**:
- [ ] Can view list of all payments
- [ ] Can filter by: Date range, Payment method, Status, Student, Grade
- [ ] Each payment shows: Date, Student, Amount, Payment method, Reference, Status
- [ ] Can search by: Student name, Receipt number, Reference number
- [ ] Can see payment status: Pending, Completed, Failed, Reversed
- [ ] Can export payment list to Excel
- [ ] Real-time updates for online payments

**Priority**: MUST
**Story Points**: 8
**References**: REQ-4.5.2, BR-FM-003

---

#### US-FIN-004: Reconcile Payments
**As a** finance manager
**I want** to reconcile payments with bank statements
**So that** all payments are accurately recorded

**Acceptance Criteria**:
- [ ] Can view unreconciled payments
- [ ] Can match payment to bank statement entry
- [ ] Can mark payment as reconciled
- [ ] Can view reconciliation history
- [ ] Reconciled payments locked from editing
- [ ] Can upload bank statement (Excel, CSV)
- [ ] System suggests matches based on amount and date
- [ ] Can manually reconcile mismatched payments
- [ ] Reconciliation action logged in audit trail

**Priority**: SHOULD
**Story Points**: 13
**References**: REQ-4.5.2, BR-FM-005

---

#### US-FIN-005: Capture Manual Payments
**As a** finance manager
**I want** to record manual payments (cash, cheque, EFT)
**So that** all payments are tracked in the system

**Acceptance Criteria**:
- [ ] Can select student account
- [ ] Can enter payment amount
- [ ] Can select payment method: Cash, Cheque, EFT, Bank deposit
- [ ] Can enter payment date
- [ ] Can enter reference/transaction number
- [ ] Can attach proof of payment (optional)
- [ ] System allocates payment to oldest fees first
- [ ] Receipt generated automatically
- [ ] Parent notified of payment received
- [ ] Payment requires approval before finalization

**Priority**: MUST
**Story Points**: 8
**References**: BR-FM-003

---

### 5.3 Financial Reporting

#### US-FIN-006: Generate Outstanding Fees Report
**As a** finance manager
**I want** to generate reports of outstanding fees
**So that** I can identify accounts needing follow-up

**Acceptance Criteria**:
- [ ] Can generate report for all students or specific grade/class
- [ ] Report shows: Student name, Total fees, Paid amount, Outstanding amount, Age (Current, 30d, 60d, 90d+)
- [ ] Can filter by: Grade, Age category, Amount threshold
- [ ] Overdue accounts (>30 days) highlighted
- [ ] Can export to: PDF, Excel, CSV
- [ ] Report includes: Student contact, Parent contact, Last payment date
- [ ] Can schedule automatic generation (weekly/monthly)
- [ ] Report saved in document repository

**Priority**: MUST
**Story Points**: 13
**References**: REQ-4.5.3, SBR-FR-003

---

#### US-FIN-007: Generate Income Report
**As a** finance manager
**I want** to generate income reports
**So that** I can track revenue and analyze payment trends

**Acceptance Criteria**:
- [ ] Can select time period: Daily, Weekly, Monthly, Term, Annual, Custom
- [ ] Report shows income by: Fee category, Grade, Payment method
- [ ] Report shows: Number of payments, Total amount, Average payment
- [ ] Includes YTD (Year-to-Date) totals
- [ ] Can compare to previous period
- [ ] Can export to: PDF, Excel, CSV
- [ ] Includes visual charts/graphs
- [ ] Report data accurate as of generation timestamp
- [ ] Excludes voided payments

**Priority**: MUST
**Story Points**: 13
**References**: REQ-4.5.3, SBR-FR-004

---

#### US-FIN-008: View Financial Dashboard
**As a** finance manager
**I want** to view a financial dashboard
**So that** I can monitor key financial metrics at a glance

**Acceptance Criteria**:
- [ ] Dashboard shows: Total fees billed (current period), Total payments received, Total outstanding, Outstanding aged (30d, 60d, 90d+)
- [ ] Dashboard shows: Collection rate percentage, Number of overdue accounts
- [ ] Dashboard shows: Today's payments, This week's payments, This month's payments
- [ ] Visual charts for: Income trend, Outstanding trend, Payment methods breakdown
- [ ] Data refreshes every 60 minutes
- [ ] Manual refresh button available
- [ ] Can filter by: Date range, Grade
- [ ] Can drill down into each metric

**Priority**: SHOULD
**Story Points**: 13
**References**: SBR-DB-001, DB-003

---

## 6. System Administration User Stories

### 6.1 Tenant Management

#### US-SYS-001: Create New School Tenant
**As a** system administrator
**I want** to create a new school tenant
**So that** a new school can use the system

**Acceptance Criteria**:
- [ ] Can enter school details: Name, Address, Contact, Email
- [ ] Can upload school logo
- [ ] Can set tenant configuration: Currency, Timezone, Language
- [ ] Can assign storage quota
- [ ] Can create initial admin user for tenant
- [ ] System creates isolated database schema for tenant
- [ ] System sends welcome email to school admin
- [ ] Tenant activated immediately
- [ ] Tenant ID generated automatically

**Priority**: MUST
**Story Points**: 13
**References**: BR-MT-001

---

#### US-SYS-002: Monitor System Performance
**As a** system administrator
**I want** to monitor system performance metrics
**So that** I can ensure the system runs smoothly

**Acceptance Criteria**:
- [ ] Can view dashboard with: CPU usage, Memory usage, Database connections, API response times, Error rate
- [ ] Can view metrics per tenant
- [ ] Can set performance alerts/thresholds
- [ ] Receive alert when threshold breached
- [ ] Can view historical performance data (30 days)
- [ ] Can export metrics to external monitoring tools
- [ ] Dashboard refreshes every 1 minute

**Priority**: SHOULD
**Story Points**: 13
**References**: REQ-5.2, SBR-NF-005

---

#### US-SYS-003: Manage Backups
**As a** system administrator
**I want** to manage system backups
**So that** data can be recovered in case of disaster

**Acceptance Criteria**:
- [ ] Can view backup schedule: Daily at 02:00, Weekly full backup
- [ ] Can view backup history: Date, Type (Full/Incremental), Size, Status
- [ ] Can initiate manual backup on-demand
- [ ] Can download backup file
- [ ] Can restore from backup (with confirmation)
- [ ] Can test backup integrity
- [ ] Receive alert if backup fails
- [ ] Backups retained per policy: Daily (7d), Weekly (4w), Monthly (12m)

**Priority**: MUST
**Story Points**: 13
**References**: REQ-5.3, SBR-NF-002, NF-003

---

#### US-SYS-004: View System Audit Logs
**As a** system administrator
**I want** to view comprehensive audit logs
**So that** I can track all system activities for security and compliance

**Acceptance Criteria**:
- [ ] Can view all user actions: Login, Logout, Create, Update, Delete, View
- [ ] Can filter by: User, Tenant, Action type, Date range, Entity type
- [ ] Can search by keywords
- [ ] Each log entry shows: Timestamp, User, Action, Entity, Before/After values, IP address
- [ ] Can export logs to CSV
- [ ] Logs retained for 7 years (compliance requirement)
- [ ] Logs immutable (cannot be deleted or modified)

**Priority**: MUST
**Story Points**: 8
**References**: BR-SA-002, BR-DI-001

---

#### US-SYS-005: Manage Security Patches
**As a** system administrator
**I want** to apply security patches promptly
**So that** system vulnerabilities are minimized

**Acceptance Criteria**:
- [ ] Can view available patches with severity: Critical, High, Medium, Low
- [ ] Can view patch details: Description, CVE references, Impact, Release date
- [ ] Can schedule patch application
- [ ] System tests patch on staging before production
- [ ] Receive notification for new critical patches
- [ ] Can rollback patch if issues occur
- [ ] Patch application logged in audit trail
- [ ] Target timelines enforced: Critical (48h), High (7d), Medium (30d)

**Priority**: MUST
**Story Points**: 13
**References**: SBR-NF-006

---

## 7. Admissions User Stories

### 7.1 Prospective Parent Application

#### US-APP-001: Submit School Application
**As a** prospective parent
**I want** to submit an online application for my child
**So that** my child can be considered for admission to the school

**Acceptance Criteria**:
- [ ] Can access public application form (no login required initially)
- [ ] Can create account with email and password
- [ ] Can enter child's personal details (name, DOB, ID/passport, gender)
- [ ] Can enter parent/guardian information for up to 4 parents
- [ ] Can mark primary contact and financially responsible parent
- [ ] Can select grade applying for
- [ ] System validates age-grade appropriateness (±2 years tolerance)
- [ ] System validates SA ID number using Luhn algorithm
- [ ] Can save application as draft and complete later
- [ ] Can submit application when all required fields complete
- [ ] Receive confirmation email with application number upon submission
- [ ] Application number format: APP-{TenantId}-{Year}-{Sequence}

**Priority**: MUST
**Story Points**: 13
**References**: BR-ADM-001 to ADM-005

---

#### US-APP-002: Upload Required Documents
**As a** prospective parent
**I want** to upload required documents for my application
**So that** the school can verify my child's information

**Acceptance Criteria**:
- [ ] Can see list of required documents based on grade and citizenship
- [ ] Can upload documents: PDF, JPG, PNG (max 10MB each)
- [ ] Required for all: Birth certificate/Passport, Parent IDs, Proof of residence, Passport photo
- [ ] Required for Grade R: Immunization record
- [ ] Required for Grades 1-12: Previous report card, Transfer letter
- [ ] Can upload multiple files per document category
- [ ] Can see upload progress indicator
- [ ] Can delete uploaded document before admin verification
- [ ] Can see verification status per document (Pending, Verified, Rejected)
- [ ] Receive notification when document rejected with reason
- [ ] All files pass virus scan before storage

**Priority**: MUST
**Story Points**: 8
**References**: BR-ADM-008 to ADM-010

---

#### US-APP-003: Pay Application Fee
**As a** prospective parent
**I want** to pay the application fee online
**So that** my application can proceed to review

**Acceptance Criteria**:
- [ ] Can see application fee amount displayed
- [ ] Can select payment method: Card, EFT, Capitec, Ozow, Instant EFT
- [ ] Payment processed through secure gateway
- [ ] Receive payment confirmation immediately
- [ ] Receipt generated automatically
- [ ] Receipt sent via email
- [ ] Receipt downloadable from portal
- [ ] Application status changes from PaymentPending → UnderReview after payment
- [ ] See notification that fee is non-refundable
- [ ] Payment must be completed within 7 days of submission

**Priority**: MUST
**Story Points**: 8
**References**: BR-ADM-006, BR-ADM-007

---

#### US-APP-004: Track Application Status
**As a** prospective parent
**I want** to track the status of my application
**So that** I know what stage my application is at

**Acceptance Criteria**:
- [ ] Can view application dashboard showing current status
- [ ] Status options: Draft, Submitted, PaymentPending, UnderReview, DocumentsRequired, InterviewScheduled, AssessmentScheduled, UnderConsideration, Approved, Rejected, Waitlisted, Expired, Withdrawn, Enrolled
- [ ] Can see status history timeline with dates
- [ ] Can see pending actions required (e.g., upload documents, pay fee)
- [ ] Receive notifications for status changes
- [ ] Can see next steps for each status
- [ ] Can see estimated timeline for decision

**Priority**: MUST
**Story Points**: 5
**References**: BR-ADM-005

---

#### US-APP-005: Respond to Interview Invitation
**As a** prospective parent
**I want** to schedule or confirm my interview appointment
**So that** I can meet with the school administration

**Acceptance Criteria**:
- [ ] Receive email/SMS notification when interview scheduled
- [ ] Can see interview details: Date, Time, Location/Meeting link, Interviewer
- [ ] For in-person: Can see school address and directions
- [ ] For online: Can see meeting link (active 15 minutes before)
- [ ] Can request to reschedule (up to 2 times)
- [ ] Receive reminder 24 hours before interview
- [ ] Can add interview to personal calendar (iCal download)
- [ ] Can see interview preparation guidelines

**Priority**: SHOULD
**Story Points**: 5
**References**: BR-ADM-011 to ADM-013

---

#### US-APP-006: Attend Placement Assessment
**As a** prospective parent
**I want** to bring my child for the placement assessment
**So that** the school can evaluate grade readiness

**Acceptance Criteria**:
- [ ] Receive notification when assessment scheduled
- [ ] Can see assessment details: Date, Time, Location, Subjects, Duration
- [ ] Can see preparation guidelines and requirements to bring
- [ ] Receive reminder 48 hours before assessment
- [ ] Can request to reschedule once if needed
- [ ] Receive notification when assessment results available
- [ ] Can view assessment feedback

**Priority**: SHOULD
**Story Points**: 3
**References**: BR-ADM-014 to ADM-016

---

#### US-APP-007: Receive Admission Decision
**As a** prospective parent
**I want** to be notified of the admission decision
**So that** I know if my child has been accepted

**Acceptance Criteria**:
- [ ] Receive notification via email + SMS when decision made
- [ ] Decision options: Accepted, Accepted with Conditions, Rejected, Waitlisted
- [ ] For Accepted: See acceptance letter, conditions (if any), enrollment steps, offer expiry date (14 days)
- [ ] For Accepted with Conditions: See clear list of conditions
- [ ] For Rejected: See respectful notification (reasons not detailed)
- [ ] For Waitlisted: See current position and estimated wait time
- [ ] Can download decision letter (PDF)

**Priority**: MUST
**Story Points**: 5
**References**: BR-ADM-017 to ADM-020

---

#### US-APP-008: Accept Admission Offer
**As a** prospective parent
**I want** to accept the admission offer
**So that** my child can be enrolled

**Acceptance Criteria**:
- [ ] Can accept offer via portal
- [ ] Can see enrollment requirements checklist
- [ ] Can complete enrollment form with additional details
- [ ] Can pay registration fee online
- [ ] Can upload medical forms
- [ ] Can opt-in for transport/aftercare/extramurals
- [ ] Can order uniform (if applicable)
- [ ] Can sign enrollment contract digitally
- [ ] Can sign POPIA consent forms
- [ ] Must complete within 14 days of acceptance
- [ ] Receive reminder at 7 days, 3 days, 1 day remaining
- [ ] Receive confirmation when enrollment complete

**Priority**: MUST
**Story Points**: 13
**References**: BR-ADM-018, BR-ADM-025 to ADM-027

---

#### US-APP-009: View Waitlist Status
**As a** prospective parent
**I want** to view my waitlist position and status
**So that** I know if a position may become available

**Acceptance Criteria**:
- [ ] Can see current waitlist position
- [ ] Can see total number of applicants on waitlist
- [ ] Can see estimated wait time (if available)
- [ ] Receive notification when position moves up
- [ ] Receive notification when position becomes available (offer sent)
- [ ] Can respond to offer: Accept or Decline (within 7 days)
- [ ] Can withdraw from waitlist anytime
- [ ] See waitlist expiry date (typically Dec 31)

**Priority**: SHOULD
**Story Points**: 5
**References**: BR-ADM-021 to ADM-024

---

#### US-APP-010: Withdraw Application
**As a** prospective parent
**I want** to withdraw my application
**So that** I can notify the school if I'm no longer interested

**Acceptance Criteria**:
- [ ] Can withdraw application at any stage before enrollment
- [ ] Must confirm withdrawal (confirmation dialog)
- [ ] Can optionally provide withdrawal reason
- [ ] Receive confirmation email
- [ ] Application status changes to Withdrawn
- [ ] If on waitlist, position freed immediately
- [ ] If interview/assessment scheduled, automatically cancelled
- [ ] Understand application fee is NOT refunded
- [ ] Withdrawn application retained in system for records

**Priority**: SHOULD
**Story Points**: 3
**References**: BR-ADM-031

---

### 7.2 School Administration - Admissions Management

#### US-ADMN-001: Review Applications
**As an** admissions officer
**I want** to review submitted applications
**So that** I can evaluate applicants for admission

**Acceptance Criteria**:
- [ ] Can view list of all applications filtered by status
- [ ] Can filter by: Grade, Status, Application date, Decision
- [ ] Can search by: Application number, Student name, Parent name
- [ ] Can view application details: Student info, Parent info, Documents, Fees
- [ ] Can see application completeness checklist
- [ ] Can see application fee payment status
- [ ] Can add internal notes (not visible to parent)
- [ ] Can assign application to specific reviewer
- [ ] Can see application history timeline

**Priority**: MUST
**Story Points**: 8
**References**: BR-ADM-005

---

#### US-ADMN-002: Verify Documents
**As an** admissions officer
**I want** to verify uploaded documents
**So that** applications have authentic and complete documentation

**Acceptance Criteria**:
- [ ] Can view all uploaded documents per application
- [ ] Can download and open documents
- [ ] Can mark document as: Verified or Rejected
- [ ] If rejected, must provide reason (min 20 characters)
- [ ] Parent receives notification for rejected documents
- [ ] Can request additional documents
- [ ] Can see verification history per document
- [ ] Cannot proceed to decision if required documents not verified
- [ ] Verification action logged with user ID and timestamp

**Priority**: MUST
**Story Points**: 8
**References**: BR-ADM-010

---

#### US-ADMN-003: Schedule Interviews
**As an** admissions officer
**I want** to schedule interviews for applicants
**So that** we can meet prospective students and parents

**Acceptance Criteria**:
- [ ] Can schedule interview for specific application
- [ ] Can set: Date, Time, Duration, Location (Office/Online), Interviewer
- [ ] For online interviews, can generate meeting link
- [ ] Interview must be scheduled 48+ hours in advance
- [ ] Can assign interviewer (Principal, HOD, Admissions Officer)
- [ ] Parent receives email/SMS notification automatically
- [ ] Reminder sent 24 hours before interview
- [ ] Can reschedule interview (max 2 times per application)
- [ ] Can cancel interview if needed
- [ ] Application status changes to InterviewScheduled

**Priority**: SHOULD
**Story Points**: 8
**References**: BR-ADM-012

---

#### US-ADMN-004: Conduct and Rate Interview
**As an** interviewer
**I want** to record interview outcomes and ratings
**So that** interviews factor into admission decisions

**Acceptance Criteria**:
- [ ] Can see scheduled interviews for my assignments
- [ ] Can mark interview status: Completed, Rescheduled, Cancelled, NoShow
- [ ] If Completed: Must provide rating (1-5 scale)
- [ ] If Completed: Must provide recommendation (Yes/No)
- [ ] Can add interview notes (up to 2000 characters)
- [ ] Rating and notes only visible to admin/principal
- [ ] Parent notified when interview completed
- [ ] If NoShow: Can reschedule or mark as failed to attend
- [ ] Interview completion date recorded
- [ ] Application proceeds to next stage after interview

**Priority**: SHOULD
**Story Points**: 8
**References**: BR-ADM-013

---

#### US-ADMN-005: Schedule Placement Assessments
**As an** admissions officer
**I want** to schedule placement assessments for applicants
**So that** we can evaluate academic readiness

**Acceptance Criteria**:
- [ ] Can schedule assessment for specific application
- [ ] Can set: Date, Time, Subjects (Math, English, etc.), Assessor
- [ ] Assessment must be scheduled 7+ days in advance
- [ ] Can assign assessor (qualified teacher/HOD)
- [ ] Parent receives notification with preparation guidelines
- [ ] Reminder sent 48 hours before assessment
- [ ] Can reschedule once if parent requests
- [ ] Application status changes to AssessmentScheduled

**Priority**: SHOULD
**Story Points**: 5
**References**: BR-ADM-015

---

#### US-ADMN-006: Record Assessment Results
**As an** assessor
**I want** to record assessment scores and feedback
**So that** results factor into admission decisions

**Acceptance Criteria**:
- [ ] Can see scheduled assessments for my assignments
- [ ] Can enter: Total score, Max score (percentage auto-calculated)
- [ ] Can mark assessment as: Passed or Failed
- [ ] Pass mark configurable per grade (typical: 40-50%)
- [ ] Must provide feedback (up to 1000 characters)
- [ ] Can indicate areas of strength and weakness
- [ ] Parent receives notification when results available
- [ ] Assessment completion date recorded
- [ ] Can allow one retake if failed (14 days gap)

**Priority**: SHOULD
**Story Points**: 5
**References**: BR-ADM-016

---

#### US-ADMN-007: Make Admission Decision
**As a** principal/admissions director
**I want** to approve or reject applications
**So that** prospective students know admission outcome

**Acceptance Criteria**:
- [ ] Can only make decision if: Fee paid, Documents verified, Interview completed (if required), Assessment completed (if required)
- [ ] Application must be in UnderConsideration status
- [ ] Decision options: Accepted, Accepted with Conditions, Rejected, Waitlisted
- [ ] For Accepted: Offer expiry set automatically (14 days)
- [ ] For Accepted with Conditions: Must document conditions (min 50 characters)
- [ ] For Rejected: Must provide reason (min 50 characters, audit only)
- [ ] For Waitlisted: Position assigned automatically (FIFO)
- [ ] Parent receives decision notification immediately
- [ ] Decision letter generated (PDF)
- [ ] Decision action logged with user ID and timestamp
- [ ] Cannot change decision once made (audit trail protection)

**Priority**: MUST
**Story Points**: 13
**References**: BR-ADM-017 to ADM-020

---

#### US-ADMN-008: Manage Waitlist
**As an** admissions officer
**I want** to manage the waitlist for each grade
**So that** positions can be offered as they become available

**Acceptance Criteria**:
- [ ] Can view waitlist per grade
- [ ] Can see: Position, Application number, Student name, Application date, Status
- [ ] Waitlist sorted by position (automatic FIFO)
- [ ] When position available, can offer to next applicant
- [ ] System automatically sends offer notification (email + SMS)
- [ ] Offer expiry: 7 days (shorter than regular acceptance)
- [ ] Can track offer responses: Pending, Accepted, Declined, Expired
- [ ] If declined/expired, offer automatically goes to next applicant
- [ ] Can manually remove applicant from waitlist (with reason)
- [ ] Can extend offer expiry (with approval)
- [ ] Can see waitlist statistics per grade

**Priority**: SHOULD
**Story Points**: 13
**References**: BR-ADM-021 to ADM-024

---

#### US-ADMN-009: Convert Application to Student
**As an** admissions officer
**I want** to convert accepted and enrolled applications to student records
**So that** students can access the school system

**Acceptance Criteria**:
- [ ] Can only convert if: Application status = Enrolled, Registration fee paid, Enrollment form complete
- [ ] System creates Student record with: Personal details from application, Grade and class assignment, Admission number (auto-generated), Parent relationships linked
- [ ] System creates Parent accounts (if not existing)
- [ ] System links parent to student
- [ ] Fee structure assigned to student
- [ ] Student and parent portal accounts activated
- [ ] Welcome email sent to parent and student
- [ ] Application.CreatedStudentId populated with new Student.Id
- [ ] Application status remains Enrolled (retained for audit)
- [ ] Conversion action logged in audit trail

**Priority**: MUST
**Story Points**: 13
**References**: BR-ADM-026, BR-ADM-027

---

#### US-ADMN-010: Monitor Grade Capacity
**As an** admissions officer
**I want** to monitor enrollment capacity per grade
**So that** I don't exceed school capacity limits

**Acceptance Criteria**:
- [ ] Can view capacity dashboard per grade
- [ ] Dashboard shows: Maximum capacity, Current enrolled, Accepted (not yet enrolled), Available positions
- [ ] Dashboard shows: Applications in progress, Waitlist count
- [ ] Visual indicator: Green (capacity available), Yellow (80% full), Red (full)
- [ ] Cannot approve application if capacity full (auto-waitlist)
- [ ] Can request capacity increase (requires principal approval with reason)
- [ ] Capacity includes enrolled + accepted applicants
- [ ] Can export capacity report
- [ ] Can set alerts when capacity reaches threshold (e.g., 90%)

**Priority**: SHOULD
**Story Points**: 8
**References**: BR-ADM-028, BR-ADM-029

---

#### US-ADMN-011: Configure Admissions Settings
**As a** principal/admin
**I want** to configure admissions settings for my school
**So that** the admissions process matches our school policies

**Acceptance Criteria**:
- [ ] Can set application fee amount per tenant
- [ ] Can set grade capacity limits per grade
- [ ] Can configure: Interview required (Yes/No) per grade
- [ ] Can configure: Assessment required (Yes/No) per grade
- [ ] Can set acceptance offer expiry period (default: 14 days)
- [ ] Can set waitlist offer expiry period (default: 7 days)
- [ ] Can set application expiry period (default: 12 months)
- [ ] Can configure assessment pass marks per grade
- [ ] Can enable/disable online interviews
- [ ] Can set interview duration limits
- [ ] Settings apply tenant-wide
- [ ] Changes logged in audit trail

**Priority**: MUST
**Story Points**: 8
**References**: BR-ADM-011, BR-ADM-014, BR-ADM-018, BR-ADM-030

---

## Story Summary Statistics

### Total User Stories: 111 (was 90)

**By Role**:
- School Administration: 15 stories
- Teachers: 13 stories
- Parents & Guardians: 11 stories
- Students: 10 stories
- Finance: 8 stories
- System Administration: 5 stories
- **Admissions (Prospective Parents): 10 stories** *(NEW)*
- **Admissions (School Admin): 11 stories** *(NEW)*

**By Priority**:
- MUST: 76 stories (68%) - was 62
- SHOULD: 31 stories (28%) - was 24
- COULD: 4 stories (4%)
- WON'T: 0 stories

**By Story Points Range**:
- Small (1-5 points): 41 stories - was 32
- Medium (8-13 points): 62 stories - was 50
- Large (21+ points): 8 stories

**Total Estimated Story Points**: 800 points (was 622)

**Estimated Sprints** (at 40 points per sprint): 20 sprints (~10 months at 2-week sprints) - was 16 sprints

---

## Sprint Planning Recommendations

### Sprint 1-2 (Foundation - MVP Core)
- User authentication and roles
- Student registration
- Academic year/term setup
- Grade and class setup
- Teacher management
- Basic parent linking
- **Admissions module setup** *(NEW)*

### Sprint 3-4 (Admissions Module) *(NEW)*
- **Application submission by parents**
- **Document upload and verification**
- **Application fee payment**
- **Interview scheduling and management**
- **Placement assessment**
- **Admission decision workflow**

### Sprint 5-6 (Waitlist & Enrollment) *(NEW)*
- **Waitlist management**
- **Offer acceptance**
- **Application-to-student conversion**
- **Grade capacity management**

### Sprint 7-8 (Academic Management) *(was Sprint 3-4)*
- Timetable creation and management
- Subject enrollment
- Teacher-subject-class assignments
- Academic calendar

### Sprint 9-10 (Assessment & Marking) *(was Sprint 5-6)*
- Mark capture
- Grading system
- Report generation
- Report publishing

### Sprint 11-12 (Financial Management) *(was Sprint 7-8)*
- Fee structure setup
- Payment processing
- Receipt generation
- Fee statements

### Sprint 13-14 (Learning Materials & Content) *(was Sprint 9-10)*
- Material upload and management
- Material categorization
- Document repository
- Content access control

### Sprint 15-16 (Online Learning) *(was Sprint 11-12)*
- Online lesson scheduling
- Live class hosting (integration)
- Lesson recordings
- Quiz/assessment platform

### Sprint 17-18 (Communication & Notifications) *(was Sprint 13-14)*
- Announcement system
- Notification engine
- Email integration
- SMS integration
- Parent-teacher messaging

### Sprint 19-20 (Reporting & Analytics) *(was Sprint 15-16)*
- Performance dashboards
- Financial reports
- Academic reports
- Analytics

---

**End of User Stories Document**
