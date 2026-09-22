import { Stack } from "expo-router";
import { RoleGuard } from "../../src/navigation/RoleGuard";
import { MarksProvider } from "../../src/providers/marks";
import { TimetableProvider } from "../../src/providers/timetable";

export default function StudentLayout() {
  return (
    <RoleGuard role="student">
      <MarksProvider>
        <TimetableProvider>
          <Stack screenOptions={{ headerShown: false }} />
        </TimetableProvider>
      </MarksProvider>
    </RoleGuard>
  );
}
