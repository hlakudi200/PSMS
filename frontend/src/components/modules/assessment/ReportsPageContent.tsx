'use client';

import React, { useState, useCallback, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { usePortalBase } from '@/utils/portal-base';
import { Card, Col, Row, Select, message } from 'antd';
import {
  CheckCircleOutlined,
  SendOutlined,
  EyeOutlined,
  FilePdfOutlined,
  DownloadOutlined,
  FileAddOutlined,
} from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type {
  ColumnConfig,
  TableQuery,
  RowAction,
  BulkAction,
  ToolbarAction,
} from '@/components/shared/enterprise-table';
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

/**
 * Roles that run the report card pipeline. A teacher holds ReportCards.View and
 * ReportCards.Comment so they can reach their own classes' cards and write the
 * comment printed under their name, but submitting, approving, publishing and
 * producing PDFs are all gated on ReportCards.Generate/Publish on the server —
 * so showing them those controls would only offer a button that returns 403.
 */
const REPORT_PIPELINE_ROLES = ['Admin', 'Principal', 'VicePrincipal', 'HOD'];

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
  const portalBase = usePortalBase();
  const { reports, totalCount, isPending, isError } = useReportState();
  const {
    getAllAsync,
    submitForApprovalAsync,
    publishAsync,
    generatePdfAsync,
    getPdfUrlAsync,
    bulkGeneratePdfsAsync,
  } = useReportActions();
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
        router.push(`${portalBase}/reports/${record.id}`);
      },
    },
    {
      key: 'submitForApproval',
      requiredPermissions: REPORT_PIPELINE_ROLES,
      label: 'Submit for approval',
      icon: <SendOutlined />,
      // RC-09: this is what starts the approval workflow. Without it a generated
      // report had no route into review at all.
      visible: (record) => record.status === ReportStatus.Generated,
      confirm: { title: 'Send this report for approval?' },
      onClick: async (record) => {
        try {
          await submitForApprovalAsync(record.id);
          message.success('Sent for approval');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
    {
      key: 'openApproval',
      requiredPermissions: REPORT_PIPELINE_ROLES,
      label: 'Open approval',
      icon: <CheckCircleOutlined />,
      // RC-09: approval happens only in the workflow, so this opens the step
      // rather than approving here. There is no direct-approve endpoint.
      visible: (record) => !!record.activeWorkflowInstanceId,
      onClick: (record) =>
        router.push(`${portalBase}/workflow/instances/${record.activeWorkflowInstanceId}`),
    },
    {
      key: 'publish',
      requiredPermissions: REPORT_PIPELINE_ROLES,
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
      // RC-04: the link is minted on click and expires in minutes, so there is
      // nothing durable to hold or forward.
      visible: (record) => record.hasPdf,
      onClick: async (record) => {
        try {
          const url = await getPdfUrlAsync(record.id);
          if (url) window.open(url, '_blank', 'noopener,noreferrer');
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
    {
      key: 'generatePdf',
      requiredPermissions: REPORT_PIPELINE_ROLES,
      label: 'Generate PDF',
      icon: <FilePdfOutlined />,
      visible: (record) => !record.hasPdf && record.status >= ReportStatus.Generated,
      onClick: async (record) => {
        try {
          await generatePdfAsync(record.id);
          message.success('PDF generation started');
          // The job produces the PDF asynchronously; refresh so the row stops
          // offering to generate a PDF it has already been asked for.
          refreshData();
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
      key: 'bulkSubmitForApproval',
      requiredPermissions: REPORT_PIPELINE_ROLES,
      label: 'Submit Selected for Approval',
      confirm: { title: 'Send all selected reports for approval?' },
      onClick: async (rows) => {
        const eligible = rows.filter((r) => r.status === ReportStatus.Generated);
        if (eligible.length === 0) {
          message.warning('No reports in "Generated" status selected');
          return;
        }
        await runParallel(eligible, submitForApprovalAsync, 'sent for approval', 'failed');
      },
    },
    {
      key: 'bulkPublish',
      requiredPermissions: REPORT_PIPELINE_ROLES,
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
      requiredPermissions: REPORT_PIPELINE_ROLES,
      label: 'Generate All PDFs',
      confirm: {
        title: 'Generate PDFs for every class on this page?',
        // Deliberately narrower than "matching current filters": classIds below
        // are derived from the loaded page, not from the whole result set.
      },
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

  // RC-02: the entry point to generation. Until this existed there was no way
  // to create a report card from the application at all.
  const toolbarActions: ToolbarAction[] = [
    {
      key: 'generateReports',
      label: 'Generate report cards',
      icon: <FileAddOutlined />,
      type: 'primary',
      onClick: () => router.push(`${portalBase}/reports/generate`),
      requiredPermissions: REPORT_PIPELINE_ROLES,
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
        toolbarActions={toolbarActions}
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
