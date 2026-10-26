import { useEffect } from "react";
import { Tabs } from "expo-router";
import { useAnnouncementsActions, useAnnouncementsState } from "../../../src/providers/announcements";
import { useMessagesActions, useMessagesState } from "../../../src/providers/messages";

export default function ParentTabsLayout() {
  const { unreadCount: unreadNews } = useAnnouncementsState();
  const { getUnreadCountAsync: getUnreadNewsAsync } = useAnnouncementsActions();
  const { unreadCount: unreadMessages } = useMessagesState();
  const { getUnreadCountAsync: getUnreadMessagesAsync } = useMessagesActions();

  // Sequential: the shared database pool is small.
  useEffect(() => {
    void (async () => {
      await getUnreadNewsAsync();
      await getUnreadMessagesAsync();
    })();
  }, [getUnreadNewsAsync, getUnreadMessagesAsync]);

  // The Messages tab holds both, so its badge counts both.
  const unread = (unreadNews ?? 0) + (unreadMessages ?? 0);

  return <Tabs screenOptions={{ headerShown: false }}>
    <Tabs.Screen name="index" options={{ title: "Home" }} />
    <Tabs.Screen name="progress" options={{ title: "Progress" }} />
    <Tabs.Screen name="fees" options={{ title: "Fees" }} />
    <Tabs.Screen name="messages" options={{ title: "Messages", tabBarBadge: unread || undefined }} />
    <Tabs.Screen name="more" options={{ title: "More" }} />
  </Tabs>;
}
