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
import {
  useLearningMaterialActions,
  useLearningMaterialState,
} from '@/providers/learning/learning_materials';
import type {
  IOnlineLesson,
  IOnlineLessonList,
  IUpdateOnlineLesson,
} from '@/providers/learning/shared/interfaces';
import { ACTIVE_LESSON_STATUSES } from '@/providers/learning/shared/online-lesson-status';

const { Text } = Typography;

// OL-001 scheduling constants, kept in sync with the backend
// OnlineLessonAppService constants of the same name. Duplicating these
// here is deliberate — the client *must* mirror the rules so we can show
// inline errors before a round trip; the backend remains the source of
// truth and re-validates on submit.
// Lessons can be booked for tomorrow (SAST) or later, never for today.
const MIN_DURATION_MINUTES = 30;
const MAX_DURATION_MINUTES = 180;
const SCHOOL_START_HOUR = 7; // 07:00 SAST
const SCHOOL_END_HOUR = 14; // 14:00 SAST
const SAST_OFFSET_MINUTES = 2 * 60; // UTC+02:00, no DST in South Africa

// Backend enum psms.Domain.Shared.Enums.OnlinePlatform.
const PLATFORM_INAPP = 7;
const PLATFORM_OPTIONS = [
  { value: PLATFORM_INAPP, label: 'In-App Live Class (PSMS — recommended)' },
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
    .max(7),
  // Required + valid URL for external platforms; omitted for the in-app
  // (LiveKit) platform, where the join route is derived server-side.
  meetingLink: z
    .string()
    .optional()
    .or(z.literal('')),
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
  // "Must be in the future" is enforced by the no-same-day check at submit,
  // which an edit that keeps the original time is allowed to skip.
  scheduledStart: z
    .custom<Dayjs>((v) => dayjs.isDayjs(v) && (v as Dayjs).isValid(), {
      message: 'Pick a start date and time.',
    }),
  durationMinutes: z
    .number({ message: 'Duration is required.' })
    .int()
    .min(MIN_DURATION_MINUTES, `Duration must be at least ${MIN_DURATION_MINUTES} minutes.`)
    .max(MAX_DURATION_MINUTES, `Duration cannot exceed ${MAX_DURATION_MINUTES} minutes.`),
  materialIds: z.array(z.string()).optional(),
}).superRefine((val, ctx) => {
  // External platforms need a valid meeting URL; in-app classes don't.
  if (val.platform !== PLATFORM_INAPP) {
    const link = val.meetingLink?.trim() ?? '';
    if (!link) {
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ['meetingLink'], message: 'Meeting link is required.' });
    } else if (!/^https?:\/\/.+/i.test(link)) {
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ['meetingLink'], message: 'Meeting link must be a valid URL (https://…).' });
    }
  }
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
  materialIds?: string[];
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
  newEndIso: string,
  excludeLessonId?: string
): ConflictResult {
  if (!existing || existing.length === 0) {
    return { hasConflict: false, conflictTitles: [] };
  }
  const newStart = new Date(newStartIso).getTime();
  const newEnd = new Date(newEndIso).getTime();
  const overlapping = existing.filter((ol) => {
    // A lesson being edited never conflicts with itself.
    if (ol.id === excludeLessonId) return false;
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
  /** When set, the modal edits and reschedules this lesson instead of creating one. */
  lesson?: IOnlineLesson;
}

const sameIds = (a: string[], b: string[]) =>
  a.length === b.length && a.every((id) => b.includes(id));

export const ScheduleLessonModal: React.FC<ScheduleLessonModalProps> = ({
  open,
  onClose,
  classSubjects,
  lesson,
}) => {
  const isEdit = !!lesson;
  const [form] = Form.useForm<ScheduleLessonFormValues>();
  const [submitting, setSubmitting] = useState(false);
  const [zodErrors, setZodErrors] = useState<Record<string, string>>({});
  // Stale-response guard for the conflict pre-check. Rapidly toggling the
  // class-subject Select can land older responses last; we record the
  // class-subject id we asked for and ignore the cached list when it no
  // longer matches the user's current selection.
  const conflictExpectedClassSubjectIdRef = useRef<string | null>(null);

  const { createAsync, updateAsync, rescheduleAsync, getByClassSubjectAsync } =
    useOnlineLessonActions();
  const {
    lessonsByClassSubject,
    lessonsByClassSubjectPending,
    isPending: createPending,
  } = useOnlineLessonState();
  const { getAllAsync: getMaterials } = useLearningMaterialActions();
  const { learningMaterials, isPending: materialsPending } = useLearningMaterialState();

  const originalStart = useMemo(
    () => (lesson ? dayjs(lesson.scheduledStartTime) : undefined),
    [lesson]
  );
  const originalMaterialIds = useMemo(
    () => (lesson?.materials ?? []).map((m) => m.id),
    [lesson]
  );

  // Reset on open so a previous attempt doesn't leak in; prefill in edit mode.
  useEffect(() => {
    if (open) {
      form.resetFields();
      setZodErrors({});
      conflictExpectedClassSubjectIdRef.current = null;
      if (lesson) {
        form.setFieldsValue({
          classSubjectId: lesson.classSubjectId,
          title: lesson.title,
          description: lesson.description ?? '',
          platform: lesson.platform,
          meetingLink: lesson.platform === PLATFORM_INAPP ? undefined : lesson.meetingLink,
          meetingId: lesson.meetingId ?? '',
          meetingPassword: lesson.meetingPassword ?? '',
          scheduledStart: dayjs(lesson.scheduledStartTime),
          durationMinutes: lesson.durationMinutes,
          materialIds: (lesson.materials ?? []).map((m) => m.id),
        });
      }
    }
  }, [open, form, lesson]);

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
  const watchedPlatform = Form.useWatch('platform', form);
  // In-app (LiveKit) live classes are hosted inside PSMS — no external link.
  const isInApp = watchedPlatform === PLATFORM_INAPP;

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
      // Only published materials can be attached; students can't open drafts.
      getMaterials({
        classSubjectId: watchedClassSubjectId,
        isPublished: true,
        maxResultCount: 200,
        sorting: 'Title',
      });
    } else if (!watchedClassSubjectId) {
      conflictExpectedClassSubjectIdRef.current = null;
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, watchedClassSubjectId]);

  // Switching class-subject invalidates the picked materials.
  const handleValuesChange = (changed: Partial<ScheduleLessonFormValues>) => {
    if ('classSubjectId' in changed) {
      form.setFieldValue('materialIds', []);
    }
  };

  const materialOptions = useMemo(() => {
    const options = (learningMaterials ?? [])
      .filter((m) => m.classSubjectId === watchedClassSubjectId)
      .map((m) => ({ value: m.id, label: m.title }));
    // Keep already-attached materials labelled even if they've since been
    // unpublished and so dropped out of the list above.
    (lesson?.materials ?? []).forEach((m) => {
      if (!options.some((o) => o.value === m.id)) {
        options.push({ value: m.id, label: m.title });
      }
    });
    return options;
  }, [learningMaterials, watchedClassSubjectId, lesson]);

  // In edit mode the OL-001 time rules only apply when the time moves; a
  // lesson now less than 24 h away can still have its title fixed.
  const timeChanged = useMemo(() => {
    if (!lesson || !originalStart) return true;
    if (!watchedStart || !dayjs.isDayjs(watchedStart)) return true;
    return (
      !watchedStart.isSame(originalStart, 'minute') ||
      watchedDuration !== lesson.durationMinutes
    );
  }, [lesson, originalStart, watchedStart, watchedDuration]);

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
      endIso,
      lesson?.id
    );
  }, [
    lessonsByClassSubject,
    watchedStart,
    watchedDuration,
    watchedClassSubjectId,
    lesson?.id,
  ]);

  const inlineSchoolHoursWarning = useMemo<string | null>(() => {
    if (!timeChanged) return null;
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
  }, [watchedStart, watchedDuration, timeChanged]);

  const inlineAdvanceWarning = useMemo<string | null>(() => {
    if (!timeChanged) return null;
    if (!watchedStart || !dayjs.isDayjs(watchedStart)) return null;
    const todaySast = sastView(Date.now()).dateKey;
    if (sastView(watchedStart.valueOf()).dateKey <= todaySast) {
      return "Lessons can't be scheduled for today. Pick tomorrow or a later day.";
    }
    return null;
  }, [watchedStart, timeChanged]);

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
    if (timeChanged && conflict.hasConflict) {
      // Confirm-don't-block: server will reject anyway, but the user has
      // been warned so they can deliberately retry once they've moved the
      // other lesson. We bail here.
      message.error('This slot overlaps an existing lesson — pick a different time.');
      return;
    }

    const start = result.data.scheduledStart;
    const end = start.add(result.data.durationMinutes, 'minute');
    const materialIds = result.data.materialIds ?? [];

    if (lesson) {
      await submitEdit(lesson, result.data, start, end, materialIds);
      return;
    }

    setSubmitting(true);
    try {
      await createAsync({
        classSubjectId: result.data.classSubjectId,
        title: result.data.title.trim(),
        description: result.data.description?.trim() || undefined,
        platform: result.data.platform,
        // In-app classes have no external link — the server derives the join
        // route. External platforms send the validated URL + optional creds.
        meetingLink:
          result.data.platform === PLATFORM_INAPP
            ? undefined
            : result.data.meetingLink?.trim(),
        meetingId:
          result.data.platform === PLATFORM_INAPP
            ? undefined
            : result.data.meetingId?.trim() || undefined,
        meetingPassword:
          result.data.platform === PLATFORM_INAPP
            ? undefined
            : result.data.meetingPassword?.trim() || undefined,
        scheduledStartTime: start.toISOString(),
        scheduledEndTime: end.toISOString(),
        isRecurring: false,
        materialIds: materialIds.length > 0 ? materialIds : undefined,
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

  // Edit = an optional Reschedule (which re-runs OL-001 server-side and
  // notifies students) followed by an Update of everything else. The time
  // moves first so a rejected slot leaves the lesson untouched.
  const submitEdit = async (
    current: IOnlineLesson,
    data: z.infer<typeof scheduleLessonSchema>,
    start: Dayjs,
    end: Dayjs,
    materialIds: string[]
  ) => {
    const isInAppLesson = current.platform === PLATFORM_INAPP;
    const update: IUpdateOnlineLesson = {
      title: data.title.trim(),
      // Empty string clears the description; the server treats null as "unchanged".
      description: data.description?.trim() ?? '',
      meetingLink: isInAppLesson ? undefined : data.meetingLink?.trim(),
      meetingId: isInAppLesson ? undefined : data.meetingId?.trim() ?? '',
      meetingPassword: isInAppLesson ? undefined : data.meetingPassword?.trim() ?? '',
      materialIds: sameIds(materialIds, originalMaterialIds) ? undefined : materialIds,
    };

    setSubmitting(true);
    let rescheduled = false;
    try {
      if (timeChanged) {
        await rescheduleAsync(current.id, {
          newStartTime: start.toISOString(),
          newEndTime: end.toISOString(),
        });
        rescheduled = true;
      }
      await updateAsync(current.id, update);
      message.success(
        rescheduled
          ? 'Lesson rescheduled. Enrolled students have been notified.'
          : 'Lesson updated'
      );
      onClose(true);
    } catch {
      // The axios interceptor shows the server message. If the time already
      // moved, say so, so the teacher doesn't retry the reschedule.
      if (rescheduled) {
        message.warning('The new time was saved, but the other changes were not.');
        onClose(true);
      }
    } finally {
      setSubmitting(false);
    }
  };

  const disabledDate = (current: Dayjs) => {
    // Disallow today and earlier, in SAST — a user in another timezone
    // browsing late at night could otherwise see their local "today"
    // greyed out even though SAST is already tomorrow (or vice versa).
    // We work via the same offset arithmetic as the school-hours check.
    if (!current) return false;
    const nowSastDateKey = sastView(Date.now()).dateKey;
    const currentSastDateKey = sastView(current.valueOf()).dateKey;
    return currentSastDateKey <= nowSastDateKey;
  };

  return (
    <Modal
      open={open}
      title={
        <Space>
          <span>{isEdit ? 'Edit online lesson' : 'Schedule online lesson'}</span>
        </Space>
      }
      onCancel={() => onClose(false)}
      onOk={handleSubmit}
      okText={isEdit ? 'Save changes' : 'Schedule'}
      confirmLoading={submitting || createPending}
      destroyOnHidden
      width={680}
    >
      <Text type="secondary" style={{ display: 'block', marginBottom: 12 }}>
        Lessons can be booked for tomorrow or later (not today),
        within school hours ({SCHOOL_START_HOUR.toString().padStart(2, '0')}
        :00–{SCHOOL_END_HOUR.toString().padStart(2, '0')}:00 SAST), and
        between {MIN_DURATION_MINUTES} and {MAX_DURATION_MINUTES} minutes
        long (OL-001).
        {isEdit && ' Changing the time notifies enrolled students.'}
      </Text>

      <Form<ScheduleLessonFormValues>
        form={form}
        layout="vertical"
        initialValues={{ durationMinutes: 60, platform: 1, materialIds: [] }}
        onValuesChange={handleValuesChange}
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
                disabled={isEdit}
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
              <Select options={PLATFORM_OPTIONS} disabled={isEdit} />
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

        <Form.Item
          label="Pre-lesson materials (optional)"
          name="materialIds"
          extra="Published materials for this class and subject, so students can prepare."
          validateStatus={zodErrors.materialIds ? 'error' : undefined}
          help={zodErrors.materialIds}
        >
          <Select
            mode="multiple"
            placeholder={
              watchedClassSubjectId
                ? 'Attach materials students should read first'
                : 'Pick a class & subject first'
            }
            disabled={!watchedClassSubjectId}
            loading={materialsPending}
            optionFilterProp="label"
            options={materialOptions}
            notFoundContent={
              materialsPending ? 'Loading…' : 'No published materials for this class and subject'
            }
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

        {isInApp ? (
          <Alert
            type="info"
            showIcon
            message="In-app live class"
            description="Students and you join the live classroom inside PSMS — no external meeting link needed. You broadcast your camera, mic and screen; students watch, chat and can raise a hand."
            style={{ marginBottom: 12 }}
          />
        ) : (
          <>
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
          </>
        )}
      </Form>
    </Modal>
  );
};
