'use client';

import React, { useState, useCallback, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Card, Col, Row, Select } from 'antd';
import { EyeOutlined } from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction } from '@/components/shared/enterprise-table';
import { AssessmentProvider, useAssessmentState, useAssessmentActions } from '@/providers/assessment/assessments';
import { useAuthState } from '@/providers/auth';
import type { IAssessmentList } from '@/providers/assessment/shared/interfaces';
import { AssessmentType, assessmentTypeLabels } from '@/providers/shared/enums';

const assessmentTypeColors: Record<AssessmentType, string> = {
  [AssessmentType.Test]: 'blue',
  [AssessmentType.Assignment]: 'cyan',
  [AssessmentType.Exam]: 'purple',
  [AssessmentType.Practical]: 'green',
  [AssessmentType.Oral]: 'orange',
  [AssessmentType.Project]: 'magenta',
  [AssessmentType.Other]: 'default',
};

const assessmentTypeMap: Record<number, { label: string; color: string }> = Object.fromEntries(
  Object.entries(assessmentTypeLabels).map(([key, label]) => [
    key,
    { label, color: assessmentTypeColors[Number(key) as AssessmentType] },
  ])
);

function MarkSheetsContent() {
  const router = useRouter();
  const { assessments, totalCount, isPending, isError } = useAssessmentState();
  const { getAllAsync } = useAssessmentActions();
  const { currentRole } = useAuthState();

  const [selectedType, setSelectedType] = useState<number | undefined>(undefined);
  const [selectedPublished, setSelectedPublished] = useState<boolean | undefined>(undefined);
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);

  const handleQueryChange = useCallback((query: TableQuery) => {
    setLastQuery(query);
    const { keyword, ...columnFilters } = query.filters ?? {};
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      assessmentType: selectedType ?? columnFilters.assessmentType as number | undefined,
      isPublished: selectedPublished ?? columnFilters.isPublished as boolean | undefined,
      name: keyword as string | undefined,
      ...columnFilters,
    });
  }, [getAllAsync, selectedType, selectedPublished]);

  const refreshData = useCallback(() => {
    if (lastQuery) handleQueryChange(lastQuery);
  }, [lastQuery, handleQueryChange]);

  // Re-fetch when filters change
  useEffect(() => {
    refreshData();
  }, [selectedType, selectedPublished]);

  const columns: ColumnConfig<IAssessmentList>[] = [
    { key: 'name', title: 'Name', dataIndex: 'name', sortable: true },
    { key: 'subjectName', title: 'Subject', dataIndex: 'subjectName', sortable: true },
    { key: 'className', title: 'Class', dataIndex: 'className', sortable: true },
    { key: 'termName', title: 'Term', dataIndex: 'termName', sortable: true, hideOnMobile: true },
    {
      key: 'assessmentType', title: 'Type', dataIndex: 'assessmentType', width: 110,
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(assessmentTypeLabels).map(([value, label]) => ({
        label,
        value: Number(value),
      })),
      renderType: 'status',
      renderConfig: { statusMap: assessmentTypeMap },
    },
    { key: 'maxMarks', title: 'Max Marks', dataIndex: 'maxMarks', sortable: true, width: 100, hideOnMobile: true },
    {
      key: 'weight', title: 'Weight', dataIndex: 'weight', sortable: true, width: 80, hideOnMobile: true,
      render: (value: number) => `${value}%`,
    },
    { key: 'markCount', title: 'Marks Count', dataIndex: 'markCount', width: 110 },
    {
      key: 'isPublished', title: 'Published', dataIndex: 'isPublished', width: 100,
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Yes', value: true },
        { label: 'No', value: false },
      ],
      renderType: 'boolean',
    },
    {
      key: 'marksReleased', title: 'Marks Released', dataIndex: 'marksReleased', width: 130,
      renderType: 'boolean',
    },
  ];

  const rowActions: RowAction<IAssessmentList>[] = [
    {
      key: 'viewMarks',
      label: 'View Marks',
      icon: <EyeOutlined />,
      onClick: (record) => {
        router.push(`/principal/mark-sheets/${record.id}`);
      },
    },
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      {/* Filters */}
      <Card size="small">
        <Row gutter={16} align="middle">
          <Col xs={24} sm={8} md={6}>
            <label style={{ display: 'block', fontSize: 13, marginBottom: 4, fontWeight: 500 }}>Assessment Type</label>
            <Select
              placeholder="All Types"
              value={selectedType}
              onChange={setSelectedType}
              allowClear
              style={{ width: '100%' }}
              options={
                Object.entries(assessmentTypeMap).map(([key, val]) => ({
                  value: Number(key),
                  label: val.label,
                }))
              }
            />
          </Col>
          <Col xs={24} sm={8} md={6}>
            <label style={{ display: 'block', fontSize: 13, marginBottom: 4, fontWeight: 500 }}>Published</label>
            <Select
              placeholder="All"
              value={selectedPublished}
              onChange={setSelectedPublished}
              allowClear
              style={{ width: '100%' }}
              options={[
                { value: true, label: 'Published' },
                { value: false, label: 'Unpublished' },
              ]}
            />
          </Col>
        </Row>
      </Card>

      {/* Assessments Table */}
      <EnterpriseTable<IAssessmentList>
        title="Mark Sheets"
        columns={columns}
        data={assessments ?? []}
        totalCount={totalCount}
        loading={isPending}
        error={isError}
        onQueryChange={handleQueryChange}
        rowKey="id"
        rowActions={rowActions}
        searchable
        searchPlaceholder="Search by assessment name..."
        searchFilterKey="keyword"
        currentUserRole={currentRole}
        exportConfig={{
          enabled: true,
          formats: ['csv', 'xlsx'],
          requiredPermissions: ['Admin', 'Principal', 'VicePrincipal'],
        }}
      />
    </div>
  );
}

export default function MarkSheetsPageContent() {
  return (
    <AssessmentProvider>
      <MarkSheetsContent />
    </AssessmentProvider>
  );
}
