'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, InputNumber, Select, message } from 'antd';
import { z } from 'zod';
import { useGradeActions } from '@/providers/academic/grades';
import type { IGrade, IGradeList } from '@/providers/academic/shared/interfaces';

// SA school phase mapping per business rule AR-005:
// Grade R-3 → Foundation; Grade 4-6 → Intermediate;
// Grade 7-9 → Senior; Grade 10-12 → FET.
const phaseForGradeLevel = (level: number): number => {
  if (level <= 3) return 1;       // Foundation
  if (level <= 6) return 2;       // Intermediate
  if (level <= 9) return 3;       // Senior
  return 4;                       // FET
};

const phaseLabels: Record<number, string> = {
  1: 'Foundation',
  2: 'Intermediate',
  3: 'Senior',
  4: 'FET',
};

const gradeSchema = z
  .object({
    gradeName: z.string().min(1, 'Grade name is required').max(100),
    gradeLevel: z.number().min(0, 'Level must be 0 (R) to 12').max(12, 'Level must be 0 (R) to 12'),
    schoolPhase: z.number().min(1).max(4),
    description: z.string().max(500).optional(),
  })
  .refine((d) => phaseForGradeLevel(d.gradeLevel) === d.schoolPhase, {
    message: 'School phase must match the grade level (auto-derived).',
    path: ['schoolPhase'],
  });

interface GradeFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  // Accept either the list DTO or the full DTO; description only exists on IGrade.
  editRecord?: IGradeList | IGrade | null;
}

const phaseOptions = Object.entries(phaseLabels).map(([value, label]) => ({
  value: Number(value),
  label,
}));

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
        // description only present on full IGrade
        const desc = 'description' in editRecord ? editRecord.description : undefined;
        form.setFieldsValue({
          gradeName: editRecord.gradeName,
          gradeLevel: editRecord.gradeLevel,
          schoolPhase: editRecord.schoolPhase,
          description: desc,
        });
      } else {
        form.resetFields();
      }
    }
  }, [open, editRecord, form]);

  // Keep schoolPhase in sync when the user changes the grade level.
  const handleGradeLevelChange = (value: number | null) => {
    if (value == null) return;
    form.setFieldsValue({ schoolPhase: phaseForGradeLevel(value) });
  };

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();
      // Always derive phase from level so the two cannot drift.
      const parsed = {
        ...values,
        schoolPhase:
          typeof values.gradeLevel === 'number'
            ? phaseForGradeLevel(values.gradeLevel)
            : values.schoolPhase,
      };
      const result = gradeSchema.safeParse(parsed);

      if (!result.success) {
        const fieldErrors = result.error.issues.map((err) => ({
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
      // Server errors are surfaced by the axios response interceptor.
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
          <InputNumber
            min={0}
            max={12}
            style={{ width: '100%' }}
            placeholder="0 (Grade R) to 12"
            onChange={(v) => handleGradeLevelChange(typeof v === 'number' ? v : null)}
          />
        </Form.Item>
        <Form.Item
          label="School Phase (auto-derived from level)"
          name="schoolPhase"
          rules={[{ required: true }]}
        >
          <Select options={phaseOptions} placeholder="Auto-derived" disabled />
        </Form.Item>
        <Form.Item label="Description" name="description">
          <Input.TextArea rows={3} maxLength={500} placeholder="Optional description" />
        </Form.Item>
      </Form>
    </Modal>
  );
};
