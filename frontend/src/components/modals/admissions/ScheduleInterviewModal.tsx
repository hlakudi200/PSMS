'use client';

import React, { useEffect, useState } from 'react';
import {
  Alert, DatePicker, Form, Input, Modal, Select, Space, Tag, TimePicker, Typography, message,
} from 'antd';
import dayjs, { Dayjs } from 'dayjs';
import { z } from 'zod';
import { useAdmissionInterviewActions } from '@/providers/admissions/admission_interviews';
import type { IInterviewer } from '@/providers/admissions/shared/interfaces';

const { Text } = Typography;

/** ADM-012: the school gives a family at least this much warning. */
const MINIMUM_NOTICE_DAYS = 2;

const schema = z
  .object({
    interviewerUserId: z.number({ error: 'Choose who will conduct the interview.' }),
    scheduledDate: z.custom<Dayjs>((v) => dayjs.isDayjs(v), 'Choose a date.'),
    scheduledTime: z.custom<Dayjs>((v) => dayjs.isDayjs(v), 'Choose a time.'),
    location: z.string().trim().max(200).optional().or(z.literal('')),
    meetingLink: z.string().trim().max(500).optional().or(z.literal('')),
    notes: z.string().trim().max(2000).optional().or(z.literal('')),
  })
  .refine((v) => !v.scheduledDate || v.scheduledDate.isAfter(dayjs().add(MINIMUM_NOTICE_DAYS, 'day'), 'minute'), {
    path: ['scheduledDate'],
    message: `The family needs at least ${MINIMUM_NOTICE_DAYS} days' notice, so pick a later date.`,
  })
  /* A URL or nothing — the server rejects a malformed one outright, and an
     interview with neither a room nor a link is one nobody can attend. */
  .refine((v) => !v.meetingLink || /^https?:\/\/.+/i.test(v.meetingLink), {
    path: ['meetingLink'],
    message: 'A meeting link has to start with http:// or https://',
  })
  .refine((v) => !!v.location || !!v.meetingLink, {
    path: ['location'],
    message: 'Say where it is — a room, or a meeting link.',
  });

type FormValues = z.infer<typeof schema>;

/**
 * Booking an interview.
 *
 * There was no way to. Schedule, Complete and MarkNoShow all existed on the
 * server and no screen called any of them, so the Interview step of the
 * admissions workflow was a step about something that could not happen.
 */
export default function ScheduleInterviewModal({
  open,
  applicationId,
  applicantName,
  onClose,
  onScheduled,
}: {
  open: boolean;
  applicationId: string;
  applicantName?: string;
  onClose: () => void;
  onScheduled: () => void;
}) {
  const { scheduleAsync, getInterviewersAsync } = useAdmissionInterviewActions();
  const [form] = Form.useForm<FormValues>();
  const [interviewers, setInterviewers] = useState<IInterviewer[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | undefined>();

  useEffect(() => {
    if (!open) return;
    setError(undefined);
    form.resetFields();
    setLoading(true);
    getInterviewersAsync()
      .then(setInterviewers)
      .catch(() => setError('Could not load the list of interviewers.'))
      .finally(() => setLoading(false));
  }, [open]);

  const save = async () => {
    const parsed = schema.safeParse(form.getFieldsValue(true));
    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message);
      return;
    }

    setSaving(true);
    setError(undefined);
    try {
      await scheduleAsync({
        applicationId,
        scheduledDate: parsed.data.scheduledDate.toISOString(),
        // A TimeSpan over the wire, which is what the server's ScheduledTime is.
        scheduledTime: parsed.data.scheduledTime.format('HH:mm:ss'),
        interviewerUserId: parsed.data.interviewerUserId,
        location: parsed.data.location || undefined,
        meetingLink: parsed.data.meetingLink || undefined,
        notes: parsed.data.notes || undefined,
      });
      message.success('Interview scheduled.');
      onScheduled();
      onClose();
    } catch (e) {
      const abp = (e as { response?: { data?: { error?: { message?: string; details?: string } } } })
        ?.response?.data?.error;
      setError(abp?.message || abp?.details || 'Could not schedule this interview.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal
      open={open}
      title={applicantName ? `Schedule an interview with ${applicantName}` : 'Schedule an interview'}
      okText="Schedule it"
      okButtonProps={{ loading: saving }}
      onOk={save}
      onCancel={onClose}
      width={640}
      destroyOnHidden
    >
      {error && <Alert type="error" showIcon message={error} style={{ marginBottom: 16 }} />}

      <Form form={form} layout="vertical">
        <Form.Item
          label="Who will conduct it"
          name="interviewerUserId"
          rules={[{ required: true }]}
          extra="Anyone the school has given the interview role — usually a teacher."
        >
          <Select
            loading={loading}
            placeholder="Choose an interviewer"
            showSearch
            optionFilterProp="label"
            options={interviewers.map((i) => ({
              value: i.userId,
              label: i.name,
              /* Their current load, so whoever books can spread it rather than
                 giving the same teacher every interview of the week. */
              title: `${i.roles.join(', ')} · ${i.upcomingInterviews} upcoming`,
            }))}
            optionRender={(option) => {
              const who = interviewers.find((i) => i.userId === option.value);
              return (
                <Space direction="vertical" size={0}>
                  <Text>{who?.name}</Text>
                  <Space size={4} wrap>
                    {who?.roles.map((r) => <Tag key={r} style={{ margin: 0 }}>{r}</Tag>)}
                    <Text type="secondary" style={{ fontSize: 12 }}>
                      {who?.upcomingInterviews === 0
                        ? 'nothing booked'
                        : `${who?.upcomingInterviews} already booked`}
                    </Text>
                  </Space>
                </Space>
              );
            }}
          />
        </Form.Item>

        <Space size={16} style={{ display: 'flex' }} align="start">
          <Form.Item
            label="Date"
            name="scheduledDate"
            rules={[{ required: true }]}
            extra={`At least ${MINIMUM_NOTICE_DAYS} days from now.`}
            style={{ flex: 1 }}
          >
            <DatePicker
              format="DD MMM YYYY"
              style={{ width: '100%' }}
              disabledDate={(d) => d.isBefore(dayjs().add(MINIMUM_NOTICE_DAYS, 'day'), 'day')}
            />
          </Form.Item>
          <Form.Item label="Time" name="scheduledTime" rules={[{ required: true }]} style={{ flex: 1 }}>
            <TimePicker format="HH:mm" minuteStep={15} style={{ width: '100%' }} />
          </Form.Item>
        </Space>

        <Form.Item label="Where" name="location" extra="A room or address, for an interview in person.">
          <Input placeholder="e.g. Reception, main building" maxLength={200} />
        </Form.Item>

        <Form.Item label="Or a meeting link" name="meetingLink" extra="For an online interview.">
          <Input placeholder="https://…" maxLength={500} />
        </Form.Item>

        <Form.Item label="Anything the interviewer should know" name="notes">
          <Input.TextArea rows={3} maxLength={2000} showCount />
        </Form.Item>
      </Form>
    </Modal>
  );
}
