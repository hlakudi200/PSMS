"use client";

import { useEffect } from "react";
import { Form, Input, Button, Card, message } from "antd";
import { UserOutlined, LockOutlined } from "@ant-design/icons";
import { useRouter } from "next/navigation";
import { useAuthState, useAuthActions } from "@/providers/auth";
import styles from "./login.module.css";

interface LoginFormValues {
  userNameOrEmailAddress: string;
  password: string;
}

export default function LoginPage() {
  const router = useRouter();
  const { isPending, isSuccess, isError, currentRole } = useAuthState();
  const { loginUser, resetStateFlags } = useAuthActions();
  const [form] = Form.useForm();

  useEffect(() => {
    // Reset flags on mount
    resetStateFlags();
  }, [resetStateFlags]);

  useEffect(() => {
    if (isSuccess && currentRole) {
      message.success("Login successful!");

      // Role-based navigation
      switch (currentRole.toLowerCase()) {
        case "admin":
          router.push("/admin");
          break;
        case "principal":
        case "viceprincipal":
        case "hod":
          router.push("/academic");
          break;
        case "admissionsofficer":
          router.push("/admissions");
          break;
        case "finance":
          router.push("/finance");
          break;
        case "teacher":
          router.push("/teacher");
          break;
        case "parent":
          router.push("/parent");
          break;
        case "student":
          router.push("/student");
          break;
        case "applicant":
          router.push("/apply");
          break;
        default:
          router.push("/");
      }
    }
  }, [isSuccess, currentRole, router]);

  useEffect(() => {
    if (isError) {
      message.error("Login failed. Please check your credentials.");
      resetStateFlags();
    }
  }, [isError, resetStateFlags]);

  const handleSubmit = async (values: LoginFormValues) => {
    await loginUser(values);
  };

  return (
    <div className={styles.container}>
      <div className={styles.loginBox}>
        <div className={styles.header}>
          <div className={styles.logo}>
            <div className={styles.logoIcon}>PSMS</div>
          </div>
          <h1 className={styles.title}>Private School Management System</h1>
          <p className={styles.subtitle}>Enterprise Edition</p>
        </div>

        <Card className={styles.card}>
          <div className={styles.cardHeader}>
            <h2>System Login</h2>
          </div>

          <Form
            form={form}
            name="login"
            onFinish={handleSubmit}
            layout="vertical"
            size="large"
            disabled={isPending}
          >
            <Form.Item
              label="Username or Email"
              name="userNameOrEmailAddress"
              rules={[
                {
                  required: true,
                  message: "Please enter your username or email",
                },
              ]}
            >
              <Input
                prefix={<UserOutlined />}
                placeholder="Username or Email"
                autoComplete="username"
              />
            </Form.Item>

            <Form.Item
              label="Password"
              name="password"
              rules={[
                {
                  required: true,
                  message: "Please enter your password",
                },
              ]}
            >
              <Input.Password
                prefix={<LockOutlined />}
                placeholder="Password"
                autoComplete="current-password"
              />
            </Form.Item>

            <Form.Item>
              <Button
                type="primary"
                htmlType="submit"
                block
                loading={isPending}
                size="large"
              >
                {isPending ? "Logging in..." : "Log In"}
              </Button>
            </Form.Item>
          </Form>

          <div className={styles.footer}>
            <div className={styles.help}>
              <a href="#">Forgot Password?</a>
              <span className={styles.separator}>|</span>
              <a href="#">Need Help?</a>
            </div>
          </div>
        </Card>

        <div className={styles.systemInfo}>
          <p>© 2024 Private School Management System</p>
          <p className={styles.version}>Version 1.0.0</p>
        </div>
      </div>
    </div>
  );
}
