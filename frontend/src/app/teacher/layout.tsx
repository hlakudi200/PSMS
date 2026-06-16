'use client';

import React from 'react';
import {
  DashboardOutlined,
  TeamOutlined,
  ClockCircleOutlined,
  BookOutlined,
  VideoCameraOutlined,
  FormOutlined,
  CheckSquareOutlined,
  FileTextOutlined,
  MessageOutlined,
  CarryOutOutlined,
} from '@ant-design/icons';
import LayoutShell from '@/components/shared/LayoutShell';
import { roleColors } from '@/utils/theme-config';

const menuItems = [
  {
    key: '/teacher',
    icon: <DashboardOutlined />,
    label: 'Dashboard',
  },
  {
    key: '/teacher/workflow',
    icon: <CarryOutOutlined />,
    label: 'My Approvals',
  },
  {
    type: 'group' as const,
    label: 'Teaching',
    children: [
      { key: '/teacher/classes', icon: <TeamOutlined />, label: 'My Classes' },
      { key: '/teacher/schedule', icon: <ClockCircleOutlined />, label: 'My Schedule' },
    ],
  },
  {
    type: 'group' as const,
    label: 'Content',
    children: [
      { key: '/teacher/materials', icon: <BookOutlined />, label: 'Learning Materials' },
      { key: '/teacher/lessons', icon: <VideoCameraOutlined />, label: 'Online Lessons' },
      { key: '/teacher/assessments', icon: <FormOutlined />, label: 'Assessments' },
    ],
  },
  {
    type: 'group' as const,
    label: 'Records',
    children: [
      { key: '/teacher/attendance', icon: <CheckSquareOutlined />, label: 'Attendance' },
      { key: '/teacher/mark-sheets', icon: <FileTextOutlined />, label: 'Mark Sheets' },
    ],
  },
  {
    type: 'group' as const,
    label: 'Communication',
    children: [
      { key: '/teacher/messages', icon: <MessageOutlined />, label: 'Messages' },
    ],
  },
];

export default function TeacherRootLayout({ children }: { children: React.ReactNode }) {
  return (
    <LayoutShell
      config={{
        basePath: '/teacher',
        sidebarTitle: 'School Management',
        sidebarSubtitle: 'Teacher Portal',
        headerTitle: 'Teacher Portal',
        menuItems,
        allowedRoles: ['Teacher'],
        accentColor: roleColors.Teacher,
        showProfileFooter: true,
        showNotificationBadge: true,
        showRoleInHeader: false,
      }}
    >
      {children}
    </LayoutShell>
  );
}
