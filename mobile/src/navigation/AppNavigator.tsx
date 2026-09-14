import { useState } from "react";
import { useAuthActions, useAuthState } from "../providers/auth";
import { AccessDeniedScreen } from "../screens/auth/AccessDeniedScreen";
import { AuthLoadingScreen } from "../screens/auth/AuthLoadingScreen";
import { LoginScreen } from "../screens/auth/LoginScreen";
import { SignInHelpScreen } from "../screens/auth/SignInHelpScreen";
import { ParentHomeScreen } from "../screens/parent/ParentHomeScreen";
import { StudentHomeScreen } from "../screens/student/StudentHomeScreen";

type AuthScreen = "login" | "help";

export function AppNavigator() {
  const { isBootstrapping, jwtToken, currentUser, currentRole, isAccessDenied } = useAuthState();
  const { signOut } = useAuthActions();
  const [authScreen, setAuthScreen] = useState<AuthScreen>("login");

  if (isBootstrapping || (jwtToken && !currentUser)) return <AuthLoadingScreen />;
  if (isAccessDenied) return <AccessDeniedScreen onBack={() => void signOut()} />;
  if (!jwtToken) return authScreen === "help" ? <SignInHelpScreen onBack={() => setAuthScreen("login")} /> : <LoginScreen onNeedHelp={() => setAuthScreen("help")} />;
  if (currentRole === "student") return <StudentHomeScreen />;
  if (currentRole === "parent") return <ParentHomeScreen />;

  return <AccessDeniedScreen onBack={() => void signOut()} />;
}
