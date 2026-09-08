'use client';

import React, { useState, useCallback, useEffect } from 'react';
import { message, Typography } from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  CheckCircleOutlined,
  StopOutlined,
} from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { DocumentProvider, useDocumentState, useDocumentActions } from '@/providers/communication/documents';
import { AcademicYearProvider, useAcademicYearState, useAcademicYearActions } from '@/providers/academic/academic_years';
import { useAuthState } from '@/providers/auth';
import {
  DocumentFormModal,
  DOCUMENT_TYPE_OPTIONS,
  DOCUMENT_AUDIENCE_OPTIONS,
} from '@/components/modals/communication/DocumentFormModal';
import type { IDocumentList } from '@/providers/communication/shared/interfaces';
import { formatBytes } from '@/utils/format-bytes';

const { Text } = Typography;

const MANAGE_ROLES = ['Admin', 'Principal', 'VicePrincipal'];

const TYPE_COLORS: Record<number, string> = {
  1: 'red', 2: 'blue', 3: 'green', 4: 'gold', 5: 'purple', 6: 'cyan', 7: 'geekblue', 8: 'default',
};
const AUDIENCE_COLORS: Record<number, string> = {
  1: 'green', 2: 'blue', 3: 'geekblue', 4: 'orange', 5: 'purple', 6: 'default',
};

const typeStatusMap = Object.fromEntries(
  DOCUMENT_TYPE_OPTIONS.map((o) => [o.value, { label: o.label, color: TYPE_COLORS[o.value] ?? 'default' }])
);
const audienceStatusMap = Object.fromEntries(
  DOCUMENT_AUDIENCE_OPTIONS.map((o) => [o.value, { label: o.label, color: AUDIENCE_COLORS[o.value] ?? 'default' }])
);

/**
 * Shared school documents (policies, forms, newsletters, handbooks …) that are
 * published to a target audience. Backed by the Communication.Documents
 * permission group, which management already held but had no page for.
 */
function DocumentsContent() {
  const { documents, totalCount, isPending, isError } = useDocumentState();
  const { getAllAsync, deleteAsync, publishAsync, unpublishAsync } = useDocumentActions();
  const { academicYears } = useAcademicYearState();
  const { getAllAsync: getAcademicYears } = useAcademicYearActions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<IDocumentList | null>(null);
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);

  useEffect(() => {
    getAcademicYears({ maxResultCount: 50 });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleQueryChange = useCallback((query: TableQuery) => {
    setLastQuery(query);
    const { keyword, ...columnFilters } = query.filters ?? {};
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      search: keyword as string | undefined,
      documentType: columnFilters.documentType as number | undefined,
      targetAudience: columnFilters.targetAudience as number | undefined,
      isPublished: columnFilters.isPublished as boolean | undefined,
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

  const columns: ColumnConfig<IDocumentList>[] = [
    { key: 'title', title: 'Title', dataIndex: 'title', sortable: true },
    {
      key: 'documentType', title: 'Type', dataIndex: 'documentType', width: 130,
      filterable: true, filterType: 'enum', filterOptions: DOCUMENT_TYPE_OPTIONS,
      renderType: 'status', renderConfig: { statusMap: typeStatusMap },
    },
    {
      key: 'targetAudience', title: 'Audience', dataIndex: 'targetAudience', width: 120,
      filterable: true, filterType: 'enum', filterOptions: DOCUMENT_AUDIENCE_OPTIONS,
      renderType: 'status', renderConfig: { statusMap: audienceStatusMap },
    },
    { key: 'category', title: 'Category', dataIndex: 'category', hideOnMobile: true },
    {
      key: 'fileName', title: 'File', dataIndex: 'fileName', hideOnMobile: true,
      render: (_: unknown, record: IDocumentList) => (
        <span>
          {record.fileName}
          <Text type="secondary"> ({formatBytes(record.fileSizeBytes)})</Text>
        </span>
      ),
    },
    {
      key: 'isPublished', title: 'Status', dataIndex: 'isPublished', width: 120,
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Published', value: true },
        { label: 'Draft', value: false },
      ],
      renderType: 'status',
      renderConfig: {
        statusMap: {
          true: { label: 'Published', color: 'green' },
          false: { label: 'Draft', color: 'default' },
        },
      },
    },
    {
      key: 'publishedDate', title: 'Published', dataIndex: 'publishedDate', width: 120,
      sortable: true, renderType: 'date', hideOnMobile: true,
    },
    { key: 'downloadCount', title: 'Downloads', dataIndex: 'downloadCount', sortable: true, width: 100 },
  ];

  const toolbarActions: ToolbarAction[] = [
    {
      key: 'new',
      label: 'New Document',
      icon: <PlusOutlined />,
      type: 'primary',
      requiredPermissions: MANAGE_ROLES,
      onClick: () => { setEditRecord(null); setModalOpen(true); },
    },
  ];

  const rowActions: RowAction<IDocumentList>[] = [
    {
      key: 'edit',
      label: 'Edit',
      icon: <EditOutlined />,
      requiredPermissions: MANAGE_ROLES,
      onClick: (record) => { setEditRecord(record); setModalOpen(true); },
    },
    {
      key: 'publish',
      label: 'Publish',
      icon: <CheckCircleOutlined />,
      requiredPermissions: MANAGE_ROLES,
      visible: (record) => !record.isPublished,
      confirm: { title: 'Publish this document?', description: 'It becomes visible to its target audience.' },
      onClick: async (record) => {
        try {
          await publishAsync(record.id);
          message.success('Document published');
          refreshData();
        } catch {
          // handled by interceptor
        }
      },
    },
    {
      key: 'unpublish',
      label: 'Unpublish',
      icon: <StopOutlined />,
      requiredPermissions: MANAGE_ROLES,
      visible: (record) => record.isPublished,
      confirm: { title: 'Unpublish this document?', description: 'It will no longer be visible to its audience.' },
      onClick: async (record) => {
        try {
          await unpublishAsync(record.id);
          message.success('Document unpublished');
          refreshData();
        } catch {
          // handled by interceptor
        }
      },
    },
    {
      key: 'delete',
      label: 'Delete',
      icon: <DeleteOutlined />,
      danger: true,
      // Communication.Documents.Delete is not in the Vice Principal grant set.
      requiredPermissions: ['Admin', 'Principal'],
      confirm: { title: 'Delete this document?', description: 'This action cannot be undone.' },
      onClick: async (record) => {
        try {
          await deleteAsync(record.id);
          message.success('Document deleted');
          refreshData();
        } catch {
          // handled by interceptor
        }
      },
    },
  ];

  return (
    <>
      <EnterpriseTable<IDocumentList>
        title="School Documents"
        columns={columns}
        data={documents ?? []}
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
          requiredPermissions: MANAGE_ROLES,
        }}
      />
      <DocumentFormModal
        open={modalOpen}
        onClose={handleModalClose}
        editRecord={editRecord}
        academicYears={(academicYears ?? []).map((y) => ({ value: y.id, label: y.yearName }))}
      />
    </>
  );
}

export default function DocumentsPageContent() {
  return (
    <AcademicYearProvider>
      <DocumentProvider>
        <DocumentsContent />
      </DocumentProvider>
    </AcademicYearProvider>
  );
}
