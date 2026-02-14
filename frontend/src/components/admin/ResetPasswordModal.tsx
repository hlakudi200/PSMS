'use client';

import React from 'react';
import { Modal, Form, Input, message } from 'antd';
import { z } from 'zod';
import { useUserActions } from '@/providers/admin/users';

const resetPasswordSchema = z.object({
  adminPassword: z.string().min(1, 'Admin password is required'),
  newPassword: z.string().min(6, 'Min 6 characters'),
  confirmPassword: z.string().min(1, 'Confirm password is required'),
}).refine(d => d.newPassword === d.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
});

interface ResetPasswordModalProps {
  open: boolean;
  onClose: () => void;
  userId: number | null;
  userName?: string;
}

export const ResetPasswordModal: React.FC<ResetPasswordModalProps> = ({
  open,
  onClose,
  userId,
  userName,
}) => {
  const [form] = Form.useForm();
  const { resetPasswordAsync } = useUserActions();
  const [loading, setLoading] = React.useState(false);

  const handleSubmit = async () => {
    if (userId === null) return;

    try {
      const values = form.getFieldsValue();
      const result = resetPasswordSchema.safeParse(values);

      if (!result.success) {
        const fieldErrors = result.error.issues.map(err => ({
          name: err.path as string[],
          errors: [err.message],
        }));
        form.setFields(fieldErrors);
        return;
      }

      setLoading(true);
      await resetPasswordAsync({
        adminPassword: result.data.adminPassword,
        userId,
        newPassword: result.data.newPassword,
      });
      message.success('Password reset successfully');
      form.resetFields();
      onClose();
    } catch {
      message.error('An error occurred');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={`Reset Password${userName ? ` - ${userName}` : ''}`}
      open={open}
      onCancel={() => { form.resetFields(); onClose(); }}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Your Admin Password" name="adminPassword" rules={[{ required: true }]}>
          <Input.Password placeholder="Enter your admin password" />
        </Form.Item>
        <Form.Item label="New Password" name="newPassword" rules={[{ required: true }]}>
          <Input.Password placeholder="Min 6 characters" />
        </Form.Item>
        <Form.Item label="Confirm Password" name="confirmPassword" rules={[{ required: true }]}>
          <Input.Password placeholder="Confirm new password" />
        </Form.Item>
      </Form>
    </Modal>
  );
};
