'use client';

import { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  Modal,
  Space,
  Statistic,
  Row,
  Col,
  Table,
  Tag,
  Typography,
  Upload,
  message,
} from 'antd';
import { DownloadOutlined, InboxOutlined } from '@ant-design/icons';
import * as XLSX from 'xlsx';
import {
  StudentProvider,
  useStudentActions,
  useStudentState,
} from '@/providers/academic/students';
import { useMarkActions } from '@/providers/assessment/marks';
import type { IAssessment, IStudentMark } from '@/providers/assessment/shared/interfaces';

const { Text, Paragraph } = Typography;

// EI-001 limits.
const MAX_ROWS = 1000;
const MAX_FILE_BYTES = 10 * 1024 * 1024; // 10 MB
const TEMPLATE_HEADERS = ['StudentIdNumber', 'SubjectCode', 'AssessmentType', 'Mark', 'MaxMark'] as const;

const ASSESSMENT_TYPE_LABEL: Record<number, string> = {
  1: 'Placement',
  2: 'Diagnostic',
  3: 'Readiness',
  4: 'Language Proficiency',
  5: 'Mathematics',
  6: 'General',
};

interface ParsedRow {
  rowNumber: number; // spreadsheet row number as the teacher sees it (header = row 1)
  studentIdNumber: string;
  markRaw: string;
  mark: number | null;
  maxMark: string;
  studentId?: string;
  studentName?: string;
  valid: boolean;
  reason?: string;
}

interface ImportMarksModalProps {
  open: boolean;
  onClose: (refresh: boolean) => void;
  assessment: IAssessment;
  // The set of students enrolled in this assessment's class (source of
  // truth from the capture grid) and those who already have a mark.
  enrolledStudentIds: Set<string>;
  recordedStudentIds: Set<string>;
}

function ImportMarksContent({
  open,
  onClose,
  assessment,
  enrolledStudentIds,
  recordedStudentIds,
}: ImportMarksModalProps) {
  const { getAllAsync: getAllStudents } = useStudentActions();
  const { students } = useStudentState();
  const { bulkRecordMarksAsync } = useMarkActions();

  const [parsed, setParsed] = useState<ParsedRow[] | null>(null);
  const [skipped, setSkipped] = useState(0);
  const [fileName, setFileName] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (open) {
      setParsed(null);
      setSkipped(0);
      setFileName(null);
      // No server-side class filter exists on Student/GetAll, so we fetch
      // active students and intersect with the grid's enrolled set below.
      // NOTE: capped at 2000 — a tenant with more active students than that
      // would need a class-scoped endpoint (tracked as a follow-up).
      getAllStudents({ isActive: true, maxResultCount: 2000 });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  // admissionNumber -> student, restricted to students actually enrolled in
  // this class (per the capture grid), not merely those whose currentClassId
  // points here — the two can diverge for transfers/promotions.
  const studentsByAdmission = useMemo(() => {
    const map = new Map<string, { id: string; name: string }>();
    (students ?? [])
      .filter((s) => enrolledStudentIds.has(s.id) && s.admissionNumber)
      .forEach((s) => map.set(s.admissionNumber!.trim(), { id: s.id, name: s.fullName ?? 'Student' }));
    return map;
  }, [students, enrolledStudentIds]);

  const downloadTemplate = () => {
    const rows = Array.from(studentsByAdmission.entries()).map(([adm, s]) => [
      adm,
      assessment.subjectName ?? '',
      ASSESSMENT_TYPE_LABEL[assessment.assessmentType] ?? '',
      '',
      assessment.maxMarks,
    ]);
    const aoa = [TEMPLATE_HEADERS as unknown as string[], ...rows];
    const ws = XLSX.utils.aoa_to_sheet(aoa);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Marks');
    XLSX.writeFile(wb, `marks-template-${assessment.name.replace(/[^\w]+/g, '-')}.xlsx`);
  };

  const parseFile = async (file: File) => {
    if (file.size > MAX_FILE_BYTES) {
      message.error('File exceeds the 10 MB limit.');
      return;
    }
    try {
      const buf = await file.arrayBuffer();
      const wb = XLSX.read(buf, { type: 'array' });
      const sheet = wb.Sheets[wb.SheetNames[0]];
      const json = XLSX.utils.sheet_to_json<Record<string, unknown>>(sheet, { defval: '' });

      if (json.length > MAX_ROWS) {
        message.error(`File has ${json.length} rows — the maximum is ${MAX_ROWS}.`);
        return;
      }

      const seen = new Set<string>();
      const rows: ParsedRow[] = [];
      let skippedCount = 0;

      json.forEach((raw, i) => {
        const studentIdNumber = String(raw.StudentIdNumber ?? '').trim();
        const markRaw = String(raw.Mark ?? '').trim();
        const maxMark = String(raw.MaxMark ?? '').trim();

        // Skip unfilled template rows (no mark entered) — counted, not failed.
        if (markRaw === '') {
          skippedCount += 1;
          return;
        }

        const row: ParsedRow = {
          rowNumber: i + 2, // the header occupies spreadsheet row 1
          studentIdNumber,
          markRaw,
          mark: null,
          maxMark,
          valid: true,
        };

        const student = studentsByAdmission.get(studentIdNumber);
        // Strict numeric check: reject "1e2", "0x10", stray text, etc.
        const isNumeric = /^-?\d+(\.\d+)?$/.test(markRaw);
        const markNum = Number(markRaw);

        if (!studentIdNumber) {
          row.valid = false;
          row.reason = 'Missing StudentIdNumber';
        } else if (!student) {
          row.valid = false;
          row.reason = 'Student not enrolled in this class';
        } else if (seen.has(studentIdNumber)) {
          row.valid = false;
          row.reason = 'Duplicate StudentIdNumber in file';
        } else if (recordedStudentIds.has(student.id)) {
          // BulkRecordMarks is create-only and rejects the whole batch on a
          // duplicate, so flag already-recorded students here instead.
          row.valid = false;
          row.reason = 'Mark already captured — clear it first';
        } else if (!isNumeric) {
          row.valid = false;
          row.reason = 'Mark is not a number';
        } else if (markNum < 0 || markNum > assessment.maxMarks) {
          row.valid = false;
          row.reason = `Mark must be 0–${assessment.maxMarks}`;
        } else if (maxMark !== '' && Number(maxMark) !== assessment.maxMarks) {
          row.valid = false;
          row.reason = `MaxMark must equal ${assessment.maxMarks}`;
        } else {
          row.studentId = student.id;
          row.studentName = student.name;
          row.mark = markNum;
        }

        if (studentIdNumber) seen.add(studentIdNumber);
        rows.push(row);
      });

      setFileName(file.name);
      setSkipped(skippedCount);
      setParsed(rows);
    } catch (e) {
      console.error(e);
      message.error('Could not read the file. Make sure it is a valid .xlsx / .xls / .csv.');
    }
  };

  const summary = useMemo(() => {
    const total = parsed?.length ?? 0;
    const invalid = parsed?.filter((r) => !r.valid).length ?? 0;
    return { total, valid: total - invalid, invalid };
  }, [parsed]);

  const downloadErrorReport = () => {
    const invalidRows = (parsed ?? []).filter((r) => !r.valid);
    const aoa = [
      ['Row', 'StudentIdNumber', 'Mark', 'Reason'],
      ...invalidRows.map((r) => [r.rowNumber, r.studentIdNumber, r.markRaw, r.reason ?? '']),
    ];
    const ws = XLSX.utils.aoa_to_sheet(aoa);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Errors');
    XLSX.writeFile(wb, 'mark-import-errors.xlsx');
  };

  const handleImport = async () => {
    if (!parsed || summary.invalid > 0 || summary.valid === 0) return;
    const studentMarks: IStudentMark[] = parsed
      .filter((r) => r.valid && r.studentId)
      .map((r) => ({ studentId: r.studentId!, rawMark: r.mark ?? undefined, wasAbsent: false }));

    setSubmitting(true);
    try {
      await bulkRecordMarksAsync({ assessmentId: assessment.id, studentMarks });
      message.success(`Imported ${studentMarks.length} mark${studentMarks.length === 1 ? '' : 's'}`);
      onClose(true);
    } catch {
      // axios interceptor surfaces server errors (e.g. a student already marked)
    } finally {
      setSubmitting(false);
    }
  };

  const previewColumns = [
    { title: 'Row', dataIndex: 'rowNumber', key: 'rowNumber', width: 60 },
    { title: 'StudentIdNumber', dataIndex: 'studentIdNumber', key: 'studentIdNumber' },
    { title: 'Student', dataIndex: 'studentName', key: 'studentName', render: (v?: string) => v ?? '—' },
    { title: 'Mark', dataIndex: 'mark', key: 'mark', width: 80, render: (v: number | null) => (v ?? '—') },
    {
      title: 'Status',
      key: 'status',
      width: 220,
      render: (_: unknown, r: ParsedRow) =>
        r.valid ? <Tag color="green">Valid</Tag> : <Tag color="red">{r.reason}</Tag>,
    },
  ];

  const canImport = !!parsed && summary.invalid === 0 && summary.valid > 0;

  return (
    <Modal
      open={open}
      title="Import marks from Excel"
      onCancel={() => onClose(false)}
      width={760}
      destroyOnHidden
      footer={[
        <Button key="cancel" onClick={() => onClose(false)}>
          Cancel
        </Button>,
        <Button
          key="import"
          type="primary"
          disabled={!canImport}
          loading={submitting}
          onClick={handleImport}
        >
          Import {summary.valid > 0 ? `${summary.valid} mark${summary.valid === 1 ? '' : 's'}` : ''}
        </Button>,
      ]}
    >
      <Paragraph type="secondary" style={{ fontSize: 13 }}>
        1. Download the template (one row per enrolled student). 2. Fill in the{' '}
        <Text code>Mark</Text> column. 3. Upload to preview and validate. 4. Import — every
        row must be valid (EI-001..004).
      </Paragraph>

      <Space style={{ marginBottom: 12 }} wrap>
        <Button icon={<DownloadOutlined />} onClick={downloadTemplate}>
          Download template
        </Button>
        <Text type="secondary" style={{ fontSize: 12 }}>
          Headers: {TEMPLATE_HEADERS.join(', ')} · Max {assessment.maxMarks} marks · ≤ {MAX_ROWS} rows · ≤ 10 MB
        </Text>
      </Space>

      <Upload.Dragger
        multiple={false}
        maxCount={1}
        accept=".xlsx,.xls,.csv"
        showUploadList={false}
        beforeUpload={(file) => {
          parseFile(file as unknown as File);
          return false;
        }}
        style={{ marginBottom: 12 }}
      >
        <p className="ant-upload-drag-icon">
          <InboxOutlined />
        </p>
        <p className="ant-upload-text">Click or drag the filled template here</p>
        <p className="ant-upload-hint" style={{ fontSize: 12 }}>
          .xlsx / .xls / .csv {fileName ? `· loaded: ${fileName}` : ''}
        </p>
      </Upload.Dragger>

      {parsed && (
        <>
          <Row gutter={12} style={{ marginBottom: 12 }}>
            <Col span={8}>
              <Statistic title="Rows" value={summary.total} />
            </Col>
            <Col span={8}>
              <Statistic title="Valid" value={summary.valid} valueStyle={{ color: '#52C41A' }} />
            </Col>
            <Col span={8}>
              <Statistic title="Invalid" value={summary.invalid} valueStyle={{ color: summary.invalid ? '#cf1322' : undefined }} />
            </Col>
          </Row>

          {skipped > 0 && (
            <Text type="secondary" style={{ display: 'block', marginBottom: 8, fontSize: 12 }}>
              {skipped} row(s) skipped — no mark entered.
            </Text>
          )}

          {summary.invalid > 0 && (
            <Alert
              type="error"
              showIcon
              style={{ marginBottom: 12 }}
              message={`${summary.invalid} row(s) are invalid — fix them and re-upload. No marks are imported until every row is valid.`}
              action={
                <Button size="small" onClick={downloadErrorReport}>
                  Error report
                </Button>
              }
            />
          )}

          {summary.total === 0 && (
            <Alert type="warning" showIcon style={{ marginBottom: 12 }} message="No mark rows found in the file." />
          )}

          <Text type="secondary" style={{ fontSize: 12 }}>
            Preview (first 10 rows)
          </Text>
          <Table<ParsedRow>
            dataSource={parsed.slice(0, 10)}
            columns={previewColumns}
            rowKey="rowNumber"
            size="small"
            pagination={false}
            style={{ marginTop: 8 }}
          />
        </>
      )}
    </Modal>
  );
}

export const ImportMarksModal: React.FC<ImportMarksModalProps> = (props) => (
  <StudentProvider>
    <ImportMarksContent {...props} />
  </StudentProvider>
);
