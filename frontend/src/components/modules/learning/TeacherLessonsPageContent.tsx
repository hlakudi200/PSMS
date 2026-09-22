'use client';

import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  Card,
  Col,
  Empty,
  Input,
  Popconfirm,
  Row,
  Select,
  Space,
  Statistic,
  Table,
  Tag,
  Tooltip,
  Typography,
  message,
} from 'antd';
import {
  CalendarOutlined,
  ClockCircleOutlined,
  CloseCircleOutlined,
  EditOutlined,
  PlayCircleOutlined,
  PlusOutlined,
  ReloadOutlined,
  VideoCameraOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useRouter } from 'next/navigation';
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
  OnlineLessonProvider,
  useOnlineLessonActions,
  useOnlineLessonState,
} from '@/providers/learning/online_lessons';
import { LearningMaterialProvider } from '@/providers/learning/learning_materials';
import type { IClassSubjectList } from '@/providers/academic/shared/interfaces';
import type {
  IOnlineLessonList,
} from '@/providers/learning/shared/interfaces';
import { ONLINE_LESSON_STATUS } from '@/providers/learning/shared/online-lesson-status';
import { ScheduleLessonModal } from '@/components/modals/learning/ScheduleLessonModal';

const { Title, Text } = Typography;

// Mirrors backend psms.Domain.Shared.Enums.OnlineLessonStatus.
const STATUS_META: Record<number, { label: string; color: string }> = {
  1: { label: 'Scheduled', color: 'blue' },
  2: { label: 'In progress', color: 'gold' },
  3: { label: 'Completed', color: 'green' },
  4: { label: 'Cancelled', color: 'red' },
};

const PLATFORM_INAPP = 7;
const PLATFORM_LABEL: Record<number, string> = {
  1: 'Zoom',
  2: 'Microsoft Teams',
  3: 'Google Meet',
  4: 'BigBlueButton',
  5: 'WebEx',
  6: 'Other',
  7: 'In-App Live',
};

function formatRange(startIso: string, endIso: string): string {
  try {
    const start = new Date(startIso);
    const end = new Date(endIso);
    const date = start.toLocaleDateString('en-ZA', {
      weekday: 'short',
      year: 'numeric',
      month: 'short',
      day: '2-digit',
    });
    const t = (d: Date) =>
      d.toLocaleTimeString('en-ZA', { hour: '2-digit', minute: '2-digit' });
    return `${date} · ${t(start)}–${t(end)}`;
  } catch {
    return `${startIso} – ${endIso}`;
  }
}

function TeacherLessonsContent() {
  const router = useRouter();
  const { currentUser } = useAuthState();
  const [scheduleOpen, setScheduleOpen] = useState(false);
  // Lesson being edited. The list row lacks the description, meeting
  // details and materials, so Edit fetches the full lesson first and the
  // modal opens once it has arrived.
  const [editingId, setEditingId] = useState<string | null>(null);

  // Filter strip state — server-side keyword + class-subject + status.
  const [searchKeyword, setSearchKeyword] = useState('');
  const [debouncedKeyword, setDebouncedKeyword] = useState('');
  const [filterClassSubjectId, setFilterClassSubjectId] = useState<string | undefined>(undefined);
  const [filterStatus, setFilterStatus] = useState<number | undefined>(undefined);

  useEffect(() => {
    const handle = setTimeout(() => setDebouncedKeyword(searchKeyword), 300);
    return () => clearTimeout(handle);
  }, [searchKeyword]);

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

  const {
    getAllAsync: getAllLessons,
    getAsync: getLesson,
    cancelAsync,
  } = useOnlineLessonActions();
  const {
    onlineLesson,
    onlineLessons,
    isPending: lessonsPending,
    isError: lessonsError,
  } = useOnlineLessonState();

  const editingLesson =
    editingId && onlineLesson?.id === editingId ? onlineLesson : undefined;

  useEffect(() => {
    if (currentUser?.id != null) {
      getByCurrentUserAsync();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentUser?.id]);

  const teacherId =
    teacher && teacher.userId === currentUser?.id ? teacher.id : null;

  useEffect(() => {
    if (teacherId) {
      getMyClassSubjects(teacherId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [teacherId]);

  const refreshLessons = useCallback(() => {
    if ((classSubjects?.length ?? 0) === 0) return;
    // mineOnly=true makes the server scope to lessons attached to the
    // calling teacher's class-subjects. Without it, totalCount and
    // upcomingCount would over-report (and another teacher's lesson titles
    // would briefly land in provider state before the client-side filter).
    getAllLessons({
      maxResultCount: 500,
      classSubjectId: filterClassSubjectId,
      status: filterStatus,
      keyword: debouncedKeyword.trim() || undefined,
      mineOnly: true,
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [classSubjects?.length, filterClassSubjectId, filterStatus, debouncedKeyword]);

  useEffect(() => {
    refreshLessons();
  }, [refreshLessons]);

  const myClassSubjectIds = useMemo(() => {
    const set = new Set<string>();
    (classSubjects ?? []).forEach((cs) => set.add(cs.id));
    return set;
  }, [classSubjects]);

  const classSubjectsById = useMemo(() => {
    const map = new Map<string, IClassSubjectList>();
    (classSubjects ?? []).forEach((cs) => map.set(cs.id, cs));
    return map;
  }, [classSubjects]);

  // Client-side scope: only show lessons attached to class-subjects this
  // teacher teaches. The backend enforces this on every mutation, but the
  // shared GetAll currently lists everything — gate visibility here too.
  const myLessons = useMemo(
    () =>
      (onlineLessons ?? []).filter((ol) =>
        myClassSubjectIds.has(ol.classSubjectId)
      ),
    [onlineLessons, myClassSubjectIds]
  );

  const upcomingCount = useMemo(
    () =>
      myLessons.filter(
        (l) =>
          l.status === ONLINE_LESSON_STATUS.Scheduled &&
          new Date(l.scheduledStartTime) >= new Date()
      ).length,
    [myLessons]
  );

  const noTeacherProfile =
    !teacherPending &&
    !teacherError &&
    currentUser != null &&
    teacher === undefined;
  const loading = teacherPending || classSubjectsPending || lessonsPending;
  const anyError = teacherError || classSubjectsError || lessonsError;
  const hasClassSubjects = (classSubjects?.length ?? 0) > 0;

  const classSubjectOptions = useMemo(
    () =>
      (classSubjects ?? []).map((cs) => ({
        value: cs.id,
        label: `${cs.className ?? 'Class'} — ${cs.subjectName ?? 'Subject'}`,
      })),
    [classSubjects]
  );

  const handleCancel = async (record: IOnlineLessonList) => {
    try {
      await cancelAsync(record.id);
      message.success(`Cancelled "${record.title}"`);
      refreshLessons();
    } catch {
      // Surfaced by axios interceptor
    }
  };

  const handleEdit = (record: IOnlineLessonList) => {
    setEditingId(record.id);
    getLesson(record.id);
  };

  const handleOpen = (record: IOnlineLessonList) => {
    // Route into the host shell where Start/End/Cancel and the meeting
    // link live. Same surface for Scheduled (where Start is gated by the
    // 15-min lead window) and InProgress lessons.
    router.push(`/teacher/lessons/${record.id}`);
  };

  const columns: ColumnsType<IOnlineLessonList> = [
    {
      title: 'Title',
      dataIndex: 'title',
      key: 'title',
      render: (v: string) => <Text strong>{v}</Text>,
    },
    {
      title: 'Class — Subject',
      key: 'classSubject',
      render: (_: unknown, row: IOnlineLessonList) => {
        const cs = classSubjectsById.get(row.classSubjectId);
        if (!cs) return <Text type="secondary">—</Text>;
        return (
          <span>
            {cs.className ?? 'Class'}{' '}
            <Text type="secondary">·</Text>{' '}
            {cs.subjectName ?? 'Subject'}
          </span>
        );
      },
    },
    {
      title: 'When',
      key: 'when',
      width: 280,
      render: (_: unknown, row: IOnlineLessonList) => (
        <Space direction="vertical" size={0}>
          <Text>{formatRange(row.scheduledStartTime, row.scheduledEndTime)}</Text>
          <Text type="secondary" style={{ fontSize: 12 }}>
            <ClockCircleOutlined /> {row.durationMinutes} min
          </Text>
        </Space>
      ),
    },
    {
      title: 'Platform',
      dataIndex: 'platform',
      key: 'platform',
      width: 140,
      render: (n: number) => <Tag>{PLATFORM_LABEL[n] ?? 'Other'}</Tag>,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 130,
      render: (s: number) => {
        const meta = STATUS_META[s];
        return meta ? <Tag color={meta.color}>{meta.label}</Tag> : <Tag>Unknown</Tag>;
      },
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 260,
      render: (_: unknown, record: IOnlineLessonList) => (
        <Space size="small">
          {record.platform === PLATFORM_INAPP
            && (record.status === ONLINE_LESSON_STATUS.Scheduled
              || record.status === ONLINE_LESSON_STATUS.InProgress) && (
            <Tooltip title="Join the in-app live classroom">
              <Button
                size="small"
                type="primary"
                icon={<VideoCameraOutlined />}
                aria-label={`Join live class ${record.title}`}
                onClick={() => router.push(`/live-class/${record.id}`)}
              >
                Live
              </Button>
            </Tooltip>
          )}
          {(record.status === ONLINE_LESSON_STATUS.Scheduled
            || record.status === ONLINE_LESSON_STATUS.InProgress) && (
            <Tooltip title="Open lesson">
              <Button
                size="small"
                type={record.platform === PLATFORM_INAPP ? 'default' : 'primary'}
                icon={<PlayCircleOutlined />}
                aria-label={`Open ${record.title}`}
                onClick={() => handleOpen(record)}
              >
                Open
              </Button>
            </Tooltip>
          )}
          {record.status === ONLINE_LESSON_STATUS.Scheduled && (
            <Tooltip title="Edit or reschedule">
              <Button
                size="small"
                icon={<EditOutlined />}
                aria-label={`Edit ${record.title}`}
                loading={editingId === record.id && !editingLesson && lessonsPending}
                onClick={() => handleEdit(record)}
              />
            </Tooltip>
          )}
          {record.status === ONLINE_LESSON_STATUS.Scheduled && (
            <Popconfirm
              title={`Cancel "${record.title}"?`}
              description="Students will no longer see this lesson on their schedule."
              onConfirm={() => handleCancel(record)}
              okText="Cancel lesson"
              cancelText="Keep"
            >
              <Tooltip title="Cancel">
                <Button
                  size="small"
                  danger
                  icon={<CloseCircleOutlined />}
                  aria-label={`Cancel ${record.title}`}
                />
              </Tooltip>
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div>
      <Space
        style={{ width: '100%', justifyContent: 'space-between', marginBottom: 16 }}
        align="start"
      >
        <div>
          <Title level={4} style={{ margin: 0 }}>
            Online Lessons
          </Title>
          <Text type="secondary">
            Schedule live lessons for your class-subjects. Lessons must be
            booked at least 24 h ahead, within school hours (OL-001).
          </Text>
        </div>
        <Space>
          <Tooltip title="Refresh">
            <Button
              icon={<ReloadOutlined />}
              onClick={refreshLessons}
              aria-label="Refresh lessons"
            />
          </Tooltip>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            disabled={!hasClassSubjects || loading}
            onClick={() => setScheduleOpen(true)}
          >
            Schedule lesson
          </Button>
        </Space>
      </Space>

      {noTeacherProfile && (
        <Alert
          type="warning"
          showIcon
          message="No teacher profile linked to your account"
          description="A school administrator needs to link your user to a teacher record before lessons appear."
          style={{ marginBottom: 16 }}
        />
      )}

      {anyError && !loading && (
        <Alert
          type="warning"
          showIcon
          message="Some lesson data could not be loaded. Refresh to retry."
          style={{ marginBottom: 16 }}
        />
      )}

      {!loading && !noTeacherProfile && !hasClassSubjects && (
        <Alert
          type="info"
          showIcon
          message="No class-subjects assigned"
          description="An administrator needs to assign you to classes and subjects before you can schedule lessons."
          style={{ marginBottom: 16 }}
        />
      )}

      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col xs={24} md={8}>
          <Card variant="borderless" style={{ borderTop: '4px solid #1890FF' }}>
            <Statistic
              title="My lessons"
              value={loading ? '—' : myLessons.length}
              prefix={<VideoCameraOutlined />}
              valueStyle={{ color: '#1890FF' }}
            />
          </Card>
        </Col>
        <Col xs={24} md={8}>
          <Card variant="borderless" style={{ borderTop: '4px solid #722ED1' }}>
            <Statistic
              title="Upcoming"
              value={loading ? '—' : upcomingCount}
              prefix={<CalendarOutlined />}
              valueStyle={{ color: '#722ED1' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Filter strip */}
      <Card variant="borderless" size="small" style={{ marginBottom: 16 }}>
        <Row gutter={[12, 12]} align="middle">
          <Col xs={24} md={8}>
            <Input.Search
              placeholder="Search by title…"
              allowClear
              value={searchKeyword}
              onChange={(e) => setSearchKeyword(e.target.value)}
              onSearch={(v) => setSearchKeyword(v)}
            />
          </Col>
          <Col xs={24} md={8}>
            <Select
              placeholder="Class & subject"
              allowClear
              showSearch
              optionFilterProp="label"
              value={filterClassSubjectId}
              onChange={setFilterClassSubjectId}
              options={classSubjectOptions}
              style={{ width: '100%' }}
            />
          </Col>
          <Col xs={24} md={8}>
            <Select
              placeholder="All statuses"
              allowClear
              value={filterStatus}
              onChange={setFilterStatus}
              options={Object.entries(STATUS_META).map(([k, v]) => ({
                value: Number(k),
                label: v.label,
              }))}
              style={{ width: '100%' }}
            />
          </Col>
        </Row>
      </Card>

      <Card variant="borderless" styles={{ body: { padding: 0 } }}>
        <Table<IOnlineLessonList>
          dataSource={myLessons}
          rowKey="id"
          loading={loading}
          pagination={{ pageSize: 20 }}
          size="small"
          columns={columns}
          scroll={{ x: 920 }}
          locale={{
            emptyText: hasClassSubjects ? (
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description="No lessons match the current filters."
              />
            ) : (
              <Empty description="No lessons" />
            ),
          }}
        />
      </Card>

      <ScheduleLessonModal
        open={scheduleOpen}
        classSubjects={classSubjects ?? []}
        onClose={(refresh) => {
          setScheduleOpen(false);
          if (refresh) refreshLessons();
        }}
      />

      <ScheduleLessonModal
        open={!!editingLesson}
        lesson={editingLesson}
        classSubjects={classSubjects ?? []}
        onClose={(refresh) => {
          setEditingId(null);
          if (refresh) refreshLessons();
        }}
      />
    </div>
  );
}

export default function TeacherLessonsPageContent() {
  return (
    <TeacherProvider>
      <ClassSubjectProvider>
        <OnlineLessonProvider>
          <LearningMaterialProvider>
            <TeacherLessonsContent />
          </LearningMaterialProvider>
        </OnlineLessonProvider>
      </ClassSubjectProvider>
    </TeacherProvider>
  );
}
