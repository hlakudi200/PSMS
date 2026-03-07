'use client';

import React from 'react';
import {
  DashboardOutlined,
  CalendarOutlined,
  BookOutlined,
  ReadOutlined,
  TeamOutlined,
  ApartmentOutlined,
  UserOutlined,
  CheckSquareOutlined,
  UsergroupAddOutlined,
  BarChartOutlined,
  FileTextOutlined,
  NotificationOutlined,
  MessageOutlined,
  ClockCircleOutlined,
  CarOutlined,
  TrophyOutlined,
  DollarOutlined,
  FormOutlined,
} from '@ant-design/icons';
import LayoutShell from '@/components/shared/LayoutShell';
import { roleColors } from '@/utils/theme-config';

const menuItems = [
  {
    key: '/principal',
    icon: <DashboardOutlined />,
    label: 'Dashboard',
  },
  {
    type: 'group' as const,
    label: 'Academic',
    children: [
      { key: '/principal/academic-years', icon: <CalendarOutlined />, label: 'Academic Years' },
      { key: '/principal/grades', icon: <BookOutlined />, label: 'Grades' },
      { key: '/principal/subjects', icon: <ReadOutlined />, label: 'Subjects' },
      { key: '/principal/classes', icon: <ApartmentOutlined />, label: 'Classes' },
      { key: '/principal/teachers', icon: <TeamOutlined />, label: 'Teachers' },
    ],
  },
  {
    type: 'group' as const,
    label: 'Students',
    children: [
      { key: '/principal/students', icon: <UserOutlined />, label: 'All Students' },
      { key: '/principal/attendance', icon: <CheckSquareOutlined />, label: 'Attendance' },
      { key: 'parents', icon: <UsergroupAddOutlined />, label: 'Parents', disabled: true },
    ],
  },
  {
    type: 'group' as const,
    label: 'Assessments',
    children: [
      { key: 'reports', icon: <BarChartOutlined />, label: 'Reports', disabled: true },
      { key: 'mark-sheets', icon: <FileTextOutlined />, label: 'Mark Sheets', disabled: true },
    ],
  },
  {
    type: 'group' as const,
    label: 'Communication',
    children: [
      { key: 'announcements', icon: <NotificationOutlined />, label: 'Announcements', disabled: true },
      { key: 'messages', icon: <MessageOutlined />, label: 'Messages', disabled: true },
    ],
  },
  {
    type: 'group' as const,
    label: 'More',
    children: [
      { key: 'timetables', icon: <ClockCircleOutlined />, label: 'Timetable', disabled: true },
      { key: 'transport', icon: <CarOutlined />, label: 'Transport', disabled: true },
      { key: 'extramurals', icon: <TrophyOutlined />, label: 'Extramurals', disabled: true },
      { key: 'finance', icon: <DollarOutlined />, label: 'Finance Overview', disabled: true },
      { key: 'admissions', icon: <FormOutlined />, label: 'Admissions', disabled: true },
    ],
  },
];

export default function PrincipalRootLayout({ children }: { children: React.ReactNode }) {
  return (
    <LayoutShell
      config={{
        basePath: '/principal',
        sidebarTitle: 'School Management',
        sidebarSubtitle: 'Principal Portal',
        headerTitle: 'School Overview',
        menuItems,
        allowedRoles: ['Principal'],
        accentColor: roleColors.Principal,
        showProfileFooter: true,
        showNotificationBadge: true,
        showRoleInHeader: false,
      }}
    >
      {children}
    </LayoutShell>
  );
}
