'use client';

import React, { useEffect, useCallback, useMemo } from 'react';
import { useParams, useRouter } from 'next/navigation';
import {
  Card,
  Descriptions,
  Tag,
  Table,
  Space,
  Button,
  Typography,
  Spin,
  Empty,
  Statistic,
  Row,
  Col,
} from 'antd';
import { ArrowLeftOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import {
  ExtramuralActivityProvider,
  useExtramuralActivityState,
  useExtramuralActivityActions,
} from '@/providers/saspecific/extramural_activities';
import {
  StudentExtramuralProvider,
  useStudentExtramuralState,
  useStudentExtramuralActions,
} from '@/providers/saspecific/student_extramurals';
import type { IStudentExtramuralList } from '@/providers/saspecific/shared/interfaces';

const { Title } = Typography;

const categoryMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Sport', color: 'blue' },
  2: { label: 'Cultural', color: 'purple' },
  3: { label: 'Academic', color: 'green' },
  4: { label: 'Social', color: 'orange' },
  5: { label: 'Other', color: 'default' },
};

const activityTypeMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Individual', color: 'cyan' },
  2: { label: 'Team', color: 'geekblue' },
};

const dayOfWeekMap: Record<number, string> = {
  0: 'Sunday',
  1: 'Monday',
  2: 'Tuesday',
  3: 'Wednesday',
  4: 'Thursday',
  5: 'Friday',
  6: 'Saturday',
};

const seasonMap: Record<number, string> = {
  1: 'Summer',
  2: 'Winter',
  3: 'All Year',
};

const enrollmentStatusMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Active', color: 'green' },
  2: { label: 'Terminated', color: 'red' },
  3: { label: 'Suspended', color: 'orange' },
};

function ExtramuralsDetailContent() {
  const params = useParams();
  const router = useRouter();
  const activityId = params.id as string;

  const { extramuralActivity, isPending: activityPending } = useExtramuralActivityState();
  const { getAsync } = useExtramuralActivityActions();
  const { studentExtramurals, isPending: studentsPending } = useStudentExtramuralState();
  const { getByActivityAsync } = useStudentExtramuralActions();

  useEffect(() => {
    if (activityId) {
      getAsync(activityId);
      getByActivityAsync(activityId);
    }
  }, [activityId, getAsync, getByActivityAsync]);

  const consentRate = useMemo(() => {
    if (!studentExtramurals || studentExtramurals.length === 0) return 0;
    const signed = studentExtramurals.filter((s: IStudentExtramuralList) => s.consentFormSigned).length;
    return Math.round((signed / studentExtramurals.length) * 100);
  }, [studentExtramurals]);

  const availableSpots = useMemo(() => {
    if (!extramuralActivity) return 0;
    if (extramuralActivity.maxCapacity == null) return null;
    return Math.max(0, extramuralActivity.maxCapacity - (extramuralActivity.currentEnrollment ?? 0));
  }, [extramuralActivity]);

  if (activityPending && !extramuralActivity) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 80 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (!activityPending && !extramuralActivity) {
    return (
      <div style={{ padding: 24 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => router.push('/principal/extramurals')} style={{ marginBottom: 16 }}>
          Back to Extramurals
        </Button>
        <Empty description="Activity not found" />
      </div>
    );
  }

  if (!extramuralActivity) return null;

  const category = categoryMap[extramuralActivity.category] ?? { label: 'Unknown', color: 'default' };
  const actType = activityTypeMap[extramuralActivity.activityType] ?? { label: 'Unknown', color: 'default' };

  const studentColumns = [
    { title: 'Student', dataIndex: 'studentName', key: 'studentName', sorter: true },
    { title: 'Admission #', dataIndex: 'studentAdmissionNumber', key: 'studentAdmissionNumber', width: 120 },
    { title: 'Team', dataIndex: 'teamAssignment', key: 'teamAssignment', render: (v: string | undefined) => v ?? '-' },
    {
      title: 'Consent', dataIndex: 'consentFormSigned', key: 'consentFormSigned', width: 90,
      render: (v: boolean) => <Tag color={v ? 'green' : 'red'}>{v ? 'Yes' : 'No'}</Tag>,
    },
    { title: 'Year', dataIndex: 'academicYearName', key: 'academicYearName' },
    {
      title: 'Status', dataIndex: 'status', key: 'status', width: 110,
      render: (v: number) => {
        const info = enrollmentStatusMap[v] ?? { label: 'Unknown', color: 'default' };
        return <Tag color={info.color}>{info.label}</Tag>;
      },
    },
    {
      title: 'Start Date', dataIndex: 'startDate', key: 'startDate', width: 110,
      render: (v: string) => v ? dayjs(v).format('DD MMM YYYY') : '-',
    },
  ];

  return (
    <div style={{ padding: 24 }}>
      <Space style={{ marginBottom: 16 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => router.push('/principal/extramurals')}>
          Back to Extramurals
        </Button>
      </Space>

      {/* Activity Info */}
      <Card style={{ marginBottom: 16 }}>
        <Title level={3} style={{ margin: 0, marginBottom: 16 }}>{extramuralActivity.activityName}</Title>
        <Descriptions bordered size="small" column={{ xs: 1, sm: 2, md: 3 }}>
          <Descriptions.Item label="Category"><Tag color={category.color}>{category.label}</Tag></Descriptions.Item>
          <Descriptions.Item label="Type"><Tag color={actType.color}>{actType.label}</Tag></Descriptions.Item>
          <Descriptions.Item label="Venue">{extramuralActivity.venue ?? 'N/A'}</Descriptions.Item>
          <Descriptions.Item label="Day">{extramuralActivity.dayOfWeek != null ? dayOfWeekMap[extramuralActivity.dayOfWeek] : 'N/A'}</Descriptions.Item>
          <Descriptions.Item label="Season">{extramuralActivity.season != null ? seasonMap[extramuralActivity.season] : 'N/A'}</Descriptions.Item>
          <Descriptions.Item label="Coach">{extramuralActivity.coachName ?? 'N/A'}</Descriptions.Item>
          <Descriptions.Item label="Fee per Term">R {(extramuralActivity.feePerTerm ?? 0).toFixed(2)}</Descriptions.Item>
          <Descriptions.Item label="Max Capacity">{extramuralActivity.maxCapacity ?? 'Unlimited'}</Descriptions.Item>
          <Descriptions.Item label="Currently Enrolled">{extramuralActivity.currentEnrollment ?? 0}</Descriptions.Item>
          <Descriptions.Item label="Registration">
            <Tag color={extramuralActivity.isRegistrationOpen ? 'green' : 'red'}>
              {extramuralActivity.isRegistrationOpen ? 'Open' : 'Closed'}
            </Tag>
          </Descriptions.Item>
          <Descriptions.Item label="Status">
            <Tag color={extramuralActivity.isActive ? 'green' : 'default'}>
              {extramuralActivity.isActive ? 'Active' : 'Inactive'}
            </Tag>
          </Descriptions.Item>
        </Descriptions>
      </Card>

      {/* Summary Stats */}
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col xs={8}>
          <Card size="small">
            <Statistic title="Total Enrolled" value={extramuralActivity.currentEnrollment ?? 0} />
          </Card>
        </Col>
        <Col xs={8}>
          <Card size="small">
            <Statistic
              title="Available Spots"
              value={availableSpots ?? 'Unlimited'}
              valueStyle={availableSpots != null && availableSpots === 0 ? { color: '#cf1322' } : undefined}
            />
          </Card>
        </Col>
        <Col xs={8}>
          <Card size="small">
            <Statistic
              title="Consent Rate"
              value={consentRate}
              suffix="%"
              valueStyle={{ color: consentRate >= 80 ? '#3f8600' : consentRate >= 50 ? '#faad14' : '#cf1322' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Enrolled Students Table */}
      <Card title="Enrolled Students">
        <Table<IStudentExtramuralList>
          dataSource={studentExtramurals ?? []}
          columns={studentColumns}
          rowKey="id"
          loading={studentsPending}
          pagination={{ pageSize: 10, showSizeChanger: true, showTotal: (total) => `${total} student(s)` }}
          size="small"
          locale={{ emptyText: <Empty description="No students enrolled" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
        />
      </Card>
    </div>
  );
}

export default function ExtramuralsDetailPage() {
  return (
    <ExtramuralActivityProvider>
      <StudentExtramuralProvider>
        <ExtramuralsDetailContent />
      </StudentExtramuralProvider>
    </ExtramuralActivityProvider>
  );
}
