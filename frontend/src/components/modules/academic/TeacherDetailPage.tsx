'use client';

import React, { useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { usePortalBase } from '@/utils/portal-base';
import {
  Card,
  Tag,
  Tabs,
  Table,
  Space,
  Button,
  Typography,
  Spin,
  Empty,
  Descriptions,
  Row,
  Col,
  Statistic,
  Avatar,
} from 'antd';
import {
  ArrowLeftOutlined,
  BookOutlined,
  TeamOutlined,
  UserOutlined,
  MailOutlined,
  PhoneOutlined,
  IdcardOutlined,
  CalendarOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { TeacherProvider, useTeacherState, useTeacherActions } from '@/providers/academic/teachers';
import { TeacherClassProvider, useTeacherClassState, useTeacherClassActions } from '@/providers/academic/teacher_classes';
import { TeacherSubjectProvider, useTeacherSubjectState, useTeacherSubjectActions } from '@/providers/academic/teacher_subjects';
import type { ITeacherClass, ITeacherSubject } from '@/providers/academic/shared/interfaces';

const { Title, Text } = Typography;

// ─── Classes Tab ──────────────────────────────────────────────
function ClassesSection({ teacherId }: { teacherId: string }) {
  const router = useRouter();
  const portalBase = usePortalBase();
  const { teacherClasses, isPending } = useTeacherClassState();
  const { getByTeacherAsync } = useTeacherClassActions();

  useEffect(() => {
    getByTeacherAsync(teacherId);
  }, [teacherId, getByTeacherAsync]);

  const columns = [
    { title: 'Class', dataIndex: 'className', key: 'className' },
    { title: 'Subject', dataIndex: 'subjectName', key: 'subjectName' },
    { title: 'Subject Code', dataIndex: 'subjectCode', key: 'subjectCode', width: 120 },
    {
      title: 'Role',
      key: 'isClassTeacher',
      render: (_: unknown, record: ITeacherClass) => (
        <Tag color={record.isClassTeacher ? 'blue' : 'default'}>
          {record.isClassTeacher ? 'Class Teacher' : 'Subject Teacher'}
        </Tag>
      ),
    },
    {
      title: '',
      key: 'action',
      width: 100,
      render: (_: unknown, record: ITeacherClass) => (
        <Button
          type="link"
          size="small"
          onClick={() => router.push(`${portalBase}/classes/${record.classId}`)}
        >
          View Class
        </Button>
      ),
    },
  ];

  return (
    <Table<ITeacherClass>
      dataSource={teacherClasses ?? []}
      columns={columns}
      rowKey="id"
      loading={isPending}
      pagination={false}
      size="small"
      locale={{ emptyText: <Empty description="No classes assigned to this teacher" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
    />
  );
}

// ─── Subjects Tab ─────────────────────────────────────────────
function SubjectsSection({ teacherId }: { teacherId: string }) {
  const { teacherSubjects, isPending } = useTeacherSubjectState();
  const { getByTeacherAsync } = useTeacherSubjectActions();

  useEffect(() => {
    getByTeacherAsync(teacherId);
  }, [teacherId, getByTeacherAsync]);

  const columns = [
    { title: 'Subject', dataIndex: 'subjectName', key: 'subjectName' },
    { title: 'Code', dataIndex: 'subjectCode', key: 'subjectCode', width: 120 },
    { title: 'Grade', dataIndex: 'gradeName', key: 'gradeName' },
    {
      title: 'Primary',
      dataIndex: 'isPrimary',
      key: 'isPrimary',
      width: 100,
      render: (isPrimary: boolean) => (
        <Tag color={isPrimary ? 'green' : 'default'}>{isPrimary ? 'Primary' : 'Secondary'}</Tag>
      ),
    },
  ];

  return (
    <Table<ITeacherSubject>
      dataSource={teacherSubjects ?? []}
      columns={columns}
      rowKey="id"
      loading={isPending}
      pagination={false}
      size="small"
      locale={{ emptyText: <Empty description="No subjects assigned to this teacher" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
    />
  );
}

// ─── Main Content ─────────────────────────────────────────────
function TeacherDetailContent() {
  const params = useParams();
  const router = useRouter();
  const portalBase = usePortalBase();
  const teacherId = params.id as string;

  const teacherState = useTeacherState();
  const { getAsync } = useTeacherActions();
  const teacher = teacherState.teacher;

  useEffect(() => {
    if (teacherId) getAsync(teacherId);
  }, [teacherId, getAsync]);

  if (teacherState.isPending && !teacher) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 80 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (teacherState.isError || (!teacherState.isPending && !teacher)) {
    return (
      <div style={{ padding: 24 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => router.push(`${portalBase}/teachers`)} style={{ marginBottom: 16 }}>
          Back to Teachers
        </Button>
        <Empty description="Teacher not found" />
      </div>
    );
  }

  if (!teacher) return null;

  const tabItems = [
    {
      key: 'classes',
      label: <span><TeamOutlined /> Classes ({teacher.classAssignmentCount})</span>,
      children: <ClassesSection teacherId={teacherId} />,
    },
    {
      key: 'subjects',
      label: <span><BookOutlined /> Subjects ({teacher.subjectAssignmentCount})</span>,
      children: <SubjectsSection teacherId={teacherId} />,
    },
    {
      key: 'details',
      label: <span><UserOutlined /> Personal Details</span>,
      children: (
        <Descriptions column={{ xs: 1, sm: 2 }} size="small" bordered>
          <Descriptions.Item label="First Name">{teacher.firstName}</Descriptions.Item>
          <Descriptions.Item label="Last Name">{teacher.lastName}</Descriptions.Item>
          {teacher.middleName && (
            <Descriptions.Item label="Middle Name">{teacher.middleName}</Descriptions.Item>
          )}
          <Descriptions.Item label="Employee Number">{teacher.employeeNumber}</Descriptions.Item>
          <Descriptions.Item label="Email">{teacher.email || '—'}</Descriptions.Item>
          <Descriptions.Item label="Phone">{teacher.phone || '—'}</Descriptions.Item>
          <Descriptions.Item label="Date of Joining">
            {teacher.dateOfJoining ? dayjs(teacher.dateOfJoining).format('DD MMM YYYY') : '—'}
          </Descriptions.Item>
          <Descriptions.Item label="Employment Status">
            {teacher.employmentStatus || '—'}
          </Descriptions.Item>
          <Descriptions.Item label="Qualifications" span={2}>
            {teacher.qualifications || '—'}
          </Descriptions.Item>
          <Descriptions.Item label="Qualified Subjects" span={2}>
            {teacher.qualifiedSubjects || '—'}
          </Descriptions.Item>
          {teacher.address && (
            <>
              <Descriptions.Item label="Street">{teacher.address.streetAddress || '—'}</Descriptions.Item>
              <Descriptions.Item label="Suburb">{teacher.address.suburb || '—'}</Descriptions.Item>
              <Descriptions.Item label="City">{teacher.address.city || '—'}</Descriptions.Item>
              <Descriptions.Item label="Province">{teacher.address.province || '—'}</Descriptions.Item>
              <Descriptions.Item label="Postal Code">{teacher.address.postalCode || '—'}</Descriptions.Item>
            </>
          )}
        </Descriptions>
      ),
    },
  ];

  return (
    <div style={{ padding: 24 }}>
      <Button
        icon={<ArrowLeftOutlined />}
        onClick={() => router.push(`${portalBase}/teachers`)}
        style={{ marginBottom: 16 }}
      >
        Back to Teachers
      </Button>

      <Card style={{ marginBottom: 24 }}>
        <Row gutter={24} align="middle">
          <Col flex="none">
            <Avatar
              size={64}
              icon={<UserOutlined />}
              src={teacher.profilePhotoUrl || undefined}
              style={{ backgroundColor: '#1677ff' }}
            />
          </Col>
          <Col flex="auto">
            <Space align="center" size="middle">
              <Title level={3} style={{ margin: 0 }}>{teacher.fullName}</Title>
              <Tag color={teacher.isActive ? 'green' : 'default'}>
                {teacher.isActive ? 'Active' : 'Inactive'}
              </Tag>
            </Space>
            <div style={{ marginTop: 8 }}>
              <Space size="large" wrap>
                <Text type="secondary"><IdcardOutlined /> {teacher.employeeNumber}</Text>
                {teacher.email && <Text type="secondary"><MailOutlined /> {teacher.email}</Text>}
                {teacher.phone && <Text type="secondary"><PhoneOutlined /> {teacher.phone}</Text>}
                {teacher.dateOfJoining && (
                  <Text type="secondary">
                    <CalendarOutlined /> Joined {dayjs(teacher.dateOfJoining).format('DD MMM YYYY')}
                  </Text>
                )}
              </Space>
            </div>
          </Col>
          <Col>
            <Row gutter={16}>
              <Col>
                <Card size="small" style={{ textAlign: 'center', minWidth: 100 }}>
                  <Statistic title="Subjects" value={teacher.subjectAssignmentCount} />
                </Card>
              </Col>
              <Col>
                <Card size="small" style={{ textAlign: 'center', minWidth: 100 }}>
                  <Statistic title="Classes" value={teacher.classAssignmentCount} />
                </Card>
              </Col>
            </Row>
          </Col>
        </Row>
      </Card>

      <Card>
        <Tabs items={tabItems} defaultActiveKey="classes" />
      </Card>
    </div>
  );
}

// ─── Wrapped with providers ───────────────────────────────────
export default function TeacherDetailPage() {
  return (
    <TeacherProvider>
      <TeacherClassProvider>
        <TeacherSubjectProvider>
          <TeacherDetailContent />
        </TeacherSubjectProvider>
      </TeacherClassProvider>
    </TeacherProvider>
  );
}
