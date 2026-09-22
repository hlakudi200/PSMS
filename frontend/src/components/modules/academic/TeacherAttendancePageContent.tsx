'use client';

import { Suspense, useCallback, useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'next/navigation';
import {
  Alert,
  Button,
  Card,
  Col,
  DatePicker,
  Empty,
  Input,
  Radio,
  Row,
  Select,
  Space,
  Table,
  Tag,
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
// Same palette as STATUS_COLOR's Tag colors, as hex — AntD's Segmented
// (and a plain Radio.Group) highlight the selected option with a white
// pill by default, which reads as blank against a light page background.
// Filling the selected button with its own status color instead keeps the
// picker legible and lets a teacher scan a filled register at a glance.
const STATUS_HEX: Record<number, string> = {
  1: '#52c41a',
  2: '#ff4d4f',
  3: '#faad14',
  4: '#1677ff',
};

const PRESENT = 1;
const ABSENT = 2;

interface AttendanceRow {
  studentId: string;
  studentName: string;
  existing?: IAttendanceList;
  status: number;
  notes: string;
  // True once a previously-captured row's status/notes has been changed
  // locally and not yet saved — drives which write path Save uses (see
  // handleSave: dirty rows go through Update, never a second Capture).
  dirty?: boolean;
}

function TeacherAttendanceContent() {
  const { currentUser } = useAuthState();
  const searchParams = useSearchParams();

  // Preselect the class when arriving from My Classes' "Attendance" quick
  // link (`?classId=...`) — previously ignored, so that button silently
  // dropped you on an empty class picker instead of the intended register.
  const [classId, setClassId] = useState<string | undefined>(
    () => searchParams.get('classId') ?? undefined
  );
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

  const { getByClassAndDateAsync, bulkCaptureAsync, updateAsync } = useAttendanceActions();
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

  // Build rows: un-recorded students default to Present; already-recorded
  // students hydrate from the server but stay editable (T-T21 — same-day
  // correction). A row already mid-edit (dirty, still pointing at the same
  // existing record) keeps its local draft instead of being clobbered by
  // this effect re-running for an unrelated reason.
  useEffect(() => {
    setRows((prev) => {
      const next: Record<string, AttendanceRow> = {};
      enrolled.forEach((sc) => {
        const existing = existingByStudent.get(sc.studentId);
        const draft = prev[sc.studentId];
        if (existing) {
          next[sc.studentId] =
            draft?.dirty && draft.existing?.id === existing.id
              ? { ...draft, studentName: sc.studentName ?? draft.studentName }
              : {
                  studentId: sc.studentId,
                  studentName: sc.studentName ?? 'Student',
                  existing,
                  status: existing.status,
                  notes: existing.notes ?? '',
                };
        } else {
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
    setRows((prev) => {
      const row = prev[studentId];
      const updated = { ...row, ...patch };
      // Mark dirty only when the values actually differ from the server
      // record — flipping a status back to its original value un-dirties
      // it, so an accidental toggle-and-toggle-back doesn't trigger a
      // no-op Update call.
      if (row?.existing) {
        updated.dirty =
          updated.status !== row.existing.status ||
          updated.notes.trim() !== (row.existing.notes ?? '');
      }
      return { ...prev, [studentId]: updated };
    });
  }, []);

  const rowList = useMemo(() => Object.values(rows), [rows]);
  const pendingEntries = useMemo(() => rowList.filter((r) => !r.existing), [rowList]);
  const dirtyEntries = useMemo(() => rowList.filter((r) => r.existing && r.dirty), [rowList]);

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
    if (pendingEntries.length === 0 && dirtyEntries.length === 0) {
      message.info('No attendance changes to save.');
      return;
    }

    const entries: IStudentAttendanceEntry[] = pendingEntries.map((r) => ({
      studentId: r.studentId,
      status: r.status,
      notes: r.notes.trim() || undefined,
    }));

    setSaving(true);
    try {
      // New students go through BulkCapture (one insert per student); a
      // same-day correction to an already-captured row goes through
      // Update instead — resubmitting it via BulkCapture would 409 on the
      // one-record-per-student-per-day duplicate check (T-T21).
      if (entries.length > 0) {
        await bulkCaptureAsync({
          classId,
          teacherId,
          attendanceDate: dateKey,
          entries,
        });
      }
      if (dirtyEntries.length > 0) {
        await Promise.all(
          dirtyEntries.map((r) =>
            updateAsync(r.existing!.id, {
              status: r.status,
              notes: r.notes.trim() || undefined,
            })
          )
        );
      }

      const parts: string[] = [];
      if (entries.length > 0) {
        const counts = entries.reduce<Record<number, number>>((acc, e) => {
          acc[e.status] = (acc[e.status] ?? 0) + 1;
          return acc;
        }, {});
        parts.push(
          Object.entries(counts)
            .map(([s, n]) => `${n} ${(STATUS_LABEL[Number(s)] ?? 'status').toLowerCase()}`)
            .join(', ')
        );
      }
      if (dirtyEntries.length > 0) {
        parts.push(`${dirtyEntries.length} corrected`);
      }
      message.success(`Saved — ${parts.join('; ')}`);
    } catch {
      // Surfaced by the axios interceptor (locked, duplicate, etc.)
    } finally {
      setSaving(false);
      // Always resync — even a partial failure (one write succeeded, the
      // other didn't) should be reflected rather than left stale.
      getByClassAndDateAsync(classId, dateKey);
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
          {row.existing && (
            <Tag color={row.dirty ? 'processing' : 'default'}>
              {row.dirty ? 'edited' : 'recorded'}
            </Tag>
          )}
        </Space>
      ),
    },
    {
      title: 'Status',
      key: 'status',
      width: 360,
      render: (_: unknown, row) => {
        // Locked rows (past dates, viewed read-only) show a plain tag —
        // and a student nobody actually recorded that day shows "No
        // record" rather than a disabled control pre-filled with Present,
        // which would misleadingly look like a real capture.
        if (locked && row.existing) {
          const color = STATUS_COLOR[row.status] ?? 'default';
          return <Tag color={color}>{STATUS_LABEL[row.status] ?? '—'}</Tag>;
        }
        if (locked && !row.existing) {
          return <Text type="secondary">No record</Text>;
        }
        return (
          <Radio.Group
            value={row.status}
            disabled={locked}
            optionType="button"
            buttonStyle="solid"
            onChange={(e) => updateRow(row.studentId, { status: Number(e.target.value) })}
          >
            {STATUS_OPTIONS.map((opt) => (
              <Radio.Button
                key={opt.value}
                value={opt.value}
                style={
                  row.status === opt.value
                    ? {
                        backgroundColor: STATUS_HEX[opt.value],
                        borderColor: STATUS_HEX[opt.value],
                        color: '#fff',
                      }
                    : undefined
                }
              >
                {opt.label}
              </Radio.Button>
            ))}
          </Radio.Group>
        );
      },
    },
    {
      title: 'Notes',
      key: 'notes',
      render: (_: unknown, row) => {
        if (locked && row.existing) {
          return row.existing.notes ? <Text type="secondary">{row.existing.notes}</Text> : <Text type="secondary">—</Text>;
        }
        if (locked && !row.existing) {
          return <Text type="secondary">—</Text>;
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
            Mark daily attendance for your class. You can correct any entry
            for as long as it&apos;s still today (AT-002) — the register
            locks automatically once the day ends.
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
            disabled={locked || !classId || (pendingEntries.length === 0 && dirtyEntries.length === 0)}
            onClick={handleSave}
          >
            Save
            {pendingEntries.length + dirtyEntries.length > 0
              ? ` (${pendingEntries.length + dirtyEntries.length})`
              : ''}
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
          message="Viewing a past register (read-only)"
          description="Editing is only allowed on the day itself. Ask an administrator to correct a past date."
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
              // On a locked (past) date, only rows with a real record count
              // — an unrecorded student defaults to a Present-shaped draft
              // that was never actually submitted, and tallying it would
              // misrepresent a day the register wasn't fully taken.
              const counted = locked ? rowList.filter((r) => r.existing) : rowList;
              const present = counted.filter((r) => r.status === PRESENT).length;
              const absent = counted.filter((r) => r.status === ABSENT).length;
              const noRecord = rowList.length - counted.length;
              return (
                <Space>
                  <CheckCircleOutlined />
                  <Text type="secondary">
                    {present} present · {absent} absent
                    {noRecord > 0 ? ` · ${noRecord} no record` : ''} · {rowList.length} total
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
              {/* useSearchParams (for the ?classId= deep link) requires a
                  Suspense boundary in the App Router. */}
              <Suspense fallback={null}>
                <TeacherAttendanceContent />
              </Suspense>
            </AttendanceProvider>
          </StudentClassProvider>
        </TeacherClassProvider>
      </ClassSubjectProvider>
    </TeacherProvider>
  );
}
