'use client';

import React, { useState, useCallback, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Card, Col, Row, Select, message } from 'antd';
import {
  CheckCircleOutlined,
  SendOutlined,
  EyeOutlined,
  FilePdfOutlined,
  DownloadOutlined,
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
  1: { label: 'Draft', color: 'default' },
  2: { label: 'Generated', color: 'blue' },
  3: { label: 'Pending Approval', color: 'orange' },
  4: { label: 'Approved', color: 'green' },
  5: { label: 'Published', color: 'purple' },
};

const reportTypeMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Term 1', color: 'blue' },
  2: { label: 'Term 2', color: 'blue' },
  3: { label: 'Term 3', color: 'blue' },
  4: { label: 'Term 4', color: 'blue' },
  5: { label: 'Mid-Year', color: 'orange' },
  6: { label: 'Year-End', color: 'green' },
  7: { label: 'Progress', color: 'cyan' },
};

function ReportsContent() {
  const router = useRouter();
  const { reports, totalCount, isPending, isError } = useReportState();
  const { getAllAsync, approveAsync, publishAsync, generatePdfAsync, bulkGeneratePdfsAsync } = useReportActions();
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
    const { keyword, ...columnFilters } = query.filters ?? {};
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      academicYearId: selectedAcademicYearId,
      termId: selectedTermId,
      status: selectedStatus ?? columnFilters.status as number | undefined,
      studentName: keyword as string | undefined,
      ...columnFilters,
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
    { key: 'studentName', title: 'Student', dataIndex: 'studentName', sortable: true },
    { key: 'studentAdmissionNumber', title: 'Adm #', dataIndex: 'studentAdmissionNumber', sortable: true, width: 100 },
    { key: 'className', title: 'Class', dataIndex: 'className', sortable: true },
    { key: 'termName', title: 'Term', dataIndex: 'termName', sortable: true },
    { key: 'academicYearName', title: 'Year', dataIndex: 'academicYearName', sortable: true, hideOnMobile: true },
    {
      key: 'reportType', title: 'Type', dataIndex: 'reportType', width: 90,
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Term 1', value: 1 },
        { label: 'Term 2', value: 2 },
        { label: 'Term 3', value: 3 },
        { label: 'Term 4', value: 4 },
        { label: 'Mid-Year', value: 5 },
        { label: 'Year-End', value: 6 },
        { label: 'Progress', value: 7 },
      ],
      renderType: 'status',
      renderConfig: { statusMap: reportTypeMap },
    },
    { key: 'overallPercentage', title: 'Overall %', dataIndex: 'overallPercentage', sortable: true, width: 100 },
    { key: 'classPosition', title: 'Position', dataIndex: 'classPosition', sortable: true, hideOnMobile: true, width: 90 },
    { key: 'subjectCount', title: 'Subjects', dataIndex: 'subjectCount', hideOnMobile: true, width: 85 },
    {
      key: 'status', title: 'Status', dataIndex: 'status',
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Draft', value: 1 },
        { label: 'Generated', value: 2 },
        { label: 'Pending Approval', value: 3 },
        { label: 'Approved', value: 4 },
        { label: 'Published', value: 5 },
      ],
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
        router.push(`/principal/reports/${record.id}`);
      },
    },
    {
      key: 'approve',
      label: 'Approve',
      icon: <CheckCircleOutlined />,
      visible: (record) => record.status === 3, // Pending Approval
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
      visible: (record) => record.status === 4, // Approved
      confirm: { title: 'Publish this report card?', description: 'Parents and students will be able to view it.' },
      onClick: async (record) => {
        await publishAsync(record.id);
        message.success('Report published');
        refreshData();
      },
    },
    {
      key: 'downloadPdf',
      label: 'Download PDF',
      icon: <DownloadOutlined />,
      visible: (record) => !!record.pdfUrl,
      onClick: (record) => {
        window.open(record.pdfUrl, '_blank');
      },
    },
    {
      key: 'generatePdf',
      label: 'Generate PDF',
      icon: <FilePdfOutlined />,
      visible: (record) => !record.pdfUrl && record.status >= 2,
      onClick: async (record) => {
        await generatePdfAsync(record.id);
        message.success('PDF generation started');
      },
    },
  ];

  const bulkActions: BulkAction<IReportList>[] = [
    {
      key: 'bulkApprove',
      label: 'Approve Selected',
      confirm: { title: 'Approve all selected reports?' },
      onClick: async (rows) => {
        const submitted = rows.filter(r => r.status === 3);
        if (submitted.length === 0) {
          message.warning('No reports in "Pending Approval" status selected');
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
        const approved = rows.filter(r => r.status === 4);
        if (approved.length === 0) {
          message.warning('No reports in "Approved" status selected');
          return;
        }
        for (const row of approved) await publishAsync(row.id);
        message.success(`${approved.length} report(s) published`);
        refreshData();
      },
    },
    {
      key: 'bulkGeneratePdfs',
      label: 'Generate All PDFs',
      confirm: { title: 'Generate PDFs for all reports matching current filters?' },
      onClick: async () => {
        if (!selectedAcademicYearId) {
          message.warning('Please select an academic year first');
          return;
        }
        // Use the first report's classId or require class filter
        const classIds = [...new Set(reports?.map(r => r.classId) ?? [])];
        if (classIds.length === 0) {
          message.warning('No reports found to generate PDFs for');
          return;
        }
        let totalQueued = 0;
        for (const classId of classIds) {
          await bulkGeneratePdfsAsync({
            classId,
            termId: selectedTermId,
          });
          totalQueued++;
        }
        message.success(`PDF generation started for ${totalQueued} class(es)`);
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
