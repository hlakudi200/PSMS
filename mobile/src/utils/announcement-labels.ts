import type { BadgeTone } from "../components/Badge";

/** Mirrors the backend AnnouncementType. */
export type AnnouncementType = 1 | 2 | 3 | 4 | 5 | 6 | 7;
/** Mirrors the backend AnnouncementPriority. */
export type AnnouncementPriority = 1 | 2 | 3 | 4; // Low | Normal | High | Urgent
/** Mirrors the backend AnnouncementAudience. A family is only ever sent All, Parents, Students, Grade or Class. */
export type AnnouncementAudience = 1 | 2 | 3 | 4 | 5 | 6 | 7;

export const ANNOUNCEMENT_TYPE_LABELS: Record<AnnouncementType, string> = {
  1: "General",
  2: "Academic",
  3: "Sport",
  4: "Event",
  5: "Emergency",
  6: "Holiday",
  7: "Administration",
};

/** Only the ones worth calling out; Low and Normal carry no badge. */
export const PRIORITY_BADGES: Partial<Record<AnnouncementPriority, { label: string; tone: BadgeTone }>> = {
  3: { label: "Important", tone: "warning" },
  4: { label: "Urgent", tone: "danger" },
};

/** Who it was sent to, from a parent's side of the school. */
export const PARENT_AUDIENCE_LABELS: Partial<Record<AnnouncementAudience, string>> = {
  1: "Everyone",
  4: "Parents",
  5: "Learners",
  6: "Your child's grade",
  7: "Your child's class",
};

export const isWebLink = (value?: string): value is string => !!value && /^https?:\/\//i.test(value);
