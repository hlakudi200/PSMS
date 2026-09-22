import { Stack } from "expo-router";
import { RoleGuard } from "../../src/navigation/RoleGuard";
import { MarksProvider } from "../../src/providers/marks";
import { SubjectsProvider } from "../../src/providers/subjects";
import { TimetableProvider } from "../../src/providers/timetable";

export default function StudentLayout() {
  return (
    <RoleGuard role="student">
      <MarksProvider>
        <SubjectsProvider>
          <TimetableProvider>
            <Stack screenOptions={{ headerShown: false }} />
          </TimetableProvider>
        </SubjectsProvider>
      </MarksProvider>
    </RoleGuard>
  );
}
