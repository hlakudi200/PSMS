import { Stack } from "expo-router";
import { RoleGuard } from "../../src/navigation/RoleGuard";
import { ChildrenProvider } from "../../src/providers/children";

export default function ParentLayout() {
  return (
    <RoleGuard role="parent">
      <ChildrenProvider>
        <Stack screenOptions={{ headerShown: false }} />
      </ChildrenProvider>
    </RoleGuard>
  );
}
