'use client';

import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  Card,
  Col,
  DatePicker,
  Empty,
  Input,
  Row,
  Segmented,
  Select,
  Space,
  Table,
  Tag,
  Tooltip,
  Typography,
  message,
} from 'antd';
import { CheckCircleOutlined, LockOutlined, SaveOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import dayjs, { Dayjs } from 'dayjs';
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
  TeacherClassProvider,
  useTeacherClassActions,
  useTeacherClassState,
} from '@/providers/academic/teacher_classes';
import {
  StudentClassProvider,
  useStudentClassActions,
  useStudentClassState,
} from '@/providers/academic/student_classes';
import {
  AttendanceProvider,
  useAttendanceActions,
  useAttendanceState,
} from '@/providers/academic/attendances';
import type { IStudentAttendanceEntry, IAttendanceList } from '@/providers/academic/shared/interfaces';

const { Title, Text } = Typography;

// AT-001 statuses. Mirrors psms.Domain.Shared.Enums.AttendanceStatus.
const STATUS_OPTIONS = [
  { value: 1, label: 'Present' },
  { value: 2, label: 'Absent' },
  { value: 3, label: 'Late' },
  { value: 4, label: 'Excused' },
];
const STATUS_LABEL: Record<number, string> = {
  1: 'Present',
  2: 'Absent',
  3: 'Late',
  4: 'Excused',
  5: 'Sick Leave',
  6: 'Holiday',
};
const STATUS_COLOR: Record<number, string> = {
  1: 'green',
  2: 'red',
  3: 'gold',
  4: 'blue',
  5: 'orange',
  6: 'default',
};

const PRESENT = 1;
const ABSENT = 2;

interface AttendanceRow {
  studentId: string;
  studentName: string;
  existing?: IAttendanceList;
  status: number;
  notes: string;
}

function TeacherAttendanceContent() {
  const { currentUser } = useAuthState();

  const [classId, setClassId] = useState<string | undefined>(undefined);
  const [date, setDate] = useState<Dayjs>(dayjs().startOf('day'));
  const [rows, setRows] = useState<Record<string, AttendanceRow>>({});
  const [saving, setSaving] = useState(false);

  const { getByCurrentUserAsync } = useTeacherActions();
  const { teacher, isPending: teacherPending, isError: teacherError } = useTeacherState();

  const { getByTeacherAsync: getMyClassSubjects } = useClassSubjectActions();
  const { classSubjects, isPending: classSubjectsPending } = useClassSubjectState();

  const { getByTeacherAsync: getMyTeacherClasses } = useTeacherClassActions();
  const { teacherClasses } = useTeacherClassState();

  const { getByClassAsync } = useStudentClassActions();
  const { studentClasses, isPending: studentsPending, isError: studentsError } = useStudentClassState();

  const { getByClassAndDateAsync, bulkCaptureAsync } = useAttendanceActions();
  const { attendances, isPending: attendancePending, isError: attendanceError } = useAttendanceState();

  useEffect(() => {
    if (currentUser?.id != null) getByCurrentUserAsync();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentUser?.id]);

  const teacherId = teacher && teacher.userId === currentUser?.id ? teacher.id : null;

  useEffect(() => {
    if (teacherId) {
      getMyClassSubjects(teacherId);
      getMyTeacherClasses(teacherId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [teacherId]);

  // Classes the teacher can take a register for: the union of classes they
  // teach a subject in AND classes they are the register/class teacher of
  // (US-TCH-011's primary user). De-duplicated by classId.
  const classOptions = useMemo(() => {
    const map = new Map<string, string>();
    (classSubjects ?? []).forEach((cs) => {
      if (!map.has(cs.classId)) map.set(cs.classId, cs.className ?? 'Class');
    });
    (teacherClasses ?? []).forEach((tc) => {
      if (!map.has(tc.classId)) map.set(tc.classId, tc.className ?? 'Class');
    });
    return Array.from(map.entries()).map(([value, label]) => ({ value, label }));
  }, [classSubjects, teacherClasses]);

  const dateKey = date.format('YYYY-MM-DD');

  // Load enrolled students + any existing attendance whenever class/date change.
  useEffect(() => {
    if (classId) {
      getByClassAsync(classId);
      getByClassAndDateAsync(classId, dateKey);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [classId, dateKey]);

  // AT-002 midnight lock (client mirror): a regular teacher can only capture
  // today's register. Past dates are locked (the server enforces this too;
  // only an admin with the ViewAll permission can backfill).
  const locked = date.isBefore(dayjs().startOf('day'), 'day');

  // Filter by classId too: provider state persists, so a fast class switch
  // could otherwise momentarily render the previous class's students.
  const enrolled = useMemo(
    () => (studentClasses ?? []).filter((sc) => sc.isActive && sc.isCurrent && sc.classId === classId),
    [studentClasses, classId]
  );

  const existingByStudent = useMemo(() => {
    const map = new Map<string, IAttendanceList>();
    (attendances ?? [])
      .filter((a) => a.classId === classId && dayjs(a.attendanceDate).format('YYYY-MM-DD') === dateKey)
      .forEach((a) => map.set(a.studentId, a));
    return map;
  }, [attendances, classId, dateKey]);

  // Build rows: existing records read-only; un-recorded students default to
  // Present and are editable.
  useEffect(() => {
    setRows((prev) => {
      const next: Record<string, AttendanceRow> = {};
      enrolled.forEach((sc) => {
        const existing = existingByStudent.get(sc.studentId);
        if (existing) {
          next[sc.studentId] = {
            studentId: sc.studentId,
            studentName: sc.studentName ?? 'Student',
            existing,
            status: existing.status,
            notes: existing.notes ?? '',
          };
        } else {
          const draft = prev[sc.studentId];
          next[sc.studentId] =
            draft && !draft.existing
              ? { ...draft, studentName: sc.studentName ?? draft.studentName }
              : {
                  studentId: sc.studentId,
                  studentName: sc.studentName ?? 'Student',
                  status: PRESENT,
                  notes: '',
                };
        }
      });
      return next;
    });
  }, [enrolled, existingByStudent]);

  const updateRow = useCallback((studentId: string, patch: Partial<AttendanceRow>) => {
    setRows((prev) => ({ ...prev, [studentId]: { ...prev[studentId], ...patch } }));
  }, []);

  const rowList = useMemo(() => Object.values(rows), [rows]);
  const pendingEntries = useMemo(() => rowList.filter((r) => !r.existing), [rowList]);

  const markAllPresent = () => {
    setRows((prev) => {
      const next = { ...prev };
      Object.values(next).forEach((r) => {
        if (!r.existing) next[r.studentId] = { ...r, status: PRESENT };
      });
      return next;
    });
  };

  const handleSave = async () => {
    if (locked || !classId || !teacherId) return;
    if (pendingEntries.length === 0) {
      message.info('No new attendance to save.');
      return;
    }

    const entries: IStudentAttendanceEntry[] = pendingEntries.map((r) => ({
      studentId: r.studentId,
      status: r.status,
      notes: r.notes.trim() || undefined,
    }));

    setSaving(true);
    try {
      await bulkCaptureAsync({
        classId,
        teacherId,
        attendanceDate: dateKey,
        entries,
      });
      // Save summary, e.g. "28 present, 2 absent, 1 late".
      const counts = entries.reduce<Record<number, number>>((acc, e) => {
        acc[e.status] = (acc[e.status] ?? 0) + 1;
        return acc;
      }, {});
      const summary = Object.entries(counts)
        .map(([s, n]) => `${n} ${(STATUS_LABEL[Number(s)] ?? 'status').toLowerCase()}`)
        .join(', ');
      message.success(`Saved — ${summary}`);
      getByClassAndDateAsync(classId, dateKey);
    } catch {
      // Surfaced by the axios interceptor (locked, duplicate, etc.)
    } finally {
      setSaving(false);
    }
  };

  const noTeacherProfile =
    !teacherPending && !teacherError && currentUser != null && teacher === undefined;
  const loading = classSubjectsPending || studentsPending || attendancePending;
  const loadError = studentsError || attendanceError;
  const hasClasses = classOptions.length > 0;

  const columns: ColumnsType<AttendanceRow> = [
    {
      title: 'Student',
      dataIndex: 'studentName',
      key: 'studentName',
      render: (v: string, row) => (
        <Space>
          <Text strong>{v}</Text>
          {row.existing && <Tag color="default">recorded</Tag>}
        </Space>
      ),
    },
    {
      title: 'Status',
      key: 'status',
      width: 360,
      render: (_: unknown, row) => {
        if (row.existing) {
          const color = STATUS_COLOR[row.status] ?? 'default';
          return <Tag color={color}>{STATUS_LABEL[row.status] ?? '—'}</Tag>;
        }
        return (
          <Segmented
            value={row.status}
            disabled={locked}
            onChange={(v) => updateRow(row.studentId, { status: Number(v) })}
            options={STATUS_OPTIONS}
          />
        );
      },
    },
    {
      title: 'Notes',
      key: 'notes',
      render: (_: unknown, row) => {
        if (row.existing) {
          return row.existing.notes ? <Text type="secondary">{row.existing.notes}</Text> : <Text type="secondary">—</Text>;
        }
        return (
          <Input
            value={row.notes}
            disabled={locked}
            maxLength={500}
            placeholder={row.status !== PRESENT ? 'Reason (recommended)' : 'Optional'}
            onChange={(e) => updateRow(row.studentId, { notes: e.target.value })}
          />
        );
      },
    },
  ];

  return (
    <div>
      <Space style={{ width: '100%', justifyContent: 'space-between', marginBottom: 16 }} align="start">
        <div>
          <Title level={4} style={{ margin: 0 }}>
            Class Attendance Register
          </Title>
          <Text type="secondary">
            Mark daily attendance for your class. Registers can only be captured
            on the day itself (AT-002) — past dates lock automatically.
          </Text>
        </div>
        <Space>
          <Button onClick={markAllPresent} disabled={locked || pendingEntries.length === 0}>
            Mark all present
          </Button>
          <Button
            type="primary"
            icon={<SaveOutlined />}
            loading={saving}
            disabled={locked || !classId || pendingEntries.length === 0}
            onClick={handleSave}
          >
            Save register{pendingEntries.length > 0 ? ` (${pendingEntries.length})` : ''}
          </Button>
        </Space>
      </Space>

      {noTeacherProfile && (
        <Alert
          type="warning"
          showIcon
          message="No teacher profile linked to your account"
          description="A school administrator needs to link your user to a teacher record."
          style={{ marginBottom: 16 }}
        />
      )}

      {!loading && !noTeacherProfile && !hasClasses && (
        <Alert
          type="info"
          showIcon
          message="No classes assigned"
          description="An administrator needs to assign you to a class before you can capture attendance."
          style={{ marginBottom: 16 }}
        />
      )}

      <Card variant="borderless" size="small" style={{ marginBottom: 16 }}>
        <Row gutter={[12, 12]} align="middle">
          <Col xs={24} md={10}>
            <Select
              placeholder="Select a class"
              showSearch
              optionFilterProp="label"
              value={classId}
              onChange={setClassId}
              options={classOptions}
              style={{ width: '100%' }}
            />
          </Col>
          <Col xs={24} md={8}>
            <DatePicker
              value={date}
              onChange={(d) => d && setDate(d.startOf('day'))}
              allowClear={false}
              format="YYYY-MM-DD"
              // AT: no future dates.
              disabledDate={(current) => current && current.isAfter(dayjs().endOf('day'))}
              style={{ width: '100%' }}
            />
          </Col>
        </Row>
      </Card>

      {loadError && !loading && (
        <Alert
          type="warning"
          showIcon
          message="Some data could not be loaded. Re-select the class or refresh."
          style={{ marginBottom: 16 }}
        />
      )}

      {locked && classId && (
        <Alert
          type="info"
          role="status"
          showIcon
          icon={<LockOutlined />}
          message="This date is locked"
          description="Attendance can only be captured on the day itself. Ask an administrator to capture or correct a past date."
          style={{ marginBottom: 16 }}
        />
      )}

      {!classId ? (
        <Card variant="borderless">
          <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Select a class to take the register." />
        </Card>
      ) : (
        <Card variant="borderless" styles={{ body: { padding: 0 } }}>
          <Table<AttendanceRow>
            dataSource={rowList}
            rowKey="studentId"
            loading={loading}
            pagination={{ pageSize: 50, showTotal: (t) => `${t} students` }}
            size="small"
            columns={columns}
            scroll={{ x: 720 }}
            locale={{
              emptyText: (
                <Empty
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description="No students enrolled in this class."
                />
              ),
            }}
            footer={() => {
              const present = rowList.filter((r) => r.status === PRESENT).length;
              const absent = rowList.filter((r) => r.status === ABSENT).length;
              return (
                <Space>
                  <CheckCircleOutlined />
                  <Text type="secondary">
                    {present} present · {absent} absent · {rowList.length} total
                  </Text>
                </Space>
              );
            }}
          />
        </Card>
      )}
    </div>
  );
}

export default function TeacherAttendancePageContent() {
  return (
    <TeacherProvider>
      <ClassSubjectProvider>
        <TeacherClassProvider>
          <StudentClassProvider>
            <AttendanceProvider>
              <TeacherAttendanceContent />
            </AttendanceProvider>
          </StudentClassProvider>
        </TeacherClassProvider>
      </ClassSubjectProvider>
    </TeacherProvider>
  );
}
