'use client';

import React, { useEffect, useCallback } from 'react';
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
import { SchoolTransportProvider, useSchoolTransportState, useSchoolTransportActions } from '@/providers/saspecific/school_transports';
import { StudentTransportProvider, useStudentTransportState, useStudentTransportActions } from '@/providers/saspecific/student_transports';
import type { IStudentTransportList } from '@/providers/saspecific/shared/interfaces';

const { Title } = Typography;

const transportTypeMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Bus', color: 'blue' },
  2: { label: 'Minibus', color: 'cyan' },
  3: { label: 'Sedan', color: 'green' },
  4: { label: 'Other', color: 'default' },
};

const directionMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Morning', color: 'orange' },
  2: { label: 'Afternoon', color: 'purple' },
  3: { label: 'Both', color: 'blue' },
};

const enrollmentStatusMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Active', color: 'green' },
  2: { label: 'Terminated', color: 'red' },
  3: { label: 'Suspended', color: 'orange' },
};

function TransportDetailContent() {
  const params = useParams();
  const router = useRouter();
  const transportId = params.id as string;

  const { schoolTransport, isPending: routeLoading, isError: routeError } = useSchoolTransportState();
  const { getAsync: getRouteAsync } = useSchoolTransportActions();
  const { studentTransports, isPending: studentsLoading } = useStudentTransportState();
  const { getByTransportAsync } = useStudentTransportActions();

  useEffect(() => {
    if (transportId) {
      getRouteAsync(transportId);
      getByTransportAsync(transportId);
    }
  }, [transportId, getRouteAsync, getByTransportAsync]);

  if (routeLoading && !schoolTransport) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 80 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (routeError || (!routeLoading && !schoolTransport)) {
    return (
      <div style={{ padding: 24 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => router.push('/principal/transport')} style={{ marginBottom: 16 }}>
          Back to Transport
        </Button>
        <Empty description="Transport route not found" />
      </div>
    );
  }

  if (!schoolTransport) return null;

  const available = schoolTransport.capacity - schoolTransport.currentEnrollment;
  const utilization = schoolTransport.capacity > 0
    ? ((schoolTransport.currentEnrollment / schoolTransport.capacity) * 100).toFixed(1)
    : '0.0';
  const typeInfo = transportTypeMap[schoolTransport.transportType] ?? { label: 'Unknown', color: 'default' };

  const studentColumns = [
    { title: 'Student', dataIndex: 'studentName', key: 'studentName' },
    { title: 'Admission #', dataIndex: 'studentAdmissionNumber', key: 'studentAdmissionNumber', width: 120 },
    {
      title: 'Direction', dataIndex: 'direction', key: 'direction', width: 100,
      render: (value: number) => {
        const info = directionMap[value];
        return info ? <Tag color={info.color}>{info.label}</Tag> : <Tag>{value}</Tag>;
      },
    },
    { title: 'Pickup Address', dataIndex: 'pickupAddress', key: 'pickupAddress', ellipsis: true },
    { title: 'Year', dataIndex: 'academicYearName', key: 'academicYearName', width: 110 },
    {
      title: 'Status', dataIndex: 'status', key: 'status', width: 110,
      render: (value: number) => {
        const info = enrollmentStatusMap[value];
        return info ? <Tag color={info.color}>{info.label}</Tag> : <Tag>{value}</Tag>;
      },
    },
    {
      title: 'Start Date', dataIndex: 'startDate', key: 'startDate', width: 110,
      render: (value: string) => value ? dayjs(value).format('DD MMM YYYY') : '-',
    },
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      {/* Back Button */}
      <Space>
        <Button icon={<ArrowLeftOutlined />} onClick={() => router.push('/principal/transport')}>
          Back to Transport
        </Button>
      </Space>

      {/* Route Info Card */}
      <Card>
        <Row justify="space-between" align="middle" style={{ marginBottom: 16 }}>
          <Col>
            <Title level={3} style={{ margin: 0 }}>{schoolTransport.routeName}</Title>
          </Col>
          <Col>
            <Tag color={schoolTransport.isActive ? 'green' : 'default'}>
              {schoolTransport.isActive ? 'Active' : 'Inactive'}
            </Tag>
          </Col>
        </Row>
        <Descriptions bordered size="small" column={{ xs: 1, sm: 2, md: 3 }}>
          <Descriptions.Item label="Route Name">{schoolTransport.routeName}</Descriptions.Item>
          <Descriptions.Item label="Type">
            <Tag color={typeInfo.color}>{typeInfo.label}</Tag>
          </Descriptions.Item>
          <Descriptions.Item label="Vehicle Number">{schoolTransport.vehicleNumber ?? '-'}</Descriptions.Item>
          <Descriptions.Item label="Driver">{schoolTransport.driverName ?? '-'}</Descriptions.Item>
          <Descriptions.Item label="Driver Phone">{schoolTransport.driverPhone ?? '-'}</Descriptions.Item>
          <Descriptions.Item label="Capacity">{schoolTransport.capacity}</Descriptions.Item>
          <Descriptions.Item label="Currently Enrolled">{schoolTransport.currentEnrollment}</Descriptions.Item>
          <Descriptions.Item label="Monthly Fee">
            R {schoolTransport.monthlyFee.toLocaleString('en-ZA', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
          </Descriptions.Item>
          <Descriptions.Item label="Morning Pickup">{schoolTransport.morningPickupTime ?? '-'}</Descriptions.Item>
          <Descriptions.Item label="Afternoon Departure">{schoolTransport.afternoonDepartureTime ?? '-'}</Descriptions.Item>
          {schoolTransport.areasCovered && (
            <Descriptions.Item label="Areas Covered" span={3}>{schoolTransport.areasCovered}</Descriptions.Item>
          )}
          {schoolTransport.description && (
            <Descriptions.Item label="Description" span={3}>{schoolTransport.description}</Descriptions.Item>
          )}
        </Descriptions>
      </Card>

      {/* Summary Stats */}
      <Row gutter={16}>
        <Col xs={8}>
          <Card size="small">
            <Statistic
              title="Total Enrolled"
              value={schoolTransport.currentEnrollment}
              valueStyle={{ color: '#1677ff' }}
            />
          </Card>
        </Col>
        <Col xs={8}>
          <Card size="small">
            <Statistic
              title="Available Spots"
              value={available}
              valueStyle={{ color: available > 0 ? '#3f8600' : '#cf1322' }}
            />
          </Card>
        </Col>
        <Col xs={8}>
          <Card size="small">
            <Statistic
              title="Capacity Utilization"
              value={utilization}
              suffix="%"
              valueStyle={{ color: Number(utilization) >= 90 ? '#cf1322' : Number(utilization) >= 70 ? '#faad14' : '#3f8600' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Student Enrollments Table */}
      <Card title="Enrolled Students">
        <Table<IStudentTransportList>
          dataSource={studentTransports ?? []}
          columns={studentColumns}
          rowKey="id"
          loading={studentsLoading}
          pagination={{ pageSize: 10, showSizeChanger: true, showTotal: (total) => `Total ${total} students` }}
          size="small"
          locale={{ emptyText: <Empty description="No students enrolled on this route" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
        />
      </Card>
    </div>
  );
}

export default function TransportDetailPage() {
  return (
    <SchoolTransportProvider>
      <StudentTransportProvider>
        <TransportDetailContent />
      </StudentTransportProvider>
    </SchoolTransportProvider>
  );
}
