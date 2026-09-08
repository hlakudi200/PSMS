'use client';

import { useEffect, useState, useCallback, KeyboardEvent } from 'react';
import { useRouter } from 'next/navigation';
import { usePortalBase } from '@/utils/portal-base';
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
  AuditOutlined,
  FormOutlined,
  WarningOutlined,
  SwapOutlined,
  EnvironmentOutlined,
  ShoppingOutlined,
  RightOutlined,
} from '@ant-design/icons';
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
  PrincipalDashboardProvider,
  usePrincipalDashboardActions,
  usePrincipalDashboardState,
} from '@/providers/dashboard/principal';
import {
  WorkflowDashboardProvider,
  useWorkflowDashboardActions,
  useWorkflowDashboardState,
} from '@/providers/workflow/workflow-dashboard';
import type { IGradePerformance } from '@/providers/dashboard/shared/interfaces';
import { IAnnouncementList } from '@/providers/communication/shared/interfaces';
import {
  AttendanceStatus,
  AnnouncementPriority,
} from '@/providers/shared/enums';
import { formatZAR } from '@/utils/currency';

interface WeeklyAttendanceDay {
  label: string;
  date: string;
  percentage: number;
}

interface AttentionItem {
  key: string;
  label: string;
  description: string;
  count: number;
  icon: React.ReactNode;
  path: string;
}

const { Text } = Typography;

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
  const portalBase = usePortalBase();

  const { getCurrentAsync } = useAcademicYearActions();
  const { academicYear: currentAcademicYear, isPending: academicYearPending } = useAcademicYearState();

  const { getSummaryAsync } = usePrincipalDashboardActions();
  const { summary, isPending: summaryPending, isError: summaryError } = usePrincipalDashboardState();

  const { getDashboardAsync: getWorkflowDashboard } = useWorkflowDashboardActions();
  const { dashboard: workflowDashboard, isPending: workflowPending } = useWorkflowDashboardState();

  const { getAllAsync: getAllAttendance } = useAttendanceActions();
  const { attendances, isPending: attendancePending } = useAttendanceState();

  const { getAllAsync: getAllAnnouncements } = useAnnouncementActions();
  const { announcements, isPending: announcementsPending } = useAnnouncementState();

  const [attendanceTodayPct, setAttendanceTodayPct] = useState<number | null>(null);
  const [weeklyAttendance, setWeeklyAttendance] = useState<WeeklyAttendanceDay[]>([]);
  const [weeklyLoading, setWeeklyLoading] = useState(true);
  const [weeklyError, setWeeklyError] = useState<string | null>(null);
  const [yearResolved, setYearResolved] = useState(false);

  const today = formatDate(new Date());

  useEffect(() => {
    let cancelled = false;
    // Resolve the current year first so the summary (classes, reports, per-grade
    // counts) is scoped to it; the attention queue is school-wide regardless.
    Promise.resolve(getCurrentAsync()).then(() => {
      if (!cancelled) setYearResolved(true);
    });
    getWorkflowDashboard();
    getAllAnnouncements({ isPublished: true, maxResultCount: 5 });
    return () => { cancelled = true; };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // One aggregated summary call replaces the previous per-entity page pulls
  // (1000 reports + 500 classes) — the backend groups by grade for us.
  useEffect(() => {
    if (!yearResolved) return;
    getSummaryAsync(currentAcademicYear?.id);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [yearResolved, currentAcademicYear?.id]);

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

  const handleCardKeyDown = (event: KeyboardEvent<HTMLDivElement>, target: string) => {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      router.push(target);
    }
  };

  const passMark = summary?.passMarkPercentage ?? 50;

  const attentionItems: AttentionItem[] = [
    {
      key: 'approvals',
      label: 'My approvals',
      description: 'Workflow steps waiting for you',
      count: workflowDashboard?.myPendingCount ?? 0,
      icon: <AuditOutlined />,
      path: `${portalBase}/workflow/my-approvals`,
    },
    {
      key: 'admissions',
      label: 'Admissions awaiting decision',
      description: 'Submitted applications not yet decided',
      count: summary?.pendingAdmissions ?? 0,
      icon: <FormOutlined />,
      path: `${portalBase}/admissions`,
    },
    {
      key: 'discipline',
      label: 'Open disciplinary cases',
      description: 'Reported, under investigation or at hearing',
      count: summary?.openDisciplinaryCases ?? 0,
      icon: <WarningOutlined />,
      path: `${portalBase}/disciplinary`,
    },
    {
      key: 'leave',
      label: 'Leave requests pending',
      description: 'Submitted or HOD-approved staff leave',
      count: summary?.pendingLeaveRequests ?? 0,
      icon: <CalendarOutlined />,
      path: `${portalBase}/staff-leave`,
    },
    {
      key: 'transfers',
      label: 'Student transfers pending',
      description: 'Awaiting principal approval',
      count: summary?.pendingTransfers ?? 0,
      icon: <SwapOutlined />,
      path: `${portalBase}/student-transfers`,
    },
    {
      key: 'fieldTrips',
      label: 'Field trips pending',
      description: 'Submitted or under review',
      count: summary?.pendingFieldTrips ?? 0,
      icon: <EnvironmentOutlined />,
      path: `${portalBase}/field-trips`,
    },
    {
      key: 'expenses',
      label: 'Expense requests pending',
      description: 'Submitted or under review',
      count: summary?.pendingExpenses ?? 0,
      icon: <ShoppingOutlined />,
      path: `${portalBase}/expenses`,
    },
    {
      key: 'feeWaivers',
      label: 'Fee waivers pending',
      description: 'Submitted or under review',
      count: summary?.pendingFeeWaivers ?? 0,
      icon: <DollarOutlined />,
      path: `${portalBase}/fee-waivers`,
    },
  ];

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
      render: (_: unknown, record: IGradePerformance) => {
        if (record.averagePercentage == null) return <Text type="secondary">—</Text>;
        const color = record.averagePercentage >= passMark ? '#3f8600' : '#cf1322';
        return <Text strong style={{ color }}>{record.averagePercentage}%</Text>;
      },
    },
    {
      title: 'Pass Rate',
      key: 'passRate',
      render: (_: unknown, record: IGradePerformance) => {
        if (record.passRate == null) return <Text type="secondary">—</Text>;
        const color =
          record.passRate >= STRONG_PASS_RATE
            ? '#3f8600'
            : record.passRate >= MODERATE_PASS_RATE
            ? '#FAAD14'
            : '#cf1322';
        return <Text strong style={{ color }}>{record.passRate}%</Text>;
      },
    },
    {
      title: 'Reports',
      key: 'reportCount',
      render: (_: unknown, record: IGradePerformance) => {
        if (record.reportCount === 0) return <Text type="secondary">—</Text>;
        return <Tag color="blue">{record.reportCount}</Tag>;
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

      {summaryError && (
        <Alert
          type="warning"
          showIcon
          message="Could not load the school summary. Counts below may be incomplete."
          style={{ marginBottom: 16 }}
        />
      )}

      {/* Stats Grid */}
      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col xs={24} sm={12} lg={6}>
          {renderStatCard(
            'Total Students',
            summary?.totalStudents ?? 0,
            <TeamOutlined />,
            '#003D73',
            `${portalBase}/students`,
            summaryPending
          )}
        </Col>
        <Col xs={24} sm={12} lg={6}>
          {renderStatCard(
            'Teachers',
            summary?.totalTeachers ?? 0,
            <UserOutlined />,
            '#52C41A',
            `${portalBase}/teachers`,
            summaryPending
          )}
        </Col>
        <Col xs={24} sm={12} lg={6}>
          {renderStatCard(
            'Classes',
            summary?.totalClasses ?? 0,
            <BookOutlined />,
            '#FAAD14',
            `${portalBase}/classes`,
            summaryPending
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
          {/* Needs your attention */}
          <Card
            title={
              <span>
                <AuditOutlined style={{ marginRight: 8 }} />
                Needs Your Attention
              </span>
            }
            extra={
              <Button type="link" onClick={() => router.push(`${portalBase}/workflow`)}>
                Workflow Dashboard
              </Button>
            }
            variant="borderless"
            style={{ marginBottom: '16px' }}
          >
            <List<AttentionItem>
              dataSource={attentionItems}
              loading={summaryPending || workflowPending}
              renderItem={(item) => (
                <List.Item
                  style={{ cursor: 'pointer' }}
                  onClick={() => router.push(item.path)}
                  role="button"
                  tabIndex={0}
                  onKeyDown={(e) => handleCardKeyDown(e, item.path)}
                  aria-label={`${item.label}, ${item.count} pending`}
                  extra={
                    <span style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                      <Badge
                        count={item.count}
                        showZero
                        overflowCount={999}
                        color={item.count > 0 ? '#FAAD14' : '#d9d9d9'}
                      />
                      <RightOutlined style={{ color: '#8c8c8c', fontSize: 12 }} />
                    </span>
                  }
                >
                  <List.Item.Meta
                    avatar={<span style={{ fontSize: 18, color: '#003D73' }}>{item.icon}</span>}
                    title={<Text strong>{item.label}</Text>}
                    description={
                      <Text type="secondary" style={{ fontSize: 12 }}>
                        {item.description}
                      </Text>
                    }
                  />
                </List.Item>
              )}
            />
          </Card>

          {/* Grade Performance Overview */}
          <Card
            title={
              <span>
                <BarChartOutlined style={{ marginRight: 8 }} />
                Grade Performance Overview
              </span>
            }
            extra={
              <Button type="link" onClick={() => router.push(`${portalBase}/reports`)}>
                View Full Report
              </Button>
            }
            variant="borderless"
            style={{ marginBottom: '16px' }}
          >
            <Table<IGradePerformance>
              dataSource={summary?.gradePerformance ?? []}
              columns={gradeColumns}
              rowKey="gradeId"
              loading={summaryPending}
              pagination={false}
              size="small"
              locale={{ emptyText: 'No grades configured' }}
            />
            <Text type="secondary" style={{ fontSize: 12 }}>
              Pass rate uses a {passMark}% pass mark over reports with an overall percentage.
            </Text>
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
              <Button type="link" onClick={() => router.push(`${portalBase}/announcements`)}>
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
          {/* Fees snapshot */}
          <Card
            variant="borderless"
            style={{ marginBottom: '16px', borderTop: '4px solid #722ED1', cursor: 'pointer' }}
            onClick={() => router.push(`${portalBase}/finance`)}
            onKeyDown={(e) => handleCardKeyDown(e, `${portalBase}/finance`)}
            role="button"
            tabIndex={0}
            aria-label="Outstanding fees, navigate"
            hoverable
          >
            <Statistic
              title="Outstanding Fees"
              value={formatZAR(summary?.outstandingFeesTotal ?? 0)}
              prefix={<DollarOutlined />}
              valueStyle={{ color: '#722ED1' }}
              loading={summaryPending}
            />
            {!summaryPending && (
              <Text type="secondary" style={{ fontSize: 12 }}>
                {summary?.overdueFeesCount ?? 0} fee{(summary?.overdueFeesCount ?? 0) === 1 ? '' : 's'} overdue
              </Text>
            )}
          </Card>

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
              onClick={() => router.push(`${portalBase}/reports`)}
            >
              View Reports
            </Button>
            <Button
              block
              icon={<NotificationOutlined />}
              style={{ marginBottom: '8px', textAlign: 'left' }}
              onClick={() => router.push(`${portalBase}/announcements`)}
            >
              Announcements
            </Button>
            <Button
              block
              icon={<CalendarOutlined />}
              style={{ marginBottom: '8px', textAlign: 'left' }}
              onClick={() => router.push(`${portalBase}/timetables`)}
            >
              Timetables
            </Button>
            <Button
              block
              icon={<TeamOutlined />}
              style={{ marginBottom: '8px', textAlign: 'left' }}
              onClick={() => router.push(`${portalBase}/admissions`)}
            >
              Admissions
            </Button>
            <Button
              block
              icon={<DollarOutlined />}
              style={{ marginBottom: '8px', textAlign: 'left' }}
              onClick={() => router.push(`${portalBase}/finance`)}
            >
              Fee Management
            </Button>
            <Button
              block
              icon={<MessageOutlined />}
              style={{ textAlign: 'left' }}
              onClick={() => router.push(`${portalBase}/messages`)}
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
      <PrincipalDashboardProvider>
        <WorkflowDashboardProvider>
          <AttendanceProvider>
            <AnnouncementProvider>
              <PrincipalDashboardContent />
            </AnnouncementProvider>
          </AttendanceProvider>
        </WorkflowDashboardProvider>
      </PrincipalDashboardProvider>
    </AcademicYearProvider>
  );
}
