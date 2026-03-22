'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, InputNumber, Select, message } from 'antd';
import { z } from 'zod';
import { useGradeActions } from '@/providers/academic/grades';
import type { IGradeList } from '@/providers/academic/shared/interfaces';

const gradeSchema = z.object({
  gradeName: z.string().min(1, 'Grade name is required').max(100),
  gradeLevel: z.number().min(0, 'Level must be 0-12').max(12, 'Level must be 0-12'),
  schoolPhase: z.number().min(1).max(4),
  description: z.string().max(500).optional(),
});

interface GradeFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IGradeList | null;
}

const phaseOptions = [
  { value: 1, label: 'Foundation' },
  { value: 2, label: 'Intermediate' },
  { value: 3, label: 'Senior' },
  { value: 4, label: 'FET' },
];

export const GradeFormModal: React.FC<GradeFormModalProps> = ({
  open,
  onClose,
  editRecord,
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useGradeActions();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;

  useEffect(() => {
    if (open) {
      if (editRecord) {
        form.setFieldsValue({
          gradeName: editRecord.gradeName,
          gradeLevel: editRecord.gradeLevel,
          schoolPhase: editRecord.schoolPhase,
          description: (editRecord as any).description,
        });
      } else {
        form.resetFields();
      }
    }
  }, [open, editRecord, form]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();
      const result = gradeSchema.safeParse(values);

      if (!result.success) {
        const fieldErrors = result.error.issues.map(err => ({
          name: err.path as string[],
          errors: [err.message],
        }));
        form.setFields(fieldErrors);
        return;
      }

      setLoading(true);
      if (isEdit) {
        await updateAsync(editRecord!.id, result.data);
      } else {
        await createAsync(result.data);
      }
      message.success(`Grade ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      message.error('An error occurred');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit Grade' : 'New Grade'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Grade Name" name="gradeName" rules={[{ required: true }]}>
          <Input placeholder="e.g. Grade 1" maxLength={100} />
        </Form.Item>
        <Form.Item label="Grade Level" name="gradeLevel" rules={[{ required: true }]}>
          <InputNumber min={0} max={12} style={{ width: '100%' }} placeholder="0-12" />
        </Form.Item>
        <Form.Item label="School Phase" name="schoolPhase" rules={[{ required: true }]}>
          <Select options={phaseOptions} placeholder="Select phase" />
        </Form.Item>
        <Form.Item label="Description" name="description">
          <Input.TextArea rows={3} maxLength={500} placeholder="Optional description" />
        </Form.Item>
      </Form>
    </Modal>
  );
};
