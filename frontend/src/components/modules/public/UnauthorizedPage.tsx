"use client";

import { Result, Button } from "antd";
import { useRouter } from "next/navigation";
import { useAuthActions, useAuthState } from "@/providers/auth";

export default function UnauthorizedPage() {
  const router = useRouter();
  const { signOut } = useAuthActions();
  const { currentRole } = useAuthState();

  const handleGoBack = () => {
    router.back();
  };

  const handleGoHome = () => {
    switch (currentRole?.toLowerCase()) {
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
      default:
        router.push("/");
    }
  };

  const handleLogout = () => {
    signOut();
  };

  return (
    <div
      style={{
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        minHeight: "100vh",
        background: "#F5F5F5",
      }}
    >
      <Result
        status="403"
        title="403"
        subTitle="Sorry, you are not authorized to access this page."
        extra={
          <div style={{ display: "flex", gap: "8px", justifyContent: "center" }}>
            <Button type="default" onClick={handleGoBack}>
              Go Back
            </Button>
            <Button type="primary" onClick={handleGoHome}>
              Go to Dashboard
            </Button>
            <Button danger onClick={handleLogout}>
              Logout
            </Button>
          </div>
        }
      />
    </div>
  );
}
