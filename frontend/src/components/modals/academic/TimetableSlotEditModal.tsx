'use client';

import React, { useEffect, useState } from 'react';
import { Modal, Form, Select, Input, TimePicker, Button, Space, Row, Col, Typography, message } from 'antd';
import { DeleteOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { z } from 'zod';
import { useSubjectState, useSubjectActions } from '@/providers/academic/subjects';
import { useTeacherState, useTeacherActions } from '@/providers/academic/teachers';
import {
  useTimetableSlotState,
  useTimetableSlotActions,
} from '@/providers/academic/timetable_slots';
import type { ITimetableSlotList } from '@/providers/academic/shared/interfaces';

const { Text } = Typography;

const TIME_FORMAT = 'HH:mm';
const TIMESPAN_FORMAT = 'HH:mm:ss';

/** dayOfWeek is 1-based from Monday, matching the backend and the grid. */
const DAY_NAMES = ['', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

const slotSchema = z
  .object({
    subjectId: z.string().min(1, 'Please select a subject'),
    teacherId: z.string().min(1, 'Please select a teacher'),
    startTime: z.string().min(1, 'Please select a start time'),
    endTime: z.string().min(1, 'Please select an end time'),
    roomNumber: z.string().optional(),
  })
  .refine((data) => data.startTime < data.endTime, {
    message: 'Start time must be before end time',
    path: ['endTime'],
  });

interface TimetableSlotEditModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  timetableId: string;
  /** Shown in the modal so the actor knows which class they are editing. */
  className?: string;
  dayOfWeek: number;
  periodNumber: number;
  /** Present when editing an existing slot; absent when filling an empty cell. */
  existingSlot?: ITimetableSlotList | null;
  /** Times the rest of this period runs at, used to prefill a new slot. */
  defaultStartTime?: string;
  defaultEndTime?: string;
}

export const TimetableSlotEditModal: React.FC<TimetableSlotEditModalProps> = ({
  open,
  onClose,
  timetableId,
  className,
  dayOfWeek,
  periodNumber,
  existingSlot,
  defaultStartTime,
  defaultEndTime,
}) => {
  const [form] = Form.useForm();
  const isEdit = !!existingSlot;

  const { subjects } = useSubjectState();
  const { getAllAsync: getAllSubjectsAsync } = useSubjectActions();
  const { teachers } = useTeacherState();
  const { getAllAsync: getAllTeachersAsync } = useTeacherActions();
  const { timetableSlot } = useTimetableSlotState();
  const { getAsync, createAsync, updateAsync, deleteAsync } = useTimetableSlotActions();

  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!open) return;
    getAllSubjectsAsync({ maxResultCount: 500 });
    getAllTeachersAsync({ maxResultCount: 500 });
    if (existingSlot) {
      getAsync(existingSlot.id);
    } else {
      form.resetFields();
      // Start a new slot on the same times the rest of the period runs at, so the
      // common case is one subject + one teacher away from done.
      form.setFieldsValue({
        startTime: defaultStartTime ? dayjs(defaultStartTime, TIMESPAN_FORMAT) : undefined,
        endTime: defaultEndTime ? dayjs(defaultEndTime, TIMESPAN_FORMAT) : undefined,
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, existingSlot?.id, defaultStartTime, defaultEndTime]);

  // Full slot detail (with subjectId/teacherId — the list DTO only has names)
  // arrives async after getAsync above resolves; populate once it matches.
  useEffect(() => {
    if (open && existingSlot && timetableSlot?.id === existingSlot.id) {
      form.setFieldsValue({
        subjectId: timetableSlot.subjectId,
        teacherId: timetableSlot.teacherId,
        startTime: dayjs(timetableSlot.startTime, TIMESPAN_FORMAT),
        endTime: dayjs(timetableSlot.endTime, TIMESPAN_FORMAT),
        roomNumber: timetableSlot.roomNumber,
      });
    }
  }, [open, existingSlot, timetableSlot]); // eslint-disable-line react-hooks/exhaustive-deps

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();
      const parsed = slotSchema.safeParse({
        subjectId: values.subjectId,
        teacherId: values.teacherId,
        startTime: values.startTime ? dayjs(values.startTime).format(TIME_FORMAT) : '',
        endTime: values.endTime ? dayjs(values.endTime).format(TIME_FORMAT) : '',
        roomNumber: values.roomNumber,
      });

      if (!parsed.success) {
        form.setFields(
          parsed.error.issues.map((err) => ({
            name: err.path as string[],
            errors: [err.message],
          }))
        );
        return;
      }

      setLoading(true);
      const startTime = `${parsed.data.startTime}:00`;
      const endTime = `${parsed.data.endTime}:00`;

      if (isEdit && existingSlot) {
        await updateAsync(existingSlot.id, {
          subjectId: parsed.data.subjectId,
          teacherId: parsed.data.teacherId,
          startTime,
          endTime,
          roomNumber: parsed.data.roomNumber,
        });
        message.success('Slot updated');
      } else {
        await createAsync({
          timetableId,
          dayOfWeek,
          periodNumber,
          startTime,
          endTime,
          subjectId: parsed.data.subjectId,
          teacherId: parsed.data.teacherId,
          roomNumber: parsed.data.roomNumber,
        });
        message.success('Slot created');
      }
      onClose(true);
    } catch {
      // Surfaced by the axios error interceptor (e.g. teacher conflict).
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = () => {
    if (!existingSlot) return;
    Modal.confirm({
      title: 'Remove this slot?',
      content: 'This cannot be undone.',
      okText: 'Remove',
      okButtonProps: { danger: true },
      onOk: async () => {
        await deleteAsync(existingSlot.id);
        message.success('Slot removed');
        onClose(true);
      },
    });
  };

  const subjectOptions = (subjects ?? []).map((s) => ({ value: s.id, label: s.subjectName }));
  const teacherOptions = (teachers ?? []).map((t) => ({ value: t.id, label: t.fullName }));

  return (
    <Modal
      title={isEdit ? 'Edit lesson' : 'Add lesson'}
      open={open}
      onCancel={() => onClose()}
      destroyOnClose
      footer={
        // Space is inline-flex, so margin-auto inside it cannot push Remove to the
        // far edge — the footer itself has to be the flex container.
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <span>
            {isEdit && (
              <Button danger icon={<DeleteOutlined />} onClick={handleDelete}>
                Remove
              </Button>
            )}
          </span>
          <Space>
            <Button onClick={() => onClose()}>Cancel</Button>
            <Button type="primary" onClick={handleSubmit} loading={loading}>
              {isEdit ? 'Save' : 'Add'}
            </Button>
          </Space>
        </div>
      }
    >
      <Text type="secondary" style={{ display: 'block', marginTop: 8 }}>
        {[className, DAY_NAMES[dayOfWeek], `period ${periodNumber}`].filter(Boolean).join(' · ')}
      </Text>

      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Subject" name="subjectId" rules={[{ required: true }]}>
          <Select options={subjectOptions} placeholder="Select subject" showSearch optionFilterProp="label" />
        </Form.Item>
        <Form.Item label="Teacher" name="teacherId" rules={[{ required: true }]}>
          <Select options={teacherOptions} placeholder="Select teacher" showSearch optionFilterProp="label" />
        </Form.Item>
        {/* Row/Col rather than Space.Compact: compact collapses the two labels. */}
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item label="Start time" name="startTime" rules={[{ required: true }]}>
              <TimePicker style={{ width: '100%' }} format={TIME_FORMAT} minuteStep={5} />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label="End time" name="endTime" rules={[{ required: true }]}>
              <TimePicker style={{ width: '100%' }} format={TIME_FORMAT} minuteStep={5} />
            </Form.Item>
          </Col>
        </Row>
        <Form.Item label="Room" name="roomNumber">
          <Input placeholder="e.g. B12" maxLength={50} />
        </Form.Item>
      </Form>
    </Modal>
  );
};
