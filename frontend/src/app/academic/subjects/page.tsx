'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { SubjectProvider, useSubjectState, useSubjectActions } from '@/providers/academic/subjects';
import { useAuthState } from '@/providers/auth';
import { SubjectFormModal } from '@/components/academic/SubjectFormModal';
import type { ISubject } from '@/providers/academic/shared/interfaces';

function SubjectsContent() {
  const { subjects, totalCount, isPending, isError } = useSubjectState();
  const { getAllAsync, deleteAsync } = useSubjectActions();
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
    { key: 'subjectCode', title: 'Code', dataIndex: 'subjectCode', sortable: true, filterable: true, width: 100 },
    { key: 'subjectName', title: 'Subject', dataIndex: 'subjectName', sortable: true, filterable: true },
    { key: 'isCore', title: 'Core', dataIndex: 'isCore', renderType: 'boolean' },
    { key: 'gradeCount', title: 'Grades', dataIndex: 'gradeCount' },
    { key: 'teacherCount', title: 'Teachers', dataIndex: 'teacherCount' },
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
        await deleteAsync(record.id);
        message.success('Subject deleted');
        refreshData();
      },
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

export default function SubjectsPage() {
  return (
    <SubjectProvider>
      <SubjectsContent />
    </SubjectProvider>
  );
}
