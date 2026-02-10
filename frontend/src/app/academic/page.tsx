'use client';

import { Card, Row, Col, Statistic, Button } from 'antd';
import {
  BookOutlined,
  TeamOutlined,
  FileTextOutlined,
} from '@ant-design/icons';
import { useRouter } from 'next/navigation';

export default function AcademicDashboard() {
  const router = useRouter();

  return (
    <div>
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Students"
              value={0}
              prefix={<TeamOutlined />}
              valueStyle={{ color: '#0066CC' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Classes"
              value={0}
              prefix={<BookOutlined />}
              valueStyle={{ color: '#52C41A' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Teachers"
              value={0}
              prefix={<TeamOutlined />}
              valueStyle={{ color: '#FAAD14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Current Term"
              value="N/A"
              prefix={<FileTextOutlined />}
              valueStyle={{ color: '#1890FF' }}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col xs={24} lg={16}>
          <Card title="Academic Overview" bordered={false}>
            <p style={{ fontSize: 13, color: '#595959' }}>
              Manage students, classes, teachers, subjects, and academic years.
              View reports and assessments.
            </p>
          </Card>
        </Col>
        <Col xs={24} lg={8}>
          <Card title="Quick Actions" bordered={false}>
            <Button block style={{ marginBottom: 8 }} onClick={() => router.push('/academic/grades')}>
              Manage Grades
            </Button>
            <Button block style={{ marginBottom: 8 }} onClick={() => router.push('/academic/classes')}>
              Manage Classes
            </Button>
            <Button block style={{ marginBottom: 8 }} onClick={() => router.push('/academic/teachers')}>
              Manage Teachers
            </Button>
            <Button block onClick={() => router.push('/academic/subjects')}>
              Manage Subjects
            </Button>
          </Card>
        </Col>
      </Row>
    </div>
  );
}
