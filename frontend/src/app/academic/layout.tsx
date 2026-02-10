'use client';

import React, { useState } from 'react';
import { Layout, Menu, Button, Typography } from 'antd';
import {
  DashboardOutlined,
  CalendarOutlined,
  BookOutlined,
  ReadOutlined,
  TeamOutlined,
  ApartmentOutlined,
  LogoutOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
} from '@ant-design/icons';
import { usePathname, useRouter } from 'next/navigation';
import { ProtectedRoute } from '@/components/auth/ProtectedRoute';
import { useAuthActions, useAuthState } from '@/providers/auth';

const { Header, Sider, Content } = Layout;
const { Text } = Typography;

const menuItems = [
  { key: '/academic', icon: <DashboardOutlined />, label: 'Dashboard' },
  { key: '/academic/academic-years', icon: <CalendarOutlined />, label: 'Academic Years' },
  { key: '/academic/grades', icon: <BookOutlined />, label: 'Grades' },
  { key: '/academic/subjects', icon: <ReadOutlined />, label: 'Subjects' },
  { key: '/academic/teachers', icon: <TeamOutlined />, label: 'Teachers' },
  { key: '/academic/classes', icon: <ApartmentOutlined />, label: 'Classes' },
];

function AcademicLayout({ children }: { children: React.ReactNode }) {
  const [collapsed, setCollapsed] = useState(false);
  const pathname = usePathname();
  const router = useRouter();
  const { signOut } = useAuthActions();
  const { currentUser, currentRole } = useAuthState();

  const selectedKey = menuItems
    .filter(item => item.key !== '/academic')
    .find(item => pathname.startsWith(item.key))?.key ?? '/academic';

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        collapsible
        collapsed={collapsed}
        onCollapse={setCollapsed}
        width={220}
        style={{ background: '#F0F0F0', borderRight: '1px solid #D9D9D9' }}
        trigger={null}
      >
        <div style={{
          height: 56,
          display: 'flex',
          alignItems: 'center',
          justifyContent: collapsed ? 'center' : 'flex-start',
          padding: collapsed ? 0 : '0 16px',
          borderBottom: '1px solid #D9D9D9',
        }}>
          {!collapsed && (
            <Text strong style={{ fontSize: 14, color: '#003D73' }}>Academic Module</Text>
          )}
        </div>
        <Menu
          mode="inline"
          selectedKeys={[selectedKey]}
          items={menuItems}
          onClick={({ key }) => router.push(key)}
          style={{ background: 'transparent', borderRight: 0 }}
        />
      </Sider>
      <Layout>
        <Header style={{
          background: '#003D73',
          padding: '0 24px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          height: 56,
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
            <Button
              type="text"
              icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
              onClick={() => setCollapsed(!collapsed)}
              style={{ color: '#FFFFFF', fontSize: 16 }}
            />
            <Text style={{ color: '#FFFFFF', fontSize: 18, fontWeight: 600 }}>
              Academic Management
            </Text>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
            <Text style={{ color: '#BAE7FF', fontSize: 13 }}>
              {currentUser?.name} {currentUser?.surname} ({currentRole})
            </Text>
            <Button
              type="primary"
              danger
              size="small"
              icon={<LogoutOutlined />}
              onClick={signOut}
            >
              Logout
            </Button>
          </div>
        </Header>
        <Content style={{ padding: 24, background: '#F5F5F5' }}>
          {children}
        </Content>
      </Layout>
    </Layout>
  );
}

export default function AcademicRootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <ProtectedRoute allowedRoles={['Admin', 'Principal', 'VicePrincipal', 'HOD']}>
      <AcademicLayout>{children}</AcademicLayout>
    </ProtectedRoute>
  );
}
