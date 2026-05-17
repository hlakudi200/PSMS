'use client';

import React, { useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, UserSwitchOutlined, EyeOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, BulkAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { ClassProvider, useClassState, useClassActions } from '@/providers/academic/classes';
import { GradeProvider } from '@/providers/academic/grades';
import { AcademicYearProvider } from '@/providers/academic/academic_years';
import { TeacherProvider } from '@/providers/academic/teachers';
import { useAuthState } from '@/providers/auth';
import { ClassFormModal } from '@/components/modals/academic/ClassFormModal';
import { AssignTeacherModal } from '@/components/modals/academic/AssignTeacherModal';
import type { IClass } from '@/providers/academic/shared/interfaces';

function ClassesContent() {
  const { classes, totalCount, isPending, isError } = useClassState();
  const { getAllAsync, deleteAsync, activateAsync, deactivateAsync } = useClassActions();
  const { currentRole } = useAuthState();
  const router = useRouter();
  const [modalOpen, setModalOpen] = useState(false);
  const [assignTeacherOpen, setAssignTeacherOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<IClass | null>(null);
  const [assignRecord, setAssignRecord] = useState<IClass | null>(null);
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

  const handleAssignTeacherClose = (refresh?: boolean) => {
    setAssignTeacherOpen(false);
    setAssignRecord(null);
    if (refresh) refreshData();
  };

  const columns: ColumnConfig<IClass>[] = [
    { key: 'className', title: 'Class', dataIndex: 'className', sortable: true },
    { key: 'gradeName', title: 'Grade', dataIndex: 'gradeName', sortable: true },
    { key: 'academicYearName', title: 'Year', dataIndex: 'academicYearName' },
    { key: 'classTeacherName', title: 'Class Teacher', dataIndex: 'classTeacherName' },
    { key: 'maxCapacity', title: 'Capacity', dataIndex: 'maxCapacity', sortable: true },
    { key: 'studentCount', title: 'Students', dataIndex: 'studentCount' },
    { key: 'availableCapacity', title: 'Available', dataIndex: 'availableCapacity' },
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
      label: 'New Class',
      icon: <PlusOutlined />,
      type: 'primary',
      onClick: () => { setEditRecord(null); setModalOpen(true); },
    },
  ];

  const rowActions: RowAction<IClass>[] = [
    {
      key: 'view',
      label: 'View Details',
      icon: <EyeOutlined />,
      onClick: (record) => { router.push(`/principal/classes/${record.id}`); },
    },
    {
      key: 'edit',
      label: 'Edit',
      icon: <EditOutlined />,
      onClick: (record) => { setEditRecord(record); setModalOpen(true); },
    },
    {
      key: 'assignTeacher',
      label: 'Assign Teacher',
      icon: <UserSwitchOutlined />,
      onClick: (record) => { setAssignRecord(record); setAssignTeacherOpen(true); },
    },
    {
      key: 'delete',
      label: 'Delete',
      icon: <DeleteOutlined />,
      danger: true,
      confirm: { title: 'Delete this class?', description: 'This action cannot be undone.' },
      onClick: async (record) => {
        try {
          await deleteAsync(record.id);
          message.success('Class deleted');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
  ];

  const runBulk = async (
    rows: IClass[],
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
      message.success(`${ok} class(es) ${okLabel}`);
    } else if (ok === 0) {
      message.error(`No classes ${okLabel}. ${fail} ${failLabel}.`);
    } else {
      message.warning(`${ok} ${okLabel}; ${fail} ${failLabel}.`);
    }
    refreshData();
  };

  const bulkActions: BulkAction<IClass>[] = [
    {
      key: 'activate',
      label: 'Activate',
      onClick: (rows) => runBulk(rows, activateAsync, 'activated', 'failed'),
    },
    {
      key: 'deactivate',
      label: 'Deactivate',
      danger: true,
      confirm: { title: 'Deactivate selected classes?' },
      onClick: (rows) => runBulk(rows, deactivateAsync, 'deactivated', 'failed'),
    },
  ];

  return (
    <>
      <EnterpriseTable<IClass>
        title="Classes"
        columns={columns}
        data={classes ?? []}
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
      <ClassFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
      />
      <AssignTeacherModal
        open={assignTeacherOpen}
        onClose={handleAssignTeacherClose}
        classRecord={assignRecord}
      />
    </>
  );
}

export default function ClassesPageContent() {
  return (
    <ClassProvider>
      <GradeProvider>
        <AcademicYearProvider>
          <TeacherProvider>
            <ClassesContent />
          </TeacherProvider>
        </AcademicYearProvider>
      </GradeProvider>
    </ClassProvider>
  );
}
