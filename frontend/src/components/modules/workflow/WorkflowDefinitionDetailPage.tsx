'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { Card, Descriptions, Tag, Button, Table, Space, message, Typography, Popconfirm, Alert } from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  ArrowUpOutlined,
  ArrowDownOutlined,
  ArrowLeftOutlined,
  CopyOutlined,
} from '@ant-design/icons';
import { useParams, useRouter } from 'next/navigation';
import { useWorkflowBasePath } from './useWorkflowBasePath';
import {
  WorkflowDefinitionProvider,
  useWorkflowDefinitionState,
  useWorkflowDefinitionActions,
} from '@/providers/workflow/workflow-definitions';
import {
  WorkflowStepProvider,
  useWorkflowStepState,
  useWorkflowStepActions,
} from '@/providers/workflow/workflow-steps';
import { WorkflowStepFormModal } from '@/components/modals/workflow/WorkflowStepFormModal';
import { WorkflowExtensionProvider } from '@/providers/workflow/workflow-extensions';
import type { IWorkflowStep } from '@/providers/workflow/shared/interfaces';
import {
  WorkflowEntityTypeLabels,
  WorkflowActionTypeLabels,
} from '@/providers/workflow/shared/interfaces';

const { Title, Text } = Typography;

function DetailContent() {
  const params = useParams();
  const router = useRouter();
  const base = useWorkflowBasePath();
  const id = params?.id as string;
  const { definition, isPending: defPending } = useWorkflowDefinitionState();
  const { getAsync: getDefinition, cloneAsync } = useWorkflowDefinitionActions();
  const { steps, isPending: stepsPending } = useWorkflowStepState();
  const { getByDefinitionAsync, deleteAsync: deleteStep, reorderAsync } = useWorkflowStepActions();
  const [modalOpen, setModalOpen] = useState(false);
  const [editStep, setEditStep] = useState<IWorkflowStep | null>(null);

  useEffect(() => {
    if (id) {
      getDefinition(id);
      getByDefinitionAsync(id);
    }
  }, [id]);

  const refreshSteps = useCallback(() => {
    if (id) {
      getByDefinitionAsync(id);
      getDefinition(id);
    }
  }, [id]);

  const handleModalClose = (refresh?: boolean) => {
    setModalOpen(false);
    setEditStep(null);
    if (refresh) refreshSteps();
  };

  const handleMoveStep = async (stepId: string, direction: 'up' | 'down') => {
    if (!steps || !id) return;
    const sortedSteps = [...steps].sort((a, b) => a.stepOrder - b.stepOrder);
    const idx = sortedSteps.findIndex(s => s.id === stepId);
    if (direction === 'up' && idx > 0) {
      [sortedSteps[idx], sortedSteps[idx - 1]] = [sortedSteps[idx - 1], sortedSteps[idx]];
    } else if (direction === 'down' && idx < sortedSteps.length - 1) {
      [sortedSteps[idx], sortedSteps[idx + 1]] = [sortedSteps[idx + 1], sortedSteps[idx]];
    }
    await reorderAsync({
      workflowDefinitionId: id,
      stepIds: sortedSteps.map(s => s.id),
    });
    message.success('Steps reordered');
    refreshSteps();
  };

  const handleDeleteStep = async (stepId: string) => {
    await deleteStep(stepId);
    message.success('Step deleted');
    refreshSteps();
  };

  // WF-33: a running instance follows the live step rows, so the server refuses
  // step edits while any instance is active; the UI mirrors that and offers a clone.
  const locked = !!definition?.hasActiveInstances;

  const handleClone = async () => {
    if (!id) return;
    const clone = await cloneAsync(id);
    if (clone) {
      message.success(`Cloned as "${clone.name}" (inactive). Edit its steps, then activate it.`);
      router.push(`${base}/definitions/${clone.id}`);
    }
  };

  const sortedSteps = [...(steps ?? definition?.steps ?? [])].sort(
    (a, b) => a.stepOrder - b.stepOrder
  );
  const nextStepOrder = sortedSteps.length > 0
    ? Math.max(...sortedSteps.map(s => s.stepOrder)) + 1
    : 1;

  const columns = [
    {
      title: '#',
      dataIndex: 'stepOrder',
      key: 'stepOrder',
      width: 50,
    },
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
    },
    {
      title: 'Role',
      dataIndex: 'assignedRole',
      key: 'assignedRole',
      render: (val: string) => <Tag>{val}</Tag>,
    },
    {
      title: 'Action',
      dataIndex: 'actionType',
      key: 'actionType',
      render: (val: number) => WorkflowActionTypeLabels[val] ?? val,
    },
    {
      title: 'Terminal',
      dataIndex: 'isTerminal',
      key: 'isTerminal',
      width: 80,
      render: (val: boolean) => val ? <Tag color="volcano">Yes</Tag> : <Tag>No</Tag>,
    },
    {
      title: 'Comment Req.',
      dataIndex: 'isCommentRequired',
      key: 'isCommentRequired',
      width: 100,
      render: (val: boolean) => val ? 'Yes' : 'No',
    },
    {
      title: 'SLA (hrs)',
      dataIndex: 'slaHours',
      key: 'slaHours',
      width: 80,
      render: (val?: number) => val ?? '—',
    },
    {
      title: 'Exit criteria',
      key: 'guardKey',
      render: (_: unknown, record: IWorkflowStep) => (
        <Space size={4} wrap>
          {record.guardKey ? <Tag color="geekblue">{record.guardKey}</Tag> : <Text type="secondary">—</Text>}
          {record.isOptional && <Tag>optional</Tag>}
        </Space>
      ),
    },
    {
      title: 'Effects / decision',
      key: 'effects',
      render: (_: unknown, record: IWorkflowStep) => {
        const parts = [
          record.entryEffectKey ? `on entry: ${record.entryEffectKey}` : null,
          record.exitEffectKey ? `on exit: ${record.exitEffectKey}` : null,
          record.decisionSchemaKey ? `decision: ${record.decisionSchemaKey}` : null,
        ].filter(Boolean);
        return parts.length ? (
          <Space direction="vertical" size={0}>
            {parts.map((p) => <Text key={p as string} style={{ fontSize: 12 }}>{p}</Text>)}
          </Space>
        ) : <Text type="secondary">—</Text>;
      },
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 180,
      render: (_: unknown, record: IWorkflowStep, index: number) => (
        <Space size="small">
          <Button
            size="small"
            icon={<ArrowUpOutlined />}
            disabled={locked || index === 0}
            onClick={() => handleMoveStep(record.id, 'up')}
          />
          <Button
            size="small"
            icon={<ArrowDownOutlined />}
            disabled={locked || index === sortedSteps.length - 1}
            onClick={() => handleMoveStep(record.id, 'down')}
          />
          <Button
            size="small"
            icon={<EditOutlined />}
            disabled={locked}
            onClick={() => { setEditStep(record); setModalOpen(true); }}
          />
          <Popconfirm
            title="Delete this step?"
            description="Steps in use by active instances cannot be deleted."
            onConfirm={() => handleDeleteStep(record.id)}
          >
            <Button size="small" icon={<DeleteOutlined />} danger disabled={locked} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div style={{ padding: 24 }}>
      <Button
        icon={<ArrowLeftOutlined />}
        onClick={() => router.push(`${base}/definitions`)}
        style={{ marginBottom: 16 }}
      >
        Back to Definitions
      </Button>

      <Card loading={defPending} style={{ marginBottom: 24 }}>
        <Title level={4}>{definition?.name ?? 'Loading...'}</Title>
        <Descriptions column={{ xs: 1, sm: 2, md: 3 }} size="small">
          <Descriptions.Item label="Entity Type">
            {definition?.entityType ? WorkflowEntityTypeLabels[definition.entityType] : '—'}
          </Descriptions.Item>
          <Descriptions.Item label="Status">
            <Tag color={definition?.isActive ? 'green' : 'default'}>
              {definition?.isActive ? 'Active' : 'Inactive'}
            </Tag>
          </Descriptions.Item>
          <Descriptions.Item label="Version">{definition?.version ?? '—'}</Descriptions.Item>
          <Descriptions.Item label="Description" span={3}>
            {definition?.description || <Text type="secondary">No description</Text>}
          </Descriptions.Item>
        </Descriptions>
      </Card>

      {locked && (
        <Alert
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
          message="Steps are locked while workflows are running on this definition."
          description="Editing them would rewire approvals mid-flight. Clone it as a new version, change the clone, then activate it — running instances finish on this version."
          action={<Button size="small" icon={<CopyOutlined />} onClick={handleClone}>Clone as new version</Button>}
        />
      )}
      <Card
        title={`Steps (${sortedSteps.length})`}
        extra={
          <Space>
            <Button icon={<CopyOutlined />} onClick={handleClone}>Clone</Button>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              disabled={locked}
              onClick={() => { setEditStep(null); setModalOpen(true); }}
            >
              Add Step
            </Button>
          </Space>
        }
      >
        <Table<IWorkflowStep>
          dataSource={sortedSteps}
          columns={columns}
          rowKey="id"
          loading={stepsPending}
          pagination={false}
          size="small"
        />
      </Card>

      <WorkflowStepFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editStep}
        definitionId={id}
        entityType={definition?.entityType}
        nextStepOrder={nextStepOrder}
      />
    </div>
  );
}

export default function WorkflowDefinitionDetailPage() {
  return (
    <WorkflowDefinitionProvider>
      <WorkflowStepProvider>
        <WorkflowExtensionProvider>
          <DetailContent />
        </WorkflowExtensionProvider>
      </WorkflowStepProvider>
    </WorkflowDefinitionProvider>
  );
}
