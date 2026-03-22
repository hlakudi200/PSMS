'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, InputNumber, message } from 'antd';
import { z } from 'zod';
import { useFeeWaiverActions } from '@/providers/financial/fee-waivers';
import type { IFeeWaiver } from '@/providers/financial/fee-waivers/context';

const feeWaiverSchema = z.object({
  studentId: z.string().min(1, 'Student is required'),
  academicYearId: z.string().min(1, 'Academic year is required'),
  waiverType: z.number().min(1, 'Waiver type is required'),
  requestedAmount: z.number().min(0.01, 'Amount must be greater than 0'),
  reason: z.string().min(1, 'Reason is required').max(1000),
});

interface FeeWaiverFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IFeeWaiver | null;
  students?: { value: string; label: string }[];
  academicYears?: { value: string; label: string }[];
}

const waiverTypeOptions = [
  { value: 1, label: 'Financial Hardship' },
  { value: 2, label: 'Sibling Discount' },
  { value: 3, label: 'Staff Discount' },
  { value: 4, label: 'Bursary' },
  { value: 5, label: 'Scholarship' },
  { value: 6, label: 'Other' },
];

export const FeeWaiverFormModal: React.FC<FeeWaiverFormModalProps> = ({
  open,
  onClose,
  editRecord,
  students = [],
  academicYears = [],
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useFeeWaiverActions();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;

  useEffect(() => {
    if (open) {
      if (editRecord) {
        form.setFieldsValue({
          studentId: editRecord.studentId,
          academicYearId: editRecord.academicYearId,
          waiverType: editRecord.waiverType,
          requestedAmount: editRecord.requestedAmount,
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
      const result = feeWaiverSchema.safeParse(values);
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
      message.success(`Fee waiver ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      // Axios interceptor handles ABP error display
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit Fee Waiver' : 'New Fee Waiver'}
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
        <Form.Item label="Waiver Type" name="waiverType" rules={[{ required: true }]}>
          <Select options={waiverTypeOptions} placeholder="Select waiver type" />
        </Form.Item>
        <Form.Item label="Requested Amount" name="requestedAmount" rules={[{ required: true }]}>
          <InputNumber
            style={{ width: '100%' }}
            min={0.01}
            precision={2}
            prefix="R"
            placeholder="0.00"
          />
        </Form.Item>
        <Form.Item label="Reason" name="reason" rules={[{ required: true }]}>
          <Input.TextArea rows={4} maxLength={1000} placeholder="Reason for fee waiver request" />
        </Form.Item>
      </Form>
    </Modal>
  );
};
