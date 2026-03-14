'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, BulkAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { GradeProvider, useGradeState, useGradeActions } from '@/providers/academic/grades';
import { useAuthState } from '@/providers/auth';
import { GradeFormModal } from '@/components/modals/academic/GradeFormModal';
import type { IGradeList } from '@/providers/academic/shared/interfaces';

function GradesContent() {
  const { grades, totalCount, isPending, isError } = useGradeState();
  const { getAllAsync, deleteAsync, activateAsync, deactivateAsync } = useGradeActions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<IGradeList | null>(null);
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

  const columns: ColumnConfig<IGradeList>[] = [
    { key: 'gradeName', title: 'Grade', dataIndex: 'gradeName', sortable: true },
    { key: 'gradeLevel', title: 'Level', dataIndex: 'gradeLevel', sortable: true, width: 80 },
    {
      key: 'schoolPhase', title: 'Phase', dataIndex: 'schoolPhase',
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Foundation', value: 0 },
        { label: 'Intermediate', value: 1 },
        { label: 'Senior', value: 2 },
        { label: 'FET', value: 3 },
      ],
      render: (value: number) => {
        const phases = ['Foundation', 'Intermediate', 'Senior', 'FET'];
        return phases[value] ?? value;
      },
    },
    { key: 'classCount', title: 'Classes', dataIndex: 'classCount', sortable: true },
    { key: 'studentCount', title: 'Students', dataIndex: 'studentCount', sortable: true },
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
      label: 'New Grade',
      icon: <PlusOutlined />,
      type: 'primary',
      onClick: () => { setEditRecord(null); setModalOpen(true); },
    },
  ];

  const rowActions: RowAction<IGradeList>[] = [
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
      confirm: { title: 'Delete this grade?', description: 'This action cannot be undone.' },
      onClick: async (record) => {
        await deleteAsync(record.id);
        message.success('Grade deleted');
        refreshData();
      },
    },
  ];

  const bulkActions: BulkAction<IGradeList>[] = [
    {
      key: 'activate',
      label: 'Activate',
      onClick: async (rows) => {
        for (const row of rows) await activateAsync(row.id);
        message.success(`${rows.length} grade(s) activated`);
        refreshData();
      },
    },
    {
      key: 'deactivate',
      label: 'Deactivate',
      danger: true,
      confirm: { title: 'Deactivate selected grades?' },
      onClick: async (rows) => {
        for (const row of rows) await deactivateAsync(row.id);
        message.success(`${rows.length} grade(s) deactivated`);
        refreshData();
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<IGradeList>
        title="Grades"
        columns={columns}
        data={grades ?? []}
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
      <GradeFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
      />
    </>
  );
}

export default function GradesPageContent() {
  return (
    <GradeProvider>
      <GradesContent />
    </GradeProvider>
  );
}
