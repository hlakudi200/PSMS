'use client';

import { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Col,
  DatePicker,
  Divider,
  Form,
  Input,
  InputNumber,
  Modal,
  Row,
  Select,
  Skeleton,
  Typography,
  message,
} from 'antd';
import { z } from 'zod';
import { AssessmentType, assessmentTypeLabels } from '@/providers/shared/enums';
import dayjs, { Dayjs } from 'dayjs';
import { useAssessmentActions, useAssessmentState } from '@/providers/assessment/assessments';
import type {
  IClassSubjectList,
  ITerm,
} from '@/providers/academic/shared/interfaces';
import type {
  ICreateAssessmentWithQuestions,
  IAssessmentList,
} from '@/providers/assessment/shared/interfaces';
import {
  QuestionBuilder,
  makeEmptyQuestion,
  MIN_QUESTIONS,
  MAX_QUESTIONS,
  MIN_OPTIONS,
  MAX_OPTIONS,
  MIN_QUESTION_TEXT,
  MAX_QUESTION_TEXT,
  MAX_OPTION_TEXT,
  type QuestionDraft,
} from './QuestionBuilder';

const { Text } = Typography;

// Mirrors backend psms.Domain.Shared.Enums.AcademicAssessmentType.
// RC-13: these were the ADMISSIONS types — a teacher setting a class test was
// picking "Placement" or "General". Driven off the shared academic enum now, so
// this list and the mark-sheet screens can no longer drift apart.
const ASSESSMENT_TYPE_OPTIONS = Object.entries(assessmentTypeLabels).map(
  ([value, label]) => ({ value: Number(value), label })
);

// Mirror backend psms.Domain.Shared.Enums.CapsAssessmentCategory.
const CAPS_CATEGORY_OPTIONS = [
  { value: 1, label: 'Formal Test' },
  { value: 2, label: 'Formal Exam' },
  { value: 3, label: 'Assignment' },
  { value: 4, label: 'Project' },
  { value: 5, label: 'Practical' },
  { value: 6, label: 'Oral' },
  { value: 7, label: 'Investigation' },
  { value: 8, label: 'Controlled Test' },
  { value: 9, label: 'Case Study' },
];

const assessmentSchema = z
  .object({
    classSubjectId: z.string().min(1, 'Class & subject is required.'),
    termId: z.string().min(1, 'Term is required.'),
    name: z
      .string()
      .trim()
      .min(3, 'Name must be at least 3 characters.')
      .max(200, 'Name must be 200 characters or fewer.'),
    description: z.string().max(2000, 'Description must be 2000 characters or fewer.').optional().or(z.literal('')),
    assessmentType: z.number({ message: 'Pick an assessment type.' }).int().min(1).max(7),
    capsCategory: z.number().int().min(1).max(9).optional().nullable(),
    maxMarks: z
      .number({ message: 'Maximum marks is required.' })
      .positive('Maximum marks must be greater than zero.')
      .max(99999),
    weight: z.number().min(0, 'Weight cannot be negative.').max(100, 'Weight cannot exceed 100.'),
    passPercentage: z.number().min(0).max(100, 'Pass percentage must be between 0 and 100.'),
    durationMinutes: z.number().int().min(1).max(600).optional().nullable(),
    instructions: z.string().max(4000, 'Instructions must be 4000 characters or fewer.').optional().or(z.literal('')),
    scheduledDate: z.custom<Dayjs>().optional().nullable(),
    dueDate: z.custom<Dayjs>().optional().nullable(),
  })
  // QA-002: a due date cannot fall before the scheduled date.
  .refine(
    (d) => !(d.scheduledDate && d.dueDate) || !d.dueDate.isBefore(d.scheduledDate),
    { message: 'Due date cannot be before the scheduled date.', path: ['dueDate'] }
  );

interface AssessmentFormValues {
  classSubjectId?: string;
  termId?: string;
  name?: string;
  description?: string;
  assessmentType?: number;
  capsCategory?: number | null;
  maxMarks?: number;
  weight?: number;
  passPercentage?: number;
  durationMinutes?: number | null;
  instructions?: string;
  scheduledDate?: Dayjs | null;
  dueDate?: Dayjs | null;
}

// QA-001 question-structure validation. Returns the first human-readable
// error, or null if every question is well-formed. The backend re-checks
// all of this; this just spares the teacher a round trip.
function validateQuestions(questions: QuestionDraft[]): string | null {
  if (questions.length < MIN_QUESTIONS)
    return `Add at least ${MIN_QUESTIONS} questions (you have ${questions.length}).`;
  if (questions.length > MAX_QUESTIONS)
    return `Remove some questions — the maximum is ${MAX_QUESTIONS}.`;

  for (let i = 0; i < questions.length; i++) {
    const q = questions[i];
    const text = q.questionText.trim();
    if (text.length < MIN_QUESTION_TEXT)
      return `Question ${i + 1}: text must be at least ${MIN_QUESTION_TEXT} characters.`;
    if (text.length > MAX_QUESTION_TEXT)
      return `Question ${i + 1}: text must be ${MAX_QUESTION_TEXT} characters or fewer.`;
    if (q.options.length < MIN_OPTIONS || q.options.length > MAX_OPTIONS)
      return `Question ${i + 1}: must have between ${MIN_OPTIONS} and ${MAX_OPTIONS} options.`;
    if (q.options.some((o) => !o.trim()))
      return `Question ${i + 1}: every option must have text.`;
    if (q.options.some((o) => o.trim().length > MAX_OPTION_TEXT))
      return `Question ${i + 1}: an option exceeds ${MAX_OPTION_TEXT} characters.`;
    // Duplicate option text makes the correct answer ambiguous — reject it
    // case-insensitively, mirroring the server check.
    const normalized = q.options.map((o) => o.trim().toLowerCase());
    if (new Set(normalized).size !== normalized.length)
      return `Question ${i + 1}: options must be distinct.`;
    if (q.correctOptionIndex < 0 || q.correctOptionIndex >= q.options.length)
      return `Question ${i + 1}: select the correct option.`;
    if (!(q.marks > 0)) return `Question ${i + 1}: marks must be greater than zero.`;
  }
  return null;
}

interface AssessmentFormModalProps {
  open: boolean;
  onClose: (refresh: boolean) => void;
  classSubjects: IClassSubjectList[];
  terms: ITerm[];
  /**
   * When set, the modal edits this assessment instead of creating a new
   * one: ClassSubject/Term/Type become read-only (the backend's
   * UpdateAssessmentDto doesn't accept them — see AssessmentAppService.
   * UpdateAsync), the question builder is hidden (questions have their own
   * management flow), and Save calls Update instead of CreateWithQuestions.
   */
  editRecord?: IAssessmentList | null;
}

export const AssessmentFormModal: React.FC<AssessmentFormModalProps> = ({
  open,
  onClose,
  classSubjects,
  terms,
  editRecord,
}) => {
  const isEdit = !!editRecord;
  const [form] = Form.useForm<AssessmentFormValues>();
  const [submitting, setSubmitting] = useState(false);
  const [zodErrors, setZodErrors] = useState<Record<string, string>>({});
  const [questions, setQuestions] = useState<QuestionDraft[]>([]);
  const [questionError, setQuestionError] = useState<string | null>(null);

  const { createWithQuestionsAsync, updateAsync, getAsync } = useAssessmentActions();
  const { assessment, isPending: detailPending } = useAssessmentState();

  // Reset everything each time the modal opens so a previous draft doesn't
  // leak in. Start with one blank question to anchor the builder (create
  // mode only — edit mode doesn't touch questions).
  useEffect(() => {
    if (open) {
      form.resetFields();
      setZodErrors({});
      setQuestionError(null);
      setQuestions([makeEmptyQuestion()]);
      if (editRecord) getAsync(editRecord.id);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, editRecord?.id]);

  // Hydrate the form once the full record arrives. Gated on the id
  // matching so a stale record from a previous open can't leak in.
  useEffect(() => {
    if (open && editRecord && assessment && assessment.id === editRecord.id) {
      form.setFieldsValue({
        classSubjectId: assessment.classSubjectId,
        termId: assessment.termId,
        name: assessment.name,
        description: assessment.description,
        assessmentType: assessment.assessmentType,
        capsCategory: assessment.capsCategory ?? null,
        maxMarks: assessment.maxMarks,
        weight: assessment.weight,
        passPercentage: assessment.passPercentage,
        durationMinutes: assessment.durationMinutes ?? null,
        instructions: assessment.instructions,
        scheduledDate: assessment.scheduledDate ? dayjs(assessment.scheduledDate) : null,
        dueDate: assessment.dueDate ? dayjs(assessment.dueDate) : null,
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, editRecord?.id, assessment?.id]);

  const hydrated = !isEdit || (!!assessment && !!editRecord && assessment.id === editRecord.id);
  const showSkeleton = open && isEdit && (detailPending || !hydrated);

  const classSubjectOptions = useMemo(
    () =>
      classSubjects.map((cs) => ({
        value: cs.id,
        label: `${cs.className ?? 'Class'} — ${cs.subjectName ?? 'Subject'}`,
      })),
    [classSubjects]
  );

  const termOptions = useMemo(
    () =>
      terms.map((t) => ({
        value: t.id,
        label: t.isCurrent ? `${t.termName} (current)` : t.termName,
      })),
    [terms]
  );

  const handleSubmit = async () => {
    let values: AssessmentFormValues;
    try {
      values = await form.validateFields();
    } catch {
      return; // AntD shows per-field errors
    }

    const result = assessmentSchema.safeParse(values);
    if (!result.success) {
      const errors: Record<string, string> = {};
      result.error.issues.forEach((issue) => {
        const field = issue.path.join('.');
        if (!errors[field]) errors[field] = issue.message;
      });
      setZodErrors(errors);
      message.error('Please fix the highlighted fields.');
      return;
    }
    setZodErrors({});

    if (!isEdit) {
      const qError = validateQuestions(questions);
      if (qError) {
        setQuestionError(qError);
        message.error(qError);
        return;
      }

      // Mirror the server invariant: question marks can't exceed MaxMarks.
      const totalMarks = questions.reduce((sum, q) => sum + (q.marks || 0), 0);
      if (totalMarks > result.data.maxMarks) {
        const msg = `Total question marks (${totalMarks}) exceed the assessment maximum (${result.data.maxMarks}).`;
        setQuestionError(msg);
        message.error(msg);
        return;
      }
    }
    setQuestionError(null);

    const d = result.data;
    setSubmitting(true);
    try {
      if (isEdit && editRecord) {
        // ClassSubjectId/TermId/AssessmentType are omitted — the backend
        // rejects (ignores, really: UpdateAssessmentDto has no such
        // fields) changes to them once an assessment exists.
        await updateAsync(editRecord.id, {
          name: d.name.trim(),
          description: d.description?.trim() || undefined,
          capsCategory: d.capsCategory ?? undefined,
          maxMarks: d.maxMarks,
          weight: d.weight,
          passPercentage: d.passPercentage,
          scheduledDate: d.scheduledDate ? d.scheduledDate.toISOString() : undefined,
          dueDate: d.dueDate ? d.dueDate.toISOString() : undefined,
          durationMinutes: d.durationMinutes ?? undefined,
          instructions: d.instructions?.trim() || undefined,
        });
        message.success('Assessment updated');
      } else {
        const payload: ICreateAssessmentWithQuestions = {
          classSubjectId: d.classSubjectId,
          termId: d.termId,
          name: d.name.trim(),
          description: d.description?.trim() || undefined,
          assessmentType: d.assessmentType,
          capsCategory: d.capsCategory ?? undefined,
          maxMarks: d.maxMarks,
          weight: d.weight,
          passPercentage: d.passPercentage,
          scheduledDate: d.scheduledDate ? d.scheduledDate.toISOString() : undefined,
          dueDate: d.dueDate ? d.dueDate.toISOString() : undefined,
          durationMinutes: d.durationMinutes ?? undefined,
          instructions: d.instructions?.trim() || undefined,
          questions: questions.map((q) => ({
            questionText: q.questionText.trim(),
            marks: q.marks,
            options: q.options.map((o) => o.trim()),
            correctOptionIndex: q.correctOptionIndex,
          })),
        };
        await createWithQuestionsAsync(payload);
        message.success('Assessment created');
      }
      onClose(true);
    } catch {
      // The axios interceptor surfaces the server message (including the
      // QA-001 codes we may not have caught client-side).
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal
      open={open}
      title={isEdit ? 'Edit assessment' : 'Create assessment'}
      okText={isEdit ? 'Save changes' : 'Create assessment'}
      onOk={handleSubmit}
      onCancel={() => onClose(false)}
      confirmLoading={submitting}
      okButtonProps={{ disabled: showSkeleton }}
      destroyOnHidden
      width={760}
      styles={{ body: { maxHeight: '70vh', overflowY: 'auto' } }}
    >
      {showSkeleton && <Skeleton active paragraph={{ rows: 6 }} />}
      {/* Keep the Form mounted at all times so hydration effects never run
          against an unconnected form — only its visibility flips. */}
      <div style={{ display: showSkeleton ? 'none' : 'block' }}>
      <Form<AssessmentFormValues>
        form={form}
        layout="vertical"
        initialValues={{
          assessmentType: AssessmentType.Test,
          maxMarks: 100,
          weight: 0,
          passPercentage: 50,
        }}
      >
        <Row gutter={12}>
          <Col xs={24} md={12}>
            <Form.Item
              label="Class & subject"
              name="classSubjectId"
              rules={[{ required: true, message: 'Pick a class & subject.' }]}
              validateStatus={zodErrors.classSubjectId ? 'error' : undefined}
              help={zodErrors.classSubjectId}
              tooltip={isEdit ? "Can't be changed after creation." : undefined}
            >
              <Select
                placeholder="Pick a class & subject"
                showSearch
                optionFilterProp="label"
                options={classSubjectOptions}
                disabled={isEdit}
              />
            </Form.Item>
          </Col>
          <Col xs={24} md={12}>
            <Form.Item
              label="Term"
              name="termId"
              rules={[{ required: true, message: 'Pick a term.' }]}
              validateStatus={zodErrors.termId ? 'error' : undefined}
              help={zodErrors.termId}
              tooltip={isEdit ? "Can't be changed after creation." : undefined}
            >
              <Select
                placeholder="Pick a term"
                showSearch
                optionFilterProp="label"
                options={termOptions}
                disabled={isEdit}
              />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item
          label="Name"
          name="name"
          rules={[{ required: true, message: 'Name is required.' }]}
          validateStatus={zodErrors.name ? 'error' : undefined}
          help={zodErrors.name}
        >
          <Input placeholder="e.g. Term 2 Mathematics Test" maxLength={200} />
        </Form.Item>

        <Form.Item
          label="Description"
          name="description"
          validateStatus={zodErrors.description ? 'error' : undefined}
          help={zodErrors.description}
        >
          <Input.TextArea rows={2} maxLength={2000} showCount placeholder="Optional" />
        </Form.Item>

        <Row gutter={12}>
          <Col xs={24} md={8}>
            <Form.Item
              label="Type"
              name="assessmentType"
              rules={[{ required: true, message: 'Pick a type.' }]}
              validateStatus={zodErrors.assessmentType ? 'error' : undefined}
              help={zodErrors.assessmentType}
              tooltip={isEdit ? "Can't be changed after creation." : undefined}
            >
              <Select options={ASSESSMENT_TYPE_OPTIONS} disabled={isEdit} />
            </Form.Item>
          </Col>
          <Col xs={24} md={8}>
            <Form.Item label="CAPS category (optional)" name="capsCategory">
              <Select allowClear placeholder="None" options={CAPS_CATEGORY_OPTIONS} />
            </Form.Item>
          </Col>
          <Col xs={24} md={8}>
            <Form.Item
              label="Maximum marks"
              name="maxMarks"
              rules={[{ required: true, message: 'Max marks is required.' }]}
              validateStatus={zodErrors.maxMarks ? 'error' : undefined}
              help={zodErrors.maxMarks}
            >
              <InputNumber min={1} max={99999} style={{ width: '100%' }} />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={12}>
          <Col xs={24} md={8}>
            <Form.Item
              label="Weight (%)"
              name="weight"
              validateStatus={zodErrors.weight ? 'error' : undefined}
              help={zodErrors.weight}
            >
              <InputNumber min={0} max={100} style={{ width: '100%' }} />
            </Form.Item>
          </Col>
          <Col xs={24} md={8}>
            <Form.Item
              label="Pass percentage (%)"
              name="passPercentage"
              validateStatus={zodErrors.passPercentage ? 'error' : undefined}
              help={zodErrors.passPercentage}
            >
              <InputNumber min={0} max={100} style={{ width: '100%' }} />
            </Form.Item>
          </Col>
          <Col xs={24} md={8}>
            <Form.Item
              label="Duration (minutes)"
              name="durationMinutes"
              validateStatus={zodErrors.durationMinutes ? 'error' : undefined}
              help={zodErrors.durationMinutes}
            >
              <InputNumber min={1} max={600} style={{ width: '100%' }} placeholder="Optional" />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={12}>
          <Col xs={24} md={12}>
            <Form.Item
              label="Scheduled date"
              name="scheduledDate"
              validateStatus={zodErrors.scheduledDate ? 'error' : undefined}
              help={zodErrors.scheduledDate}
            >
              <DatePicker showTime format="YYYY-MM-DD HH:mm" style={{ width: '100%' }} />
            </Form.Item>
          </Col>
          <Col xs={24} md={12}>
            <Form.Item
              label="Due date"
              name="dueDate"
              validateStatus={zodErrors.dueDate ? 'error' : undefined}
              help={zodErrors.dueDate}
            >
              <DatePicker showTime format="YYYY-MM-DD HH:mm" style={{ width: '100%' }} />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item
          label="Instructions"
          name="instructions"
          validateStatus={zodErrors.instructions ? 'error' : undefined}
          help={zodErrors.instructions}
        >
          <Input.TextArea rows={2} maxLength={4000} showCount placeholder="Optional instructions for students" />
        </Form.Item>
      </Form>

      {!isEdit && (
        <>
          <Divider style={{ margin: '8px 0 16px' }} />

          <Text type="secondary" style={{ display: 'block', marginBottom: 8, fontSize: 12 }}>
            Build {MIN_QUESTIONS}-{MAX_QUESTIONS} multiple-choice questions. Each
            needs {MIN_OPTIONS}-{MAX_OPTIONS} options with exactly one marked
            correct (QA-001). The assessment and its questions are saved together.
          </Text>

          {questionError && (
            <Alert
              type="error"
              role="alert"
              showIcon
              message={questionError}
              style={{ marginBottom: 12 }}
            />
          )}

          <QuestionBuilder value={questions} onChange={setQuestions} />
        </>
      )}
      {isEdit && (
        <Text type="secondary" style={{ fontSize: 12 }}>
          Use Manage questions from the assessments table to add, edit,
          delete, or reorder questions.
        </Text>
      )}
      </div>
    </Modal>
  );
};
