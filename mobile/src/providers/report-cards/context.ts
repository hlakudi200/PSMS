import { createContext } from "react";
import type { CapsAchievementLevel } from "../marks/context";

/** Mirrors the backend ReportType. */
export type ReportType = 1 | 2 | 3 | 4 | 5 | 6 | 7;
/** Mirrors the backend ConductDiligenceRating. */
export type ConductRating = 1 | 2 | 3 | 4 | 5 | 6 | 7;
/** Mirrors the backend PromotionDecision. */
export type PromotionDecision = 1 | 2 | 3 | 4;

export interface IReportCardSummary {
  id: string;
  studentId: string;
  reportType: ReportType;
  termName?: string;
  academicYearName?: string;
  overallPercentage?: number;
  overallAchievementLevel?: CapsAchievementLevel;
  classPosition?: number;
  publishedDate?: string;
  /** False for the Foundation Phase (Grades R–3): levels only, no percentages or positions (§17(4)(a)). */
  reportsPercentages: boolean;
  hasPdf: boolean;
}

export interface IReportCardSubject {
  id: string;
  subjectName: string;
  teacherName?: string;
  finalMark?: number;
  achievementLevel?: CapsAchievementLevel;
  teacherComment?: string;
  /** Grade 12: the final mark waits on the external examination. */
  awaitsExternalExamination: boolean;
}

export interface IReportCard extends IReportCardSummary {
  reportCardNumber?: string;
  className?: string;
  gradeName?: string;
  totalStudentsInClass?: number;
  daysPresent: number;
  daysAbsent: number;
  daysLate: number;
  daysInTerm?: number;
  conductRating?: ConductRating;
  diligenceRating?: ConductRating;
  behaviourComments?: string;
  teacherComment?: string;
  principalComment?: string;
  promotionDecision?: PromotionDecision;
  promotedToGradeName?: string;
  parentComment?: string;
  /** Set once a parent has acknowledged receiving the card. */
  parentAcknowledgedDate?: string;
  subjects: IReportCardSubject[];
}

export interface IReportCardsStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  /** Published cards only — the backend never lists anything else to a family. Newest first. */
  reports?: IReportCardSummary[];
  /** Whose cards `reports` holds; a parent switches between children. */
  reportsStudentId?: string;

  reportsById: Record<string, IReportCard>;
  isDetailPending: boolean;
  isDetailError: boolean;

  isAcknowledgePending: boolean;
  acknowledgeErrorMessage?: string;

  isPdfPending: boolean;
  pdfErrorMessage?: string;
}

export interface IReportCardsActionContext {
  getReportsAsync: (studentId: string) => Promise<void>;
  getReportAsync: (id: string) => Promise<void>;
  /** Records that the parent received the card, with an optional comment. */
  acknowledgeAsync: (id: string, comment?: string) => Promise<boolean>;
  /** A short-lived link to the card's PDF; never stored. */
  getPdfUrlAsync: (id: string) => Promise<string | undefined>;
}

export const INITIAL_STATE: IReportCardsStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
  reportsById: {},
  isDetailPending: false,
  isDetailError: false,
  isAcknowledgePending: false,
  isPdfPending: false,
};

export const ReportCardsStateContext = createContext<IReportCardsStateContext>(INITIAL_STATE);
export const ReportCardsActionContext = createContext<IReportCardsActionContext | undefined>(undefined);
