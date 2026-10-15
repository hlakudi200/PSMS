import { Stack } from "expo-router";
import { RoleGuard } from "../../src/navigation/RoleGuard";
import { ChildrenProvider } from "../../src/providers/children";
import { MarksProvider } from "../../src/providers/marks";

export default function ParentLayout() {
  return (
    <RoleGuard role="parent">
      <ChildrenProvider>
        <MarksProvider>
          <Stack screenOptions={{ headerShown: false }} />
        </MarksProvider>
      </ChildrenProvider>
    </RoleGuard>
  );
}
