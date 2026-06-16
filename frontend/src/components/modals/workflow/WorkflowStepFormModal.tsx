'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, InputNumber, Select, Switch, message } from 'antd';
import { z } from 'zod';
import { useWorkflowStepActions } from '@/providers/workflow/workflow-steps';
import type { IWorkflowStep } from '@/providers/workflow/shared/interfaces';
import { WorkflowActionType, WorkflowActionTypeLabels } from '@/providers/workflow/shared/interfaces';
import { WorkflowUserSelect } from './WorkflowUserSelect';

const stepSchema = z.object({
  workflowDefinitionId: z.string().min(1),
  stepOrder: z.number().min(1, 'Step order must be at least 1'),
  name: z.string().min(1, 'Name is required').max(200),
  description: z.string().max(500).optional(),
  assignedRole: z.string().min(1, 'Assigned role is required').max(100),
  actionType: z.number().min(1, 'Action type is required'),
  isTerminal: z.boolean(),
  isCommentRequired: z.boolean(),
  nextStepOnApprove: z.number().optional(),
  nextStepOnReject: z.number().optional(),
  assignedUserId: z.number().optional(),
  slaHours: z.number().min(1).optional(),
  guardExpression: z.string().max(500).optional(),
});

interface WorkflowStepFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IWorkflowStep | null;
  definitionId: string;
  nextStepOrder: number;
}

const actionTypeOptions = [
  { value: WorkflowActionType.Submit, label: 'Submit' },
  { value: WorkflowActionType.Review, label: 'Review' },
  { value: WorkflowActionType.Approve, label: 'Approve' },
  { value: WorkflowActionType.Reject, label: 'Reject' },
  { value: WorkflowActionType.Revise, label: 'Revise' },
];

const roleOptions = [
  { value: 'Admin', label: 'Admin' },
  { value: 'Principal', label: 'Principal' },
  { value: 'VicePrincipal', label: 'Vice Principal' },
  { value: 'HOD', label: 'HOD' },
  { value: 'Teacher', label: 'Teacher' },
  { value: 'Finance', label: 'Finance' },
];

export const WorkflowStepFormModal: React.FC<WorkflowStepFormModalProps> = ({
  open,
  onClose,
  editRecord,
  definitionId,
  nextStepOrder,
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useWorkflowStepActions();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;
  // The user picker is steered to users holding the currently-selected role.
  const selectedRole = Form.useWatch('assignedRole', form);

  useEffect(() => {
    if (open) {
      if (editRecord) {
        form.setFieldsValue({
          name: editRecord.name,
          description: editRecord.description,
          assignedRole: editRecord.assignedRole,
          actionType: editRecord.actionType,
          isTerminal: editRecord.isTerminal,
          isCommentRequired: editRecord.isCommentRequired,
          nextStepOnApprove: editRecord.nextStepOnApprove,
          nextStepOnReject: editRecord.nextStepOnReject,
          slaHours: editRecord.slaHours,
          assignedUserId: editRecord.assignedUserId,
          guardExpression: editRecord.guardExpression,
        });
      } else {
        form.resetFields();
        form.setFieldsValue({
          stepOrder: nextStepOrder,
          isTerminal: false,
          isCommentRequired: false,
        });
      }
    }
  }, [open, editRecord, form, nextStepOrder]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();

      if (isEdit) {
        const updateData: Record<string, unknown> = {};
        if (values.name !== undefined) updateData.name = values.name;
        if (values.description !== undefined) updateData.description = values.description;
        if (values.assignedRole !== undefined) updateData.assignedRole = values.assignedRole;
        if (values.actionType !== undefined) updateData.actionType = values.actionType;
        if (values.isTerminal !== undefined) updateData.isTerminal = values.isTerminal;
        if (values.isCommentRequired !== undefined) updateData.isCommentRequired = values.isCommentRequired;
        if (values.nextStepOnApprove !== undefined) updateData.nextStepOnApprove = values.nextStepOnApprove;
        else if (editRecord?.nextStepOnApprove) updateData.clearNextStepOnApprove = true;
        if (values.nextStepOnReject !== undefined) updateData.nextStepOnReject = values.nextStepOnReject;
        else if (editRecord?.nextStepOnReject) updateData.clearNextStepOnReject = true;
        if (values.slaHours !== undefined) updateData.slaHours = values.slaHours;
        else if (editRecord?.slaHours) updateData.clearSlaHours = true;
        if (values.assignedUserId !== undefined) updateData.assignedUserId = values.assignedUserId;
        else if (editRecord?.assignedUserId) updateData.clearAssignedUserId = true;
        if (values.guardExpression !== undefined && values.guardExpression !== '') updateData.guardExpression = values.guardExpression;
        else if (editRecord?.guardExpression) updateData.clearGuardExpression = true;

        setLoading(true);
        await updateAsync(editRecord!.id, updateData);
      } else {
        values.workflowDefinitionId = definitionId;
        const result = stepSchema.safeParse(values);
        if (!result.success) {
          const fieldErrors = result.error.issues.map(err => ({
            name: err.path as string[],
            errors: [err.message],
          }));
          form.setFields(fieldErrors);
          return;
        }
        setLoading(true);
        await createAsync(result.data);
      }
      message.success(`Step ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      // Axios interceptor handles ABP error display
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit Step' : 'New Step'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
      width={600}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Step Name" name="name" rules={[{ required: true }]}>
          <Input placeholder="e.g. Principal Review" maxLength={200} />
        </Form.Item>
        {!isEdit && (
          <Form.Item label="Step Order" name="stepOrder" rules={[{ required: true }]}>
            <InputNumber min={1} style={{ width: '100%' }} />
          </Form.Item>
        )}
        <Form.Item label="Assigned Role" name="assignedRole" rules={[{ required: true }]}>
          <Select options={roleOptions} placeholder="Select role" />
        </Form.Item>
        <Form.Item label="Action Type" name="actionType" rules={[{ required: true }]}>
          <Select options={actionTypeOptions} placeholder="Select action" />
        </Form.Item>
        <Form.Item label="Description" name="description">
          <Input.TextArea rows={2} maxLength={500} placeholder="Optional description" />
        </Form.Item>
        <div style={{ display: 'flex', gap: 16 }}>
          <Form.Item label="Terminal Step" name="isTerminal" valuePropName="checked" style={{ flex: 1 }}>
            <Switch />
          </Form.Item>
          <Form.Item label="Comment Required" name="isCommentRequired" valuePropName="checked" style={{ flex: 1 }}>
            <Switch />
          </Form.Item>
        </div>
        <div style={{ display: 'flex', gap: 16 }}>
          <Form.Item label="Next Step on Approve" name="nextStepOnApprove" style={{ flex: 1 }}>
            <InputNumber min={1} style={{ width: '100%' }} placeholder="Auto (next)" />
          </Form.Item>
          <Form.Item label="Next Step on Reject" name="nextStepOnReject" style={{ flex: 1 }}>
            <InputNumber min={1} style={{ width: '100%' }} placeholder="Terminal reject" />
          </Form.Item>
        </div>
        <div style={{ display: 'flex', gap: 16 }}>
          <Form.Item label="SLA Hours" name="slaHours" style={{ flex: 1 }}>
            <InputNumber min={1} style={{ width: '100%' }} placeholder="Optional deadline (hours)" />
          </Form.Item>
          <Form.Item label="Assigned User" name="assignedUserId" style={{ flex: 1 }}>
            <WorkflowUserSelect
              roleFilter={selectedRole}
              placeholder="Optional — pin to a specific user"
            />
          </Form.Item>
        </div>
        <Form.Item label="Guard Expression" name="guardExpression">
          <Input placeholder="Optional condition e.g. Amount > 10000" maxLength={500} />
        </Form.Item>
      </Form>
    </Modal>
  );
};
