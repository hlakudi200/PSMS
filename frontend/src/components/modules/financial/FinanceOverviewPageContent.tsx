'use client';

import React, { useState, useCallback, useEffect } from 'react';
import { useRouter } from 'next/navigation';
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

const paymentStatusMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Pending', color: 'orange' },
  2: { label: 'Completed', color: 'green' },
  3: { label: 'Failed', color: 'red' },
  4: { label: 'Cancelled', color: 'default' },
  5: { label: 'Refunded', color: 'purple' },
  6: { label: 'Partially Paid', color: 'gold' },
  7: { label: 'Overdue', color: 'volcano' },
};

const paymentMethodMap: Record<number, { label: string; color: string }> = {
  1: { label: 'EFT', color: 'blue' },
  2: { label: 'Debit Order', color: 'cyan' },
  3: { label: 'Credit Card', color: 'purple' },
  4: { label: 'Debit Card', color: 'geekblue' },
  5: { label: 'Cash', color: 'green' },
  6: { label: 'Cheque', color: 'default' },
  7: { label: 'PayFast', color: 'orange' },
  8: { label: 'SnapScan', color: 'magenta' },
  9: { label: 'Zapper', color: 'lime' },
  10: { label: 'Ozow', color: 'gold' },
  11: { label: 'Bank Deposit', color: 'volcano' },
};

function FinanceContent() {
  const router = useRouter();
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
    const { keyword, ...columnFilters } = query.filters ?? {};
    getAllFees({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      feeName: keyword as string | undefined,
      ...columnFilters,
    });
  }, [getAllFees]);

  const handlePaymentQueryChange = useCallback((query: TableQuery) => {
    setLastPaymentQuery(query);
    const { keyword, ...columnFilters } = query.filters ?? {};
    getAllPayments({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      fromDate: paymentDateRange?.[0]?.format('YYYY-MM-DD'),
      toDate: paymentDateRange?.[1]?.format('YYYY-MM-DD'),
      studentName: keyword as string | undefined,
      ...columnFilters,
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
  const completedPayments = payments?.filter(p => p.status === 2) ?? [];
  const totalCollected = completedPayments.reduce((sum, p) => sum + p.amount, 0);
  const pendingPayments = payments?.filter(p => p.status === 1) ?? [];
  const totalPending = pendingPayments.reduce((sum, p) => sum + p.amount, 0);

  // --- Fee Structures Tab ---
  const feeColumns: ColumnConfig<IFeeStructureList>[] = [
    { key: 'feeName', title: 'Fee Name', dataIndex: 'feeName', sortable: true },
    {
      key: 'feeType', title: 'Type', dataIndex: 'feeType', width: 110,
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Tuition', value: 1 },
        { label: 'Registration', value: 2 },
        { label: 'Application', value: 3 },
        { label: 'Transport', value: 4 },
        { label: 'After Care', value: 5 },
        { label: 'Extramural', value: 6 },
        { label: 'Uniform', value: 7 },
        { label: 'Stationery', value: 8 },
        { label: 'Other', value: 9 },
      ],
      renderType: 'status',
      renderConfig: { statusMap: feeTypeMap },
    },
    { key: 'gradeName', title: 'Grade', dataIndex: 'gradeName', sortable: true },
    { key: 'academicYearName', title: 'Year', dataIndex: 'academicYearName', sortable: true, hideOnMobile: true },
    { key: 'formattedAmount', title: 'Amount', dataIndex: 'formattedAmount', sortable: true, width: 120 },
    { key: 'billingFrequency', title: 'Frequency', dataIndex: 'billingFrequency', hideOnMobile: true, width: 100 },
    { key: 'studentFeeCount', title: 'Students', dataIndex: 'studentFeeCount', sortable: true, width: 90 },
    {
      key: 'isActive', title: 'Status', dataIndex: 'isActive',
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Active', value: 'true' },
        { label: 'Inactive', value: 'false' },
      ],
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
        router.push(`/principal/finance/${record.id}`);
      },
    },
  ];

  // --- Payments Tab ---
  const paymentColumns: ColumnConfig<IPaymentList>[] = [
    { key: 'receiptNumber', title: 'Receipt #', dataIndex: 'receiptNumber', sortable: true, width: 110 },
    { key: 'studentName', title: 'Student', dataIndex: 'studentName', sortable: true },
    { key: 'parentName', title: 'Parent', dataIndex: 'parentName', sortable: true, hideOnMobile: true },
    { key: 'formattedAmount', title: 'Amount', dataIndex: 'formattedAmount', sortable: true, width: 120 },
    {
      key: 'paymentMethod', title: 'Method', dataIndex: 'paymentMethod', width: 110, hideOnMobile: true,
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'EFT', value: 1 },
        { label: 'Debit Order', value: 2 },
        { label: 'Credit Card', value: 3 },
        { label: 'Debit Card', value: 4 },
        { label: 'Cash', value: 5 },
        { label: 'Cheque', value: 6 },
        { label: 'PayFast', value: 7 },
        { label: 'SnapScan', value: 8 },
        { label: 'Zapper', value: 9 },
        { label: 'Ozow', value: 10 },
        { label: 'Bank Deposit', value: 11 },
      ],
      renderType: 'status',
      renderConfig: { statusMap: paymentMethodMap },
    },
    { key: 'paymentDate', title: 'Date', dataIndex: 'paymentDate', sortable: true, filterable: true, filterType: 'date', renderType: 'date', width: 110 },
    {
      key: 'status', title: 'Status', dataIndex: 'status',
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Pending', value: 1 },
        { label: 'Completed', value: 2 },
        { label: 'Failed', value: 3 },
        { label: 'Cancelled', value: 4 },
        { label: 'Refunded', value: 5 },
        { label: 'Partially Paid', value: 6 },
        { label: 'Overdue', value: 7 },
      ],
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
        router.push(`/principal/payments/${record.id}`);
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
