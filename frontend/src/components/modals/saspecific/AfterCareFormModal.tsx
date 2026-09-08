'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, InputNumber, TimePicker, Switch, message } from 'antd';
import { z } from 'zod';
import dayjs from 'dayjs';
import { useAfterCareActions, useAfterCareState } from '@/providers/saspecific/after_cares';
import type { IAfterCareList } from '@/providers/saspecific/shared/interfaces';

// Mirrors backend psms.Domain.Shared.Enums.AfterCareType.
export const AFTER_CARE_TYPE_OPTIONS = [
  { value: 1, label: 'Standard' },
  { value: 2, label: 'Extended' },
  { value: 3, label: 'Holiday Programme' },
];

const TIME_FORMAT = 'HH:mm';
/** Backend binds StartTime / EndTime to a TimeSpan, which serialises as HH:mm:ss. */
const TIMESPAN_FORMAT = 'HH:mm:ss';

const optionalText = (max: number) =>
  z.string().max(max).optional().or(z.literal('')).transform((v) => (v ? v : undefined));

const afterCareSchema = z
  .object({
    academicYearId: z.string().min(1, 'Academic year is required'),
    programName: z.string().min(1, 'Programme name is required').max(200),
    description: optionalText(2000),
    afterCareType: z.number().int().min(1, 'Programme type is required'),
    location: optionalText(200),
    startTime: z.string().regex(/^\d{2}:\d{2}:\d{2}$/, 'Start time is required'),
    endTime: z.string().regex(/^\d{2}:\d{2}:\d{2}$/, 'End time is required'),
    daysAvailable: optionalText(100),
    capacity: z.number().int().min(0, 'Capacity must be 0 or greater'),
    supervisorName: optionalText(100),
    contactPhone: optionalText(20),
    activitiesIncluded: optionalText(2000),
    includesMeals: z.boolean(),
    includesHomeworkSupervision: z.boolean(),
    monthlyFee: z.number().min(0, 'Monthly fee must be 0 or greater'),
  })
  .refine((v) => v.endTime > v.startTime, {
    message: 'End time must be after start time',
    path: ['endTime'],
  });

interface AfterCareFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IAfterCareList | null;
  academicYears?: { value: string; label: string }[];
}

export const AfterCareFormModal: React.FC<AfterCareFormModalProps> = ({
  open,
  onClose,
  editRecord,
  academicYears = [],
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync, getAsync } = useAfterCareActions();
  const { afterCare } = useAfterCareState();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;

  // The list DTO is trimmed; pull the full programme for editing.
  useEffect(() => {
    if (open && editRecord) getAsync(editRecord.id);
    if (open && !editRecord) {
      form.resetFields();
      form.setFieldsValue({ includesMeals: false, includesHomeworkSupervision: false, capacity: 0, monthlyFee: 0 });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, editRecord?.id]);

  useEffect(() => {
    if (!open || !editRecord || !afterCare || afterCare.id !== editRecord.id) return;
    form.setFieldsValue({
      academicYearId: afterCare.academicYearId,
      programName: afterCare.programName,
      description: afterCare.description,
      afterCareType: afterCare.afterCareType,
      location: afterCare.location,
      startTime: afterCare.startTime ? dayjs(afterCare.startTime, TIMESPAN_FORMAT) : undefined,
      endTime: afterCare.endTime ? dayjs(afterCare.endTime, TIMESPAN_FORMAT) : undefined,
      daysAvailable: afterCare.daysAvailable,
      capacity: afterCare.capacity,
      supervisorName: afterCare.supervisorName,
      contactPhone: afterCare.contactPhone,
      activitiesIncluded: afterCare.activitiesIncluded,
      includesMeals: afterCare.includesMeals,
      includesHomeworkSupervision: afterCare.includesHomeworkSupervision,
      monthlyFee: afterCare.monthlyFee,
    });
  }, [open, editRecord, afterCare, form]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();
      const payload = {
        ...values,
        startTime: values.startTime ? dayjs(values.startTime).format(TIMESPAN_FORMAT) : '',
        endTime: values.endTime ? dayjs(values.endTime).format(TIMESPAN_FORMAT) : '',
        includesMeals: !!values.includesMeals,
        includesHomeworkSupervision: !!values.includesHomeworkSupervision,
        capacity: values.capacity ?? 0,
        monthlyFee: values.monthlyFee ?? 0,
      };
      const result = afterCareSchema.safeParse(payload);
      if (!result.success) {
        form.setFields(
          result.error.issues.map((err) => ({
            name: err.path as string[],
            errors: [err.message],
          }))
        );
        return;
      }
      setLoading(true);
      if (isEdit) {
        // Academic year is fixed once a programme exists (enrolments hang off it).
        const { academicYearId: _ignored, ...update } = result.data;
        void _ignored;
        await updateAsync(editRecord!.id, update);
      } else {
        await createAsync(result.data);
      }
      message.success(`After care programme ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      // Axios interceptor handles ABP error display
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit After Care Programme' : 'New After Care Programme'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
      width={680}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
          <Form.Item label="Programme Name" name="programName" rules={[{ required: true }]}>
            <Input placeholder="e.g. Afternoon Care" maxLength={200} />
          </Form.Item>
          <Form.Item label="Academic Year" name="academicYearId" rules={[{ required: true }]}>
            <Select options={academicYears} placeholder="Select academic year" disabled={isEdit} />
          </Form.Item>
          <Form.Item label="Programme Type" name="afterCareType" rules={[{ required: true }]}>
            <Select options={AFTER_CARE_TYPE_OPTIONS} placeholder="Select type" />
          </Form.Item>
          <Form.Item label="Location" name="location">
            <Input placeholder="e.g. Hall B" maxLength={200} />
          </Form.Item>
          <Form.Item label="Start Time" name="startTime" rules={[{ required: true }]}>
            <TimePicker style={{ width: '100%' }} format={TIME_FORMAT} minuteStep={5} />
          </Form.Item>
          <Form.Item label="End Time" name="endTime" rules={[{ required: true }]}>
            <TimePicker style={{ width: '100%' }} format={TIME_FORMAT} minuteStep={5} />
          </Form.Item>
          <Form.Item label="Days Available" name="daysAvailable">
            <Input placeholder="e.g. Mon–Fri" maxLength={100} />
          </Form.Item>
          <Form.Item label="Capacity" name="capacity" rules={[{ required: true }]}>
            <InputNumber style={{ width: '100%' }} min={0} precision={0} />
          </Form.Item>
          <Form.Item label="Supervisor" name="supervisorName">
            <Input maxLength={100} />
          </Form.Item>
          <Form.Item label="Contact Phone" name="contactPhone">
            <Input maxLength={20} />
          </Form.Item>
          <Form.Item label="Monthly Fee" name="monthlyFee" rules={[{ required: true }]}>
            <InputNumber style={{ width: '100%' }} min={0} precision={2} prefix="R" />
          </Form.Item>
          <div style={{ display: 'flex', gap: 24, alignItems: 'end' }}>
            <Form.Item label="Includes Meals" name="includesMeals" valuePropName="checked">
              <Switch />
            </Form.Item>
            <Form.Item label="Homework Supervision" name="includesHomeworkSupervision" valuePropName="checked">
              <Switch />
            </Form.Item>
          </div>
        </div>
        <Form.Item label="Description" name="description">
          <Input.TextArea rows={2} maxLength={2000} />
        </Form.Item>
        <Form.Item label="Activities Included" name="activitiesIncluded">
          <Input.TextArea rows={2} maxLength={2000} placeholder="e.g. Sport, arts and crafts, reading" />
        </Form.Item>
      </Form>
    </Modal>
  );
};
