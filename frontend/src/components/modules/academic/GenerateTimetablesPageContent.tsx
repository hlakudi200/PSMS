'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import {
  Alert,
  Button,
  Card,
  Checkbox,
  Col,
  DatePicker,
  Empty,
  Form,
  InputNumber,
  Row,
  Select,
  Space,
  Statistic,
  Table,
  Tag,
  TimePicker,
  Typography,
  message,
} from 'antd';
import { PlayCircleOutlined, CheckCircleOutlined, ArrowLeftOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { z } from 'zod';
import {
  TimetableProvider,
  useTimetableActions,
  useTimetableState,
} from '@/providers/academic/timetables';
import {
  AcademicYearProvider,
  useAcademicYearActions,
  useAcademicYearState,
} from '@/providers/academic/academic_years';
import type {
  IGeneratedClassTimetable,
  IUnplacedLesson,
} from '@/providers/academic/shared/interfaces';

const { Title, Text } = Typography;

const DATE_FORMAT = 'YYYY-MM-DD';
const TIME_FORMAT = 'HH:mm';

const DAY_OPTIONS = [
  { label: 'Mon', value: 1 },
  { label: 'Tue', value: 2 },
  { label: 'Wed', value: 3 },
  { label: 'Thu', value: 4 },
  { label: 'Fri', value: 5 },
  { label: 'Sat', value: 6 },
  { label: 'Sun', value: 0 },
];
const DEFAULT_WORKING_DAYS = [1, 2, 3, 4, 5];

const generateSchema = z.object({
  academicYearId: z.string().min(1, 'Please select an academic year'),
  effectiveDate: z.string().min(1, 'Please select an effective date'),
  workingDays: z.array(z.number()).min(1, 'Select at least one working day'),
  periodsPerDay: z.number().min(1).max(12),
  periodStartTime: z.string().min(1, 'Please select a period start time'),
  periodDurationMinutes: z.number().min(15).max(120),
  breakAfterPeriods: z.array(z.number()).default([]),
  breakDurationMinutes: z.number().min(0).max(120).default(30),
});

function GenerateTimetablesContent() {
  const [form] = Form.useForm();
  const router = useRouter();

  const { academicYears } = useAcademicYearState();
  const { getAllAsync: getAllAcademicYearsAsync } = useAcademicYearActions();

  const { isGenerating, generationResult } = useTimetableState();
  const { generateAsync, activateAsync } = useTimetableActions();

  const [activating, setActivating] = useState(false);

  useEffect(() => {
    getAllAcademicYearsAsync({ maxResultCount: 100 });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    const current = (academicYears ?? []).find((y) => y.isCurrent);
    if (current) {
      form.setFieldsValue({
        academicYearId: current.id,
        effectiveDate: dayjs(),
        workingDays: DEFAULT_WORKING_DAYS,
        periodsPerDay: 8,
        periodStartTime: dayjs('08:00', TIME_FORMAT),
        periodDurationMinutes: 40,
        breakAfterPeriods: [],
        breakDurationMinutes: 30,
      });
    }
  }, [academicYears]); // eslint-disable-line react-hooks/exhaustive-deps

  const academicYearOptions = (academicYears ?? []).map((y) => ({
    value: y.id,
    label: y.isCurrent ? `${y.yearName} (Current)` : y.yearName,
  }));

  // Break-after options depend on how many periods a day has. The last
  // period can't have a break after it (nothing follows), so it's excluded.
  const periodsPerDayWatch = Form.useWatch('periodsPerDay', form) ?? 8;
  const breakPeriodOptions = Array.from({ length: Math.max(0, periodsPerDayWatch - 1) }, (_, i) => ({
    value: i + 1,
    label: `After Period ${i + 1}`,
  }));

  const handleGenerate = async () => {
    try {
      const values = form.getFieldsValue();
      const parsed = generateSchema.safeParse({
        academicYearId: values.academicYearId,
        effectiveDate: values.effectiveDate ? dayjs(values.effectiveDate).format(DATE_FORMAT) : '',
        workingDays: values.workingDays ?? [],
        periodsPerDay: values.periodsPerDay,
        periodStartTime: values.periodStartTime ? dayjs(values.periodStartTime).format(TIME_FORMAT) : '',
        periodDurationMinutes: values.periodDurationMinutes,
        breakAfterPeriods: values.breakAfterPeriods ?? [],
        breakDurationMinutes: values.breakDurationMinutes ?? 30,
      });

      if (!parsed.success) {
        form.setFields(
          parsed.error.issues.map((err) => ({
            name: err.path as string[],
            errors: [err.message],
          }))
        );
        return;
      }

      await generateAsync({
        academicYearId: parsed.data.academicYearId,
        effectiveDate: parsed.data.effectiveDate,
        workingDays: parsed.data.workingDays,
        periodsPerDay: parsed.data.periodsPerDay,
        periodStartTime: `${parsed.data.periodStartTime}:00`,
        periodDurationMinutes: parsed.data.periodDurationMinutes,
        breakAfterPeriods: parsed.data.breakAfterPeriods,
        breakDurationMinutes: parsed.data.breakDurationMinutes,
      });
      message.success('Timetables generated — review the drafts below before activating.');
    } catch {
      // Surfaced by the axios error interceptor.
    }
  };

  const handleActivateAll = async () => {
    // timetableId is null for a class with nothing to activate (no subjects
    // requested, or every lesson failed to place) — only draft rows count.
    const generated = (generationResult?.classes ?? []).filter(
      (c): c is IGeneratedClassTimetable & { timetableId: string } => !!c.timetableId
    );
    if (generated.length === 0) return;

    setActivating(true);
    const results = await Promise.allSettled(
      generated.map((c) => activateAsync(c.timetableId))
    );
    setActivating(false);

    const ok = results.filter((r) => r.status === 'fulfilled').length;
    const fail = results.length - ok;
    if (fail === 0) {
      message.success(`${ok} timetable(s) activated`);
      router.push('/principal/timetables');
    } else {
      message.warning(`${ok} activated; ${fail} failed. Check the Timetables list.`);
    }
  };

  const classColumns = [
    { title: 'Class', dataIndex: 'className', key: 'className' },
    {
      title: 'Periods',
      key: 'periods',
      render: (_: unknown, r: IGeneratedClassTimetable) => `${r.slotsPlaced} / ${r.slotsRequested}`,
    },
    {
      title: 'Status',
      key: 'status',
      render: (_: unknown, r: IGeneratedClassTimetable) => {
        if (r.slotsRequested === 0) return <Tag>No subjects</Tag>;
        if (r.slotsPlaced === r.slotsRequested) return <Tag color="green">Complete</Tag>;
        if (r.slotsPlaced === 0) return <Tag color="red">Not placed</Tag>;
        return <Tag color="orange">Partial</Tag>;
      },
    },
  ];

  const unplacedColumns = [
    { title: 'Class', dataIndex: 'className', key: 'className', width: 140 },
    { title: 'Subject', dataIndex: 'subjectName', key: 'subjectName', width: 160 },
    {
      title: 'Teacher',
      key: 'teacherName',
      width: 160,
      render: (_: unknown, r: IUnplacedLesson) =>
        r.teacherName ?? <Text type="secondary">Not assigned</Text>,
    },
    {
      title: 'Why it could not be placed',
      dataIndex: 'reason',
      key: 'reason',
      render: (reason: string) => <Text type="secondary">{reason}</Text>,
    },
  ];

  return (
    <div>
      <Button
        type="link"
        icon={<ArrowLeftOutlined />}
        onClick={() => router.push('/principal/timetables')}
        style={{ paddingLeft: 0, marginBottom: 4 }}
      >
        Back to timetables
      </Button>

      <div style={{ marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>Generate timetables</Title>
        <Text type="secondary">
          Builds a draft timetable for every active class from each class&apos;s subject and teacher
          assignments. Nothing goes live until you review and activate.
        </Text>
      </div>

      <Card variant="borderless" style={{ marginBottom: 16 }}>
        <Form form={form} layout="vertical">
          <Row gutter={16}>
            <Col xs={24} md={8}>
              <Form.Item label="Academic Year" name="academicYearId" rules={[{ required: true }]}>
                <Select options={academicYearOptions} placeholder="Select academic year" />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item label="Effective Date" name="effectiveDate" rules={[{ required: true }]}>
                <DatePicker style={{ width: '100%' }} format={DATE_FORMAT} />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item label="Working Days" name="workingDays" rules={[{ required: true }]}>
                <Checkbox.Group options={DAY_OPTIONS} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col xs={24} md={8}>
              <Form.Item label="Periods per Day" name="periodsPerDay" rules={[{ required: true }]}>
                <InputNumber style={{ width: '100%' }} min={1} max={12} />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item label="Period Start Time" name="periodStartTime" rules={[{ required: true }]}>
                <TimePicker style={{ width: '100%' }} format={TIME_FORMAT} minuteStep={5} />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item label="Period Duration (minutes)" name="periodDurationMinutes" rules={[{ required: true }]}>
                <InputNumber style={{ width: '100%' }} min={15} max={120} step={5} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col xs={24} md={12}>
              <Form.Item
                label="Breaks"
                name="breakAfterPeriods"
                tooltip="e.g. pick after Period 3 and after Period 6 for periods-break-periods-break-periods."
              >
                <Select
                  mode="multiple"
                  options={breakPeriodOptions}
                  placeholder="No breaks — periods run back-to-back"
                  allowClear
                />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item label="Break Duration (minutes)" name="breakDurationMinutes">
                <InputNumber style={{ width: '100%' }} min={0} max={120} step={5} />
              </Form.Item>
            </Col>
          </Row>
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              gap: 16,
              flexWrap: 'wrap',
              borderTop: '1px solid #f0f0f0',
              paddingTop: 16,
            }}
          >
            <Text type="secondary" style={{ fontSize: 12 }}>
              Generating again replaces the drafts from the last run. Timetables you have already
              activated are left alone.
            </Text>
            <Button
              type="primary"
              icon={<PlayCircleOutlined />}
              onClick={handleGenerate}
              loading={isGenerating}
            >
              Generate
            </Button>
          </div>
        </Form>
      </Card>

      {generationResult && generationResult.message && (
        <Alert
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
          message="Nothing to generate"
          description={generationResult.message}
        />
      )}

      {generationResult && !generationResult.message && (
        <>
          <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
            <Col xs={24} sm={8}>
              <Card variant="borderless">
                <Statistic title="Classes processed" value={generationResult.totalClasses} />
              </Card>
            </Col>
            <Col xs={24} sm={8}>
              <Card variant="borderless">
                <Statistic
                  title="Periods placed"
                  value={generationResult.totalSlotsPlaced}
                  suffix={`/ ${generationResult.totalSlotsRequested}`}
                />
              </Card>
            </Col>
            <Col xs={24} sm={8}>
              <Card variant="borderless">
                <Statistic
                  title="Unplaced lessons"
                  value={generationResult.unplaced.length}
                  valueStyle={{ color: generationResult.unplaced.length > 0 ? '#cf1322' : undefined }}
                />
              </Card>
            </Col>
          </Row>

          <Card
            variant="borderless"
            title="Draft timetables"
            style={{ marginBottom: 16 }}
            extra={
              <Space>
                <Button onClick={() => router.push('/principal/timetables')}>
                  Review individually
                </Button>
                <Button
                  type="primary"
                  icon={<CheckCircleOutlined />}
                  onClick={handleActivateAll}
                  loading={activating}
                  disabled={!generationResult.classes.some((c) => c.timetableId)}
                >
                  Activate all drafts
                </Button>
              </Space>
            }
          >
            {generationResult.classes.length > 0 ? (
              <Table
                rowKey="classId"
                columns={classColumns}
                dataSource={generationResult.classes}
                pagination={false}
                size="small"
              />
            ) : (
              <Empty description="No classes had any subjects to schedule." />
            )}
          </Card>

          {generationResult.unplaced.length > 0 && (
            <Card variant="borderless" title="Unplaced lessons">
              <Alert
                type="warning"
                showIcon
                style={{ marginBottom: 12 }}
                message="These lessons need to be placed by hand"
                description="Open the class on the Timetables list, then pick a free period in its weekly grid. If the teacher has no room left, free a period elsewhere first."
              />
              <Table
                rowKey={(r) => `${r.classId}-${r.subjectId}`}
                columns={unplacedColumns}
                dataSource={generationResult.unplaced}
                pagination={false}
                size="small"
              />
            </Card>
          )}
        </>
      )}
    </div>
  );
}

export default function GenerateTimetablesPageContent() {
  return (
    <TimetableProvider>
      <AcademicYearProvider>
        <GenerateTimetablesContent />
      </AcademicYearProvider>
    </TimetableProvider>
  );
}
