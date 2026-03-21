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

const tabItems = [
  { key: '/admin/workflow', label: 'Dashboard', icon: <DashboardOutlined /> },
  { key: '/admin/workflow/definitions', label: 'Definitions', icon: <ApartmentOutlined /> },
  { key: '/admin/workflow/instances', label: 'Instances', icon: <PlayCircleOutlined /> },
  { key: '/admin/workflow/delegations', label: 'Delegations', icon: <SwapOutlined /> },
];

export default function WorkflowLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();

  // Match active tab — exact for dashboard, startsWith for sub-pages
  const activeKey = tabItems.find(
    (item) => item.key !== '/admin/workflow' && pathname.startsWith(item.key)
  )?.key ?? '/admin/workflow';

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
