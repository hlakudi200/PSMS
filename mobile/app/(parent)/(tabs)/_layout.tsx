import { Tabs } from "expo-router";

export default function ParentTabsLayout() {
  return <Tabs screenOptions={{ headerShown: false }}>
    <Tabs.Screen name="index" options={{ title: "Home" }} />
    <Tabs.Screen name="children" options={{ title: "Children" }} />
    <Tabs.Screen name="fees" options={{ title: "Fees" }} />
    <Tabs.Screen name="messages" options={{ title: "Messages" }} />
    <Tabs.Screen name="more" options={{ title: "More" }} />
  </Tabs>;
}
