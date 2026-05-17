'use client';

import { useEffect, useMemo } from 'react';
import { useRouter } from 'next/navigation';
import {
  Alert,
  Button,
  Card,
  Col,
  Empty,
  Row,
  Skeleton,
  Space,
  Tag,
  Typography,
} from 'antd';
import {
  CheckSquareOutlined,
  FileTextOutlined,
  TeamOutlined,
  EyeOutlined,
  BookOutlined,
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
import type {
  IClass,
  IClassSubjectList,
} from '@/providers/academic/shared/interfaces';

const { Title, Text } = Typography;

// Group a list of ClassSubject rows by class id and aggregate per-class
// metadata. Subjects are keyed by `subjectId` (not name) so that two
// distinct subject offerings sharing the same display name (e.g. theory
// + practical splits) are preserved and React keys stay unique.
interface GroupedSubject {
  subjectId: string;
  subjectName: string;
}

interface GroupedClass {
  classId: string;
  className: string;
  subjects: GroupedSubject[];
  isClassTeacher: boolean;
}

function groupClassSubjects(
  classSubjects: IClassSubjectList[]
): GroupedClass[] {
  const byClass = new Map<string, GroupedClass>();
  const seenSubjectIds = new Map<string, Set<string>>();

  classSubjects.forEach((cs) => {
    let group = byClass.get(cs.classId);
    if (!group) {
      group = {
        classId: cs.classId,
        className: cs.className ?? 'Class',
        subjects: [],
        // `isClassTeacher` is not on IClassSubjectList; we'd need ITeacherClass
        // for that flag. For now leave false — surfacing it is a follow-up.
        isClassTeacher: false,
      };
      byClass.set(cs.classId, group);
      seenSubjectIds.set(cs.classId, new Set());
    }
    const seen = seenSubjectIds.get(cs.classId)!;
    if (!seen.has(cs.subjectId)) {
      seen.add(cs.subjectId);
      group.subjects.push({
        subjectId: cs.subjectId,
        subjectName: cs.subjectName ?? 'Subject',
      });
    }
  });
  return Array.from(byClass.values()).sort((a, b) =>
    a.className.localeCompare(b.className)
  );
}

function TeacherMyClassesContent() {
  const router = useRouter();
  const { currentUser } = useAuthState();

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
  const {
    classes,
    isPending: classesPending,
    isError: classesError,
  } = useClassState();

  // Step 1 — resolve the current user's teacher record.
  useEffect(() => {
    if (currentUser?.id != null) {
      getByCurrentUserAsync();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentUser?.id]);

  const teacherId =
    teacher && teacher.userId === currentUser?.id ? teacher.id : null;

  // Step 2 — once we have a teacherId, fetch class subjects + active classes.
  useEffect(() => {
    if (teacherId) {
      getMyClassSubjects(teacherId);
      // We pull all active classes once and filter client-side to the
      // teacher's class ids — fewer round trips than per-class lookups.
      getActiveClassesAsync();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [teacherId]);

  const grouped = useMemo(
    () => groupClassSubjects(classSubjects ?? []),
    [classSubjects]
  );

  const classesById = useMemo(() => {
    const map = new Map<string, IClass>();
    (classes ?? []).forEach((c) => map.set(c.id, c));
    return map;
  }, [classes]);

  const classSubjectsNotYetReady =
    teacherId != null && classSubjects === undefined && !classSubjectsError;
  const classesNotYetReady =
    teacherId != null && classes === undefined && !classesError;

  const loading =
    teacherPending ||
    classSubjectsPending ||
    classesPending ||
    classSubjectsNotYetReady ||
    classesNotYetReady;

  const anyError = teacherError || classSubjectsError || classesError;

  const noTeacherProfile =
    !teacherPending &&
    !teacherError &&
    currentUser != null &&
    teacher === undefined;

  return (
    <div>
      <div style={{ marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>
          My Classes
        </Title>
        <Text type="secondary">
          Classes you are assigned to teach. Use the quick links to take
          attendance, capture marks, or open the class roster.
        </Text>
      </div>

      {noTeacherProfile && (
        <Alert
          type="warning"
          showIcon
          message="No teacher profile linked to your account"
          description="A school administrator needs to link your user to a teacher record before your classes will appear."
          style={{ marginBottom: 16 }}
        />
      )}

      {anyError && !loading && (
        <Alert
          type="warning"
          showIcon
          message="Some of your class data could not be loaded."
          description="Refresh the page; if the problem persists, contact your administrator."
          style={{ marginBottom: 16 }}
        />
      )}

      {loading ? (
        <Row gutter={[16, 16]}>
          {/* Once the teacher record resolves we know the number of class
              assignments, so we can size the skeleton grid to match.
              Fallback of 3 covers the pre-teacher-resolve frame. */}
          {Array.from({
            length: teacher?.classAssignmentCount ?? 3,
          }).map((_, i) => (
            <Col xs={24} md={12} lg={8} key={i}>
              <Card variant="borderless">
                <Skeleton active paragraph={{ rows: 3 }} />
              </Card>
            </Col>
          ))}
        </Row>
      ) : grouped.length === 0 ? (
        <Card variant="borderless">
          <Empty
            description={
              noTeacherProfile
                ? 'No classes will appear until your teacher profile is linked.'
                : 'You have no classes assigned yet. Ask an administrator to assign you to classes and subjects.'
            }
          />
        </Card>
      ) : (
        <Row gutter={[16, 16]}>
          {grouped.map((g) => {
            const cls = classesById.get(g.classId);
            const gradeName = cls?.gradeName ?? '—';
            const studentCount = cls?.studentCount ?? 0;
            return (
              <Col xs={24} md={12} lg={8} key={g.classId}>
                <Card
                  variant="borderless"
                  style={{ borderTop: '4px solid #2F54EB', height: '100%' }}
                  title={
                    <Space>
                      <Text strong>{g.className}</Text>
                      {g.isClassTeacher && (
                        <Tag color="gold">Class Teacher</Tag>
                      )}
                    </Space>
                  }
                  extra={<Tag color="blue">{gradeName}</Tag>}
                >
                  <div style={{ marginBottom: 12 }}>
                    <Text type="secondary" style={{ fontSize: 12 }}>
                      Subjects I teach
                    </Text>
                    <div style={{ marginTop: 4 }}>
                      {g.subjects.length === 0 ? (
                        <Text type="secondary">—</Text>
                      ) : (
                        g.subjects.map((s) => (
                          <Tag
                            key={s.subjectId}
                            color="cyan"
                            style={{ marginBottom: 4 }}
                          >
                            <BookOutlined /> {s.subjectName}
                          </Tag>
                        ))
                      )}
                    </div>
                  </div>

                  <div style={{ marginBottom: 12 }}>
                    <Space size="small">
                      <TeamOutlined style={{ color: '#52C41A' }} />
                      <Text strong>{studentCount}</Text>
                      <Text type="secondary">students</Text>
                    </Space>
                  </div>

                  <Space wrap>
                    <Button
                      size="small"
                      icon={<EyeOutlined />}
                      aria-label={`View roster for ${g.className}`}
                      onClick={() =>
                        router.push(`/teacher/classes/${g.classId}`)
                      }
                    >
                      View Roster
                    </Button>
                    <Button
                      size="small"
                      icon={<CheckSquareOutlined />}
                      aria-label={`Take attendance for ${g.className}`}
                      onClick={() =>
                        router.push(
                          `/teacher/attendance?classId=${g.classId}`
                        )
                      }
                    >
                      Attendance
                    </Button>
                    <Button
                      size="small"
                      icon={<FileTextOutlined />}
                      aria-label={`Open mark sheets for ${g.className}`}
                      onClick={() =>
                        router.push(
                          `/teacher/mark-sheets?classId=${g.classId}`
                        )
                      }
                    >
                      Mark Sheets
                    </Button>
                  </Space>
                </Card>
              </Col>
            );
          })}
        </Row>
      )}
    </div>
  );
}

export default function TeacherMyClassesPageContent() {
  return (
    <TeacherProvider>
      <ClassSubjectProvider>
        <ClassProvider>
          <TeacherMyClassesContent />
        </ClassProvider>
      </ClassSubjectProvider>
    </TeacherProvider>
  );
}
