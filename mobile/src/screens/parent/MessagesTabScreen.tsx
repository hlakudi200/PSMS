import { useEffect, useState } from "react";
import { ScrollView, StyleSheet } from "react-native";
import { Chip } from "../../components";
import { spacing } from "../../theme";
import { useAnnouncementsState } from "../../providers/announcements";
import { useMessagesState } from "../../providers/messages";
import { AnnouncementsScreen } from "./AnnouncementsScreen";
import { MessagesScreen } from "./MessagesScreen";

export type MessagesTabView = "teachers" | "news";

/**
 * The Messages tab: conversations with teachers (#204) and the school's
 * announcements (#203), one at a time. `initialView` lets Home's
 * announcements card open straight onto the news.
 */
export function MessagesTabScreen({ initialView = "teachers" }: { initialView?: MessagesTabView }) {
  const [view, setView] = useState<MessagesTabView>(initialView);
  const { unreadCount: unreadMessages } = useMessagesState();
  const { unreadCount: unreadNews } = useAnnouncementsState();

  // The tab stays mounted, so a second tap on Home's card arrives as a new param.
  useEffect(() => { setView(initialView); }, [initialView]);

  const switcher = (
    <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.chips}>
      <Chip
        label={unreadMessages ? `Teachers (${unreadMessages})` : "Teachers"}
        selected={view === "teachers"}
        onPress={() => setView("teachers")}
      />
      <Chip
        label={unreadNews ? `School news (${unreadNews})` : "School news"}
        selected={view === "news"}
        onPress={() => setView("news")}
      />
    </ScrollView>
  );

  return view === "news" ? <AnnouncementsScreen switcher={switcher} /> : <MessagesScreen switcher={switcher} />;
}

const styles = StyleSheet.create({
  chips: { gap: spacing.sm, marginTop: spacing.md },
});
