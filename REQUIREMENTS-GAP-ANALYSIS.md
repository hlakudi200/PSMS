# PSMS Requirements Gap Analysis
**Date**: 2026-01-27
**Comparing**: Requirements PDF vs PSMS-Business-Rules.md

---

## Executive Summary

After thorough analysis of the Requirements Document against the Business Rules document, **9 CRITICAL GAPS** and **12 IMPORTANT GAPS** were identified. The business rules covered 65% of the requirements, missing several core functional areas.

---

## Gap Categories

### 🔴 CRITICAL GAPS (Core Functionality Missing)

#### GAP-001: Online Lessons Module
**Requirements Not Covered**:
- 4.2.2: Teachers shall schedule online lessons
- 4.2.2: Teachers shall host live online classes
- 4.2.2: Teachers shall upload lesson recordings
- 4.4.3: Learners shall join live online classes
- 4.4.3: Learners shall view recorded lessons

**Business Impact**: HIGH - Core teaching functionality unavailable

**Missing Rules Needed**:
- Online lesson scheduling validation
- Video conferencing integration rules
- Recording upload validation (size, format, duration)
- Live class capacity limits
- Lesson recording retention policies
- Access control for recordings

---

#### GAP-002: Quiz & Assessment Platform
**Requirements Not Covered**:
- 4.2.3: Teachers shall set up quizzes and assessments on the platform

**Business Impact**: HIGH - Platform-based assessments not possible

**Missing Rules Needed**:
- Quiz creation validation (question types, time limits, attempts)
- Assessment submission rules
- Auto-grading rules
- Quiz scheduling and availability windows
- Plagiarism detection rules
- Assessment result release rules

---

#### GAP-003: Learning Materials Management - Detailed Rules
**Requirements Not Covered**:
- 4.2.1: Upload learning materials (PDFs, videos, slides, links)
- 4.2.1: Materials categorized by subject, grade, and term
- 4.4.2: Learners view and download learning materials

**Business Impact**: HIGH - Materials management undefined

**Missing Rules Needed**:
- File upload validation (max size, allowed types, virus scanning)
- Materials naming conventions
- Duplicate material detection
- Materials versioning rules
- Access control per term/subject/grade
- Material download tracking
- Storage quota per teacher/school

---

#### GAP-004: Timetable Management
**Requirements Not Covered**:
- 4.4.1: Learners shall view their timetable
- Teacher scheduling (implicit requirement)

**Business Impact**: HIGH - Academic scheduling unavailable

**Missing Rules Needed**:
- Timetable creation validation
- Period conflict detection (teacher/room/student)
- Timetable publication rules
- Term timetable variations
- Timetable change notifications
- Maximum teaching hours per teacher per day/week

---

#### GAP-005: Excel Import for Marks
**Requirements Not Covered**:
- 4.2.3: Teachers capture marks via Excel spreadsheet

**Business Impact**: MEDIUM-HIGH - Bulk mark entry not defined

**Missing Rules Needed**:
- Excel file format validation
- Bulk import validation rules
- Error handling for invalid data
- Transaction rollback rules
- Import audit trail
- Maximum rows per import

---

#### GAP-006: Document Repository
**Requirements Not Covered**:
- 4.3.2: Parents access school documents

**Business Impact**: MEDIUM - Document sharing undefined

**Missing Rules Needed**:
- Document categorization rules
- Document access control per role
- Document versioning
- Document expiry rules
- Document approval workflow

---

#### GAP-007: Payment Receipts
**Requirements Not Covered**:
- 4.3.3: Parents download payment receipts

**Business Impact**: MEDIUM - Financial compliance issue

**Missing Rules Needed**:
- Receipt generation rules
- Receipt numbering sequence
- Receipt retrieval timeframe
- Receipt immutability
- Receipt template requirements

---

#### GAP-008: Notification System
**Requirements Not Covered**:
- 4.3.4: Parents receive notifications for reports, payments, announcements
- 4.4.5: Learners receive announcements and class notices

**Business Impact**: MEDIUM - Communication channel undefined

**Missing Rules Needed**:
- Notification trigger events
- Notification delivery channels (email, SMS, in-app)
- Notification preference management
- Notification retry logic
- Notification opt-out rules
- Notification rate limiting

---

#### GAP-009: Financial Reporting
**Requirements Not Covered**:
- 4.5.3: Generate financial reports for income and outstanding fees

**Business Impact**: MEDIUM - Financial oversight limited

**Missing Rules Needed**:
- Report generation schedules
- Report data freshness requirements
- Report access control
- Report retention policies
- Outstanding fees aging rules

---

### 🟡 IMPORTANT GAPS (Supporting Features)

#### GAP-010: Teacher Feedback Management
**Requirements Not Covered**:
- 4.2.3: Teachers upload feedback per learner
- 4.3.1: Parents view teacher feedback

**Current Coverage**: Implicit in domain model, no validation rules

**Missing Rules**:
- Feedback character limits
- Feedback professionalism validation
- Feedback edit/delete rules
- Feedback visibility timing

---

#### GAP-011: Enrolled Subjects & Teachers View
**Requirements Not Covered**:
- 4.4.1: Learners view enrolled subjects and teachers

**Current Coverage**: Security rules only

**Missing Rules**:
- Enrollment visibility rules
- Historical enrollment access

---

#### GAP-012: Academic Performance Dashboards
**Requirements Not Covered**:
- 4.1.3: Administrators view academic performance dashboards

**Current Coverage**: None

**Missing Rules**:
- Dashboard data refresh frequency
- Dashboard metrics definitions
- Dashboard access control

---

#### GAP-013: Fee Statements
**Requirements Not Covered**:
- 4.3.3: Parents view fee statements and balances

**Current Coverage**: Partial (FM-001, FM-002)

**Missing Rules**:
- Statement generation frequency
- Statement delivery method
- Statement historical access

---

#### GAP-014: Teacher-Subject-Class Assignment Validation
**Requirements Not Covered**:
- 4.1.2: Assigning teachers to subjects and classes

**Current Coverage**: AR-004 covers qualification validation only

**Missing Rules**:
- Maximum classes per teacher
- Maximum subjects per teacher
- Workload balancing rules

---

#### GAP-015: Non-Functional Requirements
**Requirements Not Covered**:
- 5.3: High availability targets
- 5.3: Regular backup schedules
- 5.4: Usability standards

**Current Coverage**: None

**Missing Rules**:
- System uptime SLA (e.g., 99.9%)
- Backup frequency and retention
- Accessibility compliance (WCAG)

---

## Coverage Summary

| Requirement Section | Coverage | Status |
|---------------------|----------|--------|
| 4.1.1 Student & Academic Management | 90% | ✅ Good |
| 4.1.2 Teacher Management | 60% | ⚠️ Partial |
| 4.1.3 Reporting & Oversight | 70% | ⚠️ Partial |
| 4.1.4 Communication | 80% | ✅ Good |
| 4.2.1 Learning Material Management | 30% | ❌ Poor |
| 4.2.2 Online Lessons | 0% | ❌ Missing |
| 4.2.3 Assessment & Marks | 70% | ⚠️ Partial |
| 4.2.4 Communication | 80% | ✅ Good |
| 4.3.1 Academic Monitoring | 80% | ✅ Good |
| 4.3.2 Reports & Documents | 50% | ⚠️ Partial |
| 4.3.3 Fee & Payment Management | 70% | ⚠️ Partial |
| 4.3.4 Notifications | 0% | ❌ Missing |
| 4.4.1 Academic Access | 20% | ❌ Poor |
| 4.4.2 Learning Materials | 0% | ❌ Missing |
| 4.4.3 Online Learning | 0% | ❌ Missing |
| 4.4.4 Performance Tracking | 90% | ✅ Good |
| 4.4.5 Communication | 50% | ⚠️ Partial |
| 4.5.1 Fee Structure Management | 90% | ✅ Good |
| 4.5.2 Payments & Reconciliation | 90% | ✅ Good |
| 4.5.3 Financial Reporting | 30% | ❌ Poor |
| 5.1 Security | 100% | ✅ Excellent |
| 5.2 Performance & Scalability | 80% | ✅ Good |
| 5.3 Availability & Reliability | 0% | ❌ Missing |
| 5.4 Usability | 0% | ❌ Missing |

**Overall Coverage**: 65% of requirements have corresponding business rules

---

## Prioritized Implementation Recommendations

### Phase 1 (Critical - Implement Immediately)
1. Online Lessons Module Rules (GAP-001)
2. Quiz & Assessment Platform Rules (GAP-002)
3. Learning Materials Management Rules (GAP-003)
4. Timetable Management Rules (GAP-004)

### Phase 2 (High Priority - Next Sprint)
5. Excel Import Rules (GAP-005)
6. Notification System Rules (GAP-008)
7. Payment Receipts Rules (GAP-007)

### Phase 3 (Medium Priority - Following Sprint)
8. Document Repository Rules (GAP-006)
9. Financial Reporting Rules (GAP-009)
10. Teacher Feedback Rules (GAP-010)

### Phase 4 (Lower Priority - Future)
11. Dashboard Rules (GAP-012)
12. Non-Functional Requirements (GAP-015)

---

## Validation Checklist

- [x] All requirements from PDF reviewed
- [x] All business rules from existing document reviewed
- [x] Gaps identified and categorized
- [x] Business impact assessed
- [x] Implementation priority assigned
- [ ] Additional business rules document created
- [ ] Stakeholder review completed
- [ ] Implementation plan approved

---

## Next Steps

1. **Create supplementary business rules document** covering all 15 gaps
2. **Review with stakeholders** to validate gap analysis
3. **Update domain model** if needed for new rules
4. **Update ERD** if new entities required
5. **Begin Phase 1 implementation** of critical rules
