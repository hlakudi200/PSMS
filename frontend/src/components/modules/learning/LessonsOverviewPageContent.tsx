'use client';

import React, { useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Typography } from 'antd';
import { VideoCameraOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction } from '@/components/shared/enterprise-table';
import {
  OnlineLessonProvider,
  useOnlineLessonState,
  useOnlineLessonActions,
} from '@/providers/learning/online_lessons';
import { useAuthState } from '@/providers/auth';
import type { IOnlineLessonList } from '@/providers/learning/shared/interfaces';

const { Text } = Typography;

// Mirrors backend psms.Domain.Shared.Enums.OnlineLessonStatus.
const STATUS_META: Record<number, { label: string; color: string }> = {
  1: { label: 'Scheduled', color: 'blue' },
  2: { label: 'In progress', color: 'gold' },
  3: { label: 'Completed', color: 'green' },
  4: { label: 'Cancelled', color: 'red' },
};
const STATUS_IN_PROGRESS = 2;

const PLATFORM_INAPP = 7;
const PLATFORM_META: Record<number, { label: string; color: string }> = {
  1: { label: 'Zoom', color: 'blue' },
  2: { label: 'Microsoft Teams', color: 'purple' },
  3: { label: 'Google Meet', color: 'green' },
  4: { label: 'BigBlueButton', color: 'cyan' },
  5: { label: 'WebEx', color: 'geekblue' },
  6: { label: 'Other', color: 'default' },
  7: { label: 'In-App Live', color: 'magenta' },
};

/**
 * School-wide view of every scheduled / live / past online lesson. The teacher
 * portal scopes lessons to the signed-in teacher; management needs the whole
 * school, with the ability to drop into a live in-app lesson.
 */
function LessonsOverviewContent() {
  const router = useRouter();
  const { onlineLessons, totalCount, isPending, isError } = useOnlineLessonState();
  const { getAllAsync } = useOnlineLessonActions();
  const { currentRole } = useAuthState();

  const handleQueryChange = useCallback((query: TableQuery) => {
    const { keyword, ...columnFilters } = query.filters ?? {};
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      keyword: keyword as string | undefined,
      status: columnFilters.status as number | undefined,
    });
  }, [getAllAsync]);

  const columns: ColumnConfig<IOnlineLessonList>[] = [
    { key: 'title', title: 'Title', dataIndex: 'title', sortable: true },
    {
      key: 'classSubject', title: 'Class — Subject', dataIndex: 'className',
      render: (_: unknown, record: IOnlineLessonList) => (
        <span>
          {record.className ?? '—'}
          {record.subjectName ? <Text type="secondary"> — {record.subjectName}</Text> : null}
        </span>
      ),
    },
    {
      key: 'scheduledStartTime', title: 'Starts', dataIndex: 'scheduledStartTime',
      sortable: true, renderType: 'datetime', width: 170,
    },
    {
      key: 'durationMinutes', title: 'Duration', dataIndex: 'durationMinutes', width: 100,
      render: (value: number) => `${value} min`,
    },
    {
      key: 'platform', title: 'Platform', dataIndex: 'platform', width: 140,
      renderType: 'status', renderConfig: { statusMap: PLATFORM_META },
    },
    {
      key: 'status', title: 'Status', dataIndex: 'status', width: 120,
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(STATUS_META).map(([v, m]) => ({ label: m.label, value: Number(v) })),
      renderType: 'status', renderConfig: { statusMap: STATUS_META },
    },
    {
      key: 'hasRecording', title: 'Recording', dataIndex: 'hasRecording', width: 100,
      renderType: 'boolean', hideOnMobile: true,
    },
  ];

  const rowActions: RowAction<IOnlineLessonList>[] = [
    {
      key: 'joinLive',
      label: 'Join live',
      icon: <VideoCameraOutlined />,
      visible: (record) => record.status === STATUS_IN_PROGRESS && record.platform === PLATFORM_INAPP,
      onClick: (record) => { router.push(`/live-class/${record.id}`); },
    },
  ];

  return (
    <EnterpriseTable<IOnlineLessonList>
      title="Online Lessons"
      columns={columns}
      data={onlineLessons ?? []}
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

export default function LessonsOverviewPageContent() {
  return (
    <OnlineLessonProvider>
      <LessonsOverviewContent />
    </OnlineLessonProvider>
  );
}
