import { useEffect } from "react";
import { Tabs } from "expo-router";
import { useAnnouncementsActions, useAnnouncementsState } from "../../../src/providers/announcements";

export default function ParentTabsLayout() {
  const { unreadCount } = useAnnouncementsState();
  const { getUnreadCountAsync } = useAnnouncementsActions();

  useEffect(() => { void getUnreadCountAsync(); }, [getUnreadCountAsync]);

  return <Tabs screenOptions={{ headerShown: false }}>
    <Tabs.Screen name="index" options={{ title: "Home" }} />
    <Tabs.Screen name="progress" options={{ title: "Progress" }} />
    <Tabs.Screen name="fees" options={{ title: "Fees" }} />
    <Tabs.Screen name="messages" options={{ title: "Messages", tabBarBadge: unreadCount || undefined }} />
    <Tabs.Screen name="more" options={{ title: "More" }} />
  </Tabs>;
}
