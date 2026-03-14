'use client';

import React, { useEffect, useCallback, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import {
  Card,
  Descriptions,
  Tag,
  Table,
  Space,
  Button,
  Typography,
  Spin,
  Empty,
  Statistic,
  Row,
  Col,
  Alert,
  Input,
  message,
  Popconfirm,
} from 'antd';
import {
  ArrowLeftOutlined,
  CheckCircleOutlined,
  SendOutlined,
  PrinterOutlined,
  MessageOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { ReportProvider, useReportState, useReportActions } from '@/providers/assessment/reports';
import type { IReport, IReportSubject } from '@/providers/assessment/shared/interfaces';

const { Title, Text, Paragraph } = Typography;

const statusMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Draft', color: 'default' },
  2: { label: 'Generated', color: 'blue' },
  3: { label: 'Pending Approval', color: 'orange' },
  4: { label: 'Approved', color: 'green' },
  5: { label: 'Published', color: 'purple' },
};

const reportTypeMap: Record<number, string> = {
  1: 'Term 1 Report',
  2: 'Term 2 Report',
  3: 'Term 3 Report',
  4: 'Term 4 Report',
  5: 'Mid-Year Report',
  6: 'Year-End Report',
  7: 'Progress Report',
};

const achievementLabels: Record<number, { symbol: string; desc: string }> = {
  1: { symbol: '1', desc: 'Not Achieved' },
  2: { symbol: '2', desc: 'Elementary' },
  3: { symbol: '3', desc: 'Moderate' },
  4: { symbol: '4', desc: 'Adequate' },
  5: { symbol: '5', desc: 'Substantial' },
  6: { symbol: '6', desc: 'Meritorious' },
  7: { symbol: '7', desc: 'Outstanding' },
};

function getAchievementTag(level?: number) {
  if (!level) return <Text type="secondary">-</Text>;
  const info = achievementLabels[level];
  if (!info) return <Tag>{level}</Tag>;
  const colors: Record<number, string> = { 1: 'red', 2: 'orange', 3: 'gold', 4: 'lime', 5: 'green', 6: 'blue', 7: 'purple' };
  return <Tag color={colors[level] ?? 'default'}>{info.symbol} - {info.desc}</Tag>;
}

function ReportDetailContent() {
  const params = useParams();
  const router = useRouter();
  const reportId = params.id as string;

  const { report, isPending, isError } = useReportState();
  const { getAsync, approveAsync, publishAsync, addPrincipalCommentAsync } = useReportActions();

  const [principalComment, setPrincipalComment] = useState('');
  const [commentSaving, setCommentSaving] = useState(false);

  useEffect(() => {
    if (reportId) getAsync(reportId);
  }, [reportId, getAsync]);

  useEffect(() => {
    if (report?.principalComment) setPrincipalComment(report.principalComment);
  }, [report?.principalComment]);

  const refresh = useCallback(() => {
    getAsync(reportId);
  }, [reportId, getAsync]);

  const handleApprove = async () => {
    await approveAsync(reportId);
    message.success('Report approved');
    refresh();
  };

  const handlePublish = async () => {
    await publishAsync(reportId);
    message.success('Report published');
    refresh();
  };

  const handleSaveComment = async () => {
    if (!principalComment.trim()) return;
    setCommentSaving(true);
    try {
      await addPrincipalCommentAsync(reportId, { comment: principalComment.trim() });
      message.success('Comment saved');
      refresh();
    } catch {
      message.error('Failed to save comment');
    } finally {
      setCommentSaving(false);
    }
  };

  const handlePrint = () => {
    window.print();
  };

  if (isPending && !report) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 80 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (isError || (!isPending && !report)) {
    return (
      <div style={{ padding: 24 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => router.push('/principal/reports')} style={{ marginBottom: 16 }}>
          Back to Reports
        </Button>
        <Empty description="Report not found" />
      </div>
    );
  }

  if (!report) return null;

  const status = statusMap[report.status] ?? { label: 'Unknown', color: 'default' };
  const subjects = report.subjectReports ?? [];

  const subjectColumns = [
    { title: 'Subject', dataIndex: 'subjectName', key: 'subjectName', render: (v: string) => <Text strong>{v}</Text> },
    { title: 'Code', dataIndex: 'subjectCode', key: 'subjectCode', width: 80 },
    {
      title: 'Term Mark', dataIndex: 'termMark', key: 'termMark', width: 100,
      render: (v: number | undefined) => v != null ? `${v}%` : '-',
    },
    {
      title: 'Exam Mark', dataIndex: 'examMark', key: 'examMark', width: 100,
      render: (v: number | undefined) => v != null ? `${v}%` : '-',
    },
    {
      title: 'Final Mark', dataIndex: 'finalMark', key: 'finalMark', width: 100,
      render: (v: number | undefined) => v != null ? <Text strong>{v}%</Text> : '-',
    },
    {
      title: 'Level', dataIndex: 'achievementLevel', key: 'achievementLevel', width: 160,
      render: (v: number | undefined) => getAchievementTag(v),
    },
    { title: 'Teacher', dataIndex: 'teacherName', key: 'teacherName' },
    {
      title: 'Comment', dataIndex: 'teacherComment', key: 'teacherComment',
      render: (v: string | undefined) => v || <Text type="secondary">-</Text>,
      ellipsis: true,
    },
  ];

  return (
    <div style={{ padding: 24 }}>
      <style>{`@media print { .no-print { display: none !important; } }`}</style>

      <Space className="no-print" style={{ marginBottom: 16 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => router.push('/principal/reports')}>
          Back to Reports
        </Button>
        <Button icon={<PrinterOutlined />} onClick={handlePrint}>
          Print Report Card
        </Button>
      </Space>

      {/* Status Banner */}
      {report.status === 3 && (
        <Alert
          className="no-print"
          type="warning"
          showIcon
          style={{ marginBottom: 16 }}
          message="This report is awaiting your approval."
          action={
            <Popconfirm title="Approve this report?" onConfirm={handleApprove}>
              <Button type="primary" size="small" icon={<CheckCircleOutlined />}>Approve</Button>
            </Popconfirm>
          }
        />
      )}
      {report.status === 4 && (
        <Alert
          className="no-print"
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
          message="This report is approved and ready to publish."
          action={
            <Popconfirm title="Publish this report? Parents will be able to view it." onConfirm={handlePublish}>
              <Button type="primary" size="small" icon={<SendOutlined />}>Publish</Button>
            </Popconfirm>
          }
        />
      )}

      {/* Report Header */}
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Title level={3} style={{ margin: 0 }}>{report.studentName}</Title>
            <Space size="large" style={{ marginTop: 8 }}>
              <Text type="secondary">Adm #: {report.studentAdmissionNumber ?? 'N/A'}</Text>
              <Text type="secondary">Class: {report.className ?? 'N/A'}</Text>
              <Text type="secondary">{report.termName ?? ''} - {report.academicYearName ?? ''}</Text>
              <Tag color="blue">{reportTypeMap[report.reportType] ?? 'Report'}</Tag>
              <Tag color={status.color}>{status.label}</Tag>
            </Space>
          </Col>
          {report.generatedDate && (
            <Col>
              <Text type="secondary">Generated: {dayjs(report.generatedDate).format('DD MMM YYYY')}</Text>
            </Col>
          )}
        </Row>
      </Card>

      {/* Summary Stats */}
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Overall"
              value={report.overallPercentage ?? 0}
              suffix="%"
              valueStyle={{ color: (report.overallPercentage ?? 0) >= 50 ? '#3f8600' : '#cf1322' }}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Class Position"
              value={report.classPosition ?? '-'}
              suffix={report.totalStudentsInClass ? ` / ${report.totalStudentsInClass}` : ''}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic title="Subjects" value={subjects.length} />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic title="Achievement" valueRender={() => getAchievementTag(report.overallAchievementLevel)} />
          </Card>
        </Col>
      </Row>

      {/* Attendance Row */}
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col xs={8}><Card size="small"><Statistic title="Days Present" value={report.daysPresent} valueStyle={{ color: '#3f8600' }} /></Card></Col>
        <Col xs={8}><Card size="small"><Statistic title="Days Absent" value={report.daysAbsent} valueStyle={{ color: report.daysAbsent > 0 ? '#cf1322' : undefined }} /></Card></Col>
        <Col xs={8}><Card size="small"><Statistic title="Days Late" value={report.daysLate} valueStyle={{ color: report.daysLate > 0 ? '#faad14' : undefined }} /></Card></Col>
      </Row>

      {/* Subject Breakdown */}
      <Card title="Subject Breakdown" style={{ marginBottom: 16 }}>
        <Table<IReportSubject>
          dataSource={subjects}
          columns={subjectColumns}
          rowKey="id"
          pagination={false}
          size="small"
          locale={{ emptyText: <Empty description="No subject data" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
        />
      </Card>

      {/* Comments Section */}
      <Card title="Comments" style={{ marginBottom: 16 }}>
        {report.teacherComment && (
          <div style={{ marginBottom: 16 }}>
            <Text strong>Class Teacher Comment</Text>
            <Paragraph style={{ marginTop: 4, padding: '8px 12px', background: '#f6f8fa', borderRadius: 4 }}>
              {report.teacherComment}
            </Paragraph>
          </div>
        )}

        <div className="no-print" style={{ marginBottom: 16 }}>
          <Text strong>Principal Comment</Text>
          <Input.TextArea
            value={principalComment}
            onChange={(e) => setPrincipalComment(e.target.value)}
            rows={3}
            placeholder="Add your comment..."
            maxLength={1000}
            style={{ marginTop: 4 }}
          />
          <Button
            type="primary"
            size="small"
            icon={<MessageOutlined />}
            onClick={handleSaveComment}
            loading={commentSaving}
            disabled={!principalComment.trim()}
            style={{ marginTop: 8 }}
          >
            Save Comment
          </Button>
        </div>

        {report.principalComment && (
          <div className="print-only" style={{ marginBottom: 16 }}>
            <Text strong>Principal Comment</Text>
            <Paragraph style={{ marginTop: 4, padding: '8px 12px', background: '#f6f8fa', borderRadius: 4 }}>
              {report.principalComment}
            </Paragraph>
          </div>
        )}

        {report.parentComment && (
          <div>
            <Text strong>Parent Comment</Text>
            <Paragraph style={{ marginTop: 4, padding: '8px 12px', background: '#f6f8fa', borderRadius: 4 }}>
              {report.parentComment}
            </Paragraph>
          </div>
        )}
      </Card>

      {/* Approval Info */}
      {report.approvedDate && (
        <Card size="small">
          <Descriptions size="small">
            <Descriptions.Item label="Approved Date">{dayjs(report.approvedDate).format('DD MMM YYYY')}</Descriptions.Item>
            {report.publishedDate && <Descriptions.Item label="Published Date">{dayjs(report.publishedDate).format('DD MMM YYYY')}</Descriptions.Item>}
            {report.parentAcknowledgedDate && <Descriptions.Item label="Parent Acknowledged">{dayjs(report.parentAcknowledgedDate).format('DD MMM YYYY')}</Descriptions.Item>}
          </Descriptions>
        </Card>
      )}
    </div>
  );
}

export default function ReportDetailPage() {
  return (
    <ReportProvider>
      <ReportDetailContent />
    </ReportProvider>
  );
}
