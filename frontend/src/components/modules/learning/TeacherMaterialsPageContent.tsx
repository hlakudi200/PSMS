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
  Progress,
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
  CloudOutlined,
  DatabaseOutlined,
  EditOutlined,
  EyeOutlined,
  HistoryOutlined,
  InboxOutlined,
  LinkOutlined,
  ReloadOutlined,
  SendOutlined,
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
import { GradeProvider } from '@/providers/academic/grades';
import { AcademicYearProvider } from '@/providers/academic/academic_years';
import {
  LearningMaterialProvider,
  useLearningMaterialActions,
  useLearningMaterialState,
} from '@/providers/learning/learning_materials';
import type { IClassSubjectList } from '@/providers/academic/shared/interfaces';
import type { ILearningMaterialList } from '@/providers/learning/shared/interfaces';
import { MaterialUploadModal } from '@/components/modals/learning/MaterialUploadModal';
import { MaterialEditModal } from '@/components/modals/learning/MaterialEditModal';
import { MaterialVersionHistoryDrawer } from '@/components/modules/learning/MaterialVersionHistoryDrawer';

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

const materialTypeOptions = Object.entries(materialTypeLabels).map(
  ([value, { label }]) => ({ value: Number(value), label })
);

// LM-007 — default teacher storage quota. The backend does not yet expose
// a quota endpoint, so the indicator computes a *visible* approximation
// from the materials this teacher owns. Once the quota endpoint lands,
// replace this constant with the value returned from the server.
const TEACHER_STORAGE_QUOTA_BYTES = 5 * 1024 * 1024 * 1024; // 5 GB
const QUOTA_WARNING_THRESHOLD = 0.8;

function formatBytes(value: number): string {
  if (!value) return '0 B';
  const units = ['B', 'KB', 'MB', 'GB', 'TB'];
  let v = value;
  let i = 0;
  while (v >= 1024 && i < units.length - 1) {
    v /= 1024;
    i++;
  }
  return `${v.toFixed(v < 10 ? 1 : 0)} ${units[i]}`;
}

function TeacherMaterialsContent() {
  const { currentUser } = useAuthState();
  const [uploadOpen, setUploadOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<ILearningMaterialList | null>(null);
  const [versionsOpen, setVersionsOpen] = useState(false);
  const [versionsRecord, setVersionsRecord] = useState<ILearningMaterialList | null>(null);

  // Server-side filter state.
  const [filterClassSubjectId, setFilterClassSubjectId] = useState<string | undefined>(undefined);
  const [filterMaterialType, setFilterMaterialType] = useState<number | undefined>(undefined);
  const [filterIsPublished, setFilterIsPublished] = useState<boolean | undefined>(undefined);
  // `searchKeyword` reflects what's currently typed; `debouncedKeyword` is
  // what we actually send to the server. 300 ms debounce keeps the API
  // from being hammered on every keystroke (iter-1 review fix).
  const [searchKeyword, setSearchKeyword] = useState<string>('');
  const [debouncedKeyword, setDebouncedKeyword] = useState<string>('');
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
    getAllAsync: getAllMaterials,
    publishAsync,
    unpublishAsync,
  } = useLearningMaterialActions();
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

  // Pull materials filtered server-side. When no class-subject is selected
  // we still cap the page size; T-T07 versioning + paged controls will
  // refine this further. Filters re-fire the fetch whenever they change.
  const refreshMaterials = useCallback(() => {
    // Guard: without any class-subjects there's nothing valid to fetch.
    // We don't dispatch a clear here because the client-side
    // `myMaterials` filter already drops everything once
    // `myClassSubjectIds` is empty — the UI shows an empty state without
    // mutating provider state.
    if ((classSubjects?.length ?? 0) === 0) {
      return;
    }
    getAllMaterials({
      maxResultCount: 500,
      classSubjectId: filterClassSubjectId,
      materialType: filterMaterialType,
      isPublished: filterIsPublished,
      keyword: debouncedKeyword.trim() || undefined,
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    classSubjects?.length,
    filterClassSubjectId,
    filterMaterialType,
    filterIsPublished,
    debouncedKeyword,
  ]);

  useEffect(() => {
    refreshMaterials();
  }, [refreshMaterials]);

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

  // Client-side guard: only show materials whose classSubject is mine.
  // Server-side scope is enforced separately by the teacher-ownership
  // check on UploadAsync/CreateAsync (LM-006-style "view your own" is
  // still backend-wide for now).
  const myMaterials = useMemo(
    () =>
      (learningMaterials ?? []).filter((lm) =>
        myClassSubjectIds.has(lm.classSubjectId)
      ),
    [learningMaterials, myClassSubjectIds]
  );

  // Quota indicator: sum `fileSizeBytes` across my loaded materials. This
  // is an approximation — only materials in the current paged result
  // contribute, and there's no server-reported quota yet (LM-007). When a
  // dedicated /api/.../quota endpoint lands, replace this whole block
  // with the server-reported value.
  const totalBytes = useMemo(
    () => myMaterials.reduce((sum, m) => sum + (m.fileSizeBytes ?? 0), 0),
    [myMaterials]
  );
  const usedFraction = totalBytes / TEACHER_STORAGE_QUOTA_BYTES;
  const overQuotaWarning = usedFraction >= QUOTA_WARNING_THRESHOLD;

  const handleArchive = async (record: ILearningMaterialList) => {
    try {
      if (record.isPublished) {
        await unpublishAsync(record.id);
        message.success('Material archived (unpublished)');
      } else {
        await publishAsync(record.id);
        message.success('Material re-published');
      }
      refreshMaterials();
    } catch {
      // Surfaced by axios interceptor
    }
  };

  const noTeacherProfile =
    !teacherPending &&
    !teacherError &&
    currentUser != null &&
    teacher === undefined;
  const loading = teacherPending || classSubjectsPending || materialsPending;
  const anyError = teacherError || classSubjectsError || materialsError;
  const hasClassSubjects = (classSubjects?.length ?? 0) > 0;

  const classSubjectOptions = useMemo(
    () =>
      (classSubjects ?? []).map((cs) => ({
        value: cs.id,
        label: `${cs.className ?? 'Class'} — ${cs.subjectName ?? 'Subject'}`,
      })),
    [classSubjects]
  );

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
        return meta ? <Tag color={meta.color}>{meta.label}</Tag> : <Tag>Unknown</Tag>;
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
            {cs.className ?? 'Class'}{' '}
            <Text type="secondary">·</Text>{' '}
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
        v ? (
          <Tag color="green">Published</Tag>
        ) : (
          <Tag icon={<InboxOutlined />} color="default">
            Archived
          </Tag>
        ),
    },
    {
      title: 'Views',
      dataIndex: 'viewCount',
      key: 'viewCount',
      width: 80,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 240,
      render: (_: unknown, record: ILearningMaterialList) => (
        <Space size="small">
          <Tooltip title="Edit metadata">
            <Button
              size="small"
              icon={<EditOutlined />}
              aria-label={`Edit ${record.title}`}
              onClick={() => {
                setEditRecord(record);
                setEditOpen(true);
              }}
            />
          </Tooltip>
          <Tooltip title="Version history">
            <Button
              size="small"
              icon={<HistoryOutlined />}
              aria-label={`Version history for ${record.title}`}
              onClick={() => {
                setVersionsRecord(record);
                setVersionsOpen(true);
              }}
            />
          </Tooltip>
          {record.isPublished ? (
            <Popconfirm
              title="Archive this material?"
              description="It will no longer be visible to students until re-published."
              onConfirm={() => handleArchive(record)}
              okText="Archive"
            >
              <Tooltip title="Archive (unpublish)">
                <Button
                  size="small"
                  icon={<InboxOutlined />}
                  aria-label={`Archive ${record.title}`}
                />
              </Tooltip>
            </Popconfirm>
          ) : (
            <Tooltip title="Publish">
              <Button
                size="small"
                type="primary"
                icon={<SendOutlined />}
                aria-label={`Publish ${record.title}`}
                onClick={() => handleArchive(record)}
              />
            </Tooltip>
          )}
        </Space>
      ),
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
            Filter, edit, and archive materials you have uploaded across the
            classes you teach.
          </Text>
        </div>
        <Space>
          <Tooltip title="Refresh">
            <Button
              icon={<ReloadOutlined />}
              onClick={refreshMaterials}
              aria-label="Refresh materials"
            />
          </Tooltip>
          <Button
            type="primary"
            icon={<UploadOutlined />}
            disabled={!hasClassSubjects || loading}
            onClick={() => setUploadOpen(true)}
          >
            Upload Material
          </Button>
        </Space>
      </Space>

      {noTeacherProfile && (
        <Alert
          type="warning"
          showIcon
          message="No teacher profile linked to your account"
          description="You need a teacher profile linked to your user before materials will appear."
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
          description="An administrator needs to assign you to classes and subjects before you can manage materials for them."
          style={{ marginBottom: 16 }}
        />
      )}

      {/* Quota + stats — based on the materials currently loaded into the
          provider. Replace with a server-reported value once a quota
          endpoint exists (LM-007). */}
      <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
        <Col xs={24} md={12} lg={8}>
          <Card variant="borderless" style={{ borderTop: '4px solid #1890FF' }}>
            <Statistic
              title="My materials"
              value={loading ? '—' : myMaterials.length}
              prefix={<DatabaseOutlined />}
              valueStyle={{ color: '#1890FF' }}
            />
          </Card>
        </Col>
        <Col xs={24} md={12} lg={16}>
          <Card variant="borderless" style={{ borderTop: '4px solid #722ED1' }}>
            <Space style={{ width: '100%', justifyContent: 'space-between' }}>
              <Space>
                <CloudOutlined style={{ color: '#722ED1', fontSize: 20 }} />
                <div>
                  <Text type="secondary" style={{ fontSize: 12 }}>
                    Storage (approximation)
                  </Text>
                  <div>
                    <Text strong>{formatBytes(totalBytes)}</Text>{' '}
                    <Text type="secondary">
                      / {formatBytes(TEACHER_STORAGE_QUOTA_BYTES)}
                    </Text>
                  </div>
                </div>
              </Space>
              <Progress
                percent={Math.min(100, Math.round(usedFraction * 100))}
                status={overQuotaWarning ? 'exception' : 'normal'}
                style={{ width: 240 }}
                size="small"
              />
            </Space>
            {overQuotaWarning && (
              <Text type="danger" style={{ display: 'block', marginTop: 8, fontSize: 12 }}>
                Approaching the 5 GB quota cap (LM-007). Archive older
                materials or contact your administrator for an increase.
              </Text>
            )}
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
          <Col xs={24} md={6}>
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
          <Col xs={12} md={5}>
            <Select
              placeholder="Type"
              allowClear
              value={filterMaterialType}
              onChange={setFilterMaterialType}
              options={materialTypeOptions}
              style={{ width: '100%' }}
            />
          </Col>
          <Col xs={12} md={5}>
            <Select
              placeholder="All statuses"
              allowClear
              value={
                filterIsPublished === undefined
                  ? undefined
                  : filterIsPublished
                  ? 'published'
                  : 'archived'
              }
              onChange={(v) =>
                setFilterIsPublished(
                  v === undefined ? undefined : v === 'published'
                )
              }
              options={[
                { value: 'published', label: 'Published' },
                { value: 'archived', label: 'Archived' },
              ]}
              style={{ width: '100%' }}
            />
          </Col>
        </Row>
      </Card>

      <Card variant="borderless" styles={{ body: { padding: 0 } }}>
        <Table<ILearningMaterialList>
          dataSource={myMaterials}
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
                description={
                  <span>
                    <EyeOutlined style={{ marginRight: 6 }} />
                    No materials match the current filters.
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
          if (refresh) refreshMaterials();
        }}
        classSubjects={classSubjects ?? []}
      />

      <MaterialEditModal
        open={editOpen}
        onClose={(refresh) => {
          setEditOpen(false);
          setEditRecord(null);
          if (refresh) refreshMaterials();
        }}
        editRecord={editRecord}
      />

      <MaterialVersionHistoryDrawer
        open={versionsOpen}
        material={versionsRecord}
        onClose={() => {
          setVersionsOpen(false);
          setVersionsRecord(null);
          // A new version replaces the current file pointer on the
          // material, so refresh the table once the drawer closes — that
          // way the FileName/FileSizeBytes columns show the latest data.
          refreshMaterials();
        }}
      />
    </div>
  );
}

export default function TeacherMaterialsPageContent() {
  return (
    <TeacherProvider>
      <ClassSubjectProvider>
        <GradeProvider>
          <AcademicYearProvider>
            <LearningMaterialProvider>
              <TeacherMaterialsContent />
            </LearningMaterialProvider>
          </AcademicYearProvider>
        </GradeProvider>
      </ClassSubjectProvider>
    </TeacherProvider>
  );
}
