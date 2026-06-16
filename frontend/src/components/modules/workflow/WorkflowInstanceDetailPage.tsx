'use client';

import React, { useState, useEffect } from 'react';
import { Card, Descriptions, Tag, Button, Timeline, Typography, Space, Spin } from 'antd';
import {
  ArrowLeftOutlined,
  PlayCircleOutlined,
  StopOutlined,
  UndoOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined,
} from '@ant-design/icons';
import { useParams, useRouter } from 'next/navigation';
import { useWorkflowBasePath } from './useWorkflowBasePath';
import {
  WorkflowInstanceProvider,
  useWorkflowInstanceState,
  useWorkflowInstanceActions,
} from '@/providers/workflow/workflow-instances';
import { AdvanceWorkflowModal } from '@/components/modals/workflow/AdvanceWorkflowModal';
import {
  WorkflowStatus,
  WorkflowStatusLabels,
  WorkflowEntityTypeLabels,
  WorkflowActionTypeLabels,
} from '@/providers/workflow/shared/interfaces';
import type { IWorkflowTransition } from '@/providers/workflow/shared/interfaces';
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
  const { getAsync, cancelAsync, recallAsync } = useWorkflowInstanceActions();
  const [advanceOpen, setAdvanceOpen] = useState(false);

  useEffect(() => {
    if (id) getAsync(id);
  }, [id]);

  const isInProgress = wfInstance?.status === WorkflowStatus.InProgress;

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
      <Button
        icon={<ArrowLeftOutlined />}
        onClick={() => router.push(`${base}/instances`)}
        style={{ marginBottom: 16 }}
      >
        Back to Instances
      </Button>

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
              <Button icon={<UndoOutlined />} onClick={handleRecall}>Recall</Button>
              <Button danger icon={<StopOutlined />} onClick={handleCancel}>Cancel</Button>
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
            <Text copyable style={{ fontSize: 12 }}>{wfInstance?.entityId ?? '—'}</Text>
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
