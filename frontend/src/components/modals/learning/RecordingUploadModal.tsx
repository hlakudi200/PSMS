'use client';

import { useEffect, useState } from 'react';
import {
  Alert,
  Modal,
  Space,
  Typography,
  Upload,
  message,
} from 'antd';
import type { UploadFile } from 'antd';
import { CloudUploadOutlined } from '@ant-design/icons';
import { z } from 'zod';
import { useOnlineLessonActions } from '@/providers/learning/online_lessons';
import type { IOnlineLesson } from '@/providers/learning/shared/interfaces';
import { ONLINE_LESSON_STATUS } from '@/providers/learning/shared/online-lesson-status';

const { Text } = Typography;

// OL-003 — keep these in sync with backend RecordingFileExtensions and
// MaxRecordingFileBytes in OnlineLessonAppService.cs.
// TODO(config): surface via /api/.../config so the constants can't drift.
const ALLOWED_EXTENSIONS = ['.mp4', '.mov', '.avi', '.webm'] as const;
const MAX_RECORDING_SIZE_BYTES = 5 * 1024 * 1024 * 1024; // 5 GB
const ACCEPT_ATTR = ALLOWED_EXTENSIONS.join(',');

const uploadRecordingSchema = z.object({
  // Belt-and-braces UUID check on lessonId so a malformed route param
  // surfaces with a clean error message instead of round-tripping a
  // garbage value to the backend.
  lessonId: z.string().uuid({ message: 'Invalid lesson id.' }),
  file: z
    .custom<File>((v) => v instanceof File, { message: 'Pick a file.' })
    .refine(
      (f) => {
        const ext = f.name
          .slice(f.name.lastIndexOf('.'))
          .toLowerCase();
        return (ALLOWED_EXTENSIONS as readonly string[]).includes(ext);
      },
      { message: `File must be one of: ${ALLOWED_EXTENSIONS.join(', ')}.` }
    )
    .refine((f) => f.size <= MAX_RECORDING_SIZE_BYTES, {
      message: `File exceeds the 5 GB cap.`,
    }),
});

// Soft check: the ticket asks teachers to name files like
// "Subject-Grade-YYYY-MM-DD-Topic.ext". We don't *block* on this — a
// strict regex would frustrate edge cases like multi-word subjects or
// hyphenated topics — but we warn so the convention is visible.
// Uses lazy `.+?` (not `[^-]+`) so the Subject/Grade segments may
// themselves contain hyphens — the engine backtracks until the date
// chunk matches. e.g. "Math-Grade-9-2026-05-24-Algebra.mp4" passes.
const RECOMMENDED_NAME_RE = /^.+?-.+?-\d{4}-\d{2}-\d{2}-.+\.(mp4|mov|avi|webm)$/i;

function formatBytes(value: number): string {
  if (!value) return '0 B';
  const units = ['B', 'KB', 'MB', 'GB'];
  let v = value;
  let i = 0;
  while (v >= 1024 && i < units.length - 1) {
    v /= 1024;
    i++;
  }
  return `${v.toFixed(v < 10 ? 1 : 0)} ${units[i]}`;
}

interface RecordingUploadModalProps {
  open: boolean;
  onClose: (refresh: boolean) => void;
  lesson: IOnlineLesson | null;
}

export const RecordingUploadModal: React.FC<RecordingUploadModalProps> = ({
  open,
  onClose,
  lesson,
}) => {
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [submitting, setSubmitting] = useState(false);
  const [zodError, setZodError] = useState<string | null>(null);

  const { uploadRecordingAsync } = useOnlineLessonActions();
  // Intentionally NOT reading `isPending` from provider state here. That
  // flag is provider-wide; binding confirmLoading to it would spin the
  // button during unrelated refetches (e.g. the host shell's getAsync
  // after upload success). The local `submitting` flag is enough.

  useEffect(() => {
    if (open) {
      setFileList([]);
      setZodError(null);
    }
  }, [open]);

  const handleSubmit = async () => {
    if (!lesson) return;
    const file = fileList[0]?.originFileObj as File | undefined;
    if (!file) {
      setZodError('Pick a file.');
      return;
    }

    const result = uploadRecordingSchema.safeParse({
      lessonId: lesson.id,
      file,
    });
    if (!result.success) {
      const msg = result.error.issues[0]?.message ?? 'Invalid file.';
      setZodError(msg);
      return;
    }
    setZodError(null);

    setSubmitting(true);
    try {
      await uploadRecordingAsync({ lessonId: lesson.id, file });
      message.success('Recording uploaded');
      onClose(true);
    } catch {
      // Server errors (415 type, 413 size, virus scan rejection) are
      // surfaced by the axios response interceptor's message bubble.
    } finally {
      setSubmitting(false);
    }
  };

  // OL-003 acceptance criterion: only Completed lessons accept a
  // recording. The host shell already gates this, but defence-in-depth
  // — show a friendly message if the modal somehow opens on a
  // non-Completed lesson.
  const isCompleted = lesson?.status === ONLINE_LESSON_STATUS.Completed;

  // Soft filename warning — show, don't block.
  const pickedFileName = (fileList[0]?.originFileObj as File | undefined)?.name;
  const filenameWarning =
    pickedFileName != null && !RECOMMENDED_NAME_RE.test(pickedFileName);

  // The ticket asks the user to name the file
  // "Subject-Grade-Date-Topic". We surface that as a hint rather than
  // enforce it programmatically — the convention is meaningful to humans
  // and an over-eager regex would frustrate teachers with edge cases.
  const filenameHint = lesson
    ? `${lesson.subjectName ?? 'Subject'}-${lesson.className ?? 'Grade'}-` +
      `${(lesson.scheduledStartTime ?? '').slice(0, 10)}-Topic.mp4`
    : 'Subject-Grade-Date-Topic.mp4';

  return (
    <Modal
      open={open}
      title={
        <Space>
          <CloudUploadOutlined />
          <span>Upload recording</span>
        </Space>
      }
      okText="Upload"
      okButtonProps={{ disabled: !isCompleted || fileList.length === 0 }}
      confirmLoading={submitting}
      onOk={handleSubmit}
      onCancel={() => onClose(false)}
      destroyOnHidden
      width={560}
    >
      {!isCompleted && (
        <Alert
          type="warning"
          role="alert"
          showIcon
          message="This lesson is not yet completed."
          description="End the lesson before uploading a recording."
          style={{ marginBottom: 12 }}
        />
      )}

      <Text type="secondary" style={{ display: 'block', marginBottom: 8 }}>
        Suggested filename (Subject-Grade-Date-Topic):
      </Text>
      <Text code style={{ display: 'block', marginBottom: 12 }}>
        {filenameHint}
      </Text>

      <Upload.Dragger
        multiple={false}
        maxCount={1}
        fileList={fileList}
        // `accept` is advisory only. Safari / iOS pickers honour UTI
        // over extension and may grey out .webm even though Zod will
        // accept it on submit — Zod is authoritative.
        accept={ACCEPT_ATTR}
        // beforeUpload returns false so AntD doesn't auto-upload; we
        // forward the originFileObj to uploadRecordingAsync ourselves.
        beforeUpload={() => false}
        onChange={(info) => {
          setFileList(info.fileList);
          setZodError(null);
        }}
        onRemove={() => setFileList([])}
        style={{ marginBottom: 12 }}
      >
        <p className="ant-upload-drag-icon">
          <CloudUploadOutlined />
        </p>
        <p className="ant-upload-text">
          Click or drag a recording file here
        </p>
        <p className="ant-upload-hint" style={{ fontSize: 12 }}>
          Accepted: {ALLOWED_EXTENSIONS.join(', ')}. Server cap:{' '}
          {formatBytes(MAX_RECORDING_SIZE_BYTES)}.
        </p>
      </Upload.Dragger>

      {zodError && (
        <Alert
          type="error"
          role="alert"
          showIcon
          message={zodError}
          style={{ marginBottom: 8 }}
        />
      )}

      {filenameWarning && (
        <Alert
          type="info"
          role="status"
          showIcon
          message="Filename does not match the recommended pattern."
          description="Recommended: Subject-Grade-YYYY-MM-DD-Topic.ext. You can still upload — this is a soft check."
          style={{ marginBottom: 8 }}
        />
      )}

      <Text type="secondary" style={{ fontSize: 12 }}>
        Large files may take several minutes. Keep this tab open until the
        upload completes. {/* TODO(blob-storage): once Azure Blob / S3 +
        AV scanning land, tighten this copy to "the server scans the file
        before saving." See the OnlineLessonAppService TODO. */}
      </Text>
    </Modal>
  );
};
