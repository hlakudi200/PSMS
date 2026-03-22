'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import { PlusOutlined, EditOutlined, SendOutlined, StopOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { StudentTransferProvider, useStudentTransferState, useStudentTransferActions } from '@/providers/academic/student-transfers';
import { useAuthState } from '@/providers/auth';
import { StudentTransferFormModal } from '@/components/modals/academic/StudentTransferFormModal';
import type { IStudentTransfer } from '@/providers/academic/student-transfers/context';

const TransferTypeLabels: Record<number, string> = {
  1: 'Transfer In',
  2: 'Transfer Out',
};

const StatusLabels: Record<number, string> = {
  1: 'Draft', 2: 'Submitted', 3: 'Under Review', 4: 'Approved',
  5: 'Rejected', 6: 'Completed', 7: 'Cancelled',
};

const StatusColors: Record<number, string> = {
  1: 'default', 2: 'processing', 3: 'warning', 4: 'success',
  5: 'error', 6: 'success', 7: 'default',
};

function StudentTransfersContent() {
  const { studentTransfers, totalCount, isPending, isError } = useStudentTransferState();
  const { getAllAsync, submitAsync, cancelAsync } = useStudentTransferActions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<IStudentTransfer | null>(null);
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

  const refreshData = useCallback(() => {
    if (lastQuery) handleQueryChange(lastQuery);
  }, [lastQuery, handleQueryChange]);

  const handleModalClose = (refresh?: boolean) => {
    setModalOpen(false);
    setEditRecord(null);
    if (refresh) refreshData();
  };

  const columns: ColumnConfig<IStudentTransfer>[] = [
    { key: 'transferNumber', title: 'Transfer #', dataIndex: 'transferNumber', sortable: true, width: 130 },
    { key: 'studentName', title: 'Student', dataIndex: 'studentName', sortable: true },
    {
      key: 'transferType', title: 'Type', dataIndex: 'transferType',
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(TransferTypeLabels).map(([v, l]) => ({
        label: l, value: Number(v),
      })),
      render: (value: number) => TransferTypeLabels[value] ?? value,
    },
    {
      key: 'status', title: 'Status', dataIndex: 'status',
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(StatusLabels).map(([v, l]) => ({
        label: l, value: Number(v),
      })),
      renderType: 'status',
      renderConfig: {
        statusMap: Object.fromEntries(
          Object.entries(StatusLabels).map(([k, label]) => [
            k, { label, color: StatusColors[Number(k)] },
          ])
        ),
      },
    },
    {
      key: 'requestedDate', title: 'Requested', dataIndex: 'requestedDate',
      sortable: true, renderType: 'date',
    },
    {
      key: 'effectiveDate', title: 'Effective Date', dataIndex: 'effectiveDate',
      renderType: 'date',
    },
    { key: 'fromSchoolName', title: 'From School', dataIndex: 'fromSchoolName' },
    { key: 'toSchoolName', title: 'To School', dataIndex: 'toSchoolName' },
  ];

  const toolbarActions: ToolbarAction[] = [
    {
      key: 'new',
      label: 'New Transfer',
      icon: <PlusOutlined />,
      type: 'primary',
      onClick: () => { setEditRecord(null); setModalOpen(true); },
      requiredPermissions: ['Admin', 'Principal'],
    },
  ];

  const rowActions: RowAction<IStudentTransfer>[] = [
    {
      key: 'edit',
      label: 'Edit',
      icon: <EditOutlined />,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => record.status === 1,
      onClick: (record) => { setEditRecord(record); setModalOpen(true); },
    },
    {
      key: 'submit',
      label: 'Submit',
      icon: <SendOutlined />,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => record.status === 1,
      confirm: { title: 'Submit this transfer?', description: 'The transfer request will be sent for review.' },
      onClick: async (record) => {
        await submitAsync(record.id);
        message.success('Transfer submitted');
        refreshData();
      },
    },
    {
      key: 'cancel',
      label: 'Cancel',
      icon: <StopOutlined />,
      danger: true,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => [1, 2, 3].includes(record.status),
      confirm: { title: 'Cancel this transfer?', description: 'This action cannot be undone.' },
      onClick: async (record) => {
        await cancelAsync(record.id);
        message.success('Transfer cancelled');
        refreshData();
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<IStudentTransfer>
        title="Student Transfers"
        columns={columns}
        data={studentTransfers ?? []}
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
          requiredPermissions: ['Admin', 'Principal'],
        }}
      />
      <StudentTransferFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
      />
    </>
  );
}

export default function StudentTransfersPageContent() {
  return (
    <StudentTransferProvider>
      <StudentTransfersContent />
    </StudentTransferProvider>
  );
}
