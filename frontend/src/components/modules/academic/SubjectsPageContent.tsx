'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, BulkAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { SubjectProvider, useSubjectState, useSubjectActions } from '@/providers/academic/subjects';
import { useAuthState } from '@/providers/auth';
import { SubjectFormModal } from '@/components/modals/academic/SubjectFormModal';
import type { ISubject } from '@/providers/academic/shared/interfaces';

function SubjectsContent() {
  const { subjects, totalCount, isPending, isError } = useSubjectState();
  const { getAllAsync, deleteAsync, activateAsync, deactivateAsync } = useSubjectActions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<ISubject | null>(null);
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

  const columns: ColumnConfig<ISubject>[] = [
    { key: 'subjectCode', title: 'Code', dataIndex: 'subjectCode', sortable: true, width: 100 },
    { key: 'subjectName', title: 'Subject', dataIndex: 'subjectName', sortable: true },
    { key: 'isCore', title: 'Core', dataIndex: 'isCore', renderType: 'boolean',
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Core', value: 'true' },
        { label: 'Elective', value: 'false' },
      ],
    },
    { key: 'gradeCount', title: 'Grades', dataIndex: 'gradeCount' },
    { key: 'teacherCount', title: 'Teachers', dataIndex: 'teacherCount' },
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
      label: 'New Subject',
      icon: <PlusOutlined />,
      type: 'primary',
      onClick: () => { setEditRecord(null); setModalOpen(true); },
    },
  ];

  const rowActions: RowAction<ISubject>[] = [
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
      confirm: { title: 'Delete this subject?', description: 'This action cannot be undone.' },
      onClick: async (record) => {
        try {
          await deleteAsync(record.id);
          message.success('Subject deleted');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
  ];

  const runBulk = async (
    rows: ISubject[],
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
      message.success(`${ok} subject(s) ${okLabel}`);
    } else if (ok === 0) {
      message.error(`No subjects ${okLabel}. ${fail} ${failLabel}.`);
    } else {
      message.warning(`${ok} ${okLabel}; ${fail} ${failLabel}.`);
    }
    refreshData();
  };

  const bulkActions: BulkAction<ISubject>[] = [
    {
      key: 'activate',
      label: 'Activate',
      onClick: (rows) => runBulk(rows, activateAsync, 'activated', 'failed'),
    },
    {
      key: 'deactivate',
      label: 'Deactivate',
      danger: true,
      confirm: { title: 'Deactivate selected subjects?' },
      onClick: (rows) => runBulk(rows, deactivateAsync, 'deactivated', 'failed'),
    },
  ];

  return (
    <>
      <EnterpriseTable<ISubject>
        title="Subjects"
        columns={columns}
        data={subjects ?? []}
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
      <SubjectFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
      />
    </>
  );
}

export default function SubjectsPageContent() {
  return (
    <SubjectProvider>
      <SubjectsContent />
    </SubjectProvider>
  );
}
