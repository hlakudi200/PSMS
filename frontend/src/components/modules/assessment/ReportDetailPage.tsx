'use client';

import React, { useEffect, useCallback, useState } from 'react';
import { useParams, useRouter, usePathname } from 'next/navigation';
import { portalBaseFrom } from '@/utils/portal-base';
import {
  Card,
  Descriptions,
  Tag,
  Table,
  Space,
  Button,
  Typography,
  Spin,
  Empty,
  Statistic,
  Row,
  Col,
  Alert,
  Input,
  message,
  Popconfirm,
} from 'antd';
import {
  ArrowLeftOutlined,
  CheckCircleOutlined,
  SendOutlined,
  PrinterOutlined,
  MessageOutlined,
  FilePdfOutlined,
  DownloadOutlined,
  LoadingOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { ReportProvider, useReportState, useReportActions } from '@/providers/assessment/reports';
import { useAuthState } from '@/providers/auth';
import { useBrandingState } from '@/providers/branding';
import { getPrintableInk, getReadableForeground } from '@/utils/theme-config';
import { formatMark, formatPercentage } from '@/utils/marks';
import { PromotionDecisionModal, promotionDecisionLabels } from './PromotionDecisionModal';
import type { IReport, IReportSubject } from '@/providers/assessment/shared/interfaces';

const { Title, Text, Paragraph } = Typography;

const statusMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Draft', color: 'default' },
  2: { label: 'Generated', color: 'blue' },
  3: { label: 'Pending Approval', color: 'orange' },
  4: { label: 'Approved', color: 'green' },
  5: { label: 'Published', color: 'purple' },
};

const reportTypeMap: Record<number, string> = {
  1: 'Term 1 Report',
  2: 'Term 2 Report',
  3: 'Term 3 Report',
  4: 'Term 4 Report',
  5: 'Mid-Year Report',
  6: 'Year-End Report',
  7: 'Progress Report',
};

const achievementLabels: Record<number, { symbol: string; desc: string }> = {
  1: { symbol: '1', desc: 'Not Achieved (0-29%)' },
  2: { symbol: '2', desc: 'Elementary (30-39%)' },
  3: { symbol: '3', desc: 'Moderate (40-49%)' },
  4: { symbol: '4', desc: 'Adequate (50-59%)' },
  5: { symbol: '5', desc: 'Substantial (60-69%)' },
  6: { symbol: '6', desc: 'Meritorious (70-79%)' },
  7: { symbol: '7', desc: 'Outstanding (80-100%)' },
};

/**
 * RC-15. NPPPPR §31(1): in Grade 12 the school-based assessment is 25% of the
 * total mark and the external assessment 75%, and that external paper is set
 * and marked by the Department of Basic Education, not by the school. A card
 * showing the 25% has to say what it is.
 */
const EXTERNAL_EXAM_NOTE =
  '† School-based assessment component only. The National Senior Certificate '
  + 'examination is set and marked externally by the Department of Basic Education '
  + 'and is not included in this mark. The final result is issued on the '
  + "Department's statement of results.";

const achievementLabelsShort: Record<number, string> = {
  1: '1', 2: '2', 3: '3', 4: '4', 5: '5', 6: '6', 7: '7',
};

function getAchievementTag(level?: number) {
  if (!level) return <Text type="secondary">-</Text>;
  const info = achievementLabels[level];
  if (!info) return <Tag>{level}</Tag>;
  const colors: Record<number, string> = { 1: 'red', 2: 'orange', 3: 'gold', 4: 'lime', 5: 'green', 6: 'blue', 7: 'purple' };
  return <Tag color={colors[level] ?? 'default'}>{info.symbol}</Tag>;
}

/* ─── Print stylesheet ─── */
const buildPrintStyles = (ink: string, brandBg: string, brandFg: string) => `
@media print {
  /* Reset page */
  @page {
    size: A4 portrait;
    margin: 12mm 10mm 12mm 10mm;
  }

  /* Hide everything except the report card */
  body * { visibility: hidden; }
  .report-print-area, .report-print-area * { visibility: visible; }
  .report-print-area {
    position: absolute;
    left: 0; top: 0;
    width: 100%;
  }

  /* Hide interactive / screen-only elements */
  .no-print { display: none !important; }

  /* Show print-only elements */
  .print-only { display: block !important; }

  /* ─── School Header ─── */
  .print-header {
    display: flex !important;
    flex-direction: column;
    align-items: center;
    text-align: center;
    border-bottom: 3px double ${ink};
    padding-bottom: 8px;
    margin-bottom: 10px;
  }
  .print-header h2 {
    font-size: 20px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 1px;
    margin: 0;
    color: ${ink};
  }
  /* The school's logo, constrained so a tall or wide upload cannot push the
     rest of the card onto a second page. */
  .print-header .print-logo {
    max-height: 56px;
    max-width: 160px;
    object-fit: contain;
    margin-bottom: 6px;
  }
  .print-header .report-type-label {
    font-size: 14px;
    font-weight: 600;
    margin-top: 2px;
  }

  /* ─── Student Info Grid ─── */
  .print-student-info {
    display: grid !important;
    grid-template-columns: 1fr 1fr;
    gap: 2px 24px;
    border: 1px solid #000;
    padding: 8px 12px;
    margin-bottom: 10px;
    font-size: 11px;
  }
  .print-student-info .info-row {
    display: flex;
    gap: 6px;
    padding: 2px 0;
    border-bottom: 1px dotted #ccc;
  }
  .print-student-info .info-label {
    font-weight: 700;
    min-width: 110px;
    white-space: nowrap;
  }

  /* ─── Subject Table ─── */
  .print-subject-table {
    display: table !important;
    width: 100%;
    border-collapse: collapse;
    margin-bottom: 10px;
    font-size: 10px;
  }
  .print-subject-table th,
  .print-subject-table td {
    border: 1px solid #000;
    padding: 4px 6px;
    text-align: center;
  }
  .print-subject-table th {
    background: ${brandBg} !important;
    color: ${brandFg} !important;
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
    font-weight: 700;
    font-size: 9px;
    text-transform: uppercase;
  }
  .print-subject-table td:first-child {
    text-align: left;
    font-weight: 600;
  }
  .print-subject-table td:nth-child(2) {
    text-align: left;
  }
  .print-subject-table tfoot td {
    font-weight: 700;
    background: #f5f5f5 !important;
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
  }

  /* ─── RC-16: the promotion decision ─── */
  .print-promotion {
    display: block !important;
    border: 1px solid #000;
    margin-bottom: 10px;
  }
  .print-promotion-title {
    background: #f5f5f5 !important;
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
    font-size: 9px;
    font-weight: 700;
    padding: 3px 6px;
    border-bottom: 1px solid #000;
  }
  .print-promotion-body {
    font-size: 12px;
    font-weight: 700;
    padding: 6px;
  }
  .print-promotion-reason {
    font-size: 10px;
    padding: 0 6px 6px;
  }

  /* ─── RC-15: the external-examination note under the marks table ─── */
  .print-external-exam-note {
    display: block !important;
    font-size: 8.5px;
    color: #444;
    margin: -4px 0 10px;
    line-height: 1.35;
  }

  /* ─── Attendance Box ─── */
  .print-attendance {
    display: flex !important;
    gap: 0;
    border: 1px solid #000;
    margin-bottom: 10px;
    font-size: 11px;
  }
  .print-attendance .att-cell {
    flex: 1;
    text-align: center;
    padding: 6px 4px;
    border-right: 1px solid #000;
  }
  .print-attendance .att-cell:last-child {
    border-right: none;
  }
  .print-attendance .att-label {
    font-weight: 700;
    font-size: 9px;
    text-transform: uppercase;
    display: block;
  }
  .print-attendance .att-value {
    font-size: 16px;
    font-weight: 700;
  }

  /* ─── Comments ─── */
  .print-comments {
    display: block !important;
    margin-bottom: 10px;
  }
  .print-comment-block {
    border: 1px solid #000;
    padding: 6px 10px;
    margin-bottom: 6px;
    min-height: 48px;
    font-size: 11px;
  }
  .print-comment-block .comment-label {
    font-weight: 700;
    font-size: 10px;
    text-transform: uppercase;
    border-bottom: 1px solid #ccc;
    padding-bottom: 3px;
    margin-bottom: 4px;
  }

  /* ─── Signature Lines ─── */
  .print-signatures {
    display: flex !important;
    justify-content: space-between;
    margin-top: 20px;
    font-size: 10px;
  }
  .print-signatures .sig-block {
    text-align: center;
    width: 30%;
  }
  .print-signatures .sig-line {
    border-top: 1px solid #000;
    margin-top: 32px;
    padding-top: 4px;
  }

  /* ─── CAPS Legend ─── */
  .print-legend {
    display: block !important;
    border: 1px solid #000;
    padding: 6px 10px;
    margin-top: 10px;
    font-size: 8px;
    page-break-inside: avoid;
  }
  .print-legend h4 {
    font-size: 9px;
    font-weight: 700;
    margin: 0 0 4px 0;
    text-transform: uppercase;
  }
  .print-legend table {
    width: 100%;
    border-collapse: collapse;
  }
  .print-legend th,
  .print-legend td {
    border: 1px solid #ccc;
    padding: 2px 6px;
    text-align: center;
  }
  .print-legend th {
    background: #f0f0f0 !important;
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
    font-weight: 700;
  }

  /* ─── Approval / Date footer ─── */
  .print-footer {
    display: block !important;
    text-align: center;
    font-size: 9px;
    color: #666;
    margin-top: 8px;
    border-top: 1px solid #ccc;
    padding-top: 4px;
  }

  /* Hide Ant Design screen components during print */
  .screen-only { display: none !important; }

  /* Remove Ant card styling artifacts */
  .ant-card { box-shadow: none !important; border: none !important; }
}

/* Default: hide print-only elements on screen */
.print-only { display: none; }
.print-header { display: none; }
.print-student-info { display: none; }
.print-subject-table { display: none; }
.print-promotion { display: none; }
.print-external-exam-note { display: none; }
.print-attendance { display: none; }
.print-comments { display: none; }
.print-signatures { display: none; }
.print-legend { display: none; }
.print-footer { display: none; }
`;

function ReportDetailContent() {
  const params = useParams();
  const router = useRouter();
  const pathname = usePathname();
  const reportId = params.id as string;

  const { currentRole, currentUser } = useAuthState();
  // The printed card carries the school's identity, not PSMS's (issue #56).
  const { branding } = useBrandingState();
  const printInk = getPrintableInk(branding.primaryColor);
  const printStyles = buildPrintStyles(
    printInk,
    branding.primaryColor,
    getReadableForeground(branding.primaryColor)
  );
  // Managers (principal/vice-principal/admin) get the full report actions
  // (Approve / Publish / Generate PDF / Principal Comment). Everyone else — e.g.
  // a teacher reviewing the report at the HOD step via the workflow — gets a
  // clean read-only view. The server is the real boundary (those actions are
  // permission-gated); this just keeps the UI honest.
  const managementRoles = ['admin', 'principal', 'viceprincipal'];
  const myRoles = (currentUser?.roleNames?.length ? currentUser.roleNames : [currentRole])
    .filter(Boolean)
    .map((r) => (r as string).toLowerCase());
  const canManageReport = myRoles.some((r) => managementRoles.includes(r));
  // Reports detail is reached from the principal reports list and, via the
  // workflow "View full report" link, from the teacher and admin portals.
  const reportsPortalRoot = portalBaseFrom(pathname);

  const goBack = useCallback(() => {
    // Prefer browser history; fall back to the portal's home (only the principal
    // portal has a reports list page — elsewhere reports are reached via links).
    if (typeof window !== 'undefined' && window.history.length > 1) router.back();
    else router.push(reportsPortalRoot === '/principal' ? `${reportsPortalRoot}/reports` : reportsPortalRoot);
  }, [router, reportsPortalRoot]);

  const { report, isPending, isError } = useReportState();
  const {
    getAsync,
    submitForApprovalAsync,
    publishAsync,
    addPrincipalCommentAsync,
    generatePdfAsync,
    getPdfUrlAsync,
  } = useReportActions();

  const [principalComment, setPrincipalComment] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [commentSaving, setCommentSaving] = useState(false);
  const [pdfGenerating, setPdfGenerating] = useState(false);

  useEffect(() => {
    if (reportId) getAsync(reportId);
  }, [reportId, getAsync]);

  useEffect(() => {
    if (report?.principalComment) setPrincipalComment(report.principalComment);
  }, [report?.principalComment]);

  const refresh = useCallback(() => {
    getAsync(reportId);
  }, [reportId, getAsync]);

  // RC-09: submitting is what starts the approval workflow server-side, which
  // is why this is the only route into review. Nothing in the UI called it
  // before, so a generated report had nowhere to go.
  const handleSubmitForApproval = async () => {
    setSubmitting(true);
    try {
      await submitForApprovalAsync(reportId);
      message.success('Sent for approval');
      refresh();
    } catch {
      // Surfaced by the axios error interceptor.
    } finally {
      setSubmitting(false);
    }
  };

  const [promotionOpen, setPromotionOpen] = useState(false);

  const handlePublish = async () => {
    await publishAsync(reportId);
    message.success('Report published');
    refresh();
  };

  const handleSaveComment = async () => {
    if (!principalComment.trim()) return;
    setCommentSaving(true);
    try {
      await addPrincipalCommentAsync(reportId, { comment: principalComment.trim() });
      message.success('Comment saved');
      refresh();
    } catch {
      message.error('Failed to save comment');
    } finally {
      setCommentSaving(false);
    }
  };

  const handlePrint = () => {
    window.print();
  };

  const handleGeneratePdf = async () => {
    setPdfGenerating(true);
    try {
      await generatePdfAsync(reportId);
      message.success('PDF generation started. It will be available shortly.');
      // Poll for PDF URL
      const pollInterval = setInterval(async () => {
        await getAsync(reportId);
      }, 3000);
      // Stop polling after 60 seconds
      setTimeout(() => {
        clearInterval(pollInterval);
        setPdfGenerating(false);
      }, 60000);
    } catch {
      message.error('Failed to start PDF generation');
      setPdfGenerating(false);
    }
  };

  // Stop polling once the PDF exists
  useEffect(() => {
    if (report?.hasPdf && pdfGenerating) {
      setPdfGenerating(false);
      message.success('PDF is ready for download');
    }
  }, [report?.hasPdf, pdfGenerating]);

  if (isPending && !report) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 80 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (isError || (!isPending && !report)) {
    return (
      <div style={{ padding: 24 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={goBack} style={{ marginBottom: 16 }}>
          Back
        </Button>
        <Empty description="Report not found" />
      </div>
    );
  }

  if (!report) return null;

  const status = statusMap[report.status] ?? { label: 'Unknown', color: 'default' };
  const subjects = report.subjectReports ?? [];

  const subjectColumns = [
    { title: 'Subject', dataIndex: 'subjectName', key: 'subjectName', render: (v: string) => <Text strong>{v}</Text> },
    { title: 'Code', dataIndex: 'subjectCode', key: 'subjectCode', width: 80 },
    {
      title: 'Term Mark', dataIndex: 'termMark', key: 'termMark', width: 100,
      render: (v: number | undefined) => formatPercentage(v),
    },
    {
      title: 'Exam Mark', dataIndex: 'examMark', key: 'examMark', width: 100,
      render: (v: number | undefined) => formatPercentage(v),
    },
    {
      title: 'Final Mark', dataIndex: 'finalMark', key: 'finalMark', width: 100,
      render: (v: number | undefined, s: IReportSubject) => v != null
        ? <Text strong>{formatPercentage(v)}{s.awaitsExternalExamination ? ' †' : ''}</Text>
        : '-',
    },
    {
      title: 'Level', dataIndex: 'achievementLevel', key: 'achievementLevel', width: 160,
      render: (v: number | undefined) => getAchievementTag(v),
    },
    /* RC-06: the cohort figures. Blank until the class statistics pass has run
       for this term — an empty cell rather than a 0 that reads as a real mark. */
    {
      title: 'Class Avg', dataIndex: 'classAverage', key: 'classAverage', width: 100,
      render: (v: number | undefined) => v != null ? formatPercentage(v) : <Text type="secondary">-</Text>,
    },
    {
      title: 'Position', dataIndex: 'subjectPosition', key: 'subjectPosition', width: 90,
      render: (v: number | undefined) => v != null ? v : <Text type="secondary">-</Text>,
    },
    {
      title: 'Class Range', key: 'classRange', width: 130,
      render: (_: unknown, s: IReportSubject) =>
        s.lowestInClass != null && s.highestInClass != null
          ? <Text type="secondary">{formatPercentage(s.lowestInClass)} – {formatPercentage(s.highestInClass)}</Text>
          : <Text type="secondary">-</Text>,
    },
    { title: 'Teacher', dataIndex: 'teacherName', key: 'teacherName' },
    {
      title: 'Comment', dataIndex: 'teacherComment', key: 'teacherComment',
      render: (v: string | undefined) => v || <Text type="secondary">-</Text>,
      ellipsis: true,
    },
  ];

  /* RC-07: the printed footer shows the overall stored on the report — the same
     number as the Overall card above it. It used to be recomputed here from the
     subject rows, so the print-out could contradict the screen it was printed
     from. */
  const overallAvg = formatMark(report.overallPercentage);

  /* RC-16: only the year-end card carries a promotion decision — it is the one
     with the composite marks for the year behind it. */
  const isYearEndReport = report.reportType === 6;

  return (
    <div style={{ padding: 24 }} className="report-print-area">
      <style>{printStyles}</style>

      {/* ══════════════ PRINT LAYOUT (hidden on screen, shown on print) ══════════════ */}

      {/* School Header */}
      <div className="print-header">
        {branding.logoUrl && (
          // eslint-disable-next-line @next/next/no-img-element
          <img className="print-logo" src={branding.logoUrl} alt={`${branding.schoolName} logo`} />
        )}
        {/* schoolName was previously cast onto IReport, where it does not exist,
            so every printed card read the literal fallback. */}
        <h2>{branding.schoolName || 'School Report Card'}</h2>
        <div className="report-type-label">{reportTypeMap[report.reportType] ?? 'Student Report Card'}</div>
        <div style={{ fontSize: 11, marginTop: 2 }}>
          {report.termName ?? ''} {report.termName && report.academicYearName ? ' — ' : ''} {report.academicYearName ?? ''}
        </div>
      </div>

      {/* Student Info */}
      <div className="print-student-info">
        <div className="info-row"><span className="info-label">Student Name:</span><span>{report.studentName}</span></div>
        <div className="info-row"><span className="info-label">Admission No:</span><span>{report.studentAdmissionNumber ?? 'N/A'}</span></div>
        <div className="info-row"><span className="info-label">Class:</span><span>{report.className ?? 'N/A'}</span></div>
        <div className="info-row"><span className="info-label">Date Generated:</span><span>{report.generatedDate ? dayjs(report.generatedDate).format('DD MMM YYYY') : 'N/A'}</span></div>
        <div className="info-row"><span className="info-label">Class Position:</span><span>{report.classPosition ?? '-'}{report.totalStudentsInClass ? ` of ${report.totalStudentsInClass}` : ''}</span></div>
        <div className="info-row"><span className="info-label">Overall:</span><span>{formatPercentage(report.overallPercentage)}</span></div>
      </div>

      {/* Subject Table for Print */}
      <table className="print-subject-table">
        <thead>
          <tr>
            <th style={{ textAlign: 'left', width: '20%' }}>Subject</th>
            <th style={{ width: '6%' }}>Code</th>
            <th style={{ width: '9%' }}>Term Mark (%)</th>
            <th style={{ width: '9%' }}>Exam Mark (%)</th>
            <th style={{ width: '9%' }}>Final Mark (%)</th>
            <th style={{ width: '6%' }}>Level</th>
            <th style={{ width: '9%' }}>Class Avg (%)</th>
            <th style={{ width: '5%' }}>Pos</th>
            <th style={{ width: '12%' }}>Teacher</th>
            <th style={{ textAlign: 'left', width: '15%' }}>Comment</th>
          </tr>
        </thead>
        <tbody>
          {subjects.map((s) => (
            <tr key={s.id}>
              <td>{s.subjectName}</td>
              <td>{s.subjectCode ?? '-'}</td>
              <td>{formatMark(s.termMark)}</td>
              <td>{formatMark(s.examMark)}</td>
              <td style={{ fontWeight: 700 }}>
                {formatMark(s.finalMark)}{s.awaitsExternalExamination ? ' †' : ''}
              </td>
              <td>{s.achievementLevel ? achievementLabelsShort[s.achievementLevel] : '-'}</td>
              <td>{formatMark(s.classAverage)}</td>
              <td>{s.subjectPosition ?? '-'}</td>
              <td style={{ fontSize: 9, textAlign: 'left' }}>{s.teacherName ?? '-'}</td>
              <td style={{ fontSize: 9, textAlign: 'left' }}>{s.teacherComment ?? '-'}</td>
            </tr>
          ))}
        </tbody>
        <tfoot>
          <tr>
            <td colSpan={4} style={{ textAlign: 'right' }}>Overall Average:</td>
            <td>{overallAvg}%</td>
            <td>{report.overallAchievementLevel ? achievementLabelsShort[report.overallAchievementLevel] : '-'}</td>
            <td colSpan={4}></td>
          </tr>
        </tfoot>
      </table>

      {/* RC-15: what the dagger means, when anything carries one. The PDF
          prints the same note; the two must not say different things. */}
      {subjects.some((s) => s.awaitsExternalExamination) && (
        <div className="print-external-exam-note">{EXTERNAL_EXAM_NOTE}</div>
      )}

      {/* RC-16: the decision, printed. The PDF prints the same block. */}
      {report.promotionDecision && (
        <div className="print-promotion">
          <div className="print-promotion-title">PROMOTION DECISION</div>
          <div className="print-promotion-body">
            {promotionDecisionLabels[report.promotionDecision] ?? 'Recorded'}
            {report.promotedToGradeName ? ` to ${report.promotedToGradeName}` : ''}
          </div>
          {report.promotionReason && (
            <div className="print-promotion-reason">{report.promotionReason}</div>
          )}
        </div>
      )}

      {/* Attendance for Print */}
      <div className="print-attendance">
        <div className="att-cell">
          <span className="att-label">Days Present</span>
          <span className="att-value">{report.daysPresent ?? '-'}</span>
        </div>
        <div className="att-cell">
          <span className="att-label">Days Absent</span>
          <span className="att-value">{report.daysAbsent ?? '-'}</span>
        </div>
        <div className="att-cell">
          <span className="att-label">Days Late</span>
          <span className="att-value">{report.daysLate ?? '-'}</span>
        </div>
        <div className="att-cell">
          <span className="att-label">Total School Days</span>
          <span className="att-value">
            {report.daysPresent != null && report.daysAbsent != null
              ? report.daysPresent + report.daysAbsent
              : '-'}
          </span>
        </div>
      </div>

      {/* Comments for Print */}
      <div className="print-comments">
        <div className="print-comment-block">
          <div className="comment-label">Class Teacher&apos;s Comment</div>
          <div>{report.teacherComment || '\u00A0'}</div>
        </div>
        <div className="print-comment-block">
          <div className="comment-label">Principal&apos;s Comment</div>
          <div>{report.principalComment || '\u00A0'}</div>
        </div>
        {report.parentComment && (
          <div className="print-comment-block">
            <div className="comment-label">Parent&apos;s Comment</div>
            <div>{report.parentComment}</div>
          </div>
        )}
      </div>

      {/* Signature Lines for Print */}
      <div className="print-signatures">
        <div className="sig-block">
          <div className="sig-line">Class Teacher</div>
        </div>
        <div className="sig-block">
          <div className="sig-line">Principal</div>
        </div>
        <div className="sig-block">
          <div className="sig-line">Parent / Guardian</div>
        </div>
      </div>

      {/* CAPS Legend for Print */}
      <div className="print-legend">
        <h4>CAPS Achievement Level Descriptors</h4>
        <table>
          <thead>
            <tr>
              <th>Level 7</th><th>Level 6</th><th>Level 5</th><th>Level 4</th><th>Level 3</th><th>Level 2</th><th>Level 1</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>Outstanding<br />80–100%</td>
              <td>Meritorious<br />70–79%</td>
              <td>Substantial<br />60–69%</td>
              <td>Adequate<br />50–59%</td>
              <td>Moderate<br />40–49%</td>
              <td>Elementary<br />30–39%</td>
              <td>Not Achieved<br />0–29%</td>
            </tr>
          </tbody>
        </table>
      </div>

      {/* Print Footer */}
      <div className="print-footer">
        {report.approvedDate && <>Approved: {dayjs(report.approvedDate).format('DD MMM YYYY')} &nbsp;|&nbsp; </>}
        {report.publishedDate && <>Published: {dayjs(report.publishedDate).format('DD MMM YYYY')} &nbsp;|&nbsp; </>}
        Printed: {dayjs().format('DD MMM YYYY')}
      </div>

      {/* ══════════════ SCREEN LAYOUT (shown on screen, hidden on print) ══════════════ */}

      <Space className="no-print" style={{ marginBottom: 16 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={goBack}>
          Back
        </Button>
        <Button icon={<PrinterOutlined />} onClick={handlePrint}>
          Print Report Card
        </Button>
        {report.hasPdf ? (
          <Button
            type="primary"
            icon={<DownloadOutlined />}
            onClick={async () => {
                  try {
                    const url = await getPdfUrlAsync(reportId);
                    if (url) window.open(url, '_blank', 'noopener,noreferrer');
                  } catch {
                    // Surfaced by the axios error interceptor.
                  }
                }}
          >
            Download PDF
          </Button>
        ) : (
          canManageReport && report.status >= 2 && (
            <Button
              icon={pdfGenerating ? <LoadingOutlined /> : <FilePdfOutlined />}
              onClick={handleGeneratePdf}
              loading={pdfGenerating}
            >
              Generate PDF
            </Button>
          )
        )}
      </Space>

      {/* Status Banner — manage actions only for principal/vice-principal/admin */}
      {canManageReport && report.status === 2 && (
        <Alert
          className="no-print"
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
          message="This report is generated and has not been sent for approval."
          description="Submitting starts the approval workflow, so the review is recorded against the report."
          action={
            <Popconfirm
              title="Send this report for approval?"
              onConfirm={handleSubmitForApproval}
            >
              <Button type="primary" size="small" icon={<SendOutlined />} loading={submitting}>
                Submit for approval
              </Button>
            </Popconfirm>
          }
        />
      )}
      {report.status === 3 && (
        <Alert
          className="no-print"
          type="warning"
          showIcon
          style={{ marginBottom: 16 }}
          message="This report is awaiting approval."
          description="Approval happens in the workflow so the review is recorded against the report. Open the approval step to act on it."
          action={
            report.activeWorkflowInstanceId ? (
              <Button
                type="primary"
                size="small"
                icon={<CheckCircleOutlined />}
                onClick={() =>
                  router.push(`${reportsPortalRoot}/workflow/instances/${report.activeWorkflowInstanceId}`)
                }
              >
                Open approval
              </Button>
            ) : undefined
          }
        />
      )}
      {canManageReport && report.status === 4 && (
        <Alert
          className="no-print"
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
          message="This report is approved and ready to publish."
          action={
            <Popconfirm title="Publish this report? Parents will be able to view it." onConfirm={handlePublish}>
              <Button type="primary" size="small" icon={<SendOutlined />}>Publish</Button>
            </Popconfirm>
          }
        />
      )}

      {/* RC-16: the promotion decision. NPPPPR §(2b)(c) requires it to be
          reflected on the learner's report card, and the year-end card is the
          one that carries it. */}
      {isYearEndReport && (
        <Alert
          className="no-print"
          type={report.promotionDecision ? 'success' : 'warning'}
          showIcon
          style={{ marginBottom: 16 }}
          message={
            report.promotionDecision
              ? `Promotion: ${promotionDecisionLabels[report.promotionDecision] ?? 'Recorded'}${
                  report.promotedToGradeName ? ` to ${report.promotedToGradeName}` : ''
                }`
              : 'No promotion decision has been recorded on this year-end report.'
          }
          description={report.promotionReason}
          action={
            canManageReport && report.status !== 5 ? (
              <Button size="small" onClick={() => setPromotionOpen(true)}>
                {report.promotionDecision ? 'Review' : 'Record decision'}
              </Button>
            ) : undefined
          }
        />
      )}

      {isYearEndReport && (
        <PromotionDecisionModal
          open={promotionOpen}
          reportId={report.id}
          studentName={report.studentName}
          onClose={() => setPromotionOpen(false)}
          onRecorded={() => getAsync(report.id)}
        />
      )}

      {/* Report Header */}
      <Card className="no-print" style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Title level={3} style={{ margin: 0 }}>{report.studentName}</Title>
            <Space size="large" style={{ marginTop: 8 }}>
              <Text type="secondary">Adm #: {report.studentAdmissionNumber ?? 'N/A'}</Text>
              <Text type="secondary">Class: {report.className ?? 'N/A'}</Text>
              <Text type="secondary">{report.termName ?? ''} - {report.academicYearName ?? ''}</Text>
              <Tag color="blue">{reportTypeMap[report.reportType] ?? 'Report'}</Tag>
              <Tag color={status.color}>{status.label}</Tag>
            </Space>
          </Col>
          {report.generatedDate && (
            <Col>
              <Text type="secondary">Generated: {dayjs(report.generatedDate).format('DD MMM YYYY')}</Text>
            </Col>
          )}
        </Row>
      </Card>

      {/* Summary Stats */}
      <Row gutter={16} style={{ marginBottom: 16 }} className="no-print">
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Overall"
              value={report.overallPercentage ?? 0}
              suffix="%"
              valueStyle={{ color: (report.overallPercentage ?? 0) >= 50 ? '#3f8600' : '#cf1322' }}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Class Position"
              value={report.classPosition ?? '-'}
              suffix={report.totalStudentsInClass ? ` / ${report.totalStudentsInClass}` : ''}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic title="Subjects" value={subjects.length} />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic title="Achievement" valueRender={() => getAchievementTag(report.overallAchievementLevel)} />
          </Card>
        </Col>
      </Row>

      {/* Attendance Row */}
      <Row gutter={16} style={{ marginBottom: 16 }} className="no-print">
        <Col xs={8}><Card size="small"><Statistic title="Days Present" value={report.daysPresent} valueStyle={{ color: '#3f8600' }} /></Card></Col>
        <Col xs={8}><Card size="small"><Statistic title="Days Absent" value={report.daysAbsent} valueStyle={{ color: report.daysAbsent > 0 ? '#cf1322' : undefined }} /></Card></Col>
        <Col xs={8}><Card size="small"><Statistic title="Days Late" value={report.daysLate} valueStyle={{ color: report.daysLate > 0 ? '#faad14' : undefined }} /></Card></Col>
      </Row>

      {/* Subject Breakdown */}
      <Card title="Subject Breakdown" style={{ marginBottom: 16 }} className="no-print">
        <Table<IReportSubject>
          dataSource={subjects}
          columns={subjectColumns}
          rowKey="id"
          pagination={false}
          size="small"
          locale={{ emptyText: <Empty description="No subject data" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
        />
      </Card>

      {/* Comments Section */}
      <Card title="Comments" style={{ marginBottom: 16 }} className="no-print">
        {report.teacherComment && (
          <div style={{ marginBottom: 16 }}>
            <Text strong>Class Teacher Comment</Text>
            <Paragraph style={{ marginTop: 4, padding: '8px 12px', background: '#f6f8fa', borderRadius: 4 }}>
              {report.teacherComment}
            </Paragraph>
          </div>
        )}

        {canManageReport ? (
          <div style={{ marginBottom: 16 }}>
            <Text strong>Principal Comment</Text>
            <Input.TextArea
              value={principalComment}
              onChange={(e) => setPrincipalComment(e.target.value)}
              rows={3}
              placeholder="Add your comment..."
              maxLength={1000}
              style={{ marginTop: 4 }}
            />
            <Button
              type="primary"
              size="small"
              icon={<MessageOutlined />}
              onClick={handleSaveComment}
              loading={commentSaving}
              disabled={!principalComment.trim()}
              style={{ marginTop: 8 }}
            >
              Save Comment
            </Button>
          </div>
        ) : (
          report.principalComment && (
            <div style={{ marginBottom: 16 }}>
              <Text strong>Principal Comment</Text>
              <Paragraph style={{ marginTop: 4, padding: '8px 12px', background: '#f6f8fa', borderRadius: 4 }}>
                {report.principalComment}
              </Paragraph>
            </div>
          )
        )}

        {report.parentComment && (
          <div>
            <Text strong>Parent Comment</Text>
            <Paragraph style={{ marginTop: 4, padding: '8px 12px', background: '#f6f8fa', borderRadius: 4 }}>
              {report.parentComment}
            </Paragraph>
          </div>
        )}
      </Card>

      {/* Approval Info */}
      {report.approvedDate && (
        <Card size="small" className="no-print">
          <Descriptions size="small">
            <Descriptions.Item label="Approved Date">{dayjs(report.approvedDate).format('DD MMM YYYY')}</Descriptions.Item>
            {report.publishedDate && <Descriptions.Item label="Published Date">{dayjs(report.publishedDate).format('DD MMM YYYY')}</Descriptions.Item>}
            {report.parentAcknowledgedDate && <Descriptions.Item label="Parent Acknowledged">{dayjs(report.parentAcknowledgedDate).format('DD MMM YYYY')}</Descriptions.Item>}
          </Descriptions>
        </Card>
      )}
    </div>
  );
}

export default function ReportDetailPage() {
  return (
    <ReportProvider>
      <ReportDetailContent />
    </ReportProvider>
  );
}
