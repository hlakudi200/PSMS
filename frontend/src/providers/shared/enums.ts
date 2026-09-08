// Shared frontend enums mirroring backend psms.Domain.Shared.Enums.
// Keep values in sync with the C# definitions to avoid silent breakage.

export enum AttendanceStatus {
  Present = 1,
  Absent = 2,
  Late = 3,
  Excused = 4,
  SickLeave = 5,
  Holiday = 6,
}

export enum ReportStatus {
  Draft = 1,
  Generated = 2,
  PendingApproval = 3,
  Approved = 4,
  Published = 5,
}

export enum ReportType {
  Term1 = 1,
  Term2 = 2,
  Term3 = 3,
  Term4 = 4,
  MidYear = 5,
  YearEnd = 6,
  Progress = 7,
}

export enum AnnouncementPriority {
  Low = 1,
  Normal = 2,
  High = 3,
  Urgent = 4,
}

export enum AssessmentType {
  Test = 1,
  Assignment = 2,
  Exam = 3,
  Practical = 4,
  Oral = 5,
  Project = 6,
  Other = 7,
}

export enum CapsCategory {
  SBA = 1,
  Formal = 2,
  Informal = 3,
  MidYear = 4,
  FinalExam = 5,
}

export enum DisciplinaryCategory {
  Misconduct = 1,
  Bullying = 2,
  Violence = 3,
  SubstanceAbuse = 4,
  PropertyDamage = 5,
  Truancy = 6,
  AcademicDishonesty = 7,
  Harassment = 8,
  DressCode = 9,
  Other = 10,
}

export enum DisciplinarySeverity {
  Minor = 1,
  Moderate = 2,
  Serious = 3,
  VerySerious = 4,
}

export enum DisciplinaryCaseStatus {
  Draft = 1,
  Reported = 2,
  UnderInvestigation = 3,
  HearingScheduled = 4,
  HearingCompleted = 5,
  Resolved = 6,
  Appealed = 7,
  Cancelled = 8,
}

export enum TransferType {
  TransferIn = 1,
  TransferOut = 2,
}

export enum StudentTransferStatus {
  Draft = 1,
  Submitted = 2,
  UnderReview = 3,
  Approved = 4,
  Rejected = 5,
  Completed = 6,
  Cancelled = 7,
}

// Human-readable label maps (kept alongside enums so frontend tables and
// selects do not have to re-declare them).
export const attendanceStatusLabels: Record<number, string> = {
  [AttendanceStatus.Present]: 'Present',
  [AttendanceStatus.Absent]: 'Absent',
  [AttendanceStatus.Late]: 'Late',
  [AttendanceStatus.Excused]: 'Excused',
  [AttendanceStatus.SickLeave]: 'Sick Leave',
  [AttendanceStatus.Holiday]: 'Holiday',
};

export const reportStatusLabels: Record<number, string> = {
  [ReportStatus.Draft]: 'Draft',
  [ReportStatus.Generated]: 'Generated',
  [ReportStatus.PendingApproval]: 'Pending Approval',
  [ReportStatus.Approved]: 'Approved',
  [ReportStatus.Published]: 'Published',
};

export const reportTypeLabels: Record<number, string> = {
  [ReportType.Term1]: 'Term 1',
  [ReportType.Term2]: 'Term 2',
  [ReportType.Term3]: 'Term 3',
  [ReportType.Term4]: 'Term 4',
  [ReportType.MidYear]: 'Mid-Year',
  [ReportType.YearEnd]: 'Year-End',
  [ReportType.Progress]: 'Progress',
};

export const assessmentTypeLabels: Record<number, string> = {
  [AssessmentType.Test]: 'Test',
  [AssessmentType.Assignment]: 'Assignment',
  [AssessmentType.Exam]: 'Exam',
  [AssessmentType.Practical]: 'Practical',
  [AssessmentType.Oral]: 'Oral',
  [AssessmentType.Project]: 'Project',
  [AssessmentType.Other]: 'Other',
};

export const disciplinaryCategoryLabels: Record<number, string> = {
  [DisciplinaryCategory.Misconduct]: 'Misconduct',
  [DisciplinaryCategory.Bullying]: 'Bullying',
  [DisciplinaryCategory.Violence]: 'Violence',
  [DisciplinaryCategory.SubstanceAbuse]: 'Substance Abuse',
  [DisciplinaryCategory.PropertyDamage]: 'Property Damage',
  [DisciplinaryCategory.Truancy]: 'Truancy',
  [DisciplinaryCategory.AcademicDishonesty]: 'Academic Dishonesty',
  [DisciplinaryCategory.Harassment]: 'Harassment',
  [DisciplinaryCategory.DressCode]: 'Dress Code',
  [DisciplinaryCategory.Other]: 'Other',
};

export const disciplinarySeverityLabels: Record<number, string> = {
  [DisciplinarySeverity.Minor]: 'Minor',
  [DisciplinarySeverity.Moderate]: 'Moderate',
  [DisciplinarySeverity.Serious]: 'Serious',
  [DisciplinarySeverity.VerySerious]: 'Very Serious',
};

export const disciplinaryCaseStatusLabels: Record<number, string> = {
  [DisciplinaryCaseStatus.Draft]: 'Draft',
  [DisciplinaryCaseStatus.Reported]: 'Reported',
  [DisciplinaryCaseStatus.UnderInvestigation]: 'Under Investigation',
  [DisciplinaryCaseStatus.HearingScheduled]: 'Hearing Scheduled',
  [DisciplinaryCaseStatus.HearingCompleted]: 'Hearing Completed',
  [DisciplinaryCaseStatus.Resolved]: 'Resolved',
  [DisciplinaryCaseStatus.Appealed]: 'Appealed',
  [DisciplinaryCaseStatus.Cancelled]: 'Cancelled',
};

export const transferTypeLabels: Record<number, string> = {
  [TransferType.TransferIn]: 'Transfer In',
  [TransferType.TransferOut]: 'Transfer Out',
};

export const studentTransferStatusLabels: Record<number, string> = {
  [StudentTransferStatus.Draft]: 'Draft',
  [StudentTransferStatus.Submitted]: 'Submitted',
  [StudentTransferStatus.UnderReview]: 'Under Review',
  [StudentTransferStatus.Approved]: 'Approved',
  [StudentTransferStatus.Rejected]: 'Rejected',
  [StudentTransferStatus.Completed]: 'Completed',
  [StudentTransferStatus.Cancelled]: 'Cancelled',
};

// ---- Finance / HR / Admissions ----

export enum FeeType {
  Tuition = 1,
  Registration = 2,
  Application = 3,
  Transport = 4,
  AfterCare = 5,
  Extramural = 6,
  Uniform = 7,
  Stationery = 8,
  Other = 9,
}

export enum PaymentStatus {
  Pending = 1,
  Completed = 2,
  Failed = 3,
  Cancelled = 4,
  Refunded = 5,
  PartiallyPaid = 6,
  Overdue = 7,
}

export enum PaymentMethod {
  EFT = 1,
  DebitOrder = 2,
  CreditCard = 3,
  DebitCard = 4,
  Cash = 5,
  Cheque = 6,
  PayFast = 7,
  SnapScan = 8,
  Zapper = 9,
  Ozow = 10,
  BankDeposit = 11,
}

export enum ExpenseCategory {
  Stationery = 1,
  Textbooks = 2,
  Equipment = 3,
  Maintenance = 4,
  Technology = 5,
  Sports = 6,
  Cultural = 7,
  Transport = 8,
  Catering = 9,
  Training = 10,
  Other = 11,
}

export enum ExpensePriority {
  Low = 1,
  Medium = 2,
  High = 3,
  Urgent = 4,
}

export enum ExpenseRequestStatus {
  Draft = 1,
  Submitted = 2,
  UnderReview = 3,
  Approved = 4,
  Rejected = 5,
  Paid = 6,
  Cancelled = 7,
}

export enum FeeWaiverType {
  FinancialHardship = 1,
  SiblingDiscount = 2,
  StaffDiscount = 3,
  Bursary = 4,
  Scholarship = 5,
  Other = 6,
}

export enum FeeWaiverStatus {
  Draft = 1,
  Submitted = 2,
  UnderReview = 3,
  Approved = 4,
  Rejected = 5,
  Cancelled = 6,
}

export enum LeaveType {
  Annual = 1,
  Sick = 2,
  Family = 3,
  Maternity = 4,
  Paternity = 5,
  Study = 6,
  Compassionate = 7,
  Unpaid = 8,
  Other = 9,
}

export enum LeaveStatus {
  Draft = 1,
  Submitted = 2,
  HODApproved = 3,
  Approved = 4,
  Rejected = 5,
  Cancelled = 6,
}

export const feeTypeLabels: Record<number, string> = {
  [FeeType.Tuition]: 'Tuition',
  [FeeType.Registration]: 'Registration',
  [FeeType.Application]: 'Application',
  [FeeType.Transport]: 'Transport',
  [FeeType.AfterCare]: 'After Care',
  [FeeType.Extramural]: 'Extramural',
  [FeeType.Uniform]: 'Uniform',
  [FeeType.Stationery]: 'Stationery',
  [FeeType.Other]: 'Other',
};

export const paymentStatusLabels: Record<number, string> = {
  [PaymentStatus.Pending]: 'Pending',
  [PaymentStatus.Completed]: 'Completed',
  [PaymentStatus.Failed]: 'Failed',
  [PaymentStatus.Cancelled]: 'Cancelled',
  [PaymentStatus.Refunded]: 'Refunded',
  [PaymentStatus.PartiallyPaid]: 'Partially Paid',
  [PaymentStatus.Overdue]: 'Overdue',
};

export const paymentMethodLabels: Record<number, string> = {
  [PaymentMethod.EFT]: 'EFT',
  [PaymentMethod.DebitOrder]: 'Debit Order',
  [PaymentMethod.CreditCard]: 'Credit Card',
  [PaymentMethod.DebitCard]: 'Debit Card',
  [PaymentMethod.Cash]: 'Cash',
  [PaymentMethod.Cheque]: 'Cheque',
  [PaymentMethod.PayFast]: 'PayFast',
  [PaymentMethod.SnapScan]: 'SnapScan',
  [PaymentMethod.Zapper]: 'Zapper',
  [PaymentMethod.Ozow]: 'Ozow',
  [PaymentMethod.BankDeposit]: 'Bank Deposit',
};

export const expenseCategoryLabels: Record<number, string> = {
  [ExpenseCategory.Stationery]: 'Stationery',
  [ExpenseCategory.Textbooks]: 'Textbooks',
  [ExpenseCategory.Equipment]: 'Equipment',
  [ExpenseCategory.Maintenance]: 'Maintenance',
  [ExpenseCategory.Technology]: 'Technology',
  [ExpenseCategory.Sports]: 'Sports',
  [ExpenseCategory.Cultural]: 'Cultural',
  [ExpenseCategory.Transport]: 'Transport',
  [ExpenseCategory.Catering]: 'Catering',
  [ExpenseCategory.Training]: 'Training',
  [ExpenseCategory.Other]: 'Other',
};

export const expensePriorityLabels: Record<number, string> = {
  [ExpensePriority.Low]: 'Low',
  [ExpensePriority.Medium]: 'Medium',
  [ExpensePriority.High]: 'High',
  [ExpensePriority.Urgent]: 'Urgent',
};

export const expenseRequestStatusLabels: Record<number, string> = {
  [ExpenseRequestStatus.Draft]: 'Draft',
  [ExpenseRequestStatus.Submitted]: 'Submitted',
  [ExpenseRequestStatus.UnderReview]: 'Under Review',
  [ExpenseRequestStatus.Approved]: 'Approved',
  [ExpenseRequestStatus.Rejected]: 'Rejected',
  [ExpenseRequestStatus.Paid]: 'Paid',
  [ExpenseRequestStatus.Cancelled]: 'Cancelled',
};

export const feeWaiverTypeLabels: Record<number, string> = {
  [FeeWaiverType.FinancialHardship]: 'Financial Hardship',
  [FeeWaiverType.SiblingDiscount]: 'Sibling Discount',
  [FeeWaiverType.StaffDiscount]: 'Staff Discount',
  [FeeWaiverType.Bursary]: 'Bursary',
  [FeeWaiverType.Scholarship]: 'Scholarship',
  [FeeWaiverType.Other]: 'Other',
};

export const feeWaiverStatusLabels: Record<number, string> = {
  [FeeWaiverStatus.Draft]: 'Draft',
  [FeeWaiverStatus.Submitted]: 'Submitted',
  [FeeWaiverStatus.UnderReview]: 'Under Review',
  [FeeWaiverStatus.Approved]: 'Approved',
  [FeeWaiverStatus.Rejected]: 'Rejected',
  [FeeWaiverStatus.Cancelled]: 'Cancelled',
};

export const leaveTypeLabels: Record<number, string> = {
  [LeaveType.Annual]: 'Annual',
  [LeaveType.Sick]: 'Sick',
  [LeaveType.Family]: 'Family',
  [LeaveType.Maternity]: 'Maternity',
  [LeaveType.Paternity]: 'Paternity',
  [LeaveType.Study]: 'Study',
  [LeaveType.Compassionate]: 'Compassionate',
  [LeaveType.Unpaid]: 'Unpaid',
  [LeaveType.Other]: 'Other',
};

export const leaveStatusLabels: Record<number, string> = {
  [LeaveStatus.Draft]: 'Draft',
  [LeaveStatus.Submitted]: 'Submitted',
  [LeaveStatus.HODApproved]: 'HOD Approved',
  [LeaveStatus.Approved]: 'Approved',
  [LeaveStatus.Rejected]: 'Rejected',
  [LeaveStatus.Cancelled]: 'Cancelled',
};

// ---- Communications, SA-Specific, Activities ----

export enum AnnouncementType {
  General = 1,
  Academic = 2,
  Sports = 3,
  Event = 4,
  Emergency = 5,
  Holiday = 6,
  Administrative = 7,
}

export enum AnnouncementAudience {
  All = 1,
  Staff = 2,
  Teachers = 3,
  Parents = 4,
  Students = 5,
  Grade = 6,
  Class = 7,
}

export enum TransportType {
  Bus = 1,
  Minibus = 2,
  Sedan = 3,
  Other = 4,
}

export enum TransportDirection {
  Morning = 1,
  Afternoon = 2,
  Both = 3,
}

// Mirrors backend psms.Domain.Shared.Enums.EnrollmentStatus (shared by transport,
// extramural and after-care enrolments).
export enum TransportEnrollmentStatus {
  Active = 1,
  Suspended = 2,
  Terminated = 3,
  Pending = 4,
}

export enum ExtramuralCategory {
  Sport = 1,
  Cultural = 2,
  Academic = 3,
  Social = 4,
  Other = 5,
}

export enum ExtramuralActivityType {
  Individual = 1,
  Team = 2,
}

export enum FieldTripStatus {
  Draft = 1,
  Submitted = 2,
  UnderReview = 3,
  Approved = 4,
  Rejected = 5,
  Completed = 6,
  Cancelled = 7,
}

export const announcementTypeLabels: Record<number, string> = {
  [AnnouncementType.General]: 'General',
  [AnnouncementType.Academic]: 'Academic',
  [AnnouncementType.Sports]: 'Sports',
  [AnnouncementType.Event]: 'Event',
  [AnnouncementType.Emergency]: 'Emergency',
  [AnnouncementType.Holiday]: 'Holiday',
  [AnnouncementType.Administrative]: 'Administrative',
};

export const announcementAudienceLabels: Record<number, string> = {
  [AnnouncementAudience.All]: 'All',
  [AnnouncementAudience.Staff]: 'Staff',
  [AnnouncementAudience.Teachers]: 'Teachers',
  [AnnouncementAudience.Parents]: 'Parents',
  [AnnouncementAudience.Students]: 'Students',
  [AnnouncementAudience.Grade]: 'Grade',
  [AnnouncementAudience.Class]: 'Class',
};

export const transportTypeLabels: Record<number, string> = {
  [TransportType.Bus]: 'Bus',
  [TransportType.Minibus]: 'Minibus',
  [TransportType.Sedan]: 'Sedan',
  [TransportType.Other]: 'Other',
};

export const transportDirectionLabels: Record<number, string> = {
  [TransportDirection.Morning]: 'Morning',
  [TransportDirection.Afternoon]: 'Afternoon',
  [TransportDirection.Both]: 'Both',
};

export const transportEnrollmentStatusLabels: Record<number, string> = {
  [TransportEnrollmentStatus.Active]: 'Active',
  [TransportEnrollmentStatus.Suspended]: 'Suspended',
  [TransportEnrollmentStatus.Terminated]: 'Terminated',
  [TransportEnrollmentStatus.Pending]: 'Pending',
};

export const extramuralCategoryLabels: Record<number, string> = {
  [ExtramuralCategory.Sport]: 'Sport',
  [ExtramuralCategory.Cultural]: 'Cultural',
  [ExtramuralCategory.Academic]: 'Academic',
  [ExtramuralCategory.Social]: 'Social',
  [ExtramuralCategory.Other]: 'Other',
};

export const extramuralActivityTypeLabels: Record<number, string> = {
  [ExtramuralActivityType.Individual]: 'Individual',
  [ExtramuralActivityType.Team]: 'Team',
};

export const fieldTripStatusLabels: Record<number, string> = {
  [FieldTripStatus.Draft]: 'Draft',
  [FieldTripStatus.Submitted]: 'Submitted',
  [FieldTripStatus.UnderReview]: 'Under Review',
  [FieldTripStatus.Approved]: 'Approved',
  [FieldTripStatus.Rejected]: 'Rejected',
  [FieldTripStatus.Completed]: 'Completed',
  [FieldTripStatus.Cancelled]: 'Cancelled',
};
