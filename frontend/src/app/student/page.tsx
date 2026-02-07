"use client";

import { Card, Row, Col, Statistic, Button } from "antd";
import {
  BookOutlined,
  FileTextOutlined,
  TrophyOutlined,
  LogoutOutlined,
} from "@ant-design/icons";
import { ProtectedRoute } from "@/components/auth/ProtectedRoute";
import { useAuthActions, useAuthState } from "@/providers/auth";

function StudentDashboard() {
  const { signOut } = useAuthActions();
  const { currentUser } = useAuthState();

  const handleLogout = () => {
    signOut();
  };

  return (
    <div style={{ padding: "24px", background: "#F5F5F5", minHeight: "100vh" }}>
      <div
        style={{
          background: "#003D73",
          padding: "16px 24px",
          marginBottom: "24px",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <div>
          <h1 style={{ color: "#FFFFFF", margin: 0, fontSize: "24px" }}>
            Student Portal
          </h1>
          <p style={{ color: "#BAE7FF", margin: "4px 0 0 0", fontSize: "13px" }}>
            Welcome, {currentUser?.name || "Student"}
          </p>
        </div>
        <Button
          type="primary"
          danger
          icon={<LogoutOutlined />}
          onClick={handleLogout}
        >
          Logout
        </Button>
      </div>

      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="My Subjects"
              value={0}
              prefix={<BookOutlined />}
              valueStyle={{ color: "#0066CC" }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Upcoming Assessments"
              value={0}
              prefix={<FileTextOutlined />}
              valueStyle={{ color: "#FAAD14" }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Current Average"
              value={0}
              suffix="%"
              prefix={<TrophyOutlined />}
              valueStyle={{ color: "#52C41A" }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Attendance"
              value={0}
              suffix="%"
              valueStyle={{ color: "#1890FF" }}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: "16px" }}>
        <Col xs={24} lg={16}>
          <Card title="Student Overview" bordered={false}>
            <p style={{ fontSize: "13px", color: "#595959" }}>
              View your subjects, assessments, reports, learning materials, and
              online lessons.
            </p>
          </Card>
        </Col>
        <Col xs={24} lg={8}>
          <Card title="Quick Actions" bordered={false}>
            <Button block style={{ marginBottom: "8px" }}>
              My Reports
            </Button>
            <Button block style={{ marginBottom: "8px" }}>
              Learning Materials
            </Button>
            <Button block style={{ marginBottom: "8px" }}>
              Online Lessons
            </Button>
            <Button block>My Timetable</Button>
          </Card>
        </Col>
      </Row>
    </div>
  );
}

export default function StudentPage() {
  return (
    <ProtectedRoute allowedRoles={["Student"]}>
      <StudentDashboard />
    </ProtectedRoute>
  );
}
