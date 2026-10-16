import { z } from "zod";

/** Mirrors the backend NotificationChannel. In-app (1) is always on, so it is never a toggle. */
export type ExternalChannel = 2 | 3 | 4 | 5; // Email | Sms | Push | WhatsApp
export type NotificationChannel = 1 | ExternalChannel;

/** Mirrors the backend NotificationType. */
export type NotificationCategory = 1 | 2 | 3 | 4 | 5 | 6 | 7;

/** The order a parent reads them in: the phone first, then the ways that cost the school a message. */
export const EXTERNAL_CHANNELS: ExternalChannel[] = [4, 2, 3, 5];

export const CHANNEL_LABELS: Record<ExternalChannel, string> = {
  2: "Email",
  3: "SMS",
  4: "Push notifications",
  5: "WhatsApp",
};

export const CATEGORIES: NotificationCategory[] = [6, 4, 5, 2, 3, 1, 7];

export const CATEGORY_LABELS: Record<NotificationCategory, { title: string; detail: string }> = {
  1: { title: "General", detail: "School news and reminders" },
  2: { title: "Academic", detail: "Classes, timetables and report cards" },
  3: { title: "Financial", detail: "Statements and account notices" },
  4: { title: "Attendance", detail: "Absences and late arrivals" },
  5: { title: "Assessment", detail: "Tests, tasks and released marks" },
  6: { title: "Messages and announcements", detail: "From teachers and the school" },
  7: { title: "Account", detail: "Sign-in and account changes" },
};

/** Phone numbers as a person types them: digits, spaces, brackets, dashes and a leading +. */
const phoneNumber = z
  .string()
  .trim()
  .max(32, "Keep the number under 32 characters.")
  .refine((value) => value === "" || /^\+?[0-9 ()-]{7,}$/.test(value), "Enter a phone number, e.g. 082 123 4567.");

/** UpdateMyContact. Email is the account's sign-in and is not changed here. */
export const contactSchema = z.object({
  mobileNumber: phoneNumber,
  whatsAppNumber: phoneNumber,
});

export type ContactForm = z.infer<typeof contactSchema>;
