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
  WarningOutlined,
  SwapOutlined,
  EnvironmentOutlined,
  ShoppingOutlined,
  PartitionOutlined,
  VideoCameraOutlined,
  FolderOpenOutlined,
  FileProtectOutlined,
  HomeOutlined,
  SafetyCertificateOutlined,
} from '@ant-design/icons';
import LayoutShell from '@/components/shared/LayoutShell';
import { roleColors } from '@/utils/theme-config';
import { useAuthState } from '@/providers/auth';

const buildMenuItems = (isVicePrincipal: boolean) => [
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
    label: 'Learning',
    children: [
      { key: '/principal/lessons', icon: <VideoCameraOutlined />, label: 'Online Lessons' },
      { key: '/principal/materials', icon: <FolderOpenOutlined />, label: 'Learning Materials' },
    ],
  },
  {
    type: 'group' as const,
    label: 'Communication',
    children: [
      { key: '/principal/announcements', icon: <NotificationOutlined />, label: 'Announcements' },
      { key: '/principal/messages', icon: <MessageOutlined />, label: 'Messages' },
      { key: '/principal/documents', icon: <FileProtectOutlined />, label: 'Documents' },
      { key: '/principal/communication', icon: <BarChartOutlined />, label: 'Delivery Analytics' },
    ],
  },
  {
    type: 'group' as const,
    label: 'More',
    children: [
      { key: '/principal/timetables', icon: <ClockCircleOutlined />, label: 'Timetable' },
      { key: '/principal/transport', icon: <CarOutlined />, label: 'Transport' },
      { key: '/principal/extramurals', icon: <TrophyOutlined />, label: 'Extramurals' },
      { key: '/principal/after-care', icon: <HomeOutlined />, label: 'After Care' },
      { key: '/principal/finance', icon: <DollarOutlined />, label: 'Finance Overview' },
      { key: '/principal/admissions', icon: <FormOutlined />, label: 'Admissions' },
    ],
  },
  {
    type: 'group' as const,
    label: 'Operations',
    children: [
      { key: '/principal/workflow', icon: <PartitionOutlined />, label: 'Workflows' },
      { key: '/principal/disciplinary', icon: <WarningOutlined />, label: 'Disciplinary' },
      { key: '/principal/student-transfers', icon: <SwapOutlined />, label: 'Transfers' },
      { key: '/principal/staff-leave', icon: <CalendarOutlined />, label: 'Staff Leave' },
      { key: '/principal/field-trips', icon: <EnvironmentOutlined />, label: 'Field Trips' },
      { key: '/principal/fee-waivers', icon: <DollarOutlined />, label: 'Fee Waivers' },
      { key: '/principal/expenses', icon: <ShoppingOutlined />, label: 'Expenses' },
    ],
  },
  {
    type: 'group' as const,
    label: 'Administration',
    children: [
      { key: '/principal/users', icon: <UserOutlined />, label: 'Users' },
      // RoleAppService is gated on Pages.Roles, which the Vice Principal does not hold.
      ...(isVicePrincipal
        ? []
        : [{ key: '/principal/roles', icon: <SafetyCertificateOutlined />, label: 'Roles' }]),
    ],
  },
];

/**
 * The management portal. The Vice Principal shares it: the VP holds the same
 * operational permissions as the Principal (approval steps are routed to
 * either) minus a few administration grants, and previously landed in the
 * five-page academic portal with no route to approvals, admissions or finance.
 */
export default function PrincipalRootLayout({ children }: { children: React.ReactNode }) {
  const { currentRole } = useAuthState();
  const isVicePrincipal = currentRole?.toLowerCase() === 'viceprincipal';
  const menuItems = React.useMemo(() => buildMenuItems(isVicePrincipal), [isVicePrincipal]);

  return (
    <LayoutShell
      config={{
        basePath: '/principal',
        sidebarTitle: 'School Management',
        sidebarSubtitle: isVicePrincipal ? 'Vice Principal Portal' : 'Principal Portal',
        headerTitle: 'School Overview',
        menuItems,
        allowedRoles: ['Principal', 'VicePrincipal'],
        accentColor: isVicePrincipal ? roleColors.VicePrincipal : roleColors.Principal,
        showProfileFooter: true,
        showNotificationBadge: true,
        showRoleInHeader: false,
      }}
    >
      {children}
    </LayoutShell>
  );
}
