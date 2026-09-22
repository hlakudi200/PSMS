'use client';

/**
 * RC-02. Generating report cards.
 *
 * Until this screen existed there was no way to create a report card from the
 * application at all — `Report/Generate` was only reachable by calling the API
 * directly, which is why the reports list is empty on most tenants.
 *
 * The flow is preview-then-commit rather than a single button: a class is forty
 * learners, and the actor needs to see who is blocked by an outstanding mark
 * before they commit, not afterwards.
 */

import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import { usePortalBase } from '@/utils/portal-base';
import {
  Alert,
  Button,
  Card,
  Col,
  Empty,
  Form,
  InputNumber,
  Row,
  Select,
  Space,
  Statistic,
  Switch,
  Table,
  Tag,
  Tooltip,
  Typography,
  message,
} from 'antd';
import {
  ArrowLeftOutlined,
  CheckCircleOutlined,
  EyeOutlined,
  FileAddOutlined,
} from '@ant-design/icons';
import { z } from 'zod';
import {
  ReportProvider,
  useReportState,
  useReportActions,
} from '@/providers/assessment/reports';
import {
  AcademicYearProvider,
  useAcademicYearState,
  useAcademicYearActions,
} from '@/providers/academic/academic_years';
import { TermProvider, useTermState, useTermActions } from '@/providers/academic/terms';
import { ClassProvider, useClassState, useClassActions } from '@/providers/academic/classes';
import {
  BulkGenerateOutcome,
  type IBulkGenerateReportItem,
} from '@/providers/assessment/shared/interfaces';
import { ReportType, reportTypeLabels } from '@/providers/shared/enums';

const { Title, Text, Paragraph } = Typography;

/**
 * Report types that describe a single term, and so cannot be generated without
 * one. Mid-year and year-end span the whole year and carry no TermId.
 */
const TERM_SCOPED_TYPES: number[] = [
  ReportType.Term1,
  ReportType.Term2,
  ReportType.Term3,
  ReportType.Term4,
  ReportType.Progress,
];

const YEAR_SCOPED_TYPES: number[] = [ReportType.MidYear, ReportType.YearEnd];

const outcomeMeta: Record<BulkGenerateOutcome, { label: string; color: string }> = {
  [BulkGenerateOutcome.Eligible]: { label: 'Will generate', color: 'blue' },
  [BulkGenerateOutcome.Generated]: { label: 'Generated', color: 'green' },
  [BulkGenerateOutcome.SkippedExisting]: { label: 'Already has one', color: 'default' },
  [BulkGenerateOutcome.Blocked]: { label: 'Blocked', color: 'orange' },
  [BulkGenerateOutcome.Failed]: { label: 'Failed', color: 'red' },
};

const generateSchema = z
  .object({
    academicYearId: z.string().min(1, 'Select an academic year'),
    reportType: z.number({ error: 'Select a report type' }).int().min(1).max(7),
    termId: z.string().optional(),
    classId: z.string().min(1, 'Select a class'),
    useAttendanceRecords: z.boolean(),
    defaultDaysPresent: z.number().int().min(0).max(365),
    defaultDaysAbsent: z.number().int().min(0).max(365),
    defaultDaysLate: z.number().int().min(0).max(365),
  })
  .refine((d) => !TERM_SCOPED_TYPES.includes(d.reportType) || !!d.termId, {
    message: 'A term report needs a term',
    path: ['termId'],
  });

type GenerateFormValues = z.infer<typeof generateSchema>;

function GenerateReportsContent() {
  const router = useRouter();
  const portalBase = usePortalBase();
  const [form] = Form.useForm();

  const { bulkPreview, bulkResult, isPending } = useReportState();
  const { previewBulkGenerateAsync, bulkGenerateAsync } = useReportActions();
  const { academicYears } = useAcademicYearState();
  const { getAllAsync: getAllAcademicYears } = useAcademicYearActions();
  const { terms } = useTermState();
  const { getByAcademicYearAsync: getTermsByYear } = useTermActions();
  const { classes } = useClassState();
  const { getClassesByAcademicYearAsync } = useClassActions();

  const [academicYearId, setAcademicYearId] = useState<string | undefined>();
  const [reportType, setReportType] = useState<number | undefined>();
  const [selectedStudentIds, setSelectedStudentIds] = useState<React.Key[]>([]);
  const [previewing, setPreviewing] = useState(false);
  const [generating, setGenerating] = useState(false);
  /** The preview only describes the selection it was taken for. */
  const [previewStale, setPreviewStale] = useState(false);

  useEffect(() => {
    getAllAcademicYears({ maxResultCount: 50 });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (!academicYearId) return;
    getTermsByYear(academicYearId);
    getClassesByAcademicYearAsync(academicYearId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [academicYearId]);

  const needsTerm = reportType !== undefined && TERM_SCOPED_TYPES.includes(reportType);
  const isYearScoped = reportType !== undefined && YEAR_SCOPED_TYPES.includes(reportType);

  const academicYearOptions = useMemo(
    () => (academicYears ?? []).map((y) => ({ value: y.id, label: y.yearName })),
    [academicYears]
  );

  const termOptions = useMemo(
    () => (terms ?? []).map((t) => ({ value: t.id, label: t.termName })),
    [terms]
  );

  const classOptions = useMemo(
    () =>
      (classes ?? [])
        .filter((c) => c.isActive)
        .map((c) => ({
          value: c.id,
          label: `${c.className} · ${c.gradeName} · ${c.studentCount} learners`,
        })),
    [classes]
  );

  const reportTypeOptions = useMemo(
    () =>
      Object.entries(reportTypeLabels).map(([value, label]) => ({
        value: Number(value),
        label,
      })),
    []
  );

  /** Shared by preview and generate so the two always read the form the same way. */
  const parseForm = (): GenerateFormValues | null => {
    const values = form.getFieldsValue();
    const parsed = generateSchema.safeParse({
      academicYearId: values.academicYearId ?? '',
      reportType: values.reportType,
      termId: values.termId,
      classId: values.classId ?? '',
      useAttendanceRecords: values.useAttendanceRecords ?? true,
      defaultDaysPresent: values.defaultDaysPresent ?? 0,
      defaultDaysAbsent: values.defaultDaysAbsent ?? 0,
      defaultDaysLate: values.defaultDaysLate ?? 0,
    });

    if (!parsed.success) {
      form.setFields(
        parsed.error.issues.map((issue) => ({
          name: issue.path as string[],
          errors: [issue.message],
        }))
      );
      return null;
    }
    return parsed.data;
  };

  const buildInput = (data: GenerateFormValues, studentIds?: string[]) => ({
    classId: data.classId,
    academicYearId: data.academicYearId,
    reportType: data.reportType,
    // A year-scoped report carries no term even if one is still sitting in the form.
    termId: TERM_SCOPED_TYPES.includes(data.reportType) ? data.termId : undefined,
    useAttendanceRecords: data.useAttendanceRecords,
    defaultDaysPresent: data.defaultDaysPresent,
    defaultDaysAbsent: data.defaultDaysAbsent,
    defaultDaysLate: data.defaultDaysLate,
    studentIds,
  });

  const handlePreview = async () => {
    const data = parseForm();
    if (!data) return;

    setPreviewing(true);
    try {
      await previewBulkGenerateAsync(buildInput(data));
      setPreviewStale(false);
    } catch {
      // Surfaced by the axios error interceptor.
    } finally {
      setPreviewing(false);
    }
  };

  const handleGenerate = async () => {
    const data = parseForm();
    if (!data) return;
    if (selectedStudentIds.length === 0) {
      message.warning('Select at least one learner.');
      return;
    }

    setGenerating(true);
    try {
      await bulkGenerateAsync(buildInput(data, selectedStudentIds as string[]));
    } catch {
      // Surfaced by the axios error interceptor.
    } finally {
      setGenerating(false);
    }
  };

  /** Any change to the selection invalidates the preview that was taken for it. */
  const invalidatePreview = useCallback(() => setPreviewStale(true), []);

  // Default the selection to everyone the preview says is eligible — the common
  // case is "generate for the whole class".
  useEffect(() => {
    if (!bulkPreview) return;
    setSelectedStudentIds(
      bulkPreview.items
        .filter((i) => i.outcome === BulkGenerateOutcome.Eligible)
        .map((i) => i.studentId)
    );
  }, [bulkPreview]);

  useEffect(() => {
    if (bulkResult) {
      message.success(
        `${bulkResult.generatedCount} report card${bulkResult.generatedCount === 1 ? '' : 's'} generated.`
      );
    }
  }, [bulkResult]);

  const itemColumns = [
    {
      title: 'Learner',
      dataIndex: 'studentName',
      key: 'studentName',
      render: (name: string) => <Text strong>{name}</Text>,
    },
    { title: 'Adm #', dataIndex: 'admissionNumber', key: 'admissionNumber', width: 110 },
    {
      title: 'Outcome',
      dataIndex: 'outcome',
      key: 'outcome',
      width: 150,
      render: (outcome: BulkGenerateOutcome) => {
        const meta = outcomeMeta[outcome];
        return <Tag color={meta.color}>{meta.label}</Tag>;
      },
    },
    {
      title: 'Detail',
      dataIndex: 'message',
      key: 'message',
      render: (text: string, record: IBulkGenerateReportItem) =>
        record.outcome === BulkGenerateOutcome.Generated && record.reportId ? (
          <Button
            type="link"
            size="small"
            style={{ paddingLeft: 0 }}
            onClick={() => router.push(`${portalBase}/reports/${record.reportId}`)}
          >
            Open report card
          </Button>
        ) : (
          <Text type="secondary">{text ?? '—'}</Text>
        ),
    },
  ];

  const showResult = !!bulkResult;
  const active = showResult ? bulkResult : bulkPreview;

  return (
    <div style={{ padding: 24 }}>
      <Space direction="vertical" size={16} style={{ width: '100%' }}>
        <div>
          <Button
            type="link"
            icon={<ArrowLeftOutlined />}
            style={{ paddingLeft: 0 }}
            onClick={() => router.push(`${portalBase}/reports`)}
          >
            Back to report cards
          </Button>
          <Title level={3} style={{ margin: 0 }}>
            Generate report cards
          </Title>
          <Text type="secondary">
            Pick a class and a term, preview who is included, then generate.
          </Text>
        </div>

        <Card title="1 · What to generate">
          <Form
            form={form}
            layout="vertical"
            initialValues={{
              useAttendanceRecords: true,
              defaultDaysPresent: 0,
              defaultDaysAbsent: 0,
              defaultDaysLate: 0,
            }}
            onValuesChange={invalidatePreview}
          >
            <Row gutter={16}>
              <Col xs={24} md={6}>
                <Form.Item label="Academic year" name="academicYearId" rules={[{ required: true }]}>
                  <Select
                    placeholder="Select year"
                    options={academicYearOptions}
                    onChange={(value) => {
                      setAcademicYearId(value);
                      form.setFieldsValue({ termId: undefined, classId: undefined });
                    }}
                  />
                </Form.Item>
              </Col>
              <Col xs={24} md={6}>
                <Form.Item label="Report type" name="reportType" rules={[{ required: true }]}>
                  <Select
                    placeholder="Select type"
                    options={reportTypeOptions}
                    onChange={(value) => setReportType(value)}
                  />
                </Form.Item>
              </Col>
              <Col xs={24} md={6}>
                <Form.Item
                  label="Term"
                  name="termId"
                  rules={[{ required: needsTerm }]}
                  tooltip={
                    isYearScoped
                      ? 'Mid-year and year-end reports span the whole year and carry no term.'
                      : undefined
                  }
                >
                  <Select
                    placeholder={isYearScoped ? 'Not applicable' : 'Select term'}
                    options={termOptions}
                    disabled={!academicYearId || isYearScoped}
                    allowClear
                  />
                </Form.Item>
              </Col>
              <Col xs={24} md={6}>
                <Form.Item label="Class" name="classId" rules={[{ required: true }]}>
                  <Select
                    placeholder={academicYearId ? 'Select class' : 'Pick a year first'}
                    options={classOptions}
                    disabled={!academicYearId}
                    showSearch
                    optionFilterProp="label"
                  />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item
              label="Attendance"
              name="useAttendanceRecords"
              valuePropName="checked"
              tooltip="Reads each learner's attendance from the register over the term's dates, rather than putting one typed-in figure on every report card."
              style={{ marginBottom: 8 }}
            >
              <Switch
                checkedChildren="From the register"
                unCheckedChildren="Enter manually"
                disabled={!needsTerm}
              />
            </Form.Item>

            <Form.Item noStyle shouldUpdate>
              {() => {
                const manual = !form.getFieldValue('useAttendanceRecords') || !needsTerm;
                if (!manual) {
                  return (
                    <Text type="secondary">
                      Each learner&rsquo;s days present, absent and late come from the register.
                    </Text>
                  );
                }
                return (
                  <Row gutter={16}>
                    <Col xs={8} md={4}>
                      <Form.Item label="Days present" name="defaultDaysPresent">
                        <InputNumber min={0} max={365} style={{ width: '100%' }} />
                      </Form.Item>
                    </Col>
                    <Col xs={8} md={4}>
                      <Form.Item label="Days absent" name="defaultDaysAbsent">
                        <InputNumber min={0} max={365} style={{ width: '100%' }} />
                      </Form.Item>
                    </Col>
                    <Col xs={8} md={4}>
                      <Form.Item label="Days late" name="defaultDaysLate">
                        <InputNumber min={0} max={365} style={{ width: '100%' }} />
                      </Form.Item>
                    </Col>
                    <Col xs={24} md={12}>
                      <Text type="secondary">
                        These days are written to every report card in the run. Prefer the register
                        where a term is selected.
                      </Text>
                    </Col>
                  </Row>
                );
              }}
            </Form.Item>

            {isYearScoped && (
              <Alert
                type="warning"
                showIcon
                style={{ marginTop: 8 }}
                message="Mid-year and year-end reports do not aggregate marks yet"
                description="Subjects will be created without marks, and the overall percentage will be empty. Tracked as issue #242 — generate term reports until it is fixed."
              />
            )}

            <Space style={{ marginTop: 16 }}>
              <Button
                type={bulkPreview && !previewStale ? 'default' : 'primary'}
                icon={<EyeOutlined />}
                onClick={handlePreview}
                loading={previewing}
              >
                Preview
              </Button>
              {bulkPreview && previewStale && (
                <Text type="warning">The selection changed — preview again.</Text>
              )}
            </Space>
          </Form>
        </Card>

        {active && (
          <Card
            title={showResult ? '3 · Result' : '2 · Review'}
            extra={
              showResult ? (
                <Button type="primary" onClick={() => router.push(`${portalBase}/reports`)}>
                  Go to report cards
                </Button>
              ) : (
                <Tooltip
                  title={
                    previewStale
                      ? 'Preview again before generating — the selection has changed.'
                      : undefined
                  }
                >
                  <Button
                    type="primary"
                    icon={<FileAddOutlined />}
                    onClick={handleGenerate}
                    loading={generating}
                    disabled={previewStale || selectedStudentIds.length === 0}
                  >
                    Generate {selectedStudentIds.length} report card
                    {selectedStudentIds.length === 1 ? '' : 's'}
                  </Button>
                </Tooltip>
              )
            }
          >
            <Paragraph type="secondary" style={{ marginTop: 0 }}>
              {active.className}
              {active.termName ? ` · ${active.termName}` : ''}
            </Paragraph>

            <Row gutter={16} style={{ marginBottom: 16 }}>
              <Col xs={12} md={6}>
                <Statistic title="Learners" value={active.totalStudents} />
              </Col>
              {showResult ? (
                <Col xs={12} md={6}>
                  <Statistic
                    title="Generated"
                    value={active.generatedCount}
                    valueStyle={{ color: '#3f8600' }}
                    prefix={<CheckCircleOutlined />}
                  />
                </Col>
              ) : (
                <Col xs={12} md={6}>
                  <Statistic
                    title="Eligible"
                    value={active.eligibleCount}
                    valueStyle={{ color: '#1677ff' }}
                  />
                </Col>
              )}
              <Col xs={12} md={6}>
                <Statistic title="Already have one" value={active.skippedCount} />
              </Col>
              <Col xs={12} md={6}>
                <Statistic
                  title={showResult ? 'Blocked or failed' : 'Blocked'}
                  value={active.blockedCount + active.failedCount}
                  valueStyle={
                    active.blockedCount + active.failedCount > 0 ? { color: '#cf1322' } : undefined
                  }
                />
              </Col>
            </Row>

            {active.blockedCount > 0 && !showResult && (
              <Alert
                type="info"
                showIcon
                style={{ marginBottom: 16 }}
                message={`${active.blockedCount} learner${active.blockedCount === 1 ? ' has' : 's have'} incomplete marks for this term`}
                description="Their marks need to be completed, marked absent or exempted before a report card can be generated. The rest of the class is unaffected."
              />
            )}

            <Table<IBulkGenerateReportItem>
              rowKey="studentId"
              dataSource={active.items}
              columns={itemColumns}
              size="small"
              pagination={false}
              scroll={{ x: 'max-content' }}
              locale={{ emptyText: <Empty description="No learners in this class" /> }}
              rowSelection={
                showResult
                  ? undefined
                  : {
                      selectedRowKeys: selectedStudentIds,
                      onChange: setSelectedStudentIds,
                      getCheckboxProps: (record) => ({
                        // Only an eligible learner can be generated for. The others
                        // are shown so the actor can see why they are not included.
                        disabled: record.outcome !== BulkGenerateOutcome.Eligible,
                      }),
                    }
              }
            />
          </Card>
        )}

        {!active && !isPending && (
          <Card>
            <Empty description="Choose a class and term above, then preview." />
          </Card>
        )}
      </Space>
    </div>
  );
}

export default function GenerateReportsPageContent() {
  return (
    <AcademicYearProvider>
      <TermProvider>
        <ClassProvider>
          <ReportProvider>
            <GenerateReportsContent />
          </ReportProvider>
        </ClassProvider>
      </TermProvider>
    </AcademicYearProvider>
  );
}
