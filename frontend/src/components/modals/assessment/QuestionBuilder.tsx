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

interface QuestionFieldsProps {
  value: QuestionDraft;
  onChange: (patch: Partial<QuestionDraft>) => void;
}

// The text/marks/options editor for a single question — factored out so it
// can be reused both inside QuestionBuilder's per-card list (create flow)
// and standalone in a single-question add/edit dialog (question management,
// T-T19), without duplicating the option add/remove/correct-answer wiring.
export const QuestionFields: React.FC<QuestionFieldsProps> = ({ value: q, onChange }) => {
  const addOption = () => {
    if (q.options.length >= MAX_OPTIONS) return;
    onChange({ options: [...q.options, ''] });
  };

  const removeOption = (oIdx: number) => {
    if (q.options.length <= MIN_OPTIONS) return;
    const options = q.options.filter((_, i) => i !== oIdx);
    // Keep the correct-option pointer valid after a removal: if the correct
    // option itself was removed, fall back to the first; if an earlier
    // option was removed, shift the pointer down by one.
    let correct = q.correctOptionIndex;
    if (oIdx === correct) correct = 0;
    else if (oIdx < correct) correct -= 1;
    onChange({ options, correctOptionIndex: correct });
  };

  const updateOption = (oIdx: number, text: string) => {
    onChange({ options: q.options.map((o, i) => (i === oIdx ? text : o)) });
  };

  return (
    <Space direction="vertical" style={{ width: '100%' }} size={8}>
      <Input.TextArea
        value={q.questionText}
        onChange={(e) => onChange({ questionText: e.target.value })}
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
          onChange={(v) => onChange({ marks: typeof v === 'number' ? v : 1 })}
        />
      </Space>

      <Text type="secondary" style={{ fontSize: 12 }}>
        Options — select the single correct answer
      </Text>
      <Radio.Group
        value={q.correctOptionIndex}
        onChange={(e) => onChange({ correctOptionIndex: e.target.value })}
        style={{ width: '100%' }}
      >
        <Space direction="vertical" style={{ width: '100%' }} size={6}>
          {q.options.map((opt, oIdx) => (
            <Space key={oIdx} style={{ width: '100%' }} align="center">
              <Radio value={oIdx} aria-label={`Mark option ${oIdx + 1} correct`} />
              <Input
                value={opt}
                onChange={(e) => updateOption(oIdx, e.target.value)}
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
                onClick={() => removeOption(oIdx)}
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
        onClick={addOption}
      >
        Add option
      </Button>
    </Space>
  );
};

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
            <QuestionFields value={q} onChange={(patch) => updateQuestion(qIdx, patch)} />
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
