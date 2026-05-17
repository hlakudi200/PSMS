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
import {
  LeaveStatus,
  leaveTypeLabels,
  leaveStatusLabels,
} from '@/providers/shared/enums';

const LeaveTypeLabels = leaveTypeLabels;
const StatusLabels = leaveStatusLabels;

const StatusColors: Record<number, string> = {
  [LeaveStatus.Draft]: 'default',
  [LeaveStatus.Submitted]: 'processing',
  [LeaveStatus.HODApproved]: 'warning',
  [LeaveStatus.Approved]: 'success',
  [LeaveStatus.Rejected]: 'error',
  [LeaveStatus.Cancelled]: 'default',
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
      visible: (record) => record.status === LeaveStatus.Draft,
      onClick: (record) => { setEditRecord(record); setModalOpen(true); },
    },
    {
      key: 'submit',
      label: 'Submit',
      icon: <SendOutlined />,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => record.status === LeaveStatus.Draft,
      confirm: { title: 'Submit this leave request?', description: 'The request will be sent for approval.' },
      onClick: async (record) => {
        try {
          await submitAsync(record.id);
          message.success('Leave request submitted');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
    {
      key: 'cancel',
      label: 'Cancel',
      icon: <StopOutlined />,
      danger: true,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) =>
        [LeaveStatus.Draft, LeaveStatus.Submitted].includes(record.status),
      confirm: { title: 'Cancel this leave request?', description: 'This action cannot be undone.' },
      onClick: async (record) => {
        try {
          await cancelAsync(record.id);
          message.success('Leave request cancelled');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
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
