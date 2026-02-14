'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, InputNumber, DatePicker, Checkbox, message } from 'antd';
import { z } from 'zod';
import { useAcademicYearActions } from '@/providers/academic/academic_years';
import type { IAcademicYear } from '@/providers/academic/shared/interfaces';
import dayjs from 'dayjs';

const createSchema = z.object({
  year: z.number().min(2020, 'Year must be 2020+').max(2099, 'Year must be before 2100'),
  startDate: z.string().min(1, 'Start date is required'),
  endDate: z.string().min(1, 'End date is required'),
  createDefaultTerms: z.boolean(),
}).refine(data => new Date(data.endDate) > new Date(data.startDate), {
  message: 'End date must be after start date',
  path: ['endDate'],
});

const updateSchema = z.object({
  yearName: z.string().min(1, 'Year name is required').max(100),
  startDate: z.string().min(1, 'Start date is required'),
  endDate: z.string().min(1, 'End date is required'),
}).refine(data => new Date(data.endDate) > new Date(data.startDate), {
  message: 'End date must be after start date',
  path: ['endDate'],
});

interface AcademicYearFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IAcademicYear | null;
}

export const AcademicYearFormModal: React.FC<AcademicYearFormModalProps> = ({
  open,
  onClose,
  editRecord,
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useAcademicYearActions();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;

  useEffect(() => {
    if (open) {
      if (editRecord) {
        form.setFieldsValue({
          yearName: editRecord.yearName,
          startDate: editRecord.startDate ? dayjs(editRecord.startDate) : null,
          endDate: editRecord.endDate ? dayjs(editRecord.endDate) : null,
        });
      } else {
        form.resetFields();
      }
    }
  }, [open, editRecord, form]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();

      const parsed = {
        ...values,
        startDate: values.startDate ? dayjs(values.startDate).format('YYYY-MM-DD') : '',
        endDate: values.endDate ? dayjs(values.endDate).format('YYYY-MM-DD') : '',
      };

      if (isEdit) {
        const result = updateSchema.safeParse({
          yearName: parsed.yearName,
          startDate: parsed.startDate,
          endDate: parsed.endDate,
        });
        if (!result.success) {
          const fieldErrors = result.error.issues.map(err => ({
            name: err.path as string[],
            errors: [err.message],
          }));
          form.setFields(fieldErrors);
          return;
        }
        setLoading(true);
        await updateAsync(editRecord!.id, result.data);
      } else {
        const result = createSchema.safeParse({
          year: parsed.year,
          startDate: parsed.startDate,
          endDate: parsed.endDate,
          createDefaultTerms: parsed.createDefaultTerms ?? false,
        });
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
      message.success(`Academic year ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      message.error('An error occurred');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit Academic Year' : 'New Academic Year'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        {isEdit ? (
          <Form.Item label="Year Name" name="yearName" rules={[{ required: true }]}>
            <Input placeholder="e.g. 2025 Academic Year" maxLength={100} />
          </Form.Item>
        ) : (
          <Form.Item label="Year" name="year" rules={[{ required: true }]}>
            <InputNumber min={2020} max={2099} style={{ width: '100%' }} placeholder="e.g. 2025" />
          </Form.Item>
        )}
        <Form.Item label="Start Date" name="startDate" rules={[{ required: true }]}>
          <DatePicker style={{ width: '100%' }} format="YYYY-MM-DD" />
        </Form.Item>
        <Form.Item label="End Date" name="endDate" rules={[{ required: true }]}>
          <DatePicker style={{ width: '100%' }} format="YYYY-MM-DD" />
        </Form.Item>
        {!isEdit && (
          <Form.Item name="createDefaultTerms" valuePropName="checked" initialValue={false}>
            <Checkbox>Create default terms (4 terms)</Checkbox>
          </Form.Item>
        )}
      </Form>
    </Modal>
  );
};
