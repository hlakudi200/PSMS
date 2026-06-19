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
  /** WF-22: current step's action type — drives which actions are offered. */
  currentStepActionType?: number;
  /** WF-22: 1-based step order — "Send for Revision" only shows when > 1. */
  currentStepOrder?: number;
}

// The primary "forward" action for each step type. Submit / Review / Approve all
// advance the workflow; the label tells the user what their forward action means.
const forwardActionByType: Record<number, { value: number; label: string }> = {
  [WorkflowActionType.Submit]: { value: WorkflowActionType.Submit, label: 'Submit' },
  [WorkflowActionType.Review]: { value: WorkflowActionType.Review, label: 'Review & Forward' },
  [WorkflowActionType.Approve]: { value: WorkflowActionType.Approve, label: 'Approve' },
};

// Full set, used as a fallback when the step's action type is unknown.
const allActionOptions = [
  { value: WorkflowActionType.Approve, label: 'Approve' },
  { value: WorkflowActionType.Reject, label: 'Reject' },
  { value: WorkflowActionType.Review, label: 'Review & Forward' },
  { value: WorkflowActionType.Revise, label: 'Send for Revision' },
];

function buildActionOptions(actionType?: number, currentStepOrder?: number) {
  const forward = actionType != null ? forwardActionByType[actionType] : undefined;
  if (!forward) return allActionOptions;

  const options = [forward];
  // Submit steps just submit; review/approve steps can also reject.
  if (actionType !== WorkflowActionType.Submit) {
    options.push({ value: WorkflowActionType.Reject, label: 'Reject' });
  }
  // Can only send back when there is an earlier step to send it to.
  if ((currentStepOrder ?? 1) > 1) {
    options.push({ value: WorkflowActionType.Revise, label: 'Send for Revision' });
  }
  return options;
}

export const AdvanceWorkflowModal: React.FC<AdvanceWorkflowModalProps> = ({
  open,
  onClose,
  instanceId,
  currentStepName,
  isCommentRequired,
  currentStepActionType,
  currentStepOrder,
}) => {
  const [form] = Form.useForm();
  const { advanceAsync } = useWorkflowInstanceActions();
  const [loading, setLoading] = React.useState(false);

  const actionOptions = buildActionOptions(currentStepActionType, currentStepOrder);

  useEffect(() => {
    if (open) {
      form.resetFields();
      // Pre-select the step's primary (forward) action to save a click.
      form.setFieldsValue({ action: actionOptions[0]?.value });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, form, currentStepActionType, currentStepOrder]);

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
          <Select options={actionOptions} placeholder="Select action" />
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
