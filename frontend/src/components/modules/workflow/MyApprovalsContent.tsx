'use client';

import React, { useState, useCallback } from 'react';
import { PlayCircleOutlined, EyeOutlined } from '@ant-design/icons';
import { useRouter } from 'next/navigation';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction } from '@/components/shared/enterprise-table';
import {
  WorkflowInstanceProvider,
  useWorkflowInstanceState,
  useWorkflowInstanceActions,
} from '@/providers/workflow/workflow-instances';
import { AdvanceWorkflowModal } from '@/components/modals/workflow/AdvanceWorkflowModal';
import { useWorkflowBasePath } from './useWorkflowBasePath';
import type { IWorkflowInstanceList } from '@/providers/workflow/shared/interfaces';
import { WorkflowStatus, WorkflowEntityTypeLabels } from '@/providers/workflow/shared/interfaces';

/**
 * "My Approvals" — the in-progress instances whose current step is assigned to
 * the logged-in user (by user or role), served by the scoped GetMyPending
 * endpoint. Used across every approver portal (teacher, principal, admin): act
 * inline via the advance modal, or open the detail (history + entity content).
 */
function MyApprovalsList() {
  const { instances, totalCount, isPending, isError } = useWorkflowInstanceState();
  const { getMyPendingAsync } = useWorkflowInstanceActions();
  const router = useRouter();
  const base = useWorkflowBasePath();
  const [advanceModalOpen, setAdvanceModalOpen] = useState(false);
  const [selectedInstance, setSelectedInstance] = useState<IWorkflowInstanceList | null>(null);
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);

  const handleQueryChange = useCallback((query: TableQuery) => {
    setLastQuery(query);
    getMyPendingAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      // The search box stores its text under filters.keyword; forward it so the
      // server can match by subject (student name), workflow, or current step.
      keyword: query.filters?.keyword as string | undefined,
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
      // WF-20: at-a-glance "who/what" (e.g. the student's name) so identical-looking
      // rows can be told apart without opening each one.
      key: 'subjectLabel', title: 'Subject', dataIndex: 'subjectLabel',
      render: (val?: string) => val || '—',
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
  ];

  return (
    <>
      <EnterpriseTable<IWorkflowInstanceList>
        title="My Approvals"
        searchPlaceholder="Search by student, workflow, or step…"
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
        currentStepActionType={selectedInstance?.currentStepActionType}
        currentStepOrder={selectedInstance?.currentStepOrder}
      />
    </>
  );
}

export default function MyApprovalsContent() {
  return (
    <WorkflowInstanceProvider>
      <MyApprovalsList />
    </WorkflowInstanceProvider>
  );
}
