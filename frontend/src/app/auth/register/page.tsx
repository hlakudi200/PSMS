"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { Alert, Button, Form, Input, Typography, message } from "antd";
import { BankOutlined, LockOutlined, MailOutlined, UserOutlined } from "@ant-design/icons";
import { useRouter } from "next/navigation";
import { z } from "zod";
import { getAxiosInstance } from "@/utils/axios-instance";
import { useAuthActions, useAuthState } from "@/providers/auth";
import { useBrandingActions, useBrandingState } from "@/providers/branding";
import styles from "../login/login.module.css";

const { Paragraph, Text } = Typography;

/** Same reasoning as the login page: one branding request per school, not per keystroke. */
const BRANDING_LOOKUP_DEBOUNCE_MS = 500;

/**
 * ADM-APPLY. A prospective parent or guardian signs themselves up so they can
 * apply to the school.
 *
 * The school's name is asked for the same way the login page asks for it —
 * there is no session yet, so the tenant can only come from what the person
 * types — and the page brands itself once it knows which school this is.
 *
 * The email address becomes the username. A parent applying for one child
 * should not have to invent a second identifier and then remember which one
 * the school wanted.
 */
const schema = z
  .object({
    tenancyName: z.string().trim().min(1, "Enter the name of the school you are applying to."),
    name: z.string().trim().min(2, "Enter your first name."),
    surname: z.string().trim().min(2, "Enter your surname."),
    emailAddress: z.string().trim().email("Enter a valid email address."),
    password: z
      .string()
      .min(8, "Use at least 8 characters.")
      .regex(/[a-z]/, "Include a lower-case letter.")
      .regex(/[A-Z]/, "Include a capital letter.")
      .regex(/\d/, "Include a number."),
    confirmPassword: z.string(),
  })
  .refine((v) => v.password === v.confirmPassword, {
    path: ["confirmPassword"],
    message: "The two passwords do not match.",
  });

type RegisterFormValues = z.infer<typeof schema>;

export default function RegisterPage() {
  const router = useRouter();
  const [form] = Form.useForm<RegisterFormValues>();
  const { loginUser } = useAuthActions();
  const { isSuccess: signedIn, currentRole } = useAuthState();
  const { branding } = useBrandingState();
  const { loadPublicBranding, resetBranding } = useBrandingActions();

  const instanceRef = useRef(getAxiosInstance());
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const [lookedUpTenancy, setLookedUpTenancy] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [refusal, setRefusal] = useState<string | undefined>();

  const scheduleBrandingLookup = useCallback(
    (tenancyName: string) => {
      const trimmed = tenancyName.trim();
      if (debounceRef.current) clearTimeout(debounceRef.current);

      if (!trimmed) {
        if (lookedUpTenancy !== null) {
          setLookedUpTenancy(null);
          resetBranding();
        }
        return;
      }

      if (trimmed === lookedUpTenancy) return;

      debounceRef.current = setTimeout(() => {
        setLookedUpTenancy(trimmed);
        loadPublicBranding(trimmed);
      }, BRANDING_LOOKUP_DEBOUNCE_MS);
    },
    [loadPublicBranding, resetBranding, lookedUpTenancy]
  );

  useEffect(
    () => () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    },
    []
  );

  // Signing up signs you in, and an applicant's place is the application.
  useEffect(() => {
    if (signedIn && currentRole?.toLowerCase() === "applicant") router.push("/apply");
  }, [signedIn, currentRole, router]);

  const handleSubmit = async (values: RegisterFormValues) => {
    const parsed = schema.safeParse(values);
    if (!parsed.success) {
      setRefusal(parsed.error.issues[0]?.message);
      return;
    }

    setSubmitting(true);
    setRefusal(undefined);

    try {
      const tenant = await instanceRef.current.post(
        "/api/services/app/Account/IsTenantAvailable",
        { tenancyName: parsed.data.tenancyName },
        { suppressErrorModal: true }
      );

      const tenantId = tenant.data?.result?.tenantId;
      if (!tenantId) {
        setRefusal("No school is registered under that name. Check the spelling with the school.");
        return;
      }

      await instanceRef.current.post(
        "/api/services/app/Account/RegisterApplicant",
        {
          name: parsed.data.name,
          surname: parsed.data.surname,
          emailAddress: parsed.data.emailAddress,
          password: parsed.data.password,
        },
        { headers: { "Abp-TenantId": String(tenantId) }, suppressErrorModal: true }
      );

      message.success("Your account is ready.");
      await loginUser({
        tenancyName: parsed.data.tenancyName,
        userNameOrEmailAddress: parsed.data.emailAddress,
        password: parsed.data.password,
      });
    } catch (error) {
      /* The server writes a sentence for whoever is reading it — "There is
         already an account for this email address", "Too many accounts have
         been created from here recently" — so show that rather than a message
         of ours, which can only be vaguer. */
      const abp = (error as {
        response?: { data?: { error?: { message?: string; details?: string } } };
      })?.response?.data?.error;
      setRefusal(abp?.message || abp?.details || "Could not create your account. Try again shortly.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className={styles.container}>
      <div className={styles.imagePanel}>
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img src="/images/login-bg.jpg" alt="School" />
        <div className={styles.imageOverlay}>
          <h2>{branding.schoolName}</h2>
          <p>Apply for a place. Create an account, then complete your application online.</p>
        </div>
      </div>

      <div className={styles.formPanel}>
        <div className={styles.formContainer}>
          <div className={styles.logo}>
            {branding.logoUrl ? (
              /* eslint-disable-next-line @next/next/no-img-element */
              <img src={branding.logoUrl} alt={`${branding.schoolName} logo`} className={styles.logoImage} />
            ) : (
              <div className={styles.logoIcon}>PSMS</div>
            )}
            <span className={styles.logoText}>{branding.schoolName}</span>
          </div>

          <h1 className={styles.heading}>Apply.</h1>
          <Paragraph type="secondary" style={{ marginTop: -8 }}>
            Create an account for yourself as the parent or guardian. You will fill in the
            learner&apos;s details on the next screen, and you can come back to finish later.
          </Paragraph>

          {refusal && (
            <Alert
              type="error"
              showIcon
              message={refusal}
              style={{ marginBottom: 16 }}
              closable
              onClose={() => setRefusal(undefined)}
            />
          )}

          <Form form={form} name="register" onFinish={handleSubmit} layout="vertical" size="large" disabled={submitting}>
            <Form.Item
              name="tenancyName"
              label="School you are applying to"
              rules={[{ required: true, message: "Enter the name of the school." }]}
            >
              <Input
                suffix={<BankOutlined style={{ color: "#8C8C8C" }} />}
                placeholder="School name"
                onChange={(e) => scheduleBrandingLookup(e.target.value)}
              />
            </Form.Item>

            <Form.Item name="name" label="Your first name" rules={[{ required: true, message: "Enter your first name." }]}>
              <Input suffix={<UserOutlined style={{ color: "#8C8C8C" }} />} placeholder="First name" autoComplete="given-name" />
            </Form.Item>

            <Form.Item name="surname" label="Your surname" rules={[{ required: true, message: "Enter your surname." }]}>
              <Input suffix={<UserOutlined style={{ color: "#8C8C8C" }} />} placeholder="Surname" autoComplete="family-name" />
            </Form.Item>

            <Form.Item
              name="emailAddress"
              label="Email address"
              extra="This is what you will sign in with, and where the school will contact you."
              rules={[{ required: true, type: "email", message: "Enter a valid email address." }]}
            >
              <Input suffix={<MailOutlined style={{ color: "#8C8C8C" }} />} placeholder="you@example.com" autoComplete="email" />
            </Form.Item>

            <Form.Item
              name="password"
              label="Password"
              extra="At least 8 characters, with a capital letter, a lower-case letter and a number."
              rules={[{ required: true, message: "Choose a password." }]}
            >
              <Input.Password placeholder="Password" autoComplete="new-password" prefix={<LockOutlined style={{ color: "#8C8C8C" }} />} />
            </Form.Item>

            <Form.Item name="confirmPassword" label="Confirm password" rules={[{ required: true, message: "Type the password again." }]}>
              <Input.Password placeholder="Confirm password" autoComplete="new-password" />
            </Form.Item>

            <div className={styles.buttonRow}>
              <Button type="primary" htmlType="submit" loading={submitting} className={styles.signInButton}>
                {submitting ? "Creating your account..." : "Create account"}
              </Button>
            </div>
          </Form>

          <div className={styles.footer}>
            <div className={styles.help}>
              <Text type="secondary">Already applied?</Text>
              <span className={styles.separator}>|</span>
              <a href="/auth/login">Sign in</a>
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
