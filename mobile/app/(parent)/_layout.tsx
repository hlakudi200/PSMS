import { Stack } from "expo-router";
import { RoleGuard } from "../../src/navigation/RoleGuard";
import { AnnouncementsProvider } from "../../src/providers/announcements";
import { ChildrenProvider } from "../../src/providers/children";
import { MarksProvider } from "../../src/providers/marks";
import { MessagesProvider } from "../../src/providers/messages";
import { NotificationsProvider } from "../../src/providers/notifications";
import { ReportCardsProvider } from "../../src/providers/report-cards";

/**
 * One navigator, inside every provider its screens share. Each provider wraps
 * the same Stack: two Stacks side by side render the app twice, and the
 * screens in one of them cannot reach the other's provider.
 */
export default function ParentLayout() {
  return (
    <RoleGuard role="parent">
      <ChildrenProvider>
        <MarksProvider>
          {/* The report card list and its detail screen. */}
          <ReportCardsProvider>
            {/* The school news feed, its detail screen, Home and the tab badge. */}
            <AnnouncementsProvider>
              {/* Conversations with teachers, a conversation and the compose screen. */}
              <MessagesProvider>
                {/* The notification centre, Home's card and the Home tab badge. */}
                <NotificationsProvider>
                  <Stack screenOptions={{ headerShown: false }} />
                </NotificationsProvider>
              </MessagesProvider>
            </AnnouncementsProvider>
          </ReportCardsProvider>
        </MarksProvider>
      </ChildrenProvider>
    </RoleGuard>
  );
}
