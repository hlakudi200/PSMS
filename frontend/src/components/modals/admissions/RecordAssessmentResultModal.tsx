'use client';

import React, { useEffect, useState } from 'react';
import { Alert, Form, Input, InputNumber, Modal, Space, Statistic, Typography, message } from 'antd';
import { z } from 'zod';
import { useAdmissionAssessmentActions } from '@/providers/admissions/admission_assessments';
import type { IAdmissionAssessment } from '@/providers/admissions/shared/interfaces';

const { Paragraph, Text } = Typography;

const schema = z
  .object({
    totalScore: z.number({ error: 'Enter the mark.' }).min(0, 'A mark cannot be negative.'),
    maxScore: z.number({ error: 'Enter what the paper was out of.' }).min(1),
    passPercentage: z.number({ error: 'Enter the pass mark.' }).min(0).max(100),
    feedback: z.string().trim().max(2000).optional().or(z.literal('')),
  })
  .refine((v) => v.totalScore <= v.maxScore, {
    path: ['totalScore'],
    message: 'The mark cannot be more than the paper was out of.',
  });

type FormValues = z.infer<typeof schema>;

/**
 * The result of a placement assessment.
 *
 * Whether it is a pass is worked out from the mark and the pass mark, and the
 * screen shows that before it is saved — the pass flag is what the principal
 * sees on the application, and nobody should discover which way it went only
 * after committing to it.
 */
export default function RecordAssessmentResultModal({
  open,
  assessment,
  onClose,
  onRecorded,
}: {
  open: boolean;
  assessment?: IAdmissionAssessment;
  onClose: () => void;
  onRecorded: () => void;
}) {
  const { recordResultsAsync } = useAdmissionAssessmentActions();
  const [form] = Form.useForm<FormValues>();
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | undefined>();

  const total = Form.useWatch('totalScore', form);
  const outOf = Form.useWatch('maxScore', form);
  const passMark = Form.useWatch('passPercentage', form);

  useEffect(() => {
    if (!open) return;
    setError(undefined);
    form.resetFields();
    form.setFieldsValue({
      maxScore: assessment?.maxScore || 100,
      passPercentage: 50,
    });
  }, [open, assessment]);

  const percentage =
    typeof total === 'number' && typeof outOf === 'number' && outOf > 0
      ? (total / outOf) * 100
      : undefined;
  const passed = percentage != null && typeof passMark === 'number' ? percentage >= passMark : undefined;

  const save = async () => {
    const parsed = schema.safeParse(form.getFieldsValue(true));
    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message);
      return;
    }
    if (!assessment) return;

    setSaving(true);
    setError(undefined);
    try {
      await recordResultsAsync(assessment.id, {
        totalScore: parsed.data.totalScore,
        maxScore: parsed.data.maxScore,
        passPercentage: parsed.data.passPercentage,
        feedback: parsed.data.feedback || undefined,
      });
      message.success('Result recorded. Thank you.');
      onRecorded();
      onClose();
    } catch (e) {
      const abp = (e as { response?: { data?: { error?: { message?: string; details?: string } } } })
        ?.response?.data?.error;
      setError(abp?.message || abp?.details || 'Could not record this result.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal
      open={open}
      title={assessment ? `${assessment.applicantName}'s assessment` : 'Record the result'}
      okText="Save the result"
      okButtonProps={{ loading: saving }}
      onOk={save}
      onCancel={onClose}
      cancelText="Not now"
      width={620}
      destroyOnHidden
    >
      {error && <Alert type="error" showIcon message={error} style={{ marginBottom: 16 }} />}

      <Paragraph type="secondary">
        This is not the decision — the principal makes that at the end. It is what the paper says.
      </Paragraph>

      <Form form={form} layout="vertical">
        <Space size={16} wrap align="start">
          <Form.Item label="Mark" name="totalScore" rules={[{ required: true }]}>
            <InputNumber min={0} style={{ width: 120 }} />
          </Form.Item>
          <Form.Item label="Out of" name="maxScore" rules={[{ required: true }]}>
            <InputNumber min={1} style={{ width: 120 }} />
          </Form.Item>
          <Form.Item label="Pass mark" name="passPercentage" rules={[{ required: true }]}>
            <InputNumber min={0} max={100} suffix="%" style={{ width: 140 }} />
          </Form.Item>
        </Space>

        {percentage != null && (
          <Space size={32} style={{ marginBottom: 16 }}>
            <Statistic title="That is" value={percentage} precision={1} suffix="%" />
            <Statistic
              title="Which counts as"
              value={passed ? 'A pass' : 'Not a pass'}
              valueStyle={{ color: passed ? '#389e0d' : '#cf1322' }}
            />
          </Space>
        )}

        <Form.Item
          label="Anything worth saying about it"
          name="feedback"
          extra={<Text type="secondary">The principal reads this before deciding.</Text>}
        >
          <Input.TextArea rows={4} maxLength={2000} showCount placeholder="How the learner handled the paper, and what stood out." />
        </Form.Item>
      </Form>
    </Modal>
  );
}
