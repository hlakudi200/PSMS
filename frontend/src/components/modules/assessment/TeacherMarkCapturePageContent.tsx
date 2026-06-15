'use client';

import { useCallback, useEffect, useMemo, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import {
  Alert,
  Button,
  Card,
  Checkbox,
  Descriptions,
  Empty,
  Input,
  InputNumber,
  Space,
  Spin,
  Table,
  Tag,
  Tooltip,
  Typography,
  message,
} from 'antd';
import { ArrowLeftOutlined, CommentOutlined, ImportOutlined, LockOutlined, SaveOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import {
  AssessmentProvider,
  useAssessmentActions,
  useAssessmentState,
} from '@/providers/assessment/assessments';
import {
  MarkProvider,
  useMarkActions,
  useMarkState,
} from '@/providers/assessment/marks';
import {
  ClassSubjectProvider,
  useClassSubjectActions,
  useClassSubjectState,
} from '@/providers/academic/class_subjects';
import {
  StudentClassProvider,
  useStudentClassActions,
  useStudentClassState,
} from '@/providers/academic/student_classes';
import type { IMarkList, IStudentMark } from '@/providers/assessment/shared/interfaces';
import { ImportMarksModal } from '@/components/modals/assessment/ImportMarksModal';
import { FeedbackEditModal } from '@/components/modals/assessment/FeedbackEditModal';

const { Title, Text } = Typography;

// CAPS 7-level achievement scale (BR-GR-001..006). Kept in sync with the
// backend Mark.CalculateAchievementLevel thresholds.
const ACHIEVEMENT_META: Record<number, { label: string; color: string }> = {
  1: { label: 'Not Achieved', color: 'red' },
  2: { label: 'Elementary', color: 'orange' },
  3: { label: 'Moderate', color: 'gold' },
  4: { label: 'Adequate', color: 'lime' },
  5: { label: 'Substantial', color: 'green' },
  6: { label: 'Meritorious', color: 'blue' },
  7: { label: 'Outstanding', color: 'purple' },
};

const MARK_STATUS_META: Record<number, { label: string; color: string }> = {
  1: { label: 'Pending', color: 'default' },
  2: { label: 'Completed', color: 'green' },
  3: { label: 'Absent', color: 'orange' },
  4: { label: 'Exempted', color: 'blue' },
  5: { label: 'Incomplete', color: 'red' },
};

function achievementLevelFromPercent(percent: number): number {
  if (percent < 30) return 1;
  if (percent < 40) return 2;
  if (percent < 50) return 3;
  if (percent < 60) return 4;
  if (percent < 70) return 5;
  if (percent < 80) return 6;
  return 7;
}

interface CaptureRow {
  studentId: string;
  studentName: string;
  // existing mark (read-only) if already recorded
  existingMark?: IMarkList;
  // editable draft (only used when there is no existing mark)
  rawMark: number | null;
  wasAbsent: boolean;
  teacherComment: string;
}

function MarkCaptureContent() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const assessmentId = params.id;

  const { getAsync: getAssessment } = useAssessmentActions();
  const { assessment, isPending: assessmentPending, isError: assessmentError } = useAssessmentState();

  const { getByAssessmentAsync, bulkRecordMarksAsync } = useMarkActions();
  const { marks, isPending: marksPending, isError: marksError } = useMarkState();

  const { getAsync: getClassSubject } = useClassSubjectActions();
  const { classSubject, isPending: classSubjectPending, isError: classSubjectError } = useClassSubjectState();

  const { getByClassAsync } = useStudentClassActions();
  const { studentClasses, isPending: studentsPending, isError: studentsError } = useStudentClassState();

  const [rows, setRows] = useState<Record<string, CaptureRow>>({});
  const [saving, setSaving] = useState(false);
  const [importOpen, setImportOpen] = useState(false);
  const [feedbackTarget, setFeedbackTarget] = useState<{ markId: string; studentName: string } | null>(null);

  // 1. Load the assessment + its existing marks.
  useEffect(() => {
    if (assessmentId) {
      getAssessment(assessmentId);
      getByAssessmentAsync(assessmentId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [assessmentId]);

  // 2. Resolve the class behind the assessment's class-subject.
  useEffect(() => {
    if (assessment?.classSubjectId) {
      getClassSubject(assessment.classSubjectId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [assessment?.classSubjectId]);

  // 3. Load the students enrolled in that class.
  useEffect(() => {
    if (classSubject?.classId) {
      getByClassAsync(classSubject.classId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [classSubject?.classId]);

  // Released marks are locked (BR-GR / GA-002) — the whole sheet becomes
  // read-only once the assessment's marks are released.
  const locked = assessment?.marksReleased === true;

  const marksByStudent = useMemo(() => {
    const map = new Map<string, IMarkList>();
    // Scope to THIS assessment — provider state persists across the [id]
    // route segment, so a previously-viewed sheet's marks could otherwise
    // bleed in for a frame.
    (marks ?? [])
      .filter((m) => m.assessmentId === assessmentId)
      .forEach((m) => map.set(m.studentId, m));
    return map;
  }, [marks, assessmentId]);

  // Build / refresh editable rows from enrolled students merged with any
  // existing marks. Existing marks are read-only here; only students without
  // a mark yet are editable (capture is create-only via BulkRecordMarks).
  const enrolled = useMemo(
    () => (studentClasses ?? []).filter((sc) => sc.isActive && sc.isCurrent),
    [studentClasses]
  );

  useEffect(() => {
    // Only build once the loaded class-subject/marks belong to THIS
    // assessment (providers persist across the [id] segment).
    if (assessment?.id !== assessmentId) return;
    setRows((prev) => {
      const next: Record<string, CaptureRow> = {};
      enrolled.forEach((sc) => {
        const existing = marksByStudent.get(sc.studentId);
        if (existing) {
          // Recorded marks are read-only — always reflect server state.
          next[sc.studentId] = {
            studentId: sc.studentId,
            studentName: sc.studentName ?? 'Student',
            existingMark: existing,
            rawMark: null,
            wasAbsent: false,
            teacherComment: '',
          };
        } else {
          // Preserve any unsaved draft for a not-yet-recorded student so a
          // post-save re-fetch doesn't wipe in-progress entries.
          const draft = prev[sc.studentId];
          next[sc.studentId] =
            draft && !draft.existingMark
              ? { ...draft, studentName: sc.studentName ?? draft.studentName }
              : {
                  studentId: sc.studentId,
                  studentName: sc.studentName ?? 'Student',
                  rawMark: null,
                  wasAbsent: false,
                  teacherComment: '',
                };
        }
      });
      return next;
    });
  }, [enrolled, marksByStudent, assessment?.id, assessmentId]);

  const maxMarks = assessment?.maxMarks ?? 0;

  const updateRow = useCallback((studentId: string, patch: Partial<CaptureRow>) => {
    setRows((prev) => ({ ...prev, [studentId]: { ...prev[studentId], ...patch } }));
  }, []);

  const rowList = useMemo(() => Object.values(rows), [rows]);

  // Rows that are new (no existing mark) and have something to save.
  const pendingEntries = useMemo(
    () => rowList.filter((r) => !r.existingMark && (r.wasAbsent || r.rawMark != null)),
    [rowList]
  );

  // Passed to the Excel-import modal so it scopes to the same enrolled set
  // the grid uses and flags students who already have a mark.
  const enrolledStudentIds = useMemo(
    () => new Set(enrolled.map((sc) => sc.studentId)),
    [enrolled]
  );
  const recordedStudentIds = useMemo(
    () => new Set(Array.from(marksByStudent.keys())),
    [marksByStudent]
  );

  const handleSave = async () => {
    if (locked) return;

    // Client-side range validation mirroring the server (BR-GR-001).
    for (const r of pendingEntries) {
      if (!r.wasAbsent && r.rawMark != null && (r.rawMark < 0 || r.rawMark > maxMarks)) {
        message.error(`${r.studentName}: mark must be between 0 and ${maxMarks}.`);
        return;
      }
    }

    if (pendingEntries.length === 0) {
      message.info('No new marks to save.');
      return;
    }

    const studentMarks: IStudentMark[] = pendingEntries.map((r) => ({
      studentId: r.studentId,
      rawMark: r.wasAbsent ? undefined : r.rawMark ?? undefined,
      wasAbsent: r.wasAbsent,
      teacherComment: r.teacherComment.trim() || undefined,
    }));

    setSaving(true);
    try {
      await bulkRecordMarksAsync({ assessmentId, studentMarks });
      message.success(`Saved ${studentMarks.length} mark${studentMarks.length === 1 ? '' : 's'}`);
      // Re-fetch so the freshly-saved marks become read-only rows with their
      // server-computed percentage + achievement level.
      getByAssessmentAsync(assessmentId);
    } catch {
      // Surfaced by the axios interceptor (range, duplicate, locked, etc.)
    } finally {
      setSaving(false);
    }
  };

  if (assessmentPending && !assessment) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 80 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (assessmentError || (!assessmentPending && !assessment)) {
    return (
      <div>
        <Button icon={<ArrowLeftOutlined />} onClick={() => router.push('/teacher/mark-sheets')}>
          Back to Mark Sheets
        </Button>
        <Empty description="Assessment not found" style={{ marginTop: 32 }} />
      </div>
    );
  }
  if (!assessment) return null;

  const loading = marksPending || studentsPending || classSubjectPending;
  const loadError = marksError || classSubjectError || studentsError;

  const columns: ColumnsType<CaptureRow> = [
    {
      title: 'Student',
      dataIndex: 'studentName',
      key: 'studentName',
      render: (v: string, row) => (
        <Space>
          <Text strong>{v}</Text>
          {row.existingMark && (
            <Tooltip title="Already recorded">
              <Tag color="default">recorded</Tag>
            </Tooltip>
          )}
        </Space>
      ),
    },
    {
      title: `Mark (/${maxMarks})`,
      key: 'rawMark',
      width: 140,
      render: (_: unknown, row) => {
        if (row.existingMark) {
          return row.existingMark.wasAbsent
            ? <Text type="secondary">—</Text>
            : <Text>{row.existingMark.rawMark ?? '—'}</Text>;
        }
        return (
          <InputNumber
            min={0}
            max={maxMarks}
            value={row.rawMark}
            disabled={locked || row.wasAbsent}
            onChange={(v) => updateRow(row.studentId, { rawMark: typeof v === 'number' ? v : null })}
            style={{ width: '100%' }}
            aria-label={`Mark for ${row.studentName}`}
          />
        );
      },
    },
    {
      title: 'Percent',
      key: 'percent',
      width: 100,
      render: (_: unknown, row) => {
        if (row.existingMark) {
          return row.existingMark.percentage != null
            ? `${row.existingMark.percentage.toFixed(1)}%`
            : '—';
        }
        if (row.wasAbsent || row.rawMark == null || maxMarks <= 0) return <Text type="secondary">—</Text>;
        return `${((row.rawMark / maxMarks) * 100).toFixed(1)}%`;
      },
    },
    {
      title: 'Achievement',
      key: 'achievement',
      width: 150,
      render: (_: unknown, row) => {
        let level: number | undefined;
        if (row.existingMark) {
          level = row.existingMark.achievementLevel ?? undefined;
        } else if (!row.wasAbsent && row.rawMark != null && maxMarks > 0) {
          level = achievementLevelFromPercent((row.rawMark / maxMarks) * 100);
        }
        if (!level) return <Text type="secondary">—</Text>;
        const meta = ACHIEVEMENT_META[level];
        return meta ? <Tag color={meta.color}>{meta.label}</Tag> : <Tag>{level}</Tag>;
      },
    },
    {
      title: 'Absent',
      key: 'wasAbsent',
      width: 80,
      render: (_: unknown, row) => {
        if (row.existingMark) {
          return row.existingMark.wasAbsent ? <Tag color="orange">Yes</Tag> : <Tag>No</Tag>;
        }
        return (
          <Checkbox
            checked={row.wasAbsent}
            disabled={locked}
            onChange={(e) => updateRow(row.studentId, { wasAbsent: e.target.checked })}
            aria-label={`Mark ${row.studentName} absent`}
          />
        );
      },
    },
    {
      title: 'Notes',
      key: 'teacherComment',
      render: (_: unknown, row) => {
        if (row.existingMark) return <Text type="secondary">—</Text>;
        return (
          <Input
            value={row.teacherComment}
            disabled={locked}
            maxLength={1000}
            placeholder="Optional"
            onChange={(e) => updateRow(row.studentId, { teacherComment: e.target.value })}
          />
        );
      },
    },
    {
      title: 'Status',
      key: 'status',
      width: 110,
      render: (_: unknown, row) => {
        if (!row.existingMark) return <Tag>New</Tag>;
        const meta = MARK_STATUS_META[row.existingMark.status];
        return meta ? <Tag color={meta.color}>{meta.label}</Tag> : <Tag>—</Tag>;
      },
    },
    {
      title: 'Feedback',
      key: 'feedback',
      width: 120,
      render: (_: unknown, row) =>
        row.existingMark ? (
          <Button
            size="small"
            icon={<CommentOutlined />}
            onClick={() =>
              setFeedbackTarget({ markId: row.existingMark!.id, studentName: row.studentName })
            }
          >
            Feedback
          </Button>
        ) : (
          <Text type="secondary">—</Text>
        ),
    },
  ];

  return (
    <div>
      <Button
        icon={<ArrowLeftOutlined />}
        onClick={() => router.push('/teacher/mark-sheets')}
        style={{ marginBottom: 16 }}
      >
        Back to Mark Sheets
      </Button>

      <Card variant="borderless" style={{ marginBottom: 16 }}>
        <Space style={{ width: '100%', justifyContent: 'space-between' }} align="start">
          <div>
            <Title level={4} style={{ margin: 0 }}>
              {assessment.name}
            </Title>
            <Descriptions size="small" column={{ xs: 1, sm: 2, md: 4 }} style={{ marginTop: 8 }}>
              <Descriptions.Item label="Class">{assessment.className ?? '—'}</Descriptions.Item>
              <Descriptions.Item label="Subject">{assessment.subjectName ?? '—'}</Descriptions.Item>
              <Descriptions.Item label="Term">{assessment.termName ?? '—'}</Descriptions.Item>
              <Descriptions.Item label="Max marks">{assessment.maxMarks}</Descriptions.Item>
            </Descriptions>
          </div>
          <Space>
            <Button
              icon={<ImportOutlined />}
              disabled={locked}
              onClick={() => setImportOpen(true)}
            >
              Import marks
            </Button>
            <Button
              type="primary"
              icon={<SaveOutlined />}
              loading={saving}
              disabled={locked || pendingEntries.length === 0}
              onClick={handleSave}
            >
              Save marks{pendingEntries.length > 0 ? ` (${pendingEntries.length})` : ''}
            </Button>
          </Space>
        </Space>
      </Card>

      <ImportMarksModal
        open={importOpen}
        assessment={assessment}
        enrolledStudentIds={enrolledStudentIds}
        recordedStudentIds={recordedStudentIds}
        onClose={(refresh) => {
          setImportOpen(false);
          if (refresh) getByAssessmentAsync(assessmentId);
        }}
      />

      <FeedbackEditModal
        open={!!feedbackTarget}
        markId={feedbackTarget?.markId ?? null}
        studentName={feedbackTarget?.studentName}
        onClose={(refresh) => {
          setFeedbackTarget(null);
          if (refresh) getByAssessmentAsync(assessmentId);
        }}
      />

      {loadError && !loading && (
        <Alert
          type="warning"
          showIcon
          message="Some data could not be loaded"
          description="The student list or existing marks failed to load. Go back and try again."
          style={{ marginBottom: 16 }}
        />
      )}

      {locked && (
        <Alert
          type="info"
          role="status"
          showIcon
          icon={<LockOutlined />}
          message="Marks released — this sheet is locked"
          description="Released marks can no longer be edited. Ask an administrator to unlock if a correction is needed."
          style={{ marginBottom: 16 }}
        />
      )}

      <Card variant="borderless" styles={{ body: { padding: 0 } }}>
        <Table<CaptureRow>
          dataSource={rowList}
          rowKey="studentId"
          loading={loading}
          pagination={{ pageSize: 50, showTotal: (t) => `${t} students` }}
          size="small"
          columns={columns}
          scroll={{ x: 1000 }}
          locale={{
            emptyText: (
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description="No students enrolled in this class."
              />
            ),
          }}
        />
      </Card>
    </div>
  );
}

export default function TeacherMarkCapturePageContent() {
  return (
    <AssessmentProvider>
      <ClassSubjectProvider>
        <StudentClassProvider>
          <MarkProvider>
            <MarkCaptureContent />
          </MarkProvider>
        </StudentClassProvider>
      </ClassSubjectProvider>
    </AssessmentProvider>
  );
}
