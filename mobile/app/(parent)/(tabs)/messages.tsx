import { useLocalSearchParams } from "expo-router";
import { MessagesTabScreen } from "../../../src/screens/parent/MessagesTabScreen";

export default function MessagesRoute() {
  const { view } = useLocalSearchParams<{ view?: string }>();
  return <MessagesTabScreen initialView={view === "news" ? "news" : "teachers"} />;
}
