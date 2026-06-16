'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import {
  PlusOutlined,
  EyeOutlined,
  PlayCircleOutlined,
  StopOutlined,
  UndoOutlined,
  WarningOutlined,
  FilterOutlined,
} from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, BulkAction, ToolbarAction } from '@/components/shared/enterprise-table';
import {
  WorkflowInstanceProvider,
  useWorkflowInstanceState,
  useWorkflowInstanceActions,
} from '@/providers/workflow/workflow-instances';
import { useAuthState } from '@/providers/auth';
import { StartWorkflowModal } from '@/components/modals/workflow/StartWorkflowModal';
import { AdvanceWorkflowModal } from '@/components/modals/workflow/AdvanceWorkflowModal';
import type { IWorkflowInstanceList } from '@/providers/workflow/shared/interfaces';
import {
  WorkflowStatus,
  WorkflowStatusLabels,
  WorkflowEntityTypeLabels,
} from '@/providers/workflow/shared/interfaces';
import { useRouter } from 'next/navigation';
import { useWorkflowBasePath } from './useWorkflowBasePath';

function InstancesContent() {
  const { instances, totalCount, isPending, isError } = useWorkflowInstanceState();
  const { getAllAsync, cancelAsync, recallAsync, batchAdvanceAsync, getOverdueAsync, getPendingForRoleAsync } = useWorkflowInstanceActions();
  const { currentRole } = useAuthState();
  const router = useRouter();
  const base = useWorkflowBasePath();
  const [startModalOpen, setStartModalOpen] = useState(false);
  const [advanceModalOpen, setAdvanceModalOpen] = useState(false);
  const [selectedInstance, setSelectedInstance] = useState<IWorkflowInstanceList | null>(null);
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);
  const [activeFilter, setActiveFilter] = useState<string | null>(null);

  const handleQueryChange = useCallback((query: TableQuery) => {
    setLastQuery(query);
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      ...query.filters,
    });
  }, [getAllAsync]);

  const refreshData = useCallback(() => {
    if (lastQuery) handleQueryChange(lastQuery);
  }, [lastQuery, handleQueryChange]);

  const statusColorMap: Record<string, string> = {
    [WorkflowStatus.NotStarted]: 'default',
    [WorkflowStatus.InProgress]: 'processing',
    [WorkflowStatus.Completed]: 'green',
    [WorkflowStatus.Rejected]: 'red',
    [WorkflowStatus.Cancelled]: 'default',
    [WorkflowStatus.Recalled]: 'purple',
  };

  const columns: ColumnConfig<IWorkflowInstanceList>[] = [
    { key: 'workflowDefinitionName', title: 'Workflow', dataIndex: 'workflowDefinitionName', sortable: true },
    {
      key: 'entityType', title: 'Entity Type', dataIndex: 'entityType',
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(WorkflowEntityTypeLabels).map(([v, l]) => ({
        label: l, value: Number(v),
      })),
      render: (value: number) => WorkflowEntityTypeLabels[value] ?? value,
    },
    {
      key: 'status', title: 'Status', dataIndex: 'status',
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(WorkflowStatusLabels).map(([v, l]) => ({
        label: l, value: Number(v),
      })),
      renderType: 'status',
      renderConfig: {
        statusMap: Object.fromEntries(
          Object.entries(WorkflowStatusLabels).map(([v, l]) => [
            v,
            { label: l, color: statusColorMap[v] ?? 'default' },
          ])
        ),
      },
    },
    {
      key: 'currentStepName', title: 'Current Step', dataIndex: 'currentStepName',
      render: (val?: string) => val ?? '—',
    },
    {
      key: 'currentStepAssignedRole', title: 'Assigned To', dataIndex: 'currentStepAssignedRole',
      render: (val?: string) => val ?? '—',
    },
    {
      key: 'isOverdue', title: 'Overdue', dataIndex: 'isOverdue',
      width: 80,
      renderType: 'status',
      renderConfig: {
        statusMap: {
          true: { label: 'Overdue', color: 'red' },
          false: { label: 'OK', color: 'green' },
        },
      },
    },
    {
      key: 'startedDate', title: 'Started', dataIndex: 'startedDate',
      sortable: true,
      render: (val?: string) => val ? new Date(val).toLocaleDateString() : '—',
    },
  ];

  const toolbarActions: ToolbarAction[] = [
    {
      key: 'start',
      label: 'Start Workflow',
      icon: <PlusOutlined />,
      type: 'primary',
      requiredPermissions: ['Admin', 'Principal', 'VicePrincipal'],
      onClick: () => setStartModalOpen(true),
    },
    {
      key: 'overdue',
      label: activeFilter === 'overdue' ? 'Clear Filter' : 'Overdue',
      icon: <WarningOutlined />,
      type: activeFilter === 'overdue' ? 'default' : undefined,
      onClick: () => {
        if (activeFilter === 'overdue') {
          setActiveFilter(null);
          refreshData();
        } else {
          setActiveFilter('overdue');
          getOverdueAsync({ maxResultCount: 50, skipCount: 0 });
        }
      },
    },
    {
      key: 'myRole',
      label: activeFilter === 'myRole' ? 'Clear Filter' : `My Role (${currentRole})`,
      icon: <FilterOutlined />,
      type: activeFilter === 'myRole' ? 'default' : undefined,
      onClick: () => {
        if (activeFilter === 'myRole') {
          setActiveFilter(null);
          refreshData();
        } else if (currentRole) {
          setActiveFilter('myRole');
          getPendingForRoleAsync(currentRole, { maxResultCount: 50, skipCount: 0 });
        }
      },
    },
  ];

  const rowActions: RowAction<IWorkflowInstanceList>[] = [
    {
      key: 'view',
      label: 'View Details',
      icon: <EyeOutlined />,
      onClick: (record) => router.push(`${base}/instances/${record.id}`),
    },
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
    {
      key: 'cancel',
      label: 'Cancel',
      icon: <StopOutlined />,
      danger: true,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => record.status === WorkflowStatus.InProgress,
      confirm: { title: 'Cancel this workflow?', description: 'This action cannot be undone.' },
      onClick: async (record) => {
        await cancelAsync(record.id);
        message.success('Workflow cancelled');
        refreshData();
      },
    },
    {
      key: 'recall',
      label: 'Recall',
      icon: <UndoOutlined />,
      visible: (record) => record.status === WorkflowStatus.InProgress,
      confirm: { title: 'Recall this workflow?', description: 'Only the creator can recall.' },
      onClick: async (record) => {
        await recallAsync(record.id);
        message.success('Workflow recalled');
        refreshData();
      },
    },
  ];

  const bulkActions: BulkAction<IWorkflowInstanceList>[] = [
    {
      key: 'batchApprove',
      label: 'Batch Approve',
      confirm: { title: 'Approve all selected workflows?' },
      onClick: async (rows) => {
        const inProgressIds = rows
          .filter((r) => r.status === WorkflowStatus.InProgress)
          .map((r) => r.id);
        if (inProgressIds.length === 0) {
          message.warning('No in-progress workflows selected');
          return;
        }
        await batchAdvanceAsync({
          instanceIds: inProgressIds,
          action: 3, // Approve
        });
        message.success(`Batch approve submitted for ${inProgressIds.length} workflow(s)`);
        refreshData();
      },
    },
    {
      key: 'batchReject',
      label: 'Batch Reject',
      danger: true,
      confirm: { title: 'Reject all selected workflows?' },
      onClick: async (rows) => {
        const inProgressIds = rows
          .filter((r) => r.status === WorkflowStatus.InProgress)
          .map((r) => r.id);
        if (inProgressIds.length === 0) {
          message.warning('No in-progress workflows selected');
          return;
        }
        await batchAdvanceAsync({
          instanceIds: inProgressIds,
          action: 4, // Reject
        });
        message.success(`Batch reject submitted for ${inProgressIds.length} workflow(s)`);
        refreshData();
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<IWorkflowInstanceList>
        title="Workflow Instances"
        columns={columns}
        data={instances ?? []}
        totalCount={totalCount}
        loading={isPending}
        error={isError}
        onQueryChange={handleQueryChange}
        rowKey="id"
        toolbarActions={toolbarActions}
        rowActions={rowActions}
        bulkActions={bulkActions}
        selectionMode="multi"
        currentUserRole={currentRole}
        exportConfig={{
          enabled: true,
          formats: ['csv', 'xlsx'],
          requiredPermissions: ['Admin', 'Principal'],
        }}
      />
      <StartWorkflowModal
        open={startModalOpen}
        onClose={(refresh) => {
          setStartModalOpen(false);
          if (refresh) refreshData();
        }}
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

export default function WorkflowInstancesPageContent() {
  return (
    <WorkflowInstanceProvider>
      <InstancesContent />
    </WorkflowInstanceProvider>
  );
}
