'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, InputNumber, DatePicker, message } from 'antd';
import { z } from 'zod';
import { useExpenseRequestActions } from '@/providers/financial/expense-requests';
import type { IExpenseRequest } from '@/providers/financial/expense-requests/context';
import dayjs from 'dayjs';

const expenseRequestSchema = z.object({
  academicYearId: z.string().min(1, 'Academic year is required'),
  category: z.number().min(1, 'Category is required'),
  description: z.string().min(1, 'Description is required').max(1000),
  amount: z.number().min(0.01, 'Amount must be greater than 0'),
  priority: z.number().min(1, 'Priority is required'),
  requiredByDate: z.string().optional(),
});

interface ExpenseRequestFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IExpenseRequest | null;
  academicYears?: { value: string; label: string }[];
}

const categoryOptions = [
  { value: 1, label: 'Stationery' },
  { value: 2, label: 'Textbooks' },
  { value: 3, label: 'Equipment' },
  { value: 4, label: 'Maintenance' },
  { value: 5, label: 'Technology' },
  { value: 6, label: 'Sports' },
  { value: 7, label: 'Cultural' },
  { value: 8, label: 'Transport' },
  { value: 9, label: 'Catering' },
  { value: 10, label: 'Training' },
  { value: 11, label: 'Other' },
];

const priorityOptions = [
  { value: 1, label: 'Low' },
  { value: 2, label: 'Medium' },
  { value: 3, label: 'High' },
  { value: 4, label: 'Urgent' },
];

export const ExpenseRequestFormModal: React.FC<ExpenseRequestFormModalProps> = ({
  open,
  onClose,
  editRecord,
  academicYears = [],
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useExpenseRequestActions();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;

  useEffect(() => {
    if (open) {
      if (editRecord) {
        form.setFieldsValue({
          academicYearId: editRecord.academicYearId,
          category: editRecord.category,
          description: editRecord.description,
          amount: editRecord.amount,
          priority: editRecord.priority,
          requiredByDate: editRecord.requiredByDate ? dayjs(editRecord.requiredByDate) : undefined,
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
        requiredByDate: values.requiredByDate ? dayjs(values.requiredByDate).format('YYYY-MM-DD') : undefined,
      };
      const result = expenseRequestSchema.safeParse(payload);
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
      message.success(`Expense request ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      // Axios interceptor handles ABP error display
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit Expense Request' : 'New Expense Request'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Academic Year" name="academicYearId" rules={[{ required: true }]}>
          <Select options={academicYears} placeholder="Select academic year" />
        </Form.Item>
        <Form.Item label="Category" name="category" rules={[{ required: true }]}>
          <Select options={categoryOptions} placeholder="Select category" />
        </Form.Item>
        <Form.Item label="Description" name="description" rules={[{ required: true }]}>
          <Input.TextArea rows={4} maxLength={1000} placeholder="Describe the expense" />
        </Form.Item>
        <Form.Item label="Amount" name="amount" rules={[{ required: true }]}>
          <InputNumber
            style={{ width: '100%' }}
            min={0.01}
            precision={2}
            prefix="R"
            placeholder="0.00"
          />
        </Form.Item>
        <Form.Item label="Priority" name="priority" rules={[{ required: true }]}>
          <Select options={priorityOptions} placeholder="Select priority" />
        </Form.Item>
        <Form.Item label="Required By Date" name="requiredByDate">
          <DatePicker style={{ width: '100%' }} />
        </Form.Item>
      </Form>
    </Modal>
  );
};
