'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, CheckCircleOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { AcademicYearProvider, useAcademicYearState, useAcademicYearActions } from '@/providers/academic/academic_years';
import { useAuthState } from '@/providers/auth';
import { AcademicYearFormModal } from '@/components/academic/AcademicYearFormModal';
import type { IAcademicYear } from '@/providers/academic/shared/interfaces';

function AcademicYearsContent() {
  const { academicYears, totalCount, isPending, isError } = useAcademicYearState();
  const { getAllAsync, deleteAsync, setAsCurrentAsync } = useAcademicYearActions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<IAcademicYear | null>(null);
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

  const columns: ColumnConfig<IAcademicYear>[] = [
    { key: 'yearName', title: 'Year', dataIndex: 'yearName', sortable: true, filterable: true },
    { key: 'year', title: 'Year Number', dataIndex: 'year', sortable: true },
    { key: 'startDate', title: 'Start Date', dataIndex: 'startDate', sortable: true, renderType: 'date' },
    { key: 'endDate', title: 'End Date', dataIndex: 'endDate', sortable: true, renderType: 'date' },
    { key: 'termCount', title: 'Terms', dataIndex: 'termCount' },
    { key: 'classCount', title: 'Classes', dataIndex: 'classCount' },
    {
      key: 'isCurrent', title: 'Current', dataIndex: 'isCurrent',
      renderType: 'status',
      renderConfig: {
        statusMap: {
          true: { label: 'Active', color: 'green' },
          false: { label: 'No', color: 'default' },
        },
      },
    },
  ];

  const toolbarActions: ToolbarAction[] = [
    {
      key: 'new',
      label: 'New Academic Year',
      icon: <PlusOutlined />,
      type: 'primary',
      onClick: () => { setEditRecord(null); setModalOpen(true); },
    },
  ];

  const rowActions: RowAction<IAcademicYear>[] = [
    {
      key: 'edit',
      label: 'Edit',
      icon: <EditOutlined />,
      onClick: (record) => { setEditRecord(record); setModalOpen(true); },
    },
    {
      key: 'setCurrent',
      label: 'Set as Current',
      icon: <CheckCircleOutlined />,
      visible: (record) => !record.isCurrent,
      onClick: async (record) => {
        await setAsCurrentAsync(record.id);
        message.success('Academic year set as current');
        refreshData();
      },
    },
    {
      key: 'delete',
      label: 'Delete',
      icon: <DeleteOutlined />,
      danger: true,
      confirm: { title: 'Delete this academic year?', description: 'This action cannot be undone.' },
      onClick: async (record) => {
        await deleteAsync(record.id);
        message.success('Academic year deleted');
        refreshData();
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<IAcademicYear>
        title="Academic Years"
        columns={columns}
        data={academicYears ?? []}
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
          requiredPermissions: ['Admin', 'Principal', 'VicePrincipal'],
        }}
      />
      <AcademicYearFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
      />
    </>
  );
}

export default function AcademicYearsPage() {
  return (
    <AcademicYearProvider>
      <AcademicYearsContent />
    </AcademicYearProvider>
  );
}
