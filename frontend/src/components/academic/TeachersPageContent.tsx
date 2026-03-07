'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, BulkAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { TeacherProvider, useTeacherState, useTeacherActions } from '@/providers/academic/teachers';
import { useAuthState } from '@/providers/auth';
import { TeacherFormModal } from '@/components/academic/TeacherFormModal';
import type { ITeacher } from '@/providers/academic/shared/interfaces';

function TeachersContent() {
  const { teachers, totalCount, isPending, isError } = useTeacherState();
  const { getAllAsync, deleteAsync, activateAsync, deactivateAsync } = useTeacherActions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<ITeacher | null>(null);
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);

  const handleQueryChange = useCallback((query: TableQuery) => {
    setLastQuery(query);
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
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

  const columns: ColumnConfig<ITeacher>[] = [
    { key: 'employeeNumber', title: 'Emp #', dataIndex: 'employeeNumber', sortable: true, width: 100 },
    { key: 'fullName', title: 'Name', dataIndex: 'fullName', sortable: true, filterable: true },
    { key: 'email', title: 'Email', dataIndex: 'email', hideOnMobile: true },
    { key: 'phone', title: 'Phone', dataIndex: 'phone', hideOnMobile: true },
    { key: 'subjectAssignmentCount', title: 'Subjects', dataIndex: 'subjectAssignmentCount' },
    { key: 'classAssignmentCount', title: 'Classes', dataIndex: 'classAssignmentCount' },
    {
      key: 'isActive', title: 'Status', dataIndex: 'isActive',
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
      label: 'New Teacher',
      icon: <PlusOutlined />,
      type: 'primary',
      onClick: () => { setEditRecord(null); setModalOpen(true); },
    },
  ];

  const rowActions: RowAction<ITeacher>[] = [
    {
      key: 'edit',
      label: 'Edit',
      icon: <EditOutlined />,
      onClick: (record) => { setEditRecord(record); setModalOpen(true); },
    },
    {
      key: 'delete',
      label: 'Delete',
      icon: <DeleteOutlined />,
      danger: true,
      confirm: { title: 'Delete this teacher?', description: 'This action cannot be undone.' },
      onClick: async (record) => {
        await deleteAsync(record.id);
        message.success('Teacher deleted');
        refreshData();
      },
    },
  ];

  const bulkActions: BulkAction<ITeacher>[] = [
    {
      key: 'activate',
      label: 'Activate',
      onClick: async (rows) => {
        for (const row of rows) await activateAsync(row.id);
        message.success(`${rows.length} teacher(s) activated`);
        refreshData();
      },
    },
    {
      key: 'deactivate',
      label: 'Deactivate',
      danger: true,
      confirm: { title: 'Deactivate selected teachers?' },
      onClick: async (rows) => {
        for (const row of rows) await deactivateAsync(row.id);
        message.success(`${rows.length} teacher(s) deactivated`);
        refreshData();
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<ITeacher>
        title="Teachers"
        columns={columns}
        data={teachers ?? []}
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
          requiredPermissions: ['Admin', 'Principal', 'VicePrincipal'],
        }}
      />
      <TeacherFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
      />
    </>
  );
}

export default function TeachersPageContent() {
  return (
    <TeacherProvider>
      <TeachersContent />
    </TeacherProvider>
  );
}
