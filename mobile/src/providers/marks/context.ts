import { createContext } from "react";

export type CapsAchievementLevel = 1 | 2 | 3 | 4 | 5 | 6 | 7;
export type MarkStatus = 1 | 2 | 3 | 4 | 5; // Pending | Completed | Absent | Exempted | Incomplete

export interface IMark {
  id: string;
  assessmentId: string;
  rawMark?: number;
  percentage?: number;
  achievementLevel?: CapsAchievementLevel;
  status: MarkStatus;
  wasAbsent: boolean;
  isReassessment: boolean;
  feedback?: string;
  assessmentName: string;
}

export interface IAssessmentDetail {
  id: string;
  name: string;
  description?: string;
  assessmentType: number;
  maxMarks: number;
  weight: number;
  passPercentage: number;
  scheduledDate?: string;
  dueDate?: string;
  instructions?: string;
  isPublished: boolean;
  marksReleased: boolean;
  className: string;
  subjectName: string;
  termName: string;
}

export interface ITerm {
  id: string;
  academicYearId: string;
  academicYearName: string;
  termNumber: number;
  termName: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
}

export interface IMarksStateContext {
  isTermPending: boolean;
  isTermError: boolean;
  currentTerm?: ITerm;
  terms?: ITerm[];

  isMarksPending: boolean;
  isMarksError: boolean;
  marks?: IMark[];
  marksTermId?: string;

  /** Every published assessment for the student's class in the selected term — used to show
   *  assessments the teacher hasn't captured a mark for yet as "pending", not absent from the list. */
  classAssessments?: IAssessmentDetail[];

  assessmentDetailsById: Record<string, IAssessmentDetail>;
}

export interface IMarksActionContext {
  /** Loads the current term plus every term in its academic year (for the term switcher). */
  getCurrentTermAsync: () => Promise<void>;
  /** Loads the student's marks for a term, and hydrates subject/class info for each assessment. */
  getMarksByStudentAsync: (studentId: string, termId: string) => Promise<void>;
  /** Loads every published assessment for the class in this term (for the "pending, not missing" rule). */
  getClassAssessmentsAsync: (classId: string, termId: string) => Promise<void>;
  /** Returns cached assessment detail if present, else fetches and caches it. */
  getAssessmentDetailAsync: (assessmentId: string) => Promise<IAssessmentDetail | undefined>;
  /** Ensures a mark for this assessment is in state (for deep-linked detail screens). */
  ensureMarkForAssessmentAsync: (assessmentId: string) => Promise<IMark | undefined>;
}

export const INITIAL_STATE: IMarksStateContext = {
  isTermPending: false,
  isTermError: false,
  isMarksPending: false,
  isMarksError: false,
  assessmentDetailsById: {},
};

export const MarksStateContext = createContext<IMarksStateContext>(INITIAL_STATE);
export const MarksActionContext = createContext<IMarksActionContext | undefined>(undefined);
