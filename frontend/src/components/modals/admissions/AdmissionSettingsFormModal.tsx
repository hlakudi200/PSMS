'use client';

import React, { useEffect, useState } from 'react';
import { Alert, Col, DatePicker, Form, Input, InputNumber, Modal, Row, Switch, Typography, message } from 'antd';
import dayjs from 'dayjs';
import { z } from 'zod';
import { useAdmissionSettingsActions } from '@/providers/admissions/admission_settings';
import type { IAdmissionSettings } from '@/providers/admissions/shared/interfaces';

const { Text } = Typography;

/**
 * What a school decides about taking applications for one grade and year.
 *
 * These settings existed and were shown in a read-only table, so the only way
 * to change any of them was an API client. The application fee toggle in
 * particular is useless without somewhere to switch it.
 */
const schema = z
  .object({
    isAcceptingApplications: z.boolean(),
    applicationOpenDate: z.any().nullish(),
    applicationCloseDate: z.any().nullish(),
    maxCapacity: z.number().int().min(1, 'A capacity of none would turn applications away.').nullish(),
    isApplicationFeeRequired: z.boolean(),
    applicationFeeAmount: z.number().min(0, 'A fee cannot be negative.'),
    minimumAge: z.number().int().min(0).max(25).nullish(),
    maximumAge: z.number().int().min(0).max(25).nullish(),
    offerExpiryDays: z.number().int().min(1, 'An offer has to stand for at least a day.'),
    isInterviewRequired: z.boolean(),
    isAssessmentRequired: z.boolean(),
    requiredDocuments: z.string().max(1000).nullish(),
    notes: z.string().max(2000).nullish(),
  })
  .refine(
    (v) => !v.applicationOpenDate || !v.applicationCloseDate || !v.applicationCloseDate.isBefore(v.applicationOpenDate),
    { path: ['applicationCloseDate'], message: 'Applications cannot close before they open.' }
  )
  .refine((v) => v.minimumAge == null || v.maximumAge == null || v.maximumAge >= v.minimumAge, {
    path: ['maximumAge'],
    message: 'The oldest cannot be younger than the youngest.',
  })
  .refine((v) => !v.isApplicationFeeRequired || v.applicationFeeAmount > 0, {
    path: ['applicationFeeAmount'],
    message: 'Charging a fee of nothing is the same as not charging one. Set an amount, or turn the fee off.',
  });

type FormValues = z.infer<typeof schema>;

interface Props {
  open: boolean;
  settings?: IAdmissionSettings;
  onClose: () => void;
  onSaved: () => void;
}

export default function AdmissionSettingsFormModal({ open, settings, onClose, onSaved }: Props) {
  const [form] = Form.useForm<FormValues>();
  const { updateAsync } = useAdmissionSettingsActions();
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | undefined>();

  const feeRequired = Form.useWatch('isApplicationFeeRequired', form);

  useEffect(() => {
    if (!open || !settings) return;
    form.setFieldsValue({
      isAcceptingApplications: settings.isAcceptingApplications,
      applicationOpenDate: settings.applicationOpenDate ? dayjs(settings.applicationOpenDate) : null,
      applicationCloseDate: settings.applicationCloseDate ? dayjs(settings.applicationCloseDate) : null,
      maxCapacity: settings.maxCapacity ?? null,
      isApplicationFeeRequired: settings.isApplicationFeeRequired ?? settings.applicationFeeAmount > 0,
      applicationFeeAmount: settings.applicationFeeAmount,
      minimumAge: settings.minimumAge ?? null,
      maximumAge: settings.maximumAge ?? null,
      offerExpiryDays: settings.offerExpiryDays,
      isInterviewRequired: settings.isInterviewRequired,
      isAssessmentRequired: settings.isAssessmentRequired,
      requiredDocuments: settings.requiredDocuments ?? '',
      notes: settings.notes ?? '',
    });
    setError(undefined);
  }, [open, settings, form]);

  const submit = async () => {
    if (!settings) return;

    const parsed = schema.safeParse(form.getFieldsValue());
    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message);
      return;
    }

    const v = parsed.data;
    setSaving(true);
    setError(undefined);

    try {
      await updateAsync(settings.id, {
        isAcceptingApplications: v.isAcceptingApplications,
        applicationOpenDate: v.applicationOpenDate ? v.applicationOpenDate.toISOString() : undefined,
        applicationCloseDate: v.applicationCloseDate ? v.applicationCloseDate.toISOString() : undefined,
        maxCapacity: v.maxCapacity ?? undefined,
        isApplicationFeeRequired: v.isApplicationFeeRequired,
        applicationFeeAmount: v.applicationFeeAmount,
        minimumAge: v.minimumAge ?? undefined,
        maximumAge: v.maximumAge ?? undefined,
        offerExpiryDays: v.offerExpiryDays,
        isInterviewRequired: v.isInterviewRequired,
        isAssessmentRequired: v.isAssessmentRequired,
        requiredDocuments: v.requiredDocuments || undefined,
        notes: v.notes || undefined,
      });
      message.success('Admission settings saved');
      onSaved();
      onClose();
    } catch (e) {
      const abp = (e as { response?: { data?: { error?: { message?: string; details?: string } } } })
        ?.response?.data?.error;
      setError(abp?.message || abp?.details || 'Could not save these settings.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal
      open={open}
      title={
        settings
          ? `Admissions for ${settings.gradeName ?? 'every grade'} · ${settings.academicYearName}`
          : 'Admission settings'
      }
      okText="Save settings"
      okButtonProps={{ loading: saving }}
      onOk={submit}
      onCancel={onClose}
      width={720}
      destroyOnHidden
    >
      {error && <Alert type="error" showIcon message={error} style={{ marginBottom: 16 }} closable onClose={() => setError(undefined)} />}

      <Form form={form} layout="vertical">
        <Row gutter={16}>
          <Col xs={24} md={12}>
            <Form.Item label="Accepting applications" name="isAcceptingApplications" valuePropName="checked">
              <Switch checkedChildren="Open" unCheckedChildren="Closed" />
            </Form.Item>
          </Col>
          <Col xs={24} md={12}>
            <Form.Item label="Places available" name="maxCapacity" extra="Leave empty for no limit.">
              <InputNumber min={1} style={{ width: '100%' }} placeholder="No limit" />
            </Form.Item>
          </Col>
          <Col xs={24} md={12}>
            <Form.Item label="Applications open" name="applicationOpenDate">
              <DatePicker style={{ width: '100%' }} format="DD MMM YYYY" />
            </Form.Item>
          </Col>
          <Col xs={24} md={12}>
            <Form.Item label="Applications close" name="applicationCloseDate">
              <DatePicker style={{ width: '100%' }} format="DD MMM YYYY" />
            </Form.Item>
          </Col>
        </Row>

        {/* The fee, and whether there is one at all. A school that charges
            nothing should not have applications waiting on a payment. */}
        <Row gutter={16}>
          <Col xs={24} md={12}>
            <Form.Item
              label="Charge an application fee"
              name="isApplicationFeeRequired"
              valuePropName="checked"
              extra="Turn this off and applications go straight to review."
            >
              <Switch checkedChildren="Charged" unCheckedChildren="Free" />
            </Form.Item>
          </Col>
          <Col xs={24} md={12}>
            <Form.Item label="Fee amount (R)" name="applicationFeeAmount">
              <InputNumber min={0} precision={2} style={{ width: '100%' }} disabled={!feeRequired} />
            </Form.Item>
          </Col>
        </Row>

        {!feeRequired && (
          <Alert
            type="info"
            showIcon
            style={{ marginBottom: 16 }}
            message="Applications for this grade will not be charged"
            description="A submitted application goes straight to review. The amount above is kept, so turning the fee back on costs nothing."
          />
        )}

        <Row gutter={16}>
          <Col xs={12} md={6}>
            <Form.Item label="Youngest age" name="minimumAge">
              <InputNumber min={0} max={25} style={{ width: '100%' }} placeholder="Any" />
            </Form.Item>
          </Col>
          <Col xs={12} md={6}>
            <Form.Item label="Oldest age" name="maximumAge">
              <InputNumber min={0} max={25} style={{ width: '100%' }} placeholder="Any" />
            </Form.Item>
          </Col>
          <Col xs={24} md={12}>
            <Form.Item label="An offer stands for (days)" name="offerExpiryDays">
              <InputNumber min={1} style={{ width: '100%' }} />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={16}>
          <Col xs={24} md={12}>
            <Form.Item label="Interview required" name="isInterviewRequired" valuePropName="checked">
              <Switch />
            </Form.Item>
          </Col>
          <Col xs={24} md={12}>
            <Form.Item label="Assessment required" name="isAssessmentRequired" valuePropName="checked">
              <Switch />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item
          label="Documents an applicant must provide"
          name="requiredDocuments"
          extra={<Text type="secondary">One per line, as the parent will read them.</Text>}
        >
          <Input.TextArea rows={3} maxLength={1000} showCount />
        </Form.Item>

        <Form.Item label="Notes for the school" name="notes">
          <Input.TextArea rows={2} maxLength={2000} showCount />
        </Form.Item>
      </Form>
    </Modal>
  );
}
