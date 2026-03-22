'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { Card, Descriptions, Tag, Button, Table, Space, message, Typography, Popconfirm } from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  ArrowUpOutlined,
  ArrowDownOutlined,
  ArrowLeftOutlined,
} from '@ant-design/icons';
import { useParams, useRouter } from 'next/navigation';
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
import type { IWorkflowStep } from '@/providers/workflow/shared/interfaces';
import {
  WorkflowEntityTypeLabels,
  WorkflowActionTypeLabels,
} from '@/providers/workflow/shared/interfaces';

const { Title, Text } = Typography;

function DetailContent() {
  const params = useParams();
  const router = useRouter();
  const id = params?.id as string;
  const { definition, isPending: defPending } = useWorkflowDefinitionState();
  const { getAsync: getDefinition } = useWorkflowDefinitionActions();
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
      title: 'Actions',
      key: 'actions',
      width: 180,
      render: (_: unknown, record: IWorkflowStep, index: number) => (
        <Space size="small">
          <Button
            size="small"
            icon={<ArrowUpOutlined />}
            disabled={index === 0}
            onClick={() => handleMoveStep(record.id, 'up')}
          />
          <Button
            size="small"
            icon={<ArrowDownOutlined />}
            disabled={index === sortedSteps.length - 1}
            onClick={() => handleMoveStep(record.id, 'down')}
          />
          <Button
            size="small"
            icon={<EditOutlined />}
            onClick={() => { setEditStep(record); setModalOpen(true); }}
          />
          <Popconfirm
            title="Delete this step?"
            description="Steps in use by active instances cannot be deleted."
            onConfirm={() => handleDeleteStep(record.id)}
          >
            <Button size="small" icon={<DeleteOutlined />} danger />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div style={{ padding: 24 }}>
      <Button
        icon={<ArrowLeftOutlined />}
        onClick={() => router.push('/admin/workflow/definitions')}
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

      <Card
        title={`Steps (${sortedSteps.length})`}
        extra={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => { setEditStep(null); setModalOpen(true); }}
          >
            Add Step
          </Button>
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
        nextStepOrder={nextStepOrder}
      />
    </div>
  );
}

export default function WorkflowDefinitionDetailPage() {
  return (
    <WorkflowDefinitionProvider>
      <WorkflowStepProvider>
        <DetailContent />
      </WorkflowStepProvider>
    </WorkflowDefinitionProvider>
  );
}
