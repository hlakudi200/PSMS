'use client';

import React, { useEffect, useMemo } from 'react';
import { useParams, useRouter } from 'next/navigation';
import {
  Card,
  Descriptions,
  Tag,
  Tabs,
  Table,
  Space,
  Button,
  Typography,
  Avatar,
  Spin,
  Empty,
  Statistic,
  Row,
  Col,
} from 'antd';
import {
  ArrowLeftOutlined,
  UserOutlined,
  PhoneOutlined,
  MailOutlined,
  BookOutlined,
  TeamOutlined,
  DollarOutlined,
  MedicineBoxOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { StudentProvider, useStudentState, useStudentActions } from '@/providers/academic/students';
import { StudentParentProvider, useStudentParentState, useStudentParentActions } from '@/providers/academic/student_parents';
import { StudentSubjectProvider, useStudentSubjectState, useStudentSubjectActions } from '@/providers/academic/student_subjects';
import { StudentFeeProvider, useStudentFeeState, useStudentFeeActions } from '@/providers/financial/student_fees';
import type { IStudent, IStudentParent, IStudentSubject } from '@/providers/academic/shared/interfaces';
import type { IStudentFeeList } from '@/providers/financial/shared/interfaces';

const { Title, Text } = Typography;

const genderLabels: Record<number, string> = { 0: 'Male', 1: 'Female', 2: 'Other' };
const relationshipLabels: Record<number, string> = {
  0: 'Father', 1: 'Mother', 2: 'Guardian', 3: 'Grandparent', 4: 'Sibling', 5: 'Other',
};
const feeStatusLabels: Record<number, { label: string; color: string }> = {
  0: { label: 'Pending', color: 'orange' },
  1: { label: 'Partially Paid', color: 'blue' },
  2: { label: 'Paid', color: 'green' },
  3: { label: 'Overdue', color: 'red' },
  4: { label: 'Waived', color: 'default' },
  5: { label: 'Cancelled', color: 'default' },
};

function formatAddress(addr?: { streetAddress?: string; suburb?: string; city?: string; province?: string; postalCode?: string }) {
  if (!addr) return 'Not provided';
  const parts = [addr.streetAddress, addr.suburb, addr.city, addr.province, addr.postalCode].filter(Boolean);
  return parts.length > 0 ? parts.join(', ') : 'Not provided';
}

// ─── Bio Card ──────────────────────────────────────────────────
function BioSection({ student }: { student: IStudent }) {
  return (
    <Card size="small">
      <Descriptions column={{ xs: 1, sm: 2 }} size="small" bordered>
        <Descriptions.Item label="Date of Birth">{dayjs(student.dateOfBirth).format('DD MMM YYYY')}</Descriptions.Item>
        <Descriptions.Item label="Age">{student.age} years</Descriptions.Item>
        <Descriptions.Item label="Gender">{genderLabels[student.gender] ?? 'Unknown'}</Descriptions.Item>
        <Descriptions.Item label="SA Citizen">{student.isSACitizen ? 'Yes' : 'No'}</Descriptions.Item>
        <Descriptions.Item label="ID Number">{student.idNumber || 'N/A'}</Descriptions.Item>
        <Descriptions.Item label="Passport">{student.passportNumber || 'N/A'}</Descriptions.Item>
        <Descriptions.Item label="Phone">{student.phone || 'N/A'}</Descriptions.Item>
        <Descriptions.Item label="Email">{student.email || 'N/A'}</Descriptions.Item>
        <Descriptions.Item label="Admission Date">{dayjs(student.admissionDate).format('DD MMM YYYY')}</Descriptions.Item>
        <Descriptions.Item label="POPIA Consent">
          {student.popiaConsentGiven ? (
            <Tag color="green">Given {student.popiaConsentDate ? dayjs(student.popiaConsentDate).format('DD MMM YYYY') : ''}</Tag>
          ) : (
            <Tag color="red">Not Given</Tag>
          )}
        </Descriptions.Item>
        <Descriptions.Item label="Physical Address" span={2}>{formatAddress(student.physicalAddress)}</Descriptions.Item>
        <Descriptions.Item label="Postal Address" span={2}>{formatAddress(student.postalAddress)}</Descriptions.Item>
        <Descriptions.Item label="Emergency Contact">
          {student.emergencyContactName ? `${student.emergencyContactName} (${student.emergencyContactPhone || 'No phone'})` : 'Not provided'}
        </Descriptions.Item>
        <Descriptions.Item label="Medical Conditions">{student.medicalConditions || 'None recorded'}</Descriptions.Item>
      </Descriptions>
    </Card>
  );
}

// ─── Subjects Tab ──────────────────────────────────────────────
function SubjectsSection({ studentId }: { studentId: string }) {
  const { studentSubjects, isPending } = useStudentSubjectState();
  const { getByStudentAsync } = useStudentSubjectActions();

  useEffect(() => {
    getByStudentAsync(studentId);
  }, [studentId, getByStudentAsync]);

  const columns = [
    { title: 'Subject', dataIndex: 'subjectName', key: 'subjectName' },
    { title: 'Code', dataIndex: 'subjectCode', key: 'subjectCode', width: 100 },
    { title: 'Academic Year', dataIndex: 'academicYearName', key: 'academicYearName' },
    {
      title: 'Enrolled', dataIndex: 'enrollmentDate', key: 'enrollmentDate',
      render: (d: string) => dayjs(d).format('DD MMM YYYY'),
    },
    {
      title: 'Status', dataIndex: 'isActive', key: 'isActive',
      render: (active: boolean) => <Tag color={active ? 'green' : 'default'}>{active ? 'Active' : 'Inactive'}</Tag>,
    },
  ];

  return (
    <Table<IStudentSubject>
      dataSource={studentSubjects ?? []}
      columns={columns}
      rowKey="id"
      loading={isPending}
      pagination={false}
      size="small"
      locale={{ emptyText: <Empty description="No subjects enrolled" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
    />
  );
}

// ─── Parents Tab ───────────────────────────────────────────────
function ParentsSection({ studentId }: { studentId: string }) {
  const { studentParents, isPending } = useStudentParentState();
  const { getByStudentAsync } = useStudentParentActions();

  useEffect(() => {
    getByStudentAsync(studentId);
  }, [studentId, getByStudentAsync]);

  const columns = [
    { title: 'Name', dataIndex: 'parentName', key: 'parentName' },
    {
      title: 'Relationship', dataIndex: 'relationshipType', key: 'relationshipType',
      render: (val: number) => relationshipLabels[val] ?? 'Unknown',
    },
    {
      title: 'Contact', key: 'contact',
      render: (_: unknown, record: IStudentParent) => (
        <Space direction="vertical" size={0}>
          {record.parentPhone && <Text><PhoneOutlined /> {record.parentPhone}</Text>}
          {record.parentEmail && <Text><MailOutlined /> {record.parentEmail}</Text>}
        </Space>
      ),
    },
    {
      title: 'Roles', key: 'roles',
      render: (_: unknown, record: IStudentParent) => (
        <Space size={[0, 4]} wrap>
          {record.isPrimaryContact && <Tag color="blue">Primary</Tag>}
          {record.isFinanciallyResponsible && <Tag color="gold">Financial</Tag>}
          {record.canPickupStudent && <Tag color="cyan">Pickup</Tag>}
          {record.livesWithStudent && <Tag color="purple">Lives With</Tag>}
        </Space>
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
      locale={{ emptyText: <Empty description="No parents linked" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
    />
  );
}

// ─── Fees Tab ──────────────────────────────────────────────────
function FeesSection({ studentId }: { studentId: string }) {
  const { studentFees, isPending } = useStudentFeeState();
  const { getByStudentAsync } = useStudentFeeActions();

  useEffect(() => {
    getByStudentAsync(studentId);
  }, [studentId, getByStudentAsync]);

  const summary = useMemo(() => {
    const fees = studentFees ?? [];
    const totalDue = fees.reduce((sum, f) => sum + f.amountDue, 0);
    const totalPaid = fees.reduce((sum, f) => sum + f.amountPaid, 0);
    const outstanding = fees.reduce((sum, f) => sum + f.outstandingBalance, 0);
    const overdue = fees.filter(f => f.status === 3).length;
    return { totalDue, totalPaid, outstanding, overdue };
  }, [studentFees]);

  const columns = [
    { title: 'Fee', dataIndex: 'feeStructureName', key: 'feeStructureName' },
    {
      title: 'Amount Due', dataIndex: 'amountDue', key: 'amountDue',
      render: (v: number) => `R ${v.toFixed(2)}`,
    },
    {
      title: 'Paid', dataIndex: 'amountPaid', key: 'amountPaid',
      render: (v: number) => `R ${v.toFixed(2)}`,
    },
    {
      title: 'Outstanding', dataIndex: 'outstandingBalance', key: 'outstandingBalance',
      render: (v: number) => <Text type={v > 0 ? 'danger' : 'success'}>R {v.toFixed(2)}</Text>,
    },
    {
      title: 'Due Date', dataIndex: 'dueDate', key: 'dueDate',
      render: (d: string) => dayjs(d).format('DD MMM YYYY'),
    },
    {
      title: 'Status', dataIndex: 'status', key: 'status',
      render: (s: number) => {
        const info = feeStatusLabels[s] ?? { label: 'Unknown', color: 'default' };
        return <Tag color={info.color}>{info.label}</Tag>;
      },
    },
  ];

  return (
    <div>
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col xs={12} sm={6}><Statistic title="Total Due" value={summary.totalDue} prefix="R" precision={2} /></Col>
        <Col xs={12} sm={6}><Statistic title="Total Paid" value={summary.totalPaid} prefix="R" precision={2} valueStyle={{ color: '#3f8600' }} /></Col>
        <Col xs={12} sm={6}><Statistic title="Outstanding" value={summary.outstanding} prefix="R" precision={2} valueStyle={{ color: summary.outstanding > 0 ? '#cf1322' : '#3f8600' }} /></Col>
        <Col xs={12} sm={6}><Statistic title="Overdue Fees" value={summary.overdue} valueStyle={{ color: summary.overdue > 0 ? '#cf1322' : undefined }} /></Col>
      </Row>
      <Table<IStudentFeeList>
        dataSource={studentFees ?? []}
        columns={columns}
        rowKey="id"
        loading={isPending}
        pagination={false}
        size="small"
        locale={{ emptyText: <Empty description="No fees assigned" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
      />
    </div>
  );
}

// ─── Main Profile Content ──────────────────────────────────────
function StudentProfileContent() {
  const params = useParams();
  const router = useRouter();
  const studentId = params.id as string;

  const { student, isPending, isError } = useStudentState();
  const { getAsync } = useStudentActions();

  useEffect(() => {
    if (studentId) getAsync(studentId);
  }, [studentId, getAsync]);

  if (isPending && !student) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 80 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (isError || (!isPending && !student)) {
    return (
      <div style={{ padding: 24 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => router.push('/principal/students')} style={{ marginBottom: 16 }}>
          Back to Students
        </Button>
        <Empty description="Student not found" />
      </div>
    );
  }

  if (!student) return null;

  const tabItems = [
    {
      key: 'bio',
      label: <span><UserOutlined /> Personal Info</span>,
      children: <BioSection student={student} />,
    },
    {
      key: 'subjects',
      label: <span><BookOutlined /> Subjects ({student.subjectCount})</span>,
      children: <SubjectsSection studentId={studentId} />,
    },
    {
      key: 'parents',
      label: <span><TeamOutlined /> Parents ({student.parentCount})</span>,
      children: <ParentsSection studentId={studentId} />,
    },
    {
      key: 'fees',
      label: <span><DollarOutlined /> Fees</span>,
      children: <FeesSection studentId={studentId} />,
    },
  ];

  return (
    <div style={{ padding: 24 }}>
      <Button
        icon={<ArrowLeftOutlined />}
        onClick={() => router.push('/principal/students')}
        style={{ marginBottom: 16 }}
      >
        Back to Students
      </Button>

      <Card style={{ marginBottom: 24 }}>
        <Space size="large" align="start">
          <Avatar
            size={72}
            icon={<UserOutlined />}
            src={student.profilePhotoUrl}
            style={{ backgroundColor: '#003D73' }}
          />
          <div>
            <Space align="center" size="middle">
              <Title level={3} style={{ margin: 0 }}>{student.fullName}</Title>
              <Tag color={student.isActive ? 'green' : 'default'}>{student.isActive ? 'Active' : 'Inactive'}</Tag>
            </Space>
            <Space size="large" style={{ marginTop: 8 }}>
              <Text type="secondary">Admission #: {student.admissionNumber ?? 'N/A'}</Text>
              <Text type="secondary">Grade: {student.currentGradeName ?? 'N/A'}</Text>
              <Text type="secondary">Class: {student.currentClassName ?? 'N/A'}</Text>
            </Space>
          </div>
        </Space>
      </Card>

      <Card>
        <Tabs items={tabItems} defaultActiveKey="bio" />
      </Card>
    </div>
  );
}

// ─── Wrapped with all providers ────────────────────────────────
export default function StudentProfilePage() {
  return (
    <StudentProvider>
      <StudentParentProvider>
        <StudentSubjectProvider>
          <StudentFeeProvider>
            <StudentProfileContent />
          </StudentFeeProvider>
        </StudentSubjectProvider>
      </StudentParentProvider>
    </StudentProvider>
  );
}
