"use client";

import { Card, Row, Col, Statistic, Button } from "antd";
import {
  UserOutlined,
  TeamOutlined,
  SettingOutlined,
  LogoutOutlined,
} from "@ant-design/icons";
import { ProtectedRoute } from "@/components/auth/ProtectedRoute";
import { useAuthActions, useAuthState } from "@/providers/auth";

function AdminDashboard() {
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
            Admin Dashboard
          </h1>
          <p style={{ color: "#BAE7FF", margin: "4px 0 0 0", fontSize: "13px" }}>
            Welcome, {currentUser?.name || "Administrator"}
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
              title="Total Users"
              value={0}
              prefix={<UserOutlined />}
              valueStyle={{ color: "#0066CC" }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Tenants"
              value={0}
              prefix={<TeamOutlined />}
              valueStyle={{ color: "#52C41A" }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Active Sessions"
              value={0}
              prefix={<UserOutlined />}
              valueStyle={{ color: "#FAAD14" }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="System Status"
              value="Online"
              prefix={<SettingOutlined />}
              valueStyle={{ color: "#52C41A" }}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: "16px" }}>
        <Col xs={24} lg={16}>
          <Card title="System Overview" bordered={false}>
            <p style={{ fontSize: "13px", color: "#595959" }}>
              System administration dashboard. Manage users, tenants, and system
              settings.
            </p>
          </Card>
        </Col>
        <Col xs={24} lg={8}>
          <Card title="Quick Actions" bordered={false}>
            <Button block style={{ marginBottom: "8px" }}>
              Manage Users
            </Button>
            <Button block style={{ marginBottom: "8px" }}>
              Manage Tenants
            </Button>
            <Button block>System Settings</Button>
          </Card>
        </Col>
      </Row>
    </div>
  );
}

export default function AdminPage() {
  return (
    <ProtectedRoute allowedRoles={["Admin"]}>
      <AdminDashboard />
    </ProtectedRoute>
  );
}
