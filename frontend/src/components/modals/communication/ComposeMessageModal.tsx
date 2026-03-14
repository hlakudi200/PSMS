'use client';

import React, { useEffect, useState, useCallback } from 'react';
import { Modal, Form, Input, Select, message } from 'antd';
import { z } from 'zod';
import { useMessageActions } from '@/providers/communication/messages';
import { getAxiosInstance } from '@/utils/axios-instance';
import type { IAdminUser } from '@/providers/admin/shared/interfaces';

const composeSchema = z.object({
  recipientUserId: z.number({ error: 'Recipient is required' }),
  subject: z.string().optional(),
  content: z.string().min(1, 'Message body is required'),
  attachmentUrl: z.string().url('Must be a valid URL').optional().or(z.literal('')),
});

interface ComposeMessageModalProps {
  open: boolean;
  onClose: (refresh?: boolean) => void;
  replyTo?: {
    recipientUserId: number;
    recipientName?: string;
    subject?: string;
    parentMessageId?: string;
  };
}

export const ComposeMessageModal: React.FC<ComposeMessageModalProps> = ({
  open,
  onClose,
  replyTo,
}) => {
  const [form] = Form.useForm();
  const { sendAsync } = useMessageActions();
  const [loading, setLoading] = useState(false);
  const [users, setUsers] = useState<IAdminUser[]>([]);
  const [searchLoading, setSearchLoading] = useState(false);
  const isReply = !!replyTo;

  const fetchUsers = useCallback(async (keyword?: string) => {
    setSearchLoading(true);
    try {
      const instance = getAxiosInstance();
      const params = new URLSearchParams();
      params.append('MaxResultCount', '20');
      params.append('IsActive', 'true');
      if (keyword) params.append('Keyword', keyword);
      const response = await instance.get(`/api/services/app/User/GetAll?${params.toString()}`);
      setUsers(response.data.result.items ?? []);
    } catch {
      // silently fail - users won't be populated
    } finally {
      setSearchLoading(false);
    }
  }, []);

  useEffect(() => {
    if (open) {
      fetchUsers();
      if (replyTo) {
        form.setFieldsValue({
          recipientUserId: replyTo.recipientUserId,
          subject: replyTo.subject ? `Re: ${replyTo.subject.replace(/^Re:\s*/i, '')}` : undefined,
        });
      } else {
        form.resetFields();
      }
    }
  }, [open, replyTo, form, fetchUsers]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();
      const parsed = {
        recipientUserId: values.recipientUserId,
        subject: values.subject || undefined,
        content: values.content || '',
        attachmentUrl: values.attachmentUrl || undefined,
      };

      const result = composeSchema.safeParse(parsed);
      if (!result.success) {
        const fieldErrors = result.error.issues.map(err => ({
          name: err.path as string[],
          errors: [err.message],
        }));
        form.setFields(fieldErrors);
        return;
      }

      setLoading(true);
      await sendAsync({
        recipientUserId: result.data.recipientUserId,
        subject: result.data.subject,
        content: result.data.content,
        attachmentUrl: result.data.attachmentUrl || undefined,
        parentMessageId: replyTo?.parentMessageId,
      });
      message.success(isReply ? 'Reply sent' : 'Message sent');
      onClose(true);
    } catch {
      message.error('Failed to send message');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isReply ? 'Reply' : 'New Message'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
      width={520}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="To" name="recipientUserId" rules={[{ required: true }]}>
          <Select
            showSearch
            placeholder="Search for a recipient..."
            filterOption={false}
            onSearch={(val) => fetchUsers(val)}
            loading={searchLoading}
            disabled={isReply}
            options={users.map(u => ({
              value: u.id,
              label: `${u.fullName || `${u.name} ${u.surname}`} (${u.roleNames?.join(', ') || 'User'})`,
            }))}
          />
        </Form.Item>
        <Form.Item label="Subject" name="subject">
          <Input placeholder="Optional subject" maxLength={200} />
        </Form.Item>
        <Form.Item label="Message" name="content" rules={[{ required: true }]}>
          <Input.TextArea rows={5} placeholder="Type your message..." maxLength={5000} showCount />
        </Form.Item>
        <Form.Item label="Attachment URL" name="attachmentUrl">
          <Input placeholder="https://..." />
        </Form.Item>
      </Form>
    </Modal>
  );
};
