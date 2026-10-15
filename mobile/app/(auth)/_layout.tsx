import { Redirect, Stack } from "expo-router";
import { useAuthState } from "../../src/providers/auth";

export default function AuthLayout() {
  const { jwtToken, currentUser, currentRole, isAccessDenied } = useAuthState();

  // #408: signing in only saves the session; nothing on the sign-in screen moves on from it.
  // Once the account has loaded, hand over to "/" so AppNavigator stays the one place that
  // picks the Student or Parent area. An unsupported role has already been signed out.
  if (isAccessDenied) return <Redirect href="/unauthorized" />;
  if (jwtToken && currentUser && currentRole) return <Redirect href="/" />;

  return <Stack screenOptions={{ headerShown: false }} />;
}
