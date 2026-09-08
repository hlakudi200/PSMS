"use client";

import React, { useCallback } from "react";
import { useRouter } from "next/navigation";
import { usePortalBase } from '@/utils/portal-base';
import { Tabs } from "antd";
import { EyeOutlined } from "@ant-design/icons";
import { EnterpriseTable } from "@/components/shared/enterprise-table";
import type {
  ColumnConfig,
  TableQuery,
  RowAction,
} from "@/components/shared/enterprise-table";
import {
  ExtramuralActivityProvider,
  useExtramuralActivityState,
  useExtramuralActivityActions,
} from "@/providers/saspecific/extramural_activities";
import {
  StudentExtramuralProvider,
  useStudentExtramuralState,
  useStudentExtramuralActions,
} from "@/providers/saspecific/student_extramurals";
import { useAuthState } from "@/providers/auth";
import type {
  IExtramuralActivityList,
  IStudentExtramuralList,
} from "@/providers/saspecific/shared/interfaces";
import dayjs from "dayjs";

const categoryMap: Record<number, { label: string; color: string }> = {
  1: { label: "Sport", color: "blue" },
  2: { label: "Cultural", color: "purple" },
  3: { label: "Academic", color: "green" },
  4: { label: "Social", color: "orange" },
  5: { label: "Other", color: "default" },
};

const activityTypeMap: Record<number, { label: string; color: string }> = {
  1: { label: "Individual", color: "cyan" },
  2: { label: "Team", color: "geekblue" },
};

const dayOfWeekMap: Record<number, string> = {
  0: "Sunday",
  1: "Monday",
  2: "Tuesday",
  3: "Wednesday",
  4: "Thursday",
  5: "Friday",
  6: "Saturday",
};

const enrollmentStatusMap: Record<number, { label: string; color: string }> = {
  1: { label: "Active", color: "green" },
  2: { label: "Suspended", color: "orange" },
  3: { label: "Terminated", color: "red" },
  4: { label: "Pending", color: "blue" },
};

// ─── Activities Tab ─────────────────────────────────────────────
function ActivitiesTab() {
  const router = useRouter();
  const portalBase = usePortalBase();
  const { extramuralActivities, totalCount, isPending, isError } =
    useExtramuralActivityState();
  const { getAllAsync } = useExtramuralActivityActions();
  const { currentRole } = useAuthState();

  const handleQueryChange = useCallback(
    (query: TableQuery) => {
      const { keyword, ...columnFilters } = query.filters ?? {};
      getAllAsync({
        maxResultCount: query.maxResultCount,
        skipCount: query.skipCount,
        sorting: query.sorting,
        activityName: keyword as string | undefined,
        category: columnFilters.category as number | undefined,
        activityType: columnFilters.activityType as number | undefined,
        isActive: columnFilters.isActive as boolean | undefined,
      });
    },
    [getAllAsync],
  );

  const columns: ColumnConfig<IExtramuralActivityList>[] = [
    {
      key: "activityName",
      title: "Activity Name",
      dataIndex: "activityName",
      sortable: true,
    },
    {
      key: "category",
      title: "Category",
      dataIndex: "category",
      width: 110,
      filterable: true,
      filterType: "enum",
      filterOptions: [
        { label: "Sport", value: 1 },
        { label: "Cultural", value: 2 },
        { label: "Academic", value: 3 },
        { label: "Social", value: 4 },
        { label: "Other", value: 5 },
      ],
      renderType: "status",
      renderConfig: { statusMap: categoryMap },
    },
    {
      key: "activityType",
      title: "Type",
      dataIndex: "activityType",
      width: 100,
      filterable: true,
      filterType: "enum",
      filterOptions: [
        { label: "Individual", value: 1 },
        { label: "Team", value: 2 },
      ],
      renderType: "status",
      renderConfig: { statusMap: activityTypeMap },
    },
    { key: "venue", title: "Venue", dataIndex: "venue", hideOnMobile: true },
    {
      key: "dayOfWeek",
      title: "Day",
      dataIndex: "dayOfWeek",
      width: 100,
      hideOnMobile: true,
      renderType: "custom",
      render: (value: number | undefined) =>
        value != null ? (dayOfWeekMap[value] ?? "-") : "-",
    },
    {
      key: "coachName",
      title: "Coach",
      dataIndex: "coachName",
      hideOnMobile: true,
    },
    {
      key: "feePerTerm",
      title: "Fee/Term",
      dataIndex: "feePerTerm",
      width: 100,
      sortable: true,
      renderType: "custom",
      render: (value: number) => `R ${(value ?? 0).toFixed(2)}`,
    },
    {
      key: "maxCapacity",
      title: "Capacity",
      dataIndex: "maxCapacity",
      width: 90,
      hideOnMobile: true,
      renderType: "custom",
      render: (value: number | undefined) => value ?? "N/A",
    },
    {
      key: "enrollmentCount",
      title: "Enrolled",
      dataIndex: "enrollmentCount",
      width: 85,
      sortable: true,
    },
    {
      key: "isRegistrationOpen",
      title: "Registration",
      dataIndex: "isRegistrationOpen",
      width: 115,
      renderType: "status",
      renderConfig: {
        statusMap: {
          true: { label: "Open", color: "green" },
          false: { label: "Closed", color: "red" },
        },
      },
    },
    {
      key: "isActive",
      title: "Status",
      dataIndex: "isActive",
      width: 90,
      filterable: true,
      filterType: "enum",
      filterOptions: [
        { label: "Active", value: true },
        { label: "Inactive", value: false },
      ],
      renderType: "status",
      renderConfig: {
        statusMap: {
          true: { label: "Active", color: "green" },
          false: { label: "Inactive", color: "default" },
        },
      },
    },
  ];

  const rowActions: RowAction<IExtramuralActivityList>[] = [
    {
      key: "viewStudents",
      label: "View Students",
      icon: <EyeOutlined />,
      onClick: (record) => {
        router.push(`${portalBase}/extramurals/${record.id}`);
      },
    },
  ];

  return (
    <EnterpriseTable<IExtramuralActivityList>
      title="Extramural Activities"
      columns={columns}
      data={extramuralActivities ?? []}
      totalCount={totalCount}
      loading={isPending}
      error={isError}
      onQueryChange={handleQueryChange}
      rowKey="id"
      rowActions={rowActions}
      currentUserRole={currentRole}
      exportConfig={{
        enabled: true,
        formats: ["csv", "xlsx"],
        requiredPermissions: ["Admin", "Principal", "VicePrincipal"],
      }}
    />
  );
}

// ─── Student Enrollments Tab ────────────────────────────────────
function StudentEnrollmentsTab() {
  const { studentExtramurals, totalCount, isPending, isError } =
    useStudentExtramuralState();
  const { getAllAsync } = useStudentExtramuralActions();
  const { currentRole } = useAuthState();

  const handleQueryChange = useCallback(
    (query: TableQuery) => {
      const { keyword, ...columnFilters } = query.filters ?? {};
      getAllAsync({
        maxResultCount: query.maxResultCount,
        skipCount: query.skipCount,
        sorting: query.sorting,
        studentName: keyword as string | undefined,
        ...columnFilters,
      });
    },
    [getAllAsync],
  );

  const columns: ColumnConfig<IStudentExtramuralList>[] = [
    {
      key: "studentName",
      title: "Student",
      dataIndex: "studentName",
      sortable: true,
    },
    {
      key: "studentAdmissionNumber",
      title: "Admission #",
      dataIndex: "studentAdmissionNumber",
      sortable: true,
      width: 120,
    },
    {
      key: "activityName",
      title: "Activity",
      dataIndex: "activityName",
      sortable: true,
    },
    {
      key: "academicYearName",
      title: "Year",
      dataIndex: "academicYearName",
      sortable: true,
      hideOnMobile: true,
    },
    {
      key: "teamAssignment",
      title: "Team",
      dataIndex: "teamAssignment",
      hideOnMobile: true,
    },
    {
      key: "consentFormSigned",
      title: "Consent",
      dataIndex: "consentFormSigned",
      width: 90,
      renderType: "status",
      renderConfig: {
        statusMap: {
          true: { label: "Yes", color: "green" },
          false: { label: "No", color: "red" },
        },
      },
    },
    {
      key: "status",
      title: "Status",
      dataIndex: "status",
      width: 110,
      renderType: "status",
      renderConfig: { statusMap: enrollmentStatusMap },
    },
    {
      key: "startDate",
      title: "Start Date",
      dataIndex: "startDate",
      sortable: true,
      width: 110,
      renderType: "custom",
      render: (value: string) =>
        value ? dayjs(value).format("DD MMM YYYY") : "-",
    },
  ];

  return (
    <EnterpriseTable<IStudentExtramuralList>
      title="Student Enrollments"
      columns={columns}
      data={studentExtramurals ?? []}
      totalCount={totalCount}
      loading={isPending}
      error={isError}
      onQueryChange={handleQueryChange}
      rowKey="id"
      currentUserRole={currentRole}
      exportConfig={{
        enabled: true,
        formats: ["csv", "xlsx"],
        requiredPermissions: ["Admin", "Principal", "VicePrincipal"],
      }}
    />
  );
}

// ─── Main Page ──────────────────────────────────────────────────
function ExtramuralsContent() {
  const tabItems = [
    {
      key: "activities",
      label: "Activities",
      children: <ActivitiesTab />,
    },
    {
      key: "enrollments",
      label: "Student Enrollments",
      children: <StudentEnrollmentsTab />,
    },
  ];

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
      <Tabs defaultActiveKey="activities" items={tabItems} />
    </div>
  );
}

export default function ExtramuralsPageContent() {
  return (
    <ExtramuralActivityProvider>
      <StudentExtramuralProvider>
        <ExtramuralsContent />
      </StudentExtramuralProvider>
    </ExtramuralActivityProvider>
  );
}
