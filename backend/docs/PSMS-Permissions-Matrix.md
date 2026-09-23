# PSMS Permissions Matrix

## Overview

This document defines the complete Role-Based Access Control (RBAC) matrix for the Private School Management System (PSMS). It covers all modules, roles, and permissions based on the business rules defined in:
- `PSMS-Business-Rules.md` (SE-001, SE-002)
- `PSMS-Business-Rules-ADMISSIONS.md` (ADM-010, ADM-012, ADM-015, ADM-017)
- `PSMS-Business-Rules-SUPPLEMENTARY.md`
- `PSMS-User-Stories.md`

**Framework**: ASP.NET Boilerplate (ABP)
**Permission Pattern**: `{Module}.{Entity}.{Action}`

---

## Table of Contents

1. [System Roles](#1-system-roles)
2. [Permission Legend](#2-permission-legend)
3. [Admissions Module Permissions](#3-admissions-module-permissions)
4. [Academic Module Permissions](#4-academic-module-permissions)
5. [Financial Module Permissions](#5-financial-module-permissions)
6. [Assessment Module Permissions](#6-assessment-module-permissions)
7. [Communication Module Permissions](#7-communication-module-permissions)
8. [Learning Module Permissions](#8-learning-module-permissions)
9. [System Administration Permissions](#9-system-administration-permissions)
10. [Permission Implementation](#10-permission-implementation)

---

## 1. System Roles

### 1.1 Role Hierarchy

```
Host Admin (Platform Level)
│
└── Tenant Roles (School Level)
    ├── School Admin (Tenant Administrator)
    ├── Principal
    ├── Vice Principal
    ├── Head of Department (HOD)
    ├── Admissions Officer
    ├── Finance Manager
    ├── Teacher
    ├── Parent/Guardian
    └── Student
```

### 1.2 Role Definitions

| Role | Code | Description | Scope |
|------|------|-------------|-------|
| **Host Admin** | `HostAdmin` | Platform administrator, manages all tenants | Cross-tenant |
| **School Admin** | `Admin` | School IT administrator, full tenant access | Tenant-wide |
| **Principal** | `Principal` | School principal, all academic/admission decisions | Tenant-wide |
| **Vice Principal** | `VicePrincipal` | Assists principal, limited decision authority | Tenant-wide |
| **Head of Department** | `HOD` | Manages specific subject area/grade | Department-scoped |
| **Admissions Officer** | `AdmissionsOfficer` | Manages admission applications | Admissions only |
| **Finance Manager** | `Finance` | Manages fees, payments, financial reporting | Financial only |
| **Teacher** | `Teacher` | Manages assigned classes and subjects | Class/Subject-scoped |
| **Parent/Guardian** | `Parent` | Views child's information, makes payments | Child-scoped |
| **Student** | `Student` | Views own information, submits work | Self-scoped |
| **Prospective Parent** | `Applicant` | Submits and tracks applications | Application-scoped |

---

## 2. Permission Legend

| Symbol | Meaning |
|--------|---------|
| ✅ | Full access |
| 📖 | Read-only access |
| 🔒 | Own data only |
| ⚙️ | Requires configuration/approval |
| ❌ | No access |
| 👶 | Child's data only |
| 📚 | Assigned classes/subjects only |

---

## 3. Admissions Module Permissions

### 3.1 Application Management

| Permission | Host Admin | Admin | Principal | Admissions Officer | Teacher | Parent | Student | Applicant |
|------------|:----------:|:-----:|:---------:|:------------------:|:-------:|:------:|:-------:|:---------:|
| **Applications.View** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | 🔒 |
| **Applications.ViewAll** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Applications.Create** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ |
| **Applications.Edit** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | 🔒 |
| **Applications.Delete** | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Applications.Submit** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | 🔒 |
| **Applications.Withdraw** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | 🔒 |

### 3.2 Application Review & Decision (ADM-017)

| Permission | Host Admin | Admin | Principal | VP | HOD | Admissions Officer | Teacher |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:------------------:|:-------:|
| **Review.StartReview** | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ |
| **Review.RequestDocuments** | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ |
| **Review.MarkDocumentsComplete** | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ |
| **Decision.Approve** | ❌ | ⚙️ | ✅ | ⚙️ | ❌ | ❌ | ❌ |
| **Decision.Reject** | ❌ | ⚙️ | ✅ | ⚙️ | ❌ | ❌ | ❌ |
| **Decision.Waitlist** | ❌ | ⚙️ | ✅ | ⚙️ | ❌ | ❌ | ❌ |
| **Decision.ExtendOffer** | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |

> **Note**: Admission decisions (Approve/Reject/Waitlist) require `Admissions.MakeDecision` permission per ADM-017. Only Principal and designated users have this permission.

### 3.3 Document Management (ADM-010)

| Permission | Host Admin | Admin | Principal | Admissions Officer | Teacher | Parent | Applicant |
|------------|:----------:|:-----:|:---------:|:------------------:|:-------:|:------:|:---------:|
| **Documents.Upload** | ✅ | ✅ | ✅ | ✅ | ❌ | 👶 | 🔒 |
| **Documents.View** | ✅ | ✅ | ✅ | ✅ | ❌ | 👶 | 🔒 |
| **Documents.Download** | ✅ | ✅ | ✅ | ✅ | ❌ | 👶 | 🔒 |
| **Documents.Verify** | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Documents.Reject** | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Documents.Delete** | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |

### 3.4 Interview Management (ADM-012)

| Permission | Host Admin | Admin | Principal | VP | HOD | Admissions Officer | Teacher |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:------------------:|:-------:|
| **Interviews.View** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Interviews.Schedule** | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ |
| **Interviews.Reschedule** | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ |
| **Interviews.Cancel** | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ |
| **Interviews.Conduct** | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Interviews.RecordOutcome** | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |

> **Note**: Interview conductors must have `Admissions.ConductInterviews` permission per ADM-012.

### 3.5 Assessment Management (ADM-015, ADM-016)

| Permission | Host Admin | Admin | Principal | VP | HOD | Admissions Officer | Teacher |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:------------------:|:-------:|
| **Assessments.View** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 📚 |
| **Assessments.Schedule** | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ |
| **Assessments.Cancel** | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ |
| **Assessments.Conduct** | ❌ | ❌ | ✅ | ✅ | ✅ | ❌ | ✅ |
| **Assessments.RecordResults** | ❌ | ❌ | ✅ | ✅ | ✅ | ❌ | ✅ |

> **Note**: Assessors must have `Admissions.ConductAssessments` permission per ADM-015.

### 3.6 Waitlist Management (ADM-021-024)

| Permission | Host Admin | Admin | Principal | VP | Admissions Officer | Parent | Applicant |
|------------|:----------:|:-----:|:---------:|:--:|:------------------:|:------:|:---------:|
| **Waitlist.View** | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | 🔒 |
| **Waitlist.ViewAll** | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Waitlist.OfferPosition** | ❌ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Waitlist.AcceptOffer** | ❌ | ❌ | ❌ | ❌ | ❌ | 👶 | 🔒 |
| **Waitlist.DeclineOffer** | ❌ | ❌ | ❌ | ❌ | ❌ | 👶 | 🔒 |
| **Waitlist.Withdraw** | ❌ | ✅ | ✅ | ✅ | ✅ | 👶 | 🔒 |

### 3.7 Enrollment Management (ADM-025-029)

| Permission | Host Admin | Admin | Principal | VP | Admissions Officer | Parent | Applicant |
|------------|:----------:|:-----:|:---------:|:--:|:------------------:|:------:|:---------:|
| **Enrollment.View** | ✅ | ✅ | ✅ | ✅ | ✅ | 👶 | 🔒 |
| **Enrollment.AcceptOffer** | ❌ | ❌ | ❌ | ❌ | ❌ | 👶 | 🔒 |
| **Enrollment.SubmitForms** | ❌ | ✅ | ✅ | ✅ | ✅ | 👶 | 🔒 |
| **Enrollment.AssignClass** | ❌ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Enrollment.Complete** | ❌ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |

### 3.8 Admission Settings

| Permission | Host Admin | Admin | Principal | VP | Admissions Officer |
|------------|:----------:|:-----:|:---------:|:--:|:------------------:|
| **Settings.View** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Settings.Manage** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Settings.OpenApplications** | ❌ | ✅ | ✅ | ❌ | ❌ |
| **Settings.CloseApplications** | ❌ | ✅ | ✅ | ❌ | ❌ |
| **Settings.UpdateCapacity** | ❌ | ⚙️ | ✅ | ❌ | ❌ |

---

## 4. Academic Module Permissions

### 4.1 Student Management

| Permission | Host Admin | Admin | Principal | VP | HOD | Teacher | Parent | Student |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:-------:|:------:|:-------:|
| **Students.View** | ✅ | ✅ | ✅ | ✅ | ✅ | 📚 | 👶 | 🔒 |
| **Students.ViewAll** | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Students.Create** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Students.Edit** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Students.Delete** | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Students.AssignClass** | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |

### 4.2 Teacher Management

| Permission | Host Admin | Admin | Principal | VP | HOD | Teacher |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:-------:|
| **Teachers.View** | ✅ | ✅ | ✅ | ✅ | ✅ | 📖 |
| **Teachers.ViewAll** | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Teachers.Create** | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Teachers.Edit** | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Teachers.Delete** | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Teachers.AssignSubject** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Teachers.AssignClass** | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |

### 4.3 Class & Grade Management

| Permission | Host Admin | Admin | Principal | VP | HOD | Teacher |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:-------:|
| **Grades.View** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Grades.Manage** | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Classes.View** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Classes.Create** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Classes.Edit** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Classes.Delete** | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |

### 4.4 Timetable Management (TT-001 to TT-007)

| Permission | Host Admin | Admin | Principal | VP | HOD | Teacher | Parent | Student |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:-------:|:------:|:-------:|
| **Timetables.View** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 👶 | 🔒 |
| **Timetables.Create** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Timetables.Edit** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Timetables.Delete** | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Timetables.Publish** | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Timetables.MakeVariations** | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |

### 4.5 Attendance Management (AT-001 to AT-003)

| Permission | Host Admin | Admin | Principal | VP | HOD | Teacher | Parent | Student |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:-------:|:------:|:-------:|
| **Attendance.View** | ✅ | ✅ | ✅ | ✅ | ✅ | 📚 | 👶 | 🔒 |
| **Attendance.ViewAll** | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Attendance.Capture** | ❌ | ✅ | ✅ | ✅ | ✅ | 📚 | ❌ | ❌ |
| **Attendance.Edit** | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Attendance.Reports** | ✅ | ✅ | ✅ | ✅ | ✅ | 📚 | 👶 | 🔒 |

### 4.6 Academic Calendar

| Permission | Host Admin | Admin | Principal | VP | Teacher | Parent | Student |
|------------|:----------:|:-----:|:---------:|:--:|:-------:|:------:|:-------:|
| **Calendar.View** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Calendar.Manage** | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **AcademicYears.Manage** | ❌ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Terms.Manage** | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |

---

## 5. Financial Module Permissions

### 5.1 Fee Structure Management (FM-001, FM-002)

| Permission | Host Admin | Admin | Principal | Finance | Teacher | Parent |
|------------|:----------:|:-----:|:---------:|:-------:|:-------:|:------:|
| **FeeStructures.View** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| **FeeStructures.Create** | ✅ | ✅ | ⚙️ | ✅ | ❌ | ❌ |
| **FeeStructures.Edit** | ✅ | ✅ | ⚙️ | ✅ | ❌ | ❌ |
| **FeeStructures.Delete** | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **FeeStructures.Approve** | ❌ | ✅ | ✅ | ❌ | ❌ | ❌ |

### 5.2 Payment Management (FM-003 to FM-005)

| Permission | Host Admin | Admin | Principal | Finance | Admissions | Parent |
|------------|:----------:|:-----:|:---------:|:-------:|:----------:|:------:|
| **Payments.View** | ✅ | ✅ | ✅ | ✅ | 📖 | 👶 |
| **Payments.ViewAll** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Payments.Process** | ❌ | ✅ | ❌ | ✅ | ✅ | 👶 |
| **Payments.RecordManual** | ❌ | ✅ | ❌ | ✅ | ✅ | ❌ |
| **Payments.Reconcile** | ❌ | ✅ | ❌ | ✅ | ❌ | ❌ |
| **Payments.Void** | ❌ | ✅ | ⚙️ | ⚙️ | ❌ | ❌ |

### 5.3 Receipts (PR-001 to PR-006)

| Permission | Host Admin | Admin | Principal | Finance | Admissions | Parent |
|------------|:----------:|:-----:|:---------:|:-------:|:----------:|:------:|
| **Receipts.View** | ✅ | ✅ | ✅ | ✅ | 📖 | 👶 |
| **Receipts.Download** | ✅ | ✅ | ✅ | ✅ | ❌ | 👶 |
| **Receipts.Void** | ❌ | ✅ | ⚙️ | ⚙️ | ❌ | ❌ |
| **Receipts.Resend** | ❌ | ✅ | ❌ | ✅ | ❌ | ❌ |

### 5.4 Financial Reporting (FR-001 to FR-006)

| Permission | Host Admin | Admin | Principal | Finance | Teacher | Parent |
|------------|:----------:|:-----:|:---------:|:-------:|:-------:|:------:|
| **Reports.OutstandingFees** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Reports.Income** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Reports.Collections** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Reports.Export** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Statements.View** | ✅ | ✅ | ✅ | ✅ | ❌ | 👶 |
| **Statements.Generate** | ✅ | ✅ | ✅ | ✅ | ❌ | 👶 |

---

## 6. Assessment Module Permissions

### 6.1 Mark Management (GR-001 to GR-006)

| Permission | Host Admin | Admin | Principal | VP | HOD | Teacher | Parent | Student |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:-------:|:------:|:-------:|
| **Marks.View** | ✅ | ✅ | ✅ | ✅ | ✅ | 📚 | 👶 | 🔒 |
| **Marks.ViewAll** | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Marks.Create** | ❌ | ✅ | ✅ | ✅ | ✅ | 📚 | ❌ | ❌ |
| **Marks.Edit** | ❌ | ✅ | ✅ | ✅ | ✅ | 🔒 | ❌ | ❌ |
| **Marks.Delete** | ❌ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Marks.Publish** | ❌ | ✅ | ✅ | ✅ | ✅ | 📚 | ❌ | ❌ |
| **Marks.Unlock** | ❌ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Marks.Import** | ❌ | ✅ | ✅ | ✅ | ✅ | 📚 | ❌ | ❌ |

### 6.2 Assessment/Quiz Management (QA-001 to QA-007)

| Permission | Host Admin | Admin | Principal | VP | HOD | Teacher | Parent | Student |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:-------:|:------:|:-------:|
| **Quizzes.View** | ✅ | ✅ | ✅ | ✅ | ✅ | 📚 | ❌ | 📚 |
| **Quizzes.Create** | ❌ | ✅ | ✅ | ✅ | ✅ | 📚 | ❌ | ❌ |
| **Quizzes.Edit** | ❌ | ✅ | ✅ | ✅ | ✅ | 🔒 | ❌ | ❌ |
| **Quizzes.Delete** | ❌ | ✅ | ✅ | ✅ | ✅ | 🔒 | ❌ | ❌ |
| **Quizzes.Publish** | ❌ | ✅ | ✅ | ✅ | ✅ | 📚 | ❌ | ❌ |
| **Quizzes.Attempt** | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | 📚 |
| **Quizzes.ViewResults** | ✅ | ✅ | ✅ | ✅ | ✅ | 📚 | 👶 | 🔒 |

### 6.3 Report Cards (RP-001 to RP-003)

| Permission | Host Admin | Admin | Principal | VP | HOD | Teacher | Parent | Student |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:-------:|:------:|:-------:|
| **ReportCards.View** | ✅ | ✅ | ✅ | ✅ | ✅ | 📚 | 👶 | 🔒 |
| **ReportCards.Generate** | ❌ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **ReportCards.Publish** | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **ReportCards.Download** | ✅ | ✅ | ✅ | ✅ | ✅ | 📚 | 👶 | 🔒 |
| **ReportCards.Comment** | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |

**ReportCards.Comment** (RC-08) is the teacher comment on a report card and on
each of its subjects. It is split out of Generate because the class and subject
teachers are who actually write those comments, and a teacher holds no Generate
— so before this existed, a named field of a South African report card could not
be written by the person whose name is printed under it. Generating and deleting
report cards stays with the HOD and up.

### 6.4 Feedback (TF-001 to TF-005)

| Permission | Host Admin | Admin | Principal | VP | HOD | Teacher | Parent | Student |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:-------:|:------:|:-------:|
| **Feedback.View** | ✅ | ✅ | ✅ | ✅ | ✅ | 📚 | 👶 | 🔒 |
| **Feedback.Create** | ❌ | ✅ | ✅ | ✅ | ✅ | 📚 | ❌ | ❌ |
| **Feedback.Edit** | ❌ | ✅ | ✅ | ✅ | ✅ | 🔒 | ❌ | ❌ |
| **Feedback.Delete** | ❌ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |

---

## 7. Communication Module Permissions

### 7.1 Announcements (CM-001)

| Permission | Host Admin | Admin | Principal | VP | HOD | Teacher | Parent | Student |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:-------:|:------:|:-------:|
| **Announcements.View** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Announcements.Create** | ✅ | ✅ | ✅ | ✅ | ✅ | 📚 | ❌ | ❌ |
| **Announcements.Edit** | ✅ | ✅ | ✅ | ✅ | ✅ | 🔒 | ❌ | ❌ |
| **Announcements.Delete** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Announcements.SendSchoolWide** | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |

### 7.2 Messaging (CM-002)

| Permission | Host Admin | Admin | Principal | VP | HOD | Teacher | Parent | Student |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:-------:|:------:|:-------:|
| **Messages.View** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Messages.Send** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Messages.SendToAll** | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Messages.Delete** | ❌ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |

### 7.3 Notifications (NT-001 to NT-007)

| Permission | Host Admin | Admin | Principal | VP | HOD | Teacher | Parent | Student |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:-------:|:------:|:-------:|
| **Notifications.View** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Notifications.ManagePreferences** | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ |
| **Notifications.Configure** | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |

### 7.4 Documents (DR-001 to DR-005)

| Permission | Host Admin | Admin | Principal | VP | HOD | Teacher | Parent | Student |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:-------:|:------:|:-------:|
| **Documents.View** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Documents.Upload** | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Documents.Download** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Documents.Delete** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Documents.Approve** | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |

---

## 8. Learning Module Permissions

### 8.1 Learning Materials (LM-001 to LM-007)

| Permission | Host Admin | Admin | Principal | VP | HOD | Teacher | Parent | Student |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:-------:|:------:|:-------:|
| **Materials.View** | ✅ | ✅ | ✅ | ✅ | ✅ | 📚 | 👶 | 📚 |
| **Materials.Upload** | ❌ | ✅ | ✅ | ✅ | ✅ | 📚 | ❌ | ❌ |
| **Materials.Edit** | ❌ | ✅ | ✅ | ✅ | ✅ | 🔒 | ❌ | ❌ |
| **Materials.Delete** | ❌ | ✅ | ✅ | ✅ | ✅ | 🔒 | ❌ | ❌ |
| **Materials.Download** | ✅ | ✅ | ✅ | ✅ | ✅ | 📚 | 👶 | 📚 |
| **Materials.ManageVersions** | ❌ | ✅ | ✅ | ✅ | ✅ | 🔒 | ❌ | ❌ |

### 8.2 Online Lessons (OL-001 to OL-006)

| Permission | Host Admin | Admin | Principal | VP | HOD | Teacher | Parent | Student |
|------------|:----------:|:-----:|:---------:|:--:|:---:|:-------:|:------:|:-------:|
| **Lessons.View** | ✅ | ✅ | ✅ | ✅ | ✅ | 📚 | 👶 | 📚 |
| **Lessons.Schedule** | ❌ | ✅ | ✅ | ✅ | ✅ | 📚 | ❌ | ❌ |
| **Lessons.Host** | ❌ | ❌ | ✅ | ✅ | ✅ | 📚 | ❌ | ❌ |
| **Lessons.Join** | ❌ | ❌ | ✅ | ✅ | ✅ | 📚 | ❌ | 📚 |
| **Lessons.Cancel** | ❌ | ✅ | ✅ | ✅ | ✅ | 🔒 | ❌ | ❌ |
| **Recordings.View** | ✅ | ✅ | ✅ | ✅ | ✅ | 📚 | 👶 | 📚 |
| **Recordings.Upload** | ❌ | ✅ | ✅ | ✅ | ✅ | 📚 | ❌ | ❌ |
| **Recordings.Delete** | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |

---

## 9. System Administration Permissions

### 9.1 Tenant Management

| Permission | Host Admin | Admin | Principal |
|------------|:----------:|:-----:|:---------:|
| **Tenants.View** | ✅ | ❌ | ❌ |
| **Tenants.Create** | ✅ | ❌ | ❌ |
| **Tenants.Edit** | ✅ | ❌ | ❌ |
| **Tenants.Delete** | ✅ | ❌ | ❌ |
| **Tenants.Impersonate** | ✅ | ❌ | ❌ |

### 9.2 User & Role Management

| Permission | Host Admin | Admin | Principal | VP |
|------------|:----------:|:-----:|:---------:|:--:|
| **Users.View** | ✅ | ✅ | ✅ | ✅ |
| **Users.Create** | ✅ | ✅ | ✅ | ❌ |
| **Users.Edit** | ✅ | ✅ | ✅ | ❌ |
| **Users.Delete** | ✅ | ✅ | ❌ | ❌ |
| **Users.ResetPassword** | ✅ | ✅ | ✅ | ❌ |
| **Users.Unlock** | ✅ | ✅ | ✅ | ❌ |
| **Roles.View** | ✅ | ✅ | ✅ | ❌ |
| **Roles.Manage** | ✅ | ✅ | ❌ | ❌ |
| **Permissions.Assign** | ✅ | ✅ | ❌ | ❌ |

### 9.3 System Configuration

| Permission | Host Admin | Admin | Principal |
|------------|:----------:|:-----:|:---------:|
| **Settings.View** | ✅ | ✅ | ✅ |
| **Settings.Edit** | ✅ | ✅ | ⚙️ |
| **AuditLogs.View** | ✅ | ✅ | ✅ |
| **AuditLogs.Export** | ✅ | ✅ | ❌ |
| **Backups.View** | ✅ | ✅ | ❌ |
| **Backups.Create** | ✅ | ✅ | ❌ |
| **Backups.Restore** | ✅ | ❌ | ❌ |

---

## 10. Permission Implementation

### 10.1 Permission Names Class

```csharp
namespace psms.Authorization
{
    public static class PermissionNames
    {
        // Admissions Module
        public const string Admissions = "Admissions";

        public const string Admissions_Applications = "Admissions.Applications";
        public const string Admissions_Applications_View = "Admissions.Applications.View";
        public const string Admissions_Applications_ViewAll = "Admissions.Applications.ViewAll";
        public const string Admissions_Applications_Create = "Admissions.Applications.Create";
        public const string Admissions_Applications_Edit = "Admissions.Applications.Edit";
        public const string Admissions_Applications_Delete = "Admissions.Applications.Delete";
        public const string Admissions_Applications_Submit = "Admissions.Applications.Submit";
        public const string Admissions_Applications_Withdraw = "Admissions.Applications.Withdraw";

        public const string Admissions_Review = "Admissions.Review";
        public const string Admissions_Review_StartReview = "Admissions.Review.StartReview";
        public const string Admissions_Review_RequestDocuments = "Admissions.Review.RequestDocuments";

        public const string Admissions_Decision = "Admissions.Decision";
        public const string Admissions_Decision_Approve = "Admissions.Decision.Approve";
        public const string Admissions_Decision_Reject = "Admissions.Decision.Reject";
        public const string Admissions_Decision_Waitlist = "Admissions.Decision.Waitlist";
        public const string Admissions_Decision_ExtendOffer = "Admissions.Decision.ExtendOffer";

        public const string Admissions_Documents = "Admissions.Documents";
        public const string Admissions_Documents_Upload = "Admissions.Documents.Upload";
        public const string Admissions_Documents_Verify = "Admissions.Documents.Verify";
        public const string Admissions_Documents_Download = "Admissions.Documents.Download";

        public const string Admissions_Interviews = "Admissions.Interviews";
        public const string Admissions_Interviews_Schedule = "Admissions.Interviews.Schedule";
        public const string Admissions_Interviews_Conduct = "Admissions.Interviews.Conduct";

        public const string Admissions_Assessments = "Admissions.Assessments";
        public const string Admissions_Assessments_Schedule = "Admissions.Assessments.Schedule";
        public const string Admissions_Assessments_Conduct = "Admissions.Assessments.Conduct";
        public const string Admissions_Assessments_RecordResults = "Admissions.Assessments.RecordResults";

        public const string Admissions_Waitlist = "Admissions.Waitlist";
        public const string Admissions_Waitlist_View = "Admissions.Waitlist.View";
        public const string Admissions_Waitlist_OfferPosition = "Admissions.Waitlist.OfferPosition";

        public const string Admissions_Enrollment = "Admissions.Enrollment";
        public const string Admissions_Enrollment_AssignClass = "Admissions.Enrollment.AssignClass";
        public const string Admissions_Enrollment_Complete = "Admissions.Enrollment.Complete";

        public const string Admissions_Settings = "Admissions.Settings";
        public const string Admissions_Settings_Manage = "Admissions.Settings.Manage";

        // Academic Module
        public const string Academic = "Academic";

        public const string Academic_Students = "Academic.Students";
        public const string Academic_Students_View = "Academic.Students.View";
        public const string Academic_Students_ViewAll = "Academic.Students.ViewAll";
        public const string Academic_Students_Create = "Academic.Students.Create";
        public const string Academic_Students_Edit = "Academic.Students.Edit";
        public const string Academic_Students_Delete = "Academic.Students.Delete";

        public const string Academic_Teachers = "Academic.Teachers";
        public const string Academic_Teachers_View = "Academic.Teachers.View";
        public const string Academic_Teachers_Create = "Academic.Teachers.Create";
        public const string Academic_Teachers_Edit = "Academic.Teachers.Edit";
        public const string Academic_Teachers_AssignClass = "Academic.Teachers.AssignClass";

        public const string Academic_Attendance = "Academic.Attendance";
        public const string Academic_Attendance_View = "Academic.Attendance.View";
        public const string Academic_Attendance_Capture = "Academic.Attendance.Capture";
        public const string Academic_Attendance_Edit = "Academic.Attendance.Edit";

        public const string Academic_Timetables = "Academic.Timetables";
        public const string Academic_Timetables_View = "Academic.Timetables.View";
        public const string Academic_Timetables_Manage = "Academic.Timetables.Manage";
        public const string Academic_Timetables_Publish = "Academic.Timetables.Publish";

        // Financial Module
        public const string Financial = "Financial";

        public const string Financial_FeeStructures = "Financial.FeeStructures";
        public const string Financial_FeeStructures_View = "Financial.FeeStructures.View";
        public const string Financial_FeeStructures_Manage = "Financial.FeeStructures.Manage";

        public const string Financial_Payments = "Financial.Payments";
        public const string Financial_Payments_View = "Financial.Payments.View";
        public const string Financial_Payments_Process = "Financial.Payments.Process";
        public const string Financial_Payments_Reconcile = "Financial.Payments.Reconcile";

        public const string Financial_Reports = "Financial.Reports";
        public const string Financial_Reports_View = "Financial.Reports.View";
        public const string Financial_Reports_Export = "Financial.Reports.Export";

        // Assessment Module
        public const string Assessment = "Assessment";

        public const string Assessment_Marks = "Assessment.Marks";
        public const string Assessment_Marks_View = "Assessment.Marks.View";
        public const string Assessment_Marks_Create = "Assessment.Marks.Create";
        public const string Assessment_Marks_Edit = "Assessment.Marks.Edit";
        public const string Assessment_Marks_Publish = "Assessment.Marks.Publish";
        public const string Assessment_Marks_Unlock = "Assessment.Marks.Unlock";
        public const string Assessment_Marks_Import = "Assessment.Marks.Import";

        public const string Assessment_Quizzes = "Assessment.Quizzes";
        public const string Assessment_Quizzes_View = "Assessment.Quizzes.View";
        public const string Assessment_Quizzes_Create = "Assessment.Quizzes.Create";
        public const string Assessment_Quizzes_Publish = "Assessment.Quizzes.Publish";

        // Named ReportCards, not Reports — this block said Assessment.Reports.*
        // while the implementation has always used Assessment.ReportCards.*.
        public const string Assessment_ReportCards = "Assessment.ReportCards";
        public const string Assessment_ReportCards_View = "Assessment.ReportCards.View";
        public const string Assessment_ReportCards_Generate = "Assessment.ReportCards.Generate";
        public const string Assessment_ReportCards_Publish = "Assessment.ReportCards.Publish";
        public const string Assessment_ReportCards_Download = "Assessment.ReportCards.Download";
        public const string Assessment_ReportCards_Comment = "Assessment.ReportCards.Comment";

        // Communication Module
        public const string Communication = "Communication";

        public const string Communication_Announcements = "Communication.Announcements";
        public const string Communication_Announcements_View = "Communication.Announcements.View";
        public const string Communication_Announcements_Create = "Communication.Announcements.Create";
        public const string Communication_Announcements_SendSchoolWide = "Communication.Announcements.SendSchoolWide";

        public const string Communication_Messages = "Communication.Messages";
        public const string Communication_Messages_View = "Communication.Messages.View";
        public const string Communication_Messages_Send = "Communication.Messages.Send";

        // Learning Module
        public const string Learning = "Learning";

        public const string Learning_Materials = "Learning.Materials";
        public const string Learning_Materials_View = "Learning.Materials.View";
        public const string Learning_Materials_Upload = "Learning.Materials.Upload";
        public const string Learning_Materials_Download = "Learning.Materials.Download";

        public const string Learning_Lessons = "Learning.Lessons";
        public const string Learning_Lessons_View = "Learning.Lessons.View";
        public const string Learning_Lessons_Schedule = "Learning.Lessons.Schedule";
        public const string Learning_Lessons_Host = "Learning.Lessons.Host";
        public const string Learning_Lessons_Join = "Learning.Lessons.Join";

        // System Administration
        public const string Administration = "Administration";

        public const string Administration_Users = "Administration.Users";
        public const string Administration_Users_View = "Administration.Users.View";
        public const string Administration_Users_Create = "Administration.Users.Create";
        public const string Administration_Users_Edit = "Administration.Users.Edit";
        public const string Administration_Users_Delete = "Administration.Users.Delete";

        public const string Administration_Roles = "Administration.Roles";
        public const string Administration_Roles_View = "Administration.Roles.View";
        public const string Administration_Roles_Manage = "Administration.Roles.Manage";

        public const string Administration_AuditLogs = "Administration.AuditLogs";
        public const string Administration_AuditLogs_View = "Administration.AuditLogs.View";

        public const string Administration_Settings = "Administration.Settings";
        public const string Administration_Settings_View = "Administration.Settings.View";
        public const string Administration_Settings_Edit = "Administration.Settings.Edit";
    }
}
```

### 10.2 Using Permissions in Services

```csharp
[AbpAuthorize(PermissionNames.Admissions_Decision_Approve)]
public async Task<ApplicationDto> ApproveAsync(Guid id, ApplicationDecisionDto input)
{
    // Only users with Admissions.Decision.Approve permission can execute
    var application = await _applicationRepository.GetAsync(id);

    // Additional business rule check
    if (!await CanMakeDecision(application))
    {
        throw new UserFriendlyException(
            AdmissionsExceptionCodes.UnauthorizedAdmissionDecision,
            "Cannot make decision: prerequisites not met."
        );
    }

    application.Approve(AbpSession.UserId.Value, input.Reason, input.OfferExpiryDays);
    await _applicationRepository.UpdateAsync(application);

    return ObjectMapper.Map<ApplicationDto>(application);
}
```

---

## Summary

### Total Permissions by Module

| Module | Permission Count |
|--------|-----------------|
| Admissions | 35 |
| Academic | 25 |
| Financial | 15 |
| Assessment | 20 |
| Communication | 12 |
| Learning | 12 |
| Administration | 15 |
| **Total** | **134** |

### Key Principles

1. **Least Privilege**: Users get only the permissions needed for their role
2. **Data Ownership**: Users can only access data they own or are assigned to
3. **Multi-Tenancy**: All permissions are tenant-scoped (except Host Admin)
4. **Audit Trail**: All permission-related actions are logged
5. **Hierarchical Override**: Higher roles inherit lower role permissions where appropriate

---

**Document Version**: 1.0
**Last Updated**: 2026-01-29
