'use client';

import { Button, Card, Input, InputNumber, Radio, Space, Typography } from 'antd';
import { DeleteOutlined, PlusOutlined } from '@ant-design/icons';

const { Text } = Typography;

// QA-001 structure bounds — kept in sync with the backend
// AssessmentAppService constants of the same name. The server re-validates
// on submit, so these only exist to give the teacher inline feedback.
export const MIN_QUESTIONS = 5;
export const MAX_QUESTIONS = 100;
export const MIN_OPTIONS = 2;
export const MAX_OPTIONS = 6;
export const MIN_QUESTION_TEXT = 10;
export const MAX_QUESTION_TEXT = 1000;
export const MAX_OPTION_TEXT = 500;

export interface QuestionDraft {
  key: string;
  questionText: string;
  marks: number;
  options: string[];
  correctOptionIndex: number;
}

// Module-level counter so question keys are unique whether they're created
// by the modal's initial state or added later in the builder — both share
// this sequence, so they can never collide. Not used for any persisted
// value (the key is dropped before submit), only as a stable React key.
let keyCounter = 0;

export function newQuestionKey(): string {
  keyCounter += 1;
  return `q-${keyCounter}`;
}

export function makeEmptyQuestion(): QuestionDraft {
  return {
    key: newQuestionKey(),
    questionText: '',
    marks: 1,
    options: ['', ''],
    correctOptionIndex: 0,
  };
}

interface QuestionBuilderProps {
  value: QuestionDraft[];
  onChange: (questions: QuestionDraft[]) => void;
}

export const QuestionBuilder: React.FC<QuestionBuilderProps> = ({ value, onChange }) => {
  const updateQuestion = (idx: number, patch: Partial<QuestionDraft>) => {
    onChange(value.map((q, i) => (i === idx ? { ...q, ...patch } : q)));
  };

  const addQuestion = () => {
    if (value.length >= MAX_QUESTIONS) return;
    onChange([...value, makeEmptyQuestion()]);
  };

  const removeQuestion = (idx: number) => {
    onChange(value.filter((_, i) => i !== idx));
  };

  const addOption = (qIdx: number) => {
    const q = value[qIdx];
    if (q.options.length >= MAX_OPTIONS) return;
    updateQuestion(qIdx, { options: [...q.options, ''] });
  };

  const removeOption = (qIdx: number, oIdx: number) => {
    const q = value[qIdx];
    if (q.options.length <= MIN_OPTIONS) return;
    const options = q.options.filter((_, i) => i !== oIdx);
    // Keep the correct-option pointer valid after a removal: if the correct
    // option itself was removed, fall back to the first; if an earlier
    // option was removed, shift the pointer down by one.
    let correct = q.correctOptionIndex;
    if (oIdx === correct) correct = 0;
    else if (oIdx < correct) correct -= 1;
    updateQuestion(qIdx, { options, correctOptionIndex: correct });
  };

  const updateOption = (qIdx: number, oIdx: number, text: string) => {
    const q = value[qIdx];
    updateQuestion(qIdx, {
      options: q.options.map((o, i) => (i === oIdx ? text : o)),
    });
  };

  return (
    <div>
      <Space style={{ width: '100%', justifyContent: 'space-between', marginBottom: 8 }}>
        <Text strong>Questions ({value.length})</Text>
        <Text
          type={value.length < MIN_QUESTIONS ? 'danger' : 'secondary'}
          style={{ fontSize: 12 }}
        >
          {value.length < MIN_QUESTIONS
            ? `Add at least ${MIN_QUESTIONS - value.length} more (minimum ${MIN_QUESTIONS}).`
            : `Minimum ${MIN_QUESTIONS}, maximum ${MAX_QUESTIONS}.`}
        </Text>
      </Space>

      <Space direction="vertical" style={{ width: '100%' }} size={12}>
        {value.map((q, qIdx) => (
          <Card
            key={q.key}
            size="small"
            title={`Question ${qIdx + 1}`}
            extra={
              <Button
                size="small"
                danger
                type="text"
                icon={<DeleteOutlined />}
                onClick={() => removeQuestion(qIdx)}
                aria-label={`Remove question ${qIdx + 1}`}
              />
            }
          >
            <Space direction="vertical" style={{ width: '100%' }} size={8}>
              <Input.TextArea
                value={q.questionText}
                onChange={(e) => updateQuestion(qIdx, { questionText: e.target.value })}
                placeholder={`Question text (${MIN_QUESTION_TEXT}-${MAX_QUESTION_TEXT} characters)`}
                autoSize={{ minRows: 2, maxRows: 4 }}
                maxLength={MAX_QUESTION_TEXT}
                showCount
              />

              <Space>
                <Text type="secondary" style={{ fontSize: 12 }}>
                  Marks
                </Text>
                <InputNumber
                  min={0.5}
                  max={9999}
                  step={0.5}
                  value={q.marks}
                  onChange={(v) =>
                    updateQuestion(qIdx, { marks: typeof v === 'number' ? v : 1 })
                  }
                />
              </Space>

              <Text type="secondary" style={{ fontSize: 12 }}>
                Options — select the single correct answer
              </Text>
              <Radio.Group
                value={q.correctOptionIndex}
                onChange={(e) => updateQuestion(qIdx, { correctOptionIndex: e.target.value })}
                style={{ width: '100%' }}
              >
                <Space direction="vertical" style={{ width: '100%' }} size={6}>
                  {q.options.map((opt, oIdx) => (
                    <Space key={oIdx} style={{ width: '100%' }} align="center">
                      <Radio value={oIdx} aria-label={`Mark option ${oIdx + 1} correct`} />
                      <Input
                        value={opt}
                        onChange={(e) => updateOption(qIdx, oIdx, e.target.value)}
                        placeholder={`Option ${oIdx + 1}`}
                        maxLength={MAX_OPTION_TEXT}
                        style={{ width: 360 }}
                      />
                      <Button
                        size="small"
                        type="text"
                        danger
                        icon={<DeleteOutlined />}
                        disabled={q.options.length <= MIN_OPTIONS}
                        onClick={() => removeOption(qIdx, oIdx)}
                        aria-label={`Remove option ${oIdx + 1}`}
                      />
                    </Space>
                  ))}
                </Space>
              </Radio.Group>

              <Button
                size="small"
                type="dashed"
                icon={<PlusOutlined />}
                disabled={q.options.length >= MAX_OPTIONS}
                onClick={() => addOption(qIdx)}
              >
                Add option
              </Button>
            </Space>
          </Card>
        ))}
      </Space>

      <Button
        type="dashed"
        icon={<PlusOutlined />}
        onClick={addQuestion}
        disabled={value.length >= MAX_QUESTIONS}
        style={{ marginTop: 12, width: '100%' }}
      >
        Add question
      </Button>
    </div>
  );
};
