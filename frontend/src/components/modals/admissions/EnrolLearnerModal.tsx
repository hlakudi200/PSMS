'use client';

import React, { useEffect, useState } from 'react';
import {
  Alert, Checkbox, Form, Input, Modal, Result, Select, Space, Tag, Typography, message,
} from 'antd';
import { z } from 'zod';
import { useEnrollmentActions } from '@/providers/admissions/enrollments';
import type {
  IClassAvailability, IEnrollment,
} from '@/providers/admissions/shared/interfaces';

const { Paragraph, Text } = Typography;

const schema = z.object({
  classId: z.string().min(1, 'Choose the class this learner joins.'),
  admissionNumber: z.string().trim().max(50).optional().or(z.literal('')),
  emergencyContactName: z.string().trim().max(100).optional().or(z.literal('')),
  emergencyContactPhone: z.string().trim().max(20).optional().or(z.literal('')),
  medicalConditions: z.string().trim().max(1000).optional().or(z.literal('')),
  formsSubmitted: z.boolean(),
  popiaConsentGiven: z.boolean(),
});

type FormValues = z.infer<typeof schema>;

/**
 * Putting an approved applicant on the roll.
 *
 * This is the end of admissions and the start of everything else: it creates
 * the Student record, provisions the learner's portal sign-in, links the
 * parents to them, and takes one of the grade's places. Until this existed the
 * whole funnel stopped at Approved — the endpoint was written and tested, and
 * no screen called it.
 *
 * The sign-in it returns is shown once, here. The server withholds it on every
 * later read, so there is no second chance to copy it down.
 */
export default function EnrolLearnerModal({
  open,
  applicationId,
  applicantName,
  gradeId,
  gradeName,
  onClose,
  onEnrolled,
}: {
  open: boolean;
  applicationId: string;
  applicantName?: string;
  gradeId?: string;
  gradeName?: string;
  onClose: () => void;
  onEnrolled: () => void;
}) {
  const { getAvailableClassesAsync, completeEnrollmentAsync } = useEnrollmentActions();
  const [form] = Form.useForm<FormValues>();
  const [classes, setClasses] = useState<IClassAvailability[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | undefined>();
  const [done, setDone] = useState<IEnrollment | undefined>();

  useEffect(() => {
    if (!open) return;
    setError(undefined);
    setDone(undefined);
    form.resetFields();
    form.setFieldsValue({ formsSubmitted: false, popiaConsentGiven: false });

    if (!gradeId) {
      setLoading(false);
      setError('This application has no grade, so there is no class to put the learner in.');
      return;
    }

    setLoading(true);
    getAvailableClassesAsync(gradeId)
      .then(setClasses)
      .catch(() => setError('Could not load the classes for this grade.'))
      .finally(() => setLoading(false));
  }, [open, gradeId]);

  const enrol = async () => {
    const parsed = schema.safeParse(form.getFieldsValue(true));
    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message);
      return;
    }

    /* POPIA: the school is about to hold a child's records. Consent is the
       lawful basis for that, so it is asked for plainly rather than assumed,
       and refused rather than defaulted. */
    if (!parsed.data.popiaConsentGiven) {
      setError('The family must have given POPIA consent before the school can hold this learner’s records.');
      return;
    }

    setSaving(true);
    setError(undefined);
    try {
      const enrolment = await completeEnrollmentAsync({
        applicationId,
        classId: parsed.data.classId,
        admissionNumber: parsed.data.admissionNumber || undefined,
        formsSubmitted: parsed.data.formsSubmitted,
        emergencyContactName: parsed.data.emergencyContactName || undefined,
        emergencyContactPhone: parsed.data.emergencyContactPhone || undefined,
        medicalConditions: parsed.data.medicalConditions || undefined,
        popiaConsentGiven: true,
      });
      message.success('Enrolled.');
      setDone(enrolment);
      onEnrolled();
    } catch (e) {
      const abp = (e as { response?: { data?: { error?: { message?: string; details?: string } } } })
        ?.response?.data?.error;
      setError(abp?.message || abp?.details || 'Could not enrol this learner.');
    } finally {
      setSaving(false);
    }
  };

  /* Once it is done the form is gone: the learner is on the roll, and the only
     thing left to do with this dialog is write down the sign-in. */
  if (done) {
    return (
      <Modal
        open={open}
        title={null}
        footer={null}
        onCancel={onClose}
        width={600}
        destroyOnHidden
      >
        <Result
          status="success"
          title={`${done.applicantName || applicantName} is on the roll`}
          subTitle={
            <Space direction="vertical" size={2}>
              <Text>Admission number <Text strong copyable>{done.admissionNumber}</Text></Text>
              {done.assignedClassName && <Text type="secondary">Class {done.assignedClassName}</Text>}
            </Space>
          }
        />

        {/* #437: enrolled either way, but nobody can be contacted about this learner yet. */}
        {done.guardiansLinked === 0 && (
          <Alert
            type="error"
            showIcon
            style={{ marginBottom: 16 }}
            message="No parent or guardian is linked to this learner"
            description="The application had no parent on it, so the school has nobody to contact about this learner, and no one can see their report cards. Link a guardian from the learner's profile."
          />
        )}

        {done.loginUserName && (
          <Alert
            type="warning"
            showIcon
            message="Write this down now — it is shown once"
            description={
              <Space direction="vertical" size={4} style={{ marginTop: 4 }}>
                <Text>
                  Username <Text strong copyable>{done.loginUserName}</Text>
                </Text>
                {done.temporaryPassword && (
                  <Text>
                    Temporary password <Text strong copyable>{done.temporaryPassword}</Text>
                  </Text>
                )}
                <Text type="secondary">
                  Hand these to the family. The password should be changed on first sign-in,
                  and this screen is the only place either appears.
                </Text>
              </Space>
            }
          />
        )}
      </Modal>
    );
  }

  return (
    <Modal
      open={open}
      title={applicantName ? `Enrol ${applicantName}` : 'Enrol this learner'}
      okText="Enrol"
      okButtonProps={{ loading: saving }}
      onOk={enrol}
      onCancel={onClose}
      width={640}
      destroyOnHidden
    >
      {error && <Alert type="error" showIcon message={error} style={{ marginBottom: 16 }} />}

      <Paragraph type="secondary">
        This creates the learner&rsquo;s record and their sign-in, links the parents to them, and
        takes one of {gradeName ?? 'the grade'}&rsquo;s places. It cannot be undone from here.
      </Paragraph>

      <Form form={form} layout="vertical">
        <Form.Item
          label="Class"
          name="classId"
          rules={[{ required: true }]}
          extra="Only classes for this grade, with the room they have left."
        >
          <Select
            loading={loading}
            placeholder={classes.length ? 'Choose a class' : 'No classes set up for this grade'}
            disabled={!classes.length}
            options={classes.map((c) => ({
              value: c.classId,
              label: c.className,
              disabled: !c.isAvailable,
            }))}
            optionRender={(option) => {
              const c = classes.find((x) => x.classId === option.value);
              return (
                <Space>
                  <Text>{c?.className}</Text>
                  {c?.teacherName && <Text type="secondary">{c.teacherName}</Text>}
                  <Tag color={c?.isAvailable ? 'green' : 'red'} style={{ margin: 0 }}>
                    {c?.isAvailable ? `${c?.availableSpots} free` : 'Full'}
                  </Tag>
                </Space>
              );
            }}
          />
        </Form.Item>

        <Form.Item
          label="Admission number"
          name="admissionNumber"
          extra="Leave empty and the school's next number is used."
        >
          <Input maxLength={50} placeholder="Generated if left empty" />
        </Form.Item>

        <Space size={16} style={{ display: 'flex' }} align="start">
          <Form.Item label="Emergency contact" name="emergencyContactName" style={{ flex: 1 }}>
            <Input maxLength={100} placeholder="Who to call" />
          </Form.Item>
          <Form.Item label="Their number" name="emergencyContactPhone" style={{ flex: 1 }}>
            <Input maxLength={20} placeholder="082 000 0000" />
          </Form.Item>
        </Space>

        <Form.Item
          label="Medical conditions or allergies"
          name="medicalConditions"
          extra="Anything the school must know about on the first day."
        >
          <Input.TextArea rows={3} maxLength={1000} showCount />
        </Form.Item>

        <Form.Item name="formsSubmitted" valuePropName="checked">
          <Checkbox>The family has returned the enrolment forms</Checkbox>
        </Form.Item>

        <Form.Item name="popiaConsentGiven" valuePropName="checked">
          <Checkbox>
            The family has given POPIA consent for the school to hold this learner&rsquo;s records
          </Checkbox>
        </Form.Item>
      </Form>
    </Modal>
  );
}
