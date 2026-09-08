'use client';

import React, { useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { usePortalBase } from '@/utils/portal-base';
import { EyeOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction } from '@/components/shared/enterprise-table';
import { ParentProvider, useParentState, useParentActions } from '@/providers/academic/parents';
import { useAuthState } from '@/providers/auth';
import type { IParent } from '@/providers/academic/shared/interfaces';

function ParentsContent() {
  const router = useRouter();
  const portalBase = usePortalBase();
  const { parents, totalCount, isPending, isError } = useParentState();
  const { getAllAsync } = useParentActions();
  const { currentRole } = useAuthState();

  const handleQueryChange = useCallback((query: TableQuery) => {
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      ...query.filters,
    });
  }, [getAllAsync]);

  const columns: ColumnConfig<IParent>[] = [
    { key: 'fullName', title: 'Full Name', dataIndex: 'fullName', sortable: true },
    { key: 'email', title: 'Email', dataIndex: 'email', sortable: true },
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
        router.push(`${portalBase}/parents/${record.id}`);
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
