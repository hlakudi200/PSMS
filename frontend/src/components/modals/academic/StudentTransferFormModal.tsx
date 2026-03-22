'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, message } from 'antd';
import { z } from 'zod';
import { useStudentTransferActions } from '@/providers/academic/student-transfers';
import type { IStudentTransfer } from '@/providers/academic/student-transfers/context';

const studentTransferSchema = z.object({
  studentId: z.string().min(1, 'Student is required'),
  academicYearId: z.string().min(1, 'Academic year is required'),
  transferType: z.number().min(1, 'Transfer type is required'),
  reason: z.string().min(1, 'Reason is required').max(1000),
});

interface StudentTransferFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IStudentTransfer | null;
  students?: { value: string; label: string }[];
  academicYears?: { value: string; label: string }[];
}

const transferTypeOptions = [
  { value: 1, label: 'Transfer In' },
  { value: 2, label: 'Transfer Out' },
];

export const StudentTransferFormModal: React.FC<StudentTransferFormModalProps> = ({
  open,
  onClose,
  editRecord,
  students = [],
  academicYears = [],
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useStudentTransferActions();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;

  useEffect(() => {
    if (open) {
      if (editRecord) {
        form.setFieldsValue({
          studentId: editRecord.studentId,
          academicYearId: editRecord.academicYearId,
          transferType: editRecord.transferType,
          reason: editRecord.reason,
        });
      } else {
        form.resetFields();
      }
    }
  }, [open, editRecord, form]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();
      const result = studentTransferSchema.safeParse(values);
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
      message.success(`Transfer ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      // Axios interceptor handles ABP error display
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit Student Transfer' : 'New Student Transfer'}
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
        <Form.Item label="Transfer Type" name="transferType" rules={[{ required: true }]}>
          <Select options={transferTypeOptions} placeholder="Select transfer type" />
        </Form.Item>
        <Form.Item label="Reason" name="reason" rules={[{ required: true }]}>
          <Input.TextArea rows={4} maxLength={1000} placeholder="Reason for transfer" />
        </Form.Item>
      </Form>
    </Modal>
  );
};
