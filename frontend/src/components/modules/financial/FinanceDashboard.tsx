"use client";

import { Card, Row, Col, Statistic, Button } from "antd";
import {
  DollarOutlined,
  CreditCardOutlined,
  FileTextOutlined,
  LogoutOutlined,
} from "@ant-design/icons";
import { ProtectedRoute } from "@/components/shared/ProtectedRoute";
import { useAuthActions, useAuthState } from "@/providers/auth";

function FinanceDashboardContent() {
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
          <h1 style={{ color: "#FFFFFF", margin: 0, fontSize: "24px" }}>
            Financial Management
          </h1>
          <p style={{ color: "#BAE7FF", margin: "4px 0 0 0", fontSize: "13px" }}>
            Welcome, {currentUser?.name || "Finance Officer"}
          </p>
        </div>
        <Button
          type="primary"
          danger
          icon={<LogoutOutlined />}
          onClick={() => signOut()}
        >
          Logout
        </Button>
      </div>

      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic title="Total Revenue" value={0} prefix="R" suffix=".00" valueStyle={{ color: "#0066CC" }} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic title="Outstanding Fees" value={0} prefix="R" suffix=".00" valueStyle={{ color: "#FF4D4F" }} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic title="Payments Today" value={0} prefix={<CreditCardOutlined />} valueStyle={{ color: "#52C41A" }} />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic title="Invoices Generated" value={0} prefix={<FileTextOutlined />} valueStyle={{ color: "#FAAD14" }} />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: "16px" }}>
        <Col xs={24} lg={16}>
          <Card title="Financial Overview" bordered={false}>
            <p style={{ fontSize: "13px", color: "#595959" }}>
              Manage fee structures, student fees, payments, and generate financial reports.
            </p>
          </Card>
        </Col>
        <Col xs={24} lg={8}>
          <Card title="Quick Actions" bordered={false}>
            <Button block style={{ marginBottom: "8px" }}>Record Payment</Button>
            <Button block style={{ marginBottom: "8px" }}>Generate Invoice</Button>
            <Button block style={{ marginBottom: "8px" }}>Fee Structures</Button>
            <Button block>Financial Reports</Button>
          </Card>
        </Col>
      </Row>
    </div>
  );
}

export default function FinanceDashboard() {
  return (
    <ProtectedRoute allowedRoles={["Finance"]}>
      <FinanceDashboardContent />
    </ProtectedRoute>
  );
}
