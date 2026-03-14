'use client';

import React, { useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
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
  TeamOutlined,
  UserOutlined,
  MailOutlined,
  PhoneOutlined,
  IdcardOutlined,
  BankOutlined,
} from '@ant-design/icons';
import { ParentProvider, useParentState, useParentActions } from '@/providers/academic/parents';
import { StudentParentProvider, useStudentParentState, useStudentParentActions } from '@/providers/academic/student_parents';
import type { IStudentParent } from '@/providers/academic/shared/interfaces';

const { Title, Text } = Typography;

const relationshipTypeMap: Record<number, string> = {
  0: 'Father',
  1: 'Mother',
  2: 'Guardian',
  3: 'Grandparent',
  4: 'Sibling',
  5: 'Other',
};

// ─── Children Tab ─────────────────────────────────────────────
function ChildrenSection({ parentId }: { parentId: string }) {
  const router = useRouter();
  const { studentParents, isPending } = useStudentParentState();
  const { getByParentAsync } = useStudentParentActions();

  useEffect(() => {
    getByParentAsync(parentId);
  }, [parentId, getByParentAsync]);

  const columns = [
    { title: 'Student Name', dataIndex: 'studentName', key: 'studentName' },
    { title: 'Admission #', dataIndex: 'studentAdmissionNumber', key: 'studentAdmissionNumber', width: 120 },
    {
      title: 'Relationship',
      dataIndex: 'relationshipType',
      key: 'relationshipType',
      width: 120,
      render: (type: number) => relationshipTypeMap[type] ?? 'Unknown',
    },
    {
      title: 'Roles',
      key: 'roles',
      render: (_: unknown, record: IStudentParent) => (
        <Space size={4} wrap>
          {record.isPrimaryContact && <Tag color="blue">Primary Contact</Tag>}
          {record.isFinanciallyResponsible && <Tag color="gold">Financial</Tag>}
          {record.canPickupStudent && <Tag color="green">Pickup</Tag>}
          {record.livesWithStudent && <Tag color="purple">Lives With</Tag>}
        </Space>
      ),
    },
    {
      title: '',
      key: 'action',
      width: 120,
      render: (_: unknown, record: IStudentParent) => (
        <Button
          type="link"
          size="small"
          onClick={() => router.push(`/principal/students/${record.studentId}`)}
        >
          View Student
        </Button>
      ),
    },
  ];

  return (
    <Table<IStudentParent>
      dataSource={studentParents ?? []}
      columns={columns}
      rowKey="id"
      loading={isPending}
      pagination={false}
      size="small"
      locale={{ emptyText: <Empty description="No children linked to this parent" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
    />
  );
}

// ─── Main Content ─────────────────────────────────────────────
function ParentDetailContent() {
  const params = useParams();
  const router = useRouter();
  const parentId = params.id as string;

  const parentState = useParentState();
  const { getAsync } = useParentActions();
  const parent = parentState.parent;

  useEffect(() => {
    if (parentId) getAsync(parentId);
  }, [parentId, getAsync]);

  if (parentState.isPending && !parent) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 80 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (parentState.isError || (!parentState.isPending && !parent)) {
    return (
      <div style={{ padding: 24 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => router.push('/principal/parents')} style={{ marginBottom: 16 }}>
          Back to Parents
        </Button>
        <Empty description="Parent not found" />
      </div>
    );
  }

  if (!parent) return null;

  const tabItems = [
    {
      key: 'children',
      label: <span><TeamOutlined /> Children ({parent.studentCount})</span>,
      children: <ChildrenSection parentId={parentId} />,
    },
    {
      key: 'details',
      label: <span><UserOutlined /> Personal Details</span>,
      children: (
        <Descriptions column={{ xs: 1, sm: 2 }} size="small" bordered>
          <Descriptions.Item label="First Name">{parent.firstName || '—'}</Descriptions.Item>
          <Descriptions.Item label="Last Name">{parent.lastName || '—'}</Descriptions.Item>
          <Descriptions.Item label="Email">{parent.email || '—'}</Descriptions.Item>
          <Descriptions.Item label="Phone">{parent.phone || '—'}</Descriptions.Item>
          <Descriptions.Item label="Work Phone">{parent.workPhone || '—'}</Descriptions.Item>
          <Descriptions.Item label="ID Number">{parent.idNumber || '—'}</Descriptions.Item>
          <Descriptions.Item label="Occupation">{parent.occupation || '—'}</Descriptions.Item>
          <Descriptions.Item label="Employer">{parent.employer || '—'}</Descriptions.Item>
          {parent.address && (
            <>
              <Descriptions.Item label="Street">{parent.address.streetAddress || '—'}</Descriptions.Item>
              <Descriptions.Item label="Suburb">{parent.address.suburb || '—'}</Descriptions.Item>
              <Descriptions.Item label="City">{parent.address.city || '—'}</Descriptions.Item>
              <Descriptions.Item label="Province">{parent.address.province || '—'}</Descriptions.Item>
              <Descriptions.Item label="Postal Code">{parent.address.postalCode || '—'}</Descriptions.Item>
              <Descriptions.Item label="Country">{parent.address.country || '—'}</Descriptions.Item>
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
        onClick={() => router.push('/principal/parents')}
        style={{ marginBottom: 16 }}
      >
        Back to Parents
      </Button>

      <Card style={{ marginBottom: 24 }}>
        <Row gutter={24} align="middle">
          <Col flex="none">
            <Avatar
              size={64}
              icon={<UserOutlined />}
              src={parent.profilePhotoUrl || undefined}
              style={{ backgroundColor: '#722ED1' }}
            />
          </Col>
          <Col flex="auto">
            <Title level={3} style={{ margin: 0 }}>{parent.fullName}</Title>
            <div style={{ marginTop: 8 }}>
              <Space size="large" wrap>
                {parent.email && <Text type="secondary"><MailOutlined /> {parent.email}</Text>}
                {parent.phone && <Text type="secondary"><PhoneOutlined /> {parent.phone}</Text>}
                {parent.idNumber && <Text type="secondary"><IdcardOutlined /> {parent.idNumber}</Text>}
                {parent.occupation && (
                  <Text type="secondary"><BankOutlined /> {parent.occupation}{parent.employer ? ` at ${parent.employer}` : ''}</Text>
                )}
              </Space>
            </div>
          </Col>
          <Col>
            <Card size="small" style={{ textAlign: 'center', minWidth: 100 }}>
              <Statistic title="Children" value={parent.studentCount} prefix={<TeamOutlined />} />
            </Card>
          </Col>
        </Row>
      </Card>

      <Card>
        <Tabs items={tabItems} defaultActiveKey="children" />
      </Card>
    </div>
  );
}

// ─── Wrapped with providers ───────────────────────────────────
export default function ParentDetailPage() {
  return (
    <ParentProvider>
      <StudentParentProvider>
        <ParentDetailContent />
      </StudentParentProvider>
    </ParentProvider>
  );
}
