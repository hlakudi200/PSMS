'use client';

import React, { useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Card, Tabs } from 'antd';
import { EyeOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction } from '@/components/shared/enterprise-table';
import { SchoolTransportProvider, useSchoolTransportState, useSchoolTransportActions } from '@/providers/saspecific/school_transports';
import { StudentTransportProvider, useStudentTransportState, useStudentTransportActions } from '@/providers/saspecific/student_transports';
import { useAuthState } from '@/providers/auth';
import type { ISchoolTransportList, IStudentTransportList } from '@/providers/saspecific/shared/interfaces';

const transportTypeMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Bus', color: 'blue' },
  2: { label: 'Minibus', color: 'cyan' },
  3: { label: 'Sedan', color: 'green' },
  4: { label: 'Other', color: 'default' },
};

const directionMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Morning', color: 'orange' },
  2: { label: 'Afternoon', color: 'purple' },
  3: { label: 'Both', color: 'blue' },
};

const enrollmentStatusMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Active', color: 'green' },
  2: { label: 'Terminated', color: 'red' },
  3: { label: 'Suspended', color: 'orange' },
};

// ─── Routes Tab ─────────────────────────────────────────────────
function RoutesTab() {
  const router = useRouter();
  const { schoolTransports, totalCount, isPending, isError } = useSchoolTransportState();
  const { getAllAsync } = useSchoolTransportActions();
  const { currentRole } = useAuthState();

  const handleQueryChange = useCallback((query: TableQuery) => {
    const { keyword, ...columnFilters } = query.filters ?? {};
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      routeName: keyword as string | undefined,
      transportType: columnFilters.transportType as number | undefined,
      isActive: columnFilters.isActive as boolean | undefined,
    });
  }, [getAllAsync]);

  const columns: ColumnConfig<ISchoolTransportList>[] = [
    { key: 'routeName', title: 'Route Name', dataIndex: 'routeName', sortable: true },
    { key: 'vehicleNumber', title: 'Vehicle #', dataIndex: 'vehicleNumber', sortable: true, width: 110 },
    {
      key: 'transportType', title: 'Type', dataIndex: 'transportType', width: 100,
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Bus', value: 1 },
        { label: 'Minibus', value: 2 },
        { label: 'Sedan', value: 3 },
        { label: 'Other', value: 4 },
      ],
      renderType: 'status',
      renderConfig: { statusMap: transportTypeMap },
    },
    { key: 'capacity', title: 'Capacity', dataIndex: 'capacity', sortable: true, width: 90 },
    { key: 'currentEnrollment', title: 'Enrolled', dataIndex: 'currentEnrollment', sortable: true, width: 90 },
    {
      key: 'available', title: 'Available', dataIndex: 'capacity', width: 90,
      render: (_: number, record: ISchoolTransportList) => record.capacity - record.currentEnrollment,
    },
    { key: 'driverName', title: 'Driver', dataIndex: 'driverName', hideOnMobile: true },
    {
      key: 'monthlyFee', title: 'Monthly Fee', dataIndex: 'monthlyFee', sortable: true, width: 120,
      render: (value: number) => `R ${value.toLocaleString('en-ZA', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`,
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

  const rowActions: RowAction<ISchoolTransportList>[] = [
    {
      key: 'viewStudents',
      label: 'View Students',
      icon: <EyeOutlined />,
      onClick: (record) => {
        router.push(`/principal/transport/${record.id}`);
      },
    },
  ];

  return (
    <EnterpriseTable<ISchoolTransportList>
      title="Transport Routes"
      columns={columns}
      data={schoolTransports ?? []}
      totalCount={totalCount}
      loading={isPending}
      error={isError}
      onQueryChange={handleQueryChange}
      rowKey="id"
      rowActions={rowActions}
      currentUserRole={currentRole}
      exportConfig={{
        enabled: true,
        formats: ['csv', 'xlsx'],
        requiredPermissions: ['Admin', 'Principal', 'VicePrincipal'],
      }}
    />
  );
}

// ─── Enrollments Tab ────────────────────────────────────────────
function EnrollmentsTab() {
  const { studentTransports, totalCount, isPending, isError } = useStudentTransportState();
  const { getAllAsync } = useStudentTransportActions();
  const { currentRole } = useAuthState();

  const handleQueryChange = useCallback((query: TableQuery) => {
    const { keyword, ...columnFilters } = query.filters ?? {};
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      status: columnFilters.status as number | undefined,
      direction: columnFilters.direction as number | undefined,
    });
  }, [getAllAsync]);

  const columns: ColumnConfig<IStudentTransportList>[] = [
    { key: 'studentName', title: 'Student', dataIndex: 'studentName', sortable: true },
    { key: 'studentAdmissionNumber', title: 'Admission #', dataIndex: 'studentAdmissionNumber', sortable: true, width: 110 },
    { key: 'routeName', title: 'Route', dataIndex: 'routeName', sortable: true },
    {
      key: 'direction', title: 'Direction', dataIndex: 'direction', width: 100,
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Morning', value: 1 },
        { label: 'Afternoon', value: 2 },
        { label: 'Both', value: 3 },
      ],
      renderType: 'status',
      renderConfig: { statusMap: directionMap },
    },
    { key: 'academicYearName', title: 'Year', dataIndex: 'academicYearName', sortable: true, hideOnMobile: true },
    {
      key: 'status', title: 'Status', dataIndex: 'status', width: 110,
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Active', value: 1 },
        { label: 'Terminated', value: 2 },
        { label: 'Suspended', value: 3 },
      ],
      renderType: 'status',
      renderConfig: { statusMap: enrollmentStatusMap },
    },
    {
      key: 'startDate', title: 'Start Date', dataIndex: 'startDate', sortable: true, width: 110,
      renderType: 'date',
    },
  ];

  return (
    <EnterpriseTable<IStudentTransportList>
      title="Student Transport Enrollments"
      columns={columns}
      data={studentTransports ?? []}
      totalCount={totalCount}
      loading={isPending}
      error={isError}
      onQueryChange={handleQueryChange}
      rowKey="id"
      currentUserRole={currentRole}
      exportConfig={{
        enabled: true,
        formats: ['csv', 'xlsx'],
        requiredPermissions: ['Admin', 'Principal', 'VicePrincipal'],
      }}
    />
  );
}

// ─── Main Content ───────────────────────────────────────────────
function TransportContent() {
  const tabItems = [
    {
      key: 'routes',
      label: 'Routes',
      children: <RoutesTab />,
    },
    {
      key: 'enrollments',
      label: 'Student Enrollments',
      children: <EnrollmentsTab />,
    },
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      <Tabs items={tabItems} />
    </div>
  );
}

export default function TransportPageContent() {
  return (
    <SchoolTransportProvider>
      <StudentTransportProvider>
        <TransportContent />
      </StudentTransportProvider>
    </SchoolTransportProvider>
  );
}
