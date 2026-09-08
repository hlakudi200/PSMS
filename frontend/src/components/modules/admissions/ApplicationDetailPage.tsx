'use client';

import React, { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { usePortalBase } from '@/utils/portal-base';
import {
  Card,
  Descriptions,
  Tag,
  Tabs,
  Table,
  Space,
  Button,
  Typography,
  Spin,
  Empty,
  Steps,
  Alert,
  message,
  Row,
  Col,
  Statistic,
} from 'antd';
import {
  ArrowLeftOutlined,
  TeamOutlined,
  FileOutlined,
  AudioOutlined,
  FormOutlined,
  DollarOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { ApplicationProvider, useApplicationState, useApplicationActions } from '@/providers/admissions/applications';
import { ApplicantParentProvider, useApplicantParentState, useApplicantParentActions } from '@/providers/admissions/applicant_parents';
import { ApplicationDocumentProvider, useApplicationDocumentState, useApplicationDocumentActions } from '@/providers/admissions/application_documents';
import { AdmissionInterviewProvider, useAdmissionInterviewState, useAdmissionInterviewActions } from '@/providers/admissions/admission_interviews';
import { AdmissionAssessmentProvider, useAdmissionAssessmentState, useAdmissionAssessmentActions } from '@/providers/admissions/admission_assessments';
import { ApplicationFeeProvider, useApplicationFeeState, useApplicationFeeActions } from '@/providers/admissions/application_fees';
import {
  WorkflowInstanceProvider,
  useWorkflowInstanceState,
  useWorkflowInstanceActions,
} from '@/providers/workflow/workflow-instances';
import {
  WorkflowStatus,
  WorkflowStatusLabels,
  WorkflowEntityType,
} from '@/providers/workflow/shared/interfaces';
import type {
  IApplicantParent,
  IApplicationDocument,
  IAdmissionInterview,
  IAdmissionAssessment,
} from '@/providers/admissions/shared/interfaces';

const { Title, Text } = Typography;

const statusSteps = ['Draft', 'Submitted', 'UnderReview', 'Approved', 'Enrolled'];
const statusStepIndex: Record<string, number> = {
  Draft: 0, Submitted: 1, UnderReview: 2, Approved: 3, Enrolled: 4,
  Rejected: -1, Waitlisted: -1, Withdrawn: -1,
};

function getStatusColor(status: string): string {
  const map: Record<string, string> = {
    Draft: 'default', Submitted: 'blue', UnderReview: 'orange',
    Approved: 'green', Rejected: 'red', Waitlisted: 'gold',
    Enrolled: 'purple', Withdrawn: 'default',
  };
  return map[status] ?? 'default';
}

// ─── Parents Tab ───────────────────────────────────────────────
function ParentsSection({ applicationId }: { applicationId: string }) {
  const { applicantParents, isPending } = useApplicantParentState();
  const { getAllByApplicationAsync } = useApplicantParentActions();

  useEffect(() => {
    getAllByApplicationAsync(applicationId);
  }, [applicationId, getAllByApplicationAsync]);

  const columns = [
    { title: 'Name', dataIndex: 'fullName', key: 'fullName' },
    { title: 'Relationship', dataIndex: 'relationshipDisplayName', key: 'relationship' },
    { title: 'Email', dataIndex: 'email', key: 'email' },
    { title: 'Phone', dataIndex: 'phoneNumber', key: 'phone' },
    { title: 'Occupation', dataIndex: 'occupation', key: 'occupation', render: (v: string) => v || '-' },
    {
      title: 'Roles', key: 'roles',
      render: (_: unknown, r: IApplicantParent) => (
        <Space size={[0, 4]} wrap>
          {r.isPrimaryContact && <Tag color="blue">Primary</Tag>}
          {r.isFinanciallyResponsible && <Tag color="gold">Financial</Tag>}
        </Space>
      ),
    },
  ];

  return (
    <Table<IApplicantParent>
      dataSource={applicantParents ?? []}
      columns={columns}
      rowKey="id"
      loading={isPending}
      pagination={false}
      size="small"
      locale={{ emptyText: <Empty description="No parents linked" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
    />
  );
}

// ─── Documents Tab ─────────────────────────────────────────────
function DocumentsSection({ applicationId }: { applicationId: string }) {
  const { documents: applicationDocuments, isPending } = useApplicationDocumentState();
  const { getAllByApplicationAsync, getDownloadUrlAsync } = useApplicationDocumentActions();
  const [openingId, setOpeningId] = useState<string | null>(null);

  useEffect(() => {
    getAllByApplicationAsync(applicationId);
  }, [applicationId, getAllByApplicationAsync]);

  // The documents bucket is private — there is no public link. Fetch a
  // short-lived signed URL on click and open it. (Opening a blank tab first
  // and then redirecting it keeps the user gesture so the browser doesn't
  // block the pop-up while we await the URL.)
  const handleView = async (id: string) => {
    setOpeningId(id);
    // Open the tab synchronously (inside the click gesture) so the browser
    // doesn't block it while we await the signed URL. NOTE: do NOT pass
    // "noopener" in the features arg — per spec that makes window.open return
    // null, and we need the handle to redirect it. We sever opener manually.
    const tab = window.open('', '_blank');
    if (tab) tab.opener = null;
    try {
      const url = await getDownloadUrlAsync(id);
      if (url && tab) tab.location.href = url;
      else {
        tab?.close();
        if (!url) message.error('Could not open the document. Please try again.');
      }
    } finally {
      setOpeningId(null);
    }
  };

  const columns = [
    { title: 'Document', dataIndex: 'documentName', key: 'documentName' },
    { title: 'Category', dataIndex: 'categoryDisplayName', key: 'category' },
    { title: 'Size', dataIndex: 'fileSizeDisplay', key: 'size', width: 100 },
    {
      title: 'Uploaded', dataIndex: 'uploadedDate', key: 'uploaded',
      render: (d: string) => dayjs(d).format('DD MMM YYYY'),
    },
    {
      title: 'Status', key: 'status',
      render: (_: unknown, r: IApplicationDocument) => (
        <Space>
          {r.isRequired && <Tag color="orange">Required</Tag>}
          {r.isVerified ? <Tag color="green">Verified</Tag> : <Tag color="default">Pending</Tag>}
        </Space>
      ),
    },
    {
      title: '', key: 'action', width: 80,
      render: (_: unknown, r: IApplicationDocument) => (
        <Button
          type="link"
          size="small"
          loading={openingId === r.id}
          onClick={() => handleView(r.id)}
        >
          View
        </Button>
      ),
    },
  ];

  return (
    <Table<IApplicationDocument>
      dataSource={applicationDocuments ?? []}
      columns={columns}
      rowKey="id"
      loading={isPending}
      pagination={false}
      size="small"
      locale={{ emptyText: <Empty description="No documents uploaded" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
    />
  );
}

// ─── Interview Tab ─────────────────────────────────────────────
function InterviewSection({ applicationId }: { applicationId: string }) {
  const { interview: admissionInterview, isPending } = useAdmissionInterviewState();
  const { getByApplicationAsync } = useAdmissionInterviewActions();

  useEffect(() => {
    getByApplicationAsync(applicationId);
  }, [applicationId, getByApplicationAsync]);

  if (isPending) return <Spin />;
  if (!admissionInterview) return <Empty description="No interview scheduled" image={Empty.PRESENTED_IMAGE_SIMPLE} />;

  const iv = admissionInterview as IAdmissionInterview;
  return (
    <Descriptions bordered size="small" column={{ xs: 1, sm: 2 }}>
      <Descriptions.Item label="Date">{dayjs(iv.scheduledDate).format('DD MMM YYYY')}</Descriptions.Item>
      <Descriptions.Item label="Time">{iv.scheduledTime}</Descriptions.Item>
      <Descriptions.Item label="Interviewer">{iv.interviewerName}</Descriptions.Item>
      <Descriptions.Item label="Status"><Tag color={iv.statusDisplayName === 'Completed' ? 'green' : 'blue'}>{iv.statusDisplayName}</Tag></Descriptions.Item>
      <Descriptions.Item label="Location">{iv.location || iv.meetingLink || 'Not specified'}</Descriptions.Item>
      {iv.rating != null && <Descriptions.Item label="Rating">{iv.rating} / 5</Descriptions.Item>}
      {iv.recommended != null && (
        <Descriptions.Item label="Recommended">
          <Tag color={iv.recommended ? 'green' : 'red'}>{iv.recommended ? 'Yes' : 'No'}</Tag>
        </Descriptions.Item>
      )}
      {iv.notes && <Descriptions.Item label="Notes" span={2}>{iv.notes}</Descriptions.Item>}
    </Descriptions>
  );
}

// ─── Assessment Tab ────────────────────────────────────────────
function AssessmentSection({ applicationId }: { applicationId: string }) {
  const { assessment: admissionAssessment, isPending } = useAdmissionAssessmentState();
  const { getByApplicationAsync } = useAdmissionAssessmentActions();

  useEffect(() => {
    getByApplicationAsync(applicationId);
  }, [applicationId, getByApplicationAsync]);

  if (isPending) return <Spin />;
  if (!admissionAssessment) return <Empty description="No assessment scheduled" image={Empty.PRESENTED_IMAGE_SIMPLE} />;

  const a = admissionAssessment as IAdmissionAssessment;
  return (
    <div>
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col xs={8}>
          <Card size="small">
            <Statistic title="Score" value={a.totalScore} suffix={`/ ${a.maxScore}`} />
          </Card>
        </Col>
        <Col xs={8}>
          <Card size="small">
            <Statistic title="Percentage" value={a.percentage} suffix="%" valueStyle={{ color: a.passed ? '#3f8600' : '#cf1322' }} />
          </Card>
        </Col>
        <Col xs={8}>
          <Card size="small">
            <Statistic title="Result" valueRender={() => <Tag color={a.passed ? 'green' : 'red'}>{a.passed ? 'Passed' : 'Failed'}</Tag>} />
          </Card>
        </Col>
      </Row>
      <Descriptions bordered size="small" column={{ xs: 1, sm: 2 }}>
        <Descriptions.Item label="Type">{a.typeDisplayName}</Descriptions.Item>
        <Descriptions.Item label="Date">{dayjs(a.scheduledDate).format('DD MMM YYYY')}</Descriptions.Item>
        <Descriptions.Item label="Grade">{a.gradeName}</Descriptions.Item>
        <Descriptions.Item label="Assessor">{a.assessorName}</Descriptions.Item>
        {a.subjects && <Descriptions.Item label="Subjects" span={2}>{a.subjects}</Descriptions.Item>}
        {a.feedback && <Descriptions.Item label="Feedback" span={2}>{a.feedback}</Descriptions.Item>}
      </Descriptions>
    </div>
  );
}

// ─── Fee Section ───────────────────────────────────────────────
function FeeSection({ applicationId }: { applicationId: string }) {
  const { applicationFee, isPending } = useApplicationFeeState();
  const { getByApplicationAsync } = useApplicationFeeActions();

  useEffect(() => {
    getByApplicationAsync(applicationId);
  }, [applicationId, getByApplicationAsync]);

  if (isPending) return <Spin />;
  if (!applicationFee) return <Empty description="No fee record" image={Empty.PRESENTED_IMAGE_SIMPLE} />;

  return (
    <Descriptions bordered size="small" column={{ xs: 1, sm: 2 }}>
      <Descriptions.Item label="Amount">{applicationFee.amountDisplay}</Descriptions.Item>
      <Descriptions.Item label="Status">
        <Tag color={applicationFee.isPaid ? 'green' : 'orange'}>{applicationFee.statusDisplayName}</Tag>
      </Descriptions.Item>
      {applicationFee.paymentDate && (
        <Descriptions.Item label="Payment Date">{dayjs(applicationFee.paymentDate).format('DD MMM YYYY')}</Descriptions.Item>
      )}
      {applicationFee.paymentMethodDisplayName && (
        <Descriptions.Item label="Payment Method">{applicationFee.paymentMethodDisplayName}</Descriptions.Item>
      )}
      {applicationFee.paymentReference && (
        <Descriptions.Item label="Reference">{applicationFee.paymentReference}</Descriptions.Item>
      )}
      {applicationFee.receiptNumber && (
        <Descriptions.Item label="Receipt">{applicationFee.receiptNumber}</Descriptions.Item>
      )}
    </Descriptions>
  );
}

// ─── Approval Workflow Card (WF-23) ────────────────────────────
// Surfaces the application's approval workflow inline, so the principal can see
// what stage it's at and jump straight to taking action. Self-contained in its
// own WorkflowInstanceProvider so it doesn't touch the admissions state.
// The application detail page is reached from the principal portal and, via the
// workflow "View full application" link, from the admin portal. Derive the portal
// root from the path so internal links (workflow instance, Back) stay in-portal.

const workflowStatusColor: Record<number, string> = {
  [WorkflowStatus.NotStarted]: 'default',
  [WorkflowStatus.InProgress]: 'processing',
  [WorkflowStatus.Completed]: 'green',
  [WorkflowStatus.Rejected]: 'red',
  [WorkflowStatus.Cancelled]: 'default',
  [WorkflowStatus.Recalled]: 'purple',
};

function ApplicationWorkflowCardInner({ applicationId, feePaid }: { applicationId: string; feePaid: boolean }) {
  const router = useRouter();
  const portalBase = usePortalBase();
  const { instance: wfInstance } = useWorkflowInstanceState();
  const { getByEntityAsync } = useWorkflowInstanceActions();
  // Track first-fetch completion so we show a loader (not a premature "none"
  // note) until the lookup resolves. GetByEntity returns only an ACTIVE instance.
  const [loaded, setLoaded] = useState(false);

  useEffect(() => {
    let active = true;
    setLoaded(false);
    Promise.resolve(getByEntityAsync(WorkflowEntityType.Application, applicationId))
      .finally(() => { if (active) setLoaded(true); });
    return () => { active = false; };
  }, [applicationId, getByEntityAsync]);

  if (!loaded) {
    return <Card size="small" title="Approval Workflow" loading style={{ marginBottom: 16 }} />;
  }

  // GetByEntity 404s when there's no ACTIVE workflow — either none has started
  // (fee unpaid) or it already ran to completion. Tailor the copy so a decided
  // application doesn't wrongly read as "never started".
  if (!wfInstance) {
    return (
      <Card size="small" title="Approval Workflow" style={{ marginBottom: 16 }}>
        <Text type="secondary">
          {feePaid
            ? 'No active approval workflow. If a decision has been made, the workflow has already completed.'
            : 'No approval workflow yet — it starts automatically once the application fee is paid.'}
        </Text>
      </Card>
    );
  }

  return (
    <Card
      size="small"
      title="Approval Workflow"
      style={{ marginBottom: 16 }}
      extra={
        <Button type="link" onClick={() => router.push(`${portalBase}/workflow/instances/${wfInstance.id}`)}>
          View / take action
        </Button>
      }
    >
      <Space size="large" wrap>
        <span>
          <Text type="secondary">Current step: </Text>
          <Text strong>{wfInstance.currentStepName ?? '—'}</Text>
          {wfInstance.currentStepAssignedRole && (
            <Tag style={{ marginLeft: 8 }}>{wfInstance.currentStepAssignedRole}</Tag>
          )}
        </span>
        <span>
          <Text type="secondary">Status: </Text>
          <Tag color={workflowStatusColor[wfInstance.status] ?? 'default'}>
            {WorkflowStatusLabels[wfInstance.status] ?? '—'}
          </Tag>
        </span>
        {wfInstance.isOverdue && <Tag color="red">Overdue</Tag>}
      </Space>
    </Card>
  );
}

function ApplicationWorkflowCard({ applicationId, feePaid }: { applicationId: string; feePaid: boolean }) {
  return (
    <WorkflowInstanceProvider>
      <ApplicationWorkflowCardInner applicationId={applicationId} feePaid={feePaid} />
    </WorkflowInstanceProvider>
  );
}

// ─── Main Content ──────────────────────────────────────────────
function ApplicationDetailContent() {
  const params = useParams();
  const router = useRouter();
  const portalBase = usePortalBase();
  const applicationId = params.id as string;

  const { application, isPending, isError } = useApplicationState();
  const { getAsync } = useApplicationActions();

  useEffect(() => {
    if (applicationId) getAsync(applicationId);
  }, [applicationId, getAsync]);

  const goBack = () => {
    // Prefer browser history (e.g. came from the workflow instance); fall back to
    // the portal's home — the admin portal has no admissions list page.
    if (typeof window !== 'undefined' && window.history.length > 1) router.back();
    else router.push(portalBase === '/admin' ? '/admin/workflow' : `${portalBase}/admissions`);
  };

  if (isPending && !application) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 80 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (isError || (!isPending && !application)) {
    return (
      <div style={{ padding: 24 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={goBack} style={{ marginBottom: 16 }}>
          Back
        </Button>
        <Empty description="Application not found" />
      </div>
    );
  }

  if (!application) return null;

  const app = application;
  const currentStep = statusStepIndex[app.status] ?? 0;
  const isTerminal = ['Rejected', 'Withdrawn'].includes(app.status);

  const tabItems = [
    {
      key: 'parents',
      label: <span><TeamOutlined /> Parents ({app.parentCount})</span>,
      children: <ParentsSection applicationId={applicationId} />,
    },
    {
      key: 'documents',
      label: <span><FileOutlined /> Documents ({app.documentCount})</span>,
      children: <DocumentsSection applicationId={applicationId} />,
    },
    {
      key: 'interview',
      label: <span><AudioOutlined /> Interview</span>,
      children: <InterviewSection applicationId={applicationId} />,
    },
    {
      key: 'assessment',
      label: <span><FormOutlined /> Assessment</span>,
      children: <AssessmentSection applicationId={applicationId} />,
    },
    {
      key: 'fee',
      label: <span><DollarOutlined /> Fee</span>,
      children: <FeeSection applicationId={applicationId} />,
    },
  ];

  return (
    <div style={{ padding: 24 }}>
      <Button
        icon={<ArrowLeftOutlined />}
        onClick={goBack}
        style={{ marginBottom: 16 }}
      >
        Back
      </Button>

      {/* Header */}
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Space align="center" size="middle">
              <Title level={3} style={{ margin: 0 }}>{app.fullName}</Title>
              <Tag color={getStatusColor(app.status)}>{app.statusDisplayName}</Tag>
              {app.isFeePaid && <Tag color="green">Fee Paid</Tag>}
            </Space>
            <div style={{ marginTop: 8 }}>
              <Space size="large">
                <Text type="secondary">App #: {app.applicationNumber}</Text>
                <Text type="secondary">Grade: {app.applyingForGradeName}</Text>
                <Text type="secondary">Year: {app.academicYearName}</Text>
                <Text type="secondary">Applied: {dayjs(app.applicationDate).format('DD MMM YYYY')}</Text>
              </Space>
            </div>
          </Col>
        </Row>
      </Card>

      {/* Status Timeline */}
      {!isTerminal && (
        <Card size="small" style={{ marginBottom: 16 }}>
          <Steps
            current={currentStep >= 0 ? currentStep : 0}
            size="small"
            items={statusSteps.map(s => ({ title: s }))}
          />
        </Card>
      )}

      {isTerminal && (
        <Alert
          type={app.status === 'Rejected' ? 'error' : 'warning'}
          showIcon
          message={`Application ${app.statusDisplayName}`}
          description={app.decisionReason || undefined}
          style={{ marginBottom: 16 }}
        />
      )}

      {app.status === 'Waitlisted' && (
        <Alert
          type="info"
          showIcon
          message={`Waitlisted — Position #${app.waitlistPosition ?? 'N/A'}`}
          style={{ marginBottom: 16 }}
        />
      )}

      {/* Approval Workflow (WF-23) — the application's decision flows through the
          workflow; the card above links to take action. (WF-24 removed the dead
          manual Decision card that posted to a non-existent MakeDecision endpoint.) */}
      <ApplicationWorkflowCard applicationId={applicationId} feePaid={app.isFeePaid} />

      {/* Applicant Details */}
      <Card title="Applicant Information" size="small" style={{ marginBottom: 16 }}>
        <Descriptions bordered size="small" column={{ xs: 1, sm: 2 }}>
          <Descriptions.Item label="Full Name">{app.fullName}</Descriptions.Item>
          <Descriptions.Item label="Date of Birth">{dayjs(app.dateOfBirth).format('DD MMM YYYY')} (Age: {app.age})</Descriptions.Item>
          <Descriptions.Item label="Gender">{app.genderDisplayName}</Descriptions.Item>
          <Descriptions.Item label="SA Citizen">{app.isSACitizen ? 'Yes' : 'No'}</Descriptions.Item>
          <Descriptions.Item label="ID Number">{app.idNumber || 'N/A'}</Descriptions.Item>
          <Descriptions.Item label="Passport">{app.passportNumber || 'N/A'}</Descriptions.Item>
          <Descriptions.Item label="Previous School">{app.previousSchool || 'N/A'}</Descriptions.Item>
          <Descriptions.Item label="Applicant Email">{app.creatorEmailAddress}</Descriptions.Item>
          {app.submittedDate && <Descriptions.Item label="Submitted">{dayjs(app.submittedDate).format('DD MMM YYYY')}</Descriptions.Item>}
          {app.decisionDate && <Descriptions.Item label="Decision Date">{dayjs(app.decisionDate).format('DD MMM YYYY')}</Descriptions.Item>}
          {app.reviewedByUserName && <Descriptions.Item label="Reviewed By">{app.reviewedByUserName}</Descriptions.Item>}
        </Descriptions>
      </Card>

      {/* Tabs */}
      <Card>
        <Tabs items={tabItems} defaultActiveKey="parents" />
      </Card>
    </div>
  );
}

// ─── Wrapped with providers ────────────────────────────────────
export default function ApplicationDetailPage() {
  return (
    <ApplicationProvider>
      <ApplicantParentProvider>
        <ApplicationDocumentProvider>
          <AdmissionInterviewProvider>
            <AdmissionAssessmentProvider>
              <ApplicationFeeProvider>
                <ApplicationDetailContent />
              </ApplicationFeeProvider>
            </AdmissionAssessmentProvider>
          </AdmissionInterviewProvider>
        </ApplicationDocumentProvider>
      </ApplicantParentProvider>
    </ApplicationProvider>
  );
}
