'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, InputNumber, DatePicker, Select, message } from 'antd';
import { z } from 'zod';
import { useTeacherActions } from '@/providers/academic/teachers';
import type { ITeacher } from '@/providers/academic/shared/interfaces';
import dayjs from 'dayjs';

const teacherSchema = z.object({
  userId: z.number().min(1, 'User ID is required'),
  firstName: z.string().min(1, 'First name is required').max(100),
  lastName: z.string().min(1, 'Last name is required').max(100),
  middleName: z.string().max(100).optional(),
  employeeNumber: z.string().min(1, 'Employee number is required').max(50),
  email: z.string().email('Invalid email'),
  phone: z.string().min(1, 'Phone is required').max(20),
  dateOfJoining: z.string().optional(),
  qualifications: z.string().max(500).optional(),
  qualifiedSubjects: z.string().max(500).optional(),
  employmentStatus: z.string().max(50).optional(),
});

interface TeacherFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: ITeacher | null;
}

const employmentStatusOptions = [
  { value: 'Permanent', label: 'Permanent' },
  { value: 'Contract', label: 'Contract' },
  { value: 'Temporary', label: 'Temporary' },
  { value: 'Substitute', label: 'Substitute' },
];

export const TeacherFormModal: React.FC<TeacherFormModalProps> = ({
  open,
  onClose,
  editRecord,
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useTeacherActions();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;

  useEffect(() => {
    if (open) {
      if (editRecord) {
        form.setFieldsValue({
          userId: editRecord.userId,
          firstName: editRecord.firstName,
          lastName: editRecord.lastName,
          middleName: editRecord.middleName,
          employeeNumber: editRecord.employeeNumber,
          email: editRecord.email,
          phone: editRecord.phone,
          dateOfJoining: editRecord.dateOfJoining ? dayjs(editRecord.dateOfJoining) : null,
          qualifications: editRecord.qualifications,
          qualifiedSubjects: editRecord.qualifiedSubjects,
          employmentStatus: editRecord.employmentStatus,
        });
      } else {
        form.resetFields();
      }
    }
  }, [open, editRecord, form]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();
      const parsed = {
        ...values,
        middleName: values.middleName || undefined,
        dateOfJoining: values.dateOfJoining ? dayjs(values.dateOfJoining).format('YYYY-MM-DD') : undefined,
        qualifications: values.qualifications || undefined,
        qualifiedSubjects: values.qualifiedSubjects || undefined,
        employmentStatus: values.employmentStatus || undefined,
      };

      if (isEdit) {
        const { userId, employeeNumber, ...updateData } = parsed;
        setLoading(true);
        await updateAsync(editRecord!.id, updateData);
      } else {
        const result = teacherSchema.safeParse(parsed);
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
      message.success(`Teacher ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      // Server errors are surfaced by the axios response interceptor.
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit Teacher' : 'New Teacher'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
      width={640}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <div className="psms-form-grid">
          <Form.Item label="First Name" name="firstName" rules={[{ required: true }]}>
            <Input placeholder="First name" maxLength={100} />
          </Form.Item>
          <Form.Item label="Last Name" name="lastName" rules={[{ required: true }]}>
            <Input placeholder="Last name" maxLength={100} />
          </Form.Item>
          <Form.Item label="Middle Name" name="middleName">
            <Input placeholder="Middle name (optional)" maxLength={100} />
          </Form.Item>
          <Form.Item label="Employee Number" name="employeeNumber" rules={[{ required: !isEdit }]}>
            <Input placeholder="e.g. EMP001" maxLength={50} disabled={isEdit} />
          </Form.Item>
          <Form.Item label="Email" name="email" rules={[{ required: true, type: 'email' }]}>
            <Input placeholder="email@example.com" />
          </Form.Item>
          <Form.Item label="Phone" name="phone" rules={[{ required: true }]}>
            <Input placeholder="e.g. 0821234567" maxLength={20} />
          </Form.Item>
          {!isEdit && (
            <Form.Item label="User ID" name="userId" rules={[{ required: true }]}>
              <InputNumber min={1} style={{ width: '100%' }} placeholder="System user ID" />
            </Form.Item>
          )}
          <Form.Item label="Date of Joining" name="dateOfJoining">
            <DatePicker style={{ width: '100%' }} format="YYYY-MM-DD" />
          </Form.Item>
          <Form.Item label="Employment Status" name="employmentStatus">
            <Select
              options={employmentStatusOptions}
              placeholder="Select status"
              allowClear
            />
          </Form.Item>
        </div>
        <Form.Item label="Qualifications" name="qualifications">
          <Input.TextArea rows={2} maxLength={500} placeholder="e.g. B.Ed, PGCE" />
        </Form.Item>
        <Form.Item label="Qualified Subjects" name="qualifiedSubjects">
          <Input.TextArea rows={2} maxLength={500} placeholder="e.g. Mathematics, Physical Sciences" />
        </Form.Item>
      </Form>
    </Modal>
  );
};
