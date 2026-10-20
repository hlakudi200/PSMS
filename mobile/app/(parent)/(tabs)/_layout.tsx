import { useEffect } from "react";
import { Tabs } from "expo-router";
import { useAnnouncementsActions, useAnnouncementsState } from "../../../src/providers/announcements";
import { useMessagesActions, useMessagesState } from "../../../src/providers/messages";
import { useNotificationsActions, useNotificationsState } from "../../../src/providers/notifications";

export default function ParentTabsLayout() {
  const { unreadCount: unreadNews } = useAnnouncementsState();
  const { getUnreadCountAsync: getUnreadNewsAsync } = useAnnouncementsActions();
  const { unreadCount: unreadMessages } = useMessagesState();
  const { getUnreadCountAsync: getUnreadMessagesAsync } = useMessagesActions();
  const { unreadCount: unreadNotifications } = useNotificationsState();
  const { getUnreadCountAsync: getUnreadNotificationsAsync } = useNotificationsActions();

  // Sequential: the shared database pool is small.
  useEffect(() => {
    void (async () => {
      await getUnreadNotificationsAsync();
      await getUnreadNewsAsync();
      await getUnreadMessagesAsync();
    })();
  }, [getUnreadNotificationsAsync, getUnreadNewsAsync, getUnreadMessagesAsync]);

  // The Messages tab holds both, so its badge counts both.
  const unread = (unreadNews ?? 0) + (unreadMessages ?? 0);

  return <Tabs screenOptions={{ headerShown: false }}>
    {/* Home is where the notification centre opens from. */}
    <Tabs.Screen name="index" options={{ title: "Home", tabBarBadge: unreadNotifications || undefined }} />
    <Tabs.Screen name="progress" options={{ title: "Progress" }} />
    <Tabs.Screen name="fees" options={{ title: "Fees" }} />
    <Tabs.Screen name="messages" options={{ title: "Messages", tabBarBadge: unread || undefined }} />
    <Tabs.Screen name="more" options={{ title: "More" }} />
  </Tabs>;
}
