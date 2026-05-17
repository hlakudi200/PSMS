'use client';

import React, { useEffect, useState } from 'react';
import { Modal, Form, Input, InputNumber, Select, Alert, message } from 'antd';
import { z } from 'zod';
import { useClassActions } from '@/providers/academic/classes';
import { useGradeState, useGradeActions } from '@/providers/academic/grades';
import { useAcademicYearState, useAcademicYearActions } from '@/providers/academic/academic_years';
import type { IClass } from '@/providers/academic/shared/interfaces';

// SA classroom capacity norm: ≤35 for primary, ≤40 for secondary. We allow up
// to 100 (hard ceiling for unusual setups) but warn the user above 40.
const SOFT_CAPACITY_WARNING = 40;
const HARD_CAPACITY_LIMIT = 100;

const classSchema = z.object({
  className: z.string().min(1, 'Class name is required').max(100),
  gradeId: z.string().min(1, 'Grade is required'),
  academicYearId: z.string().min(1, 'Academic year is required'),
  maxCapacity: z
    .number()
    .min(1, 'Capacity must be at least 1')
    .max(HARD_CAPACITY_LIMIT, `Capacity cannot exceed ${HARD_CAPACITY_LIMIT}`),
});

const editSchema = z.object({
  className: z.string().min(1, 'Class name is required').max(100),
  maxCapacity: z
    .number()
    .min(1, 'Capacity must be at least 1')
    .max(HARD_CAPACITY_LIMIT, `Capacity cannot exceed ${HARD_CAPACITY_LIMIT}`),
});

interface ClassFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IClass | null;
}

export const ClassFormModal: React.FC<ClassFormModalProps> = ({
  open,
  onClose,
  editRecord,
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useClassActions();
  const { activeGrades } = useGradeState();
  const { getActiveGradesAsync } = useGradeActions();
  const { academicYears } = useAcademicYearState();
  const { getAllAsync: getAllAcademicYearsAsync } = useAcademicYearActions();
  const [loading, setLoading] = useState(false);
  const [capacityWarning, setCapacityWarning] = useState<string | null>(null);
  const isEdit = !!editRecord;

  useEffect(() => {
    if (open) {
      if (!isEdit) {
        getActiveGradesAsync();
        getAllAcademicYearsAsync({ maxResultCount: 100 });
      }

      if (editRecord) {
        form.setFieldsValue({
          className: editRecord.className,
          maxCapacity: editRecord.maxCapacity,
        });
      } else {
        form.resetFields();
      }
    }
  }, [open, editRecord]); // eslint-disable-line react-hooks/exhaustive-deps

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();

      if (isEdit) {
        const result = editSchema.safeParse(values);
        if (!result.success) {
          const fieldErrors = result.error.issues.map(err => ({
            name: err.path as string[],
            errors: [err.message],
          }));
          form.setFields(fieldErrors);
          return;
        }
        setLoading(true);
        await updateAsync(editRecord!.id, {
          className: result.data.className,
          maxCapacity: result.data.maxCapacity,
        });
      } else {
        const result = classSchema.safeParse(values);
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
      message.success(`Class ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      // Server errors are surfaced by the axios response interceptor.
    } finally {
      setLoading(false);
    }
  };

  const gradeOptions = (activeGrades ?? []).map(g => ({
    value: g.id,
    label: g.gradeName,
  }));

  const academicYearOptions = (academicYears ?? []).map(ay => ({
    value: ay.id,
    label: ay.yearName,
  }));

  return (
    <Modal
      title={isEdit ? 'Edit Class' : 'New Class'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Class Name" name="className" rules={[{ required: true }]}>
          <Input placeholder="e.g. 10A" maxLength={100} />
        </Form.Item>
        {!isEdit && (
          <>
            <Form.Item label="Grade" name="gradeId" rules={[{ required: true }]}>
              <Select
                options={gradeOptions}
                placeholder="Select grade"
                showSearch
                optionFilterProp="label"
              />
            </Form.Item>
            <Form.Item label="Academic Year" name="academicYearId" rules={[{ required: true }]}>
              <Select
                options={academicYearOptions}
                placeholder="Select academic year"
                showSearch
                optionFilterProp="label"
              />
            </Form.Item>
          </>
        )}
        <Form.Item label="Max Capacity" name="maxCapacity" rules={[{ required: true }]}>
          <InputNumber
            min={1}
            max={HARD_CAPACITY_LIMIT}
            style={{ width: '100%' }}
            placeholder="e.g. 35"
            onChange={(value) => {
              if (typeof value === 'number' && value > SOFT_CAPACITY_WARNING) {
                setCapacityWarning(
                  `SA classrooms typically hold no more than ${SOFT_CAPACITY_WARNING} learners. Confirm this is intentional.`
                );
              } else {
                setCapacityWarning(null);
              }
            }}
          />
        </Form.Item>
        {capacityWarning && (
          <Alert type="warning" showIcon message={capacityWarning} style={{ marginBottom: 12 }} />
        )}
      </Form>
    </Modal>
  );
};
