'use client';

import React, { useEffect, useState } from 'react';
import { Alert, Form, Input, Modal, Radio, Rate, Typography, message } from 'antd';
import { z } from 'zod';
import { useAdmissionInterviewActions } from '@/providers/admissions/admission_interviews';
import type { IAdmissionInterview } from '@/providers/admissions/shared/interfaces';

const { Paragraph, Text } = Typography;

const schema = z.object({
  rating: z.number({ error: 'Give the interview a rating.' }).min(1).max(5),
  recommended: z.boolean({ error: 'Say whether you would recommend this learner.' }),
  notes: z.string().trim().min(10, 'Write a sentence or two about how it went.').max(2000),
});

type FormValues = z.infer<typeof schema>;

/**
 * What came of the interview.
 *
 * The decision is not made here — the Principal makes it at the end of the
 * admissions workflow — so this asks for what the person in the room actually
 * knows: how the learner came across, whether they would recommend a place,
 * and why. Notes are required because a rating with no reasoning is not
 * something anyone can weigh a decision on later.
 */
export default function RecordInterviewOutcomeModal({
  open,
  interview,
  onClose,
  onRecorded,
}: {
  open: boolean;
  interview?: IAdmissionInterview;
  onClose: () => void;
  onRecorded: () => void;
}) {
  const { completeAsync, markNoShowAsync } = useAdmissionInterviewActions();
  const [form] = Form.useForm<FormValues>();
  const [saving, setSaving] = useState(false);
  const [noShowing, setNoShowing] = useState(false);
  const [error, setError] = useState<string | undefined>();

  useEffect(() => {
    if (!open) return;
    setError(undefined);
    form.resetFields();
  }, [open]);

  const sayWhatTheServerSaid = (e: unknown, fallback: string) => {
    const abp = (e as { response?: { data?: { error?: { message?: string; details?: string } } } })
      ?.response?.data?.error;
    setError(abp?.message || abp?.details || fallback);
  };

  const save = async () => {
    const parsed = schema.safeParse(form.getFieldsValue(true));
    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message);
      return;
    }
    if (!interview) return;

    setSaving(true);
    setError(undefined);
    try {
      await completeAsync(interview.id, {
        rating: parsed.data.rating,
        recommended: parsed.data.recommended,
        notes: parsed.data.notes,
      });
      message.success('Recorded. Thank you.');
      onRecorded();
      onClose();
    } catch (e) {
      sayWhatTheServerSaid(e, 'Could not record this interview.');
    } finally {
      setSaving(false);
    }
  };

  const noShow = async () => {
    if (!interview) return;
    setNoShowing(true);
    setError(undefined);
    try {
      await markNoShowAsync(interview.id);
      message.success('Marked as not attended.');
      onRecorded();
      onClose();
    } catch (e) {
      sayWhatTheServerSaid(e, 'Could not mark this as a no-show.');
    } finally {
      setNoShowing(false);
    }
  };

  return (
    <Modal
      open={open}
      title={interview ? `How did the interview with ${interview.applicantName} go?` : 'Record the interview'}
      okText="Save what happened"
      okButtonProps={{ loading: saving }}
      onOk={save}
      onCancel={onClose}
      cancelText="Not now"
      width={620}
      destroyOnHidden
      footer={(_, { OkBtn, CancelBtn }) => (
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          {/* The other thing that actually happens: the family did not turn up. */}
          <a
            onClick={noShowing ? undefined : noShow}
            style={{ color: '#cf1322', cursor: noShowing ? 'default' : 'pointer' }}
          >
            {noShowing ? 'Marking…' : 'They did not come'}
          </a>
          <div>
            <CancelBtn />
            <OkBtn />
          </div>
        </div>
      )}
    >
      {error && <Alert type="error" showIcon message={error} style={{ marginBottom: 16 }} />}

      <Paragraph type="secondary">
        This is not the decision — the principal makes that at the end. It is what you saw.
      </Paragraph>

      <Form form={form} layout="vertical">
        <Form.Item label="How did the learner come across?" name="rating" rules={[{ required: true }]}>
          <Rate />
        </Form.Item>

        <Form.Item
          label="Would you recommend a place for this learner?"
          name="recommended"
          rules={[{ required: true }]}
        >
          <Radio.Group
            options={[
              { value: true, label: 'Yes' },
              { value: false, label: 'No' },
            ]}
            optionType="button"
          />
        </Form.Item>

        <Form.Item
          label="Why"
          name="notes"
          rules={[{ required: true }]}
          extra={<Text type="secondary">The principal reads this before deciding.</Text>}
        >
          <Input.TextArea rows={5} maxLength={2000} showCount placeholder="How the interview went, and what stood out." />
        </Form.Item>
      </Form>
    </Modal>
  );
}
