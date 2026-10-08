'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, Checkbox, Select, message } from 'antd';
import { z } from 'zod';
import { useSubjectActions } from '@/providers/academic/subjects';
import type { ISubject } from '@/providers/academic/shared/interfaces';

const subjectSchema = z.object({
  subjectName: z.string().min(1, 'Subject name is required').max(200),
  subjectCode: z.string().min(1, 'Subject code is required').max(20),
  description: z.string().max(500).optional(),
  isCore: z.boolean(),
  /* RC-20. National Protocol s17(6): a language is recorded and reported at the
     level it is offered at, and the promotion rules are written in terms of
     those levels. Null for a subject that is not a language. */
  languageLevel: z.number().int().min(1).max(3).nullable().optional(),
});

const LANGUAGE_LEVELS = [
  { value: 1, label: 'Home Language' },
  { value: 2, label: 'First Additional Language' },
  { value: 3, label: 'Second Additional Language' },
];

interface SubjectFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: ISubject | null;
}

export const SubjectFormModal: React.FC<SubjectFormModalProps> = ({
  open,
  onClose,
  editRecord,
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useSubjectActions();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;

  useEffect(() => {
    if (open) {
      if (editRecord) {
        form.setFieldsValue({
          subjectName: editRecord.subjectName,
          subjectCode: editRecord.subjectCode,
          description: editRecord.description,
          isCore: editRecord.isCore,
          languageLevel: editRecord.languageLevel ?? null,
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
        isCore: values.isCore ?? false,
        languageLevel: values.languageLevel ?? null,
      };
      const result = subjectSchema.safeParse(parsed);

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
      message.success(`Subject ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      // Server errors are surfaced by the axios response interceptor.
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit Subject' : 'New Subject'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Subject Name" name="subjectName" rules={[{ required: true }]}>
          <Input placeholder="e.g. Mathematics" maxLength={200} />
        </Form.Item>
        <Form.Item label="Subject Code" name="subjectCode" rules={[{ required: true }]}>
          <Input placeholder="e.g. MATH" maxLength={20} />
        </Form.Item>
        <Form.Item label="Description" name="description">
          <Input.TextArea rows={3} maxLength={500} placeholder="Optional description" />
        </Form.Item>
        {/* RC-20. Only languages have one, so it is optional and says so. */}
        <Form.Item
          label="Language Level"
          name="languageLevel"
          tooltip="Only for language subjects. The promotion rules are written in terms of these levels, so a language needs one set."
        >
          <Select
            allowClear
            placeholder="Not a language subject"
            options={LANGUAGE_LEVELS}
          />
        </Form.Item>
        <Form.Item name="isCore" valuePropName="checked" initialValue={false}>
          <Checkbox>Core Subject</Checkbox>
        </Form.Item>
      </Form>
    </Modal>
  );
};
