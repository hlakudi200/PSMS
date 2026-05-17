'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, SendOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { FeeWaiverProvider, useFeeWaiverState, useFeeWaiverActions } from '@/providers/financial/fee-waivers';
import { useAuthState } from '@/providers/auth';
import { FeeWaiverFormModal } from '@/components/modals/financial/FeeWaiverFormModal';
import type { IFeeWaiver } from '@/providers/financial/fee-waivers/context';
import {
  FeeWaiverStatus,
  feeWaiverTypeLabels,
  feeWaiverStatusLabels,
} from '@/providers/shared/enums';
import { formatZAR } from '@/utils/currency';

const WaiverTypeLabels = feeWaiverTypeLabels;
const StatusLabels = feeWaiverStatusLabels;

const StatusColors: Record<number, string> = {
  [FeeWaiverStatus.Draft]: 'default',
  [FeeWaiverStatus.Submitted]: 'processing',
  [FeeWaiverStatus.UnderReview]: 'warning',
  [FeeWaiverStatus.Approved]: 'success',
  [FeeWaiverStatus.Rejected]: 'error',
  [FeeWaiverStatus.Cancelled]: 'default',
};

function FeeWaiversContent() {
  const { feeWaivers, totalCount, isPending, isError } = useFeeWaiverState();
  const { getAllAsync, deleteAsync, submitAsync } = useFeeWaiverActions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<IFeeWaiver | null>(null);
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

  const columns: ColumnConfig<IFeeWaiver>[] = [
    { key: 'studentName', title: 'Student', dataIndex: 'studentName', sortable: true },
    {
      key: 'waiverType', title: 'Waiver Type', dataIndex: 'waiverType',
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(WaiverTypeLabels).map(([v, l]) => ({
        label: l, value: Number(v),
      })),
      render: (value: number) => WaiverTypeLabels[value] ?? value,
    },
    {
      key: 'requestedAmount', title: 'Requested Amount', dataIndex: 'requestedAmount',
      sortable: true,
      render: (value: number) => formatZAR(value),
    },
    {
      key: 'approvedAmount', title: 'Approved Amount', dataIndex: 'approvedAmount',
      render: (value: number) => formatZAR(value),
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
      key: 'creationTime', title: 'Created', dataIndex: 'creationTime',
      sortable: true, renderType: 'date',
    },
  ];

  const toolbarActions: ToolbarAction[] = [
    {
      key: 'new',
      label: 'New Fee Waiver',
      icon: <PlusOutlined />,
      type: 'primary',
      onClick: () => { setEditRecord(null); setModalOpen(true); },
      requiredPermissions: ['Admin', 'Principal'],
    },
  ];

  const rowActions: RowAction<IFeeWaiver>[] = [
    {
      key: 'edit',
      label: 'Edit',
      icon: <EditOutlined />,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => record.status === FeeWaiverStatus.Draft,
      onClick: (record) => { setEditRecord(record); setModalOpen(true); },
    },
    {
      key: 'submit',
      label: 'Submit',
      icon: <SendOutlined />,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => record.status === FeeWaiverStatus.Draft,
      confirm: { title: 'Submit this fee waiver?', description: 'Once submitted, it will be sent for review.' },
      onClick: async (record) => {
        try {
          await submitAsync(record.id);
          message.success('Fee waiver submitted');
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
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => record.status === FeeWaiverStatus.Draft,
      confirm: { title: 'Delete this fee waiver?', description: 'This cannot be undone.' },
      onClick: async (record) => {
        try {
          await deleteAsync(record.id);
          message.success('Fee waiver deleted');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<IFeeWaiver>
        title="Fee Waivers"
        columns={columns}
        data={feeWaivers ?? []}
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
      <FeeWaiverFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
      />
    </>
  );
}

export default function FeeWaiversPageContent() {
  return (
    <FeeWaiverProvider>
      <FeeWaiversContent />
    </FeeWaiverProvider>
  );
}
