'use client';

import React, { useEffect, useMemo } from 'react';
import { Modal, Form, Input, InputNumber, Select, DatePicker, Switch, Alert, Checkbox, message, Typography } from 'antd';
import { z } from 'zod';
import dayjs from 'dayjs';
import { useWorkflowInstanceActions } from '@/providers/workflow/workflow-instances';
import {
  WorkflowActionType,
  WorkflowActionTypeLabels,
} from '@/providers/workflow/shared/interfaces';
import type {
  IWorkflowDecisionField,
  IWorkflowDecisionSchema,
  IWorkflowGuardStatus,
} from '@/providers/workflow/shared/interfaces';

const { Text } = Typography;

const advanceSchema = z.object({
  action: z.number({ error: 'Action is required' }).min(1, 'Action is required'),
  comment: z.string().max(2000).optional(),
  overrideGuard: z.boolean().optional(),
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
  /** WF-30: the step may be waived with a reason. */
  currentStepIsOptional?: boolean;
  /** WF-30: live guard status; a failing guard disables the forward action unless overridden. */
  currentStepGuard?: IWorkflowGuardStatus;
  /** WF-32: decision fields to render for forward actions. */
  decisionSchema?: IWorkflowDecisionSchema;
  /** WF-30: whether the viewer may override a failing guard (server enforces the permission). */
  canOverrideGuard?: boolean;
}

// The primary "forward" action for each step type. Submit / Review / Approve all
// advance the workflow; the label tells the user what their forward action means.
const forwardActionByType: Record<number, { value: number; label: string }> = {
  [WorkflowActionType.Submit]: { value: WorkflowActionType.Submit, label: 'Submit' },
  [WorkflowActionType.Review]: { value: WorkflowActionType.Review, label: 'Review & Forward' },
  [WorkflowActionType.Approve]: { value: WorkflowActionType.Approve, label: 'Approve' },
};

const FORWARD_ACTIONS = new Set<number>([
  WorkflowActionType.Submit,
  WorkflowActionType.Review,
  WorkflowActionType.Approve,
]);

function buildActionOptions(actionType?: number, currentStepOrder?: number, isOptional?: boolean) {
  // Steps configured with a non-forward type (legacy Reject/Revise) behave as Approve steps.
  const forward = forwardActionByType[actionType ?? WorkflowActionType.Approve] ?? forwardActionByType[WorkflowActionType.Approve];
  const options = [forward];
  // Submit steps just submit; review/approve steps can also reject.
  if (actionType !== WorkflowActionType.Submit) {
    options.push({ value: WorkflowActionType.Reject, label: 'Reject' });
  }
  // Can only send back when there is an earlier step to send it to.
  if ((currentStepOrder ?? 1) > 1) {
    options.push({ value: WorkflowActionType.Revise, label: 'Send for Revision' });
  }
  if (isOptional) {
    options.push({ value: WorkflowActionType.Waive, label: 'Waive (not required)' });
  }
  return options;
}

function DecisionFieldInput({ field }: { field: IWorkflowDecisionField }) {
  switch (field.type) {
    case 'number':
      return <InputNumber style={{ width: '100%' }} min={field.min} max={field.max} precision={2} placeholder={field.placeholder} />;
    case 'select':
      return <Select options={(field.options ?? []).map((o) => ({ value: o.value, label: o.label }))} placeholder={field.placeholder ?? 'Select'} />;
    case 'date':
      return <DatePicker style={{ width: '100%' }} />;
    case 'boolean':
      return <Switch />;
    case 'textarea':
      return <Input.TextArea rows={3} maxLength={field.max ?? 2000} placeholder={field.placeholder} />;
    default:
      return <Input maxLength={field.max ?? 500} placeholder={field.placeholder} />;
  }
}

export const AdvanceWorkflowModal: React.FC<AdvanceWorkflowModalProps> = ({
  open,
  onClose,
  instanceId,
  currentStepName,
  isCommentRequired,
  currentStepActionType,
  currentStepOrder,
  currentStepIsOptional,
  currentStepGuard,
  decisionSchema,
  canOverrideGuard,
}) => {
  const [form] = Form.useForm();
  const { advanceAsync } = useWorkflowInstanceActions();
  const [loading, setLoading] = React.useState(false);

  const actionOptions = useMemo(
    () => buildActionOptions(currentStepActionType, currentStepOrder, currentStepIsOptional),
    [currentStepActionType, currentStepOrder, currentStepIsOptional]
  );

  const selectedAction = Form.useWatch('action', form) as number | undefined;
  const overrideChecked = Form.useWatch('overrideGuard', form) as boolean | undefined;
  const isForward = selectedAction != null && FORWARD_ACTIONS.has(selectedAction);
  const isWaive = selectedAction === WorkflowActionType.Waive;
  const guardBlocks = isForward && currentStepGuard && !currentStepGuard.satisfied;
  const isReject = selectedAction === WorkflowActionType.Reject;
  // A rejection always needs a reason: the handlers store the comment as the record's
  // rejection reason (admissions requires at least 50 characters, ADM-020).
  const needsComment = !!isCommentRequired || isWaive || isReject || (guardBlocks && !!overrideChecked);
  const showDecision = isForward && !!decisionSchema && decisionSchema.fields.length > 0;

  useEffect(() => {
    if (open) {
      form.resetFields();
      // Pre-select the step's primary (forward) action to save a click.
      form.setFieldsValue({ action: actionOptions[0]?.value, overrideGuard: false });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, form, currentStepActionType, currentStepOrder, currentStepIsOptional]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();

      if (needsComment && !values.comment?.trim()) {
        form.setFields([{ name: 'comment', errors: [isWaive ? 'A reason is required to waive this step' : isReject ? 'Give the reason for rejecting (admissions needs at least 50 characters)' : 'Comment is required for this step'] }]);
        return;
      }
      if (guardBlocks && !values.overrideGuard) {
        form.setFields([{ name: 'action', errors: ['The exit criteria are not met. Complete the work, waive the step, or override with a reason.'] }]);
        return;
      }

      const result = advanceSchema.safeParse(values);
      if (!result.success) {
        form.setFields(result.error.issues.map(err => ({ name: err.path as string[], errors: [err.message] })));
        return;
      }

      // WF-32: collect the decision fields, checking required ones client-side so
      // the actor gets the message on the field (the server validates again).
      let decision: Record<string, unknown> | undefined;
      if (showDecision && decisionSchema) {
        decision = {};
        const missing: { name: string[]; errors: string[] }[] = [];
        for (const f of decisionSchema.fields) {
          let v = values[f.key];
          if (f.type === 'date' && v) v = dayjs(v).format('YYYY-MM-DD');
          const empty = v === undefined || v === null || v === '';
          if (f.required && empty) missing.push({ name: [f.key], errors: [`${f.label} is required`] });
          if (!empty) decision[f.key] = v;
        }
        if (missing.length) { form.setFields(missing); return; }
      }

      setLoading(true);
      await advanceAsync(instanceId, {
        action: result.data.action,
        comment: result.data.comment,
        decision,
        overrideGuard: guardBlocks ? !!result.data.overrideGuard : undefined,
      });
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
      {currentStepGuard && (
        <Alert
          type={currentStepGuard.satisfied ? 'success' : 'warning'}
          showIcon
          style={{ marginTop: 16 }}
          message={currentStepGuard.satisfied ? `Exit criteria met: ${currentStepGuard.displayName}` : `Exit criteria not met: ${currentStepGuard.displayName}`}
          description={!currentStepGuard.satisfied ? currentStepGuard.message : undefined}
        />
      )}
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Action" name="action" rules={[{ required: true }]}>
          <Select options={actionOptions} placeholder="Select action" />
        </Form.Item>

        {showDecision && decisionSchema && (
          <>
            <Text strong style={{ display: 'block', marginBottom: 8 }}>{decisionSchema.displayName}</Text>
            {decisionSchema.fields.map((f) => (
              <Form.Item
                key={f.key}
                label={f.label}
                name={f.key}
                valuePropName={f.type === 'boolean' ? 'checked' : 'value'}
                rules={f.required ? [{ required: true, message: `${f.label} is required` }] : []}
              >
                <DecisionFieldInput field={f} />
              </Form.Item>
            ))}
          </>
        )}

        {guardBlocks && canOverrideGuard && (
          <Form.Item name="overrideGuard" valuePropName="checked" style={{ marginBottom: 8 }}>
            <Checkbox>Override the exit criteria (a reason is required and recorded)</Checkbox>
          </Form.Item>
        )}

        <Form.Item
          label={isWaive ? 'Reason for waiving' : isReject ? 'Reason for rejecting' : 'Comment'}
          name="comment"
          rules={needsComment ? [{ required: true, message: 'Comment is required' }] : []}
        >
          <Input.TextArea
            rows={4}
            maxLength={2000}
            placeholder={needsComment ? 'Required for this action' : 'Optional comment'}
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};
