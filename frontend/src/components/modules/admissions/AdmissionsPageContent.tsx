'use client';

import React, { useState, useCallback, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { usePortalBase } from '@/utils/portal-base';
import { Card, Col, Row, Select, Statistic, Tabs, Tag, message } from 'antd';
import {
  EyeOutlined,
  FileTextOutlined,
  TeamOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined,
  UserAddOutlined,
} from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction } from '@/components/shared/enterprise-table';
import { ApplicationProvider, useApplicationState, useApplicationActions } from '@/providers/admissions/applications';
import { AdmissionSettingsProvider, useAdmissionSettingsState, useAdmissionSettingsActions } from '@/providers/admissions/admission_settings';
import { WaitlistProvider, useWaitlistState, useWaitlistActions } from '@/providers/admissions/waitlists';
import { AcademicYearProvider, useAcademicYearState, useAcademicYearActions } from '@/providers/academic/academic_years';
import { GradeProvider, useGradeState, useGradeActions } from '@/providers/academic/grades';
import { useAuthState } from '@/providers/auth';
import type { IApplicationList } from '@/providers/admissions/shared/interfaces';
import type { IAdmissionSettings } from '@/providers/admissions/shared/interfaces';
import type { IWaitlist } from '@/providers/admissions/shared/interfaces';

const applicationStatusMap: Record<string, { label: string; color: string }> = {
  Draft: { label: 'Draft', color: 'default' },
  Submitted: { label: 'Submitted', color: 'blue' },
  UnderReview: { label: 'Under Review', color: 'orange' },
  UnderConsideration: { label: 'Considering', color: 'purple' },
  Approved: { label: 'Approved', color: 'green' },
  Rejected: { label: 'Rejected', color: 'red' },
  Waitlisted: { label: 'Waitlisted', color: 'gold' },
  OfferAccepted: { label: 'Offer Accepted', color: 'cyan' },
  Enrolled: { label: 'Enrolled', color: 'green' },
  Withdrawn: { label: 'Withdrawn', color: 'default' },
  Expired: { label: 'Expired', color: 'default' },
};

const waitlistStatusMap: Record<string, { label: string; color: string }> = {
  Waiting: { label: 'Waiting', color: 'orange' },
  Offered: { label: 'Offered', color: 'blue' },
  Accepted: { label: 'Accepted', color: 'green' },
  Declined: { label: 'Declined', color: 'red' },
  Withdrawn: { label: 'Withdrawn', color: 'default' },
  Expired: { label: 'Expired', color: 'default' },
};

function AdmissionsContent() {
  const router = useRouter();
  const portalBase = usePortalBase();
  const { applications, totalCount, statistics, isPending, isError } = useApplicationState();
  const { getAllAsync, getStatisticsAsync } = useApplicationActions();
  const { admissionSettingsList, isPending: settingsPending } = useAdmissionSettingsState();
  const { getAllByAcademicYearAsync } = useAdmissionSettingsActions();
  const { waitlistEntries, totalCount: waitlistTotal, isPending: waitlistPending, isError: waitlistError } = useWaitlistState();
  const { getAllAsync: getAllWaitlist } = useWaitlistActions();
  const { academicYears } = useAcademicYearState();
  const { getAllAsync: getAllAcademicYears } = useAcademicYearActions();
  const { activeGrades } = useGradeState();
  const { getActiveGradesAsync } = useGradeActions();
  const { currentRole } = useAuthState();

  const [selectedAcademicYearId, setSelectedAcademicYearId] = useState<string | undefined>(undefined);
  const [selectedGradeId, setSelectedGradeId] = useState<string | undefined>(undefined);
  const [selectedStatus, setSelectedStatus] = useState<string | undefined>(undefined);
  const [activeTab, setActiveTab] = useState('applications');
  const [lastAppQuery, setLastAppQuery] = useState<TableQuery | null>(null);
  const [lastWaitlistQuery, setLastWaitlistQuery] = useState<TableQuery | null>(null);

  // Load dropdowns
  useEffect(() => {
    getAllAcademicYears({ maxResultCount: 50 });
    getActiveGradesAsync();
  }, []);

  // Load settings & statistics when academic year changes
  useEffect(() => {
    if (selectedAcademicYearId) {
      getAllByAcademicYearAsync(selectedAcademicYearId);
      getStatisticsAsync(selectedAcademicYearId, selectedGradeId);
    }
  }, [selectedAcademicYearId, selectedGradeId]);

  const handleAppQueryChange = useCallback((query: TableQuery) => {
    setLastAppQuery(query);
    const { keyword, ...columnFilters } = query.filters ?? {};
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      academicYearId: selectedAcademicYearId,
      gradeId: selectedGradeId,
      status: selectedStatus,
      applicantName: keyword as string | undefined,
      ...columnFilters,
    });
  }, [getAllAsync, selectedAcademicYearId, selectedGradeId, selectedStatus]);

  const handleWaitlistQueryChange = useCallback((query: TableQuery) => {
    setLastWaitlistQuery(query);
    getAllWaitlist({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
    });
  }, [getAllWaitlist]);

  // Re-fetch applications when filters change
  useEffect(() => {
    if (lastAppQuery) handleAppQueryChange(lastAppQuery);
  }, [selectedAcademicYearId, selectedGradeId, selectedStatus]);

  // --- Applications Tab ---
  const appColumns: ColumnConfig<IApplicationList>[] = [
    { key: 'applicationNumber', title: 'App #', dataIndex: 'applicationNumber', sortable: true, width: 100 },
    { key: 'fullName', title: 'Applicant', dataIndex: 'fullName', sortable: true },
    { key: 'gradeName', title: 'Grade', dataIndex: 'gradeName', sortable: true },
    { key: 'academicYearName', title: 'Year', dataIndex: 'academicYearName', sortable: true, hideOnMobile: true },
    { key: 'submittedDate', title: 'Submitted', dataIndex: 'submittedDate', sortable: true, renderType: 'date', width: 110 },
    {
      key: 'isFeePaid', title: 'Fee', dataIndex: 'isFeePaid', width: 70,
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Paid', value: 'true' },
        { label: 'No', value: 'false' },
      ],
      renderType: 'status',
      renderConfig: {
        statusMap: {
          true: { label: 'Paid', color: 'green' },
          false: { label: 'No', color: 'default' },
        },
      },
    },
    { key: 'documentCount', title: 'Docs', dataIndex: 'documentCount', hideOnMobile: true, width: 60 },
    {
      key: 'statusDisplayName', title: 'Status', dataIndex: 'statusDisplayName', sortable: true,
      filterable: true, filterType: 'enum', filterKey: 'status',
      filterOptions: Object.entries(applicationStatusMap).map(([key, val]) => ({ label: val.label, value: key })),
      renderType: 'status',
      renderConfig: { statusMap: applicationStatusMap },
    },
  ];

  const appRowActions: RowAction<IApplicationList>[] = [
    {
      key: 'view',
      label: 'View Application',
      icon: <EyeOutlined />,
      onClick: (record) => {
        router.push(`${portalBase}/admissions/${record.id}`);
      },
    },
  ];

  // --- Settings Tab ---
  const settingsColumns: ColumnConfig<IAdmissionSettings>[] = [
    { key: 'gradeName', title: 'Grade', dataIndex: 'gradeName', sortable: true, filterable: true },
    { key: 'academicYearName', title: 'Year', dataIndex: 'academicYearName', sortable: true, filterable: true },
    { key: 'applicationOpenDate', title: 'Open Date', dataIndex: 'applicationOpenDate', renderType: 'date', width: 110 },
    { key: 'applicationCloseDate', title: 'Close Date', dataIndex: 'applicationCloseDate', renderType: 'date', width: 110 },
    { key: 'maxCapacity', title: 'Capacity', dataIndex: 'maxCapacity', width: 90 },
    { key: 'currentEnrolledCount', title: 'Enrolled', dataIndex: 'currentEnrolledCount', width: 90 },
    { key: 'availableSpots', title: 'Available', dataIndex: 'availableSpots', width: 90 },
    { key: 'applicationFeeDisplay', title: 'App Fee', dataIndex: 'applicationFeeDisplay', hideOnMobile: true, width: 100 },
    {
      key: 'isAcceptingApplications', title: 'Accepting', dataIndex: 'isAcceptingApplications',
      filterable: true, filterType: 'enum',
      filterOptions: [
        { label: 'Open', value: 'true' },
        { label: 'Closed', value: 'false' },
      ],
      renderType: 'status',
      renderConfig: {
        statusMap: {
          true: { label: 'Open', color: 'green' },
          false: { label: 'Closed', color: 'red' },
        },
      },
    },
  ];

  // --- Waitlist Tab ---
  const waitlistColumns: ColumnConfig<IWaitlist>[] = [
    { key: 'position', title: '#', dataIndex: 'position', sortable: true, width: 50 },
    { key: 'applicationNumber', title: 'App #', dataIndex: 'applicationNumber', sortable: true, width: 100 },
    { key: 'applicantName', title: 'Applicant', dataIndex: 'applicantName', sortable: true, filterable: true },
    { key: 'gradeName', title: 'Grade', dataIndex: 'gradeName', sortable: true, filterable: true },
    { key: 'addedDate', title: 'Added', dataIndex: 'addedDate', sortable: true, renderType: 'date', width: 110 },
    { key: 'offerExpiryDate', title: 'Offer Expires', dataIndex: 'offerExpiryDate', renderType: 'date', hideOnMobile: true, width: 110 },
    {
      key: 'statusDisplayName', title: 'Status', dataIndex: 'statusDisplayName',
      filterable: true, filterType: 'enum',
      filterOptions: Object.entries(waitlistStatusMap).map(([key, val]) => ({ label: val.label, value: key })),
      renderType: 'status',
      renderConfig: { statusMap: waitlistStatusMap },
    },
  ];

  // No detail page exists for waitlist entries yet; row actions are intentionally
  // empty until that route is scaffolded.
  const waitlistRowActions: RowAction<IWaitlist>[] = [];

  const tabItems = [
    {
      key: 'applications',
      label: (
        <span><FileTextOutlined style={{ marginRight: 6 }} />Applications</span>
      ),
      children: (
        <EnterpriseTable<IApplicationList>
          title="Applications"
          columns={appColumns}
          data={applications ?? []}
          totalCount={totalCount}
          loading={isPending}
          error={isError}
          onQueryChange={handleAppQueryChange}
          rowKey="id"
          rowActions={appRowActions}
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
      key: 'settings',
      label: (
        <span><TeamOutlined style={{ marginRight: 6 }} />Settings</span>
      ),
      children: (
        <EnterpriseTable<IAdmissionSettings>
          title="Admission Settings"
          columns={settingsColumns}
          data={admissionSettingsList ?? []}
          totalCount={admissionSettingsList?.length}
          loading={settingsPending}
          onQueryChange={() => {}}
          rowKey="id"
          currentUserRole={currentRole}
        />
      ),
    },
    {
      key: 'waitlist',
      label: (
        <span><ClockCircleOutlined style={{ marginRight: 6 }} />Waitlist</span>
      ),
      children: (
        <EnterpriseTable<IWaitlist>
          title="Waitlist"
          columns={waitlistColumns}
          data={waitlistEntries ?? []}
          totalCount={waitlistTotal}
          loading={waitlistPending}
          error={waitlistError}
          onQueryChange={handleWaitlistQueryChange}
          rowKey="id"
          rowActions={waitlistRowActions}
          currentUserRole={currentRole}
          exportConfig={{
            enabled: true,
            formats: ['csv', 'xlsx'],
            requiredPermissions: ['Admin', 'Principal'],
          }}
        />
      ),
    },
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      {/* Filters */}
      <Card size="small">
        <Row gutter={16} align="middle">
          <Col xs={24} sm={8} md={6}>
            <label style={{ display: 'block', fontSize: 13, marginBottom: 4, fontWeight: 500 }}>Academic Year</label>
            <Select
              placeholder="All Years"
              value={selectedAcademicYearId}
              onChange={(value) => {
                setSelectedAcademicYearId(value);
                setSelectedGradeId(undefined);
              }}
              allowClear
              style={{ width: '100%' }}
              options={academicYears?.map(y => ({ value: y.id, label: y.yearName })) ?? []}
            />
          </Col>
          <Col xs={24} sm={8} md={6}>
            <label style={{ display: 'block', fontSize: 13, marginBottom: 4, fontWeight: 500 }}>Grade</label>
            <Select
              placeholder="All Grades"
              value={selectedGradeId}
              onChange={setSelectedGradeId}
              allowClear
              style={{ width: '100%' }}
              options={activeGrades?.map(g => ({ value: g.id, label: g.gradeName })) ?? []}
            />
          </Col>
          <Col xs={24} sm={8} md={6}>
            <label style={{ display: 'block', fontSize: 13, marginBottom: 4, fontWeight: 500 }}>Status</label>
            <Select
              placeholder="All Statuses"
              value={selectedStatus}
              onChange={setSelectedStatus}
              allowClear
              style={{ width: '100%' }}
              options={Object.entries(applicationStatusMap).map(([key, val]) => ({ value: key, label: val.label }))}
            />
          </Col>
        </Row>
      </Card>

      {/* Statistics */}
      {statistics && (
        <Row gutter={16}>
          <Col xs={12} sm={6}>
            <Card size="small">
              <Statistic title="Total" value={statistics.totalApplications} prefix={<FileTextOutlined />} />
            </Card>
          </Col>
          <Col xs={12} sm={6}>
            <Card size="small">
              <Statistic title="Approved" value={statistics.approvedApplications} valueStyle={{ color: '#52c41a' }} prefix={<CheckCircleOutlined />} />
            </Card>
          </Col>
          <Col xs={12} sm={6}>
            <Card size="small">
              <Statistic title="Enrolled" value={statistics.enrolledApplications} valueStyle={{ color: '#0066CC' }} prefix={<UserAddOutlined />} />
            </Card>
          </Col>
          <Col xs={12} sm={6}>
            <Card size="small">
              <Statistic title="Waitlisted" value={statistics.waitlistedApplications} valueStyle={{ color: '#faad14' }} prefix={<ClockCircleOutlined />} />
            </Card>
          </Col>
        </Row>
      )}

      {/* Tabs */}
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

export default function AdmissionsPageContent() {
  return (
    <AcademicYearProvider>
      <GradeProvider>
        <ApplicationProvider>
          <AdmissionSettingsProvider>
            <WaitlistProvider>
              <AdmissionsContent />
            </WaitlistProvider>
          </AdmissionSettingsProvider>
        </ApplicationProvider>
      </GradeProvider>
    </AcademicYearProvider>
  );
}
