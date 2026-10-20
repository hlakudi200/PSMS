import type { Href } from "expo-router";

/** Mirrors the backend NotificationType; the same categories as the settings screen. */
export type NotificationType = 1 | 2 | 3 | 4 | 5 | 6 | 7;

export const NOTIFICATION_TYPE_LABELS: Record<NotificationType, string> = {
  1: "General",
  2: "Academic",
  3: "Financial",
  4: "Attendance",
  5: "Assessment",
  6: "Message",
  7: "Account",
};

/**
 * Where a notification opens, from what it says it is about. The server puts
 * the target in EntityType and EntityId rather than a web path for anything a
 * parent reads in the app. Undefined means there is nothing to open: the
 * notification is the whole message.
 */
export function notificationTarget(entityType?: string, entityId?: string): Href | undefined {
  if (!entityId) return undefined;
  switch (entityType) {
    case "Report":
      return { pathname: "/(parent)/report/[id]", params: { id: entityId } };
    default:
      return undefined;
  }
}

/** "Today", "Yesterday", or "Mon, 13 Oct": the heading a notification is grouped under. */
export function dayLabel(value: string, now: Date = new Date()): string {
  const date = new Date(value);
  const startOf = (d: Date) => new Date(d.getFullYear(), d.getMonth(), d.getDate()).getTime();
  const days = Math.round((startOf(now) - startOf(date)) / 86_400_000);
  if (days === 0) return "Today";
  if (days === 1) return "Yesterday";
  return date.toLocaleDateString("en-ZA", {
    weekday: "short", day: "numeric", month: "short",
    ...(date.getFullYear() !== now.getFullYear() && { year: "numeric" }),
  });
}

/** "14:05": the day is already the section heading. */
export const formatTime = (value: string) =>
  new Date(value).toLocaleTimeString("en-ZA", { hour: "2-digit", minute: "2-digit" });

/** Newest first in, day sections out, each keeping that order. */
export function groupByDay<T extends { creationTime: string }>(items: T[], now: Date = new Date()) {
  const sections: { title: string; data: T[] }[] = [];
  for (const item of items) {
    const title = dayLabel(item.creationTime, now);
    const last = sections[sections.length - 1];
    if (last?.title === title) last.data.push(item);
    else sections.push({ title, data: [item] });
  }
  return sections;
}
