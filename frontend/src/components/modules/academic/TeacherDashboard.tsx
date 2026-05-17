'use client';

import { useEffect, useMemo, KeyboardEvent } from 'react';
import { useRouter } from 'next/navigation';
import {
  Alert,
  Badge,
  Button,
  Card,
  Col,
  List,
  Row,
  Skeleton,
  Statistic,
  Tag,
  Typography,
} from 'antd';
import {
  BookOutlined,
  CalendarOutlined,
  FileTextOutlined,
  NotificationOutlined,
  TeamOutlined,
  VideoCameraOutlined,
} from '@ant-design/icons';
import { useAuthState } from '@/providers/auth';
import {
  TeacherProvider,
  useTeacherActions,
  useTeacherState,
} from '@/providers/academic/teachers';
import {
  ClassSubjectProvider,
  useClassSubjectActions,
  useClassSubjectState,
} from '@/providers/academic/class_subjects';
import {
  ClassProvider,
  useClassActions,
  useClassState,
} from '@/providers/academic/classes';
import {
  OnlineLessonProvider,
  useOnlineLessonActions,
  useOnlineLessonState,
} from '@/providers/learning/online_lessons';
import {
  AssessmentProvider,
  useAssessmentActions,
  useAssessmentState,
} from '@/providers/assessment/assessments';
import {
  AnnouncementProvider,
  useAnnouncementActions,
  useAnnouncementState,
} from '@/providers/communication/announcements';
import {
  TimetableSlotProvider,
  useTimetableSlotActions,
  useTimetableSlotState,
} from '@/providers/academic/timetable_slots';
import {
  AnnouncementPriority,
  AnnouncementAudience,
} from '@/providers/shared/enums';

const { Text } = Typography;

// ──────────────────────────────────────────────────────────────────
// Helpers
// ──────────────────────────────────────────────────────────────────

function toYMD(date: Date): string {
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

function formatTime(value?: string): string {
  if (!value) return '';
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return '';
  return d.toLocaleTimeString('en-ZA', { hour: '2-digit', minute: '2-digit' });
}

// Map AnnouncementPriority enum values to AntD colors.
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

// ──────────────────────────────────────────────────────────────────
// Content
// ──────────────────────────────────────────────────────────────────

function TeacherDashboardContent() {
  const router = useRouter();
  const { currentUser } = useAuthState();

  // The Teacher provider exposes `getByCurrentUserAsync` which hits a
  // tenant-aware backend endpoint that resolves by ABP session user rather
  // than relying on a keyword search over teacher fields.
  const { getByCurrentUserAsync } = useTeacherActions();
  const {
    teacher,
    isPending: teacherPending,
    isError: teacherError,
  } = useTeacherState();

  const { getByTeacherAsync: getMyClassSubjects } = useClassSubjectActions();
  const {
    classSubjects,
    isPending: classSubjectsPending,
    isError: classSubjectsError,
  } = useClassSubjectState();

  const { getActiveClassesAsync } = useClassActions();
  // `getActiveClassesAsync` stores its results into `state.classes` — see
  // providers/academic/classes/reducer for the success-handler.
  const {
    classes,
    isPending: classesPending,
    isError: classesError,
  } = useClassState();

  const { getAllAsync: getAllOnlineLessons } = useOnlineLessonActions();
  const {
    onlineLessons,
    isPending: lessonsPending,
    isError: lessonsError,
  } = useOnlineLessonState();

  const { getAllAsync: getAllAssessments } = useAssessmentActions();
  const {
    assessments,
    isPending: assessmentsPending,
    isError: assessmentsError,
  } = useAssessmentState();

  const { getAllAsync: getAllAnnouncements } = useAnnouncementActions();
  const {
    announcements,
    isPending: announcementsPending,
    isError: announcementsError,
  } = useAnnouncementState();

  const { getByTeacherAsync: getMySlots } = useTimetableSlotActions();
  const {
    timetableSlots,
    isPending: slotsPending,
    isError: slotsError,
  } = useTimetableSlotState();

  // ── Step 1: resolve the current user's teacher record via the dedicated
  // backend endpoint. Returns null in state when no teacher profile is
  // linked, which we surface via the `noTeacherProfile` banner below.
  useEffect(() => {
    if (currentUser?.id != null) {
      getByCurrentUserAsync();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentUser?.id]);

  // Guarding the second-step effect on `teacher.userId === currentUser.id`
  // (rather than `teacher` alone) prevents a stale-teacher race when the
  // active user changes mid-session.
  const teacherId =
    teacher && teacher.userId === currentUser?.id ? teacher.id : null;

  // ── Step 2: once we have a confirmed teacherId, fetch class subjects and
  // timetable slots for this teacher.
  useEffect(() => {
    if (teacherId) {
      getMyClassSubjects(teacherId);
      getMySlots(teacherId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [teacherId]);

  // ── Independent fetches (start in parallel once we know the user). Date
  // strings are intentionally NOT memoized — a tab left open across midnight
  // would otherwise keep querying yesterday's date.
  useEffect(() => {
    if (!currentUser?.id) return;
    const todayStr = toYMD(new Date());
    getActiveClassesAsync();
    getAllOnlineLessons({
      hostTeacherUserId: currentUser.id,
      startDate: todayStr,
      endDate: todayStr,
      maxResultCount: 100,
    });
    // Cap assessment list at 500 — adequate for the dashboard widget.
    // Filtering happens client-side against my class-subject IDs because
    // the backend `IGetAssessmentsInput` does not accept a teacher filter.
    getAllAssessments({ maxResultCount: 500 });
    getAllAnnouncements({ isPublished: true, maxResultCount: 6 });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentUser?.id]);

  // ── Derive metrics.
  const myClassIds = useMemo(() => {
    const ids = new Set<string>();
    (classSubjects ?? []).forEach((cs) => ids.add(cs.classId));
    return ids;
  }, [classSubjects]);

  const myClassSubjectIds = useMemo(() => {
    const ids = new Set<string>();
    (classSubjects ?? []).forEach((cs) => ids.add(cs.id));
    return ids;
  }, [classSubjects]);

  const myClasses = useMemo(
    () => (classes ?? []).filter((c) => myClassIds.has(c.id)),
    [classes, myClassIds]
  );

  const myClassesCount = myClassIds.size;
  const myStudentsCount = useMemo(
    () => myClasses.reduce((sum, c) => sum + (c.studentCount ?? 0), 0),
    [myClasses]
  );

  const pendingMarkSheetsCount = useMemo(() => {
    if (!assessments) return null;
    // "Pending" = an assessment for one of my class subjects whose marks
    // have not been released yet. `markCount` alone does not tell us how
    // many students should be marked (that requires server-side joins),
    // so we lean on `marksReleased` as the authoritative status flag.
    return assessments.filter(
      (a) => myClassSubjectIds.has(a.classSubjectId) && !a.marksReleased
    ).length;
  }, [assessments, myClassSubjectIds]);

  // Timetable slots for today — JS Date.getDay returns 0 (Sun) through 6
  // (Sat), matching the backend's stored dayOfWeek convention.
  const todayDayOfWeek = new Date().getDay();
  const todaysSlots = useMemo(
    () => (timetableSlots ?? []).filter((s) => s.dayOfWeek === todayDayOfWeek),
    [timetableSlots, todayDayOfWeek]
  );
  const todaysOnlineLessons = onlineLessons ?? [];
  const todaysLessonsCount = todaysOnlineLessons.length + todaysSlots.length;

  // Activity feed: today's lessons + timetable slots + announcements
  // targeted to teachers/all.
  type FeedItem =
    | { kind: 'lesson'; id: string; title: string; time: string; subtitle: string }
    | { kind: 'slot'; id: string; title: string; time: string; subtitle: string }
    | {
        kind: 'announcement';
        id: string;
        title: string;
        date: string;
        priority?: number;
        isPinned?: boolean;
      };

  const teacherAudiences = useMemo(
    () =>
      new Set<number>([
        AnnouncementAudience.All,
        AnnouncementAudience.Staff,
        AnnouncementAudience.Teachers,
      ]),
    []
  );

  const activityItems: FeedItem[] = useMemo(() => {
    const items: FeedItem[] = [];
    todaysOnlineLessons.forEach((l) => {
      items.push({
        kind: 'lesson',
        id: l.id,
        title: l.title,
        time: l.scheduledStartTime,
        subtitle: `${l.className ?? 'Class'} · ${l.subjectName ?? ''}`.trim(),
      });
    });
    todaysSlots.forEach((s) => {
      items.push({
        kind: 'slot',
        id: s.id,
        title: s.subjectName ?? `Period ${s.periodNumber}`,
        time: s.startTime,
        subtitle: s.roomNumber ? `Room ${s.roomNumber}` : 'Timetable slot',
      });
    });
    (announcements ?? [])
      .filter((a) =>
        teacherAudiences.has(a.targetAudience as AnnouncementAudience)
      )
      .forEach((a) => {
        items.push({
          kind: 'announcement',
          id: a.id,
          title: a.title,
          date: a.publishDate,
          priority: a.priority,
          isPinned: a.isPinned,
        });
      });
    return items.slice(0, 10);
  }, [todaysOnlineLessons, todaysSlots, announcements, teacherAudiences]);

  const handleCardKeyDown = (
    event: KeyboardEvent<HTMLDivElement>,
    target: string
  ) => {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      router.push(target);
    }
  };

  const renderStatCard = (
    title: string,
    value: number | string | null,
    icon: React.ReactNode,
    color: string,
    target: string,
    loading: boolean,
    error: boolean
  ) => {
    if (error) {
      return (
        <Card variant="borderless" style={{ borderTop: `4px solid ${color}` }}>
          <Statistic title={title} value="—" prefix={icon} valueStyle={{ color }} />
          <Alert
            type="warning"
            showIcon
            message="Could not load"
            style={{ marginTop: 8 }}
          />
        </Card>
      );
    }
    return (
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
          value={loading ? '—' : value ?? '—'}
          prefix={icon}
          valueStyle={{ color }}
          loading={loading}
        />
      </Card>
    );
  };

  // The teacher profile is "missing" only after the resolve call has
  // settled with a genuine null response from the backend. We require
  // `teacher === undefined` (the success-with-null state) AND a teacher
  // that hasn't been pending or errored. This guards against the flicker
  // during a transient mismatch (e.g. user switch mid-session, where
  // `teacherId` momentarily falls back to null while a fresh fetch runs).
  const teacherResolveSettled = !teacherPending && !teacherError;
  const noTeacherProfile =
    teacherResolveSettled && currentUser != null && teacher === undefined;

  // Bridge the gap between teacher-resolve completing and the dependent
  // classSubjects fetch actually firing: until classSubjects has loaded
  // (or errored) for a known teacherId, the dependent cards should keep
  // showing their loading state instead of flashing "0".
  const classSubjectsNotYetReady =
    teacherId != null && classSubjects === undefined && !classSubjectsError;

  return (
    <div>
      {noTeacherProfile && (
        <Alert
          type="warning"
          showIcon
          message="No teacher profile linked to your account"
          description="A school administrator needs to link your user to a teacher record before classes and lessons will appear."
          style={{ marginBottom: 16 }}
        />
      )}

      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col xs={24} sm={12} lg={6}>
          {renderStatCard(
            'My Classes',
            myClassesCount,
            <TeamOutlined />,
            '#2F54EB',
            '/teacher/classes',
            teacherPending || classSubjectsPending || classSubjectsNotYetReady,
            teacherError || classSubjectsError
          )}
        </Col>
        <Col xs={24} sm={12} lg={6}>
          {renderStatCard(
            'My Students',
            myStudentsCount,
            <BookOutlined />,
            '#52C41A',
            '/teacher/classes',
            classesPending || classSubjectsPending || classSubjectsNotYetReady,
            classesError || classSubjectsError
          )}
        </Col>
        <Col xs={24} sm={12} lg={6}>
          {renderStatCard(
            'Pending Mark Sheets',
            pendingMarkSheetsCount,
            <FileTextOutlined />,
            '#FAAD14',
            '/teacher/mark-sheets',
            assessmentsPending ||
              classSubjectsPending ||
              classSubjectsNotYetReady,
            assessmentsError || classSubjectsError
          )}
        </Col>
        <Col xs={24} sm={12} lg={6}>
          {renderStatCard(
            "Today's Lessons",
            todaysLessonsCount,
            <VideoCameraOutlined />,
            '#1890FF',
            '/teacher/schedule',
            lessonsPending || slotsPending,
            lessonsError || slotsError
          )}
        </Col>
      </Row>

      <Row gutter={[16, 16]}>
        <Col xs={24} lg={16}>
          <Card
            variant="borderless"
            title={
              <span>
                <CalendarOutlined style={{ marginRight: 8 }} />
                Today &amp; recent activity
              </span>
            }
            style={{ marginBottom: 16 }}
          >
            {(announcementsError || lessonsError || slotsError) && (
              <Alert
                type="warning"
                showIcon
                message="Some activity could not be loaded."
                style={{ marginBottom: 12 }}
              />
            )}
            {(lessonsPending || announcementsPending || slotsPending) &&
            activityItems.length === 0 ? (
              <Skeleton active paragraph={{ rows: 3 }} />
            ) : (
              <List<FeedItem>
                dataSource={activityItems}
                locale={{ emptyText: 'No upcoming lessons or recent announcements.' }}
                renderItem={(item) => {
                  if (item.kind === 'announcement') {
                    return (
                      <List.Item
                        extra={
                          <Tag
                            color={
                              priorityColors[
                                item.priority as AnnouncementPriority
                              ] ?? 'default'
                            }
                          >
                            {priorityLabels[
                              item.priority as AnnouncementPriority
                            ] ?? 'Normal'}
                          </Tag>
                        }
                      >
                        <List.Item.Meta
                          avatar={
                            <Badge
                              dot
                              color={item.isPinned ? 'gold' : 'magenta'}
                            />
                          }
                          title={<Text strong>{item.title}</Text>}
                          description={
                            <Text type="secondary" style={{ fontSize: 12 }}>
                              {formatDisplayDate(item.date)}
                            </Text>
                          }
                        />
                      </List.Item>
                    );
                  }
                  const dotColor = item.kind === 'lesson' ? 'blue' : 'cyan';
                  return (
                    <List.Item
                      extra={<Tag color="blue">{formatTime(item.time)}</Tag>}
                    >
                      <List.Item.Meta
                        avatar={<Badge dot color={dotColor} />}
                        title={<Text strong>{item.title}</Text>}
                        description={
                          <Text type="secondary" style={{ fontSize: 12 }}>
                            {item.subtitle}
                          </Text>
                        }
                      />
                    </List.Item>
                  );
                }}
              />
            )}
          </Card>
        </Col>

        <Col xs={24} lg={8}>
          <Card
            variant="borderless"
            title={
              <span>
                <FileTextOutlined style={{ marginRight: 8 }} />
                Quick Actions
              </span>
            }
          >
            <Button
              block
              icon={<TeamOutlined />}
              style={{ marginBottom: 8, textAlign: 'left' }}
              onClick={() => router.push('/teacher/classes')}
            >
              My Classes
            </Button>
            <Button
              block
              icon={<FileTextOutlined />}
              style={{ marginBottom: 8, textAlign: 'left' }}
              onClick={() => router.push('/teacher/mark-sheets')}
            >
              Record Marks
            </Button>
            <Button
              block
              icon={<BookOutlined />}
              style={{ marginBottom: 8, textAlign: 'left' }}
              onClick={() => router.push('/teacher/materials')}
            >
              Upload Material
            </Button>
            <Button
              block
              icon={<VideoCameraOutlined />}
              style={{ marginBottom: 8, textAlign: 'left' }}
              onClick={() => router.push('/teacher/lessons')}
            >
              Schedule Lesson
            </Button>
            <Button
              block
              icon={<NotificationOutlined />}
              style={{ textAlign: 'left' }}
              onClick={() => router.push('/teacher/messages')}
            >
              Messages
            </Button>
          </Card>
        </Col>
      </Row>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────
// Provider stack
// ──────────────────────────────────────────────────────────────────

export default function TeacherDashboard() {
  return (
    <TeacherProvider>
      <ClassSubjectProvider>
        <ClassProvider>
          <OnlineLessonProvider>
            <AssessmentProvider>
              <AnnouncementProvider>
                <TimetableSlotProvider>
                  <TeacherDashboardContent />
                </TimetableSlotProvider>
              </AnnouncementProvider>
            </AssessmentProvider>
          </OnlineLessonProvider>
        </ClassProvider>
      </ClassSubjectProvider>
    </TeacherProvider>
  );
}
