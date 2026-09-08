'use client';

import React, { useEffect, useMemo } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { usePortalBase } from '@/utils/portal-base';
import {
  Card,
  Descriptions,
  Tag,
  Tabs,
  Table,
  Space,
  Button,
  Typography,
  Spin,
  Empty,
  Statistic,
  Row,
  Col,
  Progress,
} from 'antd';
import {
  ArrowLeftOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined,
  MedicineBoxOutlined,
  CalendarOutlined,
  FileTextOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import {
  AttendanceProvider,
  useAttendanceState,
  useAttendanceActions,
} from '@/providers/academic/attendances';
import type { IAttendanceList } from '@/providers/academic/shared/interfaces';

const { Title, Text } = Typography;

const statusMap: Record<number, { label: string; color: string; icon: React.ReactNode }> = {
  1: { label: 'Present', color: 'green', icon: <CheckCircleOutlined /> },
  2: { label: 'Absent', color: 'red', icon: <CloseCircleOutlined /> },
  3: { label: 'Late', color: 'orange', icon: <ClockCircleOutlined /> },
  4: { label: 'Excused', color: 'blue', icon: <CalendarOutlined /> },
  5: { label: 'Sick Leave', color: 'purple', icon: <MedicineBoxOutlined /> },
};

// ─── Record Details ─────────────────────────────────────────────
function RecordDetails({ attendance }: { attendance: NonNullable<ReturnType<typeof useAttendanceState>['attendance']> }) {
  const statusInfo = statusMap[attendance.status] ?? { label: 'Unknown', color: 'default', icon: null };

  return (
    <Card size="small">
      <Descriptions column={{ xs: 1, sm: 2 }} size="small" bordered>
        <Descriptions.Item label="Student">{attendance.studentName ?? 'N/A'}</Descriptions.Item>
        <Descriptions.Item label="Class">{attendance.className ?? 'N/A'}</Descriptions.Item>
        <Descriptions.Item label="Date">{dayjs(attendance.attendanceDate).format('DD MMM YYYY')}</Descriptions.Item>
        <Descriptions.Item label="Status">
          <Tag color={statusInfo.color} icon={statusInfo.icon}>{statusInfo.label}</Tag>
        </Descriptions.Item>
        <Descriptions.Item label="Teacher">{attendance.teacherName ?? 'N/A'}</Descriptions.Item>
        <Descriptions.Item label="Subject">{attendance.subjectName ?? 'N/A'}</Descriptions.Item>
        <Descriptions.Item label="Notes" span={2}>{attendance.notes || 'No notes'}</Descriptions.Item>
        <Descriptions.Item label="Recorded">{dayjs(attendance.creationTime).format('DD MMM YYYY HH:mm')}</Descriptions.Item>
      </Descriptions>
    </Card>
  );
}

// ─── Student History Tab ────────────────────────────────────────
function StudentHistorySection({ studentId }: { studentId: string }) {
  const { attendances, isPending } = useAttendanceState();
  const { getByStudentAsync } = useAttendanceActions();

  useEffect(() => {
    const start = dayjs().startOf('year').format('YYYY-MM-DD');
    const end = dayjs().format('YYYY-MM-DD');
    getByStudentAsync(studentId, start, end);
  }, [studentId, getByStudentAsync]);

  const columns = [
    {
      title: 'Date', dataIndex: 'attendanceDate', key: 'date',
      render: (d: string) => dayjs(d).format('DD MMM YYYY'),
      sorter: (a: IAttendanceList, b: IAttendanceList) => dayjs(a.attendanceDate).unix() - dayjs(b.attendanceDate).unix(),
      defaultSortOrder: 'descend' as const,
    },
    { title: 'Class', dataIndex: 'className', key: 'className' },
    {
      title: 'Status', dataIndex: 'status', key: 'status',
      render: (s: number) => {
        const info = statusMap[s] ?? { label: 'Unknown', color: 'default', icon: null };
        return <Tag color={info.color} icon={info.icon}>{info.label}</Tag>;
      },
      filters: Object.entries(statusMap).map(([k, v]) => ({ text: v.label, value: Number(k) })),
      onFilter: (value: unknown, record: IAttendanceList) => record.status === value,
    },
    { title: 'Notes', dataIndex: 'notes', key: 'notes', render: (n: string) => n || '—' },
  ];

  return (
    <Table<IAttendanceList>
      dataSource={attendances ?? []}
      columns={columns}
      rowKey="id"
      loading={isPending}
      pagination={{ pageSize: 15, size: 'small', showSizeChanger: true }}
      size="small"
      locale={{ emptyText: <Empty description="No attendance records found" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
    />
  );
}

// ─── Summary Tab ────────────────────────────────────────────────
function SummarySection({ studentId }: { studentId: string }) {
  const { attendanceSummary, isPending } = useAttendanceState();
  const { getStudentSummaryAsync } = useAttendanceActions();

  useEffect(() => {
    const start = dayjs().startOf('year').format('YYYY-MM-DD');
    const end = dayjs().format('YYYY-MM-DD');
    getStudentSummaryAsync(studentId, start, end);
  }, [studentId, getStudentSummaryAsync]);

  if (isPending) return <Spin />;
  if (!attendanceSummary) return <Empty description="No summary data available" />;

  const s = attendanceSummary;
  const breakdownItems = [
    { label: 'Present', count: s.presentCount, color: '#52c41a' },
    { label: 'Absent', count: s.absentCount, color: '#ff4d4f' },
    { label: 'Late', count: s.lateCount, color: '#faad14' },
    { label: 'Excused', count: s.excusedCount, color: '#1890ff' },
    { label: 'Sick Leave', count: s.sickLeaveCount, color: '#722ed1' },
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      <Row gutter={16}>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic title="Attendance Rate" value={s.attendancePercentage} suffix="%" />
            <Progress
              percent={s.attendancePercentage}
              showInfo={false}
              status={s.attendancePercentage >= 80 ? 'success' : s.attendancePercentage >= 60 ? 'normal' : 'exception'}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small"><Statistic title="Total Days" value={s.totalDays} /></Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic title="Present" value={s.presentCount} valueStyle={{ color: '#3f8600' }} prefix={<CheckCircleOutlined />} />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic title="Absent" value={s.absentCount} valueStyle={{ color: s.absentCount > 0 ? '#cf1322' : undefined }} prefix={<CloseCircleOutlined />} />
          </Card>
        </Col>
      </Row>

      <Card title="Breakdown" size="small">
        {breakdownItems.map(item => {
          const pct = s.totalDays > 0 ? Math.round((item.count / s.totalDays) * 100) : 0;
          return (
            <div key={item.label} style={{ marginBottom: 12 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                <Text>{item.label}</Text>
                <Text strong>{item.count} ({pct}%)</Text>
              </div>
              <Progress percent={pct} showInfo={false} strokeColor={item.color} size="small" />
            </div>
          );
        })}
      </Card>
    </div>
  );
}

// ─── Main Content ───────────────────────────────────────────────
function AttendanceDetailContent() {
  const params = useParams();
  const router = useRouter();
  const portalBase = usePortalBase();
  const attendanceId = params.id as string;

  const { attendance, isPending, isError } = useAttendanceState();
  const { getAsync } = useAttendanceActions();

  useEffect(() => {
    if (attendanceId) getAsync(attendanceId);
  }, [attendanceId, getAsync]);

  if (isPending && !attendance) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 80 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (isError || (!isPending && !attendance)) {
    return (
      <div style={{ padding: 24 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => router.push(`${portalBase}/attendance`)} style={{ marginBottom: 16 }}>
          Back to Attendance
        </Button>
        <Empty description="Attendance record not found" />
      </div>
    );
  }

  if (!attendance) return null;

  const statusInfo = statusMap[attendance.status] ?? { label: 'Unknown', color: 'default', icon: null };

  const tabItems = [
    {
      key: 'details',
      label: <span><FileTextOutlined /> Record Details</span>,
      children: <RecordDetails attendance={attendance} />,
    },
    {
      key: 'history',
      label: <span><CalendarOutlined /> Student History</span>,
      children: <StudentHistorySection studentId={attendance.studentId} />,
    },
    {
      key: 'summary',
      label: <span><CheckCircleOutlined /> Summary</span>,
      children: <SummarySection studentId={attendance.studentId} />,
    },
  ];

  return (
    <div style={{ padding: 24 }}>
      <Button
        icon={<ArrowLeftOutlined />}
        onClick={() => router.push(`${portalBase}/attendance`)}
        style={{ marginBottom: 16 }}
      >
        Back to Attendance
      </Button>

      <Card style={{ marginBottom: 24 }}>
        <Row gutter={24} align="middle">
          <Col>
            <Tag color={statusInfo.color} icon={statusInfo.icon} style={{ fontSize: 16, padding: '4px 12px' }}>
              {statusInfo.label}
            </Tag>
          </Col>
          <Col>
            <div>
              <Title level={3} style={{ margin: 0 }}>{attendance.studentName}</Title>
              <Space size="large" style={{ marginTop: 8 }}>
                <Text type="secondary">Class: {attendance.className ?? 'N/A'}</Text>
                <Text type="secondary">Date: {dayjs(attendance.attendanceDate).format('DD MMM YYYY')}</Text>
                <Text type="secondary">Teacher: {attendance.teacherName ?? 'N/A'}</Text>
              </Space>
            </div>
          </Col>
        </Row>
      </Card>

      <Card>
        <Tabs items={tabItems} defaultActiveKey="details" />
      </Card>
    </div>
  );
}

// ─── Wrapped with provider ──────────────────────────────────────
export default function AttendanceDetailPage() {
  return (
    <AttendanceProvider>
      <AttendanceDetailContent />
    </AttendanceProvider>
  );
}
