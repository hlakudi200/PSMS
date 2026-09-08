'use client';

import React, { useState, useCallback } from 'react';
import { message, Tag } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, KeyOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, BulkAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { UserProvider, useUserState, useUserActions } from '@/providers/admin/users';
import { useAuthState } from '@/providers/auth';
import { UserFormModal } from '@/components/modals/admin/UserFormModal';
import { ResetPasswordModal } from '@/components/modals/admin/ResetPasswordModal';
import type { IAdminUser } from '@/providers/admin/shared/interfaces';

function UsersContent() {
  const { users, totalCount, isPending, isError } = useUserState();
  const { getAllAsync, deleteAsync, activateAsync, deactivateAsync } = useUserActions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<IAdminUser | null>(null);
  const [resetOpen, setResetOpen] = useState(false);
  const [resetUserId, setResetUserId] = useState<number | null>(null);
  const [resetUserName, setResetUserName] = useState<string | undefined>();
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);

  const handleQueryChange = useCallback((query: TableQuery) => {
    setLastQuery(query);
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      keyword: query.filters?.keyword as string | undefined,
      isActive: query.filters?.isActive as boolean | undefined,
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

  const columns: ColumnConfig<IAdminUser>[] = [
    { key: 'userName', title: 'Username', dataIndex: 'userName', sortable: true },
    { key: 'name', title: 'First Name', dataIndex: 'name', sortable: true },
    { key: 'surname', title: 'Surname', dataIndex: 'surname', sortable: true, hideOnMobile: true },
    { key: 'emailAddress', title: 'Email', dataIndex: 'emailAddress', hideOnMobile: true },
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
    {
      key: 'roleNames', title: 'Roles', dataIndex: 'roleNames',
      hideOnMobile: true,
      render: (value: string[]) => (
        <>
          {(value ?? []).map(role => (
            <Tag key={role} color="blue">{role}</Tag>
          ))}
        </>
      ),
    },
    { key: 'lastLoginTime', title: 'Last Login', dataIndex: 'lastLoginTime', renderType: 'datetime', hideOnMobile: true },
    { key: 'creationTime', title: 'Created', dataIndex: 'creationTime', renderType: 'date', hideOnMobile: true },
  ];

  const toolbarActions: ToolbarAction[] = [
    {
      key: 'new',
      label: 'New User',
      requiredPermissions: ['Admin', 'Principal'],
      icon: <PlusOutlined />,
      type: 'primary',
      onClick: () => { setEditRecord(null); setModalOpen(true); },
    },
  ];

  const rowActions: RowAction<IAdminUser>[] = [
    {
      key: 'edit',
      label: 'Edit',
      requiredPermissions: ['Admin', 'Principal'],
      icon: <EditOutlined />,
      onClick: (record) => { setEditRecord(record); setModalOpen(true); },
    },
    {
      key: 'resetPassword',
      label: 'Reset Password',
      requiredPermissions: ['Admin', 'Principal'],
      icon: <KeyOutlined />,
      onClick: (record) => {
        setResetUserId(record.id);
        setResetUserName(record.userName);
        setResetOpen(true);
      },
    },
    {
      key: 'delete',
      label: 'Delete',
      requiredPermissions: ['Admin', 'Principal'],
      icon: <DeleteOutlined />,
      danger: true,
      confirm: { title: 'Delete this user?', description: 'This action cannot be undone.' },
      onClick: async (record) => {
        await deleteAsync(record.id);
        message.success('User deleted');
        refreshData();
      },
    },
  ];

  const bulkActions: BulkAction<IAdminUser>[] = [
    {
      key: 'activate',
      label: 'Activate',
      requiredPermissions: ['Admin'],
      onClick: async (rows) => {
        for (const row of rows) await activateAsync(row.id);
        message.success(`${rows.length} user(s) activated`);
        refreshData();
      },
    },
    {
      key: 'deactivate',
      label: 'Deactivate',
      danger: true,
      requiredPermissions: ['Admin'],
      confirm: { title: 'Deactivate selected users?' },
      onClick: async (rows) => {
        for (const row of rows) await deactivateAsync(row.id);
        message.success(`${rows.length} user(s) deactivated`);
        refreshData();
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<IAdminUser>
        title="Users"
        columns={columns}
        data={users ?? []}
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
        searchable
        searchPlaceholder="Search users..."
        searchFilterKey="keyword"
        exportConfig={{
          enabled: true,
          formats: ['csv', 'xlsx'],
        }}
      />
      <UserFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
      />
      <ResetPasswordModal
        open={resetOpen}
        onClose={() => { setResetOpen(false); setResetUserId(null); setResetUserName(undefined); }}
        userId={resetUserId}
        userName={resetUserName}
      />
    </>
  );
}

export default function UsersPageContent() {
  return (
    <UserProvider>
      <UsersContent />
    </UserProvider>
  );
}
