'use client';

import React, { useState, useCallback, useEffect } from 'react';
import { Card, Col, Row, Select, DatePicker, Statistic, Tabs, Empty } from 'antd';
import {
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined,
  MedicineBoxOutlined,
  EyeOutlined,
} from '@ant-design/icons';
import dayjs, { Dayjs } from 'dayjs';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction } from '@/components/shared/enterprise-table';
import { AttendanceProvider, useAttendanceState, useAttendanceActions } from '@/providers/academic/attendances';
import { ClassProvider, useClassState, useClassActions } from '@/providers/academic/classes';
import { useAuthState } from '@/providers/auth';
import type { IAttendanceList, IAttendanceSummary } from '@/providers/academic/shared/interfaces';

const { RangePicker } = DatePicker;

const statusMap: Record<number, { label: string; color: string }> = {
  0: { label: 'Present', color: 'green' },
  1: { label: 'Absent', color: 'red' },
  2: { label: 'Late', color: 'orange' },
  3: { label: 'Excused', color: 'blue' },
  4: { label: 'Sick Leave', color: 'purple' },
};

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

  // Fetch class summary when filters or tab change
  useEffect(() => {
    if (activeTab === 'summary' && selectedClassId && dateRange) {
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

  // Re-fetch when filters change
  useEffect(() => {
    refreshData();
  }, [selectedClassId, dateRange]);

  const handleDateRangeChange = (dates: [Dayjs | null, Dayjs | null] | null) => {
    if (dates && dates[0] && dates[1]) {
      setDateRange([dates[0], dates[1]]);
    }
  };

  // Compute summary stats from current records
  const stats = {
    total: attendances?.length ?? 0,
    present: attendances?.filter(a => a.status === 0).length ?? 0,
    absent: attendances?.filter(a => a.status === 1).length ?? 0,
    late: attendances?.filter(a => a.status === 2).length ?? 0,
    sickLeave: attendances?.filter(a => a.status === 4).length ?? 0,
  };

  // --- Records Tab ---
  const recordColumns: ColumnConfig<IAttendanceList>[] = [
    { key: 'attendanceDate', title: 'Date', dataIndex: 'attendanceDate', sortable: true, renderType: 'date', width: 110 },
    { key: 'studentName', title: 'Student', dataIndex: 'studentName', sortable: true, filterable: true },
    { key: 'className', title: 'Class', dataIndex: 'className', sortable: true, filterable: true },
    {
      key: 'status', title: 'Status', dataIndex: 'status',
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

  // --- Summary Tab ---
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

      {/* Tabs: Records / Class Summary */}
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
