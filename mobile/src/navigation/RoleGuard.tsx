import type { PropsWithChildren } from "react";
import { Redirect } from "expo-router";
import { AuthLoadingScreen } from "../screens/auth/AuthLoadingScreen";
import { useAuthState } from "../providers/auth";
import type { MobileRole } from "../utils/jwt-decoder";

export function RoleGuard({ role, children }: PropsWithChildren<{ role: MobileRole }>) {
  const { isBootstrapping, jwtToken, currentUser, currentRole } = useAuthState();
  if (isBootstrapping || (jwtToken && !currentUser)) return <AuthLoadingScreen />;
  if (!jwtToken) return <Redirect href="/(auth)/login" />;
  if (currentRole !== role) return <Redirect href="/unauthorized" />;
  return <>{children}</>;
}
