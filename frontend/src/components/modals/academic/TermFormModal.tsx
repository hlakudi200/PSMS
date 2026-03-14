'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, DatePicker, message } from 'antd';
import { z } from 'zod';
import { useTermActions } from '@/providers/academic/terms';
import type { ITerm } from '@/providers/academic/shared/interfaces';
import dayjs from 'dayjs';

const termSchema = z.object({
  termNumber: z.number().min(1).max(4, 'Term number must be 1-4'),
  termName: z.string().min(2, 'Term name must be at least 2 characters').max(50),
  startDate: z.string().min(1, 'Start date is required'),
  endDate: z.string().min(1, 'End date is required'),
}).refine(data => new Date(data.endDate) > new Date(data.startDate), {
  message: 'End date must be after start date',
  path: ['endDate'],
});

interface TermFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: ITerm | null;
  academicYearId: string;
  existingTermNumbers: number[];
}

export const TermFormModal: React.FC<TermFormModalProps> = ({
  open,
  onClose,
  editRecord,
  academicYearId,
  existingTermNumbers,
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useTermActions();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;

  const availableTermNumbers = isEdit
    ? [1, 2, 3, 4]
    : [1, 2, 3, 4].filter(n => !existingTermNumbers.includes(n));

  useEffect(() => {
    if (open) {
      if (editRecord) {
        form.setFieldsValue({
          termNumber: editRecord.termNumber,
          termName: editRecord.termName,
          startDate: editRecord.startDate ? dayjs(editRecord.startDate) : null,
          endDate: editRecord.endDate ? dayjs(editRecord.endDate) : null,
        });
      } else {
        form.resetFields();
        if (availableTermNumbers.length === 1) {
          const num = availableTermNumbers[0];
          form.setFieldsValue({
            termNumber: num,
            termName: `Term ${num}`,
          });
        }
      }
    }
  }, [open, editRecord, form, availableTermNumbers]);

  const handleTermNumberChange = (value: number) => {
    if (!isEdit) {
      form.setFieldsValue({ termName: `Term ${value}` });
    }
  };

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();

      const parsed = {
        termNumber: values.termNumber,
        termName: values.termName,
        startDate: values.startDate ? dayjs(values.startDate).format('YYYY-MM-DD') : '',
        endDate: values.endDate ? dayjs(values.endDate).format('YYYY-MM-DD') : '',
      };

      const result = termSchema.safeParse(parsed);
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
        await updateAsync(editRecord!.id, {
          termName: result.data.termName,
          startDate: result.data.startDate,
          endDate: result.data.endDate,
        });
      } else {
        await createAsync({
          academicYearId,
          termNumber: result.data.termNumber,
          termName: result.data.termName,
          startDate: result.data.startDate,
          endDate: result.data.endDate,
        });
      }
      message.success(`Term ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      message.error('An error occurred');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit Term' : 'New Term'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Term Number" name="termNumber" rules={[{ required: true }]}>
          <Select
            placeholder="Select term number"
            disabled={isEdit}
            onChange={handleTermNumberChange}
            options={availableTermNumbers.map(n => ({ value: n, label: `Term ${n}` }))}
          />
        </Form.Item>
        <Form.Item label="Term Name" name="termName" rules={[{ required: true }]}>
          <Input placeholder="e.g. Term 1" maxLength={50} />
        </Form.Item>
        <Form.Item label="Start Date" name="startDate" rules={[{ required: true }]}>
          <DatePicker style={{ width: '100%' }} format="YYYY-MM-DD" />
        </Form.Item>
        <Form.Item label="End Date" name="endDate" rules={[{ required: true }]}>
          <DatePicker style={{ width: '100%' }} format="YYYY-MM-DD" />
        </Form.Item>
      </Form>
    </Modal>
  );
};
