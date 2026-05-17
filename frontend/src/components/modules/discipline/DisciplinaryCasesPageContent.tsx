'use client';

import React, { useState, useCallback } from 'react';
import { message } from 'antd';
import { PlusOutlined, EditOutlined, SendOutlined, StopOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { DisciplinaryCaseProvider, useDisciplinaryCaseState, useDisciplinaryCaseActions } from '@/providers/discipline/disciplinary-cases';
import { useAuthState } from '@/providers/auth';
import { DisciplinaryCaseFormModal } from '@/components/modals/discipline/DisciplinaryCaseFormModal';
import type { IDisciplinaryCase } from '@/providers/discipline/disciplinary-cases/context';
import {
  DisciplinaryCaseStatus,
  disciplinaryCaseStatusLabels,
  disciplinaryCategoryLabels,
  disciplinarySeverityLabels,
} from '@/providers/shared/enums';

const CategoryLabels = disciplinaryCategoryLabels;
const SeverityLabels = disciplinarySeverityLabels;
const StatusLabels = disciplinaryCaseStatusLabels;

const StatusColors: Record<number, string> = {
  [DisciplinaryCaseStatus.Draft]: 'default',
  [DisciplinaryCaseStatus.Reported]: 'processing',
  [DisciplinaryCaseStatus.UnderInvestigation]: 'warning',
  [DisciplinaryCaseStatus.HearingScheduled]: 'processing',
  [DisciplinaryCaseStatus.HearingCompleted]: 'warning',
  [DisciplinaryCaseStatus.Resolved]: 'success',
  [DisciplinaryCaseStatus.Appealed]: 'error',
  [DisciplinaryCaseStatus.Cancelled]: 'default',
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

  const cancellableStatuses = new Set<number>([
    DisciplinaryCaseStatus.Draft,
    DisciplinaryCaseStatus.Reported,
    DisciplinaryCaseStatus.UnderInvestigation,
  ]);

  const rowActions: RowAction<IDisciplinaryCase>[] = [
    {
      key: 'edit',
      label: 'Edit',
      icon: <EditOutlined />,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => record.status === DisciplinaryCaseStatus.Draft,
      onClick: (record) => { setEditRecord(record); setModalOpen(true); },
    },
    {
      key: 'submit',
      label: 'Submit',
      icon: <SendOutlined />,
      requiredPermissions: ['Admin', 'Principal'],
      visible: (record) => record.status === DisciplinaryCaseStatus.Draft,
      confirm: { title: 'Submit this case?', description: 'The case will be formally reported.' },
      onClick: async (record) => {
        try {
          await submitAsync(record.id);
          message.success('Case submitted');
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
      visible: (record) => cancellableStatuses.has(record.status),
      confirm: { title: 'Cancel this case?', description: 'This action cannot be undone.' },
      onClick: async (record) => {
        try {
          await cancelAsync(record.id);
          message.success('Case cancelled');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
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
