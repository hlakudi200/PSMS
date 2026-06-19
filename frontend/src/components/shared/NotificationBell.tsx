'use client';

import React, { useCallback, useEffect, useRef, useState } from 'react';
import { Badge, Button, Drawer, List, Tag, Typography, Empty, Spin } from 'antd';
import { BellOutlined, CheckOutlined } from '@ant-design/icons';
import { useRouter } from 'next/navigation';
import {
  NotificationProvider,
  useNotificationState,
  useNotificationActions,
} from '@/providers/communication/notifications';
import type { INotificationList } from '@/providers/communication/shared/interfaces';

const { Text } = Typography;

// COMM-03: poll the unread count on this cadence (and on window focus). Real-time
// push (SignalR) is a follow-up; polling keeps the bell near-live and reliable.
const POLL_INTERVAL_MS = 45000;

// Notification priority → accent colour (4 = Critical, 3 = High in the backend enum).
const priorityColor: Record<number, string> = { 4: '#ff4d4f', 3: '#fa8c16' };

function timeAgo(iso: string): string {
  const then = new Date(iso).getTime();
  if (Number.isNaN(then)) return '';
  const secs = Math.max(0, Math.floor((Date.now() - then) / 1000));
  if (secs < 60) return 'just now';
  const mins = Math.floor(secs / 60);
  if (mins < 60) return `${mins}m ago`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return `${hrs}h ago`;
  const days = Math.floor(hrs / 24);
  if (days < 7) return `${days}d ago`;
  return new Date(iso).toLocaleDateString();
}

function NotificationBellInner() {
  const router = useRouter();
  const { unreadCount, notifications, isPending } = useNotificationState();
  const { getUnreadCountAsync, getAllAsync, markAsReadAsync, markAllAsReadAsync } = useNotificationActions();
  const [open, setOpen] = useState(false);

  // Poll the unread count: on mount, on an interval, and whenever the tab regains
  // focus. Hold the action in a ref so the interval is created exactly once — the
  // provider recreates its action fns each render, so depending on it directly
  // would tear down and reset the timer on every poll.
  const getUnreadCountRef = useRef(getUnreadCountAsync);
  getUnreadCountRef.current = getUnreadCountAsync;

  useEffect(() => {
    const tick = () => getUnreadCountRef.current();
    tick();
    const timer = setInterval(tick, POLL_INTERVAL_MS);
    window.addEventListener('focus', tick);
    return () => {
      clearInterval(timer);
      window.removeEventListener('focus', tick);
    };
  }, []);

  const loadList = useCallback(() => {
    getAllAsync({ maxResultCount: 20, sorting: 'CreationTime DESC' });
  }, [getAllAsync]);

  const openCentre = () => {
    setOpen(true);
    loadList();
  };

  const refresh = useCallback(() => {
    getUnreadCountAsync();
    loadList();
  }, [getUnreadCountAsync, loadList]);

  const handleItemClick = async (item: INotificationList) => {
    if (!item.isRead) {
      await markAsReadAsync(item.id);
      refresh();
    }
    // Only follow in-app (relative) links — guard against an absolute/external or
    // malformed ActionUrl reaching the Next router.
    if (item.actionUrl && item.actionUrl.startsWith('/')) {
      setOpen(false);
      router.push(item.actionUrl);
    }
  };

  const handleMarkAll = async () => {
    await markAllAsReadAsync();
    refresh();
  };

  const items = notifications ?? [];

  return (
    <>
      <Badge count={unreadCount ?? 0} size="small" offset={[-2, 2]} overflowCount={99}>
        <Button
          type="text"
          icon={<BellOutlined />}
          aria-label="Notifications"
          onClick={openCentre}
          style={{ color: '#BAE7FF', fontSize: 16 }}
        />
      </Badge>

      <Drawer
        title="Notifications"
        placement="right"
        width={380}
        open={open}
        onClose={() => setOpen(false)}
        extra={
          <Button type="link" size="small" icon={<CheckOutlined />} onClick={handleMarkAll} disabled={!(unreadCount ?? 0)}>
            Mark all read
          </Button>
        }
        styles={{ body: { padding: 0 } }}
      >
        {isPending && items.length === 0 ? (
          <div style={{ display: 'flex', justifyContent: 'center', padding: 48 }}><Spin /></div>
        ) : items.length === 0 ? (
          <Empty description="No notifications" style={{ marginTop: 64 }} />
        ) : (
          <List
            dataSource={items}
            rowKey="id"
            renderItem={(item) => (
              <List.Item
                onClick={() => handleItemClick(item)}
                style={{
                  cursor: 'pointer',
                  padding: '12px 20px',
                  background: item.isRead ? undefined : '#f0f7ff',
                  borderLeft: `3px solid ${item.isRead ? 'transparent' : (priorityColor[item.priority] ?? '#1677ff')}`,
                }}
              >
                <List.Item.Meta
                  title={
                    <span style={{ display: 'flex', justifyContent: 'space-between', gap: 8 }}>
                      <Text strong={!item.isRead} ellipsis style={{ maxWidth: 230 }}>{item.title}</Text>
                      <Text type="secondary" style={{ fontSize: 11, whiteSpace: 'nowrap' }}>{timeAgo(item.creationTime)}</Text>
                    </span>
                  }
                  description={
                    <>
                      <Text type="secondary" style={{ fontSize: 13 }}>{item.message}</Text>
                      {priorityColor[item.priority] && (
                        <Tag color={priorityColor[item.priority]} style={{ marginLeft: 0, marginTop: 6, fontSize: 11 }}>
                          {item.priority === 4 ? 'Critical' : 'High'}
                        </Tag>
                      )}
                    </>
                  }
                />
              </List.Item>
            )}
          />
        )}
      </Drawer>
    </>
  );
}

/**
 * COMM-03: header notification bell + centre. Self-contained (owns its
 * NotificationProvider) so any portal shell can drop it in. Polls the unread
 * count; opening the drawer loads the recent list with mark-read / mark-all-read
 * and deep-links via the notification's ActionUrl.
 */
export default function NotificationBell() {
  return (
    <NotificationProvider>
      <NotificationBellInner />
    </NotificationProvider>
  );
}
