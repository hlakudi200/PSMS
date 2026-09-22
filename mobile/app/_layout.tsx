import { Stack } from "expo-router";
import { DevInspector } from "../src/components/dev-inspector/DevInspector";
import { AuthProvider } from "../src/providers/auth";
import { installConsoleCapture } from "../src/utils/dev-inspector";

installConsoleCapture();

export default function RootLayout() {
  return (
    <AuthProvider>
      <Stack screenOptions={{ headerShown: false }} />
      <DevInspector />
    </AuthProvider>
  );
}
