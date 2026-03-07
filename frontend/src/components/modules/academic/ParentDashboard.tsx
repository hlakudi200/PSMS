"use client";

import { Card, Row, Col, Statistic, Button } from "antd";
import {
  UserOutlined,
  FileTextOutlined,
  DollarOutlined,
  LogoutOutlined,
} from "@ant-design/icons";
import { ProtectedRoute } from "@/components/shared/ProtectedRoute";
import { useAuthActions, useAuthState } from "@/providers/auth";

function ParentDashboardContent() {
  const { signOut } = useAuthActions();
  const { currentUser } = useAuthState();

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
          <h1 style={{ color: "#FFFFFF", margin: 0, fontSize: "24px" }}>Parent Portal</h1>
          <p style={{ color: "#BAE7FF", margin: "4px 0 0 0", fontSize: "13px" }}>
            Welcome, {currentUser?.name || "Parent"}
          </p>
        </div>
        <Button type="primary" danger icon={<LogoutOutlined />} onClick={() => signOut()}>
          Logout
        </Button>
      </div>

      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card><Statistic title="My Children" value={0} prefix={<UserOutlined />} valueStyle={{ color: "#0066CC" }} /></Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card><Statistic title="Unread Messages" value={0} prefix={<FileTextOutlined />} valueStyle={{ color: "#FAAD14" }} /></Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card><Statistic title="Outstanding Fees" value={0} prefix="R" suffix=".00" valueStyle={{ color: "#FF4D4F" }} /></Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card><Statistic title="Upcoming Events" value={0} prefix={<FileTextOutlined />} valueStyle={{ color: "#1890FF" }} /></Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: "16px" }}>
        <Col xs={24} lg={16}>
          <Card title="Parent Overview" bordered={false}>
            <p style={{ fontSize: "13px", color: "#595959" }}>
              View your children&apos;s academic progress, reports, attendance, and manage fees and communications.
            </p>
          </Card>
        </Col>
        <Col xs={24} lg={8}>
          <Card title="Quick Actions" bordered={false}>
            <Button block style={{ marginBottom: "8px" }}>View Reports</Button>
            <Button block style={{ marginBottom: "8px" }}>Check Attendance</Button>
            <Button block style={{ marginBottom: "8px" }}>Pay Fees</Button>
            <Button block>Messages</Button>
          </Card>
        </Col>
      </Row>
    </div>
  );
}

export default function ParentDashboard() {
  return (
    <ProtectedRoute allowedRoles={["Parent"]}>
      <ParentDashboardContent />
    </ProtectedRoute>
  );
}
