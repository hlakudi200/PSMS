'use client';

import React, { useEffect, useMemo, useState } from 'react';
import { Layout, Menu, Button, Typography, Badge, message } from 'antd';
import type { ItemType, MenuItemGroupType, MenuItemType } from 'antd/es/menu/interface';
import {
  LogoutOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  NotificationOutlined,
} from '@ant-design/icons';
import { usePathname, useRouter } from 'next/navigation';
import { ProtectedRoute } from '@/components/shared/ProtectedRoute';
import { useAuthActions, useAuthState } from '@/providers/auth';

const { Header, Sider, Content } = Layout;
const { Text } = Typography;

export interface LayoutShellConfig {
  basePath: string;
  sidebarTitle: string;
  sidebarSubtitle?: string;
  headerTitle: string;
  menuItems: ItemType[];
  allowedRoles: string[];
  accentColor?: string;
  showProfileFooter?: boolean;
  showNotificationBadge?: boolean;
  showRoleInHeader?: boolean;
}

// Recursively collect every menu item key. Type-safe walk that handles
// flat items, divider/null entries, and group children of arbitrary depth.
function collectMenuKeys(items: ItemType[]): string[] {
  const keys: string[] = [];
  const visit = (item: ItemType | undefined): void => {
    if (!item || typeof item !== 'object') return;
    const maybeKey = (item as MenuItemType).key;
    if (typeof maybeKey === 'string') keys.push(maybeKey);
    const children = (item as MenuItemGroupType).children;
    if (Array.isArray(children)) {
      children.forEach((child) => visit(child as ItemType));
    }
  };
  items.forEach(visit);
  return keys;
}

// Pick the longest menu key that is a prefix of the current pathname.
// Avoids mis-highlighting overlapping routes (e.g. /students vs /students-archive).
function pickSelectedKey(allKeys: string[], basePath: string, pathname: string): string {
  let best = basePath;
  let bestLen = -1;
  for (const k of allKeys) {
    if (k === basePath) continue;
    if (!k.startsWith(basePath)) continue;
    if (!pathname.startsWith(k)) continue;
    if (k.length > bestLen) {
      best = k;
      bestLen = k.length;
    }
  }
  // Fall through to basePath when the user is on the root of the section
  if (bestLen < 0 && pathname.startsWith(basePath)) return basePath;
  return best;
}

function ShellLayout({
  config,
  children,
}: {
  config: LayoutShellConfig;
  children: React.ReactNode;
}) {
  const {
    basePath,
    sidebarTitle,
    sidebarSubtitle,
    headerTitle,
    menuItems,
    accentColor = '#003D73',
    showProfileFooter = false,
    showNotificationBadge = false,
    showRoleInHeader = true,
  } = config;

  const collapseStorageKey = `layoutShell:collapsed:${basePath}`;

  const [collapsed, setCollapsed] = useState(false);
  const pathname = usePathname();
  const router = useRouter();
  const { signOut } = useAuthActions();
  const { currentUser, currentRole } = useAuthState();

  // Restore preference on mount (browser-only)
  useEffect(() => {
    if (typeof window === 'undefined') return;
    const stored = window.localStorage.getItem(collapseStorageKey);
    if (stored === 'true') setCollapsed(true);
    else if (stored === 'false') setCollapsed(false);
  }, [collapseStorageKey]);

  const setCollapsedPersist = (next: boolean) => {
    setCollapsed(next);
    if (typeof window !== 'undefined') {
      window.localStorage.setItem(collapseStorageKey, String(next));
    }
  };

  // Memoize the menu walk so auth state changes (which re-render the shell)
  // do not retraverse the entire menu tree on every render.
  const allKeys = useMemo(() => collectMenuKeys(menuItems), [menuItems]);
  const selectedKey = useMemo(
    () => pickSelectedKey(allKeys, basePath, pathname ?? basePath),
    [allKeys, basePath, pathname]
  );

  const handleMenuClick = ({ key }: { key: string }) => {
    if (key.startsWith(basePath)) {
      router.push(key);
    } else {
      message.info('This section is coming soon.');
    }
  };

  const initials =
    [currentUser?.name?.[0], currentUser?.surname?.[0]]
      .filter(Boolean)
      .join('')
      .toUpperCase() || '?';

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        collapsible
        collapsed={collapsed}
        onCollapse={setCollapsedPersist}
        width={220}
        trigger={null}
        style={{
          background: '#F0F0F0',
          borderRight: '1px solid #D9D9D9',
          display: 'flex',
          flexDirection: 'column',
        }}
      >
        {/* Sidebar header */}
        <div
          style={{
            height: 56,
            display: 'flex',
            alignItems: 'center',
            justifyContent: collapsed ? 'center' : 'flex-start',
            padding: collapsed ? 0 : '0 16px',
            borderBottom: '1px solid #D9D9D9',
            flexShrink: 0,
          }}
        >
          {!collapsed && (
            <div>
              <Text strong style={{ fontSize: sidebarSubtitle ? 13 : 14, color: accentColor, display: 'block' }}>
                {sidebarTitle}
              </Text>
              {sidebarSubtitle && (
                <Text style={{ fontSize: 11, color: '#595959' }}>{sidebarSubtitle}</Text>
              )}
            </div>
          )}
        </div>

        {/* Scrollable menu area */}
        <div style={{ flex: 1, overflowY: 'auto', overflowX: 'hidden' }}>
          <Menu
            mode="inline"
            selectedKeys={[selectedKey]}
            items={menuItems}
            onClick={handleMenuClick}
            style={{ background: 'transparent', borderRight: 0 }}
          />
        </div>

        {/* Profile footer */}
        {showProfileFooter && (
          <div
            style={{
              padding: collapsed ? '12px 0' : '12px 16px',
              borderTop: '1px solid #D9D9D9',
              display: 'flex',
              alignItems: 'center',
              gap: 10,
              justifyContent: collapsed ? 'center' : 'flex-start',
              flexShrink: 0,
            }}
          >
            <div
              style={{
                width: 32,
                height: 32,
                borderRadius: '50%',
                background: accentColor,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#FFFFFF',
                fontSize: 12,
                fontWeight: 700,
                flexShrink: 0,
              }}
              aria-hidden="true"
            >
              {initials}
            </div>
            {!collapsed && (
              <div style={{ minWidth: 0 }}>
                <Text
                  strong
                  style={{ fontSize: 12, color: '#1F1F1F', display: 'block', lineHeight: '16px' }}
                  ellipsis
                >
                  {currentUser?.name} {currentUser?.surname}
                </Text>
                <Text style={{ fontSize: 11, color: '#595959', lineHeight: '15px' }}>
                  {currentRole}
                </Text>
              </div>
            )}
          </div>
        )}
      </Sider>

      <Layout>
        <Header
          style={{
            background: '#003D73',
            padding: '0 24px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            height: 56,
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
            <Button
              type="text"
              icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
              onClick={() => setCollapsedPersist(!collapsed)}
              aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
              style={{ color: '#FFFFFF', fontSize: 16 }}
            />
            <Text style={{ color: '#FFFFFF', fontSize: 18, fontWeight: 600 }}>
              {headerTitle}
            </Text>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
            {showNotificationBadge && (
              <Badge dot offset={[-2, 2]}>
                <Button
                  type="text"
                  icon={<NotificationOutlined />}
                  aria-label="Notifications"
                  style={{ color: '#BAE7FF', fontSize: 16 }}
                />
              </Badge>
            )}
            <Text style={{ color: '#BAE7FF', fontSize: 13 }}>
              {currentUser?.name} {currentUser?.surname}
              {showRoleInHeader && ` (${currentRole})`}
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

export default function LayoutShell({
  config,
  children,
}: {
  config: LayoutShellConfig;
  children: React.ReactNode;
}) {
  return (
    <ProtectedRoute allowedRoles={config.allowedRoles}>
      <ShellLayout config={config}>{children}</ShellLayout>
    </ProtectedRoute>
  );
}
