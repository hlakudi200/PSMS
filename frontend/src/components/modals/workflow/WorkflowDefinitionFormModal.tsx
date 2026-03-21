'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, Switch, message } from 'antd';
import { z } from 'zod';
import { useWorkflowDefinitionActions } from '@/providers/workflow/workflow-definitions';
import type { IWorkflowDefinitionList } from '@/providers/workflow/shared/interfaces';
import { WorkflowEntityType, WorkflowEntityTypeLabels } from '@/providers/workflow/shared/interfaces';

const definitionSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  description: z.string().max(1000).optional(),
  entityType: z.number().min(1, 'Entity type is required'),
  isActive: z.boolean(),
});

interface WorkflowDefinitionFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IWorkflowDefinitionList | null;
}

const entityTypeOptions = Object.entries(WorkflowEntityTypeLabels).map(([value, label]) => ({
  value: Number(value),
  label,
}));

export const WorkflowDefinitionFormModal: React.FC<WorkflowDefinitionFormModalProps> = ({
  open,
  onClose,
  editRecord,
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useWorkflowDefinitionActions();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;

  useEffect(() => {
    if (open) {
      if (editRecord) {
        form.setFieldsValue({
          name: editRecord.name,
          entityType: editRecord.entityType,
          description: (editRecord as any).description,
        });
      } else {
        form.resetFields();
        form.setFieldsValue({ isActive: false });
      }
    }
  }, [open, editRecord, form]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();
      if (isEdit) {
        const updateData = { name: values.name, description: values.description };
        setLoading(true);
        await updateAsync(editRecord!.id, updateData);
      } else {
        const result = definitionSchema.safeParse(values);
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
      message.success(`Workflow ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      // Axios interceptor handles ABP error display
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit Workflow Definition' : 'New Workflow Definition'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Name" name="name" rules={[{ required: true }]}>
          <Input placeholder="e.g. Admissions Approval" maxLength={200} />
        </Form.Item>
        <Form.Item label="Entity Type" name="entityType" rules={[{ required: true }]}>
          <Select
            options={entityTypeOptions}
            placeholder="Select entity type"
            disabled={isEdit}
          />
        </Form.Item>
        <Form.Item label="Description" name="description">
          <Input.TextArea rows={3} maxLength={1000} placeholder="Optional description" />
        </Form.Item>
        {!isEdit && (
          <Form.Item label="Active" name="isActive" valuePropName="checked">
            <Switch />
          </Form.Item>
        )}
      </Form>
    </Modal>
  );
};
