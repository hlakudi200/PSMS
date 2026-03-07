"use client";

import { Card, Row, Col, Statistic, Button, Steps } from "antd";
import {
  FileTextOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  LogoutOutlined,
} from "@ant-design/icons";
import { ProtectedRoute } from "@/components/shared/ProtectedRoute";
import { useAuthActions, useAuthState } from "@/providers/auth";

function ApplicantDashboard() {
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
            Admissions Application
          </h1>
          <p style={{ color: "#BAE7FF", margin: "4px 0 0 0", fontSize: "13px" }}>
            Welcome, {currentUser?.name || "Applicant"}
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
        <Col xs={24} sm={12} lg={8}>
          <Card>
            <Statistic
              title="Application Status"
              value="Draft"
              prefix={<FileTextOutlined />}
              valueStyle={{ color: "#FAAD14" }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={8}>
          <Card>
            <Statistic
              title="Documents Uploaded"
              value={0}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: "#52C41A" }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={8}>
          <Card>
            <Statistic
              title="Interview Status"
              value="Not Scheduled"
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: "#8C8C8C" }}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: "16px" }}>
        <Col xs={24}>
          <Card title="Application Progress" bordered={false}>
            <Steps
              current={0}
              items={[
                {
                  title: "Personal Information",
                  description: "Complete your details",
                },
                {
                  title: "Upload Documents",
                  description: "Submit required documents",
                },
                {
                  title: "Pay Application Fee",
                  description: "Complete payment",
                },
                {
                  title: "Review & Submit",
                  description: "Submit application",
                },
              ]}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: "16px" }}>
        <Col xs={24} lg={16}>
          <Card title="Application Overview" bordered={false}>
            <p style={{ fontSize: "13px", color: "#595959" }}>
              Complete your application by filling in personal information,
              uploading required documents, and paying the application fee.
            </p>
          </Card>
        </Col>
        <Col xs={24} lg={8}>
          <Card title="Quick Actions" bordered={false}>
            <Button block style={{ marginBottom: "8px" }}>
              Continue Application
            </Button>
            <Button block style={{ marginBottom: "8px" }}>
              Upload Documents
            </Button>
            <Button block style={{ marginBottom: "8px" }}>
              Pay Application Fee
            </Button>
            <Button block>View Requirements</Button>
          </Card>
        </Col>
      </Row>
    </div>
  );
}

export default function ApplyPage() {
  return (
    <ProtectedRoute allowedRoles={["Applicant"]}>
      <ApplicantDashboard />
    </ProtectedRoute>
  );
}
