'use client';

import React, { useCallback, useEffect, useState } from 'react';
import { Tabs, message } from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  CheckCircleOutlined,
  StopOutlined,
} from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { AfterCareProvider, useAfterCareState, useAfterCareActions } from '@/providers/saspecific/after_cares';
import {
  StudentAfterCareProvider,
  useStudentAfterCareState,
  useStudentAfterCareActions,
} from '@/providers/saspecific/student_after_cares';
import { AcademicYearProvider, useAcademicYearState, useAcademicYearActions } from '@/providers/academic/academic_years';
import { useAuthState } from '@/providers/auth';
import { AfterCareFormModal, AFTER_CARE_TYPE_OPTIONS } from '@/components/modals/saspecific/AfterCareFormModal';
import type { IAfterCareList, IStudentAfterCareList } from '@/providers/saspecific/shared/interfaces';
import { formatZAR } from '@/utils/currency';

const MANAGE_ROLES = ['Admin', 'Principal', 'VicePrincipal'];

const afterCareTypeMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Standard', color: 'blue' },
  2: { label: 'Extended', color: 'purple' },
  3: { label: 'Holiday Programme', color: 'gold' },
};

// Mirrors backend psms.Domain.Shared.Enums.EnrollmentStatus.
const enrollmentStatusMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Active', color: 'green' },
  2: { label: 'Suspended', color: 'orange' },
  3: { label: 'Terminated', color: 'red' },
  4: { label: 'Pending', color: 'blue' },
};

function formatTime(value?: string): string {
  if (!value) return '—';
  return value.slice(0, 5);
}

// ─── Programmes Tab ─────────────────────────────────────────────
function ProgrammesTab() {
  const { afterCares, totalCount, isPending, isError } = useAfterCareState();
  const { getAllAsync, deleteAsync, activateAsync, deactivateAsync } = useAfterCareActions();
  const { academicYears } = useAcademicYearState();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<IAfterCareList | null>(null);
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);

  const handleQueryChange = useCallback((query: TableQuery) => {
    setLastQuery(query);
    const { keyword, ...columnFilters } = query.filters ?? {};
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      programName: keyword as string | undefined,
      afterCareType: columnFilters.afterCareType as number | undefined,
      isActive: columnFilters.isActive as boolean | undefined,
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

  const runAction = async (fn: () => Promise<void> | void, success: string) => {
    try {
      await fn();
      message.success(success);
      refreshData();
    } catch {
      // Axios interceptor handles ABP error display
    }
  };

  const columns: ColumnConfig<IAfterCareList>[] = [
    { key: 'programName', title: 'Programme', dataIndex: 'programName', sortable: true },
    {
      key: 'afterCareType', title: 'Type', dataIndex: 'afterCareType', width: 150,
      filterable: true, filterType: 'enum', filterOptions: AFTER_CARE_TYPE_OPTIONS,
      renderType: 'status', renderConfig: { statusMap: afterCareTypeMap },
    },
    {
      key: 'time', title: 'Hours', dataIndex: 'startTime', width: 120,
      render: (_: unknown, record: IAfterCareList) => `${formatTime(record.startTime)} – ${formatTime(record.endTime)}`,
    },
    { key: 'location', title: 'Location', dataIndex: 'location', hideOnMobile: true },
    { key: 'capacity', title: 'Capacity', dataIndex: 'capacity', sortable: true, width: 90 },
    { key: 'currentEnrollment', title: 'Enrolled', dataIndex: 'currentEnrollment', sortable: true, width: 90 },
    {
      key: 'available', title: 'Available', dataIndex: 'capacity', width: 90,
      render: (_: number, record: IAfterCareList) => Math.max(record.capacity - record.currentEnrollment, 0),
    },
    { key: 'supervisorName', title: 'Supervisor', dataIndex: 'supervisorName', hideOnMobile: true },
    {
      key: 'monthlyFee', title: 'Monthly Fee', dataIndex: 'monthlyFee', sortable: true, width: 120,
      render: (value: number) => formatZAR(value),
    },
    {
      key: 'isActive', title: 'Status', dataIndex: 'isActive', width: 100,
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Active', value: true },
        { label: 'Inactive', value: false },
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
      label: 'New Programme',
      icon: <PlusOutlined />,
      type: 'primary',
      requiredPermissions: MANAGE_ROLES,
      onClick: () => { setEditRecord(null); setModalOpen(true); },
    },
  ];

  const rowActions: RowAction<IAfterCareList>[] = [
    {
      key: 'edit',
      label: 'Edit',
      icon: <EditOutlined />,
      requiredPermissions: MANAGE_ROLES,
      onClick: (record) => { setEditRecord(record); setModalOpen(true); },
    },
    {
      key: 'activate',
      label: 'Activate',
      icon: <CheckCircleOutlined />,
      requiredPermissions: MANAGE_ROLES,
      visible: (record) => !record.isActive,
      onClick: (record) => runAction(() => activateAsync(record.id), 'Programme activated'),
    },
    {
      key: 'deactivate',
      label: 'Deactivate',
      icon: <StopOutlined />,
      requiredPermissions: MANAGE_ROLES,
      visible: (record) => record.isActive,
      confirm: { title: 'Deactivate this programme?', description: 'New enrolments will be blocked.' },
      onClick: (record) => runAction(() => deactivateAsync(record.id), 'Programme deactivated'),
    },
    {
      key: 'delete',
      label: 'Delete',
      icon: <DeleteOutlined />,
      danger: true,
      requiredPermissions: MANAGE_ROLES,
      disabled: (record) => record.currentEnrollment > 0,
      confirm: { title: 'Delete this programme?', description: 'This action cannot be undone.' },
      onClick: (record) => runAction(() => deleteAsync(record.id), 'Programme deleted'),
    },
  ];

  return (
    <>
      <EnterpriseTable<IAfterCareList>
        title="After Care Programmes"
        columns={columns}
        data={afterCares ?? []}
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
          requiredPermissions: MANAGE_ROLES,
        }}
      />
      <AfterCareFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
        academicYears={(academicYears ?? []).map((y) => ({ value: y.id, label: y.yearName }))}
      />
    </>
  );
}

// ─── Enrolments Tab ─────────────────────────────────────────────
function EnrolmentsTab() {
  const { studentAfterCares, totalCount, isPending, isError } = useStudentAfterCareState();
  const { getAllAsync } = useStudentAfterCareActions();
  const { currentRole } = useAuthState();

  const handleQueryChange = useCallback((query: TableQuery) => {
    const columnFilters = query.filters ?? {};
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      status: columnFilters.status as number | undefined,
    });
  }, [getAllAsync]);

  const columns: ColumnConfig<IStudentAfterCareList>[] = [
    { key: 'studentName', title: 'Student', dataIndex: 'studentName', sortable: true },
    { key: 'studentAdmissionNumber', title: 'Admission #', dataIndex: 'studentAdmissionNumber', sortable: true, width: 120 },
    { key: 'afterCareProgramName', title: 'Programme', dataIndex: 'afterCareProgramName', sortable: true },
    { key: 'daysEnrolled', title: 'Days', dataIndex: 'daysEnrolled', hideOnMobile: true },
    { key: 'academicYearName', title: 'Year', dataIndex: 'academicYearName', sortable: true, hideOnMobile: true },
    {
      key: 'status', title: 'Status', dataIndex: 'status', width: 110,
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(enrollmentStatusMap).map(([v, m]) => ({ label: m.label, value: Number(v) })),
      renderType: 'status', renderConfig: { statusMap: enrollmentStatusMap },
    },
    { key: 'startDate', title: 'Start Date', dataIndex: 'startDate', sortable: true, width: 110, renderType: 'date' },
    { key: 'endDate', title: 'End Date', dataIndex: 'endDate', width: 110, renderType: 'date', hideOnMobile: true },
  ];

  return (
    <EnterpriseTable<IStudentAfterCareList>
      title="After Care Enrolments"
      columns={columns}
      data={studentAfterCares ?? []}
      totalCount={totalCount}
      loading={isPending}
      error={isError}
      onQueryChange={handleQueryChange}
      rowKey="id"
      currentUserRole={currentRole}
      exportConfig={{
        enabled: true,
        formats: ['csv', 'xlsx'],
        requiredPermissions: MANAGE_ROLES,
      }}
    />
  );
}

// ─── Main Content ───────────────────────────────────────────────
function AfterCareContent() {
  const { getAllAsync: getAcademicYears } = useAcademicYearActions();

  useEffect(() => {
    getAcademicYears({ maxResultCount: 50 });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const tabItems = [
    { key: 'programmes', label: 'Programmes', children: <ProgrammesTab /> },
    { key: 'enrolments', label: 'Student Enrolments', children: <EnrolmentsTab /> },
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      <Tabs items={tabItems} />
    </div>
  );
}

export default function AfterCarePageContent() {
  return (
    <AcademicYearProvider>
      <AfterCareProvider>
        <StudentAfterCareProvider>
          <AfterCareContent />
        </StudentAfterCareProvider>
      </AfterCareProvider>
    </AcademicYearProvider>
  );
}
