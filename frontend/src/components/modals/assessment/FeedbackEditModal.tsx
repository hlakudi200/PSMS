'use client';

import { useEffect, useMemo, useState } from 'react';
import { Alert, Input, List, Modal, Space, Spin, Typography, message } from 'antd';
import { ClockCircleOutlined, HistoryOutlined } from '@ant-design/icons';
import { useMarkActions, useMarkState } from '@/providers/assessment/marks';
import type { IFeedbackEditEntry } from '@/providers/assessment/shared/interfaces';

const { Text, Paragraph } = Typography;

const MAX_FEEDBACK = 2000;

interface FeedbackEditModalProps {
  open: boolean;
  onClose: (refresh: boolean) => void;
  markId: string | null;
  studentName?: string;
}

export const FeedbackEditModal: React.FC<FeedbackEditModalProps> = ({
  open,
  onClose,
  markId,
  studentName,
}) => {
  const { getAsync, updateFeedbackAsync } = useMarkActions();
  const { mark, isPending } = useMarkState();

  const [feedback, setFeedback] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (open && markId) {
      getAsync(markId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, markId]);

  // Sync the textarea once the loaded mark matches the one we opened.
  useEffect(() => {
    if (open && mark && mark.id === markId) {
      setFeedback(mark.feedback ?? '');
    }
  }, [open, mark, markId]);

  const loadedMark = mark && mark.id === markId ? mark : undefined;

  // TF-004 window status. Mirrors the server: free to edit until release,
  // then only within the 48h window (feedbackEditableUntil).
  const windowStatus = useMemo(() => {
    if (!loadedMark) return { editable: false, info: '', closed: false };
    if (!loadedMark.marksReleased) {
      return { editable: true, info: 'Marks not yet released — feedback is freely editable.', closed: false };
    }
    if (loadedMark.feedbackEditableUntil) {
      const deadline = new Date(loadedMark.feedbackEditableUntil);
      if (deadline.getTime() > Date.now()) {
        return {
          editable: true,
          info: `Editable until ${deadline.toLocaleString()} (48h after release).`,
          closed: false,
        };
      }
    }
    return { editable: false, info: 'The 48-hour feedback edit window has closed.', closed: true };
  }, [loadedMark]);

  const history = useMemo<IFeedbackEditEntry[]>(() => {
    if (!loadedMark?.feedbackHistory) return [];
    try {
      const parsed = JSON.parse(loadedMark.feedbackHistory);
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }, [loadedMark]);

  const handleSubmit = async () => {
    if (!markId) return;
    if (feedback.length > MAX_FEEDBACK) {
      message.error(`Feedback must be ${MAX_FEEDBACK} characters or fewer.`);
      return;
    }
    setSubmitting(true);
    try {
      await updateFeedbackAsync(markId, { feedback: feedback.trim() });
      message.success('Feedback updated');
      onClose(true);
    } catch {
      // Server rejections (window closed, inappropriate language) are shown
      // by the axios interceptor; keep the modal open so the teacher can fix.
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal
      open={open}
      title={`Feedback${studentName ? ` — ${studentName}` : ''}`}
      okText="Save feedback"
      onOk={handleSubmit}
      onCancel={() => onClose(false)}
      confirmLoading={submitting}
      okButtonProps={{ disabled: !windowStatus.editable || isPending }}
      destroyOnHidden
      width={600}
    >
      {isPending && !loadedMark ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: 32 }}>
          <Spin />
        </div>
      ) : (
        <Space direction="vertical" style={{ width: '100%' }} size={12}>
          <Alert
            type={windowStatus.closed ? 'warning' : 'info'}
            showIcon
            icon={<ClockCircleOutlined />}
            message={windowStatus.info}
          />

          <Input.TextArea
            value={feedback}
            onChange={(e) => setFeedback(e.target.value)}
            rows={5}
            maxLength={MAX_FEEDBACK}
            showCount
            disabled={!windowStatus.editable}
            placeholder="Constructive feedback for the student…"
          />

          {history.length > 0 && (
            <div>
              <Text type="secondary">
                <HistoryOutlined /> Edit history ({history.length})
              </Text>
              <List
                size="small"
                dataSource={history.slice().reverse()}
                renderItem={(h) => (
                  <List.Item>
                    <Space direction="vertical" size={0} style={{ width: '100%' }}>
                      <Text type="secondary" style={{ fontSize: 12 }}>
                        {h.at ? new Date(h.at).toLocaleString() : 'Earlier'}
                      </Text>
                      <Paragraph style={{ margin: 0 }} type="secondary">
                        {h.previous}
                      </Paragraph>
                    </Space>
                  </List.Item>
                )}
              />
            </div>
          )}
        </Space>
      )}
    </Modal>
  );
};
