import { Redirect } from "expo-router";
import { useAuthState } from "../providers/auth";
import { AuthLoadingScreen } from "../screens/auth/AuthLoadingScreen";

export function AppNavigator() {
  const { isBootstrapping, jwtToken, currentUser, currentRole, isAccessDenied } = useAuthState();
  if (isBootstrapping || (jwtToken && !currentUser)) return <AuthLoadingScreen />;
  if (isAccessDenied) return <Redirect href="/unauthorized" />;
  if (!jwtToken) return <Redirect href="/(auth)/login" />;
  if (currentRole === "student") return <Redirect href="/(student)/(tabs)" />;
  if (currentRole === "parent") return <Redirect href="/(parent)/(tabs)" />;
  return <Redirect href="/unauthorized" />;
}
