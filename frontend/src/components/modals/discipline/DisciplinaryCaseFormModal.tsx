'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, DatePicker, message } from 'antd';
import { z } from 'zod';
import { useDisciplinaryCaseActions } from '@/providers/discipline/disciplinary-cases';
import type { IDisciplinaryCase } from '@/providers/discipline/disciplinary-cases/context';
import dayjs from 'dayjs';

const disciplinaryCaseSchema = z.object({
  studentId: z.string().min(1, 'Student is required'),
  academicYearId: z.string().min(1, 'Academic year is required'),
  incidentDate: z.string().min(1, 'Incident date is required'),
  incidentDescription: z.string().min(1, 'Description is required').max(2000),
  incidentCategory: z.number().min(1, 'Category is required'),
  severity: z.number().min(1, 'Severity is required'),
});

interface DisciplinaryCaseFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IDisciplinaryCase | null;
  students?: { value: string; label: string }[];
  academicYears?: { value: string; label: string }[];
}

const categoryOptions = [
  { value: 1, label: 'Misconduct' },
  { value: 2, label: 'Bullying' },
  { value: 3, label: 'Violence' },
  { value: 4, label: 'Substance Abuse' },
  { value: 5, label: 'Property Damage' },
  { value: 6, label: 'Truancy' },
  { value: 7, label: 'Academic Dishonesty' },
  { value: 8, label: 'Harassment' },
  { value: 9, label: 'Dress Code' },
  { value: 10, label: 'Other' },
];

const severityOptions = [
  { value: 1, label: 'Minor' },
  { value: 2, label: 'Moderate' },
  { value: 3, label: 'Serious' },
  { value: 4, label: 'Very Serious' },
];

export const DisciplinaryCaseFormModal: React.FC<DisciplinaryCaseFormModalProps> = ({
  open,
  onClose,
  editRecord,
  students = [],
  academicYears = [],
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useDisciplinaryCaseActions();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;

  useEffect(() => {
    if (open) {
      if (editRecord) {
        form.setFieldsValue({
          studentId: editRecord.studentId,
          academicYearId: editRecord.academicYearId,
          incidentDate: editRecord.incidentDate ? dayjs(editRecord.incidentDate) : undefined,
          incidentDescription: editRecord.incidentDescription,
          incidentCategory: editRecord.incidentCategory,
          severity: editRecord.severity,
        });
      } else {
        form.resetFields();
      }
    }
  }, [open, editRecord, form]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();
      const payload = {
        ...values,
        incidentDate: values.incidentDate ? dayjs(values.incidentDate).format('YYYY-MM-DD') : undefined,
      };
      const result = disciplinaryCaseSchema.safeParse(payload);
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
      message.success(`Disciplinary case ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      // Axios interceptor handles ABP error display
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit Disciplinary Case' : 'New Disciplinary Case'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Student" name="studentId" rules={[{ required: true }]}>
          <Select
            options={students}
            placeholder="Select student"
            showSearch
            filterOption={(input, option) =>
              (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
            }
          />
        </Form.Item>
        <Form.Item label="Academic Year" name="academicYearId" rules={[{ required: true }]}>
          <Select options={academicYears} placeholder="Select academic year" />
        </Form.Item>
        <Form.Item label="Incident Date" name="incidentDate" rules={[{ required: true }]}>
          <DatePicker style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item label="Category" name="incidentCategory" rules={[{ required: true }]}>
          <Select options={categoryOptions} placeholder="Select category" />
        </Form.Item>
        <Form.Item label="Severity" name="severity" rules={[{ required: true }]}>
          <Select options={severityOptions} placeholder="Select severity" />
        </Form.Item>
        <Form.Item label="Description" name="incidentDescription" rules={[{ required: true }]}>
          <Input.TextArea rows={4} maxLength={2000} placeholder="Describe the incident" />
        </Form.Item>
      </Form>
    </Modal>
  );
};
