'use client';

// NOTE: This is the temporary T-T01 wiring — the dashboard now renders inside
// the teacher LayoutShell (auth + header + sidebar are provided there).
// Real metrics, click handlers, and final layout land in T-T02.

import { Alert, Card, Row, Col, Statistic, Button } from 'antd';
import {
  BookOutlined,
  FileTextOutlined,
  TeamOutlined,
} from '@ant-design/icons';

// Until T-T02 wires up live metrics, the dashboard renders "--" placeholders
// rather than "0", and shows an explicit banner so a teacher can't mistake
// the stub for an authoritative empty state (e.g. "0 Lessons Today").
const PLACEHOLDER: string = '--';

export default function TeacherDashboard() {
  return (
    <div>
      <Alert
        type="info"
        showIcon
        closable
        message="Dashboard metrics are not yet wired up — values are placeholders. Live metrics land in the next ticket (T-T02)."
        style={{ marginBottom: 16 }}
      />
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="My Classes"
              value={PLACEHOLDER}
              prefix={<BookOutlined />}
              valueStyle={{ color: '#0066CC' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="My Students"
              value={PLACEHOLDER}
              prefix={<TeamOutlined />}
              valueStyle={{ color: '#52C41A' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Assessments Due"
              value={PLACEHOLDER}
              prefix={<FileTextOutlined />}
              valueStyle={{ color: '#FAAD14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Lessons Today"
              value={PLACEHOLDER}
              prefix={<BookOutlined />}
              valueStyle={{ color: '#1890FF' }}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col xs={24} lg={16}>
          <Card title="Teacher Overview" variant="borderless">
            <p style={{ fontSize: 13, color: '#595959', margin: 0 }}>
              Manage your classes, record assessments, upload learning materials,
              and schedule online lessons.
            </p>
          </Card>
        </Col>
        <Col xs={24} lg={8}>
          <Card title="Quick Actions" variant="borderless">
            <Button block disabled style={{ marginBottom: 8 }}>
              My Classes
            </Button>
            <Button block disabled style={{ marginBottom: 8 }}>
              Record Marks
            </Button>
            <Button block disabled style={{ marginBottom: 8 }}>
              Upload Material
            </Button>
            <Button block disabled>
              Schedule Lesson
            </Button>
          </Card>
        </Col>
      </Row>
    </div>
  );
}
