'use client';

import { useEffect, useMemo, useRef, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import {
  Alert,
  Breadcrumb,
  Button,
  Card,
  Col,
  Descriptions,
  Input,
  InputNumber,
  Modal,
  Popconfirm,
  Result,
  Row,
  Skeleton,
  Space,
  Spin,
  Tag,
  Tooltip,
  Typography,
  message,
} from 'antd';
import {
  ArrowLeftOutlined,
  ClockCircleOutlined,
  CloseCircleOutlined,
  CloudUploadOutlined,
  CopyOutlined,
  LinkOutlined,
  PlayCircleOutlined,
  StopOutlined,
  VideoCameraOutlined,
} from '@ant-design/icons';
import {
  OnlineLessonProvider,
  useOnlineLessonActions,
  useOnlineLessonState,
} from '@/providers/learning/online_lessons';
import { ONLINE_LESSON_STATUS } from '@/providers/learning/shared/online-lesson-status';
import { RecordingUploadModal } from '@/components/modals/learning/RecordingUploadModal';

const { Title, Text, Paragraph } = Typography;

// OL-006 host window — mirrors backend StartLeadWindow in
// OnlineLessonAppService.cs. The button is grey until we're inside the
// window; the server is still authoritative.
// TODO(config): surface this via a /config endpoint so the two constants
// cannot drift; for now keep the duplication tight by referencing the
// backend file in this comment.
const START_LEAD_WINDOW_MINUTES = 15;

// Server-side EndAsync caps attendeeCount at 1000 (see OnlineLessonAppService).
// We cap the input here too so the user gets immediate feedback. OL-002's
// 100-participant rule is a platform constraint, not a PSMS one.
const MAX_ATTENDEE_COUNT = 1000;

const PLATFORM_LABEL: Record<number, string> = {
  1: 'Zoom',
  2: 'Microsoft Teams',
  3: 'Google Meet',
  4: 'BigBlueButton',
  5: 'WebEx',
  6: 'Other',
};

const STATUS_META: Record<number, { label: string; color: string }> = {
  1: { label: 'Scheduled', color: 'blue' },
  2: { label: 'Live', color: 'gold' },
  3: { label: 'Completed', color: 'green' },
  4: { label: 'Cancelled', color: 'red' },
};

function formatWhen(startIso: string, endIso: string): string {
  try {
    const start = new Date(startIso);
    const end = new Date(endIso);
    const date = start.toLocaleDateString('en-ZA', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: '2-digit',
    });
    const t = (d: Date) =>
      d.toLocaleTimeString('en-ZA', { hour: '2-digit', minute: '2-digit' });
    return `${date} · ${t(start)}–${t(end)} SAST`;
  } catch {
    return `${startIso} – ${endIso}`;
  }
}

function isHttpUrl(value: string | null | undefined): value is string {
  return !!value && /^https?:\/\//i.test(value);
}

// Recording URLs may be either same-origin server-minted paths (current
// upload flow returns `/api/online-lesson-recordings/...`) or — for the
// legacy URL-only `AddRecording` flow — an absolute http(s) URL. Both
// shapes are safe to open in a new tab; anything else (javascript:,
// data:) is rejected. Normalises through the URL constructor so
// path-traversal payloads like `/api/../../evil` collapse to `/evil` and
// fail the prefix check.
function isSafeRecordingUrl(value: string | null | undefined): value is string {
  if (!value) return false;
  if (typeof window === 'undefined') {
    // SSR guard — function shouldn't be called there, but stay safe.
    return value.startsWith('/api/') || /^https?:\/\//i.test(value);
  }
  try {
    const parsed = new URL(value, window.location.origin);
    if (parsed.origin === window.location.origin) {
      return parsed.pathname.toLowerCase().startsWith('/api/');
    }
    return parsed.protocol === 'http:' || parsed.protocol === 'https:';
  } catch {
    return false;
  }
}

function TeacherHostLessonContent() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const lessonId = params?.id;

  const { getAsync, startAsync, endAsync, cancelAsync } =
    useOnlineLessonActions();
  const { onlineLesson, isPending, isError } = useOnlineLessonState();

  // We need the per-page record to track the *current* lesson without
  // having to reload on every action — startAsync/endAsync set
  // onlineLesson via the provider, so we re-derive flags below.
  useEffect(() => {
    if (lessonId) getAsync(lessonId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [lessonId]);

  const [endModalOpen, setEndModalOpen] = useState(false);
  const [recordingModalOpen, setRecordingModalOpen] = useState(false);
  const [attendeeCount, setAttendeeCount] = useState<number | null>(null);
  const [actionPending, setActionPending] = useState<
    'start' | 'end' | 'cancel' | null
  >(null);
  // Ref to the meeting-link textarea so we can focus + select it when the
  // clipboard API rejects the copy (some browsers gate clipboard writes
  // outside a user gesture). Lets the teacher still copy manually.
  const meetingLinkTextareaRef = useRef<HTMLTextAreaElement | null>(null);

  // Keep `now` in component state, refreshed every 10 s, so the
  // "minutes until start" + canStart gating recompute promptly without
  // forcing the teacher to reload. 10 s keeps the staleness window short
  // enough that the lead-window button flips on/off perceptibly close to
  // the actual boundary (the server is still authoritative).
  const [now, setNow] = useState<number>(() => Date.now());
  useEffect(() => {
    const handle = setInterval(() => setNow(Date.now()), 10_000);
    return () => clearInterval(handle);
  }, []);

  const status = onlineLesson?.status;
  const scheduledStartMs = onlineLesson?.scheduledStartTime
    ? new Date(onlineLesson.scheduledStartTime).getTime()
    : null;
  const scheduledEndMs = onlineLesson?.scheduledEndTime
    ? new Date(onlineLesson.scheduledEndTime).getTime()
    : null;

  const minutesUntilStart = scheduledStartMs
    ? Math.round((scheduledStartMs - now) / 60_000)
    : null;
  const minutesPastEnd = scheduledEndMs
    ? Math.round((now - scheduledEndMs) / 60_000)
    : null;

  const canStart = useMemo(() => {
    if (status !== ONLINE_LESSON_STATUS.Scheduled) return false;
    if (scheduledStartMs == null || scheduledEndMs == null) return false;
    const earliest = scheduledStartMs - START_LEAD_WINDOW_MINUTES * 60_000;
    return now >= earliest && now < scheduledEndMs;
  }, [status, scheduledStartMs, scheduledEndMs, now]);

  const isLive = status === ONLINE_LESSON_STATUS.InProgress;

  const handleStart = async () => {
    if (!lessonId) return;
    setActionPending('start');
    try {
      await startAsync(lessonId);
      message.success('Lesson started');
      getAsync(lessonId);
    } catch {
      // Surfaced by axios interceptor
    } finally {
      setActionPending(null);
    }
  };

  const handleEnd = async () => {
    if (!lessonId) return;
    if (attendeeCount == null || attendeeCount < 0 || attendeeCount > MAX_ATTENDEE_COUNT) {
      message.error(`Attendee count must be between 0 and ${MAX_ATTENDEE_COUNT}.`);
      return;
    }
    setActionPending('end');
    try {
      await endAsync(lessonId, attendeeCount);
      message.success('Lesson ended');
      setEndModalOpen(false);
      setAttendeeCount(null);
      getAsync(lessonId);
    } catch {
      // Surfaced by axios interceptor
    } finally {
      setActionPending(null);
    }
  };

  const handleCancel = () => {
    if (!lessonId) return;
    Modal.confirm({
      title: 'Cancel this lesson?',
      content:
        'Students will no longer see this lesson on their schedule. This cannot be undone.',
      okText: 'Cancel lesson',
      okButtonProps: { danger: true },
      cancelText: 'Keep',
      onOk: async () => {
        setActionPending('cancel');
        try {
          await cancelAsync(lessonId);
          message.success('Lesson cancelled');
          getAsync(lessonId);
        } catch {
          // Surfaced by axios interceptor
        } finally {
          setActionPending(null);
        }
      },
    });
  };

  const handleCopyLink = async () => {
    const link = onlineLesson?.meetingLink;
    if (!isHttpUrl(link)) {
      message.warning('This lesson has no meeting link set.');
      return;
    }
    try {
      await navigator.clipboard.writeText(link);
      message.success('Meeting link copied');
    } catch {
      // Clipboard API rejected (no permission, no secure context, no
      // user gesture). Focus + select the textarea so Ctrl/Cmd+C works
      // immediately from the keyboard instead of just complaining.
      const el = meetingLinkTextareaRef.current;
      if (el) {
        el.focus();
        el.select();
      }
      message.error('Could not copy automatically — the link is selected, press Ctrl/Cmd+C.');
    }
  };

  const handleOpenMeeting = () => {
    const link = onlineLesson?.meetingLink;
    if (!isHttpUrl(link)) {
      message.error('Unsupported or missing meeting link.');
      return;
    }
    window.open(link, '_blank', 'noopener,noreferrer');
  };

  if (!lessonId) {
    return (
      <Result
        status="404"
        title="Lesson not found"
        subTitle="No lesson id was supplied in the URL."
        extra={
          <Button onClick={() => router.push('/teacher/lessons')}>
            Back to lessons
          </Button>
        }
      />
    );
  }

  if (isPending && !onlineLesson) {
    return (
      <div>
        <Skeleton active paragraph={{ rows: 6 }} />
      </div>
    );
  }

  if (isError && !onlineLesson) {
    return (
      <Result
        status="error"
        title="Could not load lesson"
        subTitle="Refresh the page to try again."
        extra={
          <Button onClick={() => router.push('/teacher/lessons')}>
            Back to lessons
          </Button>
        }
      />
    );
  }

  if (!onlineLesson) {
    return null;
  }

  const statusMeta = STATUS_META[onlineLesson.status] ?? {
    label: 'Unknown',
    color: 'default',
  };

  return (
    <div>
      <Breadcrumb
        style={{ marginBottom: 16 }}
        items={[
          {
            title: (
              <a onClick={() => router.push('/teacher/lessons')}>
                <ArrowLeftOutlined /> Online lessons
              </a>
            ),
          },
          { title: onlineLesson.title },
        ]}
      />

      <Card variant="borderless" style={{ marginBottom: 16 }}>
        <Space
          style={{ width: '100%', justifyContent: 'space-between' }}
          align="start"
          wrap
        >
          <div>
            <Title level={4} style={{ marginBottom: 4 }}>
              {onlineLesson.title}
            </Title>
            <Space>
              <Tag color={statusMeta.color}>{statusMeta.label}</Tag>
              <Text type="secondary">
                {onlineLesson.className ?? 'Class'} ·{' '}
                {onlineLesson.subjectName ?? 'Subject'}
              </Text>
            </Space>
          </div>
          <Space>
            {/* Cancel is available on Scheduled lessons only. End is
                irreversible: confirmed in the modal. We disable *every*
                action button while any one is in flight so the user can't
                fire Cancel and Start back-to-back and race them. */}
            {onlineLesson.status === ONLINE_LESSON_STATUS.Scheduled && (
              <Button
                icon={<CloseCircleOutlined />}
                danger
                onClick={handleCancel}
                loading={actionPending === 'cancel'}
                disabled={actionPending != null && actionPending !== 'cancel'}
                aria-label="Cancel lesson"
              >
                Cancel lesson
              </Button>
            )}
            {onlineLesson.status === ONLINE_LESSON_STATUS.Scheduled && (
              <Tooltip
                title={
                  canStart
                    ? undefined
                    : minutesUntilStart != null && minutesUntilStart > START_LEAD_WINDOW_MINUTES
                    ? `You can start ${START_LEAD_WINDOW_MINUTES} minutes before the scheduled time (${minutesUntilStart - START_LEAD_WINDOW_MINUTES} min from now).`
                    : minutesPastEnd != null && minutesPastEnd > 0
                    ? 'This lesson’s scheduled window has passed.'
                    : 'Not yet startable.'
                }
              >
                <Button
                  type="primary"
                  icon={<PlayCircleOutlined />}
                  disabled={!canStart || (actionPending != null && actionPending !== 'start')}
                  loading={actionPending === 'start'}
                  onClick={handleStart}
                  aria-label="Start lesson"
                >
                  Start lesson
                </Button>
              </Tooltip>
            )}
            {isLive && (
              <Button
                type="primary"
                danger
                icon={<StopOutlined />}
                onClick={() => setEndModalOpen(true)}
                disabled={actionPending != null && actionPending !== 'end'}
                aria-label="End lesson"
              >
                End lesson
              </Button>
            )}
          </Space>
        </Space>
      </Card>

      <Row gutter={[16, 16]}>
        <Col xs={24} lg={14}>
          <Card variant="borderless" title="Lesson details">
            <Descriptions column={1} size="small">
              <Descriptions.Item label="When">
                {formatWhen(
                  onlineLesson.scheduledStartTime,
                  onlineLesson.scheduledEndTime
                )}
              </Descriptions.Item>
              <Descriptions.Item label="Duration">
                <ClockCircleOutlined /> {onlineLesson.durationMinutes} min
              </Descriptions.Item>
              <Descriptions.Item label="Platform">
                {PLATFORM_LABEL[onlineLesson.platform] ?? 'Other'}
              </Descriptions.Item>
              {onlineLesson.meetingId && (
                <Descriptions.Item label="Meeting ID">
                  {onlineLesson.meetingId}
                </Descriptions.Item>
              )}
              {onlineLesson.meetingPassword && (
                <Descriptions.Item label="Meeting password">
                  {onlineLesson.meetingPassword}
                </Descriptions.Item>
              )}
            </Descriptions>
            {onlineLesson.description && (
              <>
                <Title level={5} style={{ marginTop: 16 }}>
                  Description
                </Title>
                <Paragraph style={{ whiteSpace: 'pre-wrap' }}>
                  {onlineLesson.description}
                </Paragraph>
              </>
            )}
          </Card>
        </Col>

        <Col xs={24} lg={10}>
          <Card variant="borderless" title="Meeting link">
            {/* Once a lesson is in progress the host actually needs to
                join via the link. Before that, the link is still useful
                to copy & paste into a calendar invite. */}
            {isHttpUrl(onlineLesson.meetingLink) ? (
              <>
                <Input.TextArea
                  value={onlineLesson.meetingLink}
                  autoSize
                  readOnly
                  style={{ marginBottom: 12, fontFamily: 'monospace' }}
                  aria-label="Meeting link"
                  // AntD TextArea forwards the DOM ref via resizableTextArea,
                  // but the cleanest cross-version approach is to grab the
                  // native textarea via a ref to the wrapper's HTMLElement.
                  ref={(node) => {
                    // AntD v6 TextAreaRef exposes `resizableTextArea` →
                    // `textArea` for the native node. Reach in defensively
                    // so we don't crash if the shape changes.
                    const taRef = node as unknown as
                      | { resizableTextArea?: { textArea?: HTMLTextAreaElement } }
                      | null;
                    meetingLinkTextareaRef.current =
                      taRef?.resizableTextArea?.textArea ?? null;
                  }}
                />
                <Space wrap>
                  <Button icon={<CopyOutlined />} onClick={handleCopyLink}>
                    Copy link
                  </Button>
                  <Button
                    type="primary"
                    icon={<LinkOutlined />}
                    onClick={handleOpenMeeting}
                  >
                    Open meeting
                  </Button>
                </Space>
              </>
            ) : (
              <Alert
                type="warning"
                showIcon
                message="No meeting link"
                description="This lesson was scheduled without a meeting link, or the stored link does not start with http(s)://."
              />
            )}
          </Card>

          {onlineLesson.status === ONLINE_LESSON_STATUS.Scheduled &&
            minutesUntilStart != null && (
              <Alert
                role="status"
                style={{ marginTop: 12 }}
                type={canStart ? 'success' : 'info'}
                showIcon
                message={
                  canStart
                    ? 'You can start this lesson now.'
                    : minutesUntilStart > 0
                    ? `Starts in ${minutesUntilStart} minute${
                        minutesUntilStart === 1 ? '' : 's'
                      }.`
                    : `Scheduled start was ${Math.abs(minutesUntilStart)} minute${
                        Math.abs(minutesUntilStart) === 1 ? '' : 's'
                      } ago.`
                }
                description={
                  canStart
                    ? `You're within the ${START_LEAD_WINDOW_MINUTES}-minute lead window.`
                    : undefined
                }
              />
            )}

          {isLive && (
            <Alert
              role="status"
              style={{ marginTop: 12 }}
              type="warning"
              showIcon
              message="This lesson is currently live."
              description={
                <>
                  Ending the lesson is irreversible. You will be asked to
                  enter the attendee count.
                  <br />
                  <Text type="secondary" style={{ fontSize: 12 }}>
                    Live video embedding is not in scope of this build —
                    join via the platform link above.
                  </Text>
                </>
              }
            />
          )}

          {onlineLesson.status === ONLINE_LESSON_STATUS.Completed && (
            <Alert
              role="status"
              style={{ marginTop: 12 }}
              type="success"
              showIcon
              message="Lesson completed."
              description={
                onlineLesson.attendeeCount != null
                  ? `${onlineLesson.attendeeCount} attendee${
                      onlineLesson.attendeeCount === 1 ? '' : 's'
                    } recorded.`
                  : 'Attendee count was not recorded.'
              }
            />
          )}

          {/* Recording upload — only meaningful on Completed lessons per
              OL-003. We show the upload button OR the playback card based
              on whether a recording is already attached. */}
          {onlineLesson.status === ONLINE_LESSON_STATUS.Completed && (
            <Card
              variant="borderless"
              title={
                <Space>
                  <VideoCameraOutlined />
                  <span>Recording</span>
                </Space>
              }
              style={{ marginTop: 12 }}
            >
              {onlineLesson.hasRecording && onlineLesson.recordingUrl ? (
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Text type="secondary">A recording is attached to this lesson.</Text>
                  <Button
                    type="primary"
                    icon={<LinkOutlined />}
                    onClick={() => {
                      const url = onlineLesson.recordingUrl;
                      if (isSafeRecordingUrl(url)) {
                        window.open(url, '_blank', 'noopener,noreferrer');
                      } else {
                        message.error('Unsupported or unsafe recording link.');
                      }
                    }}
                  >
                    Open recording
                  </Button>
                  <Popconfirm
                    title="Replace the existing recording?"
                    description="The current recording link will be unlinked. Until blob-storage cleanup ships, the previous file remains on the server."
                    okText="Replace"
                    onConfirm={() => setRecordingModalOpen(true)}
                  >
                    <Button icon={<CloudUploadOutlined />}>
                      Replace recording
                    </Button>
                  </Popconfirm>
                </Space>
              ) : (
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Text type="secondary">
                    No recording attached yet. Upload an MP4 / MOV / AVI /
                    WebM file up to 5 GB.
                  </Text>
                  <Button
                    type="primary"
                    icon={<CloudUploadOutlined />}
                    onClick={() => setRecordingModalOpen(true)}
                    aria-label="Upload recording"
                  >
                    Upload recording
                  </Button>
                </Space>
              )}
            </Card>
          )}

          {onlineLesson.status === ONLINE_LESSON_STATUS.Cancelled && (
            <Alert
              role="status"
              style={{ marginTop: 12 }}
              type="error"
              showIcon
              message="Lesson cancelled."
            />
          )}
        </Col>
      </Row>

      <Modal
        open={endModalOpen}
        title="End live lesson"
        okText="End lesson"
        okButtonProps={{
          danger: true,
          loading: actionPending === 'end',
        }}
        onOk={handleEnd}
        onCancel={() => {
          setEndModalOpen(false);
          setAttendeeCount(null);
        }}
        destroyOnHidden
      >
        <Paragraph>
          Enter the number of students who attended this lesson. This action
          cannot be undone.
        </Paragraph>
        <InputNumber
          min={0}
          max={MAX_ATTENDEE_COUNT}
          value={attendeeCount ?? undefined}
          onChange={(v) => setAttendeeCount(typeof v === 'number' ? v : null)}
          placeholder="Attendee count"
          style={{ width: '100%' }}
          aria-label="Attendee count"
          autoFocus
        />
      </Modal>

      <RecordingUploadModal
        open={recordingModalOpen}
        lesson={onlineLesson}
        onClose={(refresh) => {
          setRecordingModalOpen(false);
          if (refresh && lessonId) getAsync(lessonId);
        }}
      />

      {/* Tiny indicator at the bottom while a refresh is in flight, so
          the teacher knows the page is doing something after they
          clicked Start / End / Cancel. */}
      {isPending && onlineLesson && (
        <div style={{ marginTop: 12, textAlign: 'center' }}>
          <Spin size="small" /> <Text type="secondary">Refreshing…</Text>
        </div>
      )}
    </div>
  );
}

export default function TeacherHostLessonPageContent() {
  return (
    <OnlineLessonProvider>
      <TeacherHostLessonContent />
    </OnlineLessonProvider>
  );
}
