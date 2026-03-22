'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import { PlusOutlined, EditOutlined, SendOutlined, StopOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { StaffLeaveRequestProvider, useStaffLeaveRequestState, useStaffLeaveRequestActions } from '@/providers/hr/staff-leave';
import { useAuthState } from '@/providers/auth';
import { StaffLeaveFormModal } from '@/components/modals/hr/StaffLeaveFormModal';
import type { IStaffLeaveRequest } from '@/providers/hr/staff-leave/context';

const LeaveTypeLabels: Record<number, string> = {
  1: 'Annual', 2: 'Sick', 3: 'Family', 4: 'Maternity',
  5: 'Paternity', 6: 'Study', 7: 'Compassionate', 8: 'Unpaid', 9: 'Other',
};

const StatusLabels: Record<number, string> = {
  1: 'Draft', 2: 'Submitted', 3: 'HOD Approved', 4: 'Approved',
  5: 'Rejected', 6: 'Cancelled',
};

const StatusColors: Record<number, string> = {
  1: 'default', 2: 'processing', 3: 'warning', 4: 'success',
  5: 'error', 6: 'default',
};

function StaffLeaveContent() {
  const { staffLeaveRequests, totalCount, isPending, isError } = useStaffLeaveRequestState();
  const { getAllAsync, submitAsync, cancelAsync } = useStaffLeaveRequestActions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<IStaffLeaveRequest | null>(null);
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

  const columns: ColumnConfig<IStaffLeaveRequest>[] = [
    { key: 'leaveNumber', title: 'Leave #', dataIndex: 'leaveNumber', sortable: true, width: 120 },
    { key: 'userName', title: 'Staff Member', dataIndex: 'userName', sortable: true },
    {
      key: 'leaveType', title: 'Leave Type', dataIndex: 'leaveType',
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(LeaveTypeLabels).map(([v, l]) => ({
        label: l, value: Number(v),
      })),
      render: (value: number) => LeaveTypeLabels[value] ?? value,
    },
    {
      key: 'startDate', title: 'Start Date', dataIndex: 'startDate',
      sortable: true, renderType: 'date',
    },
    {
      key: 'endDate', title: 'End Date', dataIndex: 'endDate',
      sortable: true, renderType: 'date',
    },
    { key: 'totalDays', title: 'Days', dataIndex: 'totalDays', sortable: true, width: 80 },
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
  ];

  const toolbarActions: ToolbarAction[] = [
    {
      key: 'new',
      label: 'New Leave Request',
      icon: <PlusOutlined />,
      type: 'primary',
      onClick: () => { setEditRecord(null); setModalOpen(true); },
      requiredPermissions: ['Admin', 'Principal'],
    },
  ];

  const rowActions: RowAction<IStaffLeaveRequest>[] = [
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
      confirm: { title: 'Submit this leave request?', description: 'The request will be sent for approval.' },
      onClick: async (record) => {
        await submitAsync(record.id);
        message.success('Leave request submitted');
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
      confirm: { title: 'Cancel this leave request?', description: 'This action cannot be undone.' },
      onClick: async (record) => {
        await cancelAsync(record.id);
        message.success('Leave request cancelled');
        refreshData();
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<IStaffLeaveRequest>
        title="Staff Leave Requests"
        columns={columns}
        data={staffLeaveRequests ?? []}
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
      <StaffLeaveFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
      />
    </>
  );
}

export default function StaffLeavePageContent() {
  return (
    <StaffLeaveRequestProvider>
      <StaffLeaveContent />
    </StaffLeaveRequestProvider>
  );
}
