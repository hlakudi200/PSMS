'use client';

import React, { useEffect, useState } from 'react';
import { Modal, Form, Select, message } from 'antd';
import { z } from 'zod';
import { useClassActions } from '@/providers/academic/classes';
import { useTeacherState, useTeacherActions } from '@/providers/academic/teachers';
import type { IClass } from '@/providers/academic/shared/interfaces';

const assignTeacherSchema = z.object({
  teacherId: z.string().min(1, 'Please select a teacher'),
});

interface AssignTeacherModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  classRecord: IClass | null;
}

export const AssignTeacherModal: React.FC<AssignTeacherModalProps> = ({
  open,
  onClose,
  classRecord,
}) => {
  const [form] = Form.useForm();
  const { assignClassTeacherAsync, removeClassTeacherAsync } = useClassActions();
  const { teachers } = useTeacherState();
  const { getAllAsync: getAllTeachersAsync } = useTeacherActions();
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (open) {
      // Raised from 100 to 500 to accommodate larger schools.
      // For >500 staff use the remote-search variant (see ticket T-212).
      getAllTeachersAsync({ maxResultCount: 500 });
      form.setFieldsValue({
        teacherId: classRecord?.classTeacherId || undefined,
      });
    }
  }, [open, classRecord]); // eslint-disable-line react-hooks/exhaustive-deps

  const handleSubmit = async () => {
    if (!classRecord) return;

    try {
      const values = form.getFieldsValue();
      const selectedTeacherId = values.teacherId || null;
      const currentTeacherId = classRecord.classTeacherId || null;

      // No change
      if (selectedTeacherId === currentTeacherId) {
        onClose();
        return;
      }

      // Remove teacher (cleared selection)
      if (!selectedTeacherId && currentTeacherId) {
        setLoading(true);
        await removeClassTeacherAsync(classRecord.id);
        message.success('Class teacher removed');
        onClose(true);
        return;
      }

      // Assign or change teacher
      if (selectedTeacherId) {
        const result = assignTeacherSchema.safeParse({ teacherId: selectedTeacherId });
        if (!result.success) {
          form.setFields(result.error.issues.map(err => ({
            name: err.path as string[],
            errors: [err.message],
          })));
          return;
        }
        setLoading(true);
        await assignClassTeacherAsync(classRecord.id, result.data.teacherId);
        message.success('Class teacher assigned');
        onClose(true);
      }
    } catch {
      // Server errors are surfaced by the axios response interceptor.
    } finally {
      setLoading(false);
    }
  };

  const teacherOptions = (teachers ?? []).map(t => ({
    value: t.id,
    label: t.fullName,
  }));

  return (
    <Modal
      title={`Assign Teacher — ${classRecord?.className ?? ''}`}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Class Teacher" name="teacherId">
          <Select
            options={teacherOptions}
            placeholder="Select a teacher"
            showSearch
            optionFilterProp="label"
            allowClear
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};
