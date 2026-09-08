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
  TeamOutlined,
  BookOutlined,
  BarChartOutlined,
  UserOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { ClassProvider, useClassState, useClassActions } from '@/providers/academic/classes';
import { StudentClassProvider, useStudentClassState, useStudentClassActions } from '@/providers/academic/student_classes';
import { ClassSubjectProvider, useClassSubjectState, useClassSubjectActions } from '@/providers/academic/class_subjects';
import { AttendanceProvider, useAttendanceState, useAttendanceActions } from '@/providers/academic/attendances';
import type { IStudentClassList, IClassSubjectList } from '@/providers/academic/shared/interfaces';

const { Title, Text } = Typography;

// ─── Students Roster Tab ───────────────────────────────────────
function RosterSection({ classId }: { classId: string }) {
  const router = useRouter();
  const portalBase = usePortalBase();
  const { studentClasses, isPending } = useStudentClassState();
  const { getByClassAsync } = useStudentClassActions();

  useEffect(() => {
    getByClassAsync(classId);
  }, [classId, getByClassAsync]);

  const columns = [
    { title: 'Student Name', dataIndex: 'studentName', key: 'studentName' },
    {
      title: 'Enrolled', dataIndex: 'enrollmentDate', key: 'enrollmentDate',
      render: (d: string) => dayjs(d).format('DD MMM YYYY'),
    },
    {
      title: 'Status', key: 'status',
      render: (_: unknown, record: IStudentClassList) => (
        <Space>
          {record.isCurrent && <Tag color="green">Current</Tag>}
          <Tag color={record.isActive ? 'blue' : 'default'}>{record.isActive ? 'Active' : 'Inactive'}</Tag>
        </Space>
      ),
    },
    {
      title: 'End Date', dataIndex: 'endDate', key: 'endDate',
      render: (d: string | null) => d ? dayjs(d).format('DD MMM YYYY') : '-',
    },
    {
      title: '', key: 'action', width: 100,
      render: (_: unknown, record: IStudentClassList) => (
        <Button
          type="link"
          size="small"
          onClick={() => router.push(`${portalBase}/students/${record.studentId}`)}
        >
          View Profile
        </Button>
      ),
    },
  ];

  return (
    <Table<IStudentClassList>
      dataSource={studentClasses ?? []}
      columns={columns}
      rowKey="id"
      loading={isPending}
      pagination={false}
      size="small"
      locale={{ emptyText: <Empty description="No students enrolled in this class" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
    />
  );
}

// ─── Subjects Tab ──────────────────────────────────────────────
function SubjectsSection({ classId }: { classId: string }) {
  const { classSubjects, isPending } = useClassSubjectState();
  const { getByClassAsync } = useClassSubjectActions();

  useEffect(() => {
    getByClassAsync(classId);
  }, [classId, getByClassAsync]);

  const columns = [
    { title: 'Subject', dataIndex: 'subjectName', key: 'subjectName' },
    { title: 'Code', dataIndex: 'subjectCode', key: 'subjectCode', width: 100 },
    { title: 'Teacher', dataIndex: 'teacherName', key: 'teacherName', render: (v: string) => v || 'Not assigned' },
    { title: 'Periods/Week', dataIndex: 'periodsPerWeek', key: 'periodsPerWeek', width: 120 },
    {
      title: 'Status', dataIndex: 'isActive', key: 'isActive',
      render: (active: boolean) => <Tag color={active ? 'green' : 'default'}>{active ? 'Active' : 'Inactive'}</Tag>,
    },
  ];

  return (
    <Table<IClassSubjectList>
      dataSource={classSubjects ?? []}
      columns={columns}
      rowKey="id"
      loading={isPending}
      pagination={false}
      size="small"
      locale={{ emptyText: <Empty description="No subjects assigned to this class" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
    />
  );
}

// ─── Attendance Tab ────────────────────────────────────────────
function AttendanceSection({ classId }: { classId: string }) {
  const { attendanceSummaries, isPending } = useAttendanceState();
  const { getClassSummaryAsync } = useAttendanceActions();

  useEffect(() => {
    const now = dayjs();
    const startOfTerm = now.startOf('year').format('YYYY-MM-DD');
    const today = now.format('YYYY-MM-DD');
    getClassSummaryAsync(classId, startOfTerm, today);
  }, [classId, getClassSummaryAsync]);

  const aggregate = useMemo(() => {
    const summaries = attendanceSummaries ?? [];
    if (summaries.length === 0) return null;
    const total = summaries.reduce((acc, s) => ({
      totalDays: acc.totalDays + s.totalDays,
      present: acc.present + s.presentCount,
      absent: acc.absent + s.absentCount,
      late: acc.late + s.lateCount,
    }), { totalDays: 0, present: 0, absent: 0, late: 0 });
    const rate = total.totalDays > 0 ? Math.round((total.present / total.totalDays) * 100) : 0;
    return { ...total, rate, studentCount: summaries.length };
  }, [attendanceSummaries]);

  if (isPending) return <Spin />;

  if (!aggregate) {
    return <Empty description="No attendance data available yet" image={Empty.PRESENTED_IMAGE_SIMPLE} />;
  }

  return (
    <div>
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic title="Attendance Rate" value={aggregate.rate} suffix="%" />
            <Progress percent={aggregate.rate} showInfo={false} status={aggregate.rate >= 80 ? 'success' : aggregate.rate >= 60 ? 'normal' : 'exception'} />
          </Card>
        </Col>
        <Col xs={12} sm={6}><Card size="small"><Statistic title="Total Present" value={aggregate.present} /></Card></Col>
        <Col xs={12} sm={6}><Card size="small"><Statistic title="Total Absent" value={aggregate.absent} valueStyle={{ color: aggregate.absent > 0 ? '#cf1322' : undefined }} /></Card></Col>
        <Col xs={12} sm={6}><Card size="small"><Statistic title="Total Late" value={aggregate.late} valueStyle={{ color: aggregate.late > 0 ? '#faad14' : undefined }} /></Card></Col>
      </Row>

      <Table
        dataSource={attendanceSummaries ?? []}
        columns={[
          { title: 'Student', dataIndex: 'studentName', key: 'studentName' },
          { title: 'Days', dataIndex: 'totalDays', key: 'totalDays', width: 80 },
          { title: 'Present', dataIndex: 'presentCount', key: 'presentCount', width: 80 },
          { title: 'Absent', dataIndex: 'absentCount', key: 'absentCount', width: 80 },
          { title: 'Late', dataIndex: 'lateCount', key: 'lateCount', width: 80 },
          {
            title: 'Rate', dataIndex: 'attendancePercentage', key: 'attendancePercentage', width: 100,
            render: (v: number) => (
              <Tag color={v >= 80 ? 'green' : v >= 60 ? 'orange' : 'red'}>{v}%</Tag>
            ),
          },
        ]}
        rowKey="studentId"
        pagination={false}
        size="small"
      />
    </div>
  );
}

// ─── Main Content ──────────────────────────────────────────────
function ClassDetailContent() {
  const params = useParams();
  const router = useRouter();
  const portalBase = usePortalBase();
  const classId = params.id as string;

  const classState = useClassState();
  const { getAsync } = useClassActions();
  const classData = classState.class;

  useEffect(() => {
    if (classId) getAsync(classId);
  }, [classId, getAsync]);

  if (classState.isPending && !classData) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 80 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (classState.isError || (!classState.isPending && !classData)) {
    return (
      <div style={{ padding: 24 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => router.push(`${portalBase}/classes`)} style={{ marginBottom: 16 }}>
          Back to Classes
        </Button>
        <Empty description="Class not found" />
      </div>
    );
  }

  if (!classData) return null;

  const capacityPct = classData.maxCapacity > 0 ? Math.round((classData.studentCount / classData.maxCapacity) * 100) : 0;

  const tabItems = [
    {
      key: 'roster',
      label: <span><TeamOutlined /> Students ({classData.studentCount})</span>,
      children: <RosterSection classId={classId} />,
    },
    {
      key: 'subjects',
      label: <span><BookOutlined /> Subjects</span>,
      children: <SubjectsSection classId={classId} />,
    },
    {
      key: 'attendance',
      label: <span><BarChartOutlined /> Attendance</span>,
      children: <AttendanceSection classId={classId} />,
    },
  ];

  return (
    <div style={{ padding: 24 }}>
      <Button
        icon={<ArrowLeftOutlined />}
        onClick={() => router.push(`${portalBase}/classes`)}
        style={{ marginBottom: 16 }}
      >
        Back to Classes
      </Button>

      <Card style={{ marginBottom: 24 }}>
        <Row gutter={24} align="middle">
          <Col flex="auto">
            <Space align="center" size="middle">
              <Title level={3} style={{ margin: 0 }}>{classData.className}</Title>
              <Tag color={classData.isActive ? 'green' : 'default'}>{classData.isActive ? 'Active' : 'Inactive'}</Tag>
            </Space>
            <div style={{ marginTop: 8 }}>
              <Space size="large">
                <Text type="secondary">Grade: {classData.gradeName}</Text>
                <Text type="secondary">Academic Year: {classData.academicYearName}</Text>
                <Text type="secondary">
                  <UserOutlined /> Teacher: {classData.classTeacherName || 'Not assigned'}
                </Text>
              </Space>
            </div>
          </Col>
          <Col>
            <Card size="small" style={{ textAlign: 'center', minWidth: 140 }}>
              <Statistic
                title="Capacity"
                value={classData.studentCount}
                suffix={`/ ${classData.maxCapacity}`}
              />
              <Progress
                percent={capacityPct}
                size="small"
                showInfo={false}
                status={capacityPct >= 90 ? 'exception' : 'normal'}
              />
            </Card>
          </Col>
        </Row>
      </Card>

      <Card>
        <Tabs items={tabItems} defaultActiveKey="roster" />
      </Card>
    </div>
  );
}

// ─── Wrapped with providers ────────────────────────────────────
export default function ClassDetailPage() {
  return (
    <ClassProvider>
      <StudentClassProvider>
        <ClassSubjectProvider>
          <AttendanceProvider>
            <ClassDetailContent />
          </AttendanceProvider>
        </ClassSubjectProvider>
      </StudentClassProvider>
    </ClassProvider>
  );
}
