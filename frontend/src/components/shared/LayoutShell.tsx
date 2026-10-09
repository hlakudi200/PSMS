'use client';

import React, { useEffect, useMemo, useState } from 'react';
import { Layout, Menu, Button, Typography, message } from 'antd';
import type { ItemType, MenuItemGroupType, MenuItemType } from 'antd/es/menu/interface';
import {
  LogoutOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
} from '@ant-design/icons';
import { usePathname, useRouter } from 'next/navigation';
import { ProtectedRoute } from '@/components/shared/ProtectedRoute';
import { useAuthActions, useAuthState } from '@/providers/auth';
import { useBrandingState } from '@/providers/branding';
import { getReadableForeground } from '@/utils/theme-config';
import NotificationBell from '@/components/shared/NotificationBell';

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

/**
 * A section's key. Prefixed so it can never collide with a route key, and so
 * `pickSelectedKey` — which matches keys against the pathname — cannot pick one.
 */
const sectionKey = (label: string) => `section:${label}`;

/**
 * Turns the flat `type: 'group'` headings into sections that open and close.
 *
 * AntD renders a group as a label with its items always beneath it, so a portal
 * with nine headings was nine headings' worth of scrolling with no way to put
 * any of it away. A submenu is the same shape of data and collapses.
 *
 * Items that are not groups — the Dashboard at the top, dividers — pass
 * through untouched.
 */
function asCollapsibleSections(items: ItemType[]): ItemType[] {
  return items.map((item) => {
    if (!item || typeof item !== 'object') return item;
    if ((item as MenuItemGroupType).type !== 'group') return item;

    const group = item as MenuItemGroupType;
    const label = typeof group.label === 'string' ? group.label : '';

    return {
      key: sectionKey(label),
      label: group.label,
      children: group.children,
    } as ItemType;
  });
}

/** The section a route key sits in, so the right one is open on arrival. */
function sectionContaining(items: ItemType[], key: string): string | undefined {
  for (const item of items) {
    if (!item || typeof item !== 'object') continue;
    if ((item as MenuItemGroupType).type !== 'group') continue;

    const group = item as MenuItemGroupType;
    const children = group.children ?? [];
    const has = children.some(
      (child) => child && typeof child === 'object' && (child as MenuItemType).key === key
    );

    if (has) return sectionKey(typeof group.label === 'string' ? group.label : '');
  }
  return undefined;
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
    showNotificationBadge = true,
    showRoleInHeader = true,
  } = config;

  const collapseStorageKey = `layoutShell:collapsed:${basePath}`;
  const openSectionsStorageKey = `layoutShell:openSections:${basePath}`;

  const [collapsed, setCollapsed] = useState(false);
  const pathname = usePathname();
  const router = useRouter();
  const { signOut } = useAuthActions();
  const { currentUser, currentRole } = useAuthState();

  // Issue #56. Note `accentColor` above is deliberately left alone — it is the
  // per-ROLE identity colour (roleColors.Principal etc.), not the school's.
  // The school's brand lands on the header chrome and the sidebar logo.
  const { branding } = useBrandingState();

  // The header background is an arbitrary tenant colour, so its foreground has
  // to be derived — a school picking a pale secondary would otherwise get
  // white-on-white across every portal.
  const headerForeground = getReadableForeground(branding.secondaryColor);

  // A configured subtitle wins; otherwise show the school's name so every
  // portal identifies the school it belongs to.
  const resolvedSubtitle = sidebarSubtitle ?? branding.schoolName;

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

  const sectionedItems = useMemo(() => asCollapsibleSections(menuItems), [menuItems]);
  const currentSection = useMemo(
    () => sectionContaining(menuItems, selectedKey),
    [menuItems, selectedKey]
  );

  /* Which sections are open. Remembered per portal, because a principal who
     works in Assessments all day should not have to reopen it every morning.
     Null means "not restored yet" — until then the section holding the current
     page is the one open, so arriving anywhere shows you where you are. */
  const [openSections, setOpenSections] = useState<string[] | null>(null);

  useEffect(() => {
    if (typeof window === 'undefined') return;
    const stored = window.localStorage.getItem(openSectionsStorageKey);
    if (stored === null) return;
    try {
      const parsed = JSON.parse(stored);
      if (Array.isArray(parsed)) setOpenSections(parsed.filter((k) => typeof k === 'string'));
    } catch {
      /* A corrupt entry is not worth failing the page over. */
    }
  }, [openSectionsStorageKey]);

  /* Navigating into a closed section opens it, so the page you are on is never
     hidden behind a heading. */
  useEffect(() => {
    if (!currentSection) return;
    setOpenSections((previous) => {
      const base = previous ?? [];
      return base.includes(currentSection) ? previous : [...base, currentSection];
    });
  }, [currentSection]);

  const onOpenChange = (keys: string[]) => {
    setOpenSections(keys);
    if (typeof window !== 'undefined') {
      window.localStorage.setItem(openSectionsStorageKey, JSON.stringify(keys));
    }
  };

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
            gap: 10,
            justifyContent: collapsed ? 'center' : 'flex-start',
            padding: collapsed ? 0 : '0 16px',
            borderBottom: '1px solid #D9D9D9',
            flexShrink: 0,
          }}
        >
          {/* School logo (issue #56). Collapsed, the logo is all that fits —
              so it stays visible and the text is what drops. */}
          {branding.logoUrl && (
            /* eslint-disable-next-line @next/next/no-img-element */
            <img
              src={branding.logoUrl}
              alt={`${branding.schoolName} logo`}
              style={{
                height: 32,
                maxWidth: collapsed ? 40 : 72,
                objectFit: 'contain',
                flexShrink: 0,
              }}
            />
          )}
          {!collapsed && (
            <div style={{ minWidth: 0 }}>
              <Text strong style={{ fontSize: 13, color: accentColor, display: 'block' }} ellipsis>
                {sidebarTitle}
              </Text>
              {resolvedSubtitle && (
                <Text style={{ fontSize: 11, color: '#595959' }} ellipsis>
                  {resolvedSubtitle}
                </Text>
              )}
            </div>
          )}
        </div>

        {/* Scrollable menu area */}
        <div style={{ flex: 1, overflowY: 'auto', overflowX: 'hidden' }}>
          <Menu
            mode="inline"
            selectedKeys={[selectedKey]}
            /* Narrowed to icons there is no room for a section title, and a
               submenu with no icon would be an empty square. The original
               groups still read correctly in that state, so they are what is
               shown. */
            items={collapsed ? menuItems : sectionedItems}
            openKeys={collapsed ? undefined : openSections ?? (currentSection ? [currentSection] : [])}
            onOpenChange={onOpenChange}
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
            background: branding.secondaryColor,
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
              style={{ color: headerForeground, fontSize: 16 }}
            />
            <Text style={{ color: headerForeground, fontSize: 18, fontWeight: 600 }}>
              {headerTitle}
            </Text>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
            {showNotificationBadge && <NotificationBell />}
            {/* Derived from the header colour, slightly muted — the header
                behind it is an arbitrary tenant colour. */}
            <Text style={{ color: headerForeground, opacity: 0.85, fontSize: 13 }}>
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
