'use client';

import React, { useState, useCallback } from 'react';
import { EyeOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction } from '@/components/shared/enterprise-table';
import { ParentProvider, useParentState, useParentActions } from '@/providers/academic/parents';
import { useAuthState } from '@/providers/auth';
import type { IParent } from '@/providers/academic/shared/interfaces';

function ParentsContent() {
  const { parents, totalCount, isPending, isError } = useParentState();
  const { getAllAsync } = useParentActions();
  const { currentRole } = useAuthState();
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);

  const handleQueryChange = useCallback((query: TableQuery) => {
    setLastQuery(query);
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
    });
  }, [getAllAsync]);

  const columns: ColumnConfig<IParent>[] = [
    { key: 'fullName', title: 'Full Name', dataIndex: 'fullName', sortable: true, filterable: true },
    { key: 'email', title: 'Email', dataIndex: 'email', sortable: true, filterable: true },
    { key: 'phone', title: 'Phone', dataIndex: 'phone', hideOnMobile: true },
    { key: 'occupation', title: 'Occupation', dataIndex: 'occupation', hideOnMobile: true },
    { key: 'studentCount', title: 'Children', dataIndex: 'studentCount', sortable: true, width: 100 },
  ];

  const rowActions: RowAction<IParent>[] = [
    {
      key: 'view',
      label: 'View Profile',
      icon: <EyeOutlined />,
      onClick: (record) => {
        // TODO: Navigate to parent profile page when implemented
        console.log('View parent profile:', record.id);
      },
    },
  ];

  return (
    <EnterpriseTable<IParent>
      title="Parents / Guardians"
      columns={columns}
      data={parents ?? []}
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

export default function ParentsPageContent() {
  return (
    <ParentProvider>
      <ParentsContent />
    </ParentProvider>
  );
}
