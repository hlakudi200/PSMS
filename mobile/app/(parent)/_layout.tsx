import { Stack } from "expo-router";
import { RoleGuard } from "../../src/navigation/RoleGuard";
import { AnnouncementsProvider } from "../../src/providers/announcements";
import { ChildrenProvider } from "../../src/providers/children";
import { MarksProvider } from "../../src/providers/marks";
import { ReportCardsProvider } from "../../src/providers/report-cards";

export default function ParentLayout() {
  return (
    <RoleGuard role="parent">
      <ChildrenProvider>
        <MarksProvider>
          {/* Shared by the feed, its detail screen, Home and the tab badge. */}
          <AnnouncementsProvider>
            <Stack screenOptions={{ headerShown: false }} />
          </AnnouncementsProvider>
          {/* Shared by the report card list and its detail screen. */}
          <ReportCardsProvider>
            <Stack screenOptions={{ headerShown: false }} />
          </ReportCardsProvider>
        </MarksProvider>
      </ChildrenProvider>
    </RoleGuard>
  );
}
