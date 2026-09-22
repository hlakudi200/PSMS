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
// Assessment
// ============================================================
export interface IAssessment {
  id: string;
  classSubjectId: string;
  termId: string;
  name: string;
  description?: string;
  assessmentType: number;
  capsCategory?: number;
  maxMarks: number;
  weight: number;
  passPercentage: number;
  scheduledDate?: string;
  dueDate?: string;
  durationMinutes?: number;
  instructions?: string;
  isPublished: boolean;
  marksReleased: boolean;
  createdByTeacherUserId: number;
  className?: string;
  subjectName?: string;
  termName?: string;
  markCount: number;
  questionCount: number;
}

export interface IAssessmentList {
  id: string;
  classSubjectId: string;
  termId: string;
  name: string;
  assessmentType: number;
  capsCategory?: number;
  maxMarks: number;
  weight: number;
  passPercentage: number;
  scheduledDate?: string;
  dueDate?: string;
  isPublished: boolean;
  marksReleased: boolean;
  className?: string;
  subjectName?: string;
  termName?: string;
  markCount: number;
  questionCount: number;
}

export interface ICreateAssessment {
  classSubjectId: string;
  termId: string;
  name: string;
  description?: string;
  assessmentType: number;
  capsCategory?: number;
  maxMarks: number;
  weight: number;
  passPercentage: number;
  scheduledDate?: string;
  dueDate?: string;
  durationMinutes?: number;
  instructions?: string;
}

export interface IUpdateAssessment {
  name?: string;
  description?: string;
  capsCategory?: number;
  maxMarks?: number;
  weight?: number;
  passPercentage?: number;
  scheduledDate?: string;
  dueDate?: string;
  durationMinutes?: number;
  instructions?: string;
}

export interface IGetAssessmentsInput extends IPagedAndSortedResultRequest {
  classSubjectId?: string;
  termId?: string;
  classId?: string;
  subjectId?: string;
  assessmentType?: number;
  isPublished?: boolean;
  name?: string;
}

// A single multiple-choice question supplied inline when creating an
// assessment together with its questions (QA-001). Mirrors the backend
// CreateAssessmentQuestionInlineDto.
export interface ICreateAssessmentQuestionInline {
  questionText: string;
  marks: number;
  options: string[];
  correctOptionIndex: number;
  explanation?: string;
  cognitiveLevel?: number;
}

// Creates an assessment + its questions atomically via
// Assessment/CreateWithQuestions (one backend unit of work).
export interface ICreateAssessmentWithQuestions extends ICreateAssessment {
  questions: ICreateAssessmentQuestionInline[];
}

// ============================================================
// AssessmentQuestion
// ============================================================
export interface IAssessmentQuestion {
  id: string;
  assessmentId: string;
  questionNumber: number;
  questionType: number;
  questionText: string;
  marks: number;
  options?: string;
  correctAnswer?: string;
  explanation?: string;
  cognitiveLevel?: number;
  isActive: boolean;
  creationTime: string;
  creatorUserId?: number;
}

export interface IAssessmentQuestionList {
  id: string;
  assessmentId: string;
  questionNumber: number;
  questionType: number;
  questionText: string;
  marks: number;
  cognitiveLevel?: number;
  isActive: boolean;
}

export interface ICreateAssessmentQuestion {
  assessmentId: string;
  questionNumber: number;
  questionType: number;
  questionText: string;
  marks: number;
  options?: string;
  correctAnswer?: string;
  explanation?: string;
  cognitiveLevel?: number;
}

export interface IUpdateAssessmentQuestion {
  questionNumber?: number;
  questionType?: number;
  questionText?: string;
  marks?: number;
  options?: string;
  correctAnswer?: string;
  explanation?: string;
  cognitiveLevel?: number;
  isActive?: boolean;
}

export interface IReorderQuestions {
  assessmentId: string;
  questionIdsInOrder: string[];
}

// ============================================================
// Mark
// ============================================================
export interface IMark {
  id: string;
  assessmentId: string;
  studentId: string;
  rawMark?: number;
  percentage?: number;
  achievementLevel?: number;
  status: number;
  wasAbsent: boolean;
  isReassessment: boolean;
  teacherComment?: string;
  feedback?: string;
  feedbackHistory?: string;
  markedDate?: string;
  markedByTeacherUserId?: number;
  isModerated: boolean;
  moderationAdjustment?: number;
  assessmentName?: string;
  assessmentMaxMarks: number;
  marksReleased?: boolean;
  feedbackEditableUntil?: string;
  studentName?: string;
  studentAdmissionNumber?: string;
}

// TF-005: one entry in a mark's feedback edit history (parsed from the
// IMark.feedbackHistory JSON string).
export interface IFeedbackEditEntry {
  at: string;
  byUserId?: number;
  previous: string;
}

export interface IUpdateFeedback {
  feedback: string;
}

export interface IMarkList {
  id: string;
  assessmentId: string;
  studentId: string;
  rawMark?: number;
  percentage?: number;
  achievementLevel?: number;
  status: number;
  wasAbsent: boolean;
  isReassessment: boolean;
  teacherComment?: string;
  feedback?: string;
  isModerated: boolean;
  moderationAdjustment?: number;
  assessmentName?: string;
  studentName?: string;
  studentAdmissionNumber?: string;
}

export interface IRecordMark {
  assessmentId: string;
  studentId: string;
  rawMark?: number;
  wasAbsent: boolean;
  isReassessment: boolean;
  teacherComment?: string;
  feedback?: string;
}

export interface IStudentMark {
  studentId: string;
  rawMark?: number;
  wasAbsent: boolean;
  teacherComment?: string;
}

export interface IBulkRecordMarks {
  assessmentId: string;
  studentMarks: IStudentMark[];
}

export interface IGetMarksInput extends IPagedAndSortedResultRequest {
  assessmentId?: string;
  studentId?: string;
  classId?: string;
  subjectId?: string;
  termId?: string;
  status?: number;
  studentName?: string;
}

// ============================================================
// Report
// ============================================================
export interface IReport {
  id: string;
  studentId: string;
  classId: string;
  termId?: string;
  academicYearId: string;
  reportType: number;
  overallPercentage?: number;
  overallAchievementLevel?: number;
  classPosition?: number;
  totalStudentsInClass?: number;
  daysPresent: number;
  daysAbsent: number;
  daysLate: number;
  teacherComment?: string;
  principalComment?: string;
  parentComment?: string;
  parentAcknowledgedDate?: string;
  promotionDecision?: number;
  promotedToGradeId?: string;
  status: number;
  generatedDate?: string;
  publishedDate?: string;
  approvedByUserId?: number;
  approvedDate?: string;
  studentName?: string;
  studentAdmissionNumber?: string;
  className?: string;
  termName?: string;
  academicYearName?: string;
  promotedToGradeName?: string;
  /**
   * RC-04: whether a PDF exists. The link itself is minted per request and
   * short-lived, so call getPdfUrlAsync when the user clicks download rather
   * than holding a URL that would outlive the page.
   */
  hasPdf: boolean;
  subjectReports: IReportSubject[];
  /**
   * RC-09. Set while an approval workflow is running for this report. The direct
   * Approve action is refused server-side while it is set, so the UI links to the
   * approval instead of offering a button that can only fail.
   */
  activeWorkflowInstanceId?: string;
}

export interface IReportList {
  id: string;
  studentId: string;
  classId: string;
  termId?: string;
  academicYearId: string;
  reportType: number;
  overallPercentage?: number;
  overallAchievementLevel?: number;
  classPosition?: number;
  status: number;
  generatedDate?: string;
  publishedDate?: string;
  studentName?: string;
  studentAdmissionNumber?: string;
  className?: string;
  termName?: string;
  academicYearName?: string;
  /** RC-04: see IReport.hasPdf. */
  hasPdf: boolean;
  subjectCount: number;
  /** RC-09: see IReport.activeWorkflowInstanceId. */
  activeWorkflowInstanceId?: string;
}

export interface IGenerateReport {
  studentId: string;
  classId: string;
  academicYearId: string;
  reportType: number;
  termId?: string;
  daysPresent: number;
  daysAbsent: number;
  daysLate: number;
  teacherComment?: string;
}

export interface IGetReportsInput extends IPagedAndSortedResultRequest {
  studentId?: string;
  classId?: string;
  termId?: string;
  academicYearId?: string;
  reportType?: number;
  status?: number;
  studentName?: string;
}

export interface IReportComment {
  comment: string;
}

export interface IBulkGenerateReportPdfsInput {
  classId: string;
  termId?: string;
}

/**
 * RC-01. Generating report cards for a whole class.
 *
 * Attendance comes from the register over the term's date range rather than
 * from a typed-in figure, because one number spread across a class would put
 * the same attendance on every report card. The defaults below are only used
 * when there is no term to read a window from.
 */
export interface IBulkGenerateReports {
  classId: string;
  academicYearId: string;
  reportType: number;
  termId?: string;
  useAttendanceRecords: boolean;
  defaultDaysPresent: number;
  defaultDaysAbsent: number;
  defaultDaysLate: number;
  /** Restrict the run to these learners. Omit for the whole class. */
  studentIds?: string[];
}

/** Mirrors the backend BulkGenerateOutcome. */
export enum BulkGenerateOutcome {
  Eligible = 1,
  Generated = 2,
  SkippedExisting = 3,
  Blocked = 4,
  Failed = 5,
}

export interface IBulkGenerateReportItem {
  studentId: string;
  studentName: string;
  admissionNumber?: string;
  outcome: BulkGenerateOutcome;
  message?: string;
  reportId?: string;
}

export interface IBulkGenerateReportsResult {
  classId: string;
  className?: string;
  termId?: string;
  termName?: string;
  isPreview: boolean;
  totalStudents: number;
  eligibleCount: number;
  generatedCount: number;
  skippedCount: number;
  blockedCount: number;
  failedCount: number;
  items: IBulkGenerateReportItem[];
}

// ============================================================
// ReportSubject
// ============================================================
export interface IReportSubject {
  id: string;
  reportId: string;
  subjectId: string;
  teacherId?: string;
  termMark?: number;
  examMark?: number;
  finalMark?: number;
  achievementLevel?: number;
  teacherComment?: string;
  subjectPosition?: number;
  classAverage?: number;
  highestInClass?: number;
  lowestInClass?: number;
  termWeight: number;
  examWeight: number;
  subjectName?: string;
  subjectCode?: string;
  teacherName?: string;
}

export interface IRecordReportSubjectMarks {
  reportSubjectId: string;
  termMark?: number;
  examMark?: number;
  termWeight: number;
  examWeight: number;
  teacherComment?: string;
}

export interface IBulkRecordReportSubjectMarks {
  reportId: string;
  subjectMarks: IRecordReportSubjectMarks[];
}
