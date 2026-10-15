import type { CapsAchievementLevel, IAssessmentDetail, IMark } from "../providers/marks/context";

export interface IMarkRow {
  key: string;
  assessmentId: string;
  assessmentName: string;
  subjectName: string;
  assessmentType?: number;
  maxMarks?: number;
  /** The scheduled date, else the due date. */
  date?: string;
  hasMark: boolean;
  marksReleased: boolean;
  percentage?: number;
  rawMark?: number;
  achievementLevel?: CapsAchievementLevel;
  wasAbsent: boolean;
  hasFeedback: boolean;
  feedback?: string;
}

export interface ISubjectSection {
  title: string;
  /** Mean of the released, non-absent percentages; undefined until one is released. */
  average?: number;
  /** Assessments without a released mark yet. */
  pendingCount: number;
  data: IMarkRow[];
}

/** A row whose score can be shown: the teacher released it and the learner wasn't absent. */
export const isGradedRow = (row: IMarkRow) =>
  row.hasMark && row.marksReleased && !row.wasAbsent && row.percentage != null;

const mean = (values: number[]) => values.reduce((a, b) => a + b, 0) / values.length;

/**
 * Groups a learner's marks by subject. Any published class assessment without a mark yet is
 * added as pending — "not released yet" must never read as "missing".
 */
export function buildSubjectSections(
  marks: IMark[],
  classAssessments: IAssessmentDetail[],
  assessmentDetailsById: Record<string, IAssessmentDetail>
): ISubjectSection[] {
  const rows = new Map<string, IMarkRow>(); // keyed by assessmentId

  for (const mark of marks) {
    const detail = assessmentDetailsById[mark.assessmentId];
    rows.set(mark.assessmentId, {
      key: mark.id,
      assessmentId: mark.assessmentId,
      assessmentName: mark.assessmentName,
      subjectName: detail?.subjectName ?? "Other",
      assessmentType: detail?.assessmentType,
      maxMarks: detail?.maxMarks,
      date: detail?.scheduledDate ?? detail?.dueDate,
      hasMark: true,
      marksReleased: detail?.marksReleased ?? false,
      percentage: mark.percentage,
      rawMark: mark.rawMark,
      achievementLevel: mark.achievementLevel,
      wasAbsent: mark.wasAbsent,
      hasFeedback: !!mark.feedback,
      feedback: mark.feedback,
    });
  }

  for (const assessment of classAssessments) {
    if (rows.has(assessment.id)) continue;
    rows.set(assessment.id, {
      key: `pending-${assessment.id}`,
      assessmentId: assessment.id,
      assessmentName: assessment.name,
      subjectName: assessment.subjectName,
      assessmentType: assessment.assessmentType,
      maxMarks: assessment.maxMarks,
      date: assessment.scheduledDate ?? assessment.dueDate,
      hasMark: false,
      marksReleased: false,
      wasAbsent: false,
      hasFeedback: false,
    });
  }

  const bySubject = new Map<string, IMarkRow[]>();
  for (const row of rows.values()) {
    const list = bySubject.get(row.subjectName) ?? [];
    list.push(row);
    bySubject.set(row.subjectName, list);
  }

  return [...bySubject.entries()]
    .sort(([a], [b]) => a.localeCompare(b))
    .map(([title, data]) => {
      const graded = data.filter(isGradedRow);
      return {
        title,
        average: graded.length ? mean(graded.map((r) => r.percentage ?? 0)) : undefined,
        pendingCount: data.filter((r) => !r.hasMark || !r.marksReleased).length,
        data: [...data].sort((a, b) => (a.date ?? "").localeCompare(b.date ?? "")),
      };
    });
}

/** The same per-subject average as buildSubjectSections, for marks alone (used for term-to-term trend). */
export function averageBySubject(
  marks: IMark[],
  assessmentDetailsById: Record<string, IAssessmentDetail>
): Record<string, number> {
  const bySubject: Record<string, number[]> = {};
  for (const mark of marks) {
    const detail = assessmentDetailsById[mark.assessmentId];
    if (!detail?.marksReleased || mark.wasAbsent || mark.percentage == null) continue;
    (bySubject[detail.subjectName] ??= []).push(mark.percentage);
  }
  return Object.fromEntries(Object.entries(bySubject).map(([subject, values]) => [subject, mean(values)]));
}
