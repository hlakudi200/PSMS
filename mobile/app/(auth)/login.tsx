import { router } from "expo-router";
import { LoginScreen } from "../../src/screens/auth/LoginScreen";
export default function LoginRoute() { return <LoginScreen onNeedHelp={() => router.push("/(auth)/help")} />; }
