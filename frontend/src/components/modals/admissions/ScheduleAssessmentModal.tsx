'use client';

import React, { useEffect, useState } from 'react';
import {
  Alert, DatePicker, Form, Input, InputNumber, Modal, Select, Space, Tag, Typography, message,
} from 'antd';
import dayjs, { Dayjs } from 'dayjs';
import { z } from 'zod';
import { useAdmissionAssessmentActions } from '@/providers/admissions/admission_assessments';
import {
  AdmissionAssessmentType, admissionAssessmentTypeOptions, type IAssessor,
} from '@/providers/admissions/shared/interfaces';

const { Text } = Typography;

/** ADM-015: a family gets a week's warning before a placement assessment. */
const MINIMUM_NOTICE_DAYS = 7;

const schema = z.object({
  type: z.number({ error: 'Choose what kind of assessment this is.' }),
  assessorUserId: z.number({ error: 'Choose who will set and mark it.' }),
  scheduledDate: z.custom<Dayjs>((v) => dayjs.isDayjs(v), 'Choose a date.')
    .refine((v) => v.isAfter(dayjs().add(MINIMUM_NOTICE_DAYS, 'day'), 'minute'), {
      message: `The family needs at least ${MINIMUM_NOTICE_DAYS} days' notice, so pick a later date.`,
    }),
  maxScore: z.number({ error: 'What is the paper out of?' }).min(1, 'A paper out of nothing cannot be marked.'),
  subjects: z.string().trim().max(500).optional().or(z.literal('')),
});

type FormValues = z.infer<typeof schema>;

/**
 * Setting a placement assessment.
 *
 * The grade decides whether there is one at all, and which one — a Grade 1
 * applicant and a Grade 11 applicant cannot sit the same paper. So the grade
 * is not asked for here: it is the grade the learner applied to.
 */
export default function ScheduleAssessmentModal({
  open,
  applicationId,
  applicantName,
  gradeId,
  gradeName,
  onClose,
  onScheduled,
}: {
  open: boolean;
  applicationId: string;
  applicantName?: string;
  gradeId?: string;
  gradeName?: string;
  onClose: () => void;
  onScheduled: () => void;
}) {
  const { scheduleAsync, getAssessorsAsync } = useAdmissionAssessmentActions();
  const [form] = Form.useForm<FormValues>();
  const [assessors, setAssessors] = useState<IAssessor[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | undefined>();

  useEffect(() => {
    if (!open) return;
    setError(undefined);
    form.resetFields();
    form.setFieldsValue({ type: AdmissionAssessmentType.Placement, maxScore: 100 });
    setLoading(true);
    getAssessorsAsync()
      .then(setAssessors)
      .catch(() => setError('Could not load the list of assessors.'))
      .finally(() => setLoading(false));
  }, [open]);

  const save = async () => {
    const parsed = schema.safeParse(form.getFieldsValue(true));
    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message);
      return;
    }

    if (!gradeId) {
      setError('This application has no grade, so there is nothing to assess them for.');
      return;
    }

    setSaving(true);
    setError(undefined);
    try {
      await scheduleAsync({
        applicationId,
        type: parsed.data.type,
        scheduledDate: parsed.data.scheduledDate.toISOString(),
        assessedGradeId: gradeId,
        assessorUserId: parsed.data.assessorUserId,
        maxScore: parsed.data.maxScore,
        subjects: parsed.data.subjects || undefined,
      });
      message.success('Assessment scheduled.');
      onScheduled();
      onClose();
    } catch (e) {
      const abp = (e as { response?: { data?: { error?: { message?: string; details?: string } } } })
        ?.response?.data?.error;
      setError(abp?.message || abp?.details || 'Could not schedule this assessment.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal
      open={open}
      title={applicantName ? `Set an assessment for ${applicantName}` : 'Set an assessment'}
      okText="Schedule it"
      okButtonProps={{ loading: saving }}
      onOk={save}
      onCancel={onClose}
      width={640}
      destroyOnHidden
    >
      {error && <Alert type="error" showIcon message={error} style={{ marginBottom: 16 }} />}

      {gradeName && (
        <Alert
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
          message={`This is the ${gradeName} paper`}
          description="The assessment is for the grade applied to, which is why each grade decides separately whether it sets one."
        />
      )}

      <Form form={form} layout="vertical">
        <Space size={16} style={{ display: 'flex' }} align="start">
          <Form.Item label="Kind of assessment" name="type" rules={[{ required: true }]} style={{ flex: 1 }}>
            <Select options={admissionAssessmentTypeOptions} />
          </Form.Item>
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
        </Space>

        <Form.Item
          label="Who will set and mark it"
          name="assessorUserId"
          rules={[{ required: true }]}
          extra="Anyone the school has given the assessment role — usually a teacher of that grade."
        >
          <Select
            loading={loading}
            placeholder="Choose an assessor"
            showSearch
            optionFilterProp="label"
            options={assessors.map((a) => ({ value: a.userId, label: a.name }))}
            optionRender={(option) => {
              const who = assessors.find((a) => a.userId === option.value);
              return (
                <Space direction="vertical" size={0}>
                  <Text>{who?.name}</Text>
                  <Space size={4} wrap>
                    {who?.roles.map((r) => <Tag key={r} style={{ margin: 0 }}>{r}</Tag>)}
                    <Text type="secondary" style={{ fontSize: 12 }}>
                      {who?.outstandingAssessments === 0
                        ? 'nothing to mark'
                        : `${who?.outstandingAssessments} still to mark`}
                    </Text>
                  </Space>
                </Space>
              );
            }}
          />
        </Form.Item>

        <Form.Item label="Out of" name="maxScore" rules={[{ required: true }]} extra="The total the paper is marked out of.">
          <InputNumber min={1} max={1000} style={{ width: 160 }} />
        </Form.Item>

        <Form.Item
          label="What it covers"
          name="subjects"
          extra="Optional — the subjects or skills on the paper, one per line."
        >
          <Input.TextArea rows={3} maxLength={500} showCount placeholder={'Numeracy\nLiteracy'} />
        </Form.Item>
      </Form>
    </Modal>
  );
}
