'use client';

import React, { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Checkbox,
  DatePicker,
  Divider,
  Form,
  Input,
  InputNumber,
  Modal,
  Radio,
  Select,
  Upload,
  message,
} from 'antd';
import type { UploadFile } from 'antd';
import { InboxOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { z } from 'zod';
import { useLearningMaterialActions } from '@/providers/learning/learning_materials';
import { useGradeState, useGradeActions } from '@/providers/academic/grades';
import { useAcademicYearState, useAcademicYearActions } from '@/providers/academic/academic_years';
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

// US-TCH-006 naming convention for uploaded videos: Subject-Grade-Date-Topic.
// Each part is squashed to letters/digits (e.g. "LifeOrientation") so the
// hyphens only ever separate parts.
function toNamePart(value: string | undefined): string {
  return (value ?? '')
    .replace(/[^A-Za-z0-9 ]+/g, ' ')
    .split(' ')
    .filter(Boolean)
    .map((w) => w[0].toUpperCase() + w.slice(1))
    .join('');
}

function suggestVideoFileName(subject?: string, grade?: string, topic?: string): string {
  return [toNamePart(subject), toNamePart(grade), dayjs().format('YYYY-MM-DD'), toNamePart(topic)]
    .filter(Boolean)
    .join('-');
}

const FILE_NAME_PATTERN = /^[A-Za-z0-9][A-Za-z0-9_-]*$/;

// Materials are tagged Core or Supplementary — an explicit choice (no
// default) rather than a bare checkbox, since both states are meaningful.
const MATERIAL_CATEGORIES = ['Core', 'Supplementary'] as const;

// Up to 5 tags (ticket-specified); 30 chars/tag is our own reasonable
// default — the ticket gives a count bound but not a per-tag length.
const MAX_TAGS = 5;
const MAX_TAG_LENGTH = 30;

const uploadSchema = z
  .object({
    classSubjectId: z.string().min(1, 'Class & subject is required'),
    // Not required: Class & subject already implies a grade
    // (ClassSubject -> Class.GradeId on the backend), and this field isn't
    // persisted yet anyway (see the backend TODO doc) — it's confirmatory,
    // not load-bearing.
    gradeId: z.string().optional(),
    termId: z.string().min(1, 'Term is required'),
    title: z.string().min(5, 'Title must be at least 5 characters').max(200),
    description: z
      .string()
      .min(10, 'Description must be at least 10 characters')
      .max(1000),
    materialType: z.number().min(1).max(8),
    materialCategory: z.enum(MATERIAL_CATEGORIES, {
      message: 'Select Core or Supplementary',
    }),
    externalLink: z.string().max(500).optional(),
    displayOrder: z.number().min(0).optional(),
    // Not yet persisted — see the in-modal notice and
    // backend/docs/LearningMaterial-Metadata-Backend-TODO.md.
    tags: z
      .array(z.string().min(1).max(MAX_TAG_LENGTH, `Each tag must be ${MAX_TAG_LENGTH} characters or fewer`))
      .max(MAX_TAGS, `Up to ${MAX_TAGS} tags allowed`)
      .optional(),
    // Day-granularity, not exact-instant: a fixed grace window still fails
    // once enough time passes between picking "Now" and actually
    // submitting (filling in the rest of the form, uploading a file, etc).
    // Time-of-day is only meaningful for a genuinely future calendar day;
    // for "today" any time is accepted — matches the picker's own
    // disabledDate, which already only blocks days before today.
    scheduledPublishDate: z
      .string()
      .optional()
      .refine((v) => !v || !dayjs(v).isBefore(dayjs(), 'day'), {
        message: "Scheduled date can't be in the past",
      }),
    notifyStudents: z.boolean().optional(),
    // Video uploads only: the name to save the file under, without extension.
    fileName: z
      .string()
      .min(1, 'File name is required')
      .max(150, 'File name must be 150 characters or fewer')
      .regex(FILE_NAME_PATTERN, 'Use letters, digits, hyphens or underscores only')
      .optional(),
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
  /**
   * Video upload: the type is fixed to Video and a Subject-Grade-Date-Topic
   * file name is suggested. Video files are stored privately and can only be
   * streamed, never downloaded by students.
   */
  videoOnly?: boolean;
}

export const MaterialUploadModal: React.FC<MaterialUploadModalProps> = ({
  open,
  onClose,
  classSubjects,
  videoOnly = false,
}) => {
  const [form] = Form.useForm();
  const { uploadAsync, requestUploadUrlAsync, uploadFileToStorageAsync } =
    useLearningMaterialActions();
  const { activeGrades } = useGradeState();
  const { getActiveGradesAsync } = useGradeActions();
  const { academicYear } = useAcademicYearState();
  const { getCurrentAsync: getCurrentAcademicYearAsync } = useAcademicYearActions();
  const [loading, setLoading] = useState(false);
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [materialType, setMaterialType] = useState<LearningMaterialType>(
    LearningMaterialType.Document
  );
  // Until the teacher types their own file name, keep it in step with the
  // suggestion as class, grade and title change.
  const [fileNameEdited, setFileNameEdited] = useState(false);

  useEffect(() => {
    if (open) {
      form.resetFields();
      setFileList([]);
      setFileNameEdited(false);
      setMaterialType(videoOnly ? LearningMaterialType.Video : LearningMaterialType.Document);
      getActiveGradesAsync();
      // Term/GetAll doesn't exist on the backend (TermAppService has no
      // GetAllAsync method — a pre-existing bug in the terms provider, not
      // something introduced here). AcademicYear/GetCurrent already returns
      // the current year's full term list inline, so we use that instead.
      getCurrentAcademicYearAsync();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, form]);

  const classSubjectOptions = useMemo(
    () =>
      classSubjects.map((cs) => ({
        value: cs.id,
        label: `${cs.className ?? 'Class'} — ${cs.subjectName ?? 'Subject'}`,
      })),
    [classSubjects]
  );

  const gradeOptions = useMemo(
    () => (activeGrades ?? []).map((g) => ({ value: g.id, label: g.gradeName })),
    [activeGrades]
  );

  const termOptions = useMemo(
    () =>
      (academicYear?.terms ?? []).map((t) => ({
        value: t.id,
        label: `${t.termName}${t.isCurrent ? ' (Current)' : ''}`,
      })),
    [academicYear]
  );

  const watchedClassSubjectId = Form.useWatch('classSubjectId', form);
  const watchedGradeId = Form.useWatch('gradeId', form);
  const watchedTitle = Form.useWatch('title', form);

  const suggestedFileName = useMemo(() => {
    if (!videoOnly) return '';
    const cs = classSubjects.find((c) => c.id === watchedClassSubjectId);
    const grade = (activeGrades ?? []).find((g) => g.id === watchedGradeId)?.gradeName;
    return suggestVideoFileName(cs?.subjectName, grade ?? cs?.className, watchedTitle);
  }, [videoOnly, classSubjects, activeGrades, watchedClassSubjectId, watchedGradeId, watchedTitle]);

  useEffect(() => {
    if (videoOnly && !fileNameEdited) {
      form.setFieldValue('fileName', suggestedFileName);
    }
  }, [videoOnly, fileNameEdited, suggestedFileName, form]);

  const pickedExtension = (() => {
    const name = fileList[0]?.name;
    const dot = name?.lastIndexOf('.') ?? -1;
    return name && dot > 0 ? name.slice(dot).toLowerCase() : '';
  })();

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
        scheduledPublishDate: values.scheduledPublishDate
          ? dayjs(values.scheduledPublishDate).toISOString()
          : undefined,
        fileName: videoOnly ? (values.fileName ?? '').trim() : undefined,
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
      // (bytes bypass our server), then record the material by object key.
      let objectKey: string | undefined;
      let fileName: string | undefined;
      if (!isExternalLink) {
        const f = file as File;
        const ticket = await requestUploadUrlAsync({
          classSubjectId: parsed.data.classSubjectId,
          fileName: f.name,
          materialType: parsed.data.materialType,
        });
        await uploadFileToStorageAsync(ticket.uploadUrl, f);
        objectKey = ticket.objectKey;
        // Video uploads are saved under the convention name (the real
        // extension is kept, so server-side type checks still apply).
        fileName = videoOnly && parsed.data.fileName
          ? `${parsed.data.fileName}${pickedExtension}`
          : f.name;
      }

      // gradeId / materialCategory / tags / scheduledPublishDate /
      // notifyStudents are validated above but deliberately not sent here —
      // IUploadLearningMaterial (and the backend DTO it mirrors) has nowhere
      // for them to go yet. See backend/docs/LearningMaterial-Metadata-Backend-TODO.md.
      await uploadAsync({
        classSubjectId: parsed.data.classSubjectId,
        termId: parsed.data.termId,
        title: parsed.data.title,
        description: parsed.data.description,
        materialType: parsed.data.materialType,
        externalLink: parsed.data.externalLink,
        displayOrder: parsed.data.displayOrder,
        objectKey,
        fileName,
      });
      message.success(videoOnly ? 'Video uploaded' : 'Learning material uploaded');
      onClose(true);
    } catch (err) {
      // The direct-to-storage PUT uses fetch, so its failure is NOT caught by
      // the axios interceptor — surface it. Axios errors (with .response) are
      // already shown by the interceptor.
      if (!(err as { response?: unknown })?.response) {
        message.error((err as Error)?.message || 'Upload failed. Please try again.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={videoOnly ? 'Upload Video' : 'Upload Learning Material'}
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

        <Form.Item
          label="Grade"
          name="gradeId"
          tooltip="Optional — already implied by the class & subject you selected above."
        >
          <Select
            options={gradeOptions}
            placeholder="Select grade (optional)"
            showSearch
            optionFilterProp="label"
            allowClear
          />
        </Form.Item>

        <Form.Item label="Term" name="termId" rules={[{ required: true }]}>
          <Select
            options={termOptions}
            placeholder="Select term"
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

        {!videoOnly && (
          <Form.Item label="Material type" required>
            <Select
              value={materialType}
              onChange={(v) => setMaterialType(v as LearningMaterialType)}
              options={materialTypeOptions}
            />
          </Form.Item>
        )}

        <Form.Item
          label="Category"
          name="materialCategory"
          rules={[{ required: true, message: 'Select Core or Supplementary' }]}
        >
          <Radio.Group
            options={MATERIAL_CATEGORIES.map((c) => ({ label: c, value: c }))}
            optionType="button"
            buttonStyle="solid"
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
                  {videoOnly
                    ? 'Students can watch this video in PSMS but cannot download it.'
                    : 'Browser-side type/size checks run before submission; server-side validation is the source of truth.'}
                </p>
              </Upload.Dragger>
            </Form.Item>
            {videoOnly && (
              <Form.Item
                label="File name"
                name="fileName"
                tooltip="Suggested as Subject-Grade-Date-Topic. You can change it."
                extra={
                  fileNameEdited && suggestedFileName ? (
                    <a
                      onClick={() => {
                        setFileNameEdited(false);
                        form.setFieldValue('fileName', suggestedFileName);
                      }}
                    >
                      Use suggested: {suggestedFileName}
                    </a>
                  ) : undefined
                }
              >
                <Input
                  placeholder="Subject-Grade-YYYY-MM-DD-Topic"
                  maxLength={150}
                  addonAfter={pickedExtension || '.mp4'}
                  onChange={() => setFileNameEdited(true)}
                />
              </Form.Item>
            )}
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

        <Divider style={{ marginTop: 8 }} />

        <Alert
          type="warning"
          showIcon
          message="Not saved yet"
          description="Tags, scheduled release, and the notify option below aren't persisted by the server yet — backend support is tracked separately. Everything above this line uploads normally."
          style={{ marginBottom: 16 }}
        />

        <Form.Item
          label={`Tags (up to ${MAX_TAGS})`}
          name="tags"
          tooltip="Not saved yet — see the notice above."
        >
          <Select
            mode="tags"
            maxCount={MAX_TAGS}
            placeholder="Type a tag and press enter"
            tokenSeparators={[',']}
            notFoundContent={null}
          />
        </Form.Item>

        <Form.Item
          label="Scheduled release date"
          name="scheduledPublishDate"
          tooltip="Not saved yet — see the notice above."
        >
          <DatePicker
            showTime={{ format: 'HH:mm' }}
            format="YYYY-MM-DD HH:mm"
            style={{ width: '100%' }}
            disabledDate={(d) => !!d && d.isBefore(dayjs(), 'day')}
            placeholder="Publish immediately"
          />
        </Form.Item>

        <Form.Item name="notifyStudents" valuePropName="checked">
          <Checkbox>Notify students when published (not saved yet)</Checkbox>
        </Form.Item>
      </Form>
    </Modal>
  );
};
