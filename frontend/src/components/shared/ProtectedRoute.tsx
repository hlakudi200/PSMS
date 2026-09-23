"use client";

import { useEffect, ReactNode } from "react";
import { useRouter } from "next/navigation";
import { Spin } from "antd";
import { useAuthState } from "@/providers/auth";

interface ProtectedRouteProps {
  children: ReactNode;
  allowedRoles?: string[];
  redirectTo?: string;
}

export const ProtectedRoute = ({
  children,
  allowedRoles,
  redirectTo = "/auth/login",
}: ProtectedRouteProps) => {
  const router = useRouter();
  const { currentUser, jwtToken, currentRole, isPending, isHydrating } = useAuthState();

  const isAuthenticated = !isPending && !!jwtToken;
  const roleRequired = !!(allowedRoles && allowedRoles.length > 0);
  const hasAllowedRole =
    roleRequired
      ? !!currentRole &&
        allowedRoles!.some(
          (role) => role.toLowerCase() === currentRole.toLowerCase()
        )
      : true;

  useEffect(() => {
    // Effects run children before parents, so this fires before AuthProvider's
    // mount effect has read the token out of sessionStorage. Redirecting on
    // "no token" here is what signed the user out on every browser refresh.
    if (isHydrating !== false || isPending) return;
    if (!jwtToken) {
      router.push(redirectTo);
      return;
    }
    if (roleRequired && currentRole && !hasAllowedRole) {
      router.push("/unauthorized");
    }
  }, [
    jwtToken,
    currentRole,
    isPending,
    isHydrating,
    router,
    redirectTo,
    roleRequired,
    hasAllowedRole,
  ]);

  // Show loading while auth state resolves
  if (isHydrating !== false || isPending || (jwtToken && !currentUser)) {
    return (
      <div
        style={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          height: "100vh",
          background: "#f5f5f5",
        }}
      >
        <Spin size="large">
          <div style={{ padding: 40 }} />
        </Spin>
      </div>
    );
  }

  // Not authenticated — block render, redirect runs from the effect
  if (!isAuthenticated) return null;

  // Authenticated but role check pending (no role yet) — block render
  if (roleRequired && !currentRole) return null;

  // Authenticated but lacks required role — block render, redirect runs from the effect
  if (roleRequired && !hasAllowedRole) return null;

  return <>{children}</>;
};
