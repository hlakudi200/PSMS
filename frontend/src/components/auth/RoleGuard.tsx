"use client";

import { ReactNode } from "react";
import { Result, Button } from "antd";
import { useRouter } from "next/navigation";
import { useAuthState } from "@/providers/auth";

interface RoleGuardProps {
  children: ReactNode;
  allowedRoles: string[];
  fallback?: ReactNode;
}

/**
 * Component-level role guard
 * Shows children only if user has one of the allowed roles
 * Otherwise shows unauthorized message or custom fallback
 */
export const RoleGuard = ({
  children,
  allowedRoles,
  fallback,
}: RoleGuardProps) => {
  const router = useRouter();
  const { currentRole } = useAuthState();

  if (!currentRole) {
    return null;
  }

  const hasRole = allowedRoles.some(
    (role) => role.toLowerCase() === currentRole.toLowerCase()
  );

  if (!hasRole) {
    if (fallback) {
      return <>{fallback}</>;
    }

    return (
      <Result
        status="403"
        title="403"
        subTitle="Sorry, you are not authorized to access this content."
        extra={
          <Button type="primary" onClick={() => router.back()}>
            Go Back
          </Button>
        }
      />
    );
  }

  return <>{children}</>;
};
