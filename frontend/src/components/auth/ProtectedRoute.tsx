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
  const { currentUser, jwtToken, currentRole, isPending } = useAuthState();

  useEffect(() => {
    // Check if user is authenticated
    if (!isPending && !jwtToken) {
      router.push(redirectTo);
      return;
    }

    // Check if user has required role
    if (
      !isPending &&
      currentRole &&
      allowedRoles &&
      allowedRoles.length > 0
    ) {
      const hasRole = allowedRoles.some(
        (role) => role.toLowerCase() === currentRole.toLowerCase()
      );

      if (!hasRole) {
        // Redirect to unauthorized page or back to login
        router.push("/unauthorized");
      }
    }
  }, [jwtToken, currentRole, allowedRoles, isPending, router, redirectTo]);

  // Show loading while checking auth
  if (isPending || (!currentUser && jwtToken)) {
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
        <Spin size="large" tip="Loading..." />
      </div>
    );
  }

  // Not authenticated
  if (!jwtToken) {
    return null;
  }

  // Authenticated - render children
  return <>{children}</>;
};
