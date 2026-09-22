import { Tabs } from "expo-router";

export default function StudentTabsLayout() {
  return <Tabs screenOptions={{ headerShown: false }}>
    <Tabs.Screen name="index" options={{ title: "Home" }} />
    <Tabs.Screen name="timetable" options={{ title: "Timetable" }} />
    <Tabs.Screen name="learning" options={{ title: "Learning" }} />
    <Tabs.Screen name="marks" options={{ title: "Marks" }} />
    <Tabs.Screen name="more" options={{ title: "More" }} />
  </Tabs>;
}
