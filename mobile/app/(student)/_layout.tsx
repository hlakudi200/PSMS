import { Stack } from "expo-router";
import { RoleGuard } from "../../src/navigation/RoleGuard";
import { MarksProvider } from "../../src/providers/marks";

export default function StudentLayout() {
  return (
    <RoleGuard role="student">
      <MarksProvider>
        <Stack screenOptions={{ headerShown: false }} />
      </MarksProvider>
    </RoleGuard>
  );
}
