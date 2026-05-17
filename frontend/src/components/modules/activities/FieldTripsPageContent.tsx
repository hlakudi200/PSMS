'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import { PlusOutlined, EditOutlined, SendOutlined, StopOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { FieldTripProvider, useFieldTripState, useFieldTripActions } from '@/providers/activities/field-trips';
import { useAuthState } from '@/providers/auth';
import { FieldTripFormModal } from '@/components/modals/activities/FieldTripFormModal';
import type { IFieldTrip } from '@/providers/activities/field-trips/context';
import { FieldTripStatus, fieldTripStatusLabels } from '@/providers/shared/enums';
import { formatZAR } from '@/utils/currency';

const StatusLabels = fieldTripStatusLabels;

const StatusColors: Record<number, string> = {
  [FieldTripStatus.Draft]: 'default',
  [FieldTripStatus.Submitted]: 'processing',
  [FieldTripStatus.UnderReview]: 'warning',
  [FieldTripStatus.Approved]: 'success',
  [FieldTripStatus.Rejected]: 'error',
  [FieldTripStatus.Completed]: 'success',
  [FieldTripStatus.Cancelled]: 'default',
};

function FieldTripsContent() {
  const { fieldTrips, totalCount, isPending, isError } = useFieldTripState();
  const { getAllAsync, submitAsync } = useFieldTripActions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<IFieldTrip | null>(null);
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

  const columns: ColumnConfig<IFieldTrip>[] = [
    { key: 'tripName', title: 'Trip Name', dataIndex: 'tripName', sortable: true },
    { key: 'destination', title: 'Destination', dataIndex: 'destination', sortable: true },
    {
      key: 'tripDate', title: 'Trip Date', dataIndex: 'tripDate',
      sortable: true, renderType: 'date',
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
      key: 'estimatedCost', title: 'Est. Cost', dataIndex: 'estimatedCost',
      sortable: true,
      render: (value: number) => formatZAR(value),
    },
    {
      key: 'approvedBudget', title: 'Approved Budget', dataIndex: 'approvedBudget',
      render: (value: number) => formatZAR(value),
    },
    { key: 'numberOfStudents', title: 'Students', dataIndex: 'numberOfStudents', width: 90 },
    { key: 'organizingTeacherName', title: 'Organizer', dataIndex: 'organizingTeacherName' },
  ];

  const toolbarActions: ToolbarAction[] = [
    {
      key: 'new',
      label: 'New Field Trip',
      icon: <PlusOutlined />,
      type: 'primary',
      onClick: () => { setEditRecord(null); setModalOpen(true); },
      requiredPermissions: ['Admin', 'Principal'],
    },
  ];

  const rowActions: RowAction<IFieldTrip>[] = [
    {
      key: 'edit',
      label: 'Edit',
      icon: <EditOutlined />,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => record.status === FieldTripStatus.Draft,
      onClick: (record) => { setEditRecord(record); setModalOpen(true); },
    },
    {
      key: 'submit',
      label: 'Submit',
      icon: <SendOutlined />,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => record.status === FieldTripStatus.Draft,
      confirm: { title: 'Submit this field trip?', description: 'The trip will be sent for approval.' },
      onClick: async (record) => {
        try {
          await submitAsync(record.id);
          message.success('Field trip submitted');
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
        [
          FieldTripStatus.Draft,
          FieldTripStatus.Submitted,
          FieldTripStatus.UnderReview,
          FieldTripStatus.Approved,
        ].includes(record.status),
      confirm: { title: 'Cancel this field trip?', description: 'This action cannot be undone.' },
      onClick: async () => {
        // Cancel requires a reason; route through the detail view.
        message.info('Use the detail view to cancel with a reason');
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<IFieldTrip>
        title="Field Trips"
        columns={columns}
        data={fieldTrips ?? []}
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
      <FieldTripFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
      />
    </>
  );
}

export default function FieldTripsPageContent() {
  return (
    <FieldTripProvider>
      <FieldTripsContent />
    </FieldTripProvider>
  );
}
