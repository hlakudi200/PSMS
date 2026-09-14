/**
 * Breaks aren't stored as data (TimetableSlot only has periods) — Generate
 * Timetables just leaves a time gap between two periods. This infers those
 * gaps from the slots' own start/end times so any weekly grid can render a
 * visible "Break" block, whether the schedule was generated or hand-edited.
 */
export interface TimetableBreak {
  key: string;
  /** Sort key that lands the break between the two periods it separates. */
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
  const [h, m] = hhmmss.split(':').map(Number);
  return h * 60 + m;
}

function fromMinutes(totalMinutes: number): string {
  const h = Math.floor(totalMinutes / 60).toString().padStart(2, '0');
  const m = (totalMinutes % 60).toString().padStart(2, '0');
  return `${h}:${m}:00`;
}

function mostCommon(values: number[]): number {
  const counts = new Map<number, number>();
  for (const v of values) counts.set(v, (counts.get(v) ?? 0) + 1);
  return Array.from(counts.entries()).sort((a, b) => b[1] - a[1])[0][0];
}

/**
 * A period's time is a property of its position in the school day, not of
 * whichever lesson (if any) happens to sit in it. This builds a
 * periodNumber -> {startTime, endTime} schedule for every period from 1 to
 * maxPeriods: periods with an actual slot somewhere use that slot's real
 * time (exact); periods with no slot at all yet — including trailing ones,
 * e.g. the last period on a day that doesn't always need it — are
 * extrapolated from the nearest known period using the timetable's
 * established period duration, so they land on the same 30/40/etc-minute
 * grid instead of being left free-form.
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

  // Backward-fill anything before the earliest known period.
  let cursor = toMinutes(known.get(firstKnown)!.startTime);
  for (let p = firstKnown - 1; p >= 1; p--) {
    const end = cursor;
    const start = end - duration;
    schedule.set(p, { startTime: fromMinutes(start), endTime: fromMinutes(end) });
    cursor = start;
  }

  // Forward-fill gaps and everything after the last known period —
  // including the trailing periods that never got a slot placed in them.
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
