// Pagination Interfaces (reuse from academic)
export interface IPagedAndSortedResultRequest {
  maxResultCount?: number;
  skipCount?: number;
  sorting?: string;
}

export interface IPagedResult<T> {
  totalCount: number;
  items: T[];
}

export interface IListResult<T> {
  items: T[];
}

// ============================================================
// AdmissionSettings
// ============================================================
export interface IAdmissionSettings {
  id: string;
  academicYearId: string;
  academicYearName: string;
  gradeId?: string;
  gradeName?: string;
  isDefaultSettings: boolean;
  applicationOpenDate?: string;
  applicationCloseDate?: string;
  isAcceptingApplications: boolean;
  isApplicationPeriodOpen: boolean;
  maxCapacity?: number;
  currentEnrolledCount: number;
  availableSpots: number;
  isCapacityFull: boolean;
  applicationFeeAmount: number;
  applicationFeeDisplay: string;
  isInterviewRequired: boolean;
  isAssessmentRequired: boolean;
  offerExpiryDays: number;
  minimumAge?: number;
  maximumAge?: number;
  requiredDocuments?: string;
  notes?: string;
}

export interface ICreateAdmissionSettings {
  academicYearId: string;
  gradeId?: string;
  applicationFeeAmount: number;
  maxCapacity?: number;
  applicationOpenDate?: string;
  applicationCloseDate?: string;
  isAcceptingApplications: boolean;
  isInterviewRequired: boolean;
  isAssessmentRequired: boolean;
  offerExpiryDays: number;
  minimumAge?: number;
  maximumAge?: number;
  requiredDocuments?: string;
  notes?: string;
}

export interface IUpdateAdmissionSettings {
  applicationFeeAmount?: number;
  maxCapacity?: number;
  applicationOpenDate?: string;
  applicationCloseDate?: string;
  isAcceptingApplications?: boolean;
  isInterviewRequired?: boolean;
  isAssessmentRequired?: boolean;
  offerExpiryDays?: number;
  minimumAge?: number;
  maximumAge?: number;
  requiredDocuments?: string;
  notes?: string;
}

export interface ICapacityStatus {
  gradeId: string;
  gradeName: string;
  academicYearId: string;
  academicYearName: string;
  capacity: number;
  currentEnrollment: number;
  approvedPendingEnrollment: number;
  underConsideration: number;
  waitlistCount: number;
  availableSpots: number;
  utilizationPercentage: number;
  projectedUtilization: number;
  isCapacityFull: boolean;
  shouldAutoWaitlist: boolean;
}

// ============================================================
// Application
// ============================================================
export interface IApplication {
  id: string;
  applicationNumber: string;
  academicYearId: string;
  academicYearName: string;
  applyingForGradeId: string;
  applyingForGradeName: string;
  firstName: string;
  middleName?: string;
  lastName: string;
  fullName: string;
  dateOfBirth: string;
  age: number;
  gender: string;
  genderDisplayName: string;
  idNumber?: string;
  passportNumber?: string;
  isSACitizen: boolean;
  previousSchool?: string;
  creatorEmailAddress: string;
  status: string;
  statusDisplayName: string;
  applicationDate: string;
  submittedDate?: string;
  decision?: string;
  decisionDisplayName?: string;
  decisionReason?: string;
  decisionDate?: string;
  reviewedDate?: string;
  reviewedByUserId?: number;
  reviewedByUserName?: string;
  offerExpiryDate?: string;
  isOfferExpired: boolean;
  createdStudentId?: string;
  parentCount: number;
  documentCount: number;
  hasInterview: boolean;
  hasAssessment: boolean;
  waitlistPosition?: number;
  isFeePaid: boolean;
  canEdit: boolean;
  canSubmit: boolean;
  canWithdraw: boolean;
  canMakeDecision: boolean;
}

export interface IApplicationList {
  id: string;
  applicationNumber: string;
  firstName: string;
  lastName: string;
  fullName: string;
  dateOfBirth: string;
  gradeName: string;
  academicYearName: string;
  status: string;
  statusDisplayName: string;
  submittedDate?: string;
  creationTime: string;
  isFeePaid: boolean;
  parentCount: number;
  documentCount: number;
  hasInterview: boolean;
  hasAssessment: boolean;
  waitlistPosition?: number;
  offerExpiryDate?: string;
  isOfferExpired: boolean;
}

export interface ICreateApplication {
  academicYearId: string;
  applyingForGradeId: string;
  firstName: string;
  middleName?: string;
  lastName: string;
  dateOfBirth: string;
  gender: string;
  idNumber?: string;
  passportNumber?: string;
  isSACitizen: boolean;
  previousSchool?: string;
  creatorEmailAddress: string;
}

export interface IUpdateApplication {
  applyingForGradeId?: string;
  firstName?: string;
  middleName?: string;
  lastName?: string;
  dateOfBirth?: string;
  gender?: string;
  idNumber?: string;
  passportNumber?: string;
  isSACitizen?: boolean;
  previousSchool?: string;
}

export interface IGetApplicationsInput extends IPagedAndSortedResultRequest {
  applicationNumber?: string;
  applicantName?: string;
  status?: string;
  academicYearId?: string;
  gradeId?: string;
  submittedDateFrom?: string;
  submittedDateTo?: string;
  isFeePaid?: boolean;
  isOnWaitlist?: boolean;
  hasExpiredOffer?: boolean;
}

export interface IApplicationStatistics {
  academicYearId: string;
  academicYearName: string;
  totalApplications: number;
  draftApplications: number;
  submittedApplications: number;
  underReviewApplications: number;
  underConsiderationApplications: number;
  approvedApplications: number;
  rejectedApplications: number;
  waitlistedApplications: number;
  enrolledApplications: number;
  withdrawnApplications: number;
  expiredApplications: number;
  pendingPayments: number;
  completedPayments: number;
  totalFeesCollected: number;
  submissionRate: number;
  approvalRate: number;
  enrollmentRate: number;
  byGrade: IGradeStatistics[];
  dailyTrend: IDailyApplicationCount[];
}

export interface IGradeStatistics {
  gradeId: string;
  gradeName: string;
  capacity: number;
  currentEnrollment: number;
  pendingApplications: number;
  approvedNotEnrolled: number;
  waitlistCount: number;
  availableSpots: number;
  capacityUtilization: number;
}

export interface IDailyApplicationCount {
  date: string;
  submitted: number;
  approved: number;
  rejected: number;
  enrolled: number;
}

// ============================================================
// ApplicantParent
// ============================================================
export interface IApplicantParent {
  id: string;
  applicationId: string;
  relationship: string;
  relationshipDisplayName: string;
  firstName: string;
  lastName: string;
  fullName: string;
  idNumber?: string;
  email: string;
  phoneNumber: string;
  alternatePhone?: string;
  streetAddress?: string;
  suburb?: string;
  city?: string;
  province?: string;
  postalCode?: string;
  occupation?: string;
  employer?: string;
  isPrimaryContact: boolean;
  isFinanciallyResponsible: boolean;
}

export interface ICreateApplicantParent {
  applicationId: string;
  relationship: string;
  firstName: string;
  lastName: string;
  idNumber?: string;
  email: string;
  phoneNumber: string;
  alternatePhone?: string;
  streetAddress?: string;
  suburb?: string;
  city?: string;
  province?: string;
  postalCode?: string;
  occupation?: string;
  employer?: string;
  isPrimaryContact: boolean;
  isFinanciallyResponsible: boolean;
}

export interface IUpdateApplicantParent {
  relationship?: string;
  firstName?: string;
  lastName?: string;
  idNumber?: string;
  email?: string;
  phoneNumber?: string;
  alternatePhone?: string;
  streetAddress?: string;
  suburb?: string;
  city?: string;
  province?: string;
  postalCode?: string;
  occupation?: string;
  employer?: string;
  isPrimaryContact?: boolean;
  isFinanciallyResponsible?: boolean;
}

// ============================================================
// ApplicationDocument
// ============================================================
export interface IApplicationDocument {
  id: string;
  applicationId: string;
  applicationNumber: string;
  category: string;
  categoryDisplayName: string;
  documentName: string;
  fileName: string;
  fileUrl: string;
  fileSizeBytes: number;
  fileSizeDisplay: string;
  contentType: string;
  uploadedDate: string;
  isRequired: boolean;
  isVerified: boolean;
  verifiedByUserId?: number;
  verifiedByUserName?: string;
  verifiedDate?: string;
  canDelete: boolean;
}

export interface IUploadDocument {
  applicationId: string;
  category: string;
  file: File;
  description?: string;
}

export interface IVerifyDocument {
  notes?: string;
}

export interface IRequiredDocumentsStatus {
  applicationId: string;
  applicationNumber: string;
  gradeName: string;
  isSACitizen: boolean;
  totalRequired: number;
  totalUploaded: number;
  totalVerified: number;
  allRequiredUploaded: boolean;
  allRequiredVerified: boolean;
  documents: IRequiredDocumentItem[];
}

export interface IRequiredDocumentItem {
  category: string;
  categoryDisplayName: string;
  description: string;
  isRequired: boolean;
  isUploaded: boolean;
  isVerified: boolean;
  documentId?: string;
  fileName?: string;
  uploadedDate?: string;
}

// ============================================================
// ApplicationFee
// ============================================================
export interface IApplicationFee {
  id: string;
  applicationId: string;
  applicationNumber: string;
  amount: number;
  currency: string;
  amountDisplay: string;
  status: string;
  statusDisplayName: string;
  isPaid: boolean;
  paymentDate?: string;
  paymentMethod?: string;
  paymentMethodDisplayName?: string;
  paymentReference?: string;
  receiptNumber?: string;
  isRefundable: boolean;
}

export interface IRecordPayment {
  paymentMethod: string;
  paymentReference: string;
  receiptNumber?: string;
}

export interface IPaymentResult {
  success: boolean;
  message: string;
  applicationId: string;
  feeId?: string;
  paymentReference: string;
  status: string;
  amount: number;
  currency: string;
  receiptNumber?: string;
  errorCode?: string;
  errorDetails?: string;
}

export interface IPaymentCallback {
  merchantReference: string;
  gatewayReference?: string;
  status: string;
  amount: number;
  paymentMethod?: string;
  signature?: string;
  rawPayload?: string;
  gateway?: string;
  transactionDate?: string;
  errorMessage?: string;
}

// ============================================================
// AdmissionInterview
// ============================================================
export interface IAdmissionInterview {
  id: string;
  applicationId: string;
  applicationNumber: string;
  applicantName: string;
  gradeName: string;
  scheduledDate: string;
  scheduledTime: string;
  location?: string;
  meetingLink?: string;
  interviewerUserId: number;
  interviewerName: string;
  status: string;
  statusDisplayName: string;
  completedDate?: string;
  rating?: number;
  recommended?: boolean;
  notes?: string;
  isUpcoming: boolean;
  isPastDue: boolean;
  canComplete: boolean;
  canReschedule: boolean;
  canCancel: boolean;
}

export interface IScheduleInterview {
  applicationId: string;
  scheduledDate: string;
  scheduledTime: string;
  interviewerUserId: number;
  interviewerName: string;
  location?: string;
  meetingLink?: string;
  notes?: string;
}

export interface IRescheduleInterview {
  newScheduledDate: string;
  newScheduledTime: string;
  location?: string;
  meetingLink?: string;
}

export interface ICompleteInterview {
  rating: number;
  recommended: boolean;
  notes?: string;
}

export interface ITimeSlot {
  date: string;
  startTime: string;
  endTime: string;
  durationMinutes: number;
  isAvailable: boolean;
  displayTime: string;
}

// ============================================================
// AdmissionAssessment
// ============================================================
export interface IAdmissionAssessment {
  id: string;
  applicationId: string;
  applicationNumber: string;
  applicantName: string;
  type: string;
  typeDisplayName: string;
  scheduledDate: string;
  assessedGradeId: string;
  gradeName: string;
  subjects?: string;
  assessorUserId: number;
  assessorName: string;
  completedDate?: string;
  totalScore: number;
  maxScore: number;
  percentage: number;
  percentageDisplay: string;
  passed: boolean;
  feedback?: string;
  isCompleted: boolean;
  isUpcoming: boolean;
}

export interface IScheduleAssessment {
  applicationId: string;
  type: string;
  scheduledDate: string;
  assessedGradeId: string;
  subjects?: string;
  assessorUserId: number;
  maxScore: number;
}

export interface IRecordAssessmentResults {
  totalScore: number;
  maxScore: number;
  passPercentage: number;
  feedback?: string;
}

// ============================================================
// Waitlist
// ============================================================
export interface IWaitlist {
  id: string;
  applicationId: string;
  applicationNumber: string;
  applicantName: string;
  gradeId: string;
  gradeName: string;
  position: number;
  addedDate: string;
  status: string;
  statusDisplayName: string;
  notifiedDate?: string;
  offerExpiryDate?: string;
  isOfferExpired: boolean;
  daysUntilOfferExpiry?: number;
  notes?: string;
  canAcceptOffer: boolean;
  canDeclineOffer: boolean;
  canWithdraw: boolean;
}

export interface IWaitlistPosition {
  applicationId: string;
  applicationNumber: string;
  gradeId: string;
  gradeName: string;
  position: number;
  addedDate: string;
  totalInWaitlist: number;
}

// ============================================================
// Enrollment
// ============================================================
export interface IEnrollment {
  id: string;
  applicationId: string;
  applicationNumber: string;
  applicantName: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  gender: string;
  gradeId: string;
  gradeName: string;
  assignedClassId?: string;
  assignedClassName?: string;
  isOfferAccepted: boolean;
  isFormsSubmitted: boolean;
  isClassAssigned: boolean;
  isEnrollmentComplete: boolean;
  offerAcceptedDate?: string;
  formsSubmittedDate?: string;
  enrollmentCompletedDate?: string;
  createdStudentId?: string;
  admissionNumber?: string;
}

export interface IAcceptOffer {
  applicationId: string;
  confirmAcceptance: boolean;
  parentSignature?: string;
}

export interface IAssignClass {
  applicationId: string;
  classId: string;
}

export interface ICompleteEnrollment {
  applicationId: string;
  classId: string;
  admissionNumber?: string;
  formsSubmitted: boolean;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  medicalConditions?: string;
  popiaConsentGiven: boolean;
}

export interface IClassAvailability {
  classId: string;
  className: string;
  gradeId: string;
  gradeName: string;
  maxCapacity: number;
  currentEnrolled: number;
  availableSpots: number;
  isAvailable: boolean;
  teacherName: string;
}
