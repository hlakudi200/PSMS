'use client';

import React, { useState } from 'react';
import { Alert, Form, Input, Modal, Typography } from 'antd';
import { z } from 'zod';

const { Paragraph, Text } = Typography;

/**
 * Why an issued report card is being taken back.
 *
 * Required, and deliberately so. Withdrawing undoes something a family has
 * already been told about and may already have read; the school should have to
 * say what was wrong with it, and that answer belongs on the record rather than
 * in somebody's memory. The parents are told, and the reason goes with it.
 */
const schema = z.object({
  reason: z
    .string()
    .trim()
    .min(3, 'Say what was wrong with the card.')
    .max(1000, 'Keep it under 1000 characters.'),
});

interface Props {
  open: boolean;
  studentName?: string;
  saving?: boolean;
  onCancel: () => void;
  onConfirm: (reason: string) => void | Promise<void>;
}

export default function WithdrawReportModal({
  open,
  studentName,
  saving,
  onCancel,
  onConfirm,
}: Props) {
  const [reason, setReason] = useState('');
  const [error, setError] = useState<string | undefined>();

  const close = () => {
    setReason('');
    setError(undefined);
    onCancel();
  };

  const submit = async () => {
    const parsed = schema.safeParse({ reason });
    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message);
      return;
    }
    setError(undefined);
    await onConfirm(parsed.data.reason);
    setReason('');
  };

  return (
    <Modal
      open={open}
      title="Withdraw this report card"
      okText="Withdraw"
      okButtonProps={{ danger: true, loading: saving }}
      onOk={submit}
      onCancel={close}
      destroyOnHidden
    >
      <Paragraph>
        The card goes back to <Text strong>Approved</Text>: it leaves the
        family&apos;s list, the verification page stops confirming it, and it can be
        corrected and issued again.
      </Paragraph>

      <Alert
        type="warning"
        showIcon
        style={{ marginBottom: 16 }}
        message={
          studentName
            ? `${studentName}'s parents will be told it was withdrawn`
            : 'The parents will be told it was withdrawn'
        }
        description="They were told the card was ready and may already have read or printed it, so it should not simply disappear. The reason you give below is sent with that message."
      />

      <Form layout="vertical">
        <Form.Item
          label="Why is it being withdrawn?"
          required
          validateStatus={error ? 'error' : undefined}
          help={error}
        >
          <Input.TextArea
            rows={3}
            maxLength={1000}
            showCount
            autoFocus
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder="e.g. The Mathematics mark was captured against the wrong learner."
          />
        </Form.Item>
      </Form>
    </Modal>
  );
}
