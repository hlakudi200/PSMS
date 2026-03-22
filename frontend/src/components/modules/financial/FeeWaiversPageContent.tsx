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

const WaiverTypeLabels: Record<number, string> = {
  1: 'Financial Hardship',
  2: 'Sibling Discount',
  3: 'Staff Discount',
  4: 'Bursary',
  5: 'Scholarship',
  6: 'Other',
};

const StatusLabels: Record<number, string> = {
  1: 'Draft',
  2: 'Submitted',
  3: 'Under Review',
  4: 'Approved',
  5: 'Rejected',
  6: 'Cancelled',
};

const StatusColors: Record<number, string> = {
  1: 'default',
  2: 'processing',
  3: 'warning',
  4: 'success',
  5: 'error',
  6: 'default',
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
      render: (value: number) => `R ${value?.toFixed(2) ?? '0.00'}`,
    },
    {
      key: 'approvedAmount', title: 'Approved Amount', dataIndex: 'approvedAmount',
      render: (value: number) => value != null ? `R ${value.toFixed(2)}` : '-',
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
      visible: (record) => record.status === 1,
      onClick: (record) => { setEditRecord(record); setModalOpen(true); },
    },
    {
      key: 'submit',
      label: 'Submit',
      icon: <SendOutlined />,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => record.status === 1,
      confirm: { title: 'Submit this fee waiver?', description: 'Once submitted, it will be sent for review.' },
      onClick: async (record) => {
        await submitAsync(record.id);
        message.success('Fee waiver submitted');
        refreshData();
      },
    },
    {
      key: 'delete',
      label: 'Delete',
      icon: <DeleteOutlined />,
      danger: true,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => record.status === 1,
      confirm: { title: 'Delete this fee waiver?', description: 'This cannot be undone.' },
      onClick: async (record) => {
        await deleteAsync(record.id);
        message.success('Fee waiver deleted');
        refreshData();
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
