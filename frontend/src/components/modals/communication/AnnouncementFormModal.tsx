'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, DatePicker, Switch, message } from 'antd';
import { z } from 'zod';
import { useAnnouncementActions } from '@/providers/communication/announcements';
import type { IAnnouncementList } from '@/providers/communication/shared/interfaces';
import dayjs from 'dayjs';

const announcementSchema = z.object({
  title: z.string().min(1, 'Title is required').max(200),
  content: z.string().min(1, 'Content is required').max(5000),
  type: z.number().min(1).max(7),
  priority: z.number().min(1).max(4),
  targetAudience: z.number().min(1).max(7),
  targetGradeId: z.string().optional(),
  targetClassId: z.string().optional(),
  publishDate: z.string().optional(),
  expiryDate: z.string().optional(),
  sendEmailNotification: z.boolean(),
  sendPushNotification: z.boolean(),
});

const typeOptions = [
  { value: 1, label: 'General' },
  { value: 2, label: 'Academic' },
  { value: 3, label: 'Sports' },
  { value: 4, label: 'Event' },
  { value: 5, label: 'Emergency' },
  { value: 6, label: 'Holiday' },
  { value: 7, label: 'Administrative' },
];

const priorityOptions = [
  { value: 1, label: 'Low' },
  { value: 2, label: 'Normal' },
  { value: 3, label: 'High' },
  { value: 4, label: 'Urgent' },
];

const audienceOptions = [
  { value: 1, label: 'All (School-wide)' },
  { value: 2, label: 'Staff' },
  { value: 3, label: 'Teachers' },
  { value: 4, label: 'Parents' },
  { value: 5, label: 'Students' },
  { value: 6, label: 'Specific Grade' },
  { value: 7, label: 'Specific Class' },
];

interface AnnouncementFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IAnnouncementList | null;
}

export const AnnouncementFormModal: React.FC<AnnouncementFormModalProps> = ({
  open,
  onClose,
  editRecord,
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useAnnouncementActions();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;

  useEffect(() => {
    if (open) {
      if (editRecord) {
        form.setFieldsValue({
          title: editRecord.title,
          type: editRecord.type,
          priority: editRecord.priority,
          targetAudience: editRecord.targetAudience,
          publishDate: editRecord.publishDate ? dayjs(editRecord.publishDate) : null,
          expiryDate: editRecord.expiryDate ? dayjs(editRecord.expiryDate) : null,
        });
      } else {
        form.resetFields();
        form.setFieldsValue({
          type: 1,
          priority: 2,
          targetAudience: 1,
          sendEmailNotification: false,
          sendPushNotification: false,
        });
      }
    }
  }, [open, editRecord, form]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();

      const payload = {
        ...values,
        publishDate: values.publishDate ? dayjs(values.publishDate).format('YYYY-MM-DD') : undefined,
        expiryDate: values.expiryDate ? dayjs(values.expiryDate).format('YYYY-MM-DD') : undefined,
        sendEmailNotification: values.sendEmailNotification ?? false,
        sendPushNotification: values.sendPushNotification ?? false,
      };

      const result = announcementSchema.safeParse(payload);

      if (!result.success) {
        const fieldErrors = result.error.issues.map((err) => ({
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
        await createAsync(result.data as Parameters<typeof createAsync>[0]);
      }
      message.success(`Announcement ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      message.error('An error occurred');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit Announcement' : 'New Announcement'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
      width={640}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Title" name="title" rules={[{ required: true }]}>
          <Input placeholder="Announcement title" maxLength={200} />
        </Form.Item>

        {!isEdit && (
          <Form.Item label="Content" name="content" rules={[{ required: true }]}>
            <Input.TextArea rows={4} placeholder="Announcement content" maxLength={5000} />
          </Form.Item>
        )}

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0 16px' }}>
          <Form.Item label="Type" name="type" rules={[{ required: true }]}>
            <Select options={typeOptions} />
          </Form.Item>

          <Form.Item label="Priority" name="priority" rules={[{ required: true }]}>
            <Select options={priorityOptions} />
          </Form.Item>

          <Form.Item label="Target Audience" name="targetAudience" rules={[{ required: true }]}>
            <Select options={audienceOptions} />
          </Form.Item>

          <Form.Item label="Publish Date" name="publishDate">
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>

          <Form.Item label="Expiry Date" name="expiryDate">
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>
        </div>

        {!isEdit && (
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0 16px' }}>
            <Form.Item label="Send Email Notification" name="sendEmailNotification" valuePropName="checked">
              <Switch />
            </Form.Item>
            <Form.Item label="Send Push Notification" name="sendPushNotification" valuePropName="checked">
              <Switch />
            </Form.Item>
          </div>
        )}
      </Form>
    </Modal>
  );
};
