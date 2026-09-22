import { createContext } from "react";

export interface IChildReportSummary {
  id: string;
  termName?: string;
  publishedDate?: string;
  overallPercentage?: number;
}

export interface IChildSummary {
  studentId: string;
  studentName: string;
  admissionNumber?: string;
  isPrimaryContact: boolean;
  className?: string;
  gradeName?: string;
  /** Attendance for the current term — undefined when the summary couldn't be loaded. */
  attendancePercentage?: number;
  attendanceDaysPresent?: number;
  attendanceTotalDays?: number;
  /** Most recently published report card — undefined when the child has none yet. */
  latestReport?: IChildReportSummary;
}

export interface IChildrenStateContext {
  isPending: boolean;
  isError: boolean;
  myChildren?: IChildSummary[];
  /** The child every other parent screen scopes its reads to. */
  selectedChildId?: string;
  /** Unread counts are per signed-in user, not per child — the backend resolves them from the session. */
  unreadAnnouncements?: number;
  unreadNotifications?: number;
}

export interface IChildrenActionContext {
  /** Loads the parent's linked children plus each child's at-a-glance summary. */
  getMyChildrenAsync: () => Promise<void>;
  selectChild: (studentId: string) => void;
}

export const INITIAL_STATE: IChildrenStateContext = {
  isPending: false,
  isError: false,
};

export const ChildrenStateContext = createContext<IChildrenStateContext>(INITIAL_STATE);
export const ChildrenActionContext = createContext<IChildrenActionContext | undefined>(undefined);
