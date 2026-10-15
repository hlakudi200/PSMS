import type { BadgeTone } from "../components/Badge";
import type { AttendanceStatus, IAttendanceRecord } from "../providers/attendance/context";

/** Backend AttendanceStatus. */
export const ATTENDANCE_STATUS_LABELS: Record<AttendanceStatus, string> = {
  1: "Present", 2: "Absent", 3: "Late", 4: "Excused", 5: "Sick leave", 6: "Holiday",
};

export const ATTENDANCE_STATUS_TONES: Record<AttendanceStatus, BadgeTone> = {
  1: "success", 2: "danger", 3: "warning", 4: "muted", 5: "muted", 6: "neutral",
};

/** A school day the learner was not at school, explained or not. A holiday is not a school day. */
export const isAbsence = (status: AttendanceStatus) => status === 2 || status === 4 || status === 5;

export interface IAbsenceStreak {
  /** Oldest day of the streak. */
  startDate: string;
  /** Newest day of the streak. */
  endDate: string;
  days: number;
  /** How many of those days had no reason given (plain "Absent"). */
  unexplainedDays: number;
  recordIds: string[];
}

/**
 * Runs of two or more consecutive school days away. "Consecutive" means adjacent registers:
 * a learner has at most one record a day, weekends have none, and a holiday is skipped
 * rather than ending the run — it is neither a day at school nor a day missed.
 */
export function findAbsenceStreaks(records: IAttendanceRecord[], minimumDays = 2): IAbsenceStreak[] {
  const oldestFirst = [...records].sort((a, b) => a.attendanceDate.localeCompare(b.attendanceDate));
  const streaks: IAbsenceStreak[] = [];
  let run: IAttendanceRecord[] = [];

  const close = () => {
    if (run.length >= minimumDays) {
      streaks.push({
        startDate: run[0].attendanceDate,
        endDate: run[run.length - 1].attendanceDate,
        days: run.length,
        unexplainedDays: run.filter((r) => r.status === 2).length,
        recordIds: run.map((r) => r.id),
      });
    }
    run = [];
  };

  for (const record of oldestFirst) {
    if (record.status === 6) continue;
    if (isAbsence(record.status)) run.push(record);
    else close();
  }
  close();

  return streaks.reverse(); // newest first, like the list
}

export interface IAttendanceMonth {
  /** e.g. "September 2026" */
  title: string;
  data: IAttendanceRecord[];
}

/** Newest month first, newest day first within it. */
export function groupByMonth(records: IAttendanceRecord[]): IAttendanceMonth[] {
  const newestFirst = [...records].sort((a, b) => b.attendanceDate.localeCompare(a.attendanceDate));
  const months: IAttendanceMonth[] = [];
  for (const record of newestFirst) {
    const title = new Date(record.attendanceDate).toLocaleDateString("en-ZA", { month: "long", year: "numeric" });
    const last = months[months.length - 1];
    if (last?.title === title) last.data.push(record);
    else months.push({ title, data: [record] });
  }
  return months;
}

export function formatSchoolDay(value: string): string {
  return new Date(value).toLocaleDateString("en-ZA", { weekday: "short", day: "numeric", month: "short" });
}
