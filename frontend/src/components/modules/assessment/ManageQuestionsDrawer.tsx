'use client';

import { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  Card,
  Drawer,
  Empty,
  Modal,
  Popconfirm,
  Skeleton,
  Space,
  Tag,
  Tooltip,
  Typography,
  message,
} from 'antd';
import {
  DeleteOutlined,
  DownOutlined,
  EditOutlined,
  PlusOutlined,
  UpOutlined,
} from '@ant-design/icons';
import {
  useAssessmentQuestionActions,
  useAssessmentQuestionState,
} from '@/providers/assessment/assessment_questions';
import type {
  IAssessmentList,
  IAssessmentQuestionList,
} from '@/providers/assessment/shared/interfaces';
import {
  QuestionFields,
  makeEmptyQuestion,
  MIN_QUESTIONS,
  MAX_QUESTIONS,
  MIN_QUESTION_TEXT,
  MAX_QUESTION_TEXT,
  MIN_OPTIONS,
  MAX_OPTIONS,
  type QuestionDraft,
} from '@/components/modals/assessment/QuestionBuilder';

const { Text } = Typography;

// QA-001 structure validation shared with the editor dialog — mirrors
// AssessmentFormModal's validateQuestions but for a single draft.
function validateDraft(q: QuestionDraft): string | null {
  const text = q.questionText.trim();
  if (text.length < MIN_QUESTION_TEXT)
    return `Question text must be at least ${MIN_QUESTION_TEXT} characters.`;
  if (text.length > MAX_QUESTION_TEXT)
    return `Question text must be ${MAX_QUESTION_TEXT} characters or fewer.`;
  if (q.options.length < MIN_OPTIONS || q.options.length > MAX_OPTIONS)
    return `Must have between ${MIN_OPTIONS} and ${MAX_OPTIONS} options.`;
  if (q.options.some((o) => !o.trim())) return 'Every option must have text.';
  const normalized = q.options.map((o) => o.trim().toLowerCase());
  if (new Set(normalized).size !== normalized.length) return 'Options must be distinct.';
  if (q.correctOptionIndex < 0 || q.correctOptionIndex >= q.options.length)
    return 'Select the correct option.';
  if (!(q.marks > 0)) return 'Marks must be greater than zero.';
  return null;
}

function draftFromFull(q: {
  questionText: string;
  marks: number;
  options?: string;
  correctAnswer?: string;
}): QuestionDraft {
  let options: string[] = [];
  try {
    options = q.options ? JSON.parse(q.options) : [];
  } catch {
    options = [];
  }
  if (options.length === 0) options = ['', ''];
  const correctOptionIndex = Math.max(0, options.indexOf(q.correctAnswer ?? ''));
  return {
    key: 'edit',
    questionText: q.questionText,
    marks: q.marks,
    options,
    correctOptionIndex,
  };
}

interface ManageQuestionsDrawerProps {
  open: boolean;
  onClose: () => void;
  assessment: IAssessmentList | null;
}

export const ManageQuestionsDrawer: React.FC<ManageQuestionsDrawerProps> = ({
  open,
  onClose,
  assessment,
}) => {
  const {
    getAsync,
    getByAssessmentAsync,
    createAsync,
    updateAsync,
    deleteAsync,
    reorderAsync,
  } = useAssessmentQuestionActions();
  const { questions, isPending, question: fullQuestion } = useAssessmentQuestionState();

  const [editorOpen, setEditorOpen] = useState(false);
  const [editorTarget, setEditorTarget] = useState<IAssessmentQuestionList | null>(null);
  const [editorDraft, setEditorDraft] = useState<QuestionDraft>(makeEmptyQuestion());
  const [editorLoading, setEditorLoading] = useState(false);
  const [editorError, setEditorError] = useState<string | null>(null);
  const [savingId, setSavingId] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [movingId, setMovingId] = useState<string | null>(null);

  useEffect(() => {
    if (open && assessment) {
      getByAssessmentAsync(assessment.id);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, assessment?.id]);

  const sortedQuestions = useMemo(
    () => (questions ?? []).slice().sort((a, b) => a.questionNumber - b.questionNumber),
    [questions]
  );

  const locked = !!assessment?.isPublished;
  const atMax = sortedQuestions.length >= MAX_QUESTIONS;
  const atMin = sortedQuestions.length <= MIN_QUESTIONS;

  const openAddEditor = () => {
    setEditorTarget(null);
    setEditorDraft(makeEmptyQuestion());
    setEditorError(null);
    setEditorOpen(true);
  };

  const openEditEditor = async (q: IAssessmentQuestionList) => {
    setEditorTarget(q);
    setEditorError(null);
    setEditorLoading(true);
    setEditorOpen(true);
    try {
      await getAsync(q.id);
    } catch {
      message.error('Could not load the question. Please try again.');
      setEditorOpen(false);
    } finally {
      setEditorLoading(false);
    }
  };

  // Hydrate the editor once the full question detail arrives.
  useEffect(() => {
    if (editorOpen && editorTarget && fullQuestion && fullQuestion.id === editorTarget.id) {
      setEditorDraft(draftFromFull(fullQuestion));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [editorOpen, editorTarget?.id, fullQuestion?.id]);

  const handleSaveEditor = async () => {
    const err = validateDraft(editorDraft);
    if (err) {
      setEditorError(err);
      return;
    }

    const otherTotal = sortedQuestions
      .filter((q) => q.id !== editorTarget?.id)
      .reduce((sum, q) => sum + q.marks, 0);
    if (assessment && otherTotal + editorDraft.marks > assessment.maxMarks) {
      setEditorError(
        `Total question marks (${otherTotal + editorDraft.marks}) would exceed the assessment maximum (${assessment.maxMarks}).`
      );
      return;
    }
    setEditorError(null);

    const trimmedOptions = editorDraft.options.map((o) => o.trim());
    const correctAnswer = trimmedOptions[editorDraft.correctOptionIndex];

    setSavingId(editorTarget?.id ?? 'new');
    try {
      if (editorTarget) {
        await updateAsync(editorTarget.id, {
          questionText: editorDraft.questionText.trim(),
          marks: editorDraft.marks,
          options: JSON.stringify(trimmedOptions),
          correctAnswer,
        });
        message.success('Question updated');
      } else if (assessment) {
        const nextNumber = sortedQuestions.reduce((max, q) => Math.max(max, q.questionNumber), 0) + 1;
        await createAsync({
          assessmentId: assessment.id,
          questionNumber: nextNumber,
          questionType: 1, // MultipleChoice — mirrors CreateWithQuestionsAsync.
          questionText: editorDraft.questionText.trim(),
          marks: editorDraft.marks,
          options: JSON.stringify(trimmedOptions),
          correctAnswer,
        });
        message.success('Question added');
      }
      setEditorOpen(false);
      if (assessment) getByAssessmentAsync(assessment.id);
    } catch {
      // Axios interceptor surfaces the server's message (e.g. duplicate
      // question number, marks-total exceeded, published-assessment guard).
    } finally {
      setSavingId(null);
    }
  };

  const handleDelete = async (q: IAssessmentQuestionList) => {
    if (!assessment) return;
    setDeletingId(q.id);
    try {
      await deleteAsync(q.id);
      message.success('Question deleted');
      getByAssessmentAsync(assessment.id);
    } catch {
      // Surfaced by axios interceptor
    } finally {
      setDeletingId(null);
    }
  };

  const handleMove = async (index: number, direction: -1 | 1) => {
    if (!assessment) return;
    const target = index + direction;
    if (target < 0 || target >= sortedQuestions.length) return;
    const reordered = sortedQuestions.slice();
    [reordered[index], reordered[target]] = [reordered[target], reordered[index]];

    setMovingId(sortedQuestions[index].id);
    try {
      await reorderAsync({
        assessmentId: assessment.id,
        questionIdsInOrder: reordered.map((q) => q.id),
      });
      getByAssessmentAsync(assessment.id);
    } catch {
      // Surfaced by axios interceptor
    } finally {
      setMovingId(null);
    }
  };

  return (
    <>
      <Drawer
        title={`Questions — ${assessment?.name ?? ''}`}
        open={open}
        onClose={onClose}
        width={620}
        destroyOnHidden
        extra={
          <Tooltip title={locked ? 'Unpublish the assessment to manage questions' : atMax ? `Maximum ${MAX_QUESTIONS} questions` : undefined}>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              disabled={locked || atMax}
              onClick={openAddEditor}
            >
              Add question
            </Button>
          </Tooltip>
        }
      >
        {locked && (
          <Alert
            type="warning"
            showIcon
            message="This assessment is published"
            description="Unpublish it first to add, edit, delete, or reorder questions."
            style={{ marginBottom: 16 }}
          />
        )}

        <Text type="secondary" style={{ display: 'block', marginBottom: 12, fontSize: 12 }}>
          {sortedQuestions.length} of {MIN_QUESTIONS}-{MAX_QUESTIONS} questions.
        </Text>

        {isPending && !questions ? (
          <Skeleton active paragraph={{ rows: 4 }} />
        ) : sortedQuestions.length === 0 ? (
          <Empty description="No questions yet" image={Empty.PRESENTED_IMAGE_SIMPLE} />
        ) : (
          <Space direction="vertical" style={{ width: '100%' }} size={8}>
            {sortedQuestions.map((q, idx) => (
              <Card key={q.id} size="small">
                <Space style={{ width: '100%', justifyContent: 'space-between' }} align="start">
                  <Space direction="vertical" size={2} style={{ maxWidth: 420 }}>
                    <Space size={6}>
                      <Tag>{`Q${idx + 1}`}</Tag>
                      <Tag color="blue">{q.marks} marks</Tag>
                    </Space>
                    <Text style={{ fontSize: 13 }}>{q.questionText}</Text>
                  </Space>
                  <Space size={4}>
                    <Tooltip title="Move up">
                      <Button
                        size="small"
                        icon={<UpOutlined />}
                        disabled={locked || idx === 0}
                        loading={movingId === q.id}
                        onClick={() => handleMove(idx, -1)}
                        aria-label={`Move question ${idx + 1} up`}
                      />
                    </Tooltip>
                    <Tooltip title="Move down">
                      <Button
                        size="small"
                        icon={<DownOutlined />}
                        disabled={locked || idx === sortedQuestions.length - 1}
                        loading={movingId === q.id}
                        onClick={() => handleMove(idx, 1)}
                        aria-label={`Move question ${idx + 1} down`}
                      />
                    </Tooltip>
                    <Tooltip title="Edit">
                      <Button
                        size="small"
                        icon={<EditOutlined />}
                        disabled={locked}
                        onClick={() => openEditEditor(q)}
                        aria-label={`Edit question ${idx + 1}`}
                      />
                    </Tooltip>
                    <Tooltip title={atMin ? `At least ${MIN_QUESTIONS} questions required` : 'Delete'}>
                      <Popconfirm
                        title="Delete this question?"
                        onConfirm={() => handleDelete(q)}
                        okText="Delete"
                        okButtonProps={{ danger: true }}
                        disabled={locked || atMin}
                      >
                        <Button
                          size="small"
                          danger
                          icon={<DeleteOutlined />}
                          disabled={locked || atMin}
                          loading={deletingId === q.id}
                          aria-label={`Delete question ${idx + 1}`}
                        />
                      </Popconfirm>
                    </Tooltip>
                  </Space>
                </Space>
              </Card>
            ))}
          </Space>
        )}
      </Drawer>

      <Modal
        title={editorTarget ? 'Edit question' : 'Add question'}
        open={editorOpen}
        onCancel={() => setEditorOpen(false)}
        onOk={handleSaveEditor}
        confirmLoading={savingId !== null}
        okButtonProps={{ disabled: editorLoading }}
        destroyOnHidden
        width={560}
      >
        {editorLoading ? (
          <Skeleton active paragraph={{ rows: 4 }} />
        ) : (
          <>
            {editorError && (
              <Alert type="error" showIcon message={editorError} style={{ marginBottom: 12 }} />
            )}
            <QuestionFields
              value={editorDraft}
              onChange={(patch) => setEditorDraft((d) => ({ ...d, ...patch }))}
            />
          </>
        )}
      </Modal>
    </>
  );
};
