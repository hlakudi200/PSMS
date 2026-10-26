import { z } from "zod";

/** The server's CreateMessageDto limits. */
export const MAX_SUBJECT_LENGTH = 200;
export const MAX_CONTENT_LENGTH = 10000;

const content = z
  .string()
  .trim()
  .min(1, "Write a message first.")
  .max(MAX_CONTENT_LENGTH, `Keep the message under ${MAX_CONTENT_LENGTH.toLocaleString("en-ZA")} characters.`);

export const composeSchema = z.object({
  recipientUserId: z.number({ error: "Choose a teacher to write to." }),
  subject: z.string().trim().max(MAX_SUBJECT_LENGTH, `Keep the subject under ${MAX_SUBJECT_LENGTH} characters.`),
  content,
});

export const replySchema = z.object({ content });

export type ComposeForm = z.infer<typeof composeSchema>;

/** "Re: Homework", never "Re: Re: Homework". */
export const replySubject = (subject?: string) =>
  subject ? `Re: ${subject.replace(/^(re:\s*)+/i, "")}` : undefined;

/** Today's messages show the time; older ones the date. */
export function formatMessageTime(value: string): string {
  const date = new Date(value);
  const today = new Date();
  return date.toDateString() === today.toDateString()
    ? date.toLocaleTimeString("en-ZA", { hour: "2-digit", minute: "2-digit" })
    : date.toLocaleDateString("en-ZA", { day: "numeric", month: "short" });
}
