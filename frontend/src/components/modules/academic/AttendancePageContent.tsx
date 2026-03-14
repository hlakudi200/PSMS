'use client';

import React, { useState, useCallback, useEffect, useMemo } from 'react';
import { Card, Col, Row, Select, DatePicker, Statistic, Tabs, Empty, Table, Tag, Progress, Typography, Alert } from 'antd';
import {
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined,
  MedicineBoxOutlined,
  EyeOutlined,
  WarningOutlined,
} from '@ant-design/icons';
import dayjs, { Dayjs } from 'dayjs';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction } from '@/components/shared/enterprise-table';
import { AttendanceProvider, useAttendanceState, useAttendanceActions } from '@/providers/academic/attendances';
import { ClassProvider, useClassState, useClassActions } from '@/providers/academic/classes';
import { useAuthState } from '@/providers/auth';
import type { IAttendanceList, IAttendanceSummary } from '@/providers/academic/shared/interfaces';

const { RangePicker } = DatePicker;
const { Text } = Typography;

const AT_RISK_THRESHOLD = 80;

const statusMap: Record<number, { label: string; color: string }> = {
  0: { label: 'Present', color: 'green' },
  1: { label: 'Absent', color: 'red' },
  2: { label: 'Late', color: 'orange' },
  3: { label: 'Excused', color: 'blue' },
  4: { label: 'Sick Leave', color: 'purple' },
};

// ─── Analytics Tab Content ─────────────────────────────────────
function AnalyticsSection({ summaries }: { summaries: IAttendanceSummary[] }) {
  const atRisk = useMemo(
    () => summaries.filter(s => s.attendancePercentage < AT_RISK_THRESHOLD).sort((a, b) => a.attendancePercentage - b.attendancePercentage),
    [summaries],
  );

  const aggregate = useMemo(() => {
    if (summaries.length === 0) return null;
    const totals = summaries.reduce((acc, s) => ({
      totalDays: acc.totalDays + s.totalDays,
      present: acc.present + s.presentCount,
      absent: acc.absent + s.absentCount,
      late: acc.late + s.lateCount,
      excused: acc.excused + s.excusedCount,
      sickLeave: acc.sickLeave + s.sickLeaveCount,
    }), { totalDays: 0, present: 0, absent: 0, late: 0, excused: 0, sickLeave: 0 });
    const rate = totals.totalDays > 0 ? Math.round((totals.present / totals.totalDays) * 100) : 0;
    return { ...totals, rate, studentCount: summaries.length };
  }, [summaries]);

  if (!aggregate) return <Empty description="Select a class and date range, then switch to the Summary tab first to load data." />;

  const breakdownItems = [
    { label: 'Present', count: aggregate.present, color: '#52c41a' },
    { label: 'Absent', count: aggregate.absent, color: '#ff4d4f' },
    { label: 'Late', count: aggregate.late, color: '#faad14' },
    { label: 'Excused', count: aggregate.excused, color: '#1890ff' },
    { label: 'Sick Leave', count: aggregate.sickLeave, color: '#722ed1' },
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      {/* Overview Stats */}
      <Row gutter={16}>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic title="Overall Rate" value={aggregate.rate} suffix="%" />
            <Progress percent={aggregate.rate} showInfo={false} status={aggregate.rate >= 80 ? 'success' : aggregate.rate >= 60 ? 'normal' : 'exception'} />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small"><Statistic title="Students" value={aggregate.studentCount} /></Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small"><Statistic title="Total Records" value={aggregate.totalDays} /></Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic title="At Risk" value={atRisk.length} valueStyle={{ color: atRisk.length > 0 ? '#cf1322' : '#3f8600' }} prefix={<WarningOutlined />} />
          </Card>
        </Col>
      </Row>

      {/* Status Breakdown */}
      <Card title="Attendance Breakdown" size="small">
        {breakdownItems.map(item => {
          const pct = aggregate.totalDays > 0 ? Math.round((item.count / aggregate.totalDays) * 100) : 0;
          return (
            <div key={item.label} style={{ marginBottom: 12 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                <Text>{item.label}</Text>
                <Text strong>{item.count} ({pct}%)</Text>
              </div>
              <Progress percent={pct} showInfo={false} strokeColor={item.color} size="small" />
            </div>
          );
        })}
      </Card>

      {/* Per-Student Attendance Distribution */}
      <Card title="Student Attendance Rates" size="small">
        <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
          {summaries
            .slice()
            .sort((a, b) => a.attendancePercentage - b.attendancePercentage)
            .map(s => (
              <div key={s.studentId} style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                <Text style={{ width: 160, flexShrink: 0, fontSize: 13 }} ellipsis>{s.studentName}</Text>
                <Progress
                  percent={s.attendancePercentage}
                  size="small"
                  style={{ flex: 1 }}
                  strokeColor={s.attendancePercentage >= 90 ? '#52c41a' : s.attendancePercentage >= AT_RISK_THRESHOLD ? '#faad14' : '#ff4d4f'}
                />
              </div>
            ))
          }
        </div>
      </Card>

      {/* At-Risk Students */}
      {atRisk.length > 0 && (
        <Card
          title={
            <span style={{ color: '#cf1322' }}>
              <WarningOutlined style={{ marginRight: 8 }} />
              At-Risk Students (below {AT_RISK_THRESHOLD}%)
            </span>
          }
          size="small"
        >
          <Alert
            type="warning"
            showIcon
            message={`${atRisk.length} student${atRisk.length !== 1 ? 's' : ''} with attendance below ${AT_RISK_THRESHOLD}% — chronic absenteeism risk`}
            style={{ marginBottom: 12 }}
          />
          <Table
            dataSource={atRisk}
            rowKey="studentId"
            pagination={false}
            size="small"
            columns={[
              { title: 'Student', dataIndex: 'studentName', key: 'studentName' },
              { title: 'Days', dataIndex: 'totalDays', key: 'totalDays', width: 70 },
              { title: 'Present', dataIndex: 'presentCount', key: 'presentCount', width: 80 },
              { title: 'Absent', dataIndex: 'absentCount', key: 'absentCount', width: 80 },
              {
                title: 'Rate', dataIndex: 'attendancePercentage', key: 'rate', width: 100,
                render: (v: number) => <Tag color="red">{v}%</Tag>,
              },
            ]}
          />
        </Card>
      )}
    </div>
  );
}

// ─── Main Content ──────────────────────────────────────────────
function AttendanceContent() {
  const {
    attendances,
    totalCount,
    attendanceSummaries,
    isPending,
    isError,
  } = useAttendanceState();
  const { getAllAsync, getClassSummaryAsync } = useAttendanceActions();
  const { classes } = useClassState();
  const { getActiveClassesAsync } = useClassActions();
  const { currentRole } = useAuthState();

  const [selectedClassId, setSelectedClassId] = useState<string | undefined>(undefined);
  const [dateRange, setDateRange] = useState<[Dayjs, Dayjs]>([
    dayjs().startOf('month'),
    dayjs(),
  ]);
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);
  const [activeTab, setActiveTab] = useState('records');

  useEffect(() => {
    getActiveClassesAsync();
  }, []);

  useEffect(() => {
    if ((activeTab === 'summary' || activeTab === 'analytics') && selectedClassId && dateRange) {
      getClassSummaryAsync(
        selectedClassId,
        dateRange[0].format('YYYY-MM-DD'),
        dateRange[1].format('YYYY-MM-DD'),
      );
    }
  }, [activeTab, selectedClassId, dateRange]);

  const handleQueryChange = useCallback((query: TableQuery) => {
    setLastQuery(query);
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      classId: selectedClassId,
      startDate: dateRange[0].format('YYYY-MM-DD'),
      endDate: dateRange[1].format('YYYY-MM-DD'),
    });
  }, [getAllAsync, selectedClassId, dateRange]);

  const refreshData = useCallback(() => {
    if (lastQuery) handleQueryChange(lastQuery);
  }, [lastQuery, handleQueryChange]);

  useEffect(() => {
    refreshData();
  }, [selectedClassId, dateRange]);

  const handleDateRangeChange = (dates: [Dayjs | null, Dayjs | null] | null) => {
    if (dates && dates[0] && dates[1]) {
      setDateRange([dates[0], dates[1]]);
    }
  };

  const stats = {
    total: attendances?.length ?? 0,
    present: attendances?.filter(a => a.status === 0).length ?? 0,
    absent: attendances?.filter(a => a.status === 1).length ?? 0,
    late: attendances?.filter(a => a.status === 2).length ?? 0,
    sickLeave: attendances?.filter(a => a.status === 4).length ?? 0,
  };

  const recordColumns: ColumnConfig<IAttendanceList>[] = [
    { key: 'attendanceDate', title: 'Date', dataIndex: 'attendanceDate', sortable: true, filterable: true, filterType: 'date', renderType: 'date', width: 110 },
    { key: 'studentName', title: 'Student', dataIndex: 'studentName', sortable: true, filterable: true },
    { key: 'className', title: 'Class', dataIndex: 'className', sortable: true, filterable: true },
    {
      key: 'status', title: 'Status', dataIndex: 'status',
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Present', value: 0 },
        { label: 'Absent', value: 1 },
        { label: 'Late', value: 2 },
        { label: 'Excused', value: 3 },
        { label: 'Sick Leave', value: 4 },
      ],
      renderType: 'status',
      renderConfig: { statusMap },
    },
    { key: 'notes', title: 'Notes', dataIndex: 'notes', hideOnMobile: true },
  ];

  const recordRowActions: RowAction<IAttendanceList>[] = [
    {
      key: 'view',
      label: 'View Details',
      icon: <EyeOutlined />,
      onClick: (record) => {
        console.log('View attendance detail:', record.id);
      },
    },
  ];

  const summaryColumns: ColumnConfig<IAttendanceSummary>[] = [
    { key: 'studentName', title: 'Student', dataIndex: 'studentName', sortable: true, filterable: true },
    { key: 'totalDays', title: 'Total Days', dataIndex: 'totalDays', sortable: true, width: 100 },
    { key: 'presentCount', title: 'Present', dataIndex: 'presentCount', sortable: true, width: 90 },
    { key: 'absentCount', title: 'Absent', dataIndex: 'absentCount', sortable: true, width: 90 },
    { key: 'lateCount', title: 'Late', dataIndex: 'lateCount', sortable: true, width: 80 },
    { key: 'excusedCount', title: 'Excused', dataIndex: 'excusedCount', sortable: true, hideOnMobile: true, width: 90 },
    { key: 'sickLeaveCount', title: 'Sick', dataIndex: 'sickLeaveCount', sortable: true, hideOnMobile: true, width: 80 },
    { key: 'attendancePercentage', title: 'Attendance %', dataIndex: 'attendancePercentage', sortable: true, width: 120 },
  ];

  const tabItems = [
    {
      key: 'records',
      label: 'Attendance Records',
      children: (
        <EnterpriseTable<IAttendanceList>
          title="Attendance Records"
          columns={recordColumns}
          data={attendances ?? []}
          totalCount={totalCount}
          loading={isPending}
          error={isError}
          onQueryChange={handleQueryChange}
          rowKey="id"
          rowActions={recordRowActions}
          currentUserRole={currentRole}
          exportConfig={{
            enabled: true,
            formats: ['csv', 'xlsx'],
            requiredPermissions: ['Admin', 'Principal', 'VicePrincipal'],
          }}
        />
      ),
    },
    {
      key: 'summary',
      label: 'Class Summary',
      children: selectedClassId ? (
        <EnterpriseTable<IAttendanceSummary>
          title="Class Attendance Summary"
          columns={summaryColumns}
          data={attendanceSummaries ?? []}
          totalCount={attendanceSummaries?.length}
          loading={isPending}
          error={isError}
          onQueryChange={() => {}}
          rowKey="studentId"
          currentUserRole={currentRole}
          exportConfig={{
            enabled: true,
            formats: ['csv', 'xlsx'],
            requiredPermissions: ['Admin', 'Principal', 'VicePrincipal'],
          }}
        />
      ) : (
        <Empty description="Select a class to view attendance summary" />
      ),
    },
    {
      key: 'analytics',
      label: 'Analytics',
      children: selectedClassId ? (
        <AnalyticsSection summaries={attendanceSummaries ?? []} />
      ) : (
        <Empty description="Select a class to view attendance analytics" />
      ),
    },
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      {/* Filters */}
      <Card size="small">
        <Row gutter={16} align="middle">
          <Col xs={24} sm={12} md={8}>
            <label style={{ display: 'block', fontSize: 13, marginBottom: 4, fontWeight: 500 }}>Date Range</label>
            <RangePicker
              value={dateRange}
              onChange={handleDateRangeChange}
              style={{ width: '100%' }}
              allowClear={false}
            />
          </Col>
          <Col xs={24} sm={12} md={8}>
            <label style={{ display: 'block', fontSize: 13, marginBottom: 4, fontWeight: 500 }}>Class</label>
            <Select
              placeholder="All Classes"
              value={selectedClassId}
              onChange={setSelectedClassId}
              allowClear
              style={{ width: '100%' }}
              options={
                classes?.map(c => ({
                  value: c.id,
                  label: `${c.className} (${c.gradeName})`,
                })) ?? []
              }
            />
          </Col>
        </Row>
      </Card>

      {/* Summary Stats */}
      <Row gutter={16}>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Present"
              value={stats.present}
              valueStyle={{ color: '#52c41a' }}
              prefix={<CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Absent"
              value={stats.absent}
              valueStyle={{ color: '#ff4d4f' }}
              prefix={<CloseCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Late"
              value={stats.late}
              valueStyle={{ color: '#faad14' }}
              prefix={<ClockCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Sick Leave"
              value={stats.sickLeave}
              valueStyle={{ color: '#722ED1' }}
              prefix={<MedicineBoxOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* Tabs: Records / Class Summary / Analytics */}
      <Card size="small" styles={{ body: { padding: 0 } }}>
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={tabItems}
          style={{ padding: '0 16px' }}
        />
      </Card>
    </div>
  );
}

export default function AttendancePageContent() {
  return (
    <ClassProvider>
      <AttendanceProvider>
        <AttendanceContent />
      </AttendanceProvider>
    </ClassProvider>
  );
}
