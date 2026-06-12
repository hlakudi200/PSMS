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
  Table,
  Tag,
  Tooltip,
  Typography,
} from 'antd';
import { EditOutlined, ReloadOutlined } from '@ant-design/icons';
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
  AssessmentProvider,
  useAssessmentActions,
  useAssessmentState,
} from '@/providers/assessment/assessments';
import type { IClassSubjectList } from '@/providers/academic/shared/interfaces';
import type { IAssessmentList } from '@/providers/assessment/shared/interfaces';

const { Title, Text } = Typography;

function TeacherMarkSheetsContent() {
  const router = useRouter();
  const { currentUser } = useAuthState();

  const [searchKeyword, setSearchKeyword] = useState('');
  const [debouncedKeyword, setDebouncedKeyword] = useState('');
  const [filterClassSubjectId, setFilterClassSubjectId] = useState<string | undefined>(undefined);

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

  const { getAllAsync: getAllAssessments } = useAssessmentActions();
  const {
    assessments,
    isPending: assessmentsPending,
    isError: assessmentsError,
  } = useAssessmentState();

  useEffect(() => {
    if (currentUser?.id != null) getByCurrentUserAsync();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentUser?.id]);

  const teacherId = teacher && teacher.userId === currentUser?.id ? teacher.id : null;

  useEffect(() => {
    if (teacherId) getMyClassSubjects(teacherId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [teacherId]);

  const refresh = useCallback(() => {
    if ((classSubjects?.length ?? 0) === 0) return;
    getAllAssessments({
      maxResultCount: 500,
      classSubjectId: filterClassSubjectId,
      name: debouncedKeyword.trim() || undefined,
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [classSubjects?.length, filterClassSubjectId, debouncedKeyword]);

  useEffect(() => {
    refresh();
  }, [refresh]);

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

  const myAssessments = useMemo(
    () => (assessments ?? []).filter((a) => myClassSubjectIds.has(a.classSubjectId)),
    [assessments, myClassSubjectIds]
  );

  const loading = teacherPending || classSubjectsPending || assessmentsPending;
  const anyError = teacherError || classSubjectsError || assessmentsError;
  const hasClassSubjects = (classSubjects?.length ?? 0) > 0;

  const noTeacherProfile =
    !teacherPending && !teacherError && currentUser != null && teacher === undefined;

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
      title: 'Assessment',
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
        return `${cs.className ?? 'Class'} · ${cs.subjectName ?? 'Subject'}`;
      },
    },
    { title: 'Term', dataIndex: 'termName', key: 'termName', render: (v?: string) => v ?? '—' },
    { title: 'Max', dataIndex: 'maxMarks', key: 'maxMarks', width: 80 },
    {
      title: 'Marks captured',
      dataIndex: 'markCount',
      key: 'markCount',
      width: 130,
      render: (n: number) => n ?? 0,
    },
    {
      title: 'Status',
      key: 'status',
      width: 150,
      render: (_: unknown, row: IAssessmentList) => (
        <Space size={4}>
          <Tag color={row.isPublished ? 'green' : 'default'}>
            {row.isPublished ? 'Published' : 'Draft'}
          </Tag>
          {row.marksReleased && <Tag color="blue">Released</Tag>}
        </Space>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 160,
      render: (_: unknown, row: IAssessmentList) => (
        <Tooltip title={row.marksReleased ? 'Marks released — view only' : 'Capture / edit marks'}>
          <Button
            size="small"
            type="primary"
            icon={<EditOutlined />}
            onClick={() => router.push(`/teacher/mark-sheets/${row.id}`)}
            aria-label={`Capture marks for ${row.name}`}
          >
            {row.marksReleased ? 'View marks' : 'Capture marks'}
          </Button>
        </Tooltip>
      ),
    },
  ];

  return (
    <div>
      <Space style={{ width: '100%', justifyContent: 'space-between', marginBottom: 16 }} align="start">
        <div>
          <Title level={4} style={{ margin: 0 }}>
            Mark Sheets
          </Title>
          <Text type="secondary">
            Capture and review marks for your assessments. Marks auto-derive a
            percentage and CAPS achievement level (BR-GR-001..006).
          </Text>
        </div>
        <Tooltip title="Refresh">
          <Button icon={<ReloadOutlined />} onClick={refresh} aria-label="Refresh" />
        </Tooltip>
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

      {anyError && !loading && (
        <Alert
          type="warning"
          showIcon
          message="Some data could not be loaded. Refresh to retry."
          style={{ marginBottom: 16 }}
        />
      )}

      {!loading && !noTeacherProfile && !hasClassSubjects && (
        <Alert
          type="info"
          showIcon
          message="No class-subjects assigned"
          description="An administrator needs to assign you to classes and subjects."
          style={{ marginBottom: 16 }}
        />
      )}

      <Card variant="borderless" size="small" style={{ marginBottom: 16 }}>
        <Row gutter={[12, 12]} align="middle">
          <Col xs={24} md={12}>
            <Input.Search
              placeholder="Search by assessment name…"
              allowClear
              value={searchKeyword}
              onChange={(e) => setSearchKeyword(e.target.value)}
              onSearch={(v) => setSearchKeyword(v)}
            />
          </Col>
          <Col xs={24} md={12}>
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
          scroll={{ x: 900 }}
          locale={{
            emptyText: hasClassSubjects ? (
              <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="No assessments match the filters." />
            ) : (
              <Empty description="No assessments" />
            ),
          }}
        />
      </Card>
    </div>
  );
}

export default function TeacherMarkSheetsPageContent() {
  return (
    <TeacherProvider>
      <ClassSubjectProvider>
        <AssessmentProvider>
          <TeacherMarkSheetsContent />
        </AssessmentProvider>
      </ClassSubjectProvider>
    </TeacherProvider>
  );
}
