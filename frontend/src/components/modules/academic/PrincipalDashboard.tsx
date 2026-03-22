'use client';

import { useEffect, useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import {
  Card,
  Row,
  Col,
  Statistic,
  Button,
  Table,
  Tag,
  Progress,
  Typography,
  List,
  Spin,
  Badge,
} from 'antd';
import {
  TeamOutlined,
  UserOutlined,
  BookOutlined,
  CheckCircleOutlined,
  FileTextOutlined,
  NotificationOutlined,
  CalendarOutlined,
  BarChartOutlined,
  DollarOutlined,
  MessageOutlined,
} from '@ant-design/icons';
import {
  StudentProvider,
  useStudentActions,
  useStudentState,
} from '@/providers/academic/students';
import {
  TeacherProvider,
  useTeacherActions,
  useTeacherState,
} from '@/providers/academic/teachers';
import {
  ClassProvider,
  useClassActions,
  useClassState,
} from '@/providers/academic/classes';
import {
  GradeProvider,
  useGradeActions,
  useGradeState,
} from '@/providers/academic/grades';
import {
  AttendanceProvider,
  useAttendanceActions,
  useAttendanceState,
} from '@/providers/academic/attendances';
import {
  AnnouncementProvider,
  useAnnouncementActions,
  useAnnouncementState,
} from '@/providers/communication/announcements';
import {
  AcademicYearProvider,
  useAcademicYearActions,
  useAcademicYearState,
} from '@/providers/academic/academic_years';
import { getAxiosInstance } from '@/utils/axios-instance';
import { IGradeList } from '@/providers/academic/shared/interfaces';
import { IAnnouncementList } from '@/providers/communication/shared/interfaces';

interface GradePerformance {
  gradeId: string;
  avgPercentage: number | null;
  passRate: number | null;
  reportCount: number;
}

const { Text } = Typography;

const ATTENDANCE_PRESENT = 1; // AttendanceStatus.Present = 1 in backend enum

interface WeeklyAttendanceDay {
  label: string;
  date: string;
  percentage: number;
}

const priorityColors: Record<number, string> = {
  0: 'blue',
  1: 'orange',
  2: 'red',
};

const priorityLabels: Record<number, string> = {
  0: 'Low',
  1: 'Medium',
  2: 'High',
};

function formatDate(date: Date): string {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

function formatPublishDate(dateStr: string): string {
  try {
    return new Date(dateStr).toLocaleDateString('en-ZA', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    });
  } catch {
    return dateStr;
  }
}

function getWeekDays(): { label: string; date: string }[] {
  const now = new Date();
  const dayOfWeek = now.getDay();
  const mondayOffset = dayOfWeek === 0 ? -6 : 1 - dayOfWeek;
  const monday = new Date(now);
  monday.setDate(now.getDate() + mondayOffset);

  const labels = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri'];
  const days: { label: string; date: string }[] = [];

  for (let i = 0; i < 5; i++) {
    const d = new Date(monday);
    d.setDate(monday.getDate() + i);
    if (d <= now) {
      days.push({ label: labels[i], date: formatDate(d) });
    }
  }
  return days;
}

function PrincipalDashboardContent() {
  const router = useRouter();

  const { getCurrentAsync } = useAcademicYearActions();
  const { academicYear: currentAcademicYear, isPending: academicYearPending } = useAcademicYearState();

  const { getAllAsync: getAllStudents } = useStudentActions();
  const { totalCount: studentCount, isPending: studentsPending } = useStudentState();

  const { getAllAsync: getAllTeachers } = useTeacherActions();
  const { totalCount: teacherCount, isPending: teachersPending } = useTeacherState();

  const { getAllAsync: getAllClasses } = useClassActions();
  const { totalCount: classCount, isPending: classesPending } = useClassState();

  const { getAllAsync: getAllGrades } = useGradeActions();
  const { grades, isPending: gradesPending } = useGradeState();

  const { getAllAsync: getAllAttendance } = useAttendanceActions();
  const { attendances, isPending: attendancePending } = useAttendanceState();

  const { getAllAsync: getAllAnnouncements } = useAnnouncementActions();
  const { announcements, isPending: announcementsPending } = useAnnouncementState();

  const [attendanceTodayPct, setAttendanceTodayPct] = useState<number>(0);
  const [weeklyAttendance, setWeeklyAttendance] = useState<WeeklyAttendanceDay[]>([]);
  const [weeklyLoading, setWeeklyLoading] = useState(true);
  const [gradePerformance, setGradePerformance] = useState<Record<string, GradePerformance>>({});
  const [gradePerformanceLoading, setGradePerformanceLoading] = useState(true);

  const today = formatDate(new Date());

  useEffect(() => {
    getCurrentAsync();
    getAllStudents({ maxResultCount: 1, skipCount: 0 });
    getAllTeachers({ maxResultCount: 1, skipCount: 0 });
    getAllClasses({ maxResultCount: 1, skipCount: 0 });
    getAllGrades({ maxResultCount: 100 });
    getAllAttendance({ startDate: today, endDate: today, maxResultCount: 1000 });
    getAllAnnouncements({ isPublished: true, maxResultCount: 5 });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (attendances && attendances.length > 0) {
      const presentCount = attendances.filter(
        (a) => a.status === ATTENDANCE_PRESENT
      ).length;
      setAttendanceTodayPct(Math.round((presentCount / attendances.length) * 100));
    }
  }, [attendances]);

  const fetchWeeklyAttendance = useCallback(async () => {
    const instance = getAxiosInstance();
    const days = getWeekDays();

    setWeeklyLoading(true);
    try {
      const results = await Promise.all(
        days.map(async ({ label, date }) => {
          try {
            const res = await instance.get(
              `/api/services/app/Attendance/GetAll?StartDate=${date}&EndDate=${date}&MaxResultCount=1000`
            );
            const items: { status: number }[] = res.data.result.items ?? [];
            const presentCount = items.filter(
              (a) => a.status === ATTENDANCE_PRESENT
            ).length;
            const percentage =
              items.length > 0
                ? Math.round((presentCount / items.length) * 100)
                : 0;
            return { label, date, percentage };
          } catch {
            return { label, date, percentage: 0 };
          }
        })
      );
      setWeeklyAttendance(results);
    } finally {
      setWeeklyLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchWeeklyAttendance();
  }, [fetchWeeklyAttendance]);

  // Fetch grade performance from reports
  const fetchGradePerformance = useCallback(async () => {
    const instance = getAxiosInstance();
    setGradePerformanceLoading(true);
    try {
      // Fetch reports with status >= Generated (2) for the current year
      const res = await instance.get(
        `/api/services/app/Report/GetAll?MaxResultCount=1000&Status=2`
      );
      const reports: { classId: string; overallPercentage?: number }[] =
        res.data.result.items ?? [];

      // We need class→grade mapping — fetch classes
      const classRes = await instance.get(
        `/api/services/app/Class/GetAll?MaxResultCount=200`
      );
      const classes: { id: string; gradeId: string }[] =
        classRes.data.result.items ?? [];
      const classToGrade: Record<string, string> = {};
      classes.forEach((c) => {
        classToGrade[c.id] = c.gradeId;
      });

      // Aggregate by grade
      const gradeMap: Record<string, { total: number; sum: number; passCount: number }> = {};
      reports.forEach((r) => {
        const gradeId = classToGrade[r.classId];
        if (!gradeId || r.overallPercentage == null) return;
        if (!gradeMap[gradeId]) gradeMap[gradeId] = { total: 0, sum: 0, passCount: 0 };
        gradeMap[gradeId].total++;
        gradeMap[gradeId].sum += r.overallPercentage;
        if (r.overallPercentage >= 50) gradeMap[gradeId].passCount++;
      });

      const result: Record<string, GradePerformance> = {};
      Object.entries(gradeMap).forEach(([gradeId, data]) => {
        result[gradeId] = {
          gradeId,
          avgPercentage: data.total > 0 ? Math.round((data.sum / data.total) * 10) / 10 : null,
          passRate: data.total > 0 ? Math.round((data.passCount / data.total) * 100) : null,
          reportCount: data.total,
        };
      });
      setGradePerformance(result);
    } catch (err) {
      console.error('Failed to fetch grade performance:', err);
    } finally {
      setGradePerformanceLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchGradePerformance();
  }, [fetchGradePerformance]);

  const gradeColumns = [
    {
      title: 'Grade',
      dataIndex: 'gradeName',
      key: 'gradeName',
      render: (text: string) => <Text strong>{text}</Text>,
    },
    {
      title: 'Students',
      dataIndex: 'studentCount',
      key: 'studentCount',
    },
    {
      title: 'Avg %',
      key: 'avgPct',
      render: (_: unknown, record: IGradeList) => {
        const perf = gradePerformance[record.id];
        if (!perf || perf.avgPercentage == null) return <Text type="secondary">—</Text>;
        const color = perf.avgPercentage >= 50 ? '#3f8600' : '#cf1322';
        return <Text strong style={{ color }}>{perf.avgPercentage}%</Text>;
      },
    },
    {
      title: 'Pass Rate',
      key: 'passRate',
      render: (_: unknown, record: IGradeList) => {
        const perf = gradePerformance[record.id];
        if (!perf || perf.passRate == null) return <Text type="secondary">—</Text>;
        const color = perf.passRate >= 80 ? '#3f8600' : perf.passRate >= 60 ? '#FAAD14' : '#cf1322';
        return <Text strong style={{ color }}>{perf.passRate}%</Text>;
      },
    },
    {
      title: 'Reports',
      key: 'reportCount',
      render: (_: unknown, record: IGradeList) => {
        const perf = gradePerformance[record.id];
        if (!perf || perf.reportCount === 0) return <Text type="secondary">—</Text>;
        return <Tag color="blue">{perf.reportCount}</Tag>;
      },
    },
  ];

  return (
    <div>
      {/* Current Academic Year Banner */}
      <Card
        bordered={false}
        loading={academicYearPending}
        style={{
          marginBottom: 16,
          background: 'linear-gradient(135deg, #003D73 0%, #005A9E 100%)',
          borderRadius: 8,
        }}
        styles={{ body: { padding: '16px 24px' } }}
      >
        {currentAcademicYear ? (
          <Row align="middle" justify="space-between">
            <Col>
              <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                <CalendarOutlined style={{ fontSize: 24, color: '#FFFFFF' }} />
                <div>
                  <Text style={{ color: 'rgba(255,255,255,0.7)', fontSize: 12, display: 'block' }}>
                    Current Academic Year
                  </Text>
                  <Text strong style={{ color: '#FFFFFF', fontSize: 20 }}>
                    {currentAcademicYear.yearName}
                  </Text>
                </div>
              </div>
            </Col>
            <Col>
              <Row gutter={32}>
                <Col>
                  <Text style={{ color: 'rgba(255,255,255,0.7)', fontSize: 12, display: 'block' }}>
                    Start Date
                  </Text>
                  <Text strong style={{ color: '#FFFFFF', fontSize: 14 }}>
                    {new Date(currentAcademicYear.startDate).toLocaleDateString('en-ZA', { day: 'numeric', month: 'short', year: 'numeric' })}
                  </Text>
                </Col>
                <Col>
                  <Text style={{ color: 'rgba(255,255,255,0.7)', fontSize: 12, display: 'block' }}>
                    End Date
                  </Text>
                  <Text strong style={{ color: '#FFFFFF', fontSize: 14 }}>
                    {new Date(currentAcademicYear.endDate).toLocaleDateString('en-ZA', { day: 'numeric', month: 'short', year: 'numeric' })}
                  </Text>
                </Col>
                <Col>
                  <Text style={{ color: 'rgba(255,255,255,0.7)', fontSize: 12, display: 'block' }}>
                    Terms
                  </Text>
                  <Text strong style={{ color: '#FFFFFF', fontSize: 14 }}>
                    {currentAcademicYear.termCount}
                  </Text>
                </Col>
                <Col>
                  <Text style={{ color: 'rgba(255,255,255,0.7)', fontSize: 12, display: 'block' }}>
                    Classes
                  </Text>
                  <Text strong style={{ color: '#FFFFFF', fontSize: 14 }}>
                    {currentAcademicYear.classCount}
                  </Text>
                </Col>
              </Row>
            </Col>
          </Row>
        ) : (
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            <CalendarOutlined style={{ fontSize: 24, color: '#FFFFFF' }} />
            <Text style={{ color: 'rgba(255,255,255,0.85)', fontSize: 14 }}>
              No current academic year has been set.
            </Text>
          </div>
        )}
      </Card>

      {/* Stats Grid */}
      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col xs={24} sm={12} lg={6}>
          <Card
            bordered={false}
            style={{ borderTop: '4px solid #003D73', cursor: 'pointer' }}
            onClick={() => router.push('/principal/students')}
            hoverable
          >
            <Statistic
              title="Total Students"
              value={studentCount ?? 0}
              prefix={<TeamOutlined />}
              valueStyle={{ color: '#003D73' }}
              loading={studentsPending}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card
            bordered={false}
            style={{ borderTop: '4px solid #52C41A', cursor: 'pointer' }}
            onClick={() => router.push('/principal/teachers')}
            hoverable
          >
            <Statistic
              title="Teachers"
              value={teacherCount ?? 0}
              prefix={<UserOutlined />}
              valueStyle={{ color: '#52C41A' }}
              loading={teachersPending}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card
            bordered={false}
            style={{ borderTop: '4px solid #FAAD14', cursor: 'pointer' }}
            onClick={() => router.push('/principal/classes')}
            hoverable
          >
            <Statistic
              title="Classes"
              value={classCount ?? 0}
              prefix={<BookOutlined />}
              valueStyle={{ color: '#FAAD14' }}
              loading={classesPending}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card bordered={false} style={{ borderTop: '4px solid #1890FF' }}>
            <Statistic
              title="Attendance Today"
              value={attendancePending ? 0 : attendanceTodayPct}
              suffix="%"
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#1890FF' }}
              loading={attendancePending}
            />
          </Card>
        </Col>
      </Row>

      {/* Main Content */}
      <Row gutter={[16, 16]}>
        {/* Left Column */}
        <Col xs={24} lg={16}>
          {/* Grade Performance Overview */}
          <Card
            title={
              <span>
                <BarChartOutlined style={{ marginRight: 8 }} />
                Grade Performance Overview
              </span>
            }
            extra={
              <Button type="link" onClick={() => router.push('/principal/reports')}>
                View Full Report
              </Button>
            }
            bordered={false}
            style={{ marginBottom: '16px' }}
          >
            <Table<IGradeList>
              dataSource={grades ?? []}
              columns={gradeColumns}
              rowKey="id"
              loading={gradesPending || gradePerformanceLoading}
              pagination={false}
              size="small"
              locale={{ emptyText: 'No grades configured' }}
            />
          </Card>

          {/* Recent Announcements */}
          <Card
            title={
              <span>
                <NotificationOutlined style={{ marginRight: 8 }} />
                Recent Announcements
              </span>
            }
            extra={
              <Button type="link" onClick={() => router.push('/principal/announcements')}>
                View All
              </Button>
            }
            bordered={false}
          >
            {announcementsPending ? (
              <div style={{ textAlign: 'center', padding: '24px' }}>
                <Spin />
              </div>
            ) : (
              <List<IAnnouncementList>
                dataSource={announcements ?? []}
                locale={{ emptyText: 'No recent announcements' }}
                renderItem={(item) => (
                  <List.Item
                    extra={
                      <Tag color={priorityColors[item.priority] ?? 'default'}>
                        {priorityLabels[item.priority] ?? 'Normal'}
                      </Tag>
                    }
                  >
                    <List.Item.Meta
                      avatar={
                        <Badge dot color={item.isPinned ? 'gold' : 'blue'} />
                      }
                      title={<Text strong>{item.title}</Text>}
                      description={
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          {formatPublishDate(item.publishDate)}
                        </Text>
                      }
                    />
                  </List.Item>
                )}
              />
            )}
          </Card>
        </Col>

        {/* Right Column */}
        <Col xs={24} lg={8}>
          {/* Quick Actions */}
          <Card
            title={
              <span>
                <FileTextOutlined style={{ marginRight: 8 }} />
                Quick Actions
              </span>
            }
            bordered={false}
            style={{ marginBottom: '16px' }}
          >
            <Button
              block
              icon={<BarChartOutlined />}
              style={{ marginBottom: '8px', textAlign: 'left' }}
              onClick={() => router.push('/principal/reports')}
            >
              View Reports
            </Button>
            <Button
              block
              icon={<NotificationOutlined />}
              style={{ marginBottom: '8px', textAlign: 'left' }}
              onClick={() => router.push('/principal/announcements')}
            >
              Announcements
            </Button>
            <Button
              block
              icon={<CalendarOutlined />}
              style={{ marginBottom: '8px', textAlign: 'left' }}
              onClick={() => router.push('/principal/timetables')}
            >
              Timetables
            </Button>
            <Button
              block
              icon={<TeamOutlined />}
              style={{ marginBottom: '8px', textAlign: 'left' }}
              onClick={() => router.push('/principal/admissions')}
            >
              Admissions
            </Button>
            <Button
              block
              icon={<DollarOutlined />}
              style={{ marginBottom: '8px', textAlign: 'left' }}
              onClick={() => router.push('/principal/finance')}
            >
              Fee Management
            </Button>
            <Button
              block
              icon={<MessageOutlined />}
              style={{ textAlign: 'left' }}
              onClick={() => router.push('/principal/messages')}
            >
              Messages
            </Button>
          </Card>

          {/* Attendance This Week */}
          <Card
            title={
              <span>
                <CheckCircleOutlined style={{ marginRight: 8 }} />
                Attendance This Week
              </span>
            }
            bordered={false}
          >
            {weeklyLoading ? (
              <div style={{ textAlign: 'center', padding: '24px' }}>
                <Spin />
              </div>
            ) : weeklyAttendance.length === 0 ? (
              <Text type="secondary">No attendance data for this week.</Text>
            ) : (
              weeklyAttendance.map((day) => (
                <div key={day.date} style={{ marginBottom: '14px' }}>
                  <div
                    style={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      marginBottom: '4px',
                    }}
                  >
                    <Text style={{ fontSize: '13px' }}>
                      {day.date === today ? `${day.label} (Today)` : day.label}
                    </Text>
                    <Text strong style={{ fontSize: '13px' }}>
                      {day.percentage}%
                    </Text>
                  </div>
                  <Progress
                    percent={day.percentage}
                    showInfo={false}
                    strokeColor={
                      day.percentage >= 90
                        ? '#52C41A'
                        : day.percentage >= 75
                        ? '#FAAD14'
                        : '#FF4D4F'
                    }
                    size="small"
                  />
                </div>
              ))
            )}
          </Card>
        </Col>
      </Row>
    </div>
  );
}

export default function PrincipalDashboard() {
  return (
    <AcademicYearProvider>
      <StudentProvider>
        <TeacherProvider>
          <ClassProvider>
            <GradeProvider>
              <AttendanceProvider>
                <AnnouncementProvider>
                  <PrincipalDashboardContent />
                </AnnouncementProvider>
              </AttendanceProvider>
            </GradeProvider>
          </ClassProvider>
        </TeacherProvider>
      </StudentProvider>
    </AcademicYearProvider>
  );
}
