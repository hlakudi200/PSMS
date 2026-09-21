'use client';

import React, { useEffect, useState } from 'react';
import {
  Divider,
  Form,
  Input,
  InputNumber,
  Modal,
  Select,
  Skeleton,
  Typography,
  Upload,
  message,
} from 'antd';
import type { UploadFile } from 'antd';
import { CloudUploadOutlined } from '@ant-design/icons';
import { z } from 'zod';
import {
  useLearningMaterialActions,
  useLearningMaterialState,
} from '@/providers/learning/learning_materials';
import type { ILearningMaterialList } from '@/providers/learning/shared/interfaces';

const { Text } = Typography;

// LearningMaterialType mirror — keep aligned with backend
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

const typeOptions = [
  { value: LearningMaterialType.Document, label: 'Document' },
  { value: LearningMaterialType.Presentation, label: 'Presentation' },
  { value: LearningMaterialType.Worksheet, label: 'Worksheet' },
  { value: LearningMaterialType.Video, label: 'Video' },
  { value: LearningMaterialType.Audio, label: 'Audio' },
  { value: LearningMaterialType.Image, label: 'Image' },
  { value: LearningMaterialType.Interactive, label: 'Interactive' },
  { value: LearningMaterialType.ExternalLink, label: 'External Link' },
];

const editSchema = z.object({
  title: z
    .string()
    .min(5, 'Title must be at least 5 characters')
    .max(200, 'Title must be at most 200 characters'),
  description: z
    .string()
    .min(10, 'Description must be at least 10 characters')
    .max(1000),
  materialType: z.number().min(1).max(8),
  // The backend returns `null` (not omitted) for materials with no link —
  // `.optional()` alone only accepts `undefined`, so a hydrated form with
  // no link would fail validation and block every save.
  externalLink: z.string().max(500).nullable().optional(),
  displayOrder: z.number().min(0).optional(),
});

interface MaterialEditModalProps {
  open: boolean;
  onClose: (refresh?: boolean) => void;
  editRecord: ILearningMaterialList | null;
}

export const MaterialEditModal: React.FC<MaterialEditModalProps> = ({
  open,
  onClose,
  editRecord,
}) => {
  const [form] = Form.useForm();
  const {
    getAsync,
    updateAsync,
    requestVersionUploadUrlAsync,
    uploadFileToStorageAsync,
    uploadNewVersionAsync,
  } = useLearningMaterialActions();
  const {
    learningMaterial,
    isPending: detailPending,
  } = useLearningMaterialState();
  const [loading, setLoading] = useState(false);
  const [replaceFileList, setReplaceFileList] = useState<UploadFile[]>([]);
  const [replaceChangeDescription, setReplaceChangeDescription] = useState('');

  // Open: fire the GetAsync that loads the full material DTO. The list
  // DTO that arrives in `editRecord` doesn't include description /
  // externalLink / displayOrder, so we have to wait for the full record.
  useEffect(() => {
    if (open && editRecord) {
      form.resetFields();
      setReplaceFileList([]);
      setReplaceChangeDescription('');
      getAsync(editRecord.id);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, editRecord?.id]);

  // Hydrate the form once the full record arrives. Gating on
  // `learningMaterial.id === editRecord.id` avoids the race where the
  // provider still has a stale record from a previous open.
  useEffect(() => {
    if (
      open &&
      editRecord &&
      learningMaterial &&
      learningMaterial.id === editRecord.id
    ) {
      form.setFieldsValue({
        title: learningMaterial.title,
        description: learningMaterial.description,
        materialType: learningMaterial.materialType,
        externalLink: learningMaterial.externalLink,
        displayOrder: learningMaterial.displayOrder,
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, editRecord?.id, learningMaterial?.id]);

  // Save handles both metadata and, if a replacement file was picked, the
  // file swap — one "Save changes" click does everything rather than
  // requiring a second, separate upload action. The file swap creates a
  // new version (preserves history / LM-003) rather than overwriting
  // FileUrl directly — same path the version history drawer uses.
  const handleSubmit = async () => {
    if (!editRecord) return;
    const values = form.getFieldsValue();
    const result = editSchema.safeParse(values);
    if (!result.success) {
      form.setFields(
        result.error.issues.map((err) => ({
          name: err.path as string[],
          errors: [err.message],
        }))
      );
      return;
    }

    const replacementFile = replaceFileList[0]?.originFileObj as File | undefined;

    setLoading(true);
    try {
      await updateAsync(editRecord.id, {
        title: result.data.title.trim(),
        description: result.data.description,
        materialType: result.data.materialType,
        externalLink: result.data.externalLink ?? undefined,
        displayOrder: result.data.displayOrder,
      });

      if (replacementFile) {
        const ticket = await requestVersionUploadUrlAsync({
          learningMaterialId: editRecord.id,
          fileName: replacementFile.name,
        });
        await uploadFileToStorageAsync(ticket.uploadUrl, replacementFile);
        await uploadNewVersionAsync({
          learningMaterialId: editRecord.id,
          changeDescription: replaceChangeDescription.trim() || undefined,
          objectKey: ticket.objectKey,
          fileName: replacementFile.name,
        });
      }

      message.success(replacementFile ? 'Material updated and file replaced' : 'Material updated');
      setReplaceFileList([]);
      setReplaceChangeDescription('');
      onClose(true);
    } catch (err) {
      // The direct-to-storage PUT uses fetch, not axios, so its failure
      // isn't caught by the axios interceptor — surface it here. Axios
      // errors (with .response) are already shown by the interceptor.
      if (!(err as { response?: unknown })?.response) {
        message.error((err as Error)?.message || 'Save failed. Please try again.');
      }
    } finally {
      setLoading(false);
    }
  };

  // True while we're waiting for the full record to land OR before it
  // matches the row we just opened. Drives the skeleton inside the modal.
  const hydrated =
    !!learningMaterial && !!editRecord && learningMaterial.id === editRecord.id;
  const showSkeleton = open && (detailPending || !hydrated);

  return (
    <Modal
      title="Edit Material"
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      // AntD v6 deprecates `destroyOnClose` in favour of `destroyOnHidden`.
      destroyOnHidden
      width={560}
      okText="Save changes"
      okButtonProps={{ disabled: showSkeleton }}
    >
      {/* Keep the Form mounted at all times — only its visibility flips —
          so resetFields() / setFieldsValue() in the open / hydration
          effects never run against an unmounted Form (which would log a
          "useForm not connected" warning). */}
      {showSkeleton && <Skeleton active paragraph={{ rows: 5 }} />}
      <div style={{ display: showSkeleton ? 'none' : 'block' }}>
        {/* AntD `required` markers below are decorative; the source of
            validation truth is the Zod schema invoked in `handleSubmit`.
            We keep the asterisks because users expect them next to
            required labels and they don't trigger form.validateFields(). */}
        <Form form={form} layout="vertical">
          <Form.Item label="Title" name="title" required>
            <Input maxLength={200} />
          </Form.Item>
          <Form.Item label="Description" name="description" required>
            <Input.TextArea rows={3} maxLength={1000} />
          </Form.Item>
          <Form.Item label="Material type" name="materialType" required>
            <Select options={typeOptions} />
          </Form.Item>
          <Form.Item label="External link" name="externalLink">
            <Input type="url" maxLength={500} placeholder="(optional)" />
          </Form.Item>
          <Form.Item label="Display order" name="displayOrder">
            <InputNumber min={0} style={{ width: '100%' }} />
          </Form.Item>
        </Form>

        {learningMaterial?.materialType !== LearningMaterialType.ExternalLink && (
          <>
            <Divider style={{ marginTop: 8 }} />
            <Text strong style={{ display: 'block', marginBottom: 8 }}>
              Replace file
            </Text>
            <Text type="secondary" style={{ fontSize: 12, display: 'block', marginBottom: 8 }}>
              Pick a file below and it&apos;s uploaded as a new version — older
              versions stay available in Version history — when you press
              Save changes. Leave this empty to keep the current file.
            </Text>
            <Upload.Dragger
              multiple={false}
              maxCount={1}
              fileList={replaceFileList}
              beforeUpload={() => false}
              onChange={(info) => setReplaceFileList(info.fileList)}
              onRemove={() => setReplaceFileList([])}
              style={{ marginBottom: 8 }}
            >
              <p className="ant-upload-drag-icon">
                <CloudUploadOutlined />
              </p>
              <p className="ant-upload-text" style={{ fontSize: 13 }}>
                Click or drag the replacement file here
              </p>
            </Upload.Dragger>
            {replaceFileList.length > 0 && (
              <Input.TextArea
                rows={2}
                placeholder="What changed? (optional, up to 500 characters)"
                maxLength={500}
                value={replaceChangeDescription}
                onChange={(e) => setReplaceChangeDescription(e.target.value)}
              />
            )}
          </>
        )}
      </div>
    </Modal>
  );
};
