'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import { PlusOutlined, EditOutlined, EyeOutlined, SendOutlined, StopOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { DisciplinaryCaseProvider, useDisciplinaryCaseState, useDisciplinaryCaseActions } from '@/providers/discipline/disciplinary-cases';
import { useAuthState } from '@/providers/auth';
import { DisciplinaryCaseFormModal } from '@/components/modals/discipline/DisciplinaryCaseFormModal';
import type { IDisciplinaryCase } from '@/providers/discipline/disciplinary-cases/context';

const CategoryLabels: Record<number, string> = {
  1: 'Misconduct', 2: 'Bullying', 3: 'Violence', 4: 'Substance Abuse',
  5: 'Property Damage', 6: 'Truancy', 7: 'Academic Dishonesty',
  8: 'Harassment', 9: 'Dress Code', 10: 'Other',
};

const SeverityLabels: Record<number, string> = {
  1: 'Minor', 2: 'Moderate', 3: 'Serious', 4: 'Very Serious',
};

const StatusLabels: Record<number, string> = {
  1: 'Draft', 2: 'Reported', 3: 'Under Investigation', 4: 'Hearing Scheduled',
  5: 'Hearing Completed', 6: 'Resolved', 7: 'Appealed', 8: 'Cancelled',
};

const StatusColors: Record<number, string> = {
  1: 'default', 2: 'processing', 3: 'warning', 4: 'processing',
  5: 'warning', 6: 'success', 7: 'error', 8: 'default',
};

function DisciplinaryCasesContent() {
  const { disciplinaryCases, totalCount, isPending, isError } = useDisciplinaryCaseState();
  const { getAllAsync, submitAsync, cancelAsync } = useDisciplinaryCaseActions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<IDisciplinaryCase | null>(null);
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

  const columns: ColumnConfig<IDisciplinaryCase>[] = [
    { key: 'caseNumber', title: 'Case #', dataIndex: 'caseNumber', sortable: true, width: 120 },
    { key: 'studentName', title: 'Student', dataIndex: 'studentName', sortable: true },
    {
      key: 'incidentDate', title: 'Incident Date', dataIndex: 'incidentDate',
      sortable: true, renderType: 'date',
    },
    {
      key: 'incidentCategory', title: 'Category', dataIndex: 'incidentCategory',
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(CategoryLabels).map(([v, l]) => ({
        label: l, value: Number(v),
      })),
      render: (value: number) => CategoryLabels[value] ?? value,
    },
    {
      key: 'severity', title: 'Severity', dataIndex: 'severity',
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(SeverityLabels).map(([v, l]) => ({
        label: l, value: Number(v),
      })),
      render: (value: number) => SeverityLabels[value] ?? value,
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
    { key: 'reportedByName', title: 'Reported By', dataIndex: 'reportedByName' },
    {
      key: 'creationTime', title: 'Created', dataIndex: 'creationTime',
      sortable: true, renderType: 'date',
    },
  ];

  const toolbarActions: ToolbarAction[] = [
    {
      key: 'new',
      label: 'New Case',
      icon: <PlusOutlined />,
      type: 'primary',
      onClick: () => { setEditRecord(null); setModalOpen(true); },
      requiredPermissions: ['Admin', 'Principal'],
    },
  ];

  const rowActions: RowAction<IDisciplinaryCase>[] = [
    {
      key: 'view',
      label: 'View Details',
      icon: <EyeOutlined />,
      onClick: () => { /* navigate to detail page if needed */ },
    },
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
      confirm: { title: 'Submit this case?', description: 'The case will be formally reported.' },
      onClick: async (record) => {
        await submitAsync(record.id);
        message.success('Case submitted');
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
      confirm: { title: 'Cancel this case?', description: 'This action cannot be undone.' },
      onClick: async (record) => {
        await cancelAsync(record.id);
        message.success('Case cancelled');
        refreshData();
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<IDisciplinaryCase>
        title="Disciplinary Cases"
        columns={columns}
        data={disciplinaryCases ?? []}
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
      <DisciplinaryCaseFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
      />
    </>
  );
}

export default function DisciplinaryCasesPageContent() {
  return (
    <DisciplinaryCaseProvider>
      <DisciplinaryCasesContent />
    </DisciplinaryCaseProvider>
  );
}
