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
  Progress,
} from 'antd';
import { ArrowLeftOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { PaymentProvider, usePaymentState, usePaymentActions } from '@/providers/financial/payments';
import { PaymentAllocationProvider, usePaymentAllocationState, usePaymentAllocationActions } from '@/providers/financial/payment_allocations';
import type { IPaymentAllocationList } from '@/providers/financial/shared/interfaces';

const { Title, Text } = Typography;

const paymentStatusMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Pending', color: 'orange' },
  2: { label: 'Completed', color: 'green' },
  3: { label: 'Failed', color: 'red' },
  4: { label: 'Cancelled', color: 'default' },
  5: { label: 'Refunded', color: 'purple' },
  6: { label: 'Partially Paid', color: 'gold' },
  7: { label: 'Overdue', color: 'volcano' },
};

const paymentMethodMap: Record<number, string> = {
  1: 'EFT',
  2: 'Debit Order',
  3: 'Credit Card',
  4: 'Debit Card',
  5: 'Cash',
  6: 'Cheque',
  7: 'PayFast',
  8: 'SnapScan',
  9: 'Zapper',
  10: 'Ozow',
  11: 'Bank Deposit',
};

function PaymentDetailContent() {
  const params = useParams();
  const router = useRouter();
  const paymentId = params.id as string;

  const { payment, isPending: paymentLoading } = usePaymentState();
  const { getAsync: getPayment } = usePaymentActions();
  const { paymentAllocations, isPending: allocationsLoading } = usePaymentAllocationState();
  const { getByPaymentAsync } = usePaymentAllocationActions();

  useEffect(() => {
    if (paymentId) {
      getPayment(paymentId);
      getByPaymentAsync(paymentId);
    }
  }, [paymentId]);

  if (paymentLoading && !payment) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 80 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (!paymentLoading && !payment) {
    return (
      <div style={{ padding: 24 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => router.push('/principal/finance')} style={{ marginBottom: 16 }}>
          Back to Finance
        </Button>
        <Empty description="Payment not found" />
      </div>
    );
  }

  if (!payment) return null;

  const status = paymentStatusMap[payment.status] ?? { label: 'Unknown', color: 'default' };
  const method = paymentMethodMap[payment.paymentMethod] ?? 'Unknown';
  const allocations = paymentAllocations ?? [];
  const totalAllocated = allocations.reduce((sum, a) => sum + a.amount, 0);
  const unallocated = payment.amount - totalAllocated;
  const allocationPercent = payment.amount > 0 ? (totalAllocated / payment.amount) * 100 : 0;

  const allocationColumns = [
    {
      title: 'Fee', dataIndex: 'feeStructureName', key: 'feeStructureName',
      render: (v: string) => <Text strong>{v}</Text>,
    },
    { title: 'Student', dataIndex: 'studentName', key: 'studentName' },
    { title: 'Receipt #', dataIndex: 'receiptNumber', key: 'receiptNumber', width: 120 },
    {
      title: 'Amount Allocated', dataIndex: 'amount', key: 'amount', width: 140,
      render: (v: number) => (
        <Text strong style={{ color: '#3f8600' }}>
          R {v.toLocaleString('en-ZA', { minimumFractionDigits: 2 })}
        </Text>
      ),
    },
    {
      title: 'Date', dataIndex: 'allocatedDate', key: 'allocatedDate', width: 120,
      render: (v: string) => v ? dayjs(v).format('DD MMM YYYY') : '-',
    },
  ];

  return (
    <div style={{ padding: 24 }}>
      <Button icon={<ArrowLeftOutlined />} onClick={() => router.push('/principal/finance')} style={{ marginBottom: 16 }}>
        Back to Finance
      </Button>

      {/* Payment Header */}
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Title level={3} style={{ margin: 0 }}>Payment {payment.receiptNumber}</Title>
            <Space size="large" style={{ marginTop: 8 }}>
              <Tag color={status.color}>{status.label}</Tag>
              <Text type="secondary">Student: {payment.studentName}</Text>
              <Text type="secondary">Parent: {payment.parentName}</Text>
            </Space>
          </Col>
          <Col>
            <Statistic
              title="Payment Amount"
              value={payment.amount}
              precision={2}
              prefix="R"
              valueStyle={{ fontSize: 24, fontWeight: 700 }}
            />
          </Col>
        </Row>
      </Card>

      {/* Payment Details */}
      <Card title="Payment Information" style={{ marginBottom: 16 }}>
        <Descriptions column={{ xs: 1, sm: 2, md: 3 }} size="small" bordered>
          <Descriptions.Item label="Receipt Number">{payment.receiptNumber}</Descriptions.Item>
          <Descriptions.Item label="Payment Method">{method}</Descriptions.Item>
          <Descriptions.Item label="Payment Date">
            {payment.paymentDate ? dayjs(payment.paymentDate).format('DD MMM YYYY') : 'N/A'}
          </Descriptions.Item>
          <Descriptions.Item label="Currency">{payment.currency}</Descriptions.Item>
          <Descriptions.Item label="Reference">{payment.paymentReference || 'N/A'}</Descriptions.Item>
          <Descriptions.Item label="Status">
            <Tag color={status.color}>{status.label}</Tag>
          </Descriptions.Item>
          <Descriptions.Item label="Student">
            {payment.studentName} ({payment.studentAdmissionNumber})
          </Descriptions.Item>
          <Descriptions.Item label="Parent">{payment.parentName}</Descriptions.Item>
          {payment.notes && (
            <Descriptions.Item label="Notes" span={3}>{payment.notes}</Descriptions.Item>
          )}
        </Descriptions>
      </Card>

      {/* Allocation Summary */}
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Total Payment"
              value={payment.amount}
              precision={2}
              prefix="R"
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Allocated"
              value={totalAllocated}
              precision={2}
              prefix="R"
              valueStyle={{ color: '#3f8600' }}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Unallocated"
              value={unallocated}
              precision={2}
              prefix="R"
              valueStyle={{ color: unallocated > 0 ? '#faad14' : '#3f8600' }}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <div style={{ marginBottom: 4 }}>
              <Text type="secondary" style={{ fontSize: 12 }}>Allocation Progress</Text>
            </div>
            <Progress
              percent={Math.round(allocationPercent)}
              status={allocationPercent >= 100 ? 'success' : 'active'}
              size="small"
            />
          </Card>
        </Col>
      </Row>

      {/* Allocations Table */}
      <Card title={`Fee Allocations (${allocations.length})`}>
        <Table<IPaymentAllocationList>
          dataSource={allocations}
          columns={allocationColumns}
          rowKey="id"
          loading={allocationsLoading}
          pagination={false}
          size="small"
          locale={{ emptyText: <Empty description="No allocations yet" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
          summary={() => {
            if (allocations.length === 0) return null;
            return (
              <Table.Summary fixed>
                <Table.Summary.Row>
                  <Table.Summary.Cell index={0} colSpan={3}>
                    <Text strong>Total Allocated</Text>
                  </Table.Summary.Cell>
                  <Table.Summary.Cell index={3}>
                    <Text strong style={{ color: '#3f8600' }}>
                      R {totalAllocated.toLocaleString('en-ZA', { minimumFractionDigits: 2 })}
                    </Text>
                  </Table.Summary.Cell>
                  <Table.Summary.Cell index={4} />
                </Table.Summary.Row>
              </Table.Summary>
            );
          }}
        />
      </Card>
    </div>
  );
}

export default function PaymentDetailPage() {
  return (
    <PaymentProvider>
      <PaymentAllocationProvider>
        <PaymentDetailContent />
      </PaymentAllocationProvider>
    </PaymentProvider>
  );
}
