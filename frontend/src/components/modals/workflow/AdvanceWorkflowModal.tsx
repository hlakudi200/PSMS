'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, message } from 'antd';
import { z } from 'zod';
import { useWorkflowInstanceActions } from '@/providers/workflow/workflow-instances';
import { WorkflowActionType, WorkflowActionTypeLabels } from '@/providers/workflow/shared/interfaces';

const advanceSchema = z.object({
  action: z.number().min(1, 'Action is required'),
  comment: z.string().max(2000).optional(),
  attachmentUrl: z.string().max(2048).optional(),
});

interface AdvanceWorkflowModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  instanceId: string;
  currentStepName?: string;
  isCommentRequired?: boolean;
}

const advanceActionOptions = [
  { value: WorkflowActionType.Approve, label: 'Approve' },
  { value: WorkflowActionType.Reject, label: 'Reject' },
  { value: WorkflowActionType.Review, label: 'Review' },
  { value: WorkflowActionType.Revise, label: 'Send for Revision' },
];

export const AdvanceWorkflowModal: React.FC<AdvanceWorkflowModalProps> = ({
  open,
  onClose,
  instanceId,
  currentStepName,
  isCommentRequired,
}) => {
  const [form] = Form.useForm();
  const { advanceAsync } = useWorkflowInstanceActions();
  const [loading, setLoading] = React.useState(false);

  useEffect(() => {
    if (open) {
      form.resetFields();
    }
  }, [open, form]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();

      if (isCommentRequired && !values.comment?.trim()) {
        form.setFields([{ name: 'comment', errors: ['Comment is required for this step'] }]);
        return;
      }

      const result = advanceSchema.safeParse(values);
      if (!result.success) {
        const fieldErrors = result.error.issues.map(err => ({
          name: err.path as string[],
          errors: [err.message],
        }));
        form.setFields(fieldErrors);
        return;
      }

      setLoading(true);
      await advanceAsync(instanceId, result.data);
      const actionLabel = WorkflowActionTypeLabels[result.data.action] ?? 'Action';
      message.success(`${actionLabel} completed successfully`);
      onClose(true);
    } catch {
      // Axios interceptor handles ABP error display
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={`Take Action — ${currentStepName ?? 'Current Step'}`}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Action" name="action" rules={[{ required: true }]}>
          <Select options={advanceActionOptions} placeholder="Select action" />
        </Form.Item>
        <Form.Item
          label="Comment"
          name="comment"
          rules={isCommentRequired ? [{ required: true, message: 'Comment is required' }] : []}
        >
          <Input.TextArea
            rows={4}
            maxLength={2000}
            placeholder={isCommentRequired ? 'Comment required for this step' : 'Optional comment'}
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};
