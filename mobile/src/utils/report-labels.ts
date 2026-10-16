import { z } from "zod";
import type { ConductRating, PromotionDecision, ReportType } from "../providers/report-cards/context";

/** Backend ReportType. */
export const REPORT_TYPE_LABELS: Record<ReportType, string> = {
  1: "Term 1", 2: "Term 2", 3: "Term 3", 4: "Term 4", 5: "Mid-year", 6: "Year-end", 7: "Progress report",
};

/** "Term 3" from the term itself, else from the report type ("Year-end"). */
export function reportTitle(report: { reportType: ReportType; termName?: string }): string {
  return report.termName ?? REPORT_TYPE_LABELS[report.reportType] ?? "Report card";
}

/** Backend ConductDiligenceRating. */
export const CONDUCT_RATING_LABELS: Record<ConductRating, string> = {
  1: "Unsatisfactory", 2: "Poor", 3: "Needs improvement", 4: "Satisfactory", 5: "Good", 6: "Very good", 7: "Excellent",
};

/** Backend PromotionDecision. */
export const PROMOTION_DECISION_LABELS: Record<PromotionDecision, string> = {
  1: "Promoted", 2: "Retained", 3: "Conditionally promoted", 4: "Progressed with support",
};

/** Backend Report.MaxParentCommentLength. */
export const MAX_PARENT_COMMENT_LENGTH = 1000;

/** The acknowledgement form: the comment is optional, but not longer than the backend keeps. */
export const acknowledgementSchema = z.object({
  comment: z
    .string()
    .trim()
    .max(MAX_PARENT_COMMENT_LENGTH, `Keep your comment under ${MAX_PARENT_COMMENT_LENGTH} characters.`)
    .optional()
    .transform((value) => (value ? value : undefined)),
});

export type AcknowledgementInput = z.infer<typeof acknowledgementSchema>;
