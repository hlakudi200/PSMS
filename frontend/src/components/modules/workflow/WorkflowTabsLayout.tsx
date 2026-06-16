'use client';

import React from 'react';
import { Tabs } from 'antd';
import {
  DashboardOutlined,
  ApartmentOutlined,
  PlayCircleOutlined,
  SwapOutlined,
} from '@ant-design/icons';
import { useRouter, usePathname } from 'next/navigation';
import { useWorkflowBasePath } from './useWorkflowBasePath';

/**
 * Shared workflow sub-navigation (Dashboard / Definitions / Instances /
 * Delegations). Base-path aware so the same tabs work under any role portal
 * that mounts the workflow module (e.g. /admin/workflow, /principal/workflow).
 */
export default function WorkflowTabsLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const base = useWorkflowBasePath();

  const tabItems = [
    { key: base, label: 'Dashboard', icon: <DashboardOutlined /> },
    { key: `${base}/definitions`, label: 'Definitions', icon: <ApartmentOutlined /> },
    { key: `${base}/instances`, label: 'Instances', icon: <PlayCircleOutlined /> },
    { key: `${base}/delegations`, label: 'Delegations', icon: <SwapOutlined /> },
  ];

  // Match active tab — exact for dashboard, startsWith for sub-pages
  const activeKey =
    tabItems.find((item) => item.key !== base && pathname.startsWith(item.key))?.key ?? base;

  return (
    <div>
      <Tabs
        activeKey={activeKey}
        onChange={(key) => router.push(key)}
        items={tabItems.map((item) => ({
          key: item.key,
          label: (
            <span>
              {item.icon}
              <span style={{ marginLeft: 8 }}>{item.label}</span>
            </span>
          ),
        }))}
        style={{ marginBottom: 0, paddingLeft: 16, paddingRight: 16 }}
      />
      {children}
    </div>
  );
}
