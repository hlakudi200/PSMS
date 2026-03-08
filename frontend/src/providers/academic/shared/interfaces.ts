// Pagination Interfaces
export interface IPagedAndSortedResultRequest {
  maxResultCount?: number;
  skipCount?: number;
  sorting?: string;
  keyword?: string;
  [key: string]: unknown;
}

export interface IPagedResult<T> {
  totalCount: number;
  items: T[];
}

export interface IListResult<T> {
  items: T[];
}

// Shared DTOs
export interface IAddressDto {
  streetAddress?: string;
  suburb?: string;
  city?: string;
  province?: string;
  postalCode?: string;
  country?: string;
}

// ============================================================
// Grade
// ============================================================
export interface IGrade {
  id: string;
  gradeLevel: number;
  gradeName: string;
  schoolPhase: number;
  schoolPhaseDisplayName?: string;
  description?: string;
  isActive: boolean;
  classCount: number;
  studentCount: number;
  subjectCount: number;
}

export interface ICreateGrade {
  gradeLevel: number;
  gradeName: string;
  schoolPhase: number;
  description?: string;
}

export interface IGradeList {
  id: string;
  gradeLevel: number;
  gradeName: string;
  schoolPhase: number;
  isActive: boolean;
  classCount: number;
  studentCount: number;
}

export interface IUpdateGrade {
  gradeName?: string;
  schoolPhase?: number;
  description?: string;
  isActive?: boolean;
}

// ============================================================
// AcademicYear
// ============================================================
export interface IAcademicYear {
  id: string;
  year: number;
  yearName: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
  termCount: number;
  classCount: number;
  terms: IAcademicYearTerm[];
}

export interface IAcademicYearTerm {
  id: string;
  termNumber: number;
  termName: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
}

export interface ICreateAcademicYear {
  year: number;
  startDate: string;
  endDate: string;
  createDefaultTerms: boolean;
}

export interface IUpdateAcademicYear {
  yearName?: string;
  startDate?: string;
  endDate?: string;
}

// ============================================================
// Term
// ============================================================
export interface ITerm {
  id: string;
  academicYearId: string;
  academicYearName: string;
  termNumber: number;
  termName: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
  totalDays: number;
}

export interface ICreateTerm {
  academicYearId: string;
  termNumber: number;
  termName: string;
  startDate: string;
  endDate: string;
}

export interface IUpdateTerm {
  termName?: string;
  startDate?: string;
  endDate?: string;
}

// ============================================================
// Subject
// ============================================================
export interface ISubject {
  id: string;
  subjectName: string;
  subjectCode: string;
  description?: string;
  isCore: boolean;
  isActive: boolean;
  gradeCount: number;
  teacherCount: number;
}

export interface ICreateSubject {
  subjectName: string;
  subjectCode: string;
  description?: string;
  isCore: boolean;
}

export interface IUpdateSubject {
  subjectName?: string;
  subjectCode?: string;
  description?: string;
  isCore?: boolean;
  isActive?: boolean;
}

// ============================================================
// Class
// ============================================================
export interface IClass {
  id: string;
  className: string;
  gradeId: string;
  gradeName: string;
  academicYearId: string;
  academicYearName: string;
  maxCapacity: number;
  classTeacherId?: string;
  classTeacherName?: string;
  isActive: boolean;
  studentCount: number;
  teacherAssignmentCount: number;
  availableCapacity: number;
}

export interface ICreateClass {
  className: string;
  gradeId: string;
  academicYearId: string;
  maxCapacity: number;
  classTeacherId?: string;
}

export interface IUpdateClass {
  className?: string;
  maxCapacity?: number;
  classTeacherId?: string;
  isActive?: boolean;
}

// ============================================================
// Teacher
// ============================================================
export interface ITeacher {
  id: string;
  userId: number;
  firstName: string;
  lastName: string;
  middleName?: string;
  fullName: string;
  employeeNumber: string;
  email: string;
  phone: string;
  dateOfJoining?: string;
  address?: IAddressDto;
  qualifications?: string;
  qualifiedSubjects?: string;
  employmentStatus?: string;
  profilePhotoUrl?: string;
  isActive: boolean;
  subjectAssignmentCount: number;
  classAssignmentCount: number;
}

export interface ICreateTeacher {
  userId: number;
  firstName: string;
  lastName: string;
  middleName?: string;
  employeeNumber: string;
  email: string;
  phone: string;
  dateOfJoining?: string;
  address?: IAddressDto;
  qualifications?: string;
  qualifiedSubjects?: string;
  employmentStatus?: string;
}

export interface IUpdateTeacher {
  firstName?: string;
  lastName?: string;
  middleName?: string;
  email?: string;
  phone?: string;
  dateOfJoining?: string;
  address?: IAddressDto;
  qualifications?: string;
  qualifiedSubjects?: string;
  employmentStatus?: string;
  profilePhotoUrl?: string;
  isActive?: boolean;
}

// ============================================================
// Student
// ============================================================
export interface IStudent {
  id: string;
  firstName: string;
  lastName: string;
  middleName?: string;
  fullName?: string;
  dateOfBirth: string;
  age: number;
  gender: number;
  idNumber?: string;
  passportNumber?: string;
  isSACitizen: boolean;
  admissionNumber?: string;
  admissionDate: string;
  currentGradeId: string;
  currentGradeName?: string;
  currentClassId: string;
  currentClassName?: string;
  phone?: string;
  email?: string;
  physicalAddress?: IAddressDto;
  postalAddress?: IAddressDto;
  profilePhotoUrl?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  medicalConditions?: string;
  popiaConsentGiven: boolean;
  popiaConsentDate?: string;
  isActive: boolean;
  parentCount: number;
  subjectCount: number;
}

export interface IStudentList {
  id: string;
  fullName?: string;
  admissionNumber?: string;
  dateOfBirth: string;
  age: number;
  gender: number;
  currentGradeId: string;
  currentGradeName?: string;
  currentClassId: string;
  currentClassName?: string;
  isActive: boolean;
  parentCount: number;
}

export interface ICreateStudent {
  firstName: string;
  lastName: string;
  middleName?: string;
  dateOfBirth: string;
  gender: number;
  idNumber?: string;
  passportNumber?: string;
  isSACitizen: boolean;
  admissionNumber: string;
  admissionDate: string;
  currentGradeId: string;
  currentClassId: string;
  phone?: string;
  email?: string;
  physicalAddress?: IAddressDto;
  postalAddress?: IAddressDto;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  medicalConditions?: string;
  popiaConsentGiven: boolean;
}

export interface IUpdateStudent {
  firstName?: string;
  lastName?: string;
  middleName?: string;
  dateOfBirth?: string;
  gender?: number;
  idNumber?: string;
  passportNumber?: string;
  isSACitizen?: boolean;
  phone?: string;
  email?: string;
  physicalAddress?: IAddressDto;
  postalAddress?: IAddressDto;
  profilePhotoUrl?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  medicalConditions?: string;
  popiaConsentGiven?: boolean;
  isActive?: boolean;
}

// ============================================================
// Parent
// ============================================================
export interface IParent {
  id: string;
  userId: number;
  firstName?: string;
  lastName?: string;
  fullName?: string;
  email?: string;
  phone?: string;
  idNumber?: string;
  address?: IAddressDto;
  occupation?: string;
  employer?: string;
  workPhone?: string;
  profilePhotoUrl?: string;
  studentCount: number;
}

export interface IParentList {
  id: string;
  firstName?: string;
  lastName?: string;
  fullName?: string;
  email?: string;
  phone?: string;
  studentCount: number;
}

export interface ICreateParent {
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  idNumber?: string;
  address?: IAddressDto;
  occupation?: string;
  employer?: string;
  workPhone?: string;
}

export interface IUpdateParent {
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  idNumber?: string;
  address?: IAddressDto;
  occupation?: string;
  employer?: string;
  workPhone?: string;
}

// ============================================================
// Attendance
// ============================================================
export interface IAttendance {
  id: string;
  creationTime: string;
  creatorUserId?: number;
  studentId: string;
  studentName?: string;
  classId: string;
  className?: string;
  teacherId: string;
  teacherName?: string;
  subjectId?: string;
  subjectName?: string;
  attendanceDate: string;
  status: number;
  notes?: string;
}

export interface IAttendanceList {
  id: string;
  studentId: string;
  studentName?: string;
  classId: string;
  className?: string;
  attendanceDate: string;
  status: number;
  notes?: string;
}

export interface IAttendanceSummary {
  studentId: string;
  studentName?: string;
  totalDays: number;
  presentCount: number;
  absentCount: number;
  lateCount: number;
  excusedCount: number;
  sickLeaveCount: number;
  attendancePercentage: number;
}

export interface ICaptureAttendance {
  studentId: string;
  classId: string;
  teacherId: string;
  attendanceDate: string;
  status: number;
  notes?: string;
  subjectId?: string;
}

export interface IStudentAttendanceEntry {
  studentId: string;
  status: number;
  notes?: string;
}

export interface IBulkCaptureAttendance {
  classId: string;
  teacherId: string;
  attendanceDate: string;
  subjectId?: string;
  entries: IStudentAttendanceEntry[];
}

export interface IGetAttendanceInput extends IPagedAndSortedResultRequest {
  classId?: string;
  studentId?: string;
  teacherId?: string;
  startDate?: string;
  endDate?: string;
  status?: number;
}

export interface IUpdateAttendance {
  status?: number;
  notes?: string;
}

// ============================================================
// GradeSubject
// ============================================================
export interface IGradeSubject {
  id: string;
  gradeId: string;
  subjectId: string;
  isRequired: boolean;
  gradeName?: string;
  gradeLevel: number;
  subjectName?: string;
  subjectCode?: string;
  isCore: boolean;
}

export interface IAssignSubjectToGrade {
  gradeId: string;
  subjectId: string;
  isRequired: boolean;
}

// ============================================================
// ClassSubject
// ============================================================
export interface IClassSubject {
  id: string;
  classId: string;
  subjectId: string;
  teacherId?: string;
  periodsPerWeek: number;
  isActive: boolean;
  className?: string;
  gradeId: string;
  gradeName?: string;
  subjectName?: string;
  subjectCode?: string;
  teacherName?: string;
}

export interface IClassSubjectList {
  id: string;
  classId: string;
  subjectId: string;
  teacherId?: string;
  periodsPerWeek: number;
  isActive: boolean;
  className?: string;
  subjectName?: string;
  subjectCode?: string;
  teacherName?: string;
}

export interface ICreateClassSubject {
  classId: string;
  subjectId: string;
  teacherId?: string;
  periodsPerWeek: number;
}

export interface IUpdateClassSubject {
  teacherId?: string;
  periodsPerWeek?: number;
  isActive?: boolean;
}

export interface IGetClassSubjectsInput extends IPagedAndSortedResultRequest {
  classId?: string;
  subjectId?: string;
  teacherId?: string;
  isActive?: boolean;
}

// ============================================================
// TeacherSubject
// ============================================================
export interface ITeacherSubject {
  id: string;
  teacherId: string;
  teacherName?: string;
  subjectId: string;
  subjectName?: string;
  subjectCode?: string;
  gradeId: string;
  gradeName?: string;
  isPrimary: boolean;
}

export interface IAssignTeacherSubject {
  teacherId: string;
  subjectId: string;
  gradeId: string;
  isPrimary: boolean;
}

// ============================================================
// TeacherClass
// ============================================================
export interface ITeacherClass {
  id: string;
  teacherId: string;
  teacherName?: string;
  classId: string;
  className?: string;
  subjectId: string;
  subjectName?: string;
  subjectCode?: string;
  isClassTeacher: boolean;
}

export interface IAssignTeacherClass {
  teacherId: string;
  classId: string;
  subjectId: string;
  isClassTeacher: boolean;
}

// ============================================================
// StudentParent
// ============================================================
export interface IStudentParent {
  id: string;
  studentId: string;
  studentName?: string;
  studentAdmissionNumber?: string;
  parentId: string;
  parentName?: string;
  parentEmail?: string;
  parentPhone?: string;
  relationshipType: number;
  isPrimaryContact: boolean;
  isFinanciallyResponsible: boolean;
  canPickupStudent: boolean;
  livesWithStudent: boolean;
}

export interface ILinkStudentParent {
  studentId: string;
  parentId: string;
  relationshipType: number;
  isPrimaryContact: boolean;
  isFinanciallyResponsible: boolean;
  canPickupStudent: boolean;
  livesWithStudent: boolean;
}

// ============================================================
// StudentSubject
// ============================================================
export interface IStudentSubject {
  id: string;
  studentId: string;
  studentName?: string;
  studentAdmissionNumber?: string;
  subjectId: string;
  subjectName?: string;
  subjectCode?: string;
  academicYearId: string;
  academicYearName?: string;
  enrollmentDate: string;
  isActive: boolean;
}

export interface IEnrollStudentSubject {
  studentId: string;
  subjectId: string;
  academicYearId: string;
}

// ============================================================
// StudentClass
// ============================================================
export interface IStudentClass {
  id: string;
  studentId: string;
  studentName?: string;
  classId: string;
  className?: string;
  academicYearId: string;
  academicYearName?: string;
  enrollmentDate: string;
  endDate?: string;
  isCurrent: boolean;
  isActive: boolean;
}

export interface IStudentClassList {
  id: string;
  studentId: string;
  studentName?: string;
  classId: string;
  className?: string;
  academicYearName?: string;
  enrollmentDate: string;
  endDate?: string;
  isCurrent: boolean;
  isActive: boolean;
}

export interface ICreateStudentClass {
  studentId: string;
  classId: string;
  academicYearId: string;
  enrollmentDate: string;
}

export interface IUpdateStudentClass {
  isActive?: boolean;
}

// ============================================================
// EmergencyContact
// ============================================================
export interface IEmergencyContact {
  id: string;
  studentId: string;
  studentName?: string;
  firstName?: string;
  lastName?: string;
  fullName?: string;
  relationship: number;
  primaryPhone?: string;
  secondaryPhone?: string;
  workPhone?: string;
  email?: string;
  address?: string;
  priority: number;
  canPickUp: boolean;
  canMakeMedicalDecisions: boolean;
  isActive: boolean;
}

export interface IEmergencyContactList {
  id: string;
  studentId: string;
  firstName?: string;
  lastName?: string;
  fullName?: string;
  relationship: number;
  primaryPhone?: string;
  priority: number;
  canPickUp: boolean;
  isActive: boolean;
}

export interface ICreateEmergencyContact {
  studentId: string;
  firstName: string;
  lastName: string;
  relationship: number;
  primaryPhone: string;
  secondaryPhone?: string;
  workPhone?: string;
  email?: string;
  address?: string;
  priority: number;
  canPickUp: boolean;
  canMakeMedicalDecisions: boolean;
}

export interface IUpdateEmergencyContact {
  firstName?: string;
  lastName?: string;
  relationship?: number;
  primaryPhone?: string;
  secondaryPhone?: string;
  workPhone?: string;
  email?: string;
  address?: string;
  priority?: number;
  canPickUp?: boolean;
  canMakeMedicalDecisions?: boolean;
  isActive?: boolean;
}

// ============================================================
// MedicalInfo
// ============================================================
export interface IMedicalInfo {
  id: string;
  studentId: string;
  studentName?: string;
  bloodType?: string;
  allergies?: string;
  chronicConditions?: string;
  currentMedications?: string;
  dietaryRequirements?: string;
  specialNeeds?: string;
  doctorName?: string;
  doctorPhone?: string;
  medicalAidProvider?: string;
  medicalAidNumber?: string;
  medicalAidPlan?: string;
  medicalAidMainMember?: string;
  immunizationStatus?: string;
  lastPhysicalExamDate?: string;
  canReceiveOTCMedication: boolean;
  additionalNotes?: string;
}

export interface ICreateUpdateMedicalInfo {
  studentId: string;
  bloodType?: string;
  allergies?: string;
  chronicConditions?: string;
  currentMedications?: string;
  dietaryRequirements?: string;
  specialNeeds?: string;
  doctorName?: string;
  doctorPhone?: string;
  medicalAidProvider?: string;
  medicalAidNumber?: string;
  medicalAidPlan?: string;
  medicalAidMainMember?: string;
  immunizationStatus?: string;
  lastPhysicalExamDate?: string;
  canReceiveOTCMedication: boolean;
  additionalNotes?: string;
}

// ============================================================
// POPIAConsent
// ============================================================
export interface IPOPIAConsent {
  id: string;
  studentId: string;
  studentName?: string;
  consentDate: string;
  allowPhotography: boolean;
  allowDataSharing: boolean;
  allowNameInPublications: boolean;
  allowMarketingUse: boolean;
  parentSignatureUserId: number;
  ipAddress?: string;
}

export interface ICreatePOPIAConsent {
  studentId: string;
  consentDate: string;
  allowPhotography: boolean;
  allowDataSharing: boolean;
  allowNameInPublications: boolean;
  allowMarketingUse: boolean;
  parentSignatureUserId: number;
}

export interface IUpdatePOPIAConsent {
  allowPhotography?: boolean;
  allowDataSharing?: boolean;
  allowNameInPublications?: boolean;
  allowMarketingUse?: boolean;
}

export interface IGetPOPIAConsentsInput extends IPagedAndSortedResultRequest {
  studentId?: string;
  allowPhotography?: boolean;
  allowDataSharing?: boolean;
  allowNameInPublications?: boolean;
  allowMarketingUse?: boolean;
}

// ============================================================
// TermEvent
// ============================================================
export interface ITermEvent {
  id: string;
  creationTime: string;
  creatorUserId?: number;
  termId: string;
  termName?: string;
  academicYearName?: string;
  eventName?: string;
  eventType: number;
  eventDate: string;
  description?: string;
}

export interface ITermEventList {
  id: string;
  termId: string;
  termName?: string;
  eventName?: string;
  eventType: number;
  eventDate: string;
}

export interface ICreateTermEvent {
  termId: string;
  eventName: string;
  eventType: number;
  eventDate: string;
  description?: string;
}

export interface IUpdateTermEvent {
  eventName?: string;
  eventType?: number;
  eventDate?: string;
  description?: string;
}

// ============================================================
// Timetable
// ============================================================
export interface ITimetable {
  id: string;
  classId: string;
  className?: string;
  effectiveDate: string;
  endDate?: string;
  isActive: boolean;
  slotCount: number;
}

export interface ITimetableList {
  id: string;
  classId: string;
  className?: string;
  effectiveDate: string;
  endDate?: string;
  isActive: boolean;
  slotCount: number;
}

export interface ICreateTimetable {
  classId: string;
  effectiveDate: string;
  endDate?: string;
}

export interface IUpdateTimetable {
  effectiveDate?: string;
  endDate?: string;
}

// ============================================================
// TimetableSlot
// ============================================================
export interface ITimetableSlot {
  id: string;
  timetableId: string;
  dayOfWeek: number;
  periodNumber: number;
  startTime: string;
  endTime: string;
  subjectId: string;
  subjectName?: string;
  teacherId: string;
  teacherName?: string;
  roomNumber?: string;
}

export interface ITimetableSlotList {
  id: string;
  dayOfWeek: number;
  periodNumber: number;
  startTime: string;
  endTime: string;
  subjectName?: string;
  teacherName?: string;
  roomNumber?: string;
}

export interface ICreateTimetableSlot {
  timetableId: string;
  dayOfWeek: number;
  periodNumber: number;
  startTime: string;
  endTime: string;
  subjectId: string;
  teacherId: string;
  roomNumber?: string;
}

export interface IUpdateTimetableSlot {
  startTime?: string;
  endTime?: string;
  subjectId?: string;
  teacherId?: string;
  roomNumber?: string;
}
