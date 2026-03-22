'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import { PlusOutlined, EditOutlined, SendOutlined, StopOutlined, DeleteOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { ExpenseRequestProvider, useExpenseRequestState, useExpenseRequestActions } from '@/providers/financial/expense-requests';
import { useAuthState } from '@/providers/auth';
import { ExpenseRequestFormModal } from '@/components/modals/financial/ExpenseRequestFormModal';
import type { IExpenseRequest } from '@/providers/financial/expense-requests/context';

const CategoryLabels: Record<number, string> = {
  1: 'Stationery', 2: 'Textbooks', 3: 'Equipment', 4: 'Maintenance',
  5: 'Technology', 6: 'Sports', 7: 'Cultural', 8: 'Transport',
  9: 'Catering', 10: 'Training', 11: 'Other',
};

const PriorityLabels: Record<number, string> = {
  1: 'Low', 2: 'Medium', 3: 'High', 4: 'Urgent',
};

const PriorityColors: Record<number, string> = {
  1: 'default', 2: 'processing', 3: 'warning', 4: 'error',
};

const StatusLabels: Record<number, string> = {
  1: 'Draft', 2: 'Submitted', 3: 'Under Review', 4: 'Approved',
  5: 'Rejected', 6: 'Paid', 7: 'Cancelled',
};

const StatusColors: Record<number, string> = {
  1: 'default', 2: 'processing', 3: 'warning', 4: 'success',
  5: 'error', 6: 'success', 7: 'default',
};

function ExpenseRequestsContent() {
  const { expenseRequests, totalCount, isPending, isError } = useExpenseRequestState();
  const { getAllAsync, submitAsync, cancelAsync } = useExpenseRequestActions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<IExpenseRequest | null>(null);
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

  const columns: ColumnConfig<IExpenseRequest>[] = [
    { key: 'requestNumber', title: 'Request #', dataIndex: 'requestNumber', sortable: true, width: 130 },
    { key: 'requestedByName', title: 'Requested By', dataIndex: 'requestedByName', sortable: true },
    {
      key: 'category', title: 'Category', dataIndex: 'category',
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(CategoryLabels).map(([v, l]) => ({
        label: l, value: Number(v),
      })),
      render: (value: number) => CategoryLabels[value] ?? value,
    },
    { key: 'description', title: 'Description', dataIndex: 'description' },
    {
      key: 'amount', title: 'Amount', dataIndex: 'amount',
      sortable: true,
      render: (value: number) => `R ${value?.toFixed(2) ?? '0.00'}`,
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
      key: 'priority', title: 'Priority', dataIndex: 'priority',
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(PriorityLabels).map(([v, l]) => ({
        label: l, value: Number(v),
      })),
      renderType: 'status',
      renderConfig: {
        statusMap: Object.fromEntries(
          Object.entries(PriorityLabels).map(([k, label]) => [
            k, { label, color: PriorityColors[Number(k)] },
          ])
        ),
      },
    },
    {
      key: 'requiredByDate', title: 'Required By', dataIndex: 'requiredByDate',
      renderType: 'date',
    },
  ];

  const toolbarActions: ToolbarAction[] = [
    {
      key: 'new',
      label: 'New Expense Request',
      icon: <PlusOutlined />,
      type: 'primary',
      onClick: () => { setEditRecord(null); setModalOpen(true); },
      requiredPermissions: ['Admin', 'Principal'],
    },
  ];

  const rowActions: RowAction<IExpenseRequest>[] = [
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
      confirm: { title: 'Submit this expense request?', description: 'The request will be sent for approval.' },
      onClick: async (record) => {
        await submitAsync(record.id);
        message.success('Expense request submitted');
        refreshData();
      },
    },
    {
      key: 'cancel',
      label: 'Cancel',
      icon: <StopOutlined />,
      danger: true,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => [1, 2].includes(record.status),
      confirm: { title: 'Cancel this expense request?', description: 'This action cannot be undone.' },
      onClick: async (record) => {
        await cancelAsync(record.id);
        message.success('Expense request cancelled');
        refreshData();
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<IExpenseRequest>
        title="Expense Requests"
        columns={columns}
        data={expenseRequests ?? []}
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
      <ExpenseRequestFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
      />
    </>
  );
}

export default function ExpenseRequestsPageContent() {
  return (
    <ExpenseRequestProvider>
      <ExpenseRequestsContent />
    </ExpenseRequestProvider>
  );
}
