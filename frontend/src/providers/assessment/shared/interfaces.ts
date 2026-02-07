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
  markedDate?: string;
  markedByTeacherUserId?: number;
  isModerated: boolean;
  moderationAdjustment?: number;
  assessmentName?: string;
  assessmentMaxMarks: number;
  studentName?: string;
  studentAdmissionNumber?: string;
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
  subjectReports: IReportSubject[];
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
  subjectCount: number;
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
