'use client';

/**
 * RC-12. Capturing per-subject marks and comments on a report card.
 *
 * `ReportSubjectAppService` has exposed RecordMarks, BulkRecordMarks and
 * AddTeacherComment since the module was written, and the frontend has had a
 * complete `report_subjects` provider for them — mounted nowhere, with no
 * consumer. So the only way a subject ever got a mark was the automatic
 * aggregation during generation, and `TeacherComment` could not be set at all,
 * which is why a South African report card's named per-subject comment was
 * always blank.
 *
 * The weights are shown and editable because they are what the mark is composed
 * from, and because a school running an approved variation needs to see the
 * split it is actually using. They must total 100 — the backend refuses
 * anything else, since 80/80 produced a final mark of 160.
 */

import React, { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  Input,
  InputNumber,
  Modal,
  Space,
  Table,
  Tag,
  Typography,
  message,
} from 'antd';
import { z } from 'zod';
import {
  ReportSubjectProvider,
  useReportSubjectActions,
  useReportSubjectState,
} from '@/providers/assessment/report_subjects';
import {
  TeacherProvider,
  useTeacherState,
  useTeacherActions,
} from '@/providers/academic/teachers';
import type { IReportSubject } from '@/providers/assessment/shared/interfaces';
import { formatMark, formatPercentage } from '@/utils/marks';

const { Text } = Typography;

/** One row as the form holds it, before it is sent. */
interface DraftRow {
  reportSubjectId: string;
  subjectName?: string;
  termMark?: number | null;
  examMark?: number | null;
  termWeight: number;
  examWeight: number;
  teacherComment?: string;
  awaitsExternalExamination?: boolean;
  /** Whose subject this is, so a teacher only writes their own comment. */
  teacherId?: string;
}

const rowSchema = z
  .object({
    termMark: z.number().min(0).max(100).nullable().optional(),
    examMark: z.number().min(0).max(100).nullable().optional(),
    termWeight: z.number().min(0).max(100),
    examWeight: z.number().min(0).max(100),
    teacherComment: z.string().max(1000).optional(),
  })
  .refine((r) => r.termWeight + r.examWeight === 100, {
    message: 'The school-based and examination weights must add to 100%',
    path: ['termWeight'],
  });

interface Props {
  open: boolean;
  reportId: string;
  /** ReportStatus, so the screen can say which of the two closed states it is. */
  reportStatus?: number;
  /** A year-end mark is the promotion mark, a whole number under NPPPPR §31(3). */
  isYearEnd?: boolean;
  /**
   * RC-08. Whether this person may change the marks. A teacher holds
   * ReportCards.Comment but not Generate: they see the marks and write the
   * comments.
   */
  canEditMarks?: boolean;
  onClose: () => void;
  onSaved?: () => void;
}

const SubjectMarksModalInner: React.FC<Props> = ({
  open,
  reportId,
  reportStatus,
  isYearEnd,
  canEditMarks,
  onClose,
  onSaved,
}) => {
  const { getByReportAsync, bulkRecordMarksAsync, addTeacherCommentAsync } =
    useReportSubjectActions();
  const { reportSubjects } = useReportSubjectState();
  const { teacher: me } = useTeacherState();
  const { getByCurrentUserAsync } = useTeacherActions();

  const [rows, setRows] = useState<DraftRow[]>([]);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);

  /* Marks close at approval, the same boundary the backend draws. */
  const closed = reportStatus === 4 || reportStatus === 5;
  const marksReadOnly = closed || !canEditMarks;
  const commentsReadOnly = closed;
  const readOnly = marksReadOnly && commentsReadOnly;

  /* RC-08: the comment prints under the subject as that teacher's judgement, so
     a teacher writes only their own subject's. Senior staff (who capture marks)
     are not restricted; neither is a caller whose teacher profile did not
     resolve, since the server has the final say either way. */
  const mayCommentOn = (row: DraftRow): boolean => {
    if (commentsReadOnly) return false;
    if (canEditMarks || !me?.id) return true;
    return row.teacherId === me.id;
  };

  /* Who am I, so the comment boxes that are not mine can be closed. Only
     needed for a comment-only caller; senior staff write any subject's. */
  useEffect(() => {
    if (open && !canEditMarks) getByCurrentUserAsync();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, canEditMarks]);

  useEffect(() => {
    if (!open) return;

    setLoading(true);
    Promise.resolve(getByReportAsync(reportId))
      .catch(() => message.error('Could not load this report\u2019s subjects'))
      .finally(() => setLoading(false));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, reportId]);

  useEffect(() => {
    if (!reportSubjects) return;

    setRows(
      reportSubjects.map((s: IReportSubject) => ({
        reportSubjectId: s.id,
        subjectName: s.subjectName,
        termMark: s.termMark ?? null,
        examMark: s.examMark ?? null,
        termWeight: s.termWeight ?? 40,
        examWeight: s.examWeight ?? 60,
        teacherComment: s.teacherComment ?? '',
        awaitsExternalExamination: s.awaitsExternalExamination,
        teacherId: s.teacherId,
      })),
    );
  }, [reportSubjects]);

  /** What came back from the server, to compare a save against. */
  const original = useMemo(
    () => new Map((reportSubjects ?? []).map((s: IReportSubject) => [s.id, s])),
    [reportSubjects],
  );

  const changed = (row: DraftRow): boolean => {
    const was = original.get(row.reportSubjectId);
    if (!was) return true;

    return (
      (was.termMark ?? null) !== (row.termMark ?? null)
      || (was.examMark ?? null) !== (row.examMark ?? null)
      || (was.termWeight ?? 40) !== row.termWeight
      || (was.examWeight ?? 60) !== row.examWeight
      || (was.teacherComment ?? '') !== (row.teacherComment ?? '')
    );
  };

  const update = (id: string, patch: Partial<DraftRow>) =>
    setRows((current) =>
      current.map((r) => (r.reportSubjectId === id ? { ...r, ...patch } : r)),
    );

  /** The final mark as the backend will compute it, so the row previews itself. */
  const previewFinal = (row: DraftRow): number | undefined => {
    const term = row.termMark ?? undefined;
    const exam = row.examMark ?? undefined;

    const composed =
      term !== undefined && exam !== undefined
        ? (term * row.termWeight) / 100 + (exam * row.examWeight) / 100
        : (term ?? exam);

    if (composed === undefined) return undefined;

    /* A year-end mark is the promotion mark, rounded half-up to a whole number
       under NPPPPR §31(3) — which the server applies, so the preview has to as
       well or the row shows 74.67 and then saves as 75. */
    return isYearEnd ? Math.floor(composed + 0.5) : composed;
  };

  const invalidRows = useMemo(
    () => rows.filter((r) => !rowSchema.safeParse(r).success),
    [rows],
  );

  const handleSave = async () => {
    if (invalidRows.length > 0) {
      message.error(
        `Check ${invalidRows.length === 1 ? 'the highlighted subject' : 'the highlighted subjects'}: the weights must add to 100%.`,
      );
      return;
    }

    /* Only what actually changed. Re-sending every row rewrote rows nobody
       touched — and recording a mark clears the external-examination note, so a
       no-op save on a Grade 12 card would have quietly dropped the footnote off
       every subject. */
    const edited = rows.filter(changed);

    if (edited.length === 0) {
      onClose();
      return;
    }

    setSaving(true);
    try {
      if (canEditMarks) {
        await bulkRecordMarksAsync({
          reportId,
          subjectMarks: edited.map((r) => ({
            reportSubjectId: r.reportSubjectId,
            termMark: r.termMark ?? undefined,
            examMark: r.examMark ?? undefined,
            termWeight: r.termWeight,
            examWeight: r.examWeight,
            /* '' rather than undefined, or a comment entered by mistake could
               never be cleared — the server only writes a non-null value. */
            teacherComment: r.teacherComment?.trim() ?? '',
          })),
        });
      } else {
        /* RC-08: a teacher holds the comment permission but not Generate, so
           their save goes one comment at a time through the endpoint that
           permission gates. */
        for (const row of edited.filter(mayCommentOn)) {
          await addTeacherCommentAsync(row.reportSubjectId, row.teacherComment?.trim() ?? '');
        }
      }

      message.success(canEditMarks ? 'Subject marks saved' : 'Subject comments saved');
      onSaved?.();
      onClose();
    } catch {
      message.error(
        canEditMarks ? 'Could not save the subject marks' : 'Could not save the subject comments',
      );
    } finally {
      setSaving(false);
    }
  };

  const columns = [
    {
      title: 'Subject',
      dataIndex: 'subjectName',
      key: 'subjectName',
      width: 180,
      render: (v: string, row: DraftRow) => (
        <Space direction="vertical" size={0}>
          <Text strong>{v ?? 'Unknown subject'}</Text>
          {row.awaitsExternalExamination && (
            <Tag color="orange">School-based component only</Tag>
          )}
        </Space>
      ),
    },
    {
      title: 'School-based (%)',
      key: 'termMark',
      width: 130,
      render: (_: unknown, row: DraftRow) => (
        <InputNumber
          min={0}
          max={100}
          value={row.termMark ?? undefined}
          disabled={marksReadOnly}
          style={{ width: '100%' }}
          onChange={(v) => update(row.reportSubjectId, { termMark: v as number | null })}
        />
      ),
    },
    {
      title: 'Examination (%)',
      key: 'examMark',
      width: 130,
      render: (_: unknown, row: DraftRow) => (
        <InputNumber
          min={0}
          max={100}
          value={row.examMark ?? undefined}
          disabled={marksReadOnly}
          style={{ width: '100%' }}
          onChange={(v) => update(row.reportSubjectId, { examMark: v as number | null })}
        />
      ),
    },
    {
      title: 'Weights',
      key: 'weights',
      width: 170,
      render: (_: unknown, row: DraftRow) => {
        const total = row.termWeight + row.examWeight;

        return (
          <Space direction="vertical" size={2} style={{ width: '100%' }}>
            <Space size={4}>
              <InputNumber
                min={0}
                max={100}
                value={row.termWeight}
                disabled={marksReadOnly}
                style={{ width: 70 }}
                onChange={(v) =>
                  update(row.reportSubjectId, { termWeight: (v as number) ?? 0 })
                }
              />
              <Text type="secondary">:</Text>
              <InputNumber
                min={0}
                max={100}
                value={row.examWeight}
                disabled={marksReadOnly}
                style={{ width: 70 }}
                onChange={(v) =>
                  update(row.reportSubjectId, { examWeight: (v as number) ?? 0 })
                }
              />
            </Space>
            {total !== 100 && (
              <Text type="danger" style={{ fontSize: 11 }}>
                Must add to 100 (currently {total})
              </Text>
            )}
          </Space>
        );
      },
    },
    {
      title: 'Final',
      key: 'final',
      width: 90,
      render: (_: unknown, row: DraftRow) => {
        const value = previewFinal(row);

        return value === undefined ? (
          <Text type="secondary">-</Text>
        ) : (
          <Text strong>{formatPercentage(value)}</Text>
        );
      },
    },
    {
      title: 'Teacher comment',
      key: 'teacherComment',
      render: (_: unknown, row: DraftRow) => (
        <Input.TextArea
          rows={1}
          maxLength={1000}
          value={row.teacherComment}
          disabled={!mayCommentOn(row)}
          placeholder={
            mayCommentOn(row)
              ? 'Strengths and what to work on'
              : `Written by ${row.teacherId ? 'this subject’s teacher' : 'the subject teacher'}`
          }
          onChange={(e) =>
            update(row.reportSubjectId, { teacherComment: e.target.value })
          }
        />
      ),
    },
  ];

  return (
    <Modal
      open={open}
      title="Subject marks and comments"
      onCancel={onClose}
      width={1000}
      footer={[
        <Button key="cancel" onClick={onClose}>
          {readOnly ? 'Close' : 'Cancel'}
        </Button>,
        !readOnly && (
          <Button
            key="save"
            type="primary"
            loading={saving}
            disabled={rows.length === 0}
            onClick={handleSave}
          >
            {canEditMarks ? 'Save marks' : 'Save comments'}
          </Button>
        ),
      ]}
    >
      <Space direction="vertical" size="middle" style={{ width: '100%' }}>
        {closed && (
          <Alert
            type="info"
            showIcon
            message={
              reportStatus === 5
                ? 'This report card has been published, so it can no longer be changed.'
                : 'This report card has been approved, so its marks and comments are fixed.'
            }
          />
        )}

        {!closed && !canEditMarks && (
          <Alert
            type="info"
            showIcon
            message="You can write the subject comments here. The marks are set by whoever generates the report."
          />
        )}

        {!closed && canEditMarks && (
          <Alert
            type="info"
            showIcon
            message="The final mark is the two components at the weights beside them."
            description={
              <Text type="secondary">
                The weights come from the school&apos;s assessment settings and the grade&apos;s
                band. Change them here only for a subject that genuinely differs.
              </Text>
            }
          />
        )}

        <Table<DraftRow>
          rowKey="reportSubjectId"
          size="small"
          loading={loading && rows.length === 0}
          dataSource={rows}
          columns={columns}
          pagination={false}
          scroll={{ x: 900 }}
        />
      </Space>
    </Modal>
  );
};

/** Mounts the provider the screen needs, which nothing had mounted before. */
export const SubjectMarksModal: React.FC<Props> = (props) => (
  <TeacherProvider>
    <ReportSubjectProvider>
      <SubjectMarksModalInner {...props} />
    </ReportSubjectProvider>
  </TeacherProvider>
);

export default SubjectMarksModal;
