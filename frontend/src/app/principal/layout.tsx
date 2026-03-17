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
      { key: '/principal/parents', icon: <UsergroupAddOutlined />, label: 'Parents' },
    ],
  },
  {
    type: 'group' as const,
    label: 'Assessments',
    children: [
      { key: '/principal/reports', icon: <BarChartOutlined />, label: 'Reports' },
      { key: '/principal/mark-sheets', icon: <FileTextOutlined />, label: 'Mark Sheets' },
    ],
  },
  {
    type: 'group' as const,
    label: 'Communication',
    children: [
      { key: '/principal/announcements', icon: <NotificationOutlined />, label: 'Announcements' },
      { key: '/principal/messages', icon: <MessageOutlined />, label: 'Messages' },
    ],
  },
  {
    type: 'group' as const,
    label: 'More',
    children: [
      { key: '/principal/timetables', icon: <ClockCircleOutlined />, label: 'Timetable' },
      { key: '/principal/transport', icon: <CarOutlined />, label: 'Transport' },
      { key: '/principal/extramurals', icon: <TrophyOutlined />, label: 'Extramurals' },
      { key: '/principal/finance', icon: <DollarOutlined />, label: 'Finance Overview' },
      { key: '/principal/admissions', icon: <FormOutlined />, label: 'Admissions' },
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
