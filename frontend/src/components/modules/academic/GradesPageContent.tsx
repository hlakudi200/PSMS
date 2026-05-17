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
        { label: 'Foundation', value: 1 },
        { label: 'Intermediate', value: 2 },
        { label: 'Senior', value: 3 },
        { label: 'FET', value: 4 },
      ],
      render: (value: number) => {
        const phases: Record<number, string> = { 1: 'Foundation', 2: 'Intermediate', 3: 'Senior', 4: 'FET' };
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
        try {
          await deleteAsync(record.id);
          message.success('Grade deleted');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
  ];

  const runBulk = async (
    rows: IGradeList[],
    fn: (id: string) => void | Promise<unknown>,
    okLabel: string,
    failLabel: string
  ) => {
    const results = await Promise.allSettled(
      rows.map((r) => Promise.resolve(fn(r.id) as unknown))
    );
    const ok = results.filter((r) => r.status === 'fulfilled').length;
    const fail = results.length - ok;
    if (fail === 0) {
      message.success(`${ok} grade(s) ${okLabel}`);
    } else if (ok === 0) {
      message.error(`No grades ${okLabel}. ${fail} ${failLabel}.`);
    } else {
      message.warning(`${ok} ${okLabel}; ${fail} ${failLabel}.`);
    }
    refreshData();
  };

  const bulkActions: BulkAction<IGradeList>[] = [
    {
      key: 'activate',
      label: 'Activate',
      onClick: (rows) => runBulk(rows, activateAsync, 'activated', 'failed'),
    },
    {
      key: 'deactivate',
      label: 'Deactivate',
      danger: true,
      confirm: { title: 'Deactivate selected grades?' },
      onClick: (rows) => runBulk(rows, deactivateAsync, 'deactivated', 'failed'),
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
