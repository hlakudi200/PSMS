import dayjs, { Dayjs } from 'dayjs';
import type { ITermEventList, ITimetableSlotList } from '@/providers/academic/shared/interfaces';

// Backend psms.Domain.Shared.Enums.EventType: days with no teaching.
const HOLIDAY_EVENT_TYPES = new Set([1 /* PublicHoliday */, 2 /* SchoolHoliday */]);

// Lesson times are school-local (SAST, UTC+02:00 year-round, no DST).
const SAST_OFFSET_HOURS = 2;

// Guards the day list and the .ics against an accidental multi-year range.
export const MAX_RANGE_DAYS = 366;

/** One calendar day of a teacher's schedule. */
export interface ScheduleDay {
  /** YYYY-MM-DD */
  date: string;
  slots: ITimetableSlotList[];
  /** Name of the public/school holiday that cancels teaching on this day. */
  holiday?: string;
}

/**
 * Local calendar date of a term event. The API returns timestamptz values
 * in UTC, so an event entered as local midnight arrives as 22:00 the day
 * before; formatting in the browser's (SAST) zone gives the intended date.
 */
export function eventDateKey(eventDate: string): string {
  return dayjs(eventDate).format('YYYY-MM-DD');
}

/** Holiday name per date, from the term calendar. */
export function holidaysByDate(events: ITermEventList[] | undefined): Map<string, string> {
  const map = new Map<string, string>();
  (events ?? [])
    .filter((e) => HOLIDAY_EVENT_TYPES.has(e.eventType))
    .forEach((e) => map.set(eventDateKey(e.eventDate), e.eventName || 'Holiday'));
  return map;
}

/**
 * Expands the weekly timetable over real dates. Every day that has periods
 * (or a holiday on a weekday) is returned; days with nothing on them —
 * weekends, days the timetable doesn't use — are left out.
 */
export function expandSchedule(
  slots: ITimetableSlotList[],
  start: Dayjs,
  end: Dayjs,
  holidays: Map<string, string>
): ScheduleDay[] {
  const byDay = new Map<number, ITimetableSlotList[]>();
  slots.forEach((s) => byDay.set(s.dayOfWeek, [...(byDay.get(s.dayOfWeek) ?? []), s]));
  byDay.forEach((list) => list.sort((a, b) => a.periodNumber - b.periodNumber));

  const days: ScheduleDay[] = [];
  let cursor = start.startOf('day');
  const last = end.startOf('day');
  for (let i = 0; i < MAX_RANGE_DAYS && !cursor.isAfter(last); i++) {
    const date = cursor.format('YYYY-MM-DD');
    const daySlots = byDay.get(cursor.day()) ?? [];
    const holiday = holidays.get(date);
    const isWeekday = cursor.day() >= 1 && cursor.day() <= 5;
    if (daySlots.length > 0 || (holiday && isWeekday)) {
      days.push({ date, slots: holiday ? [] : daySlots, holiday });
    }
    cursor = cursor.add(1, 'day');
  }
  return days;
}

// ── iCalendar (RFC 5545) ────────────────────────────────────────────────

function icsEscape(value: string): string {
  return value.replace(/\\/g, '\\\\').replace(/;/g, '\\;').replace(/,/g, '\\,').replace(/\r?\n/g, '\\n');
}

/** UTC timestamp for a school-local date + "HH:mm:ss" time. */
function icsUtc(date: string, time: string): string {
  const [h, m] = time.split(':').map(Number);
  const utc = new Date(`${date}T00:00:00Z`);
  utc.setUTCHours(h - SAST_OFFSET_HOURS, m, 0, 0);
  return utc.toISOString().replace(/[-:]/g, '').replace(/\.\d{3}/, '');
}

/**
 * One VEVENT per dated period (holidays already removed). UIDs are stable per
 * slot + date, so re-importing an updated export replaces events rather than
 * duplicating them in calendars that honour UIDs.
 */
export function buildScheduleIcs(days: ScheduleDay[], calendarName: string): string {
  const stamp = new Date().toISOString().replace(/[-:]/g, '').replace(/\.\d{3}/, '');
  const lines = [
    'BEGIN:VCALENDAR',
    'VERSION:2.0',
    'PRODID:-//PSMS//Teacher Schedule//EN',
    'CALSCALE:GREGORIAN',
    'METHOD:PUBLISH',
    `X-WR-CALNAME:${icsEscape(calendarName)}`,
  ];
  days.forEach((day) =>
    day.slots.forEach((slot) => {
      lines.push(
        'BEGIN:VEVENT',
        `UID:${slot.id}-${day.date.replace(/-/g, '')}@psms`,
        `DTSTAMP:${stamp}`,
        `DTSTART:${icsUtc(day.date, slot.startTime)}`,
        `DTEND:${icsUtc(day.date, slot.endTime)}`,
        `SUMMARY:${icsEscape(slot.subjectName ?? `Period ${slot.periodNumber}`)}`,
        `DESCRIPTION:${icsEscape(`Period ${slot.periodNumber}`)}`,
        ...(slot.roomNumber ? [`LOCATION:${icsEscape(`Room ${slot.roomNumber}`)}`] : []),
        'END:VEVENT'
      );
    })
  );
  lines.push('END:VCALENDAR');
  return lines.join('\r\n') + '\r\n';
}

export function downloadTextFile(content: string, fileName: string, mimeType: string): void {
  const url = URL.createObjectURL(new Blob([content], { type: mimeType }));
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}
