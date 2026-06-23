'use client';

import React, { useCallback, useEffect, useState } from 'react';
import { Card, Table, Row, Col, Statistic, Tag, Typography, Spin, Button, message } from 'antd';
import { ReloadOutlined } from '@ant-design/icons';
import { getAxiosInstance } from '@/utils/axios-instance';

const { Title, Text } = Typography;

// Mirrors backend NotificationChannel / NotificationDeliveryStatus enums.
const channelLabels: Record<number, string> = { 1: 'In-app', 2: 'Email', 3: 'SMS', 4: 'Push', 5: 'WhatsApp' };
const statusLabels: Record<number, string> = { 1: 'Pending', 2: 'Sent', 3: 'Delivered', 4: 'Read', 5: 'Failed', 6: 'Suppressed' };
const statusColors: Record<number, string> = { 1: 'default', 2: 'blue', 3: 'green', 4: 'purple', 5: 'red', 6: 'orange' };

interface ChannelStats {
  channel: number;
  pending: number; sent: number; delivered: number; read: number; failed: number; suppressed: number; total: number;
}
interface DeliveryRow {
  id: string; channel: number; recipientUserId: number; status: number; error?: string; creationTime: string;
}

export default function CommunicationAnalyticsContent() {
  const [loading, setLoading] = useState(true);
  const [byChannel, setByChannel] = useState<ChannelStats[]>([]);
  const [total, setTotal] = useState(0);
  const [recent, setRecent] = useState<DeliveryRow[]>([]);

  const load = useCallback(async () => {
    setLoading(true);
    const api = getAxiosInstance();
    // allSettled so one failing endpoint still renders the other panel.
    const [summary, deliveries] = await Promise.allSettled([
      api.get('/api/services/app/NotificationAnalytics/GetSummary'),
      api.get('/api/services/app/NotificationAnalytics/GetDeliveries?MaxResultCount=25&SkipCount=0&Sorting=CreationTime%20DESC'),
    ]);
    if (summary.status === 'fulfilled') {
      setByChannel(summary.value.data.result.byChannel ?? []);
      setTotal(summary.value.data.result.total ?? 0);
    }
    if (deliveries.status === 'fulfilled') {
      setRecent(deliveries.value.data.result.items ?? []);
    }
    if (summary.status === 'rejected' && deliveries.status === 'rejected') {
      message.error('Could not load communication analytics.');
    } else if (summary.status === 'rejected' || deliveries.status === 'rejected') {
      message.warning('Some analytics could not be loaded.');
    }
    setLoading(false);
  }, []);

  useEffect(() => { load(); }, [load]);

  const channelColumns = [
    { title: 'Channel', dataIndex: 'channel', key: 'channel', render: (c: number) => <Text strong>{channelLabels[c] ?? c}</Text> },
    { title: 'Sent', dataIndex: 'sent', key: 'sent' },
    { title: 'Delivered', dataIndex: 'delivered', key: 'delivered' },
    { title: 'Read', dataIndex: 'read', key: 'read' },
    { title: 'Failed', dataIndex: 'failed', key: 'failed', render: (v: number) => v ? <Text type="danger">{v}</Text> : 0 },
    { title: 'Suppressed', dataIndex: 'suppressed', key: 'suppressed', render: (v: number) => v ? <Text type="warning">{v}</Text> : 0 },
    { title: 'Total', dataIndex: 'total', key: 'total', render: (v: number) => <Text strong>{v}</Text> },
  ];

  const recentColumns = [
    { title: 'Channel', dataIndex: 'channel', key: 'channel', render: (c: number) => channelLabels[c] ?? c },
    { title: 'Recipient (user id)', dataIndex: 'recipientUserId', key: 'recipientUserId' },
    {
      title: 'Status', dataIndex: 'status', key: 'status',
      render: (s: number) => <Tag color={statusColors[s] ?? 'default'}>{statusLabels[s] ?? s}</Tag>,
    },
    { title: 'When', dataIndex: 'creationTime', key: 'creationTime', render: (t: string) => new Date(t).toLocaleString() },
    { title: 'Detail', dataIndex: 'error', key: 'error', render: (e?: string) => e ? <Text type="secondary" style={{ fontSize: 12 }}>{e}</Text> : '—' },
  ];

  const sum = (k: keyof ChannelStats) => byChannel.reduce((a, c) => a + (c[k] as number), 0);

  return (
    <div style={{ padding: 24 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={3} style={{ margin: 0 }}>Communication Analytics</Title>
        <Button icon={<ReloadOutlined />} onClick={load}>Refresh</Button>
      </div>

      {loading ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: 80 }}><Spin size="large" /></div>
      ) : (
        <>
          <Row gutter={16} style={{ marginBottom: 16 }}>
            <Col xs={12} sm={6}><Card size="small"><Statistic title="Total dispatched" value={total} /></Card></Col>
            <Col xs={12} sm={6}><Card size="small"><Statistic title="Delivered" value={sum('delivered')} valueStyle={{ color: '#3f8600' }} /></Card></Col>
            <Col xs={12} sm={6}><Card size="small"><Statistic title="Failed" value={sum('failed')} valueStyle={{ color: '#cf1322' }} /></Card></Col>
            <Col xs={12} sm={6}><Card size="small"><Statistic title="Suppressed" value={sum('suppressed')} valueStyle={{ color: '#d46b08' }} /></Card></Col>
          </Row>

          <Card title="By channel" size="small" style={{ marginBottom: 16 }}>
            <Table<ChannelStats>
              dataSource={byChannel}
              columns={channelColumns}
              rowKey="channel"
              pagination={false}
              size="small"
              locale={{ emptyText: 'No notifications dispatched yet.' }}
            />
            <Text type="secondary" style={{ fontSize: 12 }}>
              “Read” reflects external-channel delivery receipts. In-app read-state is tracked on the
              notification itself, so the in-app “Read” count here is not representative.
            </Text>
          </Card>

          <Card title="Recent deliveries" size="small">
            <Table<DeliveryRow>
              dataSource={recent}
              columns={recentColumns}
              rowKey="id"
              pagination={false}
              size="small"
              locale={{ emptyText: 'No delivery records yet.' }}
            />
          </Card>
        </>
      )}
    </div>
  );
}
