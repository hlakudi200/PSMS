'use client';

import React, { useState, useEffect } from 'react';
import { Card, Descriptions, Tag, Button, Timeline, Typography, Space, Spin, Table, Alert } from 'antd';
import {
  ArrowLeftOutlined,
  PlayCircleOutlined,
  StopOutlined,
  UndoOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined,
  FileSearchOutlined,
} from '@ant-design/icons';
import { useParams, useRouter } from 'next/navigation';
import { useWorkflowBasePath } from './useWorkflowBasePath';
import { useAuthState } from '@/providers/auth';
import {
  WorkflowInstanceProvider,
  useWorkflowInstanceState,
  useWorkflowInstanceActions,
} from '@/providers/workflow/workflow-instances';
import { AdvanceWorkflowModal } from '@/components/modals/workflow/AdvanceWorkflowModal';
import {
  WorkflowStatus,
  WorkflowStatusLabels,
  WorkflowEntityType,
  WorkflowEntityTypeLabels,
  WorkflowActionTypeLabels,
} from '@/providers/workflow/shared/interfaces';
import type { IWorkflowTransition, IWorkflowEntitySummary } from '@/providers/workflow/shared/interfaces';
import { message } from 'antd';

const { Title, Text } = Typography;

const statusColors: Record<number, string> = {
  [WorkflowStatus.NotStarted]: 'default',
  [WorkflowStatus.InProgress]: 'processing',
  [WorkflowStatus.Completed]: 'success',
  [WorkflowStatus.Rejected]: 'error',
  [WorkflowStatus.Cancelled]: 'default',
  [WorkflowStatus.Recalled]: 'purple',
};

const actionColors: Record<number, string> = {
  1: 'blue',
  2: 'cyan',
  3: 'green',
  4: 'red',
  5: 'orange',
  6: 'default',
  7: 'purple',
};

function DetailContent() {
  const params = useParams();
  const router = useRouter();
  const base = useWorkflowBasePath();
  const id = params?.id as string;
  const { instance: wfInstance, isPending } = useWorkflowInstanceState();
  const { getAsync, cancelAsync, recallAsync, getEntitySummaryAsync } = useWorkflowInstanceActions();
  const { currentRole, currentUser } = useAuthState();
  const [advanceOpen, setAdvanceOpen] = useState(false);
  const [entitySummary, setEntitySummary] = useState<IWorkflowEntitySummary | null>(null);
  const [summaryLoaded, setSummaryLoaded] = useState(false);

  // Cancel/Recall require Workflow.Instances.Cancel/Recall, held only by
  // Admin/Principal/VicePrincipal (WF-01). This hides them from approvers
  // (Teacher/HOD) as a UX nicety — the server is the real boundary
  // (CancelAsync/RecallAsync are [AbpAuthorize]'d on those permissions). Gate on
  // the user's full role set (not just the primary role) so a multi-role
  // manager isn't wrongly blocked.
  const managementRoles = ['admin', 'principal', 'viceprincipal'];
  const myRoles = (currentUser?.roleNames?.length ? currentUser.roleNames : [currentRole])
    .filter(Boolean)
    .map((r) => (r as string).toLowerCase());
  const canManageInstance = myRoles.some((r) => managementRoles.includes(r));

  useEffect(() => {
    if (!id) return;
    let active = true;
    // Clear any previous instance's summary immediately so navigating between
    // approvals never briefly shows the wrong record's content; ignore a stale
    // (out-of-order) response.
    setEntitySummary(null);
    setSummaryLoaded(false);
    getAsync(id);
    getEntitySummaryAsync(id).then((s) => {
      if (active) {
        setEntitySummary(s ?? null);
        setSummaryLoaded(true);
      }
    });
    return () => {
      active = false;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  const isInProgress = wfInstance?.status === WorkflowStatus.InProgress;

  // WF-23/25/26: deep-link to the full underlying record when it has a detail
  // page, so the action-taker can open the actual application/report in one click.
  const portalRoot = base.replace(/\/workflow$/, '');
  // Which portals have a detail page for each entity type. Reports are viewable
  // by the teacher (HOD Review step), the principal, and the admin; admissions by
  // the principal and the admin. The target is built under the CURRENT portal so
  // the link respects what the viewer can actually open.
  const fullRecordRoutes: Record<number, { segment: string; label: string; portals: string[] }> = {
    [WorkflowEntityType.Application]: { segment: 'admissions', label: 'View full application', portals: ['/principal', '/admin'] },
    [WorkflowEntityType.Report]: { segment: 'reports', label: 'View full report', portals: ['/principal', '/teacher', '/admin'] },
  };
  const fullRecord = wfInstance ? fullRecordRoutes[wfInstance.entityType] : undefined;
  const fullRecordHref = fullRecord && wfInstance && fullRecord.portals.includes(portalRoot)
    ? `${portalRoot}/${fullRecord.segment}/${wfInstance.entityId}`
    : undefined;

  const handleCancel = async () => {
    await cancelAsync(id);
    message.success('Workflow cancelled');
    getAsync(id);
  };

  const handleRecall = async () => {
    await recallAsync(id);
    message.success('Workflow recalled');
    getAsync(id);
  };

  if (isPending && !wfInstance) {
    return <Spin size="large" style={{ display: 'block', margin: '100px auto' }} />;
  }

  return (
    <div style={{ padding: 24 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Button
          icon={<ArrowLeftOutlined />}
          onClick={() => {
            // Return to wherever the user came from; fall back to the portal's
            // workflow home when opened via a direct link (no SPA history).
            if (typeof window !== 'undefined' && window.history.length > 1) router.back();
            else router.push(base);
          }}
        >
          Back
        </Button>
        {fullRecordHref && (
          <Button
            type="primary"
            ghost
            icon={<FileSearchOutlined />}
            onClick={() => router.push(fullRecordHref)}
          >
            {fullRecord!.label}
          </Button>
        )}
      </div>

      {entitySummary && (
        <Card title="What you're approving" style={{ marginBottom: 24 }}>
          <Title level={5} style={{ marginTop: 0, marginBottom: 4 }}>{entitySummary.title}</Title>
          {entitySummary.subtitle && (
            <Text type="secondary">{entitySummary.subtitle}</Text>
          )}

          {(entitySummary.sections ?? []).map((section) => (
            <Descriptions
              key={section.heading}
              title={section.heading}
              column={{ xs: 1, sm: 2 }}
              size="small"
              style={{ marginTop: 16 }}
            >
              {section.fields.map((f) => (
                <Descriptions.Item key={f.label} label={f.label}>{f.value}</Descriptions.Item>
              ))}
            </Descriptions>
          ))}

          {(entitySummary.tables ?? []).map((tbl) => (
            <div key={tbl.heading} style={{ marginTop: 16 }}>
              <Text strong>{tbl.heading}</Text>
              <Table
                size="small"
                style={{ marginTop: 8 }}
                pagination={false}
                rowKey="__rowKey"
                columns={tbl.columns.map((c, ci) => ({ title: c, dataIndex: String(ci), key: String(ci) }))}
                dataSource={tbl.rows.map((row, ri) => {
                  const rec: Record<string, string> = { __rowKey: String(ri) };
                  row.forEach((cell, ci) => { rec[String(ci)] = cell; });
                  return rec;
                })}
              />
            </div>
          ))}
        </Card>
      )}

      {summaryLoaded && !entitySummary && (
        <Alert
          type="info"
          showIcon
          style={{ marginBottom: 24 }}
          message="No linked record to display"
          description="The record this approval refers to could not be found — it may have been removed, or this item isn't linked to a viewable record."
        />
      )}

      <Card style={{ marginBottom: 24 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 16 }}>
          <Title level={4} style={{ margin: 0 }}>{wfInstance?.workflowDefinitionName ?? 'Loading...'}</Title>
          {isInProgress && (
            <Space>
              <Button
                type="primary"
                icon={<PlayCircleOutlined />}
                onClick={() => setAdvanceOpen(true)}
              >
                Take Action
              </Button>
              {canManageInstance && (
                <>
                  <Button icon={<UndoOutlined />} onClick={handleRecall}>Recall</Button>
                  <Button danger icon={<StopOutlined />} onClick={handleCancel}>Cancel</Button>
                </>
              )}
            </Space>
          )}
        </div>

        <Descriptions column={{ xs: 1, sm: 2, md: 3 }} size="small">
          <Descriptions.Item label="Status">
            <Tag color={statusColors[wfInstance?.status ?? 0]}>
              {WorkflowStatusLabels[wfInstance?.status ?? 0] ?? '—'}
            </Tag>
            {wfInstance?.isOverdue && <Tag color="red">OVERDUE</Tag>}
          </Descriptions.Item>
          <Descriptions.Item label="Entity Type">
            {wfInstance?.entityType ? WorkflowEntityTypeLabels[wfInstance.entityType] : '—'}
          </Descriptions.Item>
          <Descriptions.Item label="Entity ID">
            {wfInstance?.entityId ? (
              // Show only the first GUID segment to avoid noise; copy the full id.
              <Text copyable={{ text: wfInstance.entityId }} style={{ fontSize: 12 }}>
                {wfInstance.entityId.split('-')[0]}…
              </Text>
            ) : '—'}
          </Descriptions.Item>
          <Descriptions.Item label="Current Step">
            {wfInstance?.currentStepName ?? '—'}
            {wfInstance?.currentStepAssignedRole && (
              <Tag style={{ marginLeft: 8 }}>{wfInstance.currentStepAssignedRole}</Tag>
            )}
          </Descriptions.Item>
          <Descriptions.Item label="Step Order">{wfInstance?.currentStepOrder ?? '—'}</Descriptions.Item>
          <Descriptions.Item label="Version">{wfInstance?.workflowDefinitionVersion ?? '—'}</Descriptions.Item>
          <Descriptions.Item label="Started">
            {wfInstance?.startedDate ? new Date(wfInstance.startedDate).toLocaleString() : '—'}
          </Descriptions.Item>
          <Descriptions.Item label="Due Date">
            {wfInstance?.currentStepDueDate ? new Date(wfInstance.currentStepDueDate).toLocaleString() : '—'}
          </Descriptions.Item>
          <Descriptions.Item label="Completed">
            {wfInstance?.completedDate ? new Date(wfInstance.completedDate).toLocaleString() : '—'}
          </Descriptions.Item>
          {wfInstance?.completionComment && (
            <Descriptions.Item label="Completion Comment" span={3}>
              {wfInstance.completionComment}
            </Descriptions.Item>
          )}
        </Descriptions>
      </Card>

      <Card title="Transition History">
        <Timeline
          items={(wfInstance?.transitions ?? []).map((t: IWorkflowTransition) => ({
            color: actionColors[t.action] ?? 'gray',
            dot: t.action === 3 ? <CheckCircleOutlined /> :
                 t.action === 4 ? <CloseCircleOutlined /> :
                 <ClockCircleOutlined />,
            children: (
              <div>
                <Tag color={actionColors[t.action]}>
                  {WorkflowActionTypeLabels[t.action] ?? t.action}
                </Tag>
                <Text strong>{t.fromStepName ?? '—'}</Text>
                {t.toStepName && (
                  <>
                    {' → '}
                    <Text strong>{t.toStepName}</Text>
                  </>
                )}
                <br />
                <Text type="secondary" style={{ fontSize: 12 }}>
                  by {t.actorUserName ?? 'System'} · {new Date(t.transitionDate).toLocaleString()}
                </Text>
                {t.comment && (
                  <div style={{ marginTop: 4, padding: '4px 8px', background: '#f5f5f5', borderRadius: 4 }}>
                    <Text italic style={{ fontSize: 12 }}>{t.comment}</Text>
                  </div>
                )}
              </div>
            ),
          }))}
        />
      </Card>

      <AdvanceWorkflowModal
        open={advanceOpen}
        onClose={(refresh) => {
          setAdvanceOpen(false);
          if (refresh) getAsync(id);
        }}
        instanceId={id}
        currentStepName={wfInstance?.currentStepName}
        isCommentRequired={wfInstance?.currentStepIsCommentRequired}
        currentStepActionType={wfInstance?.currentStepActionType}
        currentStepOrder={wfInstance?.currentStepOrder}
      />
    </div>
  );
}

export default function WorkflowInstanceDetailPage() {
  return (
    <WorkflowInstanceProvider>
      <DetailContent />
    </WorkflowInstanceProvider>
  );
}
