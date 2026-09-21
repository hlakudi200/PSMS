import { router } from "expo-router";
import { SignInHelpScreen } from "../../src/screens/auth/SignInHelpScreen";
export default function HelpRoute() { return <SignInHelpScreen onBack={() => router.back()} />; }
