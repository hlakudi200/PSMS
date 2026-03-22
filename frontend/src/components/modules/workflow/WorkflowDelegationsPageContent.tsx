'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import { PlusOutlined, StopOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, ToolbarAction } from '@/components/shared/enterprise-table';
import {
  WorkflowDelegationProvider,
  useWorkflowDelegationState,
  useWorkflowDelegationActions,
} from '@/providers/workflow/workflow-delegations';
import { useAuthState } from '@/providers/auth';
import { WorkflowDelegationFormModal } from '@/components/modals/workflow/WorkflowDelegationFormModal';
import type { IWorkflowDelegation } from '@/providers/workflow/shared/interfaces';
import { WorkflowEntityTypeLabels } from '@/providers/workflow/shared/interfaces';

function DelegationsContent() {
  const { delegations, totalCount, isPending, isError } = useWorkflowDelegationState();
  const { getAllAsync, revokeAsync } = useWorkflowDelegationActions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);

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

  const handleModalClose = (refresh?: boolean) => {
    setModalOpen(false);
    if (refresh) refreshData();
  };

  const columns: ColumnConfig<IWorkflowDelegation>[] = [
    { key: 'delegatorUserName', title: 'Delegator', dataIndex: 'delegatorUserName', sortable: true },
    { key: 'delegateUserName', title: 'Delegate', dataIndex: 'delegateUserName', sortable: true },
    {
      key: 'startDate', title: 'Start Date', dataIndex: 'startDate',
      sortable: true,
      render: (val: string) => new Date(val).toLocaleDateString(),
    },
    {
      key: 'endDate', title: 'End Date', dataIndex: 'endDate',
      sortable: true,
      render: (val: string) => new Date(val).toLocaleDateString(),
    },
    { key: 'reason', title: 'Reason', dataIndex: 'reason', render: (val?: string) => val ?? '—' },
    {
      key: 'entityType', title: 'Scope', dataIndex: 'entityType',
      render: (val?: number) => val ? WorkflowEntityTypeLabels[val] : 'All Types',
    },
    {
      key: 'assignedRole', title: 'Role Scope', dataIndex: 'assignedRole',
      render: (val?: string) => val ?? 'All Roles',
    },
    {
      key: 'isActive', title: 'Status', dataIndex: 'isActive',
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Active', value: 'true' },
        { label: 'Revoked', value: 'false' },
      ],
      renderType: 'status',
      renderConfig: {
        statusMap: {
          true: { label: 'Active', color: 'green' },
          false: { label: 'Revoked', color: 'default' },
        },
      },
    },
    {
      key: 'isEffective', title: 'Effective', dataIndex: 'isEffective',
      renderType: 'status',
      renderConfig: {
        statusMap: {
          true: { label: 'Effective', color: 'blue' },
          false: { label: 'Inactive', color: 'default' },
        },
      },
    },
  ];

  const toolbarActions: ToolbarAction[] = [
    {
      key: 'new',
      label: 'New Delegation',
      requiredPermissions: ['Admin', 'Principal', 'VicePrincipal'],
      icon: <PlusOutlined />,
      type: 'primary',
      onClick: () => setModalOpen(true),
    },
  ];

  const rowActions: RowAction<IWorkflowDelegation>[] = [
    {
      key: 'revoke',
      label: 'Revoke',
      icon: <StopOutlined />,
      danger: true,
      requiredPermissions: ['Admin', 'Principal', 'VicePrincipal'],
      visible: (record) => record.isActive,
      confirm: { title: 'Revoke this delegation?', description: 'The delegate will lose workflow access.' },
      onClick: async (record) => {
        await revokeAsync(record.id);
        message.success('Delegation revoked');
        refreshData();
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<IWorkflowDelegation>
        title="Workflow Delegations"
        columns={columns}
        data={delegations ?? []}
        totalCount={totalCount}
        loading={isPending}
        error={isError}
        onQueryChange={handleQueryChange}
        rowKey="id"
        toolbarActions={toolbarActions}
        rowActions={rowActions}
        currentUserRole={currentRole}
      />
      <WorkflowDelegationFormModal
        open={modalOpen}
        onClose={handleModalClose}
      />
    </>
  );
}

export default function WorkflowDelegationsPageContent() {
  return (
    <WorkflowDelegationProvider>
      <DelegationsContent />
    </WorkflowDelegationProvider>
  );
}
