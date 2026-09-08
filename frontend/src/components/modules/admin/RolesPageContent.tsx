'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { RoleProvider, useRoleState, useRoleActions } from '@/providers/admin/roles';
import { useAuthState } from '@/providers/auth';
import { RoleFormModal } from '@/components/modals/admin/RoleFormModal';
import type { IAdminRole } from '@/providers/admin/shared/interfaces';

function RolesContent() {
  const { roles, totalCount, isPending, isError } = useRoleState();
  const { getAllAsync, deleteAsync } = useRoleActions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<IAdminRole | null>(null);
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);

  const handleQueryChange = useCallback((query: TableQuery) => {
    setLastQuery(query);
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      keyword: query.filters?.keyword as string | undefined,
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

  const columns: ColumnConfig<IAdminRole>[] = [
    { key: 'name', title: 'Name', dataIndex: 'name', sortable: true },
    { key: 'displayName', title: 'Display Name', dataIndex: 'displayName', sortable: true },
    { key: 'isStatic', title: 'Static', dataIndex: 'isStatic', renderType: 'boolean' },
    { key: 'isDefault', title: 'Default', dataIndex: 'isDefault', renderType: 'boolean' },
    { key: 'creationTime', title: 'Created', dataIndex: 'creationTime', renderType: 'date', hideOnMobile: true },
  ];

  const toolbarActions: ToolbarAction[] = [
    {
      key: 'new',
      label: 'New Role',
      icon: <PlusOutlined />,
      type: 'primary',
      requiredPermissions: ['Admin'],
      onClick: () => { setEditRecord(null); setModalOpen(true); },
    },
  ];

  const rowActions: RowAction<IAdminRole>[] = [
    {
      key: 'edit',
      label: 'Edit',
      icon: <EditOutlined />,
      requiredPermissions: ['Admin'],
      onClick: (record) => { setEditRecord(record); setModalOpen(true); },
    },
    {
      key: 'delete',
      label: 'Delete',
      icon: <DeleteOutlined />,
      danger: true,
      requiredPermissions: ['Admin'],
      confirm: { title: 'Delete this role?', description: 'This action cannot be undone.' },
      disabled: (record) => record.isStatic,
      onClick: async (record) => {
        await deleteAsync(record.id);
        message.success('Role deleted');
        refreshData();
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<IAdminRole>
        title="Roles"
        columns={columns}
        data={roles ?? []}
        totalCount={totalCount}
        loading={isPending}
        error={isError}
        onQueryChange={handleQueryChange}
        rowKey="id"
        toolbarActions={toolbarActions}
        rowActions={rowActions}
        selectionMode="none"
        currentUserRole={currentRole}
        searchable
        searchPlaceholder="Search roles..."
        searchFilterKey="keyword"
        exportConfig={{
          enabled: true,
          formats: ['csv', 'xlsx'],
        }}
      />
      <RoleFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
      />
    </>
  );
}

export default function RolesPageContent() {
  return (
    <RoleProvider>
      <RolesContent />
    </RoleProvider>
  );
}
