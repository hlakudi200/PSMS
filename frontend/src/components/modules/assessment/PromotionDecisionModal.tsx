'use client';

/**
 * RC-16. Recording the promotion decision on a year-end report card.
 *
 * The year-end card exists to carry this decision — NPPPPR §(2b)(c): "the
 * decision reached at the meeting contemplated above must be reflected on the
 * learner's report card" — and nothing in the product could record one.
 *
 * The screen shows the national requirements clause by clause, with what the
 * learner actually achieved against each, and then asks for the decision. It
 * does not make the decision: §(2b) puts a retention behind a meeting of subject
 * staff and then a meeting with the parent, and progression is a judgement about
 * a learner's best interests that no rule reaches. So a decision that departs
 * from the requirements is allowed, and asked to say why.
 */

import React, { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  Form,
  Input,
  List,
  Modal,
  Select,
  Space,
  Spin,
  Tag,
  Typography,
  message,
} from 'antd';
import { CheckCircleTwoTone, CloseCircleTwoTone } from '@ant-design/icons';
import { z } from 'zod';
import { useReportActions, useReportState } from '@/providers/assessment/reports';
import type { IPromotionAdvice } from '@/providers/assessment/shared/interfaces';

const { Text, Paragraph } = Typography;

/** Matches the backend PromotionDecision enum. */
export const PromotionDecisionValue = {
  Promoted: 1,
  Retained: 2,
  ConditionalPromotion: 3,
  ProgressedWithSupport: 4,
} as const;

export const promotionDecisionLabels: Record<number, string> = {
  [PromotionDecisionValue.Promoted]: 'Promoted',
  [PromotionDecisionValue.Retained]: 'Not promoted — retained',
  [PromotionDecisionValue.ConditionalPromotion]: 'Conditionally promoted',
  [PromotionDecisionValue.ProgressedWithSupport]: 'Progressed with support',
};

/**
 * A retained learner stays where they are, so the destination grade is only
 * asked for — and only sent — when they move.
 */
const movesToAnotherGrade = (decision?: number) =>
  decision !== undefined && decision !== PromotionDecisionValue.Retained;

const schema = z
  .object({
    decision: z.number({ error: 'Choose a decision' }).int().min(1).max(4),
    promotedToGradeId: z.string().optional(),
    reason: z.string().max(1000, 'Keep the reason under 1000 characters').optional(),
  })
  .refine((d) => !movesToAnotherGrade(d.decision) || !!d.promotedToGradeId, {
    message: 'Say which grade the learner moves into',
    path: ['promotedToGradeId'],
  });

type PromotionFormValues = z.infer<typeof schema>;

interface Props {
  open: boolean;
  reportId: string;
  studentName?: string;
  onClose: () => void;
  onRecorded?: () => void;
}

export const PromotionDecisionModal: React.FC<Props> = ({
  open,
  reportId,
  studentName,
  onClose,
  onRecorded,
}) => {
  const [form] = Form.useForm<PromotionFormValues>();
  const { getPromotionAdviceAsync, recordPromotionDecisionAsync } = useReportActions();
  const { promotionAdvice, isPending } = useReportState();

  const [decision, setDecision] = useState<number | undefined>();
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (open) getPromotionAdviceAsync(reportId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, reportId]);

  const advice: IPromotionAdvice | undefined =
    promotionAdvice?.reportId === reportId ? promotionAdvice : undefined;

  /* Seed the form from whatever is already recorded, or from what the rules
     point to when nothing is. */
  useEffect(() => {
    if (!advice) return;

    const seeded = advice.recorded ?? advice.recommended;
    setDecision(seeded);
    form.setFieldsValue({
      decision: seeded,
      promotedToGradeId:
        advice.promotedToGradeId ?? advice.gradeOptions?.find((g) => g.isNextGrade)?.id,
      reason: undefined,
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [advice]);

  const gradeOptions = useMemo(
    () =>
      (advice?.gradeOptions ?? []).map((g) => ({
        value: g.id,
        label: g.isNextGrade
          ? `${g.gradeName} (next grade)`
          : g.isCurrentGrade
            ? `${g.gradeName} (current grade)`
            : g.gradeName,
      })),
    [advice],
  );

  /* The rules say promote and the person says retain, or the other way round —
     either way the decision should carry what it was based on. */
  const departsFromTheRules =
    advice?.isEvaluable === true &&
    decision !== undefined &&
    ((advice.meetsRequirements && decision !== PromotionDecisionValue.Promoted) ||
      (!advice.meetsRequirements && decision === PromotionDecisionValue.Promoted));

  const handleSubmit = async () => {
    const values = await form.validateFields();
    const parsed = schema.safeParse(values);

    if (!parsed.success) {
      message.error(parsed.error.issues[0]?.message ?? 'Check the form');
      return;
    }

    setSaving(true);
    try {
      await recordPromotionDecisionAsync({
        reportId,
        decision: parsed.data.decision,
        promotedToGradeId: movesToAnotherGrade(parsed.data.decision)
          ? parsed.data.promotedToGradeId
          : undefined,
        reason: parsed.data.reason?.trim() || undefined,
      });
      message.success('Promotion decision recorded');
      onRecorded?.();
      onClose();
    } catch {
      message.error('Could not record the promotion decision');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal
      open={open}
      title={studentName ? `Promotion — ${studentName}` : 'Promotion decision'}
      onCancel={onClose}
      width={720}
      footer={[
        <Button key="cancel" onClick={onClose}>
          Cancel
        </Button>,
        <Button
          key="save"
          type="primary"
          loading={saving}
          disabled={!advice}
          onClick={handleSubmit}
        >
          Record decision
        </Button>,
      ]}
    >
      {!advice && isPending && (
        <div style={{ textAlign: 'center', padding: 32 }}>
          <Spin />
        </div>
      )}

      {advice && (
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
          {advice.isEvaluable === false ? (
            <Alert type="info" showIcon message={advice.notEvaluableReason} />
          ) : (
            <>
              <Alert
                type={advice.meetsRequirements ? 'success' : 'warning'}
                showIcon
                message={
                  advice.meetsRequirements
                    ? `${advice.gradeName ?? 'This grade'}: the national promotion requirements are met`
                    : `${advice.gradeName ?? 'This grade'}: the national promotion requirements are not met`
                }
                description={
                  <Text type="secondary">
                    The decision is still yours to make. NPPPPR §(2b) puts a retention behind a
                    meeting of subject staff and then a meeting with the parent.
                  </Text>
                }
              />

              <List
                size="small"
                bordered
                dataSource={advice.requirements ?? []}
                renderItem={(r) => (
                  <List.Item>
                    <Space align="start" style={{ width: '100%' }}>
                      {r.isMet ? (
                        <CheckCircleTwoTone twoToneColor="#52c41a" />
                      ) : (
                        <CloseCircleTwoTone twoToneColor="#ff4d4f" />
                      )}
                      <div>
                        <div>{r.description}</div>
                        <Text type="secondary" style={{ fontSize: 12 }}>
                          <Tag>{r.clause}</Tag>
                          {r.detail}
                        </Text>
                      </div>
                    </Space>
                  </List.Item>
                )}
              />
            </>
          )}

          <Form form={form} layout="vertical">
            <Form.Item label="Decision" name="decision" rules={[{ required: true }]}>
              <Select
                placeholder="Choose a decision"
                onChange={(value: number) => setDecision(value)}
                options={Object.entries(promotionDecisionLabels).map(([value, label]) => ({
                  value: Number(value),
                  label,
                }))}
              />
            </Form.Item>

            {movesToAnotherGrade(decision) && (
              <Form.Item
                label="Moves into"
                name="promotedToGradeId"
                rules={[{ required: true, message: 'Say which grade the learner moves into' }]}
              >
                <Select placeholder="Choose a grade" options={gradeOptions} />
              </Form.Item>
            )}

            <Form.Item
              label={departsFromTheRules ? 'Reason (required — this departs from the requirements)' : 'Reason'}
              name="reason"
              rules={[
                {
                  required: departsFromTheRules,
                  message: 'Say what this decision was based on',
                },
                { max: 1000, message: 'Keep the reason under 1000 characters' },
              ]}
            >
              <Input.TextArea
                rows={3}
                maxLength={1000}
                showCount
                placeholder="What the decision was based on — the staff meeting, the parent meeting, the support plan."
              />
            </Form.Item>
          </Form>

          {advice.recorded !== undefined && advice.recorded !== null && (
            <Paragraph type="secondary" style={{ marginBottom: 0 }}>
              Currently recorded: <Text strong>{promotionDecisionLabels[advice.recorded]}</Text>
              {advice.promotedToGradeName ? ` to ${advice.promotedToGradeName}` : ''}
            </Paragraph>
          )}
        </Space>
      )}
    </Modal>
  );
};

export default PromotionDecisionModal;
