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
  1: { label: 'General', color: 'default' },
  2: { label: 'Academic', color: 'blue' },
  3: { label: 'Sports', color: 'green' },
  4: { label: 'Event', color: 'purple' },
  5: { label: 'Emergency', color: 'red' },
  6: { label: 'Holiday', color: 'orange' },
  7: { label: 'Administrative', color: 'cyan' },
};

const priorityMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Low', color: 'default' },
  2: { label: 'Normal', color: 'blue' },
  3: { label: 'High', color: 'orange' },
  4: { label: 'Urgent', color: 'red' },
};

const audienceMap: Record<number, { label: string; color: string }> = {
  1: { label: 'All', color: 'green' },
  2: { label: 'Staff', color: 'geekblue' },
  3: { label: 'Teachers', color: 'blue' },
  4: { label: 'Parents', color: 'purple' },
  5: { label: 'Students', color: 'orange' },
  6: { label: 'Grade', color: 'cyan' },
  7: { label: 'Class', color: 'magenta' },
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
        { label: 'General', value: 1 },
        { label: 'Academic', value: 2 },
        { label: 'Sports', value: 3 },
        { label: 'Event', value: 4 },
        { label: 'Emergency', value: 5 },
        { label: 'Holiday', value: 6 },
        { label: 'Administrative', value: 7 },
      ],
      renderType: 'status',
      renderConfig: { statusMap: typeMap },
    },
    {
      key: 'priority', title: 'Priority', dataIndex: 'priority', width: 90,
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Low', value: 1 },
        { label: 'Normal', value: 2 },
        { label: 'High', value: 3 },
        { label: 'Urgent', value: 4 },
      ],
      renderType: 'status',
      renderConfig: { statusMap: priorityMap },
    },
    {
      key: 'targetAudience', title: 'Audience', dataIndex: 'targetAudience', width: 100,
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'All', value: 1 },
        { label: 'Staff', value: 2 },
        { label: 'Teachers', value: 3 },
        { label: 'Parents', value: 4 },
        { label: 'Students', value: 5 },
        { label: 'Grade', value: 6 },
        { label: 'Class', value: 7 },
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
