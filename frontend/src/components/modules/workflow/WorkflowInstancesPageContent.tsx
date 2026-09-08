'use client';

import React, { useState, useCallback } from 'react';
import { message, Modal, Input, Typography } from 'antd';
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
  const { currentRole, currentUser } = useAuthState();
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
      // WF-20: at-a-glance "who/what" each instance is about (e.g. student name).
      key: 'subjectLabel', title: 'Subject', dataIndex: 'subjectLabel',
      render: (val?: string) => val || '—',
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
      // WF-34: the server only lets the creator recall; don't advertise it to others.
      visible: (record) => record.status === WorkflowStatus.InProgress
        && record.creatorUserId != null && record.creatorUserId === currentUser?.id,
      confirm: { title: 'Recall this workflow?', description: 'Only the creator can recall.' },
      onClick: async (record) => {
        await recallAsync(record.id);
        message.success('Workflow recalled');
        refreshData();
      },
    },
  ];

  // WF-34: batch actions carry a comment (steps may require one) and report the
  // per-instance outcome instead of a blanket success. Steps with unmet exit
  // criteria or decision fields fail individually and are listed.
  const runBatch = (rows: IWorkflowInstanceList[], action: number, verb: string) => {
    const inProgressIds = rows.filter((r) => r.status === WorkflowStatus.InProgress).map((r) => r.id);
    if (inProgressIds.length === 0) {
      message.warning('No in-progress workflows selected');
      return;
    }
    let comment = '';
    Modal.confirm({
      title: `${verb} ${inProgressIds.length} workflow(s)?`,
      content: (
        <div style={{ marginTop: 12 }}>
          <Typography.Text type="secondary">Comment (applied to every selected workflow; required by some steps)</Typography.Text>
          <Input.TextArea rows={3} maxLength={2000} onChange={(e) => { comment = e.target.value; }} />
        </div>
      ),
      okText: verb,
      onOk: async () => {
        const result = await batchAdvanceAsync({ instanceIds: inProgressIds, action, comment: comment || undefined });
        if (!result) return;
        if (result.failedCount === 0) {
          message.success(`${verb}d ${result.successCount} workflow(s)`);
        } else {
          Modal.warning({
            title: `${result.successCount} succeeded, ${result.failedCount} failed`,
            content: (
              <ul style={{ paddingLeft: 18, marginTop: 8 }}>
                {result.failures.map((fl) => (
                  <li key={fl.instanceId}><Typography.Text code>{fl.instanceId.split('-')[0]}…</Typography.Text> {fl.error}</li>
                ))}
              </ul>
            ),
          });
        }
        refreshData();
      },
    });
  };

  const bulkActions: BulkAction<IWorkflowInstanceList>[] = [
    {
      key: 'batchApprove',
      label: 'Batch Approve',
      onClick: (rows) => runBatch(rows, 3, 'Approve'),
    },
    {
      key: 'batchReject',
      label: 'Batch Reject',
      danger: true,
      onClick: (rows) => runBatch(rows, 4, 'Reject'),
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
        currentStepActionType={selectedInstance?.currentStepActionType}
        currentStepOrder={selectedInstance?.currentStepOrder}
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
