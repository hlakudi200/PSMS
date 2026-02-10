'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, Switch, Row, Col, message } from 'antd';
import { z } from 'zod';
import { useUserActions, useUserState } from '@/providers/admin/users';
import type { IAdminUser } from '@/providers/admin/shared/interfaces';

const userCreateSchema = z.object({
  userName: z.string().min(1, 'Username is required').max(256),
  name: z.string().min(1, 'First name is required').max(64),
  surname: z.string().min(1, 'Surname is required').max(64),
  emailAddress: z.string().email('Invalid email address'),
  password: z.string().min(6, 'Min 6 characters'),
  isActive: z.boolean().default(true),
  roleNames: z.array(z.string()).min(1, 'At least one role required'),
});

const userUpdateSchema = userCreateSchema.omit({ password: true });

interface UserFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IAdminUser | null;
}

export const UserFormModal: React.FC<UserFormModalProps> = ({
  open,
  onClose,
  editRecord,
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync, getRolesAsync } = useUserActions();
  const { roles } = useUserState();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;

  useEffect(() => {
    if (open) {
      getRolesAsync();
      if (editRecord) {
        form.setFieldsValue({
          userName: editRecord.userName,
          name: editRecord.name,
          surname: editRecord.surname,
          emailAddress: editRecord.emailAddress,
          isActive: editRecord.isActive,
          roleNames: editRecord.roleNames,
        });
      } else {
        form.resetFields();
        form.setFieldsValue({ isActive: true });
      }
    }
  }, [open, editRecord, form, getRolesAsync]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();
      const schema = isEdit ? userUpdateSchema : userCreateSchema;
      const result = schema.safeParse(values);

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
        await createAsync(result.data as z.infer<typeof userCreateSchema>);
      }
      message.success(`User ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      message.error('An error occurred');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit User' : 'New User'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
      width={640}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item label="Username" name="userName" rules={[{ required: true }]}>
              <Input placeholder="Username" maxLength={256} />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label="Email" name="emailAddress" rules={[{ required: true }]}>
              <Input placeholder="email@example.com" />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item label="First Name" name="name" rules={[{ required: true }]}>
              <Input placeholder="First Name" maxLength={64} />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label="Surname" name="surname" rules={[{ required: true }]}>
              <Input placeholder="Surname" maxLength={64} />
            </Form.Item>
          </Col>
        </Row>
        {!isEdit && (
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item label="Password" name="password" rules={[{ required: true }]}>
                <Input.Password placeholder="Min 6 characters" />
              </Form.Item>
            </Col>
          </Row>
        )}
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item label="Roles" name="roleNames" rules={[{ required: true }]}>
              <Select
                mode="multiple"
                placeholder="Select roles"
                options={(roles ?? []).map(r => ({
                  label: r.displayName,
                  value: r.name,
                }))}
              />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label="Active" name="isActive" valuePropName="checked">
              <Switch />
            </Form.Item>
          </Col>
        </Row>
      </Form>
    </Modal>
  );
};
