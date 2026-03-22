'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, InputNumber, Select, DatePicker, message } from 'antd';
import { z } from 'zod';
import { useWorkflowDelegationActions } from '@/providers/workflow/workflow-delegations';
import { WorkflowEntityTypeLabels } from '@/providers/workflow/shared/interfaces';

const delegationSchema = z.object({
  delegateUserId: z.number({ error: 'Delegate user ID is required' }).min(1, 'Delegate user ID is required'),
  startDate: z.string({ error: 'Start date is required' }).min(1, 'Start date is required'),
  endDate: z.string({error: 'End date is required' }).min(1, 'End date is required'),
  reason: z.string().max(500).optional(),
  entityType: z.number().optional(),
  assignedRole: z.string().max(100).optional(),
}).refine(
  (data) => new Date(data.endDate) > new Date(data.startDate),
  { message: 'End date must be after start date', path: ['endDate'] }
);

interface WorkflowDelegationFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
}

const entityTypeOptions = [
  { value: undefined, label: 'All Workflow Types' },
  ...Object.entries(WorkflowEntityTypeLabels).map(([value, label]) => ({
    value: Number(value),
    label,
  })),
];

const roleOptions = [
  { value: undefined, label: 'All Roles' },
  { value: 'Admin', label: 'Admin' },
  { value: 'Principal', label: 'Principal' },
  { value: 'VicePrincipal', label: 'Vice Principal' },
  { value: 'HOD', label: 'HOD' },
  { value: 'Teacher', label: 'Teacher' },
  { value: 'Finance', label: 'Finance' },
];

export const WorkflowDelegationFormModal: React.FC<WorkflowDelegationFormModalProps> = ({
  open,
  onClose,
}) => {
  const [form] = Form.useForm();
  const { createAsync } = useWorkflowDelegationActions();
  const [loading, setLoading] = React.useState(false);

  useEffect(() => {
    if (open) {
      form.resetFields();
    }
  }, [open, form]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();

      const toDateString = (val: unknown): string => {
        if (!val) return '';
        if (typeof val === 'string') return val;
        if (typeof val === 'object' && val !== null && 'format' in val && typeof (val as any).format === 'function') {
          return (val as any).format('YYYY-MM-DD');
        }
        return String(val);
      };

      const input = {
        delegateUserId: values.delegateUserId,
        startDate: toDateString(values.startDate),
        endDate: toDateString(values.endDate),
        reason: values.reason,
        entityType: values.entityType,
        assignedRole: values.assignedRole,
      };

      const result = delegationSchema.safeParse(input);
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
      message.success('Delegation created successfully');
      onClose(true);
    } catch {
      // Axios interceptor handles ABP error display
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title="New Delegation"
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
      width={600}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Delegate User ID" name="delegateUserId" rules={[{ required: true }]}>
          <InputNumber min={1} style={{ width: '100%' }} placeholder="User ID to delegate to" />
        </Form.Item>
        <div style={{ display: 'flex', gap: 16 }}>
          <Form.Item label="Start Date" name="startDate" rules={[{ required: true }]} style={{ flex: 1 }}>
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item label="End Date" name="endDate" rules={[{ required: true }]} style={{ flex: 1 }}>
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>
        </div>
        <Form.Item label="Reason" name="reason">
          <Input.TextArea rows={2} maxLength={500} placeholder="e.g. On leave, Conference" />
        </Form.Item>
        <div style={{ display: 'flex', gap: 16 }}>
          <Form.Item label="Scope: Entity Type" name="entityType" style={{ flex: 1 }}>
            <Select options={entityTypeOptions} placeholder="All types" allowClear />
          </Form.Item>
          <Form.Item label="Scope: Role" name="assignedRole" style={{ flex: 1 }}>
            <Select options={roleOptions} placeholder="All roles" allowClear />
          </Form.Item>
        </div>
      </Form>
    </Modal>
  );
};
