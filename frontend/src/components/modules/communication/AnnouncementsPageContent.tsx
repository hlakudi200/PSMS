'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  SendOutlined,
  StopOutlined,
  PushpinOutlined,
} from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { AnnouncementProvider, useAnnouncementState, useAnnouncementActions } from '@/providers/communication/announcements';
import { useAuthState } from '@/providers/auth';
import { AnnouncementFormModal } from '@/components/modals/communication/AnnouncementFormModal';
import type { IAnnouncementList } from '@/providers/communication/shared/interfaces';
import {
  AnnouncementType,
  AnnouncementPriority,
  AnnouncementAudience,
  announcementTypeLabels,
  announcementAudienceLabels,
} from '@/providers/shared/enums';

const typeColors: Record<AnnouncementType, string> = {
  [AnnouncementType.General]: 'default',
  [AnnouncementType.Academic]: 'blue',
  [AnnouncementType.Sports]: 'green',
  [AnnouncementType.Event]: 'purple',
  [AnnouncementType.Emergency]: 'red',
  [AnnouncementType.Holiday]: 'orange',
  [AnnouncementType.Administrative]: 'cyan',
};

const priorityColors: Record<AnnouncementPriority, string> = {
  [AnnouncementPriority.Low]: 'default',
  [AnnouncementPriority.Normal]: 'blue',
  [AnnouncementPriority.High]: 'orange',
  [AnnouncementPriority.Urgent]: 'red',
};

const audienceColors: Record<AnnouncementAudience, string> = {
  [AnnouncementAudience.All]: 'green',
  [AnnouncementAudience.Staff]: 'geekblue',
  [AnnouncementAudience.Teachers]: 'blue',
  [AnnouncementAudience.Parents]: 'purple',
  [AnnouncementAudience.Students]: 'orange',
  [AnnouncementAudience.Grade]: 'cyan',
  [AnnouncementAudience.Class]: 'magenta',
};

const priorityLabels: Record<AnnouncementPriority, string> = {
  [AnnouncementPriority.Low]: 'Low',
  [AnnouncementPriority.Normal]: 'Normal',
  [AnnouncementPriority.High]: 'High',
  [AnnouncementPriority.Urgent]: 'Urgent',
};

const typeMap: Record<number, { label: string; color: string }> = Object.fromEntries(
  Object.entries(announcementTypeLabels).map(([k, label]) => [
    k,
    { label, color: typeColors[Number(k) as AnnouncementType] },
  ])
);

const priorityMap: Record<number, { label: string; color: string }> = Object.fromEntries(
  Object.entries(priorityLabels).map(([k, label]) => [
    k,
    { label, color: priorityColors[Number(k) as AnnouncementPriority] },
  ])
);

const audienceMap: Record<number, { label: string; color: string }> = Object.fromEntries(
  Object.entries(announcementAudienceLabels).map(([k, label]) => [
    k,
    { label, color: audienceColors[Number(k) as AnnouncementAudience] },
  ])
);

function AnnouncementsContent() {
  const { announcements, totalCount, isPending, isError } = useAnnouncementState();
  const { getAllAsync, deleteAsync, publishAsync, unpublishAsync, pinAsync, unpinAsync } = useAnnouncementActions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<IAnnouncementList | null>(null);
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);

  const handleQueryChange = useCallback((query: TableQuery) => {
    setLastQuery(query);
    const { keyword, ...columnFilters } = query.filters ?? {};
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      search: keyword as string | undefined,
      ...columnFilters,
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

  const columns: ColumnConfig<IAnnouncementList>[] = [
    { key: 'title', title: 'Title', dataIndex: 'title', sortable: true },
    {
      key: 'type', title: 'Type', dataIndex: 'type', width: 100,
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(announcementTypeLabels).map(([value, label]) => ({
        label,
        value: Number(value),
      })),
      renderType: 'status',
      renderConfig: { statusMap: typeMap },
    },
    {
      key: 'priority', title: 'Priority', dataIndex: 'priority', width: 90,
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(priorityLabels).map(([value, label]) => ({
        label,
        value: Number(value),
      })),
      renderType: 'status',
      renderConfig: { statusMap: priorityMap },
    },
    {
      key: 'targetAudience', title: 'Audience', dataIndex: 'targetAudience', width: 100,
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(announcementAudienceLabels).map(([value, label]) => ({
        label,
        value: Number(value),
      })),
      renderType: 'status',
      renderConfig: { statusMap: audienceMap },
    },
    { key: 'publishDate', title: 'Publish Date', dataIndex: 'publishDate', sortable: true, renderType: 'date', width: 110 },
    { key: 'expiryDate', title: 'Expiry', dataIndex: 'expiryDate', sortable: true, renderType: 'date', hideOnMobile: true, width: 110 },
    { key: 'readCount', title: 'Reads', dataIndex: 'readCount', sortable: true, hideOnMobile: true, width: 70 },
    {
      key: 'isPinned', title: 'Pinned', dataIndex: 'isPinned', width: 80,
      renderType: 'status',
      renderConfig: {
        statusMap: {
          true: { label: 'Pinned', color: 'orange' },
          false: { label: '-', color: 'default' },
        },
      },
    },
    {
      key: 'isPublished', title: 'Status', dataIndex: 'isPublished',
      renderType: 'status',
      renderConfig: {
        statusMap: {
          true: { label: 'Published', color: 'green' },
          false: { label: 'Draft', color: 'default' },
        },
      },
    },
  ];

  const toolbarActions: ToolbarAction[] = [
    {
      key: 'new',
      label: 'New Announcement',
      icon: <PlusOutlined />,
      type: 'primary',
      onClick: () => { setEditRecord(null); setModalOpen(true); },
    },
  ];

  const rowActions: RowAction<IAnnouncementList>[] = [
    {
      key: 'edit',
      label: 'Edit',
      icon: <EditOutlined />,
      onClick: (record) => { setEditRecord(record); setModalOpen(true); },
    },
    {
      key: 'publish',
      label: 'Publish',
      icon: <SendOutlined />,
      visible: (record) => !record.isPublished,
      confirm: { title: 'Publish this announcement?', description: 'It will become visible to the target audience.' },
      onClick: async (record) => {
        try {
          await publishAsync(record.id);
          message.success('Announcement published');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
    {
      key: 'unpublish',
      label: 'Unpublish',
      icon: <StopOutlined />,
      visible: (record) => record.isPublished,
      confirm: { title: 'Unpublish this announcement?', description: 'It will no longer be visible.' },
      onClick: async (record) => {
        try {
          await unpublishAsync(record.id);
          message.success('Announcement unpublished');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
    {
      key: 'pin',
      label: 'Pin',
      icon: <PushpinOutlined />,
      visible: (record) => !record.isPinned,
      onClick: async (record) => {
        try {
          await pinAsync(record.id);
          message.success('Announcement pinned');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
    {
      key: 'unpin',
      label: 'Unpin',
      icon: <PushpinOutlined />,
      visible: (record) => record.isPinned,
      onClick: async (record) => {
        try {
          await unpinAsync(record.id);
          message.success('Announcement unpinned');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
    {
      key: 'delete',
      label: 'Delete',
      icon: <DeleteOutlined />,
      danger: true,
      confirm: { title: 'Delete this announcement?', description: 'This action cannot be undone.' },
      onClick: async (record) => {
        try {
          await deleteAsync(record.id);
          message.success('Announcement deleted');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<IAnnouncementList>
        title="Announcements"
        columns={columns}
        data={announcements ?? []}
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
      <AnnouncementFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
      />
    </>
  );
}

export default function AnnouncementsPageContent() {
  return (
    <AnnouncementProvider>
      <AnnouncementsContent />
    </AnnouncementProvider>
  );
}
