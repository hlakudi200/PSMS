'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { Card, Descriptions, Tag, Button, message, Typography } from 'antd';
import { PlusOutlined, ArrowLeftOutlined } from '@ant-design/icons';
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
import WorkflowStepFlow from './WorkflowStepFlow';
import { WorkflowExtensionProvider } from '@/providers/workflow/workflow-extensions';
import type { IWorkflowStep } from '@/providers/workflow/shared/interfaces';
import { WorkflowEntityTypeLabels } from '@/providers/workflow/shared/interfaces';

const { Title, Text } = Typography;

function DetailContent() {
  const params = useParams();
  const router = useRouter();
  const base = useWorkflowBasePath();
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

  /**
   * The order the person just dragged the steps into.
   *
   * This used to report "Steps reordered" whatever happened: the provider
   * swallowed the failure and the message fired unconditionally, so a reorder
   * the server had refused looked exactly like one that worked — which is why
   * the arrows appeared to do nothing.
   */
  const handleReorder = async (orderedIds: string[]) => {
    if (!id) return;
    try {
      await reorderAsync({ workflowDefinitionId: id, stepIds: orderedIds });
      message.success('Order saved');
    } catch (e) {
      const abp = (e as { response?: { data?: { error?: { message?: string; details?: string } } } })
        ?.response?.data?.error;
      message.error(abp?.message || abp?.details || 'Could not save the new order.');
    } finally {
      refreshSteps();
    }
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
        <WorkflowStepFlow
          steps={sortedSteps}
          loading={stepsPending}
          onEdit={(step) => { setEditStep(step); setModalOpen(true); }}
          onDelete={handleDeleteStep}
          onReorder={handleReorder}
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
