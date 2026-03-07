'use client';

import React, { useState, useCallback, useEffect } from 'react';
import { Card, Col, Row, Select, DatePicker, Statistic, Tabs } from 'antd';
import {
  DollarOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  EyeOutlined,
} from '@ant-design/icons';
import dayjs, { Dayjs } from 'dayjs';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction } from '@/components/shared/enterprise-table';
import { FeeStructureProvider, useFeeStructureState, useFeeStructureActions } from '@/providers/financial/fee_structures';
import { PaymentProvider, usePaymentState, usePaymentActions } from '@/providers/financial/payments';
import { useAuthState } from '@/providers/auth';
import type { IFeeStructureList } from '@/providers/financial/shared/interfaces';
import type { IPaymentList } from '@/providers/financial/shared/interfaces';

const { RangePicker } = DatePicker;

const feeTypeMap: Record<number, { label: string; color: string }> = {
  0: { label: 'Tuition', color: 'blue' },
  1: { label: 'Registration', color: 'green' },
  2: { label: 'Uniform', color: 'purple' },
  3: { label: 'Transport', color: 'orange' },
  4: { label: 'Stationery', color: 'cyan' },
  5: { label: 'Other', color: 'default' },
};

const paymentStatusMap: Record<number, { label: string; color: string }> = {
  0: { label: 'Pending', color: 'orange' },
  1: { label: 'Completed', color: 'green' },
  2: { label: 'Failed', color: 'red' },
  3: { label: 'Refunded', color: 'purple' },
  4: { label: 'Cancelled', color: 'default' },
};

const paymentMethodMap: Record<number, { label: string; color: string }> = {
  0: { label: 'Cash', color: 'green' },
  1: { label: 'EFT', color: 'blue' },
  2: { label: 'Card', color: 'purple' },
  3: { label: 'Debit Order', color: 'cyan' },
  4: { label: 'Other', color: 'default' },
};

function FinanceContent() {
  const { feeStructures, totalCount: feeTotal, isPending: feePending, isError: feeError } = useFeeStructureState();
  const { getAllAsync: getAllFees } = useFeeStructureActions();
  const { payments, totalCount: paymentTotal, isPending: paymentPending, isError: paymentError } = usePaymentState();
  const { getAllAsync: getAllPayments } = usePaymentActions();
  const { currentRole } = useAuthState();

  const [activeTab, setActiveTab] = useState('fees');
  const [paymentDateRange, setPaymentDateRange] = useState<[Dayjs, Dayjs] | null>(null);
  const [lastFeeQuery, setLastFeeQuery] = useState<TableQuery | null>(null);
  const [lastPaymentQuery, setLastPaymentQuery] = useState<TableQuery | null>(null);

  const handleFeeQueryChange = useCallback((query: TableQuery) => {
    setLastFeeQuery(query);
    getAllFees({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
    });
  }, [getAllFees]);

  const handlePaymentQueryChange = useCallback((query: TableQuery) => {
    setLastPaymentQuery(query);
    getAllPayments({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      fromDate: paymentDateRange?.[0]?.format('YYYY-MM-DD'),
      toDate: paymentDateRange?.[1]?.format('YYYY-MM-DD'),
    });
  }, [getAllPayments, paymentDateRange]);

  // Re-fetch payments when date range changes
  useEffect(() => {
    if (lastPaymentQuery) handlePaymentQueryChange(lastPaymentQuery);
  }, [paymentDateRange]);

  const handleDateRangeChange = (dates: [Dayjs | null, Dayjs | null] | null) => {
    if (dates && dates[0] && dates[1]) {
      setPaymentDateRange([dates[0], dates[1]]);
    } else {
      setPaymentDateRange(null);
    }
  };

  // Payment summary stats
  const completedPayments = payments?.filter(p => p.status === 1) ?? [];
  const totalCollected = completedPayments.reduce((sum, p) => sum + p.amount, 0);
  const pendingPayments = payments?.filter(p => p.status === 0) ?? [];
  const totalPending = pendingPayments.reduce((sum, p) => sum + p.amount, 0);

  // --- Fee Structures Tab ---
  const feeColumns: ColumnConfig<IFeeStructureList>[] = [
    { key: 'feeName', title: 'Fee Name', dataIndex: 'feeName', sortable: true, filterable: true },
    {
      key: 'feeType', title: 'Type', dataIndex: 'feeType', width: 110,
      renderType: 'status',
      renderConfig: { statusMap: feeTypeMap },
    },
    { key: 'gradeName', title: 'Grade', dataIndex: 'gradeName', sortable: true, filterable: true },
    { key: 'academicYearName', title: 'Year', dataIndex: 'academicYearName', sortable: true, hideOnMobile: true },
    { key: 'formattedAmount', title: 'Amount', dataIndex: 'formattedAmount', sortable: true, width: 120 },
    { key: 'billingFrequency', title: 'Frequency', dataIndex: 'billingFrequency', hideOnMobile: true, width: 100 },
    { key: 'studentFeeCount', title: 'Students', dataIndex: 'studentFeeCount', sortable: true, width: 90 },
    {
      key: 'isActive', title: 'Status', dataIndex: 'isActive',
      renderType: 'status',
      renderConfig: {
        statusMap: {
          true: { label: 'Active', color: 'green' },
          false: { label: 'Inactive', color: 'default' },
        },
      },
    },
  ];

  const feeRowActions: RowAction<IFeeStructureList>[] = [
    {
      key: 'view',
      label: 'View Details',
      icon: <EyeOutlined />,
      onClick: (record) => {
        console.log('View fee structure:', record.id);
      },
    },
  ];

  // --- Payments Tab ---
  const paymentColumns: ColumnConfig<IPaymentList>[] = [
    { key: 'receiptNumber', title: 'Receipt #', dataIndex: 'receiptNumber', sortable: true, width: 110 },
    { key: 'studentName', title: 'Student', dataIndex: 'studentName', sortable: true, filterable: true },
    { key: 'parentName', title: 'Parent', dataIndex: 'parentName', sortable: true, hideOnMobile: true },
    { key: 'formattedAmount', title: 'Amount', dataIndex: 'formattedAmount', sortable: true, width: 120 },
    {
      key: 'paymentMethod', title: 'Method', dataIndex: 'paymentMethod', width: 110, hideOnMobile: true,
      renderType: 'status',
      renderConfig: { statusMap: paymentMethodMap },
    },
    { key: 'paymentDate', title: 'Date', dataIndex: 'paymentDate', sortable: true, renderType: 'date', width: 110 },
    {
      key: 'status', title: 'Status', dataIndex: 'status',
      renderType: 'status',
      renderConfig: { statusMap: paymentStatusMap },
    },
  ];

  const paymentRowActions: RowAction<IPaymentList>[] = [
    {
      key: 'view',
      label: 'View Details',
      icon: <EyeOutlined />,
      onClick: (record) => {
        console.log('View payment:', record.id);
      },
    },
  ];

  const tabItems = [
    {
      key: 'fees',
      label: 'Fee Structures',
      children: (
        <EnterpriseTable<IFeeStructureList>
          title="Fee Structures"
          columns={feeColumns}
          data={feeStructures ?? []}
          totalCount={feeTotal}
          loading={feePending}
          error={feeError}
          onQueryChange={handleFeeQueryChange}
          rowKey="id"
          rowActions={feeRowActions}
          currentUserRole={currentRole}
          exportConfig={{
            enabled: true,
            formats: ['csv', 'xlsx'],
            requiredPermissions: ['Admin', 'Principal'],
          }}
        />
      ),
    },
    {
      key: 'payments',
      label: 'Payments',
      children: (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          {/* Payment Filters */}
          <Card size="small">
            <Row gutter={16} align="middle">
              <Col xs={24} sm={12} md={8}>
                <label style={{ display: 'block', fontSize: 13, marginBottom: 4, fontWeight: 500 }}>Date Range</label>
                <RangePicker
                  value={paymentDateRange}
                  onChange={handleDateRangeChange}
                  style={{ width: '100%' }}
                  allowClear
                />
              </Col>
            </Row>
          </Card>

          {/* Payment Summary */}
          <Row gutter={16}>
            <Col xs={12} sm={8}>
              <Card size="small">
                <Statistic
                  title="Total Collected"
                  value={totalCollected}
                  precision={2}
                  prefix={<DollarOutlined />}
                  valueStyle={{ color: '#52c41a' }}
                />
              </Card>
            </Col>
            <Col xs={12} sm={8}>
              <Card size="small">
                <Statistic
                  title="Pending"
                  value={totalPending}
                  precision={2}
                  prefix={<ClockCircleOutlined />}
                  valueStyle={{ color: '#faad14' }}
                />
              </Card>
            </Col>
            <Col xs={12} sm={8}>
              <Card size="small">
                <Statistic
                  title="Completed Payments"
                  value={completedPayments.length}
                  prefix={<CheckCircleOutlined />}
                  valueStyle={{ color: '#0066CC' }}
                />
              </Card>
            </Col>
          </Row>

          {/* Payments Table */}
          <EnterpriseTable<IPaymentList>
            title="Payments"
            columns={paymentColumns}
            data={payments ?? []}
            totalCount={paymentTotal}
            loading={paymentPending}
            error={paymentError}
            onQueryChange={handlePaymentQueryChange}
            rowKey="id"
            rowActions={paymentRowActions}
            currentUserRole={currentRole}
            exportConfig={{
              enabled: true,
              formats: ['csv', 'xlsx'],
              requiredPermissions: ['Admin', 'Principal'],
            }}
          />
        </div>
      ),
    },
  ];

  return (
    <Card size="small" styles={{ body: { padding: 0 } }}>
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={tabItems}
        style={{ padding: '0 16px' }}
      />
    </Card>
  );
}

export default function FinanceOverviewPageContent() {
  return (
    <FeeStructureProvider>
      <PaymentProvider>
        <FinanceContent />
      </PaymentProvider>
    </FeeStructureProvider>
  );
}
