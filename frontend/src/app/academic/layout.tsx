'use client';

import React from 'react';
import {
  DashboardOutlined,
  CalendarOutlined,
  BookOutlined,
  ReadOutlined,
  TeamOutlined,
  ApartmentOutlined,
} from '@ant-design/icons';
import LayoutShell from '@/components/shared/LayoutShell';

const menuItems = [
  { key: '/academic', icon: <DashboardOutlined />, label: 'Dashboard' },
  { key: '/academic/academic-years', icon: <CalendarOutlined />, label: 'Academic Years' },
  { key: '/academic/grades', icon: <BookOutlined />, label: 'Grades' },
  { key: '/academic/subjects', icon: <ReadOutlined />, label: 'Subjects' },
  { key: '/academic/teachers', icon: <TeamOutlined />, label: 'Teachers' },
  { key: '/academic/classes', icon: <ApartmentOutlined />, label: 'Classes' },
];

export default function AcademicRootLayout({ children }: { children: React.ReactNode }) {
  return (
    <LayoutShell
      config={{
        basePath: '/academic',
        sidebarTitle: 'Academic Module',
        headerTitle: 'Academic Management',
        menuItems,
        allowedRoles: ['Admin', 'Principal', 'VicePrincipal', 'HOD'],
      }}
    >
      {children}
    </LayoutShell>
  );
}
