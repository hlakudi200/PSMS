'use client';

import React, { useCallback } from 'react';
import { Typography } from 'antd';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery } from '@/components/shared/enterprise-table';
import {
  LearningMaterialProvider,
  useLearningMaterialState,
  useLearningMaterialActions,
} from '@/providers/learning/learning_materials';
import { useAuthState } from '@/providers/auth';
import type { ILearningMaterialList } from '@/providers/learning/shared/interfaces';

const { Text } = Typography;

// Mirrors backend psms.Domain.Shared.Enums.LearningMaterialType.
const MATERIAL_TYPE_META: Record<number, { label: string; color: string }> = {
  1: { label: 'Document', color: 'blue' },
  2: { label: 'Video', color: 'purple' },
  3: { label: 'Audio', color: 'cyan' },
  4: { label: 'Presentation', color: 'gold' },
  5: { label: 'Worksheet', color: 'orange' },
  6: { label: 'External Link', color: 'magenta' },
  7: { label: 'Image', color: 'green' },
  8: { label: 'Interactive', color: 'geekblue' },
};

function formatBytes(bytes?: number): string {
  if (bytes == null || bytes <= 0) return '—';
  const units = ['B', 'KB', 'MB', 'GB'];
  let value = bytes;
  let unit = 0;
  while (value >= 1024 && unit < units.length - 1) {
    value /= 1024;
    unit += 1;
  }
  return `${value.toFixed(unit === 0 ? 0 : 1)} ${units[unit]}`;
}

/**
 * School-wide, read-only view of learning materials across every class and
 * subject. Teachers manage their own materials in the teacher portal; this
 * gives management oversight of what has been published.
 */
function MaterialsOverviewContent() {
  const { learningMaterials, totalCount, isPending, isError } = useLearningMaterialState();
  const { getAllAsync } = useLearningMaterialActions();
  const { currentRole } = useAuthState();

  const handleQueryChange = useCallback((query: TableQuery) => {
    const { keyword, ...columnFilters } = query.filters ?? {};
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      keyword: keyword as string | undefined,
      materialType: columnFilters.materialType as number | undefined,
      isPublished: columnFilters.isPublished as boolean | undefined,
    });
  }, [getAllAsync]);

  const columns: ColumnConfig<ILearningMaterialList>[] = [
    { key: 'title', title: 'Title', dataIndex: 'title', sortable: true },
    {
      key: 'materialType', title: 'Type', dataIndex: 'materialType', width: 140,
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(MATERIAL_TYPE_META).map(([v, m]) => ({ label: m.label, value: Number(v) })),
      renderType: 'status', renderConfig: { statusMap: MATERIAL_TYPE_META },
    },
    {
      key: 'fileName', title: 'File', dataIndex: 'fileName', hideOnMobile: true,
      render: (_: unknown, record: ILearningMaterialList) => (
        <span>
          {record.fileName ?? '—'}
          {record.fileSizeBytes ? <Text type="secondary"> ({formatBytes(record.fileSizeBytes)})</Text> : null}
        </span>
      ),
    },
    {
      key: 'isPublished', title: 'Status', dataIndex: 'isPublished', width: 120,
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Published', value: true },
        { label: 'Draft', value: false },
      ],
      renderType: 'status',
      renderConfig: {
        statusMap: {
          true: { label: 'Published', color: 'green' },
          false: { label: 'Draft', color: 'default' },
        },
      },
    },
    { key: 'viewCount', title: 'Views', dataIndex: 'viewCount', sortable: true, width: 90 },
  ];

  return (
    <EnterpriseTable<ILearningMaterialList>
      title="Learning Materials"
      columns={columns}
      data={learningMaterials ?? []}
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

export default function MaterialsOverviewPageContent() {
  return (
    <LearningMaterialProvider>
      <MaterialsOverviewContent />
    </LearningMaterialProvider>
  );
}
