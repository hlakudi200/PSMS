'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, InputNumber, DatePicker, message } from 'antd';
import { z } from 'zod';
import { useFieldTripActions } from '@/providers/activities/field-trips';
import type { IFieldTrip } from '@/providers/activities/field-trips/context';
import dayjs from 'dayjs';

const fieldTripSchema = z.object({
  tripName: z.string().min(1, 'Trip name is required').max(200),
  academicYearId: z.string().min(1, 'Academic year is required'),
  organizingTeacherId: z.string().min(1, 'Organizing teacher is required'),
  destination: z.string().min(1, 'Destination is required').max(500),
  tripDate: z.string().min(1, 'Trip date is required'),
  estimatedCost: z.number().min(0, 'Estimated cost must be 0 or greater'),
  numberOfStudents: z.number().min(1, 'Number of students is required'),
  numberOfChaperones: z.number().min(1, 'Number of chaperones is required'),
});

interface FieldTripFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IFieldTrip | null;
  teachers?: { value: string; label: string }[];
  academicYears?: { value: string; label: string }[];
}

export const FieldTripFormModal: React.FC<FieldTripFormModalProps> = ({
  open,
  onClose,
  editRecord,
  teachers = [],
  academicYears = [],
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useFieldTripActions();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;

  useEffect(() => {
    if (open) {
      if (editRecord) {
        form.setFieldsValue({
          tripName: editRecord.tripName,
          academicYearId: editRecord.academicYearId,
          organizingTeacherId: editRecord.organizingTeacherId,
          destination: editRecord.destination,
          tripDate: editRecord.tripDate ? dayjs(editRecord.tripDate) : undefined,
          estimatedCost: editRecord.estimatedCost,
          numberOfStudents: editRecord.numberOfStudents,
          numberOfChaperones: editRecord.numberOfChaperones,
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
        tripDate: values.tripDate ? dayjs(values.tripDate).format('YYYY-MM-DD') : undefined,
      };
      const result = fieldTripSchema.safeParse(payload);
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
      message.success(`Field trip ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      // Axios interceptor handles ABP error display
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit Field Trip' : 'New Field Trip'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
      width={600}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Trip Name" name="tripName" rules={[{ required: true }]}>
          <Input placeholder="e.g. Science Museum Visit" maxLength={200} />
        </Form.Item>
        <Form.Item label="Academic Year" name="academicYearId" rules={[{ required: true }]}>
          <Select options={academicYears} placeholder="Select academic year" />
        </Form.Item>
        <Form.Item label="Organizing Teacher" name="organizingTeacherId" rules={[{ required: true }]}>
          <Select
            options={teachers}
            placeholder="Select teacher"
            showSearch
            filterOption={(input, option) =>
              (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
            }
          />
        </Form.Item>
        <Form.Item label="Destination" name="destination" rules={[{ required: true }]}>
          <Input.TextArea rows={2} maxLength={500} placeholder="Trip destination" />
        </Form.Item>
        <Form.Item label="Trip Date" name="tripDate" rules={[{ required: true }]}>
          <DatePicker style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item label="Estimated Cost" name="estimatedCost" rules={[{ required: true }]}>
          <InputNumber
            style={{ width: '100%' }}
            min={0}
            precision={2}
            prefix="R"
            placeholder="0.00"
          />
        </Form.Item>
        <Form.Item label="Number of Students" name="numberOfStudents" rules={[{ required: true }]}>
          <InputNumber style={{ width: '100%' }} min={1} placeholder="0" />
        </Form.Item>
        <Form.Item label="Number of Chaperones" name="numberOfChaperones" rules={[{ required: true }]}>
          <InputNumber style={{ width: '100%' }} min={1} placeholder="0" />
        </Form.Item>
      </Form>
    </Modal>
  );
};
