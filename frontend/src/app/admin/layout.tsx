'use client';

import React from 'react';
import {
  DashboardOutlined,
  UserOutlined,
  SafetyCertificateOutlined,
  SettingOutlined,
} from '@ant-design/icons';
import LayoutShell from '@/components/shared/LayoutShell';

const menuItems = [
  { key: '/admin', icon: <DashboardOutlined />, label: 'Dashboard' },
  { key: '/admin/users', icon: <UserOutlined />, label: 'Users' },
  { key: '/admin/roles', icon: <SafetyCertificateOutlined />, label: 'Roles' },
  { key: '/admin/settings', icon: <SettingOutlined />, label: 'School Settings' },
];

export default function AdminRootLayout({ children }: { children: React.ReactNode }) {
  return (
    <LayoutShell
      config={{
        basePath: '/admin',
        sidebarTitle: 'Admin Module',
        headerTitle: 'School Administration',
        menuItems,
        allowedRoles: ['Admin'],
      }}
    >
      {children}
    </LayoutShell>
  );
}
