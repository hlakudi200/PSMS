'use client';

import React, { useEffect, useMemo, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { usePortalBase } from '@/utils/portal-base';
import {
  Card,
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
  DatePicker,
  Modal,
  Tooltip,
} from 'antd';
import {
  ArrowLeftOutlined,
  TeamOutlined,
  BookOutlined,
  BarChartOutlined,
  UserOutlined,
  WarningOutlined,
  HistoryOutlined,
} from '@ant-design/icons';
import dayjs, { Dayjs } from 'dayjs';
import { ClassProvider, useClassState, useClassActions } from '@/providers/academic/classes';
import { StudentClassProvider, useStudentClassState, useStudentClassActions } from '@/providers/academic/student_classes';
import { ClassSubjectProvider, useClassSubjectState, useClassSubjectActions } from '@/providers/academic/class_subjects';
import { AttendanceProvider, useAttendanceState, useAttendanceActions } from '@/providers/academic/attendances';
import type { IStudentClassList, IClassSubjectList, IAttendanceList } from '@/providers/academic/shared/interfaces';

const { RangePicker } = DatePicker;

const ATTENDANCE_STATUS_LABEL: Record<number, string> = {
  1: 'Present',
  2: 'Absent',
  3: 'Late',
  4: 'Excused',
  5: 'Sick Leave',
  6: 'Holiday',
};
const ATTENDANCE_STATUS_COLOR: Record<number, string> = {
  1: 'green',
  2: 'red',
  3: 'gold',
  4: 'blue',
  5: 'orange',
  6: 'default',
};
const ABSENT_STATUS = 2;
// A run of this many or more trailing absences gets flagged in the report —
// a common school-policy threshold for follow-up, not a hard business rule.
const CONSECUTIVE_ABSENCE_WARNING_THRESHOLD = 3;

// Longest trailing run of consecutive Absent records, counting back from
// the most recent record in the (already date-range-filtered) list. Gaps
// where nothing was recorded (weekends, holidays) don't break the streak
// since there's no record for those days to begin with — this counts
// consecutive *recorded* absences, not consecutive calendar days.
function currentConsecutiveAbsences(records: IAttendanceList[]): number {
  const sorted = records.slice().sort((a, b) => a.attendanceDate.localeCompare(b.attendanceDate));
  let streak = 0;
  for (let i = sorted.length - 1; i >= 0; i--) {
    if (sorted[i].status === ABSENT_STATUS) streak++;
    else break;
  }
  return streak;
}

const { Title, Text } = Typography;

// Only these portals mount a student profile detail route today. Gating on
// an allowlist (rather than excluding known-absent ones) means a future
// portal without one fails safe — no dead "View Profile" link — instead of
// needing to remember to add it here too.
const PORTALS_WITH_STUDENT_PROFILES = ['/principal', '/academic'];

// ─── Students Roster Tab ───────────────────────────────────────
function RosterSection({ classId }: { classId: string }) {
  const router = useRouter();
  const portalBase = usePortalBase();
  const canViewStudentProfile = PORTALS_WITH_STUDENT_PROFILES.includes(portalBase);
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
    ...(canViewStudentProfile ? [{
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
    }] : []),
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
// T-T21: class-level + per-student summaries across a teacher-selected
// date range, consecutive-absence indicators, and a per-student history
// drill-down — this tab is what "View Roster" opens into, so it doubles
// as the ticket's "enable student attendance history from roster view".
function AttendanceSection({ classId }: { classId: string }) {
  const { attendanceSummaries, attendances, isPending } = useAttendanceState();
  const { getClassSummaryAsync, getAllAsync } = useAttendanceActions();

  const [range, setRange] = useState<[Dayjs, Dayjs]>([dayjs().startOf('year'), dayjs()]);
  const [historyStudent, setHistoryStudent] = useState<{ id: string; name: string } | null>(null);

  const startKey = range[0].format('YYYY-MM-DD');
  const endKey = range[1].format('YYYY-MM-DD');

  useEffect(() => {
    getClassSummaryAsync(classId, startKey, endKey);
    // The raw per-day records power the consecutive-absence column and
    // the history drill-down below — one fetch for the whole class rather
    // than a separate round trip per student.
    getAllAsync({ classId, startDate: startKey, endDate: endKey, maxResultCount: 5000 });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [classId, startKey, endKey]);

  const recordsByStudent = useMemo(() => {
    const map = new Map<string, IAttendanceList[]>();
    (attendances ?? []).forEach((a) => {
      const list = map.get(a.studentId) ?? [];
      list.push(a);
      map.set(a.studentId, list);
    });
    return map;
  }, [attendances]);

  const consecutiveByStudent = useMemo(() => {
    const map = new Map<string, number>();
    recordsByStudent.forEach((records, studentId) => {
      map.set(studentId, currentConsecutiveAbsences(records));
    });
    return map;
  }, [recordsByStudent]);

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

  const historyRecords = useMemo(() => {
    if (!historyStudent) return [];
    return (recordsByStudent.get(historyStudent.id) ?? [])
      .slice()
      .sort((a, b) => b.attendanceDate.localeCompare(a.attendanceDate));
  }, [historyStudent, recordsByStudent]);

  const rangePicker = (
    <RangePicker
      value={range}
      onChange={(vals) => {
        if (vals && vals[0] && vals[1]) setRange([vals[0], vals[1]]);
      }}
      allowClear={false}
      disabledDate={(current) => !!current && current.isAfter(dayjs().endOf('day'))}
      style={{ marginBottom: 16 }}
    />
  );

  if (isPending && !aggregate) {
    return (
      <div>
        {rangePicker}
        <Spin />
      </div>
    );
  }

  if (!aggregate) {
    return (
      <div>
        {rangePicker}
        <Empty description="No attendance data available for this date range" image={Empty.PRESENTED_IMAGE_SIMPLE} />
      </div>
    );
  }

  return (
    <div>
      {rangePicker}

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
          {
            title: 'Consecutive absences', key: 'consecutive', width: 170,
            render: (_: unknown, row: { studentId: string }) => {
              const streak = consecutiveByStudent.get(row.studentId) ?? 0;
              if (streak === 0) return <Text type="secondary">—</Text>;
              const flagged = streak >= CONSECUTIVE_ABSENCE_WARNING_THRESHOLD;
              return (
                <Tag color={flagged ? 'red' : 'default'} icon={flagged ? <WarningOutlined /> : undefined}>
                  {streak} day{streak === 1 ? '' : 's'}
                </Tag>
              );
            },
          },
          {
            title: '', key: 'history', width: 90,
            render: (_: unknown, row: { studentId: string; studentName?: string }) => (
              <Tooltip title="View attendance history">
                <Button
                  type="link"
                  size="small"
                  icon={<HistoryOutlined />}
                  onClick={() => setHistoryStudent({ id: row.studentId, name: row.studentName ?? 'Student' })}
                >
                  History
                </Button>
              </Tooltip>
            ),
          },
        ]}
        rowKey="studentId"
        pagination={false}
        size="small"
      />

      <Modal
        title={historyStudent ? `Attendance history — ${historyStudent.name}` : 'Attendance history'}
        open={!!historyStudent}
        onCancel={() => setHistoryStudent(null)}
        footer={null}
        destroyOnHidden
      >
        <Table<IAttendanceList>
          dataSource={historyRecords}
          rowKey="id"
          size="small"
          pagination={{ pageSize: 10 }}
          columns={[
            {
              title: 'Date', dataIndex: 'attendanceDate', key: 'attendanceDate', width: 120,
              render: (d: string) => dayjs(d).format('DD MMM YYYY'),
            },
            {
              title: 'Status', dataIndex: 'status', key: 'status',
              render: (s: number) => (
                <Tag color={ATTENDANCE_STATUS_COLOR[s] ?? 'default'}>{ATTENDANCE_STATUS_LABEL[s] ?? '—'}</Tag>
              ),
            },
            {
              title: 'Notes', dataIndex: 'notes', key: 'notes',
              render: (n?: string) => n || <Text type="secondary">—</Text>,
            },
          ]}
          locale={{ emptyText: <Empty description="No records in this date range" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
        />
      </Modal>
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
