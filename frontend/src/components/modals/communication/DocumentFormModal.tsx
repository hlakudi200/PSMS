'use client';

import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, InputNumber, message } from 'antd';
import { z } from 'zod';
import { useDocumentActions, useDocumentState } from '@/providers/communication/documents';
import type { IDocumentList } from '@/providers/communication/shared/interfaces';

// Mirrors backend psms.Domain.Shared.Enums.SharedDocumentType / DocumentAudience.
export const DOCUMENT_TYPE_OPTIONS = [
  { value: 1, label: 'Policy' },
  { value: 2, label: 'Form' },
  { value: 3, label: 'Newsletter' },
  { value: 4, label: 'Calendar' },
  { value: 5, label: 'Curriculum' },
  { value: 6, label: 'Handbook' },
  { value: 7, label: 'Template' },
  { value: 8, label: 'Other' },
];

export const DOCUMENT_AUDIENCE_OPTIONS = [
  { value: 1, label: 'Public' },
  { value: 2, label: 'Staff' },
  { value: 3, label: 'Teachers' },
  { value: 4, label: 'Parents' },
  { value: 5, label: 'Students' },
  { value: 6, label: 'All' },
];

const optionalText = (max: number) =>
  z.string().max(max).optional().or(z.literal('')).transform((v) => (v ? v : undefined));

const documentSchema = z.object({
  title: z.string().min(1, 'Title is required').max(200),
  description: optionalText(1000),
  fileName: z.string().min(1, 'File name is required').max(256),
  fileUrl: z.string().min(1, 'File URL is required').max(500).url('Must be a valid URL'),
  fileSizeBytes: z.number().int().min(0, 'Size must be 0 or greater'),
  contentType: optionalText(100),
  category: optionalText(50),
  documentType: z.number({ error: 'Document type is required' }).int().min(1, 'Document type is required'),
  targetAudience: z.number({ error: 'Audience is required' }).int().min(1, 'Audience is required'),
  academicYearId: z.string().optional(),
});

interface DocumentFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IDocumentList | null;
  academicYears?: { value: string; label: string }[];
}

export const DocumentFormModal: React.FC<DocumentFormModalProps> = ({
  open,
  onClose,
  editRecord,
  academicYears = [],
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync, getAsync } = useDocumentActions();
  const { document } = useDocumentState();
  const [loading, setLoading] = React.useState(false);
  const isEdit = !!editRecord;

  // The list DTO is trimmed; pull the full record so description / URL / year
  // can be edited.
  useEffect(() => {
    if (open && editRecord) getAsync(editRecord.id);
    if (open && !editRecord) form.resetFields();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, editRecord?.id]);

  useEffect(() => {
    if (!open || !editRecord || !document || document.id !== editRecord.id) return;
    form.setFieldsValue({
      title: document.title,
      description: document.description,
      fileName: document.fileName,
      fileUrl: document.fileUrl,
      fileSizeBytes: document.fileSizeBytes,
      contentType: document.contentType,
      category: document.category,
      documentType: document.documentType,
      targetAudience: document.targetAudience,
      academicYearId: document.academicYearId ?? undefined,
    });
  }, [open, editRecord, document, form]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();
      const result = documentSchema.safeParse({
        ...values,
        fileSizeBytes: values.fileSizeBytes ?? 0,
      });
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
        // The backend treats an absent academicYearId as "no change"; an explicit
        // flag is needed to clear a year that was previously assigned.
        const hadYear = !!document?.academicYearId && document.id === editRecord!.id;
        await updateAsync(editRecord!.id, {
          ...result.data,
          clearAcademicYearId: hadYear && !result.data.academicYearId ? true : undefined,
        });
      } else {
        await createAsync(result.data);
      }
      message.success(`Document ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      // Axios interceptor handles ABP error display
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit Document' : 'New Document'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
      width={640}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Title" name="title" rules={[{ required: true }]}>
          <Input placeholder="e.g. Code of Conduct 2026" maxLength={200} />
        </Form.Item>
        <Form.Item label="Description" name="description">
          <Input.TextArea rows={2} maxLength={1000} placeholder="What this document is for" />
        </Form.Item>
        <Form.Item label="File URL" name="fileUrl" rules={[{ required: true }]}>
          <Input placeholder="https://…" maxLength={500} />
        </Form.Item>
        <Form.Item label="File Name" name="fileName" rules={[{ required: true }]}>
          <Input placeholder="e.g. code-of-conduct.pdf" maxLength={256} />
        </Form.Item>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
          <Form.Item label="File Size (bytes)" name="fileSizeBytes">
            <InputNumber style={{ width: '100%' }} min={0} precision={0} placeholder="0" />
          </Form.Item>
          <Form.Item label="Content Type" name="contentType">
            <Input placeholder="e.g. application/pdf" maxLength={100} />
          </Form.Item>
          <Form.Item label="Document Type" name="documentType" rules={[{ required: true }]}>
            <Select options={DOCUMENT_TYPE_OPTIONS} placeholder="Select type" />
          </Form.Item>
          <Form.Item label="Audience" name="targetAudience" rules={[{ required: true }]}>
            <Select options={DOCUMENT_AUDIENCE_OPTIONS} placeholder="Who can see it" />
          </Form.Item>
          <Form.Item label="Category" name="category">
            <Input placeholder="e.g. Policies" maxLength={50} />
          </Form.Item>
          <Form.Item label="Academic Year" name="academicYearId">
            <Select options={academicYears} placeholder="Optional" allowClear />
          </Form.Item>
        </div>
      </Form>
    </Modal>
  );
};
