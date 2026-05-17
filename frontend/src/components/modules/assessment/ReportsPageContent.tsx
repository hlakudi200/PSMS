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
import {
  ReportStatus,
  ReportType,
  reportStatusLabels,
  reportTypeLabels,
} from '@/providers/shared/enums';

const reportStatusColors: Record<ReportStatus, string> = {
  [ReportStatus.Draft]: 'default',
  [ReportStatus.Generated]: 'blue',
  [ReportStatus.PendingApproval]: 'orange',
  [ReportStatus.Approved]: 'green',
  [ReportStatus.Published]: 'purple',
};

const reportTypeColors: Record<ReportType, string> = {
  [ReportType.Term1]: 'blue',
  [ReportType.Term2]: 'blue',
  [ReportType.Term3]: 'blue',
  [ReportType.Term4]: 'blue',
  [ReportType.MidYear]: 'orange',
  [ReportType.YearEnd]: 'green',
  [ReportType.Progress]: 'cyan',
};

const reportStatusMap: Record<number, { label: string; color: string }> = Object.fromEntries(
  Object.entries(reportStatusLabels).map(([k, label]) => [
    k,
    { label, color: reportStatusColors[Number(k) as ReportStatus] },
  ])
);

const reportTypeMap: Record<number, { label: string; color: string }> = Object.fromEntries(
  Object.entries(reportTypeLabels).map(([k, label]) => [
    k,
    { label, color: reportTypeColors[Number(k) as ReportType] },
  ])
);

function ReportsContent() {
  const router = useRouter();
  const { reports, totalCount, isPending, isError } = useReportState();
  const { getAllAsync, approveAsync, publishAsync, generatePdfAsync, bulkGeneratePdfsAsync } = useReportActions();
  const { academicYears } = useAcademicYearState();
  const { getAllAsync: getAllAcademicYears } = useAcademicYearActions();
  const { terms } = useTermState();
  const { getByAcademicYearAsync: getTermsByYear } = useTermActions();
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

  // Load terms scoped to the selected academic year
  useEffect(() => {
    if (selectedAcademicYearId) {
      getTermsByYear(selectedAcademicYearId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
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
      filterOptions: Object.entries(reportTypeLabels).map(([value, label]) => ({
        label,
        value: Number(value),
      })),
      renderType: 'status',
      renderConfig: { statusMap: reportTypeMap },
    },
    { key: 'overallPercentage', title: 'Overall %', dataIndex: 'overallPercentage', sortable: true, width: 100 },
    { key: 'classPosition', title: 'Position', dataIndex: 'classPosition', sortable: true, hideOnMobile: true, width: 90 },
    { key: 'subjectCount', title: 'Subjects', dataIndex: 'subjectCount', hideOnMobile: true, width: 85 },
    {
      key: 'status', title: 'Status', dataIndex: 'status',
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(reportStatusLabels).map(([value, label]) => ({
        label,
        value: Number(value),
      })),
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
      visible: (record) => record.status === ReportStatus.PendingApproval,
      confirm: { title: 'Approve this report card?', description: 'The report will be ready for publication.' },
      onClick: async (record) => {
        try {
          await approveAsync(record.id);
          message.success('Report approved');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
    {
      key: 'publish',
      label: 'Publish',
      icon: <SendOutlined />,
      visible: (record) => record.status === ReportStatus.Approved,
      confirm: { title: 'Publish this report card?', description: 'Parents and students will be able to view it.' },
      onClick: async (record) => {
        try {
          await publishAsync(record.id);
          message.success('Report published');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
    {
      key: 'downloadPdf',
      label: 'Download PDF',
      icon: <DownloadOutlined />,
      visible: (record) => !!record.pdfUrl,
      onClick: (record) => {
        if (record.pdfUrl) window.open(record.pdfUrl, '_blank', 'noopener,noreferrer');
      },
    },
    {
      key: 'generatePdf',
      label: 'Generate PDF',
      icon: <FilePdfOutlined />,
      visible: (record) => !record.pdfUrl && record.status >= ReportStatus.Generated,
      onClick: async (record) => {
        try {
          await generatePdfAsync(record.id);
          message.success('PDF generation started');
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
  ];

  const runParallel = async (
    rows: IReportList[],
    fn: (id: string) => void | Promise<unknown>,
    okLabel: string,
    failLabel: string
  ) => {
    const results = await Promise.allSettled(
      rows.map((r) => Promise.resolve(fn(r.id) as unknown))
    );
    const ok = results.filter((r) => r.status === 'fulfilled').length;
    const fail = results.length - ok;
    if (fail === 0) {
      message.success(`${ok} report(s) ${okLabel}`);
    } else if (ok === 0) {
      message.error(`No reports ${okLabel}. ${fail} ${failLabel}.`);
    } else {
      message.warning(`${ok} ${okLabel}; ${fail} ${failLabel}.`);
    }
    refreshData();
  };

  const bulkActions: BulkAction<IReportList>[] = [
    {
      key: 'bulkApprove',
      label: 'Approve Selected',
      confirm: { title: 'Approve all selected reports?' },
      onClick: async (rows) => {
        const eligible = rows.filter((r) => r.status === ReportStatus.PendingApproval);
        if (eligible.length === 0) {
          message.warning('No reports in "Pending Approval" status selected');
          return;
        }
        await runParallel(eligible, approveAsync, 'approved', 'failed');
      },
    },
    {
      key: 'bulkPublish',
      label: 'Publish Selected',
      confirm: { title: 'Publish all selected reports?' },
      onClick: async (rows) => {
        const eligible = rows.filter((r) => r.status === ReportStatus.Approved);
        if (eligible.length === 0) {
          message.warning('No reports in "Approved" status selected');
          return;
        }
        await runParallel(eligible, publishAsync, 'published', 'failed');
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
        const classIds = [...new Set(reports?.map((r) => r.classId) ?? [])];
        if (classIds.length === 0) {
          message.warning('No reports found to generate PDFs for');
          return;
        }
        const results = await Promise.allSettled(
          classIds.map((classId) =>
            Promise.resolve(
              bulkGeneratePdfsAsync({ classId, termId: selectedTermId }) as unknown
            )
          )
        );
        const ok = results.filter((r) => r.status === 'fulfilled').length;
        const fail = results.length - ok;
        if (fail === 0) {
          message.success(`PDF generation started for ${ok} class(es)`);
        } else {
          message.warning(`PDF generation: ${ok} class(es) started; ${fail} failed.`);
        }
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
