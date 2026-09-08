'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, EyeOutlined, CheckOutlined, StopOutlined, CopyOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { WorkflowDefinitionProvider, useWorkflowDefinitionState, useWorkflowDefinitionActions } from '@/providers/workflow/workflow-definitions';
import { useAuthState } from '@/providers/auth';
import { WorkflowDefinitionFormModal } from '@/components/modals/workflow/WorkflowDefinitionFormModal';
import type { IWorkflowDefinitionList } from '@/providers/workflow/shared/interfaces';
import { WorkflowEntityTypeLabels } from '@/providers/workflow/shared/interfaces';
import { useRouter } from 'next/navigation';
import { useWorkflowBasePath } from './useWorkflowBasePath';

function DefinitionsContent() {
  const { definitions, totalCount, isPending, isError } = useWorkflowDefinitionState();
  const { getAllAsync, deleteAsync, activateAsync, deactivateAsync, cloneAsync } = useWorkflowDefinitionActions();
  const { currentRole } = useAuthState();
  const router = useRouter();
  const base = useWorkflowBasePath();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<IWorkflowDefinitionList | null>(null);
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
    setEditRecord(null);
    if (refresh) refreshData();
  };

  const columns: ColumnConfig<IWorkflowDefinitionList>[] = [
    { key: 'name', title: 'Name', dataIndex: 'name', sortable: true },
    {
      key: 'entityType', title: 'Entity Type', dataIndex: 'entityType',
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(WorkflowEntityTypeLabels).map(([v, l]) => ({
        label: l, value: Number(v),
      })),
      render: (value: number) => WorkflowEntityTypeLabels[value] ?? value,
    },
    { key: 'stepCount', title: 'Steps', dataIndex: 'stepCount', sortable: true, width: 80 },
    { key: 'version', title: 'Version', dataIndex: 'version', width: 80 },
    {
      key: 'isActive', title: 'Status', dataIndex: 'isActive',
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Active', value: 'true' },
        { label: 'Inactive', value: 'false' },
      ],
      renderType: 'status',
      renderConfig: {
        statusMap: {
          true: { label: 'Active', color: 'green' },
          false: { label: 'Inactive', color: 'default' },
        },
      },
    },
  ];

  const toolbarActions: ToolbarAction[] = [
    {
      key: 'new',
      label: 'New Definition',
      icon: <PlusOutlined />,
      type: 'primary',
      onClick: () => { setEditRecord(null); setModalOpen(true); },
      requiredPermissions: ['Admin', 'Principal'],
    },
  ];

  const rowActions: RowAction<IWorkflowDefinitionList>[] = [
    {
      key: 'view',
      label: 'View Details',
      icon: <EyeOutlined />,
      onClick: (record) => router.push(`${base}/definitions/${record.id}`),
    },
    {
      key: 'edit',
      label: 'Edit',
      icon: <EditOutlined />,
      requiredPermissions: ['Admin', 'Principal'],
      onClick: (record) => { setEditRecord(record); setModalOpen(true); },
    },
    {
      key: 'clone',
      label: 'Clone as new version',
      icon: <CopyOutlined />,
      requiredPermissions: ['Admin', 'Principal'],
      onClick: async (record) => {
        const clone = await cloneAsync(record.id);
        if (clone) {
          message.success(`Cloned as "${clone.name}" (inactive). Edit its steps, then activate it.`);
          router.push(`${base}/definitions/${clone.id}`);
        }
      },
    },
    {
      key: 'activate',
      label: 'Activate',
      icon: <CheckOutlined />,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => !record.isActive,
      onClick: async (record) => {
        await activateAsync(record.id);
        message.success('Definition activated');
        refreshData();
      },
    },
    {
      key: 'deactivate',
      label: 'Deactivate',
      icon: <StopOutlined />,
      danger: true,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => record.isActive,
      onClick: async (record) => {
        await deactivateAsync(record.id);
        message.success('Definition deactivated');
        refreshData();
      },
    },
    {
      key: 'delete',
      label: 'Delete',
      icon: <DeleteOutlined />,
      danger: true,
      // Principal/VP hold Workflow.Definitions.Delete (WF-01 full config), so the
      // UI mirrors that rather than restricting delete to Admin alone.
      requiredPermissions: ['Admin', 'Principal'],
      confirm: { title: 'Delete this definition?', description: 'This cannot be undone. Definitions with active instances cannot be deleted.' },
      onClick: async (record) => {
        await deleteAsync(record.id);
        message.success('Definition deleted');
        refreshData();
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<IWorkflowDefinitionList>
        title="Workflow Definitions"
        columns={columns}
        data={definitions ?? []}
        totalCount={totalCount}
        loading={isPending}
        error={isError}
        onQueryChange={handleQueryChange}
        rowKey="id"
        toolbarActions={toolbarActions}
        rowActions={rowActions}
        currentUserRole={currentRole}
        exportConfig={{
          enabled: true,
          formats: ['csv', 'xlsx'],
          requiredPermissions: ['Admin', 'Principal'],
        }}
      />
      <WorkflowDefinitionFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
      />
    </>
  );
}

export default function WorkflowDefinitionsPageContent() {
  return (
    <WorkflowDefinitionProvider>
      <DefinitionsContent />
    </WorkflowDefinitionProvider>
  );
}
