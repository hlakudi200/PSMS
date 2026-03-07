"use client";

import { Card, Row, Col, Statistic, Button } from "antd";
import {
  FileAddOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  LogoutOutlined,
} from "@ant-design/icons";
import { ProtectedRoute } from "@/components/shared/ProtectedRoute";
import { useAuthActions, useAuthState } from "@/providers/auth";

function AdmissionsDashboardContent() {
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
          <h1 style={{ color: "#FFFFFF", margin: 0, fontSize: "24px" }}>Admissions Management</h1>
          <p style={{ color: "#BAE7FF", margin: "4px 0 0 0", fontSize: "13px" }}>
            Welcome, {currentUser?.name || "Admissions Officer"}
          </p>
        </div>
        <Button type="primary" danger icon={<LogoutOutlined />} onClick={() => signOut()}>
          Logout
        </Button>
      </div>

      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card><Statistic title="Total Applications" value={0} prefix={<FileAddOutlined />} valueStyle={{ color: "#0066CC" }} /></Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card><Statistic title="Approved" value={0} prefix={<CheckCircleOutlined />} valueStyle={{ color: "#52C41A" }} /></Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card><Statistic title="Pending Review" value={0} prefix={<ClockCircleOutlined />} valueStyle={{ color: "#FAAD14" }} /></Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card><Statistic title="Interviews Scheduled" value={0} prefix={<ClockCircleOutlined />} valueStyle={{ color: "#1890FF" }} /></Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: "16px" }}>
        <Col xs={24} lg={16}>
          <Card title="Admissions Overview" bordered={false}>
            <p style={{ fontSize: "13px", color: "#595959" }}>
              Manage student applications, schedule interviews and assessments, review documents, and process enrollments.
            </p>
          </Card>
        </Col>
        <Col xs={24} lg={8}>
          <Card title="Quick Actions" bordered={false}>
            <Button block style={{ marginBottom: "8px" }}>View Applications</Button>
            <Button block style={{ marginBottom: "8px" }}>Schedule Interview</Button>
            <Button block style={{ marginBottom: "8px" }}>Review Documents</Button>
            <Button block>Process Enrollment</Button>
          </Card>
        </Col>
      </Row>
    </div>
  );
}

export default function AdmissionsDashboard() {
  return (
    <ProtectedRoute allowedRoles={["AdmissionsOfficer"]}>
      <AdmissionsDashboardContent />
    </ProtectedRoute>
  );
}
