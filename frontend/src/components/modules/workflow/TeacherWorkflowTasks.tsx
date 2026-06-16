'use client';

import React, { useState, useCallback } from 'react';
import { PlayCircleOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction } from '@/components/shared/enterprise-table';
import {
  WorkflowInstanceProvider,
  useWorkflowInstanceState,
  useWorkflowInstanceActions,
} from '@/providers/workflow/workflow-instances';
import { AdvanceWorkflowModal } from '@/components/modals/workflow/AdvanceWorkflowModal';
import type { IWorkflowInstanceList } from '@/providers/workflow/shared/interfaces';
import { WorkflowStatus, WorkflowEntityTypeLabels } from '@/providers/workflow/shared/interfaces';

/**
 * WF-03: a teacher's focused "My Approvals" list — only the in-progress
 * instances whose current step is assigned to them (by user or role), served by
 * the scoped GetMyPending endpoint. Acting on a step is done inline via the
 * advance modal; the heavier admin instance detail (with cancel/recall) is not
 * exposed here.
 */
function MyTasksContent() {
  const { instances, totalCount, isPending, isError } = useWorkflowInstanceState();
  const { getMyPendingAsync } = useWorkflowInstanceActions();
  const [advanceModalOpen, setAdvanceModalOpen] = useState(false);
  const [selectedInstance, setSelectedInstance] = useState<IWorkflowInstanceList | null>(null);
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);

  const handleQueryChange = useCallback((query: TableQuery) => {
    setLastQuery(query);
    getMyPendingAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
    });
  }, [getMyPendingAsync]);

  const refreshData = useCallback(() => {
    if (lastQuery) handleQueryChange(lastQuery);
  }, [lastQuery, handleQueryChange]);

  const columns: ColumnConfig<IWorkflowInstanceList>[] = [
    { key: 'workflowDefinitionName', title: 'Workflow', dataIndex: 'workflowDefinitionName', sortable: true },
    {
      key: 'entityType', title: 'Type', dataIndex: 'entityType',
      render: (value: number) => WorkflowEntityTypeLabels[value] ?? value,
    },
    {
      key: 'currentStepName', title: 'Awaiting', dataIndex: 'currentStepName',
      render: (val?: string) => val ?? '—',
    },
    {
      key: 'isOverdue', title: 'Overdue', dataIndex: 'isOverdue',
      width: 90,
      renderType: 'status',
      renderConfig: {
        statusMap: {
          true: { label: 'Overdue', color: 'red' },
          false: { label: 'On time', color: 'green' },
        },
      },
    },
    {
      key: 'startedDate', title: 'Started', dataIndex: 'startedDate',
      sortable: true,
      render: (val?: string) => (val ? new Date(val).toLocaleDateString() : '—'),
    },
  ];

  const rowActions: RowAction<IWorkflowInstanceList>[] = [
    {
      key: 'advance',
      label: 'Take Action',
      icon: <PlayCircleOutlined />,
      visible: (record) => record.status === WorkflowStatus.InProgress,
      onClick: (record) => {
        setSelectedInstance(record);
        setAdvanceModalOpen(true);
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<IWorkflowInstanceList>
        title="My Approvals"
        columns={columns}
        data={instances ?? []}
        totalCount={totalCount}
        loading={isPending}
        error={isError}
        onQueryChange={handleQueryChange}
        rowKey="id"
        rowActions={rowActions}
      />
      <AdvanceWorkflowModal
        open={advanceModalOpen}
        onClose={(refresh) => {
          setAdvanceModalOpen(false);
          setSelectedInstance(null);
          if (refresh) refreshData();
        }}
        instanceId={selectedInstance?.id ?? ''}
        currentStepName={selectedInstance?.currentStepName}
        isCommentRequired={selectedInstance?.currentStepIsCommentRequired}
      />
    </>
  );
}

export default function TeacherWorkflowTasks() {
  return (
    <WorkflowInstanceProvider>
      <MyTasksContent />
    </WorkflowInstanceProvider>
  );
}
