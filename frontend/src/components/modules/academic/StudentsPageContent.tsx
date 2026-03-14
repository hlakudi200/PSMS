'use client';

import React, { useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { EyeOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction } from '@/components/shared/enterprise-table';
import { StudentProvider, useStudentState, useStudentActions } from '@/providers/academic/students';
import { useAuthState } from '@/providers/auth';
import type { IStudentList } from '@/providers/academic/shared/interfaces';

function StudentsContent() {
  const { students, totalCount, isPending, isError } = useStudentState();
  const { getAllAsync } = useStudentActions();
  const { currentRole } = useAuthState();
  const router = useRouter();
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

  const columns: ColumnConfig<IStudentList>[] = [
    { key: 'admissionNumber', title: 'Admission #', dataIndex: 'admissionNumber', sortable: true, width: 120 },
    { key: 'fullName', title: 'Full Name', dataIndex: 'fullName', sortable: true },
    { key: 'currentGradeName', title: 'Grade', dataIndex: 'currentGradeName', sortable: true },
    { key: 'currentClassName', title: 'Class', dataIndex: 'currentClassName', sortable: true },
    { key: 'gender', title: 'Gender', dataIndex: 'gender', hideOnMobile: true,
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Male', value: 1 },
        { label: 'Female', value: 2 },
        { label: 'Other', value: 3 },
      ],
      renderType: 'status',
      renderConfig: {
        statusMap: {
          1: { label: 'Male', color: 'blue' },
          2: { label: 'Female', color: 'pink' },
          3: { label: 'Other', color: 'default' },
        },
      },
    },
    { key: 'age', title: 'Age', dataIndex: 'age', sortable: true, hideOnMobile: true, width: 70 },
    {
      key: 'isActive', title: 'Status', dataIndex: 'isActive',
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

  const rowActions: RowAction<IStudentList>[] = [
    {
      key: 'view',
      label: 'View Profile',
      icon: <EyeOutlined />,
      onClick: (record) => {
        router.push(`/principal/students/${record.id}`);
      },
    },
  ];

  return (
    <EnterpriseTable<IStudentList>
      title="All Students"
      columns={columns}
      data={students ?? []}
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

export default function StudentsPageContent() {
  return (
    <StudentProvider>
      <StudentsContent />
    </StudentProvider>
  );
}
