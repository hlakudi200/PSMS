import { router } from "expo-router";
import { AccessDeniedScreen } from "../src/screens/auth/AccessDeniedScreen";
import { useAuthActions } from "../src/providers/auth";

export default function UnauthorizedRoute() {
  const { signOut } = useAuthActions();
  return <AccessDeniedScreen onBack={() => { void signOut().then(() => router.replace("/(auth)/login")); }} />;
}
