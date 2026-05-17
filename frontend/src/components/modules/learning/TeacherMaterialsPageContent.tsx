'use client';

import { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  Card,
  Empty,
  Space,
  Table,
  Tag,
  Typography,
} from 'antd';
import {
  EyeOutlined,
  LinkOutlined,
  UploadOutlined,
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
  LearningMaterialProvider,
  useLearningMaterialActions,
  useLearningMaterialState,
} from '@/providers/learning/learning_materials';
import type { IClassSubjectList } from '@/providers/academic/shared/interfaces';
import type { ILearningMaterialList } from '@/providers/learning/shared/interfaces';
import { MaterialUploadModal } from '@/components/modals/learning/MaterialUploadModal';

const { Title, Text } = Typography;

// Keep in sync with backend psms.Domain.Shared.Enums.LearningMaterialType.
const materialTypeLabels: Record<number, { label: string; color: string }> = {
  1: { label: 'Document', color: 'blue' },
  2: { label: 'Video', color: 'purple' },
  3: { label: 'Audio', color: 'cyan' },
  4: { label: 'Presentation', color: 'gold' },
  5: { label: 'Worksheet', color: 'orange' },
  6: { label: 'External Link', color: 'magenta' },
  7: { label: 'Image', color: 'green' },
  8: { label: 'Interactive', color: 'geekblue' },
};

function TeacherMaterialsContent() {
  const { currentUser } = useAuthState();
  const [uploadOpen, setUploadOpen] = useState(false);

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

  const { getAllAsync: getAllMaterials } = useLearningMaterialActions();
  const {
    learningMaterials,
    isPending: materialsPending,
    isError: materialsError,
  } = useLearningMaterialState();

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

  // After class subjects load, fetch every material for the tenant and
  // filter client-side to mine. The backend GetAll endpoint doesn't accept
  // a teacher filter, and per-class-subject fetches would be N+1. 500
  // materials per tenant is plenty for the upload-and-list flow this
  // ticket targets — T-T06 (Library) adds server-side filtering.
  useEffect(() => {
    if ((classSubjects?.length ?? 0) > 0) {
      getAllMaterials({ maxResultCount: 500 });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [classSubjects?.length]);

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

  const myMaterials = useMemo(
    () =>
      (learningMaterials ?? []).filter((lm) =>
        myClassSubjectIds.has(lm.classSubjectId)
      ),
    [learningMaterials, myClassSubjectIds]
  );

  const noTeacherProfile =
    !teacherPending &&
    !teacherError &&
    currentUser != null &&
    teacher === undefined;
  const loading = teacherPending || classSubjectsPending || materialsPending;
  const anyError = teacherError || classSubjectsError || materialsError;
  const hasClassSubjects = (classSubjects?.length ?? 0) > 0;

  const columns: ColumnsType<ILearningMaterialList> = [
    {
      title: 'Title',
      dataIndex: 'title',
      key: 'title',
      render: (v: string) => <Text strong>{v}</Text>,
    },
    {
      title: 'Type',
      dataIndex: 'materialType',
      key: 'materialType',
      width: 130,
      render: (n: number) => {
        const meta = materialTypeLabels[n];
        if (!meta) return <Tag>Unknown</Tag>;
        return <Tag color={meta.color}>{meta.label}</Tag>;
      },
    },
    {
      title: 'Class — Subject',
      key: 'classSubject',
      render: (_: unknown, row: ILearningMaterialList) => {
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
      title: 'File',
      dataIndex: 'fileName',
      key: 'fileName',
      width: 220,
      render: (v?: string) =>
        v ? (
          <Text type="secondary" style={{ fontSize: 12 }}>
            {v}
          </Text>
        ) : (
          <Tag icon={<LinkOutlined />} color="magenta">
            External
          </Tag>
        ),
    },
    {
      title: 'Status',
      dataIndex: 'isPublished',
      key: 'isPublished',
      width: 110,
      render: (v: boolean) =>
        v ? <Tag color="green">Published</Tag> : <Tag>Draft</Tag>,
    },
    {
      title: 'Views',
      dataIndex: 'viewCount',
      key: 'viewCount',
      width: 80,
    },
  ];

  return (
    <div>
      <Space
        style={{
          width: '100%',
          justifyContent: 'space-between',
          marginBottom: 16,
        }}
        align="start"
      >
        <div>
          <Title level={4} style={{ margin: 0 }}>
            Learning Materials
          </Title>
          <Text type="secondary">
            Upload notes, worksheets, videos, or external links for the
            classes you teach. Detailed library filters / versioning live in
            later tickets (T-T06, T-T07).
          </Text>
        </div>
        <Button
          type="primary"
          icon={<UploadOutlined />}
          disabled={!hasClassSubjects || loading}
          onClick={() => setUploadOpen(true)}
        >
          Upload Material
        </Button>
      </Space>

      {noTeacherProfile && (
        <Alert
          type="warning"
          showIcon
          message="No teacher profile linked to your account"
          description="You need a teacher profile linked to your user before you can upload materials."
          style={{ marginBottom: 16 }}
        />
      )}

      {anyError && !loading && (
        <Alert
          type="warning"
          showIcon
          message="Some material data could not be loaded. Refresh to retry."
          style={{ marginBottom: 16 }}
        />
      )}

      {!loading && !noTeacherProfile && !hasClassSubjects && (
        <Alert
          type="info"
          showIcon
          message="No class-subjects assigned"
          description="An administrator needs to assign you to classes and subjects before you can upload materials for them."
          style={{ marginBottom: 16 }}
        />
      )}

      <Card variant="borderless" styles={{ body: { padding: 0 } }}>
        <Table<ILearningMaterialList>
          dataSource={myMaterials}
          rowKey="id"
          loading={loading}
          pagination={{ pageSize: 20 }}
          size="small"
          columns={columns}
          locale={{
            emptyText: hasClassSubjects ? (
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description={
                  <span>
                    <EyeOutlined style={{ marginRight: 6 }} />
                    No materials yet — click <strong>Upload Material</strong>{' '}
                    above to add one.
                  </span>
                }
              />
            ) : (
              <Empty description="No materials" />
            ),
          }}
        />
      </Card>

      <MaterialUploadModal
        open={uploadOpen}
        onClose={(refresh) => {
          setUploadOpen(false);
          if (refresh) getAllMaterials({ maxResultCount: 500 });
        }}
        classSubjects={classSubjects ?? []}
      />
    </div>
  );
}

export default function TeacherMaterialsPageContent() {
  return (
    <TeacherProvider>
      <ClassSubjectProvider>
        <LearningMaterialProvider>
          <TeacherMaterialsContent />
        </LearningMaterialProvider>
      </ClassSubjectProvider>
    </TeacherProvider>
  );
}
