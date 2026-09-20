'use client';

import { useEffect, useState } from 'react';
import {
  Alert,
  Button,
  Checkbox,
  Divider,
  Input,
  InputNumber,
  Modal,
  Space,
  Spin,
  Tag,
  Typography,
  message,
} from 'antd';
import { LockOutlined } from '@ant-design/icons';
import { useMarkActions, useMarkState } from '@/providers/assessment/marks';

const { Text, Paragraph } = Typography;

// MarkStatus enum (backend/src/psms.Core/Domain/Shared/Enums/MarkStatus.cs).
const MARK_STATUS_COMPLETED = 2;

interface EditMarkModalProps {
  open: boolean;
  onClose: (refresh: boolean) => void;
  markId: string | null;
  studentName?: string;
  maxMarks: number;
}

export const EditMarkModal: React.FC<EditMarkModalProps> = ({
  open,
  onClose,
  markId,
  studentName,
  maxMarks,
}) => {
  const { getAsync, updateMarkAsync, applyModerationAsync } = useMarkActions();
  const { mark, isPending } = useMarkState();

  const [rawMark, setRawMark] = useState<number | null>(null);
  const [wasAbsent, setWasAbsent] = useState(false);
  const [teacherComment, setTeacherComment] = useState('');
  const [saving, setSaving] = useState(false);

  const [moderationAdjustment, setModerationAdjustment] = useState<number | null>(null);
  const [moderating, setModerating] = useState(false);

  useEffect(() => {
    if (open && markId) {
      getAsync(markId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, markId]);

  // Sync local form state once the loaded mark matches the one we opened.
  useEffect(() => {
    if (open && mark && mark.id === markId) {
      setRawMark(mark.rawMark ?? null);
      setWasAbsent(mark.wasAbsent);
      setTeacherComment(mark.teacherComment ?? '');
      setModerationAdjustment(null);
    }
  }, [open, mark, markId]);

  const loadedMark = mark && mark.id === markId ? mark : undefined;

  // GA-002: a completed mark on a released assessment is locked. Teachers
  // don't hold the Unlock permission (see the page-level banner), so there's
  // nothing actionable to offer here beyond explaining why it's read-only.
  const locked = !!loadedMark && loadedMark.status === MARK_STATUS_COMPLETED && !!loadedMark.marksReleased;

  const handleSave = async () => {
    if (!markId || !loadedMark) return;
    if (!wasAbsent && rawMark != null && (rawMark < 0 || rawMark > maxMarks)) {
      message.error(`Mark must be between 0 and ${maxMarks}.`);
      return;
    }
    if (!wasAbsent && rawMark == null) {
      message.error('Enter a mark, or mark the student absent.');
      return;
    }

    setSaving(true);
    try {
      await updateMarkAsync(markId, {
        assessmentId: loadedMark.assessmentId,
        studentId: loadedMark.studentId,
        rawMark: wasAbsent ? undefined : rawMark ?? undefined,
        wasAbsent,
        // Preserved as-is — UpdateMark replaces the whole record, and
        // reassessment/feedback have their own dedicated flows.
        isReassessment: loadedMark.isReassessment,
        feedback: loadedMark.feedback,
        teacherComment: teacherComment.trim() || undefined,
      });
      message.success('Mark updated');
      onClose(true);
    } catch {
      // Surfaced by the axios interceptor (range, locked, etc.)
    } finally {
      setSaving(false);
    }
  };

  const handleApplyModeration = async () => {
    if (!markId || moderationAdjustment == null) return;
    setModerating(true);
    try {
      await applyModerationAsync(markId, moderationAdjustment);
      message.success('Moderation applied');
      // Refresh in place so the teacher sees the recalculated percentage/
      // achievement level without losing their place in the modal.
      await getAsync(markId);
      setModerationAdjustment(null);
    } catch {
      // Surfaced by the axios interceptor (locked, no raw mark, etc.)
    } finally {
      setModerating(false);
    }
  };

  return (
    <Modal
      open={open}
      title={`Edit Mark${studentName ? ` — ${studentName}` : ''}`}
      onCancel={() => onClose(true)}
      destroyOnHidden
      width={520}
      footer={
        <Space>
          <Button onClick={() => onClose(true)}>Close</Button>
          <Button type="primary" onClick={handleSave} loading={saving} disabled={locked || isPending}>
            Save changes
          </Button>
        </Space>
      }
    >
      {isPending && !loadedMark ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: 32 }}>
          <Spin />
        </div>
      ) : (
        <Space direction="vertical" style={{ width: '100%' }} size={12}>
          {locked && (
            <Alert
              type="info"
              showIcon
              icon={<LockOutlined />}
              message="This mark is locked"
              description="Marks have been released. Ask an administrator to unlock this mark before editing."
            />
          )}

          <div>
            <Text strong>Was Absent</Text>
            <div>
              <Checkbox
                checked={wasAbsent}
                disabled={locked}
                onChange={(e) => setWasAbsent(e.target.checked)}
              >
                Student did not write this assessment
              </Checkbox>
            </div>
          </div>

          <div>
            <Text strong>Raw Mark (/{maxMarks})</Text>
            <InputNumber
              min={0}
              max={maxMarks}
              value={rawMark}
              disabled={locked || wasAbsent}
              onChange={(v) => setRawMark(typeof v === 'number' ? v : null)}
              style={{ width: '100%', marginTop: 4 }}
            />
          </div>

          <div>
            <Text strong>Teacher Comment</Text>
            <Input
              value={teacherComment}
              disabled={locked}
              maxLength={1000}
              placeholder="Optional"
              onChange={(e) => setTeacherComment(e.target.value)}
              style={{ marginTop: 4 }}
            />
          </div>

          <Divider style={{ margin: '4px 0' }} />

          <div>
            <Text strong>Moderation</Text>
            <Paragraph type="secondary" style={{ marginTop: 4, marginBottom: 8, fontSize: 12 }}>
              A separate adjustment on top of the raw mark — e.g. a paper remarked after review,
              or a mark agreed after the student missed the original assessment. The raw mark stays
              on record; this adjustment is tracked alongside it.
            </Paragraph>

            {loadedMark?.isModerated && (
              <Tag color="purple" style={{ marginBottom: 8 }}>
                Currently moderated: {loadedMark.moderationAdjustment! > 0 ? '+' : ''}
                {loadedMark.moderationAdjustment}
              </Tag>
            )}

            <Space.Compact style={{ width: '100%' }}>
              <InputNumber
                value={moderationAdjustment}
                disabled={locked || wasAbsent}
                onChange={(v) => setModerationAdjustment(typeof v === 'number' ? v : null)}
                placeholder="Adjustment (e.g. -2 or 3)"
                style={{ width: '100%' }}
              />
              <Button
                onClick={handleApplyModeration}
                loading={moderating}
                disabled={locked || wasAbsent || moderationAdjustment == null}
              >
                Apply
              </Button>
            </Space.Compact>
            {wasAbsent && (
              <Text type="secondary" style={{ fontSize: 12 }}>
                Moderation needs a recorded raw mark — uncheck &quot;Was Absent&quot; first.
              </Text>
            )}
          </div>
        </Space>
      )}
    </Modal>
  );
};
