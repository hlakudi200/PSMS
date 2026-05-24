'use client';

import { useEffect, useMemo, useRef, useState } from 'react';
import {
  Alert,
  Col,
  DatePicker,
  Form,
  Input,
  InputNumber,
  Modal,
  Row,
  Select,
  Space,
  Typography,
  message,
} from 'antd';
import { z } from 'zod';
import dayjs, { Dayjs } from 'dayjs';
import {
  useOnlineLessonActions,
  useOnlineLessonState,
} from '@/providers/learning/online_lessons';
import type {
  IClassSubjectList,
} from '@/providers/academic/shared/interfaces';
import type {
  IOnlineLessonList,
} from '@/providers/learning/shared/interfaces';
import { ACTIVE_LESSON_STATUSES } from '@/providers/learning/shared/online-lesson-status';

const { Text } = Typography;

// OL-001 scheduling constants, kept in sync with the backend
// OnlineLessonAppService constants of the same name. Duplicating these
// here is deliberate — the client *must* mirror the rules so we can show
// inline errors before a round trip; the backend remains the source of
// truth and re-validates on submit.
const MIN_ADVANCE_HOURS = 24;
const MIN_DURATION_MINUTES = 30;
const MAX_DURATION_MINUTES = 180;
const SCHOOL_START_HOUR = 7; // 07:00 SAST
const SCHOOL_END_HOUR = 17; // 17:00 SAST
const SAST_OFFSET_MINUTES = 2 * 60; // UTC+02:00, no DST in South Africa

// Backend enum psms.Domain.Shared.Enums.OnlinePlatform.
const PLATFORM_OPTIONS = [
  { value: 1, label: 'Zoom' },
  { value: 2, label: 'Microsoft Teams' },
  { value: 3, label: 'Google Meet' },
  { value: 4, label: 'BigBlueButton' },
  { value: 5, label: 'WebEx' },
  { value: 6, label: 'Other / Custom' },
];

const scheduleLessonSchema = z.object({
  classSubjectId: z
    .string()
    .min(1, 'Class & subject is required.'),
  title: z
    .string()
    .min(3, 'Title must be at least 3 characters.')
    .max(200, 'Title must be 200 characters or fewer.'),
  description: z
    .string()
    .max(2000, 'Description must be 2000 characters or fewer.')
    .optional()
    .or(z.literal('')),
  platform: z
    .number({ message: 'Pick a platform.' })
    .int()
    .min(1)
    .max(6),
  meetingLink: z
    .string()
    .min(1, 'Meeting link is required.')
    .url('Meeting link must be a valid URL (https://…).'),
  meetingId: z
    .string()
    .max(100, 'Meeting ID must be 100 characters or fewer.')
    .optional()
    .or(z.literal('')),
  meetingPassword: z
    .string()
    .max(50, 'Meeting password must be 50 characters or fewer.')
    .optional()
    .or(z.literal('')),
  scheduledStart: z
    .custom<Dayjs>((v) => dayjs.isDayjs(v) && (v as Dayjs).isValid(), {
      message: 'Pick a start date and time.',
    })
    .refine((v) => v.isAfter(dayjs()), {
      message: 'Start time must be in the future.',
    }),
  durationMinutes: z
    .number({ message: 'Duration is required.' })
    .int()
    .min(MIN_DURATION_MINUTES, `Duration must be at least ${MIN_DURATION_MINUTES} minutes.`)
    .max(MAX_DURATION_MINUTES, `Duration cannot exceed ${MAX_DURATION_MINUTES} minutes.`),
});

interface ScheduleLessonFormValues {
  classSubjectId?: string;
  title?: string;
  description?: string;
  platform?: number;
  meetingLink?: string;
  meetingId?: string;
  meetingPassword?: string;
  scheduledStart?: Dayjs;
  durationMinutes?: number;
}

/**
 * Returns the {hour, minute, dateKey} of an instant when viewed in SAST.
 * We avoid the dayjs utc plugin (not currently extended) by going through
 * a JS Date with UTC getters after applying the SAST offset.
 */
function sastView(instantMs: number): {
  hourDecimal: number;
  dateKey: string;
} {
  const d = new Date(instantMs + SAST_OFFSET_MINUTES * 60_000);
  const hourDecimal = d.getUTCHours() + d.getUTCMinutes() / 60;
  const yyyy = d.getUTCFullYear();
  const mm = (d.getUTCMonth() + 1).toString().padStart(2, '0');
  const dd = d.getUTCDate().toString().padStart(2, '0');
  return { hourDecimal, dateKey: `${yyyy}-${mm}-${dd}` };
}

interface ConflictResult {
  hasConflict: boolean;
  conflictTitles: string[];
}

function detectConflicts(
  existing: IOnlineLessonList[] | undefined,
  newStartIso: string,
  newEndIso: string
): ConflictResult {
  if (!existing || existing.length === 0) {
    return { hasConflict: false, conflictTitles: [] };
  }
  const newStart = new Date(newStartIso).getTime();
  const newEnd = new Date(newEndIso).getTime();
  const overlapping = existing.filter((ol) => {
    // Only Scheduled/InProgress rows block a slot — Cancelled/Completed
    // cannot be revived in place. Same convention as the backend overlap
    // query in OnlineLessonAppService.ValidateScheduleOrThrowAsync.
    if (!ACTIVE_LESSON_STATUSES.includes(ol.status as typeof ACTIVE_LESSON_STATUSES[number])) {
      return false;
    }
    const otherStart = new Date(ol.scheduledStartTime).getTime();
    const otherEnd = new Date(ol.scheduledEndTime).getTime();
    return otherStart < newEnd && otherEnd > newStart;
  });
  return {
    hasConflict: overlapping.length > 0,
    conflictTitles: overlapping.map((o) => o.title),
  };
}

interface ScheduleLessonModalProps {
  open: boolean;
  onClose: (refresh: boolean) => void;
  classSubjects: IClassSubjectList[];
}

export const ScheduleLessonModal: React.FC<ScheduleLessonModalProps> = ({
  open,
  onClose,
  classSubjects,
}) => {
  const [form] = Form.useForm<ScheduleLessonFormValues>();
  const [submitting, setSubmitting] = useState(false);
  const [zodErrors, setZodErrors] = useState<Record<string, string>>({});
  // Stale-response guard for the conflict pre-check. Rapidly toggling the
  // class-subject Select can land older responses last; we record the
  // class-subject id we asked for and ignore the cached list when it no
  // longer matches the user's current selection.
  const conflictExpectedClassSubjectIdRef = useRef<string | null>(null);

  const { createAsync, getByClassSubjectAsync } = useOnlineLessonActions();
  const {
    lessonsByClassSubject,
    lessonsByClassSubjectPending,
    isPending: createPending,
  } = useOnlineLessonState();

  // Reset on open so a previous attempt doesn't leak in.
  useEffect(() => {
    if (open) {
      form.resetFields();
      setZodErrors({});
      conflictExpectedClassSubjectIdRef.current = null;
    }
  }, [open, form]);

  const classSubjectOptions = useMemo(
    () =>
      classSubjects.map((cs) => ({
        value: cs.id,
        label: `${cs.className ?? 'Class'} — ${cs.subjectName ?? 'Subject'}`,
      })),
    [classSubjects]
  );

  // Conflict pre-check: when the user picks a class-subject, fetch its
  // existing lessons up-front so we can flag overlap before they submit.
  // Re-fetched whenever the class-subject changes — debouncing isn't
  // needed because the select is the only trigger.
  const watchedClassSubjectId = Form.useWatch('classSubjectId', form);
  const watchedStart = Form.useWatch('scheduledStart', form);
  const watchedDuration = Form.useWatch('durationMinutes', form);

  useEffect(() => {
    if (open && watchedClassSubjectId) {
      // Remember the class-subject we're expecting results for. The
      // provider's reducer doesn't know about request ids, so we filter
      // stale results client-side: if the user switches before the
      // response lands, the `lessonsByClassSubject` cache (which the
      // provider unconditionally writes) won't match our ref and the
      // conflict-detection memo treats it as empty.
      conflictExpectedClassSubjectIdRef.current = watchedClassSubjectId;
      getByClassSubjectAsync(watchedClassSubjectId);
    } else if (!watchedClassSubjectId) {
      conflictExpectedClassSubjectIdRef.current = null;
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, watchedClassSubjectId]);

  // Live conflict detection — recomputes only when start, duration, or
  // the fetched lesson list changes. The pre-check is a courtesy: the
  // server re-validates on submit so a stale list can't bypass OL-001.
  const conflict = useMemo<ConflictResult>(() => {
    if (!watchedStart || !watchedDuration || !dayjs.isDayjs(watchedStart)) {
      return { hasConflict: false, conflictTitles: [] };
    }
    // Drop stale responses: only trust the cached lesson list when the
    // user is still asking about the class-subject we fetched for.
    if (
      conflictExpectedClassSubjectIdRef.current !== watchedClassSubjectId
    ) {
      return { hasConflict: false, conflictTitles: [] };
    }
    const endIso = watchedStart.add(watchedDuration, 'minute').toISOString();
    return detectConflicts(
      lessonsByClassSubject,
      watchedStart.toISOString(),
      endIso
    );
  }, [
    lessonsByClassSubject,
    watchedStart,
    watchedDuration,
    watchedClassSubjectId,
  ]);

  const inlineSchoolHoursWarning = useMemo<string | null>(() => {
    if (!watchedStart || !watchedDuration || !dayjs.isDayjs(watchedStart)) {
      return null;
    }
    const start = watchedStart as Dayjs;
    const end = start.add(watchedDuration, 'minute');
    const startSast = sastView(start.valueOf());
    const endSast = sastView(end.valueOf());
    if (startSast.dateKey !== endSast.dateKey) {
      return 'Lesson must start and end on the same calendar day (SAST).';
    }
    if (
      startSast.hourDecimal < SCHOOL_START_HOUR ||
      endSast.hourDecimal > SCHOOL_END_HOUR
    ) {
      return `Lesson must fall within school hours (${SCHOOL_START_HOUR
        .toString()
        .padStart(2, '0')}:00–${SCHOOL_END_HOUR.toString().padStart(2, '0')}:00 SAST).`;
    }
    return null;
  }, [watchedStart, watchedDuration]);

  const inlineAdvanceWarning = useMemo<string | null>(() => {
    if (!watchedStart || !dayjs.isDayjs(watchedStart)) return null;
    const threshold = dayjs().add(MIN_ADVANCE_HOURS, 'hour');
    if (watchedStart.isBefore(threshold)) {
      return `Lessons must be scheduled at least ${MIN_ADVANCE_HOURS} hours in advance.`;
    }
    return null;
  }, [watchedStart]);

  const handleSubmit = async () => {
    let values: ScheduleLessonFormValues;
    try {
      values = await form.validateFields();
    } catch {
      return; // AntD shows per-field errors
    }
    // Re-validate with Zod (covers cross-field rules like duration bounds
    // and the URL shape that the Form.Item rules alone don't enforce).
    const result = scheduleLessonSchema.safeParse(values);
    if (!result.success) {
      const errors: Record<string, string> = {};
      result.error.issues.forEach((issue) => {
        const field = issue.path.join('.');
        if (!errors[field]) errors[field] = issue.message;
      });
      setZodErrors(errors);
      message.error('Please fix the highlighted fields.');
      return;
    }
    setZodErrors({});

    if (inlineAdvanceWarning) {
      message.error(inlineAdvanceWarning);
      return;
    }
    if (inlineSchoolHoursWarning) {
      message.error(inlineSchoolHoursWarning);
      return;
    }
    if (conflict.hasConflict) {
      // Confirm-don't-block: server will reject anyway, but the user has
      // been warned so they can deliberately retry once they've moved the
      // other lesson. We bail here.
      message.error('This slot overlaps an existing lesson — pick a different time.');
      return;
    }

    const start = result.data.scheduledStart;
    const end = start.add(result.data.durationMinutes, 'minute');

    setSubmitting(true);
    try {
      await createAsync({
        classSubjectId: result.data.classSubjectId,
        title: result.data.title.trim(),
        description: result.data.description?.trim() || undefined,
        platform: result.data.platform,
        meetingLink: result.data.meetingLink.trim(),
        meetingId: result.data.meetingId?.trim() || undefined,
        meetingPassword: result.data.meetingPassword?.trim() || undefined,
        scheduledStartTime: start.toISOString(),
        scheduledEndTime: end.toISOString(),
        isRecurring: false,
      });
      message.success('Lesson scheduled');
      onClose(true);
    } catch {
      // axios interceptor surfaces the server message — including the
      // OL-001 codes we may not have caught client-side.
    } finally {
      setSubmitting(false);
    }
  };

  const disabledDate = (current: Dayjs) => {
    // Disallow days before "today in SAST" — a user in another timezone
    // browsing late at night could otherwise see their local "today"
    // greyed out even though SAST is already tomorrow (or vice versa).
    // We work via the same offset arithmetic as the school-hours check.
    if (!current) return false;
    const nowSastDateKey = sastView(Date.now()).dateKey;
    const currentSastDateKey = sastView(current.valueOf()).dateKey;
    return currentSastDateKey < nowSastDateKey;
  };

  return (
    <Modal
      open={open}
      title={
        <Space>
          <span>Schedule online lesson</span>
        </Space>
      }
      onCancel={() => onClose(false)}
      onOk={handleSubmit}
      okText="Schedule"
      confirmLoading={submitting || createPending}
      destroyOnHidden
      width={680}
    >
      <Text type="secondary" style={{ display: 'block', marginBottom: 12 }}>
        Lessons must start at least {MIN_ADVANCE_HOURS} hours from now,
        within school hours ({SCHOOL_START_HOUR.toString().padStart(2, '0')}
        :00–{SCHOOL_END_HOUR.toString().padStart(2, '0')}:00 SAST), and
        between {MIN_DURATION_MINUTES} and {MAX_DURATION_MINUTES} minutes
        long (OL-001).
      </Text>

      <Form<ScheduleLessonFormValues>
        form={form}
        layout="vertical"
        initialValues={{ durationMinutes: 60, platform: 1 }}
      >
        <Row gutter={12}>
          <Col xs={24} md={12}>
            <Form.Item
              label="Class & subject"
              name="classSubjectId"
              rules={[{ required: true, message: 'Pick a class & subject.' }]}
              validateStatus={zodErrors.classSubjectId ? 'error' : undefined}
              help={zodErrors.classSubjectId}
            >
              <Select
                placeholder="Pick a class & subject"
                showSearch
                optionFilterProp="label"
                options={classSubjectOptions}
              />
            </Form.Item>
          </Col>
          <Col xs={24} md={12}>
            <Form.Item
              label="Platform"
              name="platform"
              rules={[{ required: true, message: 'Pick a platform.' }]}
              validateStatus={zodErrors.platform ? 'error' : undefined}
              help={zodErrors.platform}
            >
              <Select options={PLATFORM_OPTIONS} />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item
          label="Title"
          name="title"
          rules={[
            { required: true, message: 'Title is required.' },
            { max: 200, message: 'Title must be 200 characters or fewer.' },
          ]}
          validateStatus={zodErrors.title ? 'error' : undefined}
          help={zodErrors.title}
        >
          <Input placeholder="e.g. Chapter 4 — Photosynthesis review" />
        </Form.Item>

        <Form.Item
          label="Description"
          name="description"
          validateStatus={zodErrors.description ? 'error' : undefined}
          help={zodErrors.description}
        >
          <Input.TextArea
            rows={3}
            placeholder="Optional notes shown to students."
            maxLength={2000}
            showCount
          />
        </Form.Item>

        <Row gutter={12}>
          <Col xs={24} md={14}>
            <Form.Item
              label="Start (your local time)"
              name="scheduledStart"
              rules={[{ required: true, message: 'Pick a start date and time.' }]}
              validateStatus={zodErrors.scheduledStart ? 'error' : undefined}
              help={zodErrors.scheduledStart}
            >
              <DatePicker
                showTime={{ format: 'HH:mm', minuteStep: 5 }}
                format="YYYY-MM-DD HH:mm"
                disabledDate={disabledDate}
                style={{ width: '100%' }}
              />
            </Form.Item>
          </Col>
          <Col xs={24} md={10}>
            <Form.Item
              label="Duration (minutes)"
              name="durationMinutes"
              rules={[
                { required: true, message: 'Duration is required.' },
                {
                  type: 'number',
                  min: MIN_DURATION_MINUTES,
                  max: MAX_DURATION_MINUTES,
                  message: `Duration must be ${MIN_DURATION_MINUTES}–${MAX_DURATION_MINUTES} minutes.`,
                },
              ]}
              validateStatus={zodErrors.durationMinutes ? 'error' : undefined}
              help={zodErrors.durationMinutes}
            >
              <InputNumber
                min={MIN_DURATION_MINUTES}
                max={MAX_DURATION_MINUTES}
                step={5}
                style={{ width: '100%' }}
              />
            </Form.Item>
          </Col>
        </Row>

        {inlineAdvanceWarning && (
          <Alert
            // role="alert" makes screen readers announce the change when
            // the user picks a date/time that violates OL-001.
            role="alert"
            type="warning"
            showIcon
            message={inlineAdvanceWarning}
            style={{ marginBottom: 12 }}
          />
        )}

        {inlineSchoolHoursWarning && (
          <Alert
            role="alert"
            type="warning"
            showIcon
            message={inlineSchoolHoursWarning}
            style={{ marginBottom: 12 }}
          />
        )}

        {/* Conflict warning is its own alert so a user with both a school-
            hours problem AND a conflict sees both. */}
        {watchedClassSubjectId && conflict.hasConflict && (
          <Alert
            role="alert"
            type="warning"
            showIcon
            message="This slot overlaps an existing lesson"
            description={
              <span>
                Conflicts with:{' '}
                <Text strong>{conflict.conflictTitles.join(', ')}</Text>
              </span>
            }
            style={{ marginBottom: 12 }}
          />
        )}
        {watchedClassSubjectId && lessonsByClassSubjectPending && (
          <Text type="secondary" style={{ fontSize: 12 }}>
            Checking for conflicts…
          </Text>
        )}

        <Form.Item
          label="Meeting link"
          name="meetingLink"
          rules={[
            { required: true, message: 'Meeting link is required.' },
            { type: 'url', message: 'Must be a valid URL.' },
          ]}
          validateStatus={zodErrors.meetingLink ? 'error' : undefined}
          help={zodErrors.meetingLink}
        >
          <Input placeholder="https://zoom.us/j/123456789" />
        </Form.Item>

        <Row gutter={12}>
          <Col xs={24} md={12}>
            <Form.Item
              label="Meeting ID (optional)"
              name="meetingId"
              validateStatus={zodErrors.meetingId ? 'error' : undefined}
              help={zodErrors.meetingId}
            >
              <Input placeholder="123-456-789" />
            </Form.Item>
          </Col>
          <Col xs={24} md={12}>
            <Form.Item
              label="Meeting password (optional)"
              name="meetingPassword"
              validateStatus={zodErrors.meetingPassword ? 'error' : undefined}
              help={zodErrors.meetingPassword}
            >
              <Input placeholder="Optional passcode" />
            </Form.Item>
          </Col>
        </Row>
      </Form>
    </Modal>
  );
};
