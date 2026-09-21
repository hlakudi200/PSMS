import type { BadgeTone } from "../components/Badge";
import type { CapsAchievementLevel, MarkStatus } from "../providers/marks/context";

export const ACHIEVEMENT_LEVEL_LABELS: Record<CapsAchievementLevel, string> = {
  1: "Not Achieved",
  2: "Elementary Achievement",
  3: "Moderate Achievement",
  4: "Adequate Achievement",
  5: "Substantial Achievement",
  6: "Meritorious Achievement",
  7: "Outstanding Achievement",
};

export const ACHIEVEMENT_LEVEL_TONES: Record<CapsAchievementLevel, BadgeTone> = {
  1: "danger", 2: "danger", 3: "warning", 4: "warning", 5: "success", 6: "success", 7: "success",
};

export const MARK_STATUS_LABELS: Record<MarkStatus, string> = {
  1: "Pending", 2: "Completed", 3: "Absent", 4: "Exempted", 5: "Incomplete",
};

export const MARK_STATUS_TONES: Record<MarkStatus, BadgeTone> = {
  1: "muted", 2: "success", 3: "danger", 4: "muted", 5: "warning",
};

const MARK_STATUS_PENDING: MarkStatus = 1;

/** A mark whose assessment hasn't had its marks released yet reads as "Pending", never "missing". */
export function markDisplayStatusLabel(status: MarkStatus, marksReleased: boolean): string {
  if (!marksReleased) return MARK_STATUS_LABELS[MARK_STATUS_PENDING];
  return MARK_STATUS_LABELS[status];
}

export function markDisplayStatusTone(status: MarkStatus, marksReleased: boolean): BadgeTone {
  if (!marksReleased) return MARK_STATUS_TONES[MARK_STATUS_PENDING];
  return MARK_STATUS_TONES[status];
}

export function formatPercentage(value?: number): string {
  if (value === undefined || value === null) return "—";
  return `${Math.round(value * 10) / 10}%`;
}

export function formatDate(value?: string): string {
  if (!value) return "—";
  return new Date(value).toLocaleDateString("en-ZA", { day: "numeric", month: "short", year: "numeric" });
}
