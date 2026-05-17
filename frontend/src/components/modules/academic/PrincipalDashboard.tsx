'use client';

import { useEffect, useState, useCallback, useMemo, KeyboardEvent } from 'react';
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
  Alert,
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
import {
  ReportProvider,
  useReportActions,
  useReportState,
} from '@/providers/assessment/reports';
import { IGradeList } from '@/providers/academic/shared/interfaces';
import { IAnnouncementList } from '@/providers/communication/shared/interfaces';
import {
  AttendanceStatus,
  AnnouncementPriority,
} from '@/providers/shared/enums';

interface GradePerformance {
  gradeId: string;
  avgPercentage: number | null;
  passRate: number | null;
  reportCount: number;
}

interface WeeklyAttendanceDay {
  label: string;
  date: string;
  percentage: number;
}

const { Text } = Typography;

const PASS_PERCENTAGE = 50;
const STRONG_PASS_RATE = 80;
const MODERATE_PASS_RATE = 60;

const priorityColors: Record<AnnouncementPriority, string> = {
  [AnnouncementPriority.Low]: 'blue',
  [AnnouncementPriority.Normal]: 'default',
  [AnnouncementPriority.High]: 'orange',
  [AnnouncementPriority.Urgent]: 'red',
};

const priorityLabels: Record<AnnouncementPriority, string> = {
  [AnnouncementPriority.Low]: 'Low',
  [AnnouncementPriority.Normal]: 'Normal',
  [AnnouncementPriority.High]: 'High',
  [AnnouncementPriority.Urgent]: 'Urgent',
};

function formatDate(date: Date): string {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

function formatDisplayDate(value?: string): string {
  if (!value) return '—';
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return value;
  return d.toLocaleDateString('en-ZA', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  });
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
  const { classes, totalCount: classCount, isPending: classesPending } = useClassState();

  const { getAllAsync: getAllGrades } = useGradeActions();
  const { grades, isPending: gradesPending } = useGradeState();

  const { getAllAsync: getAllAttendance } = useAttendanceActions();
  const { attendances, isPending: attendancePending } = useAttendanceState();

  const { getAllAsync: getAllAnnouncements } = useAnnouncementActions();
  const { announcements, isPending: announcementsPending } = useAnnouncementState();

  const { getAllAsync: getAllReports } = useReportActions();
  const { reports, isPending: reportsPending } = useReportState();

  const [attendanceTodayPct, setAttendanceTodayPct] = useState<number | null>(null);
  const [weeklyAttendance, setWeeklyAttendance] = useState<WeeklyAttendanceDay[]>([]);
  const [weeklyLoading, setWeeklyLoading] = useState(true);
  const [weeklyError, setWeeklyError] = useState<string | null>(null);

  const today = formatDate(new Date());

  // Initial single-page loads via providers
  useEffect(() => {
    getCurrentAsync();
    getAllStudents({ maxResultCount: 1, skipCount: 0 });
    getAllTeachers({ maxResultCount: 1, skipCount: 0 });
    // Classes are needed both for the count card and to map classId -> gradeId
    // in the performance widget below, so pull a reasonable page once.
    // Why: the backend ReportListDto exposes ClassId but not GradeId; see
    // backend ticket T-115 to expose GradeId directly and remove this dependency.
    getAllClasses({ maxResultCount: 500, skipCount: 0 });
    getAllGrades({ maxResultCount: 100 });
    getAllAnnouncements({ isPublished: true, maxResultCount: 5 });
    // Reports for grade-performance aggregation (no status filter — pull all
    // reports for current tenant and aggregate any with an overall percentage).
    getAllReports({ maxResultCount: 1000 });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Single weekly attendance range query, then bucket by date
  const fetchWeeklyAttendance = useCallback(async () => {
    setWeeklyLoading(true);
    setWeeklyError(null);
    const days = getWeekDays();
    if (days.length === 0) {
      setWeeklyAttendance([]);
      setAttendanceTodayPct(null);
      setWeeklyLoading(false);
      return;
    }
    const startDate = days[0].date;
    const endDate = days[days.length - 1].date;
    try {
      await getAllAttendance({ startDate, endDate, maxResultCount: 5000 });
    } catch {
      setWeeklyError('Could not load weekly attendance.');
    } finally {
      setWeeklyLoading(false);
    }
  }, [getAllAttendance]);

  // Derive per-day percentages from the loaded attendance set
  useEffect(() => {
    const days = getWeekDays();
    const buckets = new Map<string, { total: number; present: number }>();
    days.forEach((d) => buckets.set(d.date, { total: 0, present: 0 }));
    (attendances ?? []).forEach((a) => {
      const date = (a.attendanceDate ?? '').slice(0, 10);
      const bucket = buckets.get(date);
      if (!bucket) return;
      bucket.total += 1;
      if (a.status === AttendanceStatus.Present) bucket.present += 1;
    });

    const next: WeeklyAttendanceDay[] = days.map((d) => {
      const b = buckets.get(d.date) ?? { total: 0, present: 0 };
      const percentage = b.total > 0 ? Math.round((b.present / b.total) * 100) : 0;
      return { label: d.label, date: d.date, percentage };
    });
    setWeeklyAttendance(next);
    const todayBucket = buckets.get(today);
    if (todayBucket && todayBucket.total > 0) {
      setAttendanceTodayPct(Math.round((todayBucket.present / todayBucket.total) * 100));
    } else {
      setAttendanceTodayPct(null);
    }
  }, [attendances, today]);

  useEffect(() => {
    fetchWeeklyAttendance();
  }, [fetchWeeklyAttendance]);

  // Aggregate grade performance from reports + classes
  const gradePerformance = useMemo<Record<string, GradePerformance>>(() => {
    if (!reports || !classes) return {};
    const classToGrade = new Map<string, string>();
    classes.forEach((c) => classToGrade.set(c.id, c.gradeId));

    const acc = new Map<string, { total: number; sum: number; passCount: number }>();
    reports.forEach((r) => {
      const gradeId = classToGrade.get(r.classId);
      if (!gradeId || r.overallPercentage == null) return;
      const pct = Number(r.overallPercentage);
      if (Number.isNaN(pct)) return;
      const entry = acc.get(gradeId) ?? { total: 0, sum: 0, passCount: 0 };
      entry.total += 1;
      entry.sum += pct;
      if (pct >= PASS_PERCENTAGE) entry.passCount += 1;
      acc.set(gradeId, entry);
    });

    const result: Record<string, GradePerformance> = {};
    acc.forEach((data, gradeId) => {
      result[gradeId] = {
        gradeId,
        avgPercentage:
          data.total > 0 ? Math.round((data.sum / data.total) * 10) / 10 : null,
        passRate:
          data.total > 0 ? Math.round((data.passCount / data.total) * 100) : null,
        reportCount: data.total,
      };
    });
    return result;
  }, [reports, classes]);

  const handleCardKeyDown = (event: KeyboardEvent<HTMLDivElement>, target: string) => {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      router.push(target);
    }
  };

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
        const color = perf.avgPercentage >= PASS_PERCENTAGE ? '#3f8600' : '#cf1322';
        return <Text strong style={{ color }}>{perf.avgPercentage}%</Text>;
      },
    },
    {
      title: 'Pass Rate',
      key: 'passRate',
      render: (_: unknown, record: IGradeList) => {
        const perf = gradePerformance[record.id];
        if (!perf || perf.passRate == null) return <Text type="secondary">—</Text>;
        const color =
          perf.passRate >= STRONG_PASS_RATE
            ? '#3f8600'
            : perf.passRate >= MODERATE_PASS_RATE
            ? '#FAAD14'
            : '#cf1322';
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

  const renderStatCard = (
    title: string,
    value: number | string,
    icon: React.ReactNode,
    color: string,
    target: string,
    loading: boolean,
    suffix?: string
  ) => (
    <Card
      variant="borderless"
      style={{ borderTop: `4px solid ${color}`, cursor: 'pointer' }}
      onClick={() => router.push(target)}
      onKeyDown={(e) => handleCardKeyDown(e, target)}
      role="button"
      tabIndex={0}
      aria-label={`${title}, navigate`}
      hoverable
    >
      <Statistic
        title={title}
        value={value}
        prefix={icon}
        suffix={suffix}
        valueStyle={{ color }}
        loading={loading}
      />
    </Card>
  );

  return (
    <div>
      {/* Current Academic Year Banner */}
      <Card
        variant="borderless"
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
                    {formatDisplayDate(currentAcademicYear.startDate)}
                  </Text>
                </Col>
                <Col>
                  <Text style={{ color: 'rgba(255,255,255,0.7)', fontSize: 12, display: 'block' }}>
                    End Date
                  </Text>
                  <Text strong style={{ color: '#FFFFFF', fontSize: 14 }}>
                    {formatDisplayDate(currentAcademicYear.endDate)}
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
          {renderStatCard(
            'Total Students',
            studentCount ?? 0,
            <TeamOutlined />,
            '#003D73',
            '/principal/students',
            studentsPending
          )}
        </Col>
        <Col xs={24} sm={12} lg={6}>
          {renderStatCard(
            'Teachers',
            teacherCount ?? 0,
            <UserOutlined />,
            '#52C41A',
            '/principal/teachers',
            teachersPending
          )}
        </Col>
        <Col xs={24} sm={12} lg={6}>
          {renderStatCard(
            'Classes',
            classCount ?? 0,
            <BookOutlined />,
            '#FAAD14',
            '/principal/classes',
            classesPending
          )}
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card variant="borderless" style={{ borderTop: '4px solid #1890FF' }}>
            <Statistic
              title="Attendance Today"
              value={attendanceTodayPct ?? 0}
              suffix={attendanceTodayPct == null ? '' : '%'}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#1890FF' }}
              loading={attendancePending}
            />
            {!attendancePending && attendanceTodayPct == null && (
              <Text type="secondary" style={{ fontSize: 12 }}>
                No attendance captured today
              </Text>
            )}
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
            variant="borderless"
            style={{ marginBottom: '16px' }}
          >
            <Table<IGradeList>
              dataSource={grades ?? []}
              columns={gradeColumns}
              rowKey="id"
              loading={gradesPending || reportsPending || classesPending}
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
            variant="borderless"
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
                      <Tag color={priorityColors[item.priority as AnnouncementPriority] ?? 'default'}>
                        {priorityLabels[item.priority as AnnouncementPriority] ?? 'Normal'}
                      </Tag>
                    }
                  >
                    <List.Item.Meta
                      avatar={<Badge dot color={item.isPinned ? 'gold' : 'blue'} />}
                      title={<Text strong>{item.title}</Text>}
                      description={
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          {formatDisplayDate(item.publishDate)}
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
            variant="borderless"
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
            variant="borderless"
          >
            {weeklyError && (
              <Alert
                type="warning"
                showIcon
                message={weeklyError}
                style={{ marginBottom: 12 }}
              />
            )}
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
                  <ReportProvider>
                    <PrincipalDashboardContent />
                  </ReportProvider>
                </AnnouncementProvider>
              </AttendanceProvider>
            </GradeProvider>
          </ClassProvider>
        </TeacherProvider>
      </StudentProvider>
    </AcademicYearProvider>
  );
}
