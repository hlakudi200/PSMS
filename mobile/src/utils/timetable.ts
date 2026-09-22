import type { ITimetableSlot } from "../providers/timetable/context";

/**
 * Breaks aren't stored as data (a TimetableSlot only has periods) — the
 * schedule just leaves a time gap between two periods. This infers those
 * gaps from the slots' own start/end times, mirroring the web grid's
 * `utils/timetable-grid.detectBreaks` so both surfaces agree on what counts
 * as a break.
 */
export interface TimetableBreak {
  key: string;
  sortOrder: number;
  afterPeriod: number;
  startTime: string;
  endTime: string;
}

interface PeriodTimed {
  periodNumber: number;
  startTime?: string;
  endTime?: string;
}

export function detectBreaks<T extends PeriodTimed>(slots: T[]): TimetableBreak[] {
  const byPeriod = new Map<number, T>();
  for (const slot of slots) {
    if (slot.startTime && slot.endTime && !byPeriod.has(slot.periodNumber)) {
      byPeriod.set(slot.periodNumber, slot);
    }
  }

  const periods = Array.from(byPeriod.keys()).sort((a, b) => a - b);
  const breaks: TimetableBreak[] = [];

  for (let i = 0; i < periods.length - 1; i++) {
    const current = byPeriod.get(periods[i])!;
    const next = byPeriod.get(periods[i + 1])!;
    if (current.endTime! < next.startTime!) {
      breaks.push({
        key: `break-after-${periods[i]}`,
        sortOrder: periods[i] + 0.5,
        afterPeriod: periods[i],
        startTime: current.endTime!,
        endTime: next.startTime!,
      });
    }
  }

  return breaks;
}

interface PeriodTime {
  startTime: string;
  endTime: string;
}

function toMinutes(hhmmss: string): number {
  const [h, m] = hhmmss.split(":").map(Number);
  return h * 60 + m;
}

function fromMinutes(totalMinutes: number): string {
  const h = Math.floor(totalMinutes / 60).toString().padStart(2, "0");
  const m = (totalMinutes % 60).toString().padStart(2, "0");
  return `${h}:${m}:00`;
}

function mostCommon(values: number[]): number {
  const counts = new Map<number, number>();
  for (const v of values) counts.set(v, (counts.get(v) ?? 0) + 1);
  return Array.from(counts.entries()).sort((a, b) => b[1] - a[1])[0][0];
}

/**
 * A period's time is a property of its position in the school day, not of
 * whichever lesson (if any) happens to sit in it. Builds a periodNumber ->
 * {startTime, endTime} schedule for every period from 1 to maxPeriods:
 * periods with a real slot use that slot's real time; periods with no slot
 * at all — a free period, or a trailing period nothing has been placed in
 * yet — are extrapolated from the nearest known period using the
 * timetable's established period duration.
 */
export function buildPeriodSchedule<T extends PeriodTimed>(
  slots: T[],
  maxPeriods: number
): Map<number, PeriodTime> {
  const known = new Map<number, PeriodTime>();
  for (const slot of slots) {
    if (slot.startTime && slot.endTime && !known.has(slot.periodNumber)) {
      known.set(slot.periodNumber, { startTime: slot.startTime, endTime: slot.endTime });
    }
  }
  if (known.size === 0) return known;

  const duration = mostCommon(
    Array.from(known.values()).map((t) => toMinutes(t.endTime) - toMinutes(t.startTime))
  );

  const schedule = new Map(known);
  const knownPeriods = Array.from(known.keys()).sort((a, b) => a - b);
  const firstKnown = knownPeriods[0];

  let cursor = toMinutes(known.get(firstKnown)!.startTime);
  for (let p = firstKnown - 1; p >= 1; p--) {
    const end = cursor;
    const start = end - duration;
    schedule.set(p, { startTime: fromMinutes(start), endTime: fromMinutes(end) });
    cursor = start;
  }

  cursor = toMinutes(known.get(firstKnown)!.endTime);
  for (let p = firstKnown + 1; p <= maxPeriods; p++) {
    const existing = known.get(p);
    if (existing) {
      cursor = toMinutes(existing.endTime);
      continue;
    }
    const start = cursor;
    const end = start + duration;
    schedule.set(p, { startTime: fromMinutes(start), endTime: fromMinutes(end) });
    cursor = end;
  }

  return schedule;
}

/** JavaScript Date.getDay convention: 0 = Sunday … 6 = Saturday — matches the backend's DayOfWeek. */
export const DAY_LABELS_FULL: Record<number, string> = {
  0: "Sunday", 1: "Monday", 2: "Tuesday", 3: "Wednesday", 4: "Thursday", 5: "Friday", 6: "Saturday",
};

/** Default school week is Mon–Fri; Saturday is added only if the timetable actually uses it. */
export const WEEKDAY_KEYS = [1, 2, 3, 4, 5] as const;

export function formatTimeShort(value?: string): string {
  if (!value) return "";
  const parts = value.split(":");
  if (parts.length < 2) return value;
  return `${parts[0]}:${parts[1]}`;
}

export interface IScheduleRow {
  key: string;
  /** Integer period number, or a break's fractional sort order (periodNumber + 0.5). */
  period: number;
  isBreak: boolean;
  startTime?: string;
  endTime?: string;
  /** Present when a lesson occupies this period on this day; absent means a free period. */
  slot?: ITimetableSlot;
}

export interface IWeekGrid {
  days: number[];
  maxPeriods: number;
  rowsByDay: Map<number, IScheduleRow[]>;
}

/**
 * Builds a full period-by-period schedule for every day the timetable uses,
 * including free periods (a period with no lesson placed in it, shown
 * explicitly rather than left out) and break rows (inferred, not stored).
 * Mirrors the web admin grid's row-building in
 * `TimetablesPageContent.tsx`, adapted for a single student's read-only view.
 */
export function buildWeekGrid(slots: ITimetableSlot[]): IWeekGrid {
  const usesSaturday = slots.some((s) => s.dayOfWeek === 6);
  const days: number[] = usesSaturday ? [...WEEKDAY_KEYS, 6] : [...WEEKDAY_KEYS];

  // Default to 8 so a freshly-published timetable still shows a full day
  // shape rather than nothing.
  const maxPeriods = Math.max(8, ...slots.map((s) => s.periodNumber));
  const periodSchedule = buildPeriodSchedule(slots, maxPeriods);

  const slotsByDay = new Map<number, ITimetableSlot[]>();
  for (const slot of slots) {
    const list = slotsByDay.get(slot.dayOfWeek) ?? [];
    list.push(slot);
    slotsByDay.set(slot.dayOfWeek, list);
  }

  const breaks = detectBreaks(slots);

  const rowsByDay = new Map<number, IScheduleRow[]>();
  for (const day of days) {
    const daySlots = slotsByDay.get(day) ?? [];

    const periodRows: IScheduleRow[] = Array.from({ length: maxPeriods }, (_, i) => {
      const period = i + 1;
      const slot = daySlots.find((s) => s.periodNumber === period);
      const scheduled = periodSchedule.get(period);
      return {
        key: `${day}-p${period}`,
        period,
        isBreak: false,
        startTime: slot?.startTime ?? scheduled?.startTime,
        endTime: slot?.endTime ?? scheduled?.endTime,
        slot,
      };
    });

    const breakRows: IScheduleRow[] = breaks.map((b) => ({
      key: `${day}-${b.key}`,
      period: b.sortOrder,
      isBreak: true,
      startTime: b.startTime,
      endTime: b.endTime,
    }));

    rowsByDay.set(day, [...periodRows, ...breakRows].sort((a, b) => a.period - b.period));
  }

  return { days, maxPeriods, rowsByDay };
}

/** Index of the row whose time range contains `now`, or -1 if none does (e.g. before/after school, or times unknown). */
export function findCurrentRowIndex(rows: IScheduleRow[], now: Date): number {
  const h = String(now.getHours()).padStart(2, "0");
  const m = String(now.getMinutes()).padStart(2, "0");
  const s = String(now.getSeconds()).padStart(2, "0");
  const nowTime = `${h}:${m}:${s}`;
  return rows.findIndex((r) => !!r.startTime && !!r.endTime && r.startTime <= nowTime && nowTime < r.endTime);
}
