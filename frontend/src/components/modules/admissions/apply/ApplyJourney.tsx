'use client';

import React, { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Alert, Button, Card, Empty, Form, List, Space, Spin, Steps, Tag, Typography, message,
} from 'antd';
import { ArrowLeftOutlined, ArrowRightOutlined, LogoutOutlined, PlusOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { ProtectedRoute } from '@/components/shared/ProtectedRoute';
import { useAuthActions } from '@/providers/auth';
import { useBrandingState } from '@/providers/branding';
import {
  ApplicationProvider, useApplicationActions, useApplicationState,
} from '@/providers/admissions/applications';
import { ApplicantParentProvider, useApplicantParentState } from '@/providers/admissions/applicant_parents';
import { ApplicationDocumentProvider } from '@/providers/admissions/application_documents';
import { ApplicationFeeProvider } from '@/providers/admissions/application_fees';
import {
  AdmissionSettingsProvider, useAdmissionSettingsActions,
} from '@/providers/admissions/admission_settings';
import type { IOpenIntake } from '@/providers/admissions/admission_settings/context';
import type { ICreateApplication } from '@/providers/admissions/shared/interfaces';
import LearnerStep from './LearnerStep';
import ParentsStep from './ParentsStep';
import DocumentsStep from './DocumentsStep';
import ReviewStep from './ReviewStep';
import { learnerSchema } from './schema';

const { Title, Paragraph, Text } = Typography;

const DRAFT = 1;

const statusLabels: Record<number, { label: string; color: string }> = {
  1: { label: 'Not sent yet', color: 'default' },
  2: { label: 'Submitted', color: 'processing' },
  3: { label: 'Waiting for the fee', color: 'warning' },
  4: { label: 'Being reviewed', color: 'processing' },
  5: { label: 'Interview arranged', color: 'processing' },
  6: { label: 'Assessment arranged', color: 'processing' },
  7: { label: 'Decision pending', color: 'processing' },
  8: { label: 'Offered a place', color: 'success' },
  9: { label: 'Waitlisted', color: 'warning' },
  10: { label: 'Not successful', color: 'error' },
  11: { label: 'Offer accepted', color: 'success' },
  12: { label: 'Offer declined', color: 'default' },
  13: { label: 'Withdrawn', color: 'default' },
  14: { label: 'Enrolled', color: 'success' },
};

/**
 * Where a prospective parent fills in an application.
 *
 * This page was a placeholder: four statistics hardcoded to zero and four
 * buttons that did nothing. Everything behind it existed — the application, its
 * parents, its documents, its fee, the lifecycle — and there was no way in.
 */
function ApplyContent() {
  const { branding } = useBrandingState();
  const { applicantParents } = useApplicantParentState();
  const { signOut } = useAuthActions();

  const { applications, application, isPending } = useApplicationState();
  const { getMineAsync, getAsync, createAsync, updateAsync, submitAsync } = useApplicationActions();
  const { getOpenIntakesAsync } = useAdmissionSettingsActions();

  const [form] = Form.useForm();
  const [intakes, setIntakes] = useState<IOpenIntake[]>([]);
  const [loadingIntakes, setLoadingIntakes] = useState(true);
  const [workingOn, setWorkingOn] = useState<string | null>(null);
  /* Whether the form is open on a brand new application. Explicit, because
     there is nothing else to infer it from: a new one has no id yet, and
     "has the form been touched" is false until the parent types — so Start
     an application appeared to do nothing at all. */
  const [starting, setStarting] = useState(false);
  const [step, setStep] = useState(0);
  const [saving, setSaving] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | undefined>();

  useEffect(() => {
    getMineAsync().catch(() => undefined);
    getOpenIntakesAsync()
      .then(setIntakes)
      .catch(() => undefined)
      .finally(() => setLoadingIntakes(false));
  }, []);

  const refresh = useCallback(() => {
    if (workingOn) getAsync(workingOn);
  }, [workingOn, getAsync]);

  const sayWhatTheServerSaid = (e: unknown, fallback: string) => {
    const abp = (e as { response?: { data?: { error?: { message?: string; details?: string } } } })
      ?.response?.data?.error;
    setError(abp?.message || abp?.details || fallback);
  };

  const startNew = () => {
    setWorkingOn(null);
    setStarting(true);
    setStep(0);
    setError(undefined);
    form.resetFields();
    form.setFieldsValue({ isSACitizen: true });
  };

  const openExisting = async (id: string) => {
    setWorkingOn(id);
    setStarting(false);
    setError(undefined);
    await getAsync(id);
    setStep(0);
  };

  /* Fill the learner form from whatever is already saved, so coming back to a
     draft shows what you wrote rather than an empty page. */
  useEffect(() => {
    if (!workingOn || !application || application.id !== workingOn) return;

    form.setFieldsValue({
      intake: `${application.academicYearId}|${application.applyingForGradeId ?? ''}`,
      firstName: application.firstName,
      middleName: application.middleName ?? '',
      lastName: application.lastName,
      dateOfBirth: application.dateOfBirth ? dayjs(application.dateOfBirth) : undefined,
      gender: application.gender,
      isSACitizen: application.isSACitizen,
      idNumber: application.idNumber ?? '',
      passportNumber: application.passportNumber ?? '',
      previousSchool: application.previousSchool ?? '',
    });
  }, [workingOn, application, form]);

  const current = workingOn && application?.id === workingOn ? application : undefined;
  const locked = !!current && current.status !== DRAFT;

  const chosenIntake = useMemo(() => {
    const key = form.getFieldValue('intake');
    return intakes.find((i) => `${i.academicYearId}|${i.gradeId ?? ''}` === key);
  }, [intakes, form, step, current]);

  const saveLearner = async () => {
    const parsed = learnerSchema.safeParse(form.getFieldsValue());
    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message);
      return false;
    }

    const [academicYearId, gradeId] = parsed.data.intake.split('|');
    if (!gradeId) {
      setError('That intake covers every grade. Ask the school which grade to apply for.');
      return false;
    }

    const payload = {
      academicYearId,
      applyingForGradeId: gradeId,
      firstName: parsed.data.firstName,
      middleName: parsed.data.middleName || undefined,
      lastName: parsed.data.lastName,
      dateOfBirth: parsed.data.dateOfBirth.toISOString(),
      gender: parsed.data.gender,
      isSACitizen: parsed.data.isSACitizen,
      idNumber: parsed.data.idNumber || undefined,
      passportNumber: parsed.data.passportNumber || undefined,
      previousSchool: parsed.data.previousSchool || undefined,
    };

    setSaving(true);
    setError(undefined);
    try {
      if (workingOn) {
        await updateAsync(workingOn, payload);
        await getAsync(workingOn);
      } else {
        /* No address: the server takes it from the signed-in account, which is
           this parent's own and the only right answer here. */
        const created = await createAsync(payload as ICreateApplication);
        if (created?.id) {
          setWorkingOn(created.id);
          setStarting(false);
          await getAsync(created.id);
        }
        await getMineAsync();
      }
      return true;
    } catch (e) {
      sayWhatTheServerSaid(e, 'Could not save these details.');
      return false;
    } finally {
      setSaving(false);
    }
  };

  const submit = async () => {
    if (!workingOn) return;
    setSubmitting(true);
    setError(undefined);
    try {
      await submitAsync(workingOn);
      message.success('Submitted. The school has it now.');
      await getAsync(workingOn);
      await getMineAsync();
    } catch (e) {
      sayWhatTheServerSaid(e, 'Could not submit this application.');
    } finally {
      setSubmitting(false);
    }
  };

  const next = async () => {
    if (step === 0) {
      const ok = await saveLearner();
      if (!ok) return;
    }
    setError(undefined);
    setStep((s) => Math.min(s + 1, 3));
  };

  /* ---------- the list of what this parent has started ---------- */
  if (!workingOn && !starting) {
    const mine = applications ?? [];

    return (
      <div style={{ maxWidth: 820, margin: '0 auto', padding: '32px 16px' }}>
        <Space style={{ width: '100%', justifyContent: 'space-between', marginBottom: 8 }}>
          <Title level={3} style={{ margin: 0 }}>
            {branding.schoolName}
          </Title>
          <Button icon={<LogoutOutlined />} onClick={signOut}>
            Sign out
          </Button>
        </Space>
        <Paragraph type="secondary">Applications you have started.</Paragraph>

        {isPending && <Spin />}

        {!isPending && mine.length === 0 && (
          <Card>
            <Empty description="You have not started an application yet" image={Empty.PRESENTED_IMAGE_SIMPLE}>
              <Button type="primary" icon={<PlusOutlined />} onClick={startNew} disabled={loadingIntakes}>
                Start an application
              </Button>
            </Empty>
          </Card>
        )}

        {!isPending && mine.length > 0 && (
          <>
            <List
              bordered
              dataSource={mine}
              renderItem={(a) => (
                <List.Item
                  actions={[
                    <Button key="o" type="link" onClick={() => openExisting(a.id)}>
                      {a.status === DRAFT ? 'Carry on' : 'View'}
                    </Button>,
                  ]}
                >
                  <List.Item.Meta
                    title={
                      <Space wrap>
                        <Text strong>{a.fullName}</Text>
                        <Tag color={statusLabels[a.status]?.color}>
                          {statusLabels[a.status]?.label ?? 'In progress'}
                        </Tag>
                      </Space>
                    }
                    description={`${a.gradeName ?? ''} ${a.academicYearName ?? ''} · ${a.applicationNumber}`}
                  />
                </List.Item>
              )}
            />
            <Button style={{ marginTop: 12 }} icon={<PlusOutlined />} onClick={startNew}>
              Apply for another learner
            </Button>
          </>
        )}

        {!loadingIntakes && intakes.length === 0 && (
          <Alert
            type="warning"
            showIcon
            style={{ marginTop: 16 }}
            message="This school is not taking applications at the moment"
            description="Nothing is open to apply for. Contact the school to ask when applications open."
          />
        )}
      </div>
    );
  }

  /* ---------- the journey ---------- */
  return (
    <div style={{ maxWidth: 900, margin: '0 auto', padding: '32px 16px' }}>
      <Space style={{ width: '100%', justifyContent: 'space-between', marginBottom: 16 }}>
        <Button
          icon={<ArrowLeftOutlined />}
          onClick={() => {
            setWorkingOn(null);
            setStarting(false);
            setStep(0);
            form.resetFields();
          }}
        >
          My applications
        </Button>
        {current?.applicationNumber && <Text type="secondary">{current.applicationNumber}</Text>}
      </Space>

      <Steps
        current={step}
        onChange={(s) => { if (s < step || locked) setStep(s); }}
        style={{ marginBottom: 24 }}
        items={[
          { title: 'Learner' },
          { title: 'Parents', disabled: !workingOn },
          { title: 'Documents', disabled: !workingOn },
          { title: 'Review' },
        ]}
      />

      {error && (
        <Alert type="error" showIcon message={error} closable onClose={() => setError(undefined)} style={{ marginBottom: 16 }} />
      )}

      {locked && step < 3 && (
        <Alert
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
          message="This application is with the school"
          description="You can still add documents, but the learner's details and the parents are fixed now."
        />
      )}

      <Card>
        {step === 0 && <LearnerStep form={form} intakes={intakes} locked={locked} />}
        {step === 1 && workingOn && (
          <ParentsStep applicationId={workingOn} locked={locked} onChanged={refresh} />
        )}
        {step === 2 && workingOn && (
          <DocumentsStep
            applicationId={workingOn}
            requiredDocuments={chosenIntake?.requiredDocuments}
            locked={false}
            onChanged={refresh}
          />
        )}
        {step === 3 && current && (
          <ReviewStep
            application={current}
            parents={applicantParents ?? []}
            documentCount={current.documentCount ?? 0}
            onSubmit={submit}
            submitting={submitting}
            onRefresh={refresh}
          />
        )}
        {step === 3 && !current && <Spin />}
      </Card>

      {step < 3 && (
        <Space style={{ marginTop: 16 }}>
          {step > 0 && <Button onClick={() => setStep((s) => s - 1)}>Back</Button>}
          <Button type="primary" icon={<ArrowRightOutlined />} loading={saving} onClick={next}>
            {step === 0 ? 'Save and continue' : 'Continue'}
          </Button>
        </Space>
      )}
    </div>
  );
}

export default function ApplyJourney() {
  return (
    <ProtectedRoute allowedRoles={['Applicant']}>
      <AdmissionSettingsProvider>
        <ApplicationProvider>
          <ApplicantParentProvider>
            <ApplicationDocumentProvider>
              <ApplicationFeeProvider>
                <ApplyContent />
              </ApplicationFeeProvider>
            </ApplicationDocumentProvider>
          </ApplicantParentProvider>
        </ApplicationProvider>
      </AdmissionSettingsProvider>
    </ProtectedRoute>
  );
}
