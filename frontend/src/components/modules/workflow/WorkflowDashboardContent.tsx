'use client';

import React, { useEffect } from 'react';
import { Card, Col, Row, Statistic, Table, Tag, Timeline, Typography, Spin } from 'antd';
import {
  CheckCircleOutlined,
  ClockCircleOutlined,
  CloseCircleOutlined,
  ExclamationCircleOutlined,
  UserOutlined,
  RightOutlined,
} from '@ant-design/icons';
import { useRouter } from 'next/navigation';
import { useWorkflowBasePath } from './useWorkflowBasePath';
import {
  WorkflowDashboardProvider,
  useWorkflowDashboardState,
  useWorkflowDashboardActions,
} from '@/providers/workflow/workflow-dashboard';
import {
  WorkflowEntityTypeLabels,
  WorkflowActionTypeLabels,
} from '@/providers/workflow/shared/interfaces';
import type { IWorkflowEntityTypeSummary, IWorkflowRoleSummary, IWorkflowActivity } from '@/providers/workflow/shared/interfaces';

const { Title, Text } = Typography;

function DashboardContent() {
  const { dashboard, recentActivity, isPending } = useWorkflowDashboardState();
  const { getDashboardAsync, getRecentActivityAsync } = useWorkflowDashboardActions();
  const router = useRouter();
  const base = useWorkflowBasePath();

  useEffect(() => {
    getDashboardAsync();
    getRecentActivityAsync(15);
  }, []);

  if (isPending && !dashboard) {
    return <Spin size="large" style={{ display: 'block', margin: '100px auto' }} />;
  }

  const actionColors: Record<number, string> = {
    1: 'blue',    // Submit
    2: 'cyan',    // Review
    3: 'green',   // Approve
    4: 'red',     // Reject
    5: 'orange',  // Revise
    6: 'default', // Cancel
    7: 'purple',  // Recall
  };

  const entityTypeColumns = [
    {
      title: 'Entity Type',
      dataIndex: 'entityType',
      key: 'entityType',
      render: (val: number) => WorkflowEntityTypeLabels[val] ?? val,
    },
    { title: 'Active', dataIndex: 'activeCount', key: 'activeCount' },
    { title: 'Completed', dataIndex: 'completedCount', key: 'completedCount' },
    { title: 'Rejected', dataIndex: 'rejectedCount', key: 'rejectedCount' },
    {
      title: 'Overdue',
      dataIndex: 'overdueCount',
      key: 'overdueCount',
      render: (val: number) => val > 0 ? <Text type="danger">{val}</Text> : val,
    },
  ];

  const roleColumns = [
    { title: 'Role', dataIndex: 'roleName', key: 'roleName' },
    { title: 'Pending', dataIndex: 'pendingCount', key: 'pendingCount' },
    {
      title: 'Overdue',
      dataIndex: 'overdueCount',
      key: 'overdueCount',
      render: (val: number) => val > 0 ? <Text type="danger">{val}</Text> : val,
    },
  ];

  return (
    <div style={{ padding: 24 }}>
      <Title level={4} style={{ marginBottom: 24 }}>Workflow Dashboard</Title>

      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Active Workflows"
              value={dashboard?.totalActive ?? 0}
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Completed"
              value={dashboard?.totalCompleted ?? 0}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Rejected"
              value={dashboard?.totalRejected ?? 0}
              prefix={<CloseCircleOutlined />}
              valueStyle={{ color: '#ff4d4f' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Overdue"
              value={dashboard?.totalOverdue ?? 0}
              prefix={<ExclamationCircleOutlined />}
              valueStyle={{ color: dashboard?.totalOverdue ? '#ff4d4f' : undefined }}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} md={8}>
          <Card>
            <Statistic
              title="My Pending"
              value={dashboard?.myPendingCount ?? 0}
              prefix={<UserOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={8}>
          <Card>
            <Statistic
              title="Avg. Completion (hours)"
              value={dashboard?.averageCompletionHours?.toFixed(1) ?? '—'}
              precision={1}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} lg={14}>
          <Card title="By Entity Type" size="small">
            <Table<IWorkflowEntityTypeSummary>
              dataSource={dashboard?.byEntityType ?? []}
              columns={entityTypeColumns}
              rowKey="entityType"
              pagination={false}
              size="small"
            />
          </Card>
        </Col>
        <Col xs={24} lg={10}>
          <Card title="By Role" size="small">
            <Table<IWorkflowRoleSummary>
              dataSource={dashboard?.byRole ?? []}
              columns={roleColumns}
              rowKey="roleName"
              pagination={false}
              size="small"
            />
          </Card>
        </Col>
      </Row>

      <Card title="Recent Activity" size="small">
        <Timeline
          items={(recentActivity ?? []).map((activity: IWorkflowActivity) => ({
            color: actionColors[activity.action] ?? 'gray',
            children: (
              <div
                style={{ cursor: 'pointer' }}
                onClick={() => router.push(`${base}/instances/${activity.workflowInstanceId}`)}
              >
                <Tag color={actionColors[activity.action]}>
                  {WorkflowActionTypeLabels[activity.action] ?? activity.action}
                </Tag>
                <Text strong>{activity.workflowDefinitionName}</Text>
                {' — '}
                <Text type="secondary">
                  {activity.fromStepName}
                  {activity.toStepName ? ` → ${activity.toStepName}` : ''}
                </Text>
                <RightOutlined style={{ fontSize: 10, marginLeft: 8, color: '#999' }} />
                <br />
                <Text type="secondary" style={{ fontSize: 12 }}>
                  by {activity.actorUserName ?? 'System'} · {new Date(activity.transitionDate).toLocaleString()}
                </Text>
                {activity.comment && (
                  <div style={{ marginTop: 4 }}>
                    <Text italic style={{ fontSize: 12 }}>{activity.comment}</Text>
                  </div>
                )}
              </div>
            ),
          }))}
        />
      </Card>
    </div>
  );
}

export default function WorkflowDashboardContent() {
  return (
    <WorkflowDashboardProvider>
      <DashboardContent />
    </WorkflowDashboardProvider>
  );
}
