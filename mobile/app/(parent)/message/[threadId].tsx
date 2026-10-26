import { useLocalSearchParams } from "expo-router";
import { MessageThreadScreen } from "../../../src/screens/parent/MessageThreadScreen";

export default function MessageThreadRoute() {
  const { threadId } = useLocalSearchParams<{ threadId: string }>();
  return <MessageThreadScreen threadId={threadId} />;
}
