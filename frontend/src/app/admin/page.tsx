"use client";

import { useEffect, useState } from "react";
import { Card, Row, Col, Statistic, Button, Tag } from "antd";
import {
  UserOutlined,
  SafetyCertificateOutlined,
  SettingOutlined,
} from "@ant-design/icons";
import { useAuthState } from "@/providers/auth";
import { useRouter } from "next/navigation";
import { getAxiosInstance } from "@/utils/axios-instance";

export default function AdminPage() {
  const { currentTenant } = useAuthState();
  const router = useRouter();
  const [userCount, setUserCount] = useState<number>(0);
  const [roleCount, setRoleCount] = useState<number>(0);

  useEffect(() => {
    const instance = getAxiosInstance();

    instance
      .get("/api/services/app/User/GetAll?MaxResultCount=1&SkipCount=0")
      .then((res) => setUserCount(res.data.result.totalCount))
      .catch(() => {});

    instance
      .get("/api/services/app/Role/GetAll?MaxResultCount=1&SkipCount=0")
      .then((res) => setRoleCount(res.data.result.totalCount))
      .catch(() => {});
  }, []);

  return (
    <div>
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Total Users"
              value={userCount}
              prefix={<UserOutlined />}
              valueStyle={{ color: "#0066CC" }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Total Roles"
              value={roleCount}
              prefix={<SafetyCertificateOutlined />}
              valueStyle={{ color: "#52C41A" }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="School Status"
              value={currentTenant?.name ?? "—"}
              prefix={<SettingOutlined />}
              valueStyle={{ color: "#003D73", fontSize: 20 }}
              suffix={
                currentTenant ? (
                  <Tag
                    color={currentTenant.isActive ? "green" : "default"}
                    style={{ marginLeft: 8 }}
                  >
                    {currentTenant.isActive ? "Active" : "Inactive"}
                  </Tag>
                ) : null
              }
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col xs={24} lg={8}>
          <Card title="Quick Actions" bordered={false}>
            <Button
              block
              style={{ marginBottom: 8 }}
              onClick={() => router.push("/admin/users")}
            >
              Manage Users
            </Button>
            <Button
              block
              style={{ marginBottom: 8 }}
              onClick={() => router.push("/admin/roles")}
            >
              Manage Roles
            </Button>
            <Button
              block
              onClick={() => router.push("/admin/settings")}
            >
              School Settings
            </Button>
          </Card>
        </Col>
        <Col xs={24} lg={16}>
          <Card title="Overview" bordered={false}>
            <p style={{ fontSize: 13, color: "#595959" }}>
              School administration dashboard. Use the sidebar or quick actions
              to manage users, roles, and school settings for your tenant.
            </p>
          </Card>
        </Col>
      </Row>
    </div>
  );
}
