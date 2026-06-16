'use client';

import React, { useEffect, useMemo, useState } from 'react';
import {
  Form,
  Input,
  InputNumber,
  Modal,
  Select,
  Upload,
  message,
} from 'antd';
import type { UploadFile } from 'antd';
import { InboxOutlined } from '@ant-design/icons';
import { z } from 'zod';
import { useLearningMaterialActions } from '@/providers/learning/learning_materials';
import type { IClassSubjectList } from '@/providers/academic/shared/interfaces';

// LearningMaterialType enum mirror — keep in sync with backend
// psms.Domain.Shared.Enums.LearningMaterialType.
enum LearningMaterialType {
  Document = 1,
  Video = 2,
  Audio = 3,
  Presentation = 4,
  Worksheet = 5,
  ExternalLink = 6,
  Image = 7,
  Interactive = 8,
}

const materialTypeOptions = [
  { value: LearningMaterialType.Document, label: 'Document (PDF/Word/Text)' },
  { value: LearningMaterialType.Presentation, label: 'Presentation (PPT)' },
  { value: LearningMaterialType.Worksheet, label: 'Worksheet' },
  { value: LearningMaterialType.Video, label: 'Video' },
  { value: LearningMaterialType.Audio, label: 'Audio' },
  { value: LearningMaterialType.Image, label: 'Image' },
  { value: LearningMaterialType.Interactive, label: 'Interactive' },
  { value: LearningMaterialType.ExternalLink, label: 'External Link' },
];

// File caps mirroring backend LM-001 (see LearningMaterialAppService.cs).
const VIDEO_MAX_BYTES = 500 * 1024 * 1024;
const DOCUMENT_MAX_BYTES = 50 * 1024 * 1024;

// Mirror of backend ValidateFile extension whitelist. Document /
// Presentation / Worksheet all share the backend's broad DocumentExtensions
// set; matching that here keeps the picker from rejecting valid files
// before the server even sees them.
const DOCUMENT_EXTS = [
  '.pdf', '.doc', '.docx', '.ppt', '.pptx', '.xls', '.xlsx',
  '.txt', '.csv', '.rtf', '.zip',
];
const EXTENSION_WHITELIST: Record<LearningMaterialType, string[]> = {
  [LearningMaterialType.Document]: DOCUMENT_EXTS,
  [LearningMaterialType.Presentation]: DOCUMENT_EXTS,
  [LearningMaterialType.Worksheet]: DOCUMENT_EXTS,
  [LearningMaterialType.Video]: ['.mp4', '.mov', '.avi', '.webm', '.mkv'],
  [LearningMaterialType.Audio]: ['.mp3', '.wav', '.ogg', '.m4a', '.flac'],
  [LearningMaterialType.Image]: ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp', '.svg'],
  [LearningMaterialType.Interactive]: ['.zip', '.html'],
  [LearningMaterialType.ExternalLink]: [],
};

// Allowed `accept` attribute per material type (used by the browser's
// file picker to filter selectable files — purely UX, server-side still
// enforces the whitelist).
function acceptForType(t: LearningMaterialType): string {
  return EXTENSION_WHITELIST[t]?.join(',') ?? '';
}

function maxBytesForType(t: LearningMaterialType): number {
  return t === LearningMaterialType.Video ? VIDEO_MAX_BYTES : DOCUMENT_MAX_BYTES;
}

const uploadSchema = z
  .object({
    classSubjectId: z.string().min(1, 'Class & subject is required'),
    termId: z.string().optional(),
    title: z.string().min(5, 'Title must be at least 5 characters').max(200),
    description: z
      .string()
      .min(10, 'Description must be at least 10 characters')
      .max(1000),
    materialType: z.number().min(1).max(8),
    externalLink: z.string().max(500).optional(),
    displayOrder: z.number().min(0).optional(),
  })
  .refine(
    (d) =>
      d.materialType !== LearningMaterialType.ExternalLink ||
      (d.externalLink != null && d.externalLink.length > 0),
    {
      message: 'External link is required for ExternalLink material type',
      path: ['externalLink'],
    }
  );

interface MaterialUploadModalProps {
  open: boolean;
  onClose: (refresh?: boolean) => void;
  classSubjects: IClassSubjectList[];
}

export const MaterialUploadModal: React.FC<MaterialUploadModalProps> = ({
  open,
  onClose,
  classSubjects,
}) => {
  const [form] = Form.useForm();
  const { uploadAsync, requestUploadUrlAsync, uploadFileToStorageAsync } =
    useLearningMaterialActions();
  const [loading, setLoading] = useState(false);
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [materialType, setMaterialType] = useState<LearningMaterialType>(
    LearningMaterialType.Document
  );

  useEffect(() => {
    if (open) {
      form.resetFields();
      setFileList([]);
      setMaterialType(LearningMaterialType.Document);
    }
  }, [open, form]);

  const classSubjectOptions = useMemo(
    () =>
      classSubjects.map((cs) => ({
        value: cs.id,
        label: `${cs.className ?? 'Class'} — ${cs.subjectName ?? 'Subject'}`,
      })),
    [classSubjects]
  );

  const isExternalLink = materialType === LearningMaterialType.ExternalLink;
  const accept = acceptForType(materialType);
  const maxBytes = maxBytesForType(materialType);

  const handleBeforeUpload = (file: File) => {
    if (file.size > maxBytes) {
      message.error(
        `File is ${(file.size / 1024 / 1024).toFixed(1)} MB — exceeds the ${
          maxBytes / 1024 / 1024
        } MB cap for this material type.`
      );
      return Upload.LIST_IGNORE;
    }
    const extension = '.' + (file.name.split('.').pop() ?? '').toLowerCase();
    const allowed = EXTENSION_WHITELIST[materialType] ?? [];
    if (allowed.length > 0 && !allowed.includes(extension)) {
      message.error(
        `Unsupported extension '${extension}'. Allowed: ${allowed.join(', ')}.`
      );
      return Upload.LIST_IGNORE;
    }
    // Returning false stops AntD's auto-upload — we handle the upload
    // manually in handleSubmit via the provider's multipart endpoint.
    return false;
  };

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();
      const parsed = uploadSchema.safeParse({
        ...values,
        materialType,
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

      const file = fileList[0]?.originFileObj;
      if (!isExternalLink && !file) {
        // The Upload.Dragger sits outside the AntD form value tree (owns
        // its own fileList state), so `setFields({ name: 'file' })` would
        // target a non-existent field and produce no visible feedback.
        // Use a toast instead.
        message.error('Please select a file to upload.');
        return;
      }

      setLoading(true);

      // Direct upload: get a signed URL, PUT the file straight to storage
      // (bytes bypass our server), then record the material with its URL.
      let fileUrl: string | undefined;
      let fileName: string | undefined;
      let fileSizeBytes: number | undefined;
      let contentType: string | undefined;
      if (!isExternalLink) {
        const f = file as File;
        const ticket = await requestUploadUrlAsync({
          classSubjectId: parsed.data.classSubjectId,
          fileName: f.name,
          materialType: parsed.data.materialType,
        });
        await uploadFileToStorageAsync(ticket.uploadUrl, f);
        fileUrl = ticket.publicUrl;
        fileName = f.name;
        fileSizeBytes = f.size;
        contentType = f.type || 'application/octet-stream';
      }

      await uploadAsync({
        classSubjectId: parsed.data.classSubjectId,
        termId: parsed.data.termId,
        title: parsed.data.title,
        description: parsed.data.description,
        materialType: parsed.data.materialType,
        externalLink: parsed.data.externalLink,
        displayOrder: parsed.data.displayOrder,
        fileUrl,
        fileName,
        fileSizeBytes,
        contentType,
      });
      message.success('Learning material uploaded');
      onClose(true);
    } catch {
      // Server errors are surfaced by the axios response interceptor.
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title="Upload Learning Material"
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
      width={640}
      okText="Upload"
    >
      <Form form={form} layout="vertical">
        <Form.Item
          label="Class &amp; subject"
          name="classSubjectId"
          rules={[{ required: true }]}
        >
          <Select
            options={classSubjectOptions}
            placeholder="Select the class-subject this material belongs to"
            showSearch
            optionFilterProp="label"
          />
        </Form.Item>

        <Form.Item label="Title" name="title" rules={[{ required: true }]}>
          <Input
            placeholder="e.g. Algebra basics — Chapter 1 notes"
            maxLength={200}
          />
        </Form.Item>

        <Form.Item
          label="Description"
          name="description"
          rules={[{ required: true }]}
        >
          <Input.TextArea
            rows={3}
            maxLength={1000}
            placeholder="What's in this material and how should students use it?"
          />
        </Form.Item>

        <Form.Item label="Material type" required>
          <Select
            value={materialType}
            onChange={(v) => setMaterialType(v as LearningMaterialType)}
            options={materialTypeOptions}
          />
        </Form.Item>

        {isExternalLink ? (
          <Form.Item
            label="External link"
            name="externalLink"
            rules={[{ required: true, message: 'URL is required' }]}
          >
            <Input
              placeholder="https://example.com/resource"
              maxLength={500}
              type="url"
            />
          </Form.Item>
        ) : (
          <>
            <Form.Item
              label={`File (${accept || 'any'} — max ${maxBytes / 1024 / 1024} MB)`}
              // No AntD `rules` here: the Upload.Dragger keeps its own
              // file-list state. The "select a file" check happens in
              // handleSubmit; this label is just visual.
            >
              <Upload.Dragger
                accept={accept || undefined}
                multiple={false}
                maxCount={1}
                fileList={fileList}
                beforeUpload={handleBeforeUpload}
                onChange={(info) => setFileList(info.fileList)}
                onRemove={() => setFileList([])}
              >
                <p className="ant-upload-drag-icon">
                  <InboxOutlined />
                </p>
                <p className="ant-upload-text">
                  Click or drag a file to this area
                </p>
                <p className="ant-upload-hint" style={{ fontSize: 12 }}>
                  Browser-side type/size checks run before submission;
                  server-side validation is the source of truth.
                </p>
              </Upload.Dragger>
            </Form.Item>
            <Form.Item
              label="Optional supporting link"
              name="externalLink"
            >
              <Input
                placeholder="https://example.com/related (optional)"
                maxLength={500}
                type="url"
              />
            </Form.Item>
          </>
        )}

        <Form.Item label="Display order" name="displayOrder">
          <InputNumber
            min={0}
            placeholder="0"
            style={{ width: '100%' }}
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};
