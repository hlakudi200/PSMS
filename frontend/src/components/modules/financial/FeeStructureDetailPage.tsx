'use client';

import React, { useEffect, useCallback } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { usePortalBase } from '@/utils/portal-base';
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
import { FeeStructureProvider, useFeeStructureState, useFeeStructureActions } from '@/providers/financial/fee_structures';
import { StudentFeeProvider, useStudentFeeState, useStudentFeeActions } from '@/providers/financial/student_fees';
import type { IStudentFeeList } from '@/providers/financial/shared/interfaces';

const { Title, Text } = Typography;

const feeTypeMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Tuition', color: 'blue' },
  2: { label: 'Registration', color: 'green' },
  3: { label: 'Application', color: 'geekblue' },
  4: { label: 'Transport', color: 'orange' },
  5: { label: 'After Care', color: 'magenta' },
  6: { label: 'Extramural', color: 'lime' },
  7: { label: 'Uniform', color: 'purple' },
  8: { label: 'Stationery', color: 'cyan' },
  9: { label: 'Other', color: 'default' },
};

const feeStatusMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Pending', color: 'orange' },
  2: { label: 'Paid', color: 'green' },
  3: { label: 'Partially Paid', color: 'blue' },
  4: { label: 'Overdue', color: 'red' },
  5: { label: 'Waived', color: 'default' },
  6: { label: 'Cancelled', color: 'default' },
};

function FeeStructureDetailContent() {
  const params = useParams();
  const router = useRouter();
  const portalBase = usePortalBase();
  const feeStructureId = params.id as string;

  const { feeStructure, isPending: feeLoading } = useFeeStructureState();
  const { getAsync: getFeeStructure } = useFeeStructureActions();
  const { studentFees, totalCount, isPending: feesLoading } = useStudentFeeState();
  const { getAllAsync: getStudentFees } = useStudentFeeActions();

  useEffect(() => {
    if (feeStructureId) {
      getFeeStructure(feeStructureId);
      getStudentFees({ feeStructureId, maxResultCount: 100 });
    }
  }, [feeStructureId]);

  const refresh = useCallback(() => {
    getFeeStructure(feeStructureId);
    getStudentFees({ feeStructureId, maxResultCount: 100 });
  }, [feeStructureId]);

  if (feeLoading && !feeStructure) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 80 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (!feeLoading && !feeStructure) {
    return (
      <div style={{ padding: 24 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => router.push(`${portalBase}/finance`)} style={{ marginBottom: 16 }}>
          Back to Finance
        </Button>
        <Empty description="Fee structure not found" />
      </div>
    );
  }

  if (!feeStructure) return null;

  const fees = studentFees ?? [];
  const feeType = feeTypeMap[feeStructure.feeType] ?? { label: 'Unknown', color: 'default' };

  // Summary calculations
  const totalDue = fees.reduce((sum, f) => sum + f.amountDue, 0);
  const totalPaid = fees.reduce((sum, f) => sum + f.amountPaid, 0);
  const totalOutstanding = fees.reduce((sum, f) => sum + f.outstandingBalance, 0);
  const paidCount = fees.filter(f => f.status === 2).length;
  const overdueCount = fees.filter(f => f.status === 4).length;

  const columns = [
    {
      title: 'Student', dataIndex: 'studentName', key: 'studentName',
      render: (v: string) => <Text strong>{v}</Text>,
      sorter: (a: IStudentFeeList, b: IStudentFeeList) => (a.studentName ?? '').localeCompare(b.studentName ?? ''),
    },
    { title: 'Admission #', dataIndex: 'studentAdmissionNumber', key: 'studentAdmissionNumber', width: 130 },
    {
      title: 'Amount Due', dataIndex: 'amountDue', key: 'amountDue', width: 120,
      render: (v: number) => `R ${v.toLocaleString('en-ZA', { minimumFractionDigits: 2 })}`,
      sorter: (a: IStudentFeeList, b: IStudentFeeList) => a.amountDue - b.amountDue,
    },
    {
      title: 'Paid', dataIndex: 'amountPaid', key: 'amountPaid', width: 120,
      render: (v: number) => (
        <Text style={{ color: v > 0 ? '#3f8600' : undefined }}>
          R {v.toLocaleString('en-ZA', { minimumFractionDigits: 2 })}
        </Text>
      ),
      sorter: (a: IStudentFeeList, b: IStudentFeeList) => a.amountPaid - b.amountPaid,
    },
    {
      title: 'Outstanding', dataIndex: 'outstandingBalance', key: 'outstandingBalance', width: 130,
      render: (v: number) => (
        <Text style={{ color: v > 0 ? '#cf1322' : '#3f8600' }}>
          R {v.toLocaleString('en-ZA', { minimumFractionDigits: 2 })}
        </Text>
      ),
      sorter: (a: IStudentFeeList, b: IStudentFeeList) => a.outstandingBalance - b.outstandingBalance,
    },
    {
      title: 'Due Date', dataIndex: 'dueDate', key: 'dueDate', width: 120,
      render: (v: string) => v ? dayjs(v).format('DD MMM YYYY') : '-',
      sorter: (a: IStudentFeeList, b: IStudentFeeList) => dayjs(a.dueDate).unix() - dayjs(b.dueDate).unix(),
    },
    {
      title: 'Status', dataIndex: 'status', key: 'status', width: 120,
      render: (s: number) => {
        const info = feeStatusMap[s] ?? { label: 'Unknown', color: 'default' };
        return <Tag color={info.color}>{info.label}</Tag>;
      },
      filters: Object.entries(feeStatusMap).map(([k, v]) => ({ text: v.label, value: Number(k) })),
      onFilter: (value: unknown, record: IStudentFeeList) => record.status === value,
    },
  ];

  return (
    <div style={{ padding: 24 }}>
      <Button icon={<ArrowLeftOutlined />} onClick={() => router.push(`${portalBase}/finance`)} style={{ marginBottom: 16 }}>
        Back to Finance
      </Button>

      {/* Fee Structure Header */}
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Title level={3} style={{ margin: 0 }}>{feeStructure.feeName}</Title>
            <Space size="large" style={{ marginTop: 8 }}>
              <Tag color={feeType.color}>{feeType.label}</Tag>
              <Text type="secondary">Grade: {feeStructure.gradeName}</Text>
              <Text type="secondary">Year: {feeStructure.academicYearName}</Text>
              <Text type="secondary">Frequency: {feeStructure.billingFrequency}</Text>
              <Tag color={feeStructure.isActive ? 'green' : 'default'}>
                {feeStructure.isActive ? 'Active' : 'Inactive'}
              </Tag>
            </Space>
          </Col>
          <Col>
            <Statistic
              title="Fee Amount"
              value={feeStructure.amount}
              precision={2}
              prefix="R"
              valueStyle={{ fontSize: 24, fontWeight: 700 }}
            />
          </Col>
        </Row>
      </Card>

      {/* Summary Stats */}
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Total Due"
              value={totalDue}
              precision={2}
              prefix="R"
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Total Collected"
              value={totalPaid}
              precision={2}
              prefix="R"
              valueStyle={{ color: '#3f8600' }}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Outstanding"
              value={totalOutstanding}
              precision={2}
              prefix="R"
              valueStyle={{ color: totalOutstanding > 0 ? '#cf1322' : '#3f8600' }}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Collection Rate"
              value={totalDue > 0 ? (totalPaid / totalDue) * 100 : 0}
              precision={1}
              suffix="%"
              valueStyle={{ color: totalDue > 0 && (totalPaid / totalDue) >= 0.7 ? '#3f8600' : '#cf1322' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Quick Info */}
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col xs={8}>
          <Card size="small">
            <Statistic title="Students Assigned" value={fees.length} />
          </Card>
        </Col>
        <Col xs={8}>
          <Card size="small">
            <Statistic title="Fully Paid" value={paidCount} valueStyle={{ color: '#3f8600' }} />
          </Card>
        </Col>
        <Col xs={8}>
          <Card size="small">
            <Statistic title="Overdue" value={overdueCount} valueStyle={{ color: overdueCount > 0 ? '#cf1322' : undefined }} />
          </Card>
        </Col>
      </Row>

      {/* Student Fees Table */}
      <Card title={`Student Fees (${fees.length})`}>
        <Table<IStudentFeeList>
          dataSource={fees}
          columns={columns}
          rowKey="id"
          loading={feesLoading}
          pagination={fees.length > 20 ? { pageSize: 20, showSizeChanger: true } : false}
          size="small"
          locale={{ emptyText: <Empty description="No students assigned to this fee" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
          summary={() => {
            if (fees.length === 0) return null;
            return (
              <Table.Summary fixed>
                <Table.Summary.Row>
                  <Table.Summary.Cell index={0} colSpan={2}>
                    <Text strong>Totals</Text>
                  </Table.Summary.Cell>
                  <Table.Summary.Cell index={2}>
                    <Text strong>R {totalDue.toLocaleString('en-ZA', { minimumFractionDigits: 2 })}</Text>
                  </Table.Summary.Cell>
                  <Table.Summary.Cell index={3}>
                    <Text strong style={{ color: '#3f8600' }}>
                      R {totalPaid.toLocaleString('en-ZA', { minimumFractionDigits: 2 })}
                    </Text>
                  </Table.Summary.Cell>
                  <Table.Summary.Cell index={4}>
                    <Text strong style={{ color: totalOutstanding > 0 ? '#cf1322' : '#3f8600' }}>
                      R {totalOutstanding.toLocaleString('en-ZA', { minimumFractionDigits: 2 })}
                    </Text>
                  </Table.Summary.Cell>
                  <Table.Summary.Cell index={5} colSpan={2} />
                </Table.Summary.Row>
              </Table.Summary>
            );
          }}
        />
      </Card>
    </div>
  );
}

export default function FeeStructureDetailPage() {
  return (
    <FeeStructureProvider>
      <StudentFeeProvider>
        <FeeStructureDetailContent />
      </StudentFeeProvider>
    </FeeStructureProvider>
  );
}
