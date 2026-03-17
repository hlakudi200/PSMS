'use client';

import React, { useEffect, useMemo } from 'react';
import { useParams, useRouter } from 'next/navigation';
import {
  Card,
  Descriptions,
  Tag,
  Table,
  Button,
  Typography,
  Spin,
  Empty,
  Statistic,
  Row,
  Col,
} from 'antd';
import { ArrowLeftOutlined } from '@ant-design/icons';
import { AssessmentProvider, useAssessmentState, useAssessmentActions } from '@/providers/assessment/assessments';
import { MarkProvider, useMarkState, useMarkActions } from '@/providers/assessment/marks';
import type { IMarkList } from '@/providers/assessment/shared/interfaces';

const { Text } = Typography;

const assessmentTypeMap: Record<number, string> = {
  1: 'Test',
  2: 'Assignment',
  3: 'Exam',
  4: 'Practical',
  5: 'Oral',
  6: 'Project',
  7: 'Other',
};

const achievementLevelMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Not Achieved', color: 'red' },
  2: { label: 'Elementary', color: 'orange' },
  3: { label: 'Moderate', color: 'gold' },
  4: { label: 'Adequate', color: 'lime' },
  5: { label: 'Substantial', color: 'green' },
  6: { label: 'Meritorious', color: 'blue' },
  7: { label: 'Outstanding', color: 'purple' },
};

const markStatusMap: Record<number, { label: string; color: string }> = {
  1: { label: 'Pending', color: 'default' },
  2: { label: 'Completed', color: 'green' },
  3: { label: 'Absent', color: 'orange' },
  4: { label: 'Exempted', color: 'blue' },
};

function MarkSheetDetailContent() {
  const params = useParams();
  const router = useRouter();
  const assessmentId = params.id as string;

  const { assessment, isPending: assessmentPending, isError: assessmentError } = useAssessmentState();
  const { getAsync: getAssessment } = useAssessmentActions();
  const { marks, isPending: marksPending } = useMarkState();
  const { getByAssessmentAsync } = useMarkActions();

  useEffect(() => {
    if (assessmentId) {
      getAssessment(assessmentId);
      getByAssessmentAsync(assessmentId);
    }
  }, [assessmentId]);

  // Compute summary stats
  const stats = useMemo(() => {
    if (!marks || marks.length === 0) {
      return { totalStudents: 0, avgPercentage: 0, passRate: 0, highestMark: 0 };
    }

    const completed = marks.filter(m => m.status === 2 && m.rawMark != null);
    const totalStudents = marks.length;
    const avgPercentage = completed.length > 0
      ? completed.reduce((sum, m) => sum + (m.percentage ?? 0), 0) / completed.length
      : 0;
    const passPercentage = assessment?.passPercentage ?? 50;
    const passing = completed.filter(m => (m.percentage ?? 0) >= passPercentage);
    const passRate = completed.length > 0 ? (passing.length / completed.length) * 100 : 0;
    const highestMark = completed.length > 0
      ? Math.max(...completed.map(m => m.rawMark ?? 0))
      : 0;

    return { totalStudents, avgPercentage, passRate, highestMark };
  }, [marks, assessment?.passPercentage]);

  if (assessmentPending && !assessment) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 80 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (assessmentError || (!assessmentPending && !assessment)) {
    return (
      <div style={{ padding: 24 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={() => router.push('/principal/mark-sheets')} style={{ marginBottom: 16 }}>
          Back to Mark Sheets
        </Button>
        <Empty description="Assessment not found" />
      </div>
    );
  }

  if (!assessment) return null;

  const markColumns = [
    {
      title: 'Student Name', dataIndex: 'studentName', key: 'studentName',
      sorter: (a: IMarkList, b: IMarkList) => (a.studentName ?? '').localeCompare(b.studentName ?? ''),
      render: (v: string) => <Text strong>{v}</Text>,
    },
    {
      title: 'Admission #', dataIndex: 'studentAdmissionNumber', key: 'studentAdmissionNumber', width: 120,
    },
    {
      title: 'Raw Mark', dataIndex: 'rawMark', key: 'rawMark', width: 100,
      sorter: (a: IMarkList, b: IMarkList) => (a.rawMark ?? 0) - (b.rawMark ?? 0),
      render: (v: number | undefined) => v != null ? `${v} / ${assessment.maxMarks}` : '-',
    },
    {
      title: 'Percentage', dataIndex: 'percentage', key: 'percentage', width: 110,
      sorter: (a: IMarkList, b: IMarkList) => (a.percentage ?? 0) - (b.percentage ?? 0),
      render: (v: number | undefined) => v != null ? `${v.toFixed(1)}%` : '-',
    },
    {
      title: 'Achievement Level', dataIndex: 'achievementLevel', key: 'achievementLevel', width: 160,
      render: (v: number | undefined) => {
        if (!v) return <Text type="secondary">-</Text>;
        const info = achievementLevelMap[v];
        return info ? <Tag color={info.color}>{info.label}</Tag> : <Tag>{v}</Tag>;
      },
    },
    {
      title: 'Status', dataIndex: 'status', key: 'status', width: 110,
      filters: Object.entries(markStatusMap).map(([key, val]) => ({ text: val.label, value: Number(key) })),
      onFilter: (value: unknown, record: IMarkList) => record.status === value,
      render: (v: number) => {
        const info = markStatusMap[v];
        return info ? <Tag color={info.color}>{info.label}</Tag> : <Tag>{v}</Tag>;
      },
    },
    {
      title: 'Absent', dataIndex: 'wasAbsent', key: 'wasAbsent', width: 80,
      render: (v: boolean) => v ? <Tag color="orange">Yes</Tag> : <Tag color="default">No</Tag>,
    },
  ];

  return (
    <div style={{ padding: 24 }}>
      {/* Back button */}
      <Button
        icon={<ArrowLeftOutlined />}
        onClick={() => router.push('/principal/mark-sheets')}
        style={{ marginBottom: 16 }}
      >
        Back to Mark Sheets
      </Button>

      {/* Assessment Info */}
      <Card style={{ marginBottom: 16 }}>
        <Descriptions title={assessment.name} bordered size="small" column={{ xs: 1, sm: 2, md: 3 }}>
          <Descriptions.Item label="Subject">{assessment.subjectName ?? 'N/A'}</Descriptions.Item>
          <Descriptions.Item label="Class">{assessment.className ?? 'N/A'}</Descriptions.Item>
          <Descriptions.Item label="Term">{assessment.termName ?? 'N/A'}</Descriptions.Item>
          <Descriptions.Item label="Type">
            <Tag color="blue">{assessmentTypeMap[assessment.assessmentType] ?? 'Unknown'}</Tag>
          </Descriptions.Item>
          <Descriptions.Item label="Max Marks">{assessment.maxMarks}</Descriptions.Item>
          <Descriptions.Item label="Weight">{assessment.weight}%</Descriptions.Item>
          <Descriptions.Item label="Pass %">{assessment.passPercentage}%</Descriptions.Item>
          <Descriptions.Item label="Published">
            {assessment.isPublished ? <Tag color="green">Yes</Tag> : <Tag color="default">No</Tag>}
          </Descriptions.Item>
          <Descriptions.Item label="Marks Released">
            {assessment.marksReleased ? <Tag color="green">Yes</Tag> : <Tag color="default">No</Tag>}
          </Descriptions.Item>
        </Descriptions>
      </Card>

      {/* Summary Stats */}
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic title="Total Students" value={stats.totalStudents} />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Average %"
              value={stats.avgPercentage}
              precision={1}
              suffix="%"
              valueStyle={{ color: stats.avgPercentage >= (assessment.passPercentage ?? 50) ? '#3f8600' : '#cf1322' }}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic
              title="Pass Rate"
              value={stats.passRate}
              precision={1}
              suffix="%"
              valueStyle={{ color: stats.passRate >= 50 ? '#3f8600' : '#cf1322' }}
            />
          </Card>
        </Col>
        <Col xs={12} sm={6}>
          <Card size="small">
            <Statistic title="Highest Mark" value={stats.highestMark} suffix={`/ ${assessment.maxMarks}`} />
          </Card>
        </Col>
      </Row>

      {/* Marks Table */}
      <Card title="Student Marks">
        <Table<IMarkList>
          dataSource={marks ?? []}
          columns={markColumns}
          rowKey="id"
          loading={marksPending}
          pagination={{ pageSize: 50, showSizeChanger: true, showTotal: (total) => `${total} students` }}
          size="small"
          locale={{ emptyText: <Empty description="No marks recorded" image={Empty.PRESENTED_IMAGE_SIMPLE} /> }}
        />
      </Card>
    </div>
  );
}

export default function MarkSheetDetailPage() {
  return (
    <AssessmentProvider>
      <MarkProvider>
        <MarkSheetDetailContent />
      </MarkProvider>
    </AssessmentProvider>
  );
}
