'use client';

import { useEffect, useMemo, useRef, useState } from 'react';
import axios from 'axios';
import { Alert, Button, Input, List, Modal, Space, Spin, Tag, Tooltip, Typography, message } from 'antd';
import type { TextAreaRef } from 'antd/es/input/TextArea';
import {
  ClearOutlined,
  ClockCircleOutlined,
  EnterOutlined,
  HistoryOutlined,
  LockOutlined,
  UnorderedListOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { z } from 'zod';
import { useMarkActions, useMarkState } from '@/providers/assessment/marks';
import type { IFeedbackEditEntry } from '@/providers/assessment/shared/interfaces';

const { Text, Paragraph } = Typography;

// US-TCH-010: 20–2000 characters. Mirrors Mark.Min/MaxFeedbackLength.
const MIN_FEEDBACK = 20;
const MAX_FEEDBACK = 2000;
// TF-004: hours after release that feedback stays editable.
const EDIT_WINDOW_HOURS = 48;
// Below this, the window notice turns into a warning.
const WARN_BELOW_HOURS = 6;
const BULLET = '• ';

// Mirrors AssessmentExceptionCodes. The code arrives in the X-Error-Code
// header; error.message carries the server's sentence (PsmsErrorInfoConverter).
const FEEDBACK_ERRORS: Record<string, string> = {
  ASM_FEEDBACK_INAPPROPRIATE_LANGUAGE: 'Some words aren’t allowed in feedback',
  ASM_FEEDBACK_EDIT_WINDOW_EXPIRED: 'The edit window has closed',
  ASM_FEEDBACK_TOO_SHORT: 'Feedback is too short',
  ASM_FEEDBACK_TOO_LONG: 'Feedback is too long',
};

// Empty is allowed: it removes the feedback.
const feedbackSchema = z.object({
  feedback: z
    .string()
    .trim()
    .max(MAX_FEEDBACK, `Feedback must be ${MAX_FEEDBACK} characters or fewer.`)
    .refine((v) => v.length === 0 || v.length >= MIN_FEEDBACK, {
      message: `Feedback must be at least ${MIN_FEEDBACK} characters, or empty to remove it.`,
    }),
});

/** "1 day 4 hours", "3 hours 12 minutes", "12 minutes". */
function formatRemaining(ms: number): string {
  const minutes = Math.max(1, Math.floor(ms / 60_000));
  const days = Math.floor(minutes / 1440);
  const hours = Math.floor((minutes % 1440) / 60);
  const mins = minutes % 60;
  const part = (n: number, unit: string) => `${n} ${unit}${n === 1 ? '' : 's'}`;
  if (days > 0) return hours > 0 ? `${part(days, 'day')} ${part(hours, 'hour')}` : part(days, 'day');
  if (hours > 0) return mins > 0 ? `${part(hours, 'hour')} ${part(mins, 'minute')}` : part(hours, 'hour');
  return part(mins, 'minute');
}

const formatWhen = (value: string | Date) => dayjs(value).format('ddd D MMM YYYY, HH:mm');

interface FeedbackEditModalProps {
  open: boolean;
  onClose: (refresh: boolean) => void;
  markId: string | null;
  studentName?: string;
}

interface WindowStatus {
  editable: boolean;
  type: 'info' | 'warning';
  title: string;
  detail?: string;
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
  const [validationError, setValidationError] = useState<string | null>(null);
  const [serverError, setServerError] = useState<{ title: string; detail: string } | null>(null);
  const [now, setNow] = useState(() => Date.now());
  const textRef = useRef<TextAreaRef>(null);

  useEffect(() => {
    if (open && markId) {
      getAsync(markId);
      setValidationError(null);
      setServerError(null);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, markId]);

  // Sync the textarea once the loaded mark matches the one we opened.
  useEffect(() => {
    if (open && mark && mark.id === markId) {
      setFeedback(mark.feedback ?? '');
    }
  }, [open, mark, markId]);

  // Keep the countdown live, and lock the editor if the window closes while
  // the modal is open, rather than letting the save fail.
  useEffect(() => {
    if (!open) return;
    setNow(Date.now());
    const timer = setInterval(() => setNow(Date.now()), 30_000);
    return () => clearInterval(timer);
  }, [open]);

  const loadedMark = mark && mark.id === markId ? mark : undefined;

  // TF-004, mirroring the server: free to edit until marks are released, then
  // only within 48h of release (feedbackEditableUntil). A released mark with
  // no deadline is treated as closed, as the server does.
  const windowStatus = useMemo<WindowStatus>(() => {
    if (!loadedMark) return { editable: false, type: 'info', title: '' };
    if (!loadedMark.marksReleased) {
      return {
        editable: true,
        type: 'info',
        title: 'Marks aren’t released yet, so there’s no time limit.',
        detail: `Once they are released you’ll have ${EDIT_WINDOW_HOURS} hours to make changes.`,
      };
    }
    const deadline = loadedMark.feedbackEditableUntil ? dayjs(loadedMark.feedbackEditableUntil) : null;
    const remaining = deadline ? deadline.valueOf() - now : 0;
    if (deadline && remaining > 0) {
      return {
        editable: true,
        type: remaining < WARN_BELOW_HOURS * 3_600_000 ? 'warning' : 'info',
        title: `You can edit this feedback for another ${formatRemaining(remaining)}.`,
        detail: `Editing locks on ${formatWhen(deadline.toDate())}, ${EDIT_WINDOW_HOURS} hours after the marks were released.`,
      };
    }
    return {
      editable: false,
      type: 'warning',
      title: 'Feedback is locked.',
      detail: deadline
        ? `The ${EDIT_WINDOW_HOURS}-hour edit window closed on ${formatWhen(deadline.toDate())}. Contact an administrator if a correction is needed.`
        : 'The edit window for this assessment has closed. Contact an administrator if a correction is needed.',
    };
  }, [loadedMark, now]);

  const history = useMemo<IFeedbackEditEntry[]>(() => {
    if (!loadedMark?.feedbackHistory) return [];
    try {
      const parsed = JSON.parse(loadedMark.feedbackHistory);
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }, [loadedMark]);

  const trimmedLength = feedback.trim().length;
  const tooShort = trimmedLength > 0 && trimmedLength < MIN_FEEDBACK;
  const unchanged = feedback.trim() === (loadedMark?.feedback ?? '').trim();

  const handleChange = (value: string) => {
    setFeedback(value);
    setValidationError(null);
    setServerError(null);
  };

  // Formatting helpers (US-TCH-010: line breaks and paragraphs). Feedback is
  // plain text, so these only insert characters at the cursor.
  const editAtCursor = (edit: (value: string, start: number, end: number) => { value: string; cursor: number }) => {
    const el = textRef.current?.resizableTextArea?.textArea;
    const start = el?.selectionStart ?? feedback.length;
    const end = el?.selectionEnd ?? feedback.length;
    const next = edit(feedback, start, end);
    if (next.value.length > MAX_FEEDBACK) return;
    handleChange(next.value);
    requestAnimationFrame(() => {
      el?.focus();
      el?.setSelectionRange(next.cursor, next.cursor);
    });
  };

  const addBullet = () =>
    editAtCursor((value, start) => {
      const lineStart = value.lastIndexOf('\n', start - 1) + 1;
      if (value.startsWith(BULLET, lineStart)) return { value, cursor: start };
      return {
        value: value.slice(0, lineStart) + BULLET + value.slice(lineStart),
        cursor: start + BULLET.length,
      };
    });

  const addParagraph = () =>
    editAtCursor((value, start, end) => ({
      value: `${value.slice(0, start)}\n\n${value.slice(end)}`,
      cursor: start + 2,
    }));

  // Strip trailing spaces and collapse runs of blank lines to one.
  const tidySpacing = () =>
    handleChange(
      feedback
        .split('\n')
        .map((line) => line.replace(/[ \t]+$/, ''))
        .join('\n')
        .replace(/\n{3,}/g, '\n\n')
        .trim()
    );

  const handleSubmit = async () => {
    if (!markId || !windowStatus.editable) return;

    const result = feedbackSchema.safeParse({ feedback });
    if (!result.success) {
      setValidationError(result.error.issues[0]?.message ?? 'Feedback is not valid.');
      return;
    }

    setSubmitting(true);
    try {
      await updateFeedbackAsync(markId, { feedback: result.data.feedback });
      message.success(result.data.feedback ? 'Feedback saved' : 'Feedback removed');
      onClose(true);
    } catch (error) {
      const response = axios.isAxiosError(error) ? error.response : undefined;
      if (axios.isAxiosError(error) && !response) {
        setServerError({
          title: 'Feedback could not be saved',
          detail: 'The server could not be reached. Check your connection and try again.',
        });
      } else {
        const abpError = response?.data?.error;
        const code: string | undefined = response?.headers?.['x-error-code'] ?? abpError?.message;
        // Model validation (e.g. the 2000 cap) lists its reasons separately.
        const validation = (abpError?.validationErrors ?? [])
          .map((v: { message: string }) => v.message)
          .join(' ');
        setServerError({
          title: (code && FEEDBACK_ERRORS[code]) || 'Feedback could not be saved',
          detail: validation || abpError?.message || 'Please try again.',
        });
        // The window closed between opening and saving: reload so the modal
        // shows the locked state.
        if (code === 'ASM_FEEDBACK_EDIT_WINDOW_EXPIRED') getAsync(markId);
      }
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
      okButtonProps={{ disabled: !windowStatus.editable || isPending || unchanged || tooShort }}
      destroyOnHidden
      width={640}
    >
      {isPending && !loadedMark ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: 32 }}>
          <Spin />
        </div>
      ) : (
        <Space direction="vertical" style={{ width: '100%' }} size={12}>
          <Text type="secondary">
            Detailed written feedback for the student ({MIN_FEEDBACK}&ndash;{MAX_FEEDBACK} characters).
            This is separate from the short report comment on the mark sheet.
          </Text>

          <Alert
            type={windowStatus.type}
            showIcon
            icon={windowStatus.editable ? <ClockCircleOutlined /> : <LockOutlined />}
            message={windowStatus.title}
            description={windowStatus.detail}
          />

          {serverError && (
            <Alert
              type="error"
              showIcon
              role="alert"
              message={serverError.title}
              description={serverError.detail}
            />
          )}

          <div>
            <Space size={4} style={{ marginBottom: 6 }}>
              <Tooltip title="Start the current line with a bullet">
                <Button
                  size="small"
                  icon={<UnorderedListOutlined />}
                  onClick={addBullet}
                  disabled={!windowStatus.editable}
                >
                  Bullet
                </Button>
              </Tooltip>
              <Tooltip title="Insert a blank line at the cursor">
                <Button
                  size="small"
                  icon={<EnterOutlined />}
                  onClick={addParagraph}
                  disabled={!windowStatus.editable}
                >
                  New paragraph
                </Button>
              </Tooltip>
              <Tooltip title="Remove trailing spaces and extra blank lines">
                <Button
                  size="small"
                  icon={<ClearOutlined />}
                  onClick={tidySpacing}
                  disabled={!windowStatus.editable || !feedback}
                >
                  Tidy spacing
                </Button>
              </Tooltip>
            </Space>

            <Input.TextArea
              ref={textRef}
              value={feedback}
              onChange={(e) => handleChange(e.target.value)}
              autoSize={{ minRows: 6, maxRows: 14 }}
              maxLength={MAX_FEEDBACK}
              status={validationError || tooShort ? 'error' : undefined}
              showCount={{
                formatter: ({ count, maxLength }) =>
                  tooShort ? `${count} / ${maxLength} · at least ${MIN_FEEDBACK}` : `${count} / ${maxLength}`,
              }}
              disabled={!windowStatus.editable}
              placeholder="What the student did well, and what to work on next…"
              aria-label="Feedback"
            />
            {(validationError || tooShort) && (
              <Text type="danger" style={{ display: 'block', marginTop: 4 }}>
                {validationError ?? `${MIN_FEEDBACK - trimmedLength} more character${MIN_FEEDBACK - trimmedLength === 1 ? '' : 's'} needed.`}
              </Text>
            )}
          </div>

          {history.length > 0 && (
            <div style={{ marginTop: 8 }}>
              <Text strong>
                <HistoryOutlined /> Edit history ({history.length})
              </Text>
              <Text type="secondary" style={{ display: 'block', fontSize: 12 }}>
                Earlier versions, newest first. Each shows the text as it was before it was replaced.
              </Text>
              <List
                size="small"
                dataSource={history.map((entry, i) => ({ ...entry, version: i + 1 })).reverse()}
                renderItem={(entry) => (
                  <List.Item>
                    <Space direction="vertical" size={2} style={{ width: '100%' }}>
                      <Space size={8}>
                        <Tag style={{ margin: 0 }}>Version {entry.version}</Tag>
                        <Text type="secondary" style={{ fontSize: 12 }}>
                          {entry.at ? `Replaced ${formatWhen(entry.at)}` : 'Replaced earlier'}
                        </Text>
                      </Space>
                      <Paragraph type="secondary" style={{ margin: 0, whiteSpace: 'pre-wrap' }}>
                        {entry.previous}
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
