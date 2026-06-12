'use client';

import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  Card,
  Col,
  Empty,
  Input,
  Row,
  Select,
  Space,
  Statistic,
  Table,
  Tag,
  Tooltip,
  Typography,
} from 'antd';
import {
  FileDoneOutlined,
  FormOutlined,
  PlusOutlined,
  ReloadOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
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
  AcademicYearProvider,
  useAcademicYearActions,
  useAcademicYearState,
} from '@/providers/academic/academic_years';
import {
  TermProvider,
  useTermActions,
  useTermState,
} from '@/providers/academic/terms';
import {
  AssessmentProvider,
  useAssessmentActions,
  useAssessmentState,
} from '@/providers/assessment/assessments';
import type { IClassSubjectList } from '@/providers/academic/shared/interfaces';
import type { IAssessmentList } from '@/providers/assessment/shared/interfaces';
import { AssessmentFormModal } from '@/components/modals/assessment/AssessmentFormModal';

const { Title, Text } = Typography;

// Mirror backend psms.Domain.Shared.Enums.AssessmentType.
const TYPE_LABEL: Record<number, string> = {
  1: 'Placement',
  2: 'Diagnostic',
  3: 'Readiness',
  4: 'Language Proficiency',
  5: 'Mathematics',
  6: 'General',
};

function TeacherAssessmentsContent() {
  const { currentUser } = useAuthState();
  const [createOpen, setCreateOpen] = useState(false);

  const [searchKeyword, setSearchKeyword] = useState('');
  const [debouncedKeyword, setDebouncedKeyword] = useState('');
  const [filterClassSubjectId, setFilterClassSubjectId] = useState<string | undefined>(undefined);
  const [filterType, setFilterType] = useState<number | undefined>(undefined);

  useEffect(() => {
    const handle = setTimeout(() => setDebouncedKeyword(searchKeyword), 300);
    return () => clearTimeout(handle);
  }, [searchKeyword]);

  const { getByCurrentUserAsync } = useTeacherActions();
  const { teacher, isPending: teacherPending, isError: teacherError } = useTeacherState();

  const { getByTeacherAsync: getMyClassSubjects } = useClassSubjectActions();
  const {
    classSubjects,
    isPending: classSubjectsPending,
    isError: classSubjectsError,
  } = useClassSubjectState();

  const { getAllAsync: getAllAcademicYears } = useAcademicYearActions();
  const { academicYears } = useAcademicYearState();

  const { getByAcademicYearAsync: getTermsByYear } = useTermActions();
  const { terms } = useTermState();

  const { getAllAsync: getAllAssessments } = useAssessmentActions();
  const {
    assessments,
    isPending: assessmentsPending,
    isError: assessmentsError,
  } = useAssessmentState();

  useEffect(() => {
    if (currentUser?.id != null) {
      getByCurrentUserAsync();
      getAllAcademicYears({ maxResultCount: 50 });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentUser?.id]);

  // Terms are scoped to an academic year (there is no flat Term/GetAll on
  // the backend). Default to the current year, falling back to the first,
  // and load that year's terms for the create-assessment dropdown.
  const defaultAcademicYearId = useMemo(() => {
    const years = academicYears ?? [];
    return years.find((y) => y.isCurrent)?.id ?? years[0]?.id;
  }, [academicYears]);

  useEffect(() => {
    if (defaultAcademicYearId) {
      getTermsByYear(defaultAcademicYearId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [defaultAcademicYearId]);

  const teacherId = teacher && teacher.userId === currentUser?.id ? teacher.id : null;

  useEffect(() => {
    if (teacherId) {
      getMyClassSubjects(teacherId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [teacherId]);

  const refreshAssessments = useCallback(() => {
    if ((classSubjects?.length ?? 0) === 0) return;
    getAllAssessments({
      maxResultCount: 500,
      classSubjectId: filterClassSubjectId,
      assessmentType: filterType,
      name: debouncedKeyword.trim() || undefined,
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [classSubjects?.length, filterClassSubjectId, filterType, debouncedKeyword]);

  useEffect(() => {
    refreshAssessments();
  }, [refreshAssessments]);

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

  // Client-side scope: the shared Assessment/GetAll is not teacher-scoped,
  // so only surface assessments for class-subjects this teacher teaches.
  const myAssessments = useMemo(
    () => (assessments ?? []).filter((a) => myClassSubjectIds.has(a.classSubjectId)),
    [assessments, myClassSubjectIds]
  );

  const publishedCount = useMemo(
    () => myAssessments.filter((a) => a.isPublished).length,
    [myAssessments]
  );

  const noTeacherProfile =
    !teacherPending && !teacherError && currentUser != null && teacher === undefined;
  const loading = teacherPending || classSubjectsPending || assessmentsPending;
  const anyError = teacherError || classSubjectsError || assessmentsError;
  const hasClassSubjects = (classSubjects?.length ?? 0) > 0;

  const classSubjectOptions = useMemo(
    () =>
      (classSubjects ?? []).map((cs) => ({
        value: cs.id,
        label: `${cs.className ?? 'Class'} — ${cs.subjectName ?? 'Subject'}`,
      })),
    [classSubjects]
  );

  const columns: ColumnsType<IAssessmentList> = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      render: (v: string) => <Text strong>{v}</Text>,
    },
    {
      title: 'Class — Subject',
      key: 'classSubject',
      render: (_: unknown, row: IAssessmentList) => {
        const cs = classSubjectsById.get(row.classSubjectId);
        if (!cs) return <Text type="secondary">—</Text>;
        return (
          <span>
            {cs.className ?? 'Class'} <Text type="secondary">·</Text>{' '}
            {cs.subjectName ?? 'Subject'}
          </span>
        );
      },
    },
    {
      title: 'Term',
      dataIndex: 'termName',
      key: 'termName',
      render: (v?: string) => v ?? '—',
    },
    {
      title: 'Type',
      dataIndex: 'assessmentType',
      key: 'assessmentType',
      width: 150,
      render: (n: number) => <Tag>{TYPE_LABEL[n] ?? 'Other'}</Tag>,
    },
    {
      title: 'Max',
      dataIndex: 'maxMarks',
      key: 'maxMarks',
      width: 80,
    },
    {
      title: 'Questions',
      dataIndex: 'questionCount',
      key: 'questionCount',
      width: 100,
      render: (n: number) => n ?? 0,
    },
    {
      title: 'Status',
      key: 'status',
      width: 160,
      render: (_: unknown, row: IAssessmentList) => (
        <Space size={4}>
          <Tag color={row.isPublished ? 'green' : 'default'}>
            {row.isPublished ? 'Published' : 'Draft'}
          </Tag>
          {row.marksReleased && <Tag color="blue">Marks released</Tag>}
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
            Assessments
          </Title>
          <Text type="secondary">
            Create tests and quizzes for your class-subjects. Each assessment
            needs 5-100 multiple-choice questions (QA-001).
          </Text>
        </div>
        <Space>
          <Tooltip title="Refresh">
            <Button
              icon={<ReloadOutlined />}
              onClick={refreshAssessments}
              aria-label="Refresh assessments"
            />
          </Tooltip>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            disabled={!hasClassSubjects || loading}
            onClick={() => setCreateOpen(true)}
          >
            Create assessment
          </Button>
        </Space>
      </Space>

      {noTeacherProfile && (
        <Alert
          type="warning"
          showIcon
          message="No teacher profile linked to your account"
          description="A school administrator needs to link your user to a teacher record before assessments appear."
          style={{ marginBottom: 16 }}
        />
      )}

      {anyError && !loading && (
        <Alert
          type="warning"
          showIcon
          message="Some assessment data could not be loaded. Refresh to retry."
          style={{ marginBottom: 16 }}
        />
      )}

      {!loading && !noTeacherProfile && !hasClassSubjects && (
        <Alert
          type="info"
          showIcon
          message="No class-subjects assigned"
          description="An administrator needs to assign you to classes and subjects before you can create assessments."
          style={{ marginBottom: 16 }}
        />
      )}

      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col xs={24} md={8}>
          <Card variant="borderless" style={{ borderTop: '4px solid #1890FF' }}>
            <Statistic
              title="My assessments"
              value={loading ? '—' : myAssessments.length}
              prefix={<FormOutlined />}
              valueStyle={{ color: '#1890FF' }}
            />
          </Card>
        </Col>
        <Col xs={24} md={8}>
          <Card variant="borderless" style={{ borderTop: '4px solid #52C41A' }}>
            <Statistic
              title="Published"
              value={loading ? '—' : publishedCount}
              prefix={<FileDoneOutlined />}
              valueStyle={{ color: '#52C41A' }}
            />
          </Card>
        </Col>
      </Row>

      <Card variant="borderless" size="small" style={{ marginBottom: 16 }}>
        <Row gutter={[12, 12]} align="middle">
          <Col xs={24} md={8}>
            <Input.Search
              placeholder="Search by name…"
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
              placeholder="All types"
              allowClear
              value={filterType}
              onChange={setFilterType}
              options={Object.entries(TYPE_LABEL).map(([k, v]) => ({
                value: Number(k),
                label: v,
              }))}
              style={{ width: '100%' }}
            />
          </Col>
        </Row>
      </Card>

      <Card variant="borderless" styles={{ body: { padding: 0 } }}>
        <Table<IAssessmentList>
          dataSource={myAssessments}
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
                description="No assessments match the current filters."
              />
            ) : (
              <Empty description="No assessments" />
            ),
          }}
        />
      </Card>

      <AssessmentFormModal
        open={createOpen}
        classSubjects={classSubjects ?? []}
        terms={terms ?? []}
        onClose={(refresh) => {
          setCreateOpen(false);
          if (refresh) refreshAssessments();
        }}
      />
    </div>
  );
}

export default function TeacherAssessmentsPageContent() {
  return (
    <TeacherProvider>
      <ClassSubjectProvider>
        <AcademicYearProvider>
          <TermProvider>
            <AssessmentProvider>
              <TeacherAssessmentsContent />
            </AssessmentProvider>
          </TermProvider>
        </AcademicYearProvider>
      </ClassSubjectProvider>
    </TeacherProvider>
  );
}
