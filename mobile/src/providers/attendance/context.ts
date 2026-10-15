import { createContext } from "react";

/** Mirrors the backend AttendanceStatus. */
export type AttendanceStatus = 1 | 2 | 3 | 4 | 5 | 6; // Present | Absent | Late | Excused | SickLeave | Holiday

export interface IAttendanceRecord {
  id: string;
  attendanceDate: string;
  status: AttendanceStatus;
  /** The teacher's note on the day. */
  notes?: string;
  className?: string;
}

/** Counts are school days: the backend keeps at most one register entry per learner per day. */
export interface IAttendanceSummary {
  totalDays: number;
  presentCount: number;
  absentCount: number;
  lateCount: number;
  excusedCount: number;
  sickLeaveCount: number;
  /** Present or late, over every recorded day. 0 when nothing is recorded — check totalDays. */
  attendancePercentage: number;
}

export interface IAttendanceRange {
  startDate: string;
  endDate: string;
}

export interface IAttendanceStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  summary?: IAttendanceSummary;
  /** Newest first, as the backend returns them. */
  records?: IAttendanceRecord[];
  /** Whose attendance, and for which range, the data above is — the screen checks it matches. */
  studentId?: string;
  range?: IAttendanceRange;
}

export interface IAttendanceActionContext {
  /** Loads the summary and the day-by-day register for one child over a date range. */
  getAttendanceAsync: (studentId: string, range: IAttendanceRange) => Promise<void>;
}

export const INITIAL_STATE: IAttendanceStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const AttendanceStateContext = createContext<IAttendanceStateContext>(INITIAL_STATE);
export const AttendanceActionContext = createContext<IAttendanceActionContext | undefined>(undefined);
