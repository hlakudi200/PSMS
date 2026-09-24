'use client';

import React, { useState, useCallback, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { usePortalBase } from '@/utils/portal-base';
import { Card, Col, Row, Select, Space, Switch, Tag, Tooltip, Typography, message } from 'antd';
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
import { ClassProvider, useClassState, useClassActions } from '@/providers/academic/classes';
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

/**
 * Above this many report cards, don't watch the PDFs appear — enqueue and say
 * so instead.
 *
 * Waiting is done by asking for each card's download link until it exists, and
 * that link is MINTED on Supabase per call rather than read from our database.
 * At a whole grade — 120 cards — that is 120 storage calls every few seconds
 * for several minutes, which costs far more than the generation it is watching.
 *
 * The real fix is one server-side batch record the screen can poll once per
 * cycle whatever the size; tracked as issue #312. Until then this keeps the
 * progress view for the batch sizes it was built for and steps out of the way
 * for the rest.
 */
const PDF_PROGRESS_WATCH_LIMIT = 25;

/**
 * How many PDFs to ask for at once. One request per card, all in parallel, put
 * 29 concurrent POSTs on the API and some came back 500; a whole grade would be
 * 120. Queuing a job is cheap server-side, so there is nothing to win by
 * flooding it.
 */
const PDF_QUEUE_CHUNK = 5;

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
  } = useReportActions();
  const { academicYears } = useAcademicYearState();
  const { getAllAsync: getAllAcademicYears } = useAcademicYearActions();
  const { terms } = useTermState();
  const { getByAcademicYearAsync: getTermsByYear } = useTermActions();
  const { activeGrades } = useGradeState();
  const { getActiveGradesAsync } = useGradeActions();
  const { classes } = useClassState();
  const { getAllAsync: getAllClasses } = useClassActions();
  const { currentRole } = useAuthState();

  const [selectedAcademicYearId, setSelectedAcademicYearId] = useState<string | undefined>(undefined);
  const [selectedTermId, setSelectedTermId] = useState<string | undefined>(undefined);
  const [selectedStatus, setSelectedStatus] = useState<number | undefined>(undefined);
  // #301: the whole school in one list is unusable — a principal works a class
  // at a time. The API has always accepted ClassId; nothing sent it.
  const [selectedClassId, setSelectedClassId] = useState<string | undefined>(undefined);
  const [onlyMyRegisterClass, setOnlyMyRegisterClass] = useState(false);
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);

  // Load filter dropdowns
  useEffect(() => {
    getAllAcademicYears({ maxResultCount: 50 });
    getActiveGradesAsync();
    getAllClasses({ maxResultCount: 200 });
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
      classId: selectedClassId,
      status: selectedStatus ?? columnFilters.status as number | undefined,
      studentName: keyword as string | undefined,
      ...columnFilters,
    });
  }, [getAllAsync, selectedAcademicYearId, selectedTermId, selectedClassId, selectedStatus]);

  const refreshData = useCallback(() => {
    if (lastQuery) handleQueryChange(lastQuery);
  }, [lastQuery, handleQueryChange]);

  // Re-fetch when filters change
  useEffect(() => {
    refreshData();
  }, [selectedAcademicYearId, selectedTermId, selectedClassId, selectedStatus]);

  const columns: ColumnConfig<IReportList>[] = [
    { key: 'studentName', title: 'Student', dataIndex: 'studentName', sortable: true },
    { key: 'studentAdmissionNumber', title: 'Adm #', dataIndex: 'studentAdmissionNumber', sortable: true, width: 100 },
    {
      key: 'className', title: 'Class', dataIndex: 'className', sortable: true,
      // Whose card is whose to sign. A teacher teaches subjects across several
      // classes but registers only one, and the Class Teacher line is theirs
      // alone — without this the list gave them no way to tell them apart.
      render: (value: string, record: IReportList) => (
        <Space size={6}>
          <span>{value}</span>
          {record.isMyRegisterClass && (
            <Tooltip title="You are the class teacher — this card's Class Teacher line is yours to sign">
              <Tag color="blue" style={{ marginInlineEnd: 0 }}>My class</Tag>
            </Tooltip>
          )}
        </Space>
      ),
    },
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
        // #298: the PDF is produced by a background job, so the row still had
        // hasPdf false when this refreshed a moment later — the Download action
        // never appeared and pressing Generate looked like it did nothing. Wait
        // for the job, then refresh, so the download is actually offered.
        const key = `pdf-${record.id}`;
        try {
          await generatePdfAsync(record.id);
          message.loading({ content: `Producing ${record.studentName}'s PDF…`, key, duration: 0 });

          const deadline = Date.now() + 60_000;
          let url: string | undefined;
          while (!url && Date.now() < deadline) {
            await new Promise((r) => setTimeout(r, 2000));
            url = await getPdfUrlAsync(record.id, { quiet: true });
          }

          if (url) {
            message.success({ content: `${record.studentName}'s PDF is ready to download`, key });
          } else {
            message.info({
              content: 'The PDF is still being produced. It will appear on this row shortly.',
              key,
            });
          }
          refreshData();
        } catch {
          message.destroy(key);
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
      label: 'Generate PDFs for Selected',
      confirm: { title: 'Generate PDFs for the selected report cards?' },
      onClick: async (rows) => {
        /* Was "Generate All PDFs", and did none of what it looked like it did:
           it refused unless an Academic Year filter was set — a year it never
           sent to the API — ignored the rows you had ticked, and then generated
           for every report in each class on the page, not the page and not the
           selection. It now does what its neighbours do and works on what you
           selected. */
        const eligible = rows.filter((r) => !r.hasPdf && r.status >= ReportStatus.Generated);
        if (eligible.length === 0) {
          message.info(
            rows.every((r) => r.hasPdf)
              ? 'Every selected report card already has a PDF.'
              : 'None of the selected report cards can have a PDF yet — generate them first.',
          );
          return;
        }

        const key = 'bulk-pdf';
        try {
          /* Queued a few at a time rather than all at once. Firing one request
             per card in parallel put 29 concurrent POSTs on the API and some
             came back 500; at a whole grade it would be 120. Queuing is cheap
             server-side, so there is nothing to gain from the concurrency. */
          message.loading({ content: `Queueing ${eligible.length} PDF(s)…`, key, duration: 0 });
          let queued = 0;
          const failed: string[] = [];
          for (let i = 0; i < eligible.length; i += PDF_QUEUE_CHUNK) {
            const chunk = eligible.slice(i, i + PDF_QUEUE_CHUNK);
            const settled = await Promise.allSettled(
              chunk.map((r) => Promise.resolve(generatePdfAsync(r.id, { quiet: true }))),
            );
            settled.forEach((res, n) => {
              if (res.status === 'fulfilled') queued++;
              else failed.push(chunk[n].studentName ?? chunk[n].id);
            });
          }

          if (queued === 0) {
            message.error({ content: 'None of the PDFs could be queued. Please try again.', key });
            return;
          }
          if (failed.length) {
            message.warning({
              content: `${queued} of ${eligible.length} PDFs queued; ${failed.length} could not be started `
                + `(${failed.slice(0, 3).join(', ')}${failed.length > 3 ? '…' : ''}). Try those again.`,
              key,
              duration: 10,
            });
          }

          if (eligible.length > PDF_PROGRESS_WATCH_LIMIT) {
            if (!failed.length) {
              message.success({
                content: `${queued} report card PDFs queued. They are produced in the background — `
                  + `refresh this list in a few minutes to download them.`,
                key,
                duration: 8,
              });
            }
            /* One refresh well after the queue has had a chance, rather than
               polling every card. */
            setTimeout(refreshData, 30_000);
            return;
          }

          message.loading({ content: `Producing ${queued} PDF(s)…`, key, duration: 0 });

          /* The PDFs are produced by a background job, so wait for them rather
             than refreshing into rows that still offer Generate (#298). */
          const deadline = Date.now() + 120_000;
          const queuedRows = eligible.filter((r) => !failed.includes(r.studentName ?? r.id));
          const waiting = new Map(queuedRows.map((r) => [r.id, r.studentName]));
          while (waiting.size > 0 && Date.now() < deadline) {
            await new Promise((r) => setTimeout(r, 3000));
            const settled = await Promise.all(
              [...waiting.keys()].map(async (id) => [id, await getPdfUrlAsync(id, { quiet: true })] as const),
            );
            for (const [id, url] of settled) if (url) waiting.delete(id);
          }

          if (waiting.size === 0) {
            message.success({ content: `${queuedRows.length} PDF(s) ready to download`, key });
          } else {
            message.info({
              content: `${queuedRows.length - waiting.size} of ${queuedRows.length} PDFs ready; the rest are still being produced.`,
              key,
            });
          }
          refreshData();
        } catch {
          message.destroy(key);
          // Surfaced by axios interceptor
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

  /* Applied to the loaded page rather than the query: whether a card is mine to
     sign is computed per row from the class register, not stored on the report,
     so there is nothing for the server to filter on. */
  const visibleReports = onlyMyRegisterClass
    ? (reports ?? []).filter((r) => r.isMyRegisterClass)
    : (reports ?? []);
  const myRegisterCount = (reports ?? []).filter((r) => r.isMyRegisterClass).length;

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
            <label style={{ display: 'block', fontSize: 13, marginBottom: 4, fontWeight: 500 }}>Class</label>
            <Select
              placeholder="All Classes"
              value={selectedClassId}
              onChange={setSelectedClassId}
              allowClear
              showSearch
              optionFilterProp="label"
              style={{ width: '100%' }}
              options={
                [...(classes ?? [])]
                  .sort((a, b) => (a.className ?? '').localeCompare(b.className ?? ''))
                  .map((c) => ({ value: c.id, label: c.className })) ?? []
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
        {myRegisterCount > 0 && (
          <Row style={{ marginTop: 12 }}>
            <Col xs={24}>
              <Space size={8}>
                <Switch
                  size="small"
                  checked={onlyMyRegisterClass}
                  onChange={setOnlyMyRegisterClass}
                  id="only-my-register-class"
                />
                <Typography.Text>
                  Only my register class
                </Typography.Text>
                <Typography.Text type="secondary">
                  ({myRegisterCount} of {(reports ?? []).length} on this page {myRegisterCount === 1 ? 'is' : 'are'} mine to sign)
                </Typography.Text>
              </Space>
            </Col>
          </Row>
        )}
      </Card>

      {/* Reports Table */}
      <EnterpriseTable<IReportList>
        title="Report Cards"
        columns={columns}
        data={visibleReports}
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
          <ClassProvider>
            <ReportProvider>
              <ReportsContent />
            </ReportProvider>
          </ClassProvider>
        </GradeProvider>
      </TermProvider>
    </AcademicYearProvider>
  );
}
