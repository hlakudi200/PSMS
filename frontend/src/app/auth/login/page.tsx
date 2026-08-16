"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { Form, Input, Button, message } from "antd";
import { UserOutlined, LockOutlined, BankOutlined } from "@ant-design/icons";
import { useRouter } from "next/navigation";
import { useAuthState, useAuthActions } from "@/providers/auth";
import { useBrandingActions, useBrandingState } from "@/providers/branding";
import styles from "./login.module.css";

/**
 * Issue #56, AC 6. There is no session yet, so the tenant can only come from
 * what the user types. Wait for a pause in typing before asking the server for
 * that school's branding — one request per school, not one per keystroke.
 */
const BRANDING_LOOKUP_DEBOUNCE_MS = 500;

interface LoginFormValues {
  tenancyName: string;
  userNameOrEmailAddress: string;
  password: string;
}

export default function LoginPage() {
  const router = useRouter();
  const { isPending, isSuccess, isError, currentRole } = useAuthState();
  const { loginUser, resetStateFlags } = useAuthActions();
  const { branding } = useBrandingState();
  const { loadPublicBranding } = useBrandingActions();
  const [form] = Form.useForm();

  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const [lookedUpTenancy, setLookedUpTenancy] = useState<string | null>(null);

  useEffect(() => {
    resetStateFlags();
  }, [resetStateFlags]);

  const scheduleBrandingLookup = useCallback(
    (tenancyName: string) => {
      const trimmed = tenancyName.trim();

      if (debounceRef.current) clearTimeout(debounceRef.current);
      if (!trimmed || trimmed === lookedUpTenancy) return;

      debounceRef.current = setTimeout(() => {
        setLookedUpTenancy(trimmed);
        loadPublicBranding(trimmed);
      }, BRANDING_LOOKUP_DEBOUNCE_MS);
    },
    [loadPublicBranding, lookedUpTenancy]
  );

  // Brand the page for whatever school is prefilled, and clean up any pending
  // timer so an unmounted page never fires a request.
  useEffect(() => {
    const initial = form.getFieldValue("tenancyName");
    if (initial) scheduleBrandingLookup(initial);

    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (isSuccess && currentRole) {
      message.success("Login successful!");

      switch (currentRole.toLowerCase()) {
        case "admin":
          router.push("/admin");
          break;
        case "principal":
          router.push("/principal");
          break;
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
      {/* Left - Image */}
      <div className={styles.imagePanel}>
        <img
          src="/images/login-bg.jpg"
          alt="School"
        />
        <div className={styles.imageOverlay}>
          <h2>{branding.schoolName}</h2>
          <p>Empowering educators, students, and parents with seamless school administration.</p>
        </div>
      </div>

      {/* Right - Form */}
      <div className={styles.formPanel}>
        <div className={styles.formContainer}>
          <div className={styles.logo}>
            {branding.logoUrl ? (
              /* eslint-disable-next-line @next/next/no-img-element */
              <img
                src={branding.logoUrl}
                alt={`${branding.schoolName} logo`}
                className={styles.logoImage}
              />
            ) : (
              <div className={styles.logoIcon}>PSMS</div>
            )}
            <span className={styles.logoText}>{branding.schoolName}</span>
          </div>

          <h1 className={styles.heading}>Login.</h1>

          <Form
            form={form}
            name="login"
            onFinish={handleSubmit}
            layout="vertical"
            size="large"
            disabled={isPending}
          >
            <Form.Item
              name="tenancyName"
              initialValue="Default"
              rules={[{ required: true, message: "Please enter your school name" }]}
            >
              <Input
                suffix={<BankOutlined style={{ color: "#8C8C8C" }} />}
                placeholder="School Name"
                onChange={(e) => scheduleBrandingLookup(e.target.value)}
              />
            </Form.Item>

            <Form.Item
              name="userNameOrEmailAddress"
              rules={[{ required: true, message: "Please enter your username or email" }]}
            >
              <Input
                suffix={<UserOutlined style={{ color: "#8C8C8C" }} />}
                placeholder="Username or Email"
                autoComplete="username"
              />
            </Form.Item>

            <Form.Item
              name="password"
              rules={[{ required: true, message: "Please enter your password" }]}
            >
              <Input.Password
                placeholder="Password"
                autoComplete="current-password"
              />
            </Form.Item>

            <div className={styles.buttonRow}>
              <Button
                type="primary"
                htmlType="submit"
                loading={isPending}
                className={styles.signInButton}
              >
                {isPending ? "Signing in..." : "Sign In"}
              </Button>
            </div>
          </Form>

          <div className={styles.footer}>
            <div className={styles.help}>
              <a href="#">Forgot Password?</a>
              <span className={styles.separator}>|</span>
              <a href="#">Need Help?</a>
            </div>
          </div>

          <div className={styles.systemInfo}>
            <p>&copy; 2025 Private School Management System</p>
          </div>
        </div>
      </div>
    </div>
  );
}
