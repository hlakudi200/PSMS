'use client';

import { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Card,
  Col,
  Empty,
  Row,
  Skeleton,
  Space,
  Statistic,
  Table,
  Tabs,
  Tag,
  Typography,
} from 'antd';
import {
  ClockCircleOutlined,
  TeamOutlined,
  WarningOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { CoffeeOutlined } from '@ant-design/icons';
import { useAuthState } from '@/providers/auth';
import {
  TeacherProvider,
  useTeacherActions,
  useTeacherState,
} from '@/providers/academic/teachers';
import {
  TimetableSlotProvider,
  useTimetableSlotActions,
  useTimetableSlotState,
} from '@/providers/academic/timetable_slots';
import { detectBreaks } from '@/utils/timetable-grid';
import type { ITimetableSlotList } from '@/providers/academic/shared/interfaces';

const { Title, Text } = Typography;

// SA labor-law cap from business rule TT-005 (Supplementary). The dashboard's
// "workload" stat compares the teacher's weekly period count to this number.
const MAX_PERIODS_PER_WEEK = 35;

// JavaScript Date.getDay convention: 0 = Sunday … 6 = Saturday.
const DAY_LABELS_FULL: Record<number, string> = {
  0: 'Sunday',
  1: 'Monday',
  2: 'Tuesday',
  3: 'Wednesday',
  4: 'Thursday',
  5: 'Friday',
  6: 'Saturday',
};

// Default school week is Mon–Fri. Saturday is shown only if the data uses
// it. Sunday is intentionally excluded from the weekly grid.
const WEEKDAY_KEYS = [1, 2, 3, 4, 5] as const;

function formatTimeShort(value?: string): string {
  // The backend serializes TimeSpan as "HH:MM:SS" (e.g. "08:00:00"). We
  // present the short form; if upstream ever sends a full datetime string
  // we still slice safely.
  if (!value) return '';
  const parts = value.split(':');
  if (parts.length < 2) return value;
  return `${parts[0]}:${parts[1]}`;
}

interface ScheduleRow {
  periodNumber: number;
  startTime?: string;
  endTime?: string;
  /** True for a synthetic row rendered as a full-width "Break" bar. */
  isBreak?: boolean;
  // Slot per weekday, keyed by `day-<n>` so AntD Table dataIndex works.
  [key: `day-${number}`]: ITimetableSlotList | undefined;
}

function TeacherScheduleContent() {
  const { currentUser } = useAuthState();
  const [activeTab, setActiveTab] = useState<'today' | 'week'>('today');
  // `new Date().getDay()` at render time would differ between the SSR
  // shell (UTC) and the SA client (UTC+2), tripping React's hydration
  // check after 22:00 UTC. Resolve the day once on mount instead.
  const [todayDayOfWeek, setTodayDayOfWeek] = useState<number | null>(null);
  useEffect(() => {
    setTodayDayOfWeek(new Date().getDay());
  }, []);

  const { getByCurrentUserAsync } = useTeacherActions();
  const {
    teacher,
    isPending: teacherPending,
    isError: teacherError,
  } = useTeacherState();

  const { getByTeacherAsync: getMySlots } = useTimetableSlotActions();
  const {
    timetableSlots,
    isPending: slotsPending,
    isError: slotsError,
  } = useTimetableSlotState();

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
      getMySlots(teacherId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [teacherId]);

  const slots = useMemo(() => timetableSlots ?? [], [timetableSlots]);

  const todaysSlots = useMemo(
    () =>
      todayDayOfWeek == null
        ? []
        : slots
            .filter((s) => s.dayOfWeek === todayDayOfWeek)
            .slice()
            .sort((a, b) => a.periodNumber - b.periodNumber),
    [slots, todayDayOfWeek]
  );

  // Decide whether Saturday should appear in the weekly grid: only when the
  // data has slots on day-of-week 6.
  const showSaturday = useMemo(
    () => slots.some((s) => s.dayOfWeek === 6),
    [slots]
  );
  const weekDays = showSaturday
    ? [...WEEKDAY_KEYS, 6]
    : [...WEEKDAY_KEYS];

  // Build the weekly grid rows: one row per distinct period number, with a
  // column per day. The row's header time (startTime/endTime) is sampled
  // from the first matching slot — see SBR-TT-001: a school's period
  // schedule is defined per-day so period 1 always begins at the same time
  // across the week. If a future schema allows per-day time variance we'd
  // need to surface the time inside each day-cell instead of the header.
  const weeklyRows = useMemo<ScheduleRow[]>(() => {
    if (slots.length === 0) return [];
    const periodSet = new Set<number>();
    slots.forEach((s) => periodSet.add(s.periodNumber));
    const periods = Array.from(periodSet).sort((a, b) => a - b);

    const periodRows = periods.map((p) => {
      const slotsAtPeriod = slots.filter((s) => s.periodNumber === p);
      const sample = slotsAtPeriod[0];
      const row: ScheduleRow = {
        periodNumber: p,
        startTime: sample?.startTime,
        endTime: sample?.endTime,
      };
      weekDays.forEach((d) => {
        row[`day-${d}`] = slotsAtPeriod.find((s) => s.dayOfWeek === d);
      });
      return row;
    });

    // Breaks aren't stored data — infer them from the time gap between
    // consecutive periods so the grid shows a "Break" bar instead of an
    // unexplained gap.
    const breakRows: ScheduleRow[] = detectBreaks(slots).map((b) => ({
      periodNumber: b.sortOrder,
      isBreak: true,
      startTime: b.startTime,
      endTime: b.endTime,
    }));

    return [...periodRows, ...breakRows].sort((a, b) => a.periodNumber - b.periodNumber);
    // weekDays is stable per render via showSaturday, so the deps below are
    // sufficient — including weekDays directly would cause useMemo churn.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [slots, showSaturday]);

  // TT-005 counts every weekly recurring teaching period toward the
  // teacher's workload. Slots are returned per active timetable, so a
  // simple length-count across all returned slots (Mon-Sat) is the right
  // measure here.
  const periodsPerWeek = slots.length;
  const overloaded = periodsPerWeek > MAX_PERIODS_PER_WEEK;

  const renderSlotCell = (slot?: ITimetableSlotList) => {
    if (!slot) {
      // Screen-reader users navigating the weekly grid need an explicit
      // signal for "free period"; the em-dash glyph alone is poor signal.
      return (
        <span aria-label="No class">
          <Text type="secondary" aria-hidden>
            —
          </Text>
        </span>
      );
    }
    return (
      <div>
        <Text strong style={{ fontSize: 12 }}>
          {slot.subjectName ?? `Period ${slot.periodNumber}`}
        </Text>
        {slot.roomNumber && (
          <div>
            <Text type="secondary" style={{ fontSize: 11 }}>
              Room {slot.roomNumber}
            </Text>
          </div>
        )}
      </div>
    );
  };

  const weeklyColumns: ColumnsType<ScheduleRow> = useMemo(() => {
    const cols: ColumnsType<ScheduleRow> = [
      {
        title: 'Period',
        key: 'periodNumber',
        width: 90,
        render: (_: unknown, row: ScheduleRow) => {
          if (row.isBreak) {
            return {
              children: (
                <div style={{ textAlign: 'center', fontWeight: 600, color: '#ad6800' }}>
                  <CoffeeOutlined /> Break &nbsp;{formatTimeShort(row.startTime)}–{formatTimeShort(row.endTime)}
                </div>
              ),
              props: { colSpan: weekDays.length + 1 },
            };
          }
          return (
            <div>
              <Text strong>Period {row.periodNumber}</Text>
              {row.startTime && (
                <div>
                  <Text type="secondary" style={{ fontSize: 11 }}>
                    {formatTimeShort(row.startTime)}
                    {row.endTime ? `–${formatTimeShort(row.endTime)}` : ''}
                  </Text>
                </div>
              )}
            </div>
          );
        },
      },
    ];
    weekDays.forEach((d) => {
      cols.push({
        title: DAY_LABELS_FULL[d],
        key: `day-${d}`,
        render: (_: unknown, row: ScheduleRow) => {
          if (row.isBreak) return { children: null, props: { colSpan: 0 } };
          return renderSlotCell(row[`day-${d}`]);
        },
      });
    });
    return cols;
    // `weekDays` and `renderSlotCell` are both stable per render via the
    // single source of truth `showSaturday` — they're recreated each
    // render but reference no other shifting state, so the memo only
    // needs to re-run when `showSaturday` flips. Listing them in deps
    // would force unnecessary recomputation on every render.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [showSaturday]);

  const noTeacherProfile =
    !teacherPending &&
    !teacherError &&
    currentUser != null &&
    teacher === undefined;
  const loading = teacherPending || slotsPending;
  const anyError = teacherError || slotsError;

  const renderToday = () => {
    // todayDayOfWeek is null until the post-mount effect fires (avoids
    // SSR/CSR hydration drift). Show a skeleton until we know the day.
    if (todayDayOfWeek == null) return <Skeleton active paragraph={{ rows: 4 }} />;
    if (todaysSlots.length === 0) {
      // Weekend handling: Sunday is always non-teaching here; Saturday is
      // only non-teaching when the school's timetable doesn't use it.
      const isWeekend =
        todayDayOfWeek === 0 || (todayDayOfWeek === 6 && !showSaturday);
      return (
        <Empty
          description={
            isWeekend
              ? `Enjoy your weekend — no periods scheduled for ${DAY_LABELS_FULL[todayDayOfWeek]}.`
              : `No periods scheduled for ${DAY_LABELS_FULL[todayDayOfWeek]}.`
          }
        />
      );
    }
    return (
      <Table<ITimetableSlotList>
        dataSource={todaysSlots}
        rowKey="id"
        pagination={false}
        size="small"
        scroll={{ x: 560 }}
        columns={[
          {
            title: 'Period',
            dataIndex: 'periodNumber',
            key: 'periodNumber',
            width: 90,
            render: (n: number) => <Text strong>Period {n}</Text>,
          },
          {
            title: 'Time',
            key: 'time',
            width: 140,
            render: (_: unknown, s: ITimetableSlotList) =>
              `${formatTimeShort(s.startTime)} – ${formatTimeShort(s.endTime)}`,
          },
          {
            title: 'Subject',
            dataIndex: 'subjectName',
            key: 'subjectName',
            render: (v?: string) => v ?? <Text type="secondary">—</Text>,
          },
          {
            title: 'Room',
            dataIndex: 'roomNumber',
            key: 'roomNumber',
            width: 100,
            render: (v?: string) =>
              v ? <Tag color="blue">Room {v}</Tag> : <Text type="secondary">—</Text>,
          },
        ]}
      />
    );
  };

  const renderWeek = () => {
    if (weeklyRows.length === 0) {
      return <Empty description="No timetable slots assigned." />;
    }
    return (
      <Table<ScheduleRow>
        dataSource={weeklyRows}
        rowKey="periodNumber"
        pagination={false}
        size="small"
        columns={weeklyColumns}
        scroll={{ x: 800 }}
      />
    );
  };

  return (
    <div>
      <div style={{ marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>
          My Schedule
        </Title>
        <Text type="secondary">
          Your weekly teaching periods. The workload count below is governed
          by SA business rule TT-005 (max {MAX_PERIODS_PER_WEEK} periods per
          week).
        </Text>
      </div>

      {noTeacherProfile && (
        <Alert
          type="warning"
          showIcon
          message="No teacher profile linked to your account"
          description="Periods will appear once an administrator links your user to a teacher record and assigns timetable slots."
          style={{ marginBottom: 16 }}
        />
      )}

      {anyError && !loading && (
        <Alert
          type="warning"
          showIcon
          message="Your schedule could not be loaded. Please refresh."
          style={{ marginBottom: 16 }}
        />
      )}

      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card variant="borderless" style={{ borderTop: '4px solid #1890FF' }}>
            <Statistic
              title="Periods this week"
              value={loading ? '—' : periodsPerWeek}
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: overloaded ? '#cf1322' : '#1890FF' }}
              loading={loading}
            />
            {overloaded && (
              <Space size="small" style={{ marginTop: 4 }}>
                <WarningOutlined style={{ color: '#cf1322' }} />
                <Text type="danger" style={{ fontSize: 12 }}>
                  Exceeds the {MAX_PERIODS_PER_WEEK}-period weekly limit
                </Text>
              </Space>
            )}
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card variant="borderless" style={{ borderTop: '4px solid #52C41A' }}>
            <Statistic
              title="Today's periods"
              value={loading ? '—' : todaysSlots.length}
              prefix={<TeamOutlined />}
              valueStyle={{ color: '#52C41A' }}
              loading={loading}
            />
            <Text type="secondary" style={{ fontSize: 12 }}>
              {todayDayOfWeek == null ? '—' : DAY_LABELS_FULL[todayDayOfWeek]}
            </Text>
          </Card>
        </Col>
      </Row>

      <Card variant="borderless" styles={{ body: { padding: 0 } }}>
        <Tabs
          activeKey={activeTab}
          onChange={(k) => setActiveTab(k as 'today' | 'week')}
          style={{ padding: '0 16px' }}
          items={[
            {
              key: 'today',
              label:
                todayDayOfWeek == null
                  ? 'Today'
                  : `Today (${DAY_LABELS_FULL[todayDayOfWeek]})`,
              children: loading ? (
                <Skeleton active paragraph={{ rows: 4 }} />
              ) : (
                renderToday()
              ),
            },
            {
              key: 'week',
              label: 'Weekly view',
              children: loading ? (
                <Skeleton active paragraph={{ rows: 6 }} />
              ) : (
                renderWeek()
              ),
            },
          ]}
        />
      </Card>
    </div>
  );
}

export default function TeacherSchedulePageContent() {
  return (
    <TeacherProvider>
      <TimetableSlotProvider>
        <TeacherScheduleContent />
      </TimetableSlotProvider>
    </TeacherProvider>
  );
}
