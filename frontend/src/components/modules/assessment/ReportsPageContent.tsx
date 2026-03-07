'use client';

import React, { useState, useCallback, useEffect } from 'react';
import { Card, Col, Row, Select, message } from 'antd';
import {
  CheckCircleOutlined,
  SendOutlined,
  EyeOutlined,
} from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, BulkAction } from '@/components/shared/enterprise-table';
import { ReportProvider, useReportState, useReportActions } from '@/providers/assessment/reports';
import { AcademicYearProvider, useAcademicYearState, useAcademicYearActions } from '@/providers/academic/academic_years';
import { TermProvider, useTermState, useTermActions } from '@/providers/academic/terms';
import { GradeProvider, useGradeState, useGradeActions } from '@/providers/academic/grades';
import { useAuthState } from '@/providers/auth';
import type { IReportList } from '@/providers/assessment/shared/interfaces';

const reportStatusMap: Record<number, { label: string; color: string }> = {
  0: { label: 'Draft', color: 'default' },
  1: { label: 'Submitted', color: 'blue' },
  2: { label: 'Approved', color: 'green' },
  3: { label: 'Published', color: 'purple' },
  4: { label: 'Acknowledged', color: 'cyan' },
};

const reportTypeMap: Record<number, { label: string; color: string }> = {
  0: { label: 'Term', color: 'blue' },
  1: { label: 'Mid-Year', color: 'orange' },
  2: { label: 'Final', color: 'green' },
};

function ReportsContent() {
  const { reports, totalCount, isPending, isError } = useReportState();
  const { getAllAsync, approveAsync, publishAsync } = useReportActions();
  const { academicYears } = useAcademicYearState();
  const { getAllAsync: getAllAcademicYears } = useAcademicYearActions();
  const { terms } = useTermState();
  const { getAllAsync: getAllTerms } = useTermActions();
  const { activeGrades } = useGradeState();
  const { getActiveGradesAsync } = useGradeActions();
  const { currentRole } = useAuthState();

  const [selectedAcademicYearId, setSelectedAcademicYearId] = useState<string | undefined>(undefined);
  const [selectedTermId, setSelectedTermId] = useState<string | undefined>(undefined);
  const [selectedStatus, setSelectedStatus] = useState<number | undefined>(undefined);
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);

  // Load filter dropdowns
  useEffect(() => {
    getAllAcademicYears({ maxResultCount: 50 });
    getActiveGradesAsync();
  }, []);

  // Load terms when academic year changes
  useEffect(() => {
    if (selectedAcademicYearId) {
      getAllTerms({ maxResultCount: 10 });
    }
  }, [selectedAcademicYearId]);

  const handleQueryChange = useCallback((query: TableQuery) => {
    setLastQuery(query);
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      academicYearId: selectedAcademicYearId,
      termId: selectedTermId,
      status: selectedStatus,
    });
  }, [getAllAsync, selectedAcademicYearId, selectedTermId, selectedStatus]);

  const refreshData = useCallback(() => {
    if (lastQuery) handleQueryChange(lastQuery);
  }, [lastQuery, handleQueryChange]);

  // Re-fetch when filters change
  useEffect(() => {
    refreshData();
  }, [selectedAcademicYearId, selectedTermId, selectedStatus]);

  const columns: ColumnConfig<IReportList>[] = [
    { key: 'studentName', title: 'Student', dataIndex: 'studentName', sortable: true, filterable: true },
    { key: 'studentAdmissionNumber', title: 'Adm #', dataIndex: 'studentAdmissionNumber', sortable: true, width: 100 },
    { key: 'className', title: 'Class', dataIndex: 'className', sortable: true, filterable: true },
    { key: 'termName', title: 'Term', dataIndex: 'termName', sortable: true },
    { key: 'academicYearName', title: 'Year', dataIndex: 'academicYearName', sortable: true, hideOnMobile: true },
    {
      key: 'reportType', title: 'Type', dataIndex: 'reportType', width: 90,
      renderType: 'status',
      renderConfig: { statusMap: reportTypeMap },
    },
    { key: 'overallPercentage', title: 'Overall %', dataIndex: 'overallPercentage', sortable: true, width: 100 },
    { key: 'classPosition', title: 'Position', dataIndex: 'classPosition', sortable: true, hideOnMobile: true, width: 90 },
    { key: 'subjectCount', title: 'Subjects', dataIndex: 'subjectCount', hideOnMobile: true, width: 85 },
    {
      key: 'status', title: 'Status', dataIndex: 'status',
      renderType: 'status',
      renderConfig: { statusMap: reportStatusMap },
    },
  ];

  const rowActions: RowAction<IReportList>[] = [
    {
      key: 'view',
      label: 'View Report',
      icon: <EyeOutlined />,
      onClick: (record) => {
        // TODO: Navigate to report detail page
        console.log('View report:', record.id);
      },
    },
    {
      key: 'approve',
      label: 'Approve',
      icon: <CheckCircleOutlined />,
      visible: (record) => record.status === 1, // Submitted
      confirm: { title: 'Approve this report card?', description: 'The report will be ready for publication.' },
      onClick: async (record) => {
        await approveAsync(record.id);
        message.success('Report approved');
        refreshData();
      },
    },
    {
      key: 'publish',
      label: 'Publish',
      icon: <SendOutlined />,
      visible: (record) => record.status === 2, // Approved
      confirm: { title: 'Publish this report card?', description: 'Parents and students will be able to view it.' },
      onClick: async (record) => {
        await publishAsync(record.id);
        message.success('Report published');
        refreshData();
      },
    },
  ];

  const bulkActions: BulkAction<IReportList>[] = [
    {
      key: 'bulkApprove',
      label: 'Approve Selected',
      confirm: { title: 'Approve all selected reports?' },
      onClick: async (rows) => {
        const submitted = rows.filter(r => r.status === 1);
        if (submitted.length === 0) {
          message.warning('No reports in "Submitted" status selected');
          return;
        }
        for (const row of submitted) await approveAsync(row.id);
        message.success(`${submitted.length} report(s) approved`);
        refreshData();
      },
    },
    {
      key: 'bulkPublish',
      label: 'Publish Selected',
      confirm: { title: 'Publish all selected reports?' },
      onClick: async (rows) => {
        const approved = rows.filter(r => r.status === 2);
        if (approved.length === 0) {
          message.warning('No reports in "Approved" status selected');
          return;
        }
        for (const row of approved) await publishAsync(row.id);
        message.success(`${approved.length} report(s) published`);
        refreshData();
      },
    },
  ];

  // Filter terms by selected academic year
  const filteredTerms = selectedAcademicYearId
    ? terms?.filter(t => t.academicYearId === selectedAcademicYearId)
    : terms;

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      {/* Filters */}
      <Card size="small">
        <Row gutter={16} align="middle">
          <Col xs={24} sm={8} md={6}>
            <label style={{ display: 'block', fontSize: 13, marginBottom: 4, fontWeight: 500 }}>Academic Year</label>
            <Select
              placeholder="All Years"
              value={selectedAcademicYearId}
              onChange={(value) => {
                setSelectedAcademicYearId(value);
                setSelectedTermId(undefined);
              }}
              allowClear
              style={{ width: '100%' }}
              options={
                academicYears?.map(y => ({
                  value: y.id,
                  label: y.yearName,
                })) ?? []
              }
            />
          </Col>
          <Col xs={24} sm={8} md={6}>
            <label style={{ display: 'block', fontSize: 13, marginBottom: 4, fontWeight: 500 }}>Term</label>
            <Select
              placeholder="All Terms"
              value={selectedTermId}
              onChange={setSelectedTermId}
              allowClear
              disabled={!selectedAcademicYearId}
              style={{ width: '100%' }}
              options={
                filteredTerms?.map(t => ({
                  value: t.id,
                  label: t.termName,
                })) ?? []
              }
            />
          </Col>
          <Col xs={24} sm={8} md={6}>
            <label style={{ display: 'block', fontSize: 13, marginBottom: 4, fontWeight: 500 }}>Status</label>
            <Select
              placeholder="All Statuses"
              value={selectedStatus}
              onChange={setSelectedStatus}
              allowClear
              style={{ width: '100%' }}
              options={
                Object.entries(reportStatusMap).map(([key, val]) => ({
                  value: Number(key),
                  label: val.label,
                }))
              }
            />
          </Col>
        </Row>
      </Card>

      {/* Reports Table */}
      <EnterpriseTable<IReportList>
        title="Report Cards"
        columns={columns}
        data={reports ?? []}
        totalCount={totalCount}
        loading={isPending}
        error={isError}
        onQueryChange={handleQueryChange}
        rowKey="id"
        rowActions={rowActions}
        bulkActions={bulkActions}
        selectionMode="multi"
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

export default function ReportsPageContent() {
  return (
    <AcademicYearProvider>
      <TermProvider>
        <GradeProvider>
          <ReportProvider>
            <ReportsContent />
          </ReportProvider>
        </GradeProvider>
      </TermProvider>
    </AcademicYearProvider>
  );
}
