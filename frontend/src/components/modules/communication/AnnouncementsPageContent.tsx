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

const typeMap: Record<number, { label: string; color: string }> = {
  0: { label: 'General', color: 'default' },
  1: { label: 'Academic', color: 'blue' },
  2: { label: 'Event', color: 'purple' },
  3: { label: 'Emergency', color: 'red' },
};

const priorityMap: Record<number, { label: string; color: string }> = {
  0: { label: 'Low', color: 'default' },
  1: { label: 'Normal', color: 'blue' },
  2: { label: 'High', color: 'red' },
};

const audienceMap: Record<number, { label: string; color: string }> = {
  0: { label: 'All', color: 'green' },
  1: { label: 'Teachers', color: 'blue' },
  2: { label: 'Parents', color: 'purple' },
  3: { label: 'Students', color: 'orange' },
  4: { label: 'Specific', color: 'cyan' },
};

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
      filterOptions: [
        { label: 'General', value: 0 },
        { label: 'Academic', value: 1 },
        { label: 'Event', value: 2 },
        { label: 'Emergency', value: 3 },
      ],
      renderType: 'status',
      renderConfig: { statusMap: typeMap },
    },
    {
      key: 'priority', title: 'Priority', dataIndex: 'priority', width: 90,
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Low', value: 0 },
        { label: 'Normal', value: 1 },
        { label: 'High', value: 2 },
      ],
      renderType: 'status',
      renderConfig: { statusMap: priorityMap },
    },
    {
      key: 'targetAudience', title: 'Audience', dataIndex: 'targetAudience', width: 100,
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'All', value: 0 },
        { label: 'Teachers', value: 1 },
        { label: 'Parents', value: 2 },
        { label: 'Students', value: 3 },
        { label: 'Specific', value: 4 },
      ],
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
        await publishAsync(record.id);
        message.success('Announcement published');
        refreshData();
      },
    },
    {
      key: 'unpublish',
      label: 'Unpublish',
      icon: <StopOutlined />,
      visible: (record) => record.isPublished,
      confirm: { title: 'Unpublish this announcement?', description: 'It will no longer be visible.' },
      onClick: async (record) => {
        await unpublishAsync(record.id);
        message.success('Announcement unpublished');
        refreshData();
      },
    },
    {
      key: 'pin',
      label: 'Pin',
      icon: <PushpinOutlined />,
      visible: (record) => !record.isPinned,
      onClick: async (record) => {
        await pinAsync(record.id);
        message.success('Announcement pinned');
        refreshData();
      },
    },
    {
      key: 'unpin',
      label: 'Unpin',
      icon: <PushpinOutlined />,
      visible: (record) => record.isPinned,
      onClick: async (record) => {
        await unpinAsync(record.id);
        message.success('Announcement unpinned');
        refreshData();
      },
    },
    {
      key: 'delete',
      label: 'Delete',
      icon: <DeleteOutlined />,
      danger: true,
      confirm: { title: 'Delete this announcement?', description: 'This action cannot be undone.' },
      onClick: async (record) => {
        await deleteAsync(record.id);
        message.success('Announcement deleted');
        refreshData();
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
