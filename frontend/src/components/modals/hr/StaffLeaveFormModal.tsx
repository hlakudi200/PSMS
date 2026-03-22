'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, DatePicker, message } from 'antd';
import { z } from 'zod';
import { useStaffLeaveRequestActions } from '@/providers/hr/staff-leave';
import type { IStaffLeaveRequest } from '@/providers/hr/staff-leave/context';
import dayjs from 'dayjs';

const staffLeaveSchema = z.object({
  leaveType: z.number().min(1, 'Leave type is required'),
  startDate: z.string().min(1, 'Start date is required'),
  endDate: z.string().min(1, 'End date is required'),
  reason: z.string().min(1, 'Reason is required').max(1000),
});

interface StaffLeaveFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IStaffLeaveRequest | null;
}

const leaveTypeOptions = [
  { value: 1, label: 'Annual' },
  { value: 2, label: 'Sick' },
  { value: 3, label: 'Family' },
  { value: 4, label: 'Maternity' },
  { value: 5, label: 'Paternity' },
  { value: 6, label: 'Study' },
  { value: 7, label: 'Compassionate' },
  { value: 8, label: 'Unpaid' },
  { value: 9, label: 'Other' },
];

export const StaffLeaveFormModal: React.FC<StaffLeaveFormModalProps> = ({
  open,
  onClose,
  editRecord,
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useStaffLeaveRequestActions();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;

  useEffect(() => {
    if (open) {
      if (editRecord) {
        form.setFieldsValue({
          leaveType: editRecord.leaveType,
          startDate: editRecord.startDate ? dayjs(editRecord.startDate) : undefined,
          endDate: editRecord.endDate ? dayjs(editRecord.endDate) : undefined,
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
      const payload = {
        ...values,
        startDate: values.startDate ? dayjs(values.startDate).format('YYYY-MM-DD') : undefined,
        endDate: values.endDate ? dayjs(values.endDate).format('YYYY-MM-DD') : undefined,
      };
      const result = staffLeaveSchema.safeParse(payload);
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
      message.success(`Leave request ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      // Axios interceptor handles ABP error display
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit Leave Request' : 'New Leave Request'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Leave Type" name="leaveType" rules={[{ required: true }]}>
          <Select options={leaveTypeOptions} placeholder="Select leave type" />
        </Form.Item>
        <Form.Item label="Start Date" name="startDate" rules={[{ required: true }]}>
          <DatePicker style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item label="End Date" name="endDate" rules={[{ required: true }]}>
          <DatePicker style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item label="Reason" name="reason" rules={[{ required: true }]}>
          <Input.TextArea rows={4} maxLength={1000} placeholder="Reason for leave request" />
        </Form.Item>
      </Form>
    </Modal>
  );
};
