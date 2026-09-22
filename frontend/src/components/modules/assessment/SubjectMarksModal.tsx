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
import type { IReportSubject } from '@/providers/assessment/shared/interfaces';
import { formatPercentage } from '@/utils/marks';

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
  /** Marks close at approval, the same boundary the backend draws. */
  readOnly?: boolean;
  onClose: () => void;
  onSaved?: () => void;
}

const SubjectMarksModalInner: React.FC<Props> = ({
  open,
  reportId,
  readOnly,
  onClose,
  onSaved,
}) => {
  const { getByReportAsync, bulkRecordMarksAsync } = useReportSubjectActions();
  const { reportSubjects, isPending } = useReportSubjectState();

  const [rows, setRows] = useState<DraftRow[]>([]);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (open) getByReportAsync(reportId);
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
      })),
    );
  }, [reportSubjects]);

  const update = (id: string, patch: Partial<DraftRow>) =>
    setRows((current) =>
      current.map((r) => (r.reportSubjectId === id ? { ...r, ...patch } : r)),
    );

  /** The final mark as the backend will compute it, so the row previews itself. */
  const previewFinal = (row: DraftRow): number | undefined => {
    const term = row.termMark ?? undefined;
    const exam = row.examMark ?? undefined;

    if (term !== undefined && exam !== undefined)
      return (term * row.termWeight) / 100 + (exam * row.examWeight) / 100;

    return term ?? exam;
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

    setSaving(true);
    try {
      await bulkRecordMarksAsync({
        reportId,
        subjectMarks: rows.map((r) => ({
          reportSubjectId: r.reportSubjectId,
          termMark: r.termMark ?? undefined,
          examMark: r.examMark ?? undefined,
          termWeight: r.termWeight,
          examWeight: r.examWeight,
          teacherComment: r.teacherComment?.trim() || undefined,
        })),
      });
      message.success('Subject marks saved');
      onSaved?.();
      onClose();
    } catch {
      message.error('Could not save the subject marks');
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
          disabled={readOnly}
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
          disabled={readOnly}
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
                disabled={readOnly}
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
                disabled={readOnly}
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
          disabled={readOnly}
          placeholder="Strengths and what to work on"
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
            Save marks
          </Button>
        ),
      ]}
    >
      <Space direction="vertical" size="middle" style={{ width: '100%' }}>
        {readOnly && (
          <Alert
            type="info"
            showIcon
            message="This report has been approved, so its marks are fixed."
          />
        )}

        {!readOnly && (
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
          loading={isPending && rows.length === 0}
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
  <ReportSubjectProvider>
    <SubjectMarksModalInner {...props} />
  </ReportSubjectProvider>
);

export default SubjectMarksModal;
