import { useCallback, useState, type ReactNode } from "react";
import { ActivityIndicator, FlatList, Pressable, RefreshControl, ScrollView, StyleSheet, Text, View } from "react-native";
import { router, useFocusEffect } from "expo-router";
import { Badge, Button, Card, Chip, Screen, StateView } from "../../components";
import { colors, spacing, typography } from "../../theme";
import { useMessagesActions, useMessagesState } from "../../providers/messages";
import type { IMessageSummary, MessageBox } from "../../providers/messages/context";
import { formatMessageTime } from "../../utils/message-forms";

const BOXES: { box: MessageBox; label: string }[] = [
  { box: "inbox", label: "Inbox" },
  { box: "sent", label: "Sent" },
];

/** `switcher` is the Messages tab's Teachers / School news choice, shown in the header. */
export function MessagesScreen({ switcher }: { switcher?: ReactNode }) {
  const [box, setBox] = useState<MessageBox>("inbox");
  const state = useMessagesState();
  const { getBoxAsync, loadMoreAsync, getUnreadCountAsync } = useMessagesActions();
  const list = state[box];

  // On focus as well as on switching box: coming back from a conversation or a
  // new message, the list and the badge should show what was just read or sent.
  useFocusEffect(useCallback(() => {
    void (async () => {
      await getBoxAsync(box);
      await getUnreadCountAsync();
    })();
  }, [box, getBoxAsync, getUnreadCountAsync]));

  const unread = state.unreadCount ?? state.inbox?.items.filter((m) => !m.isRead).length ?? 0;

  const header = (
    <View>
      <Badge label="MESSAGES" />
      <Text style={styles.title}>Messages</Text>
      <Text style={styles.subtitle}>Write to your children&apos;s teachers. Replies arrive here.</Text>
      {switcher}
      <View style={styles.compose}>
        <Button onPress={() => router.push("/(parent)/message/compose")}>New message</Button>
      </View>
      <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.chips}>
        {BOXES.map(({ box: b, label }) => (
          <Chip
            key={b}
            label={b === "inbox" && unread ? `${label} (${unread})` : label}
            selected={b === box}
            onPress={() => setBox(b)}
          />
        ))}
      </ScrollView>
      {state.isListError && !!list && (
        <Text style={styles.inlineError}>Couldn&apos;t refresh — showing what loaded earlier.</Text>
      )}
    </View>
  );

  if (!list) {
    return (
      <Screen>
        {header}
        {state.isListError ? (
          <>
            <StateView state="error" message="Couldn't load your messages." />
            <Button onPress={() => void getBoxAsync(box)}>Retry</Button>
          </>
        ) : (
          <StateView state="loading" message="Loading messages…" />
        )}
      </Screen>
    );
  }

  return (
    <Screen>
      <FlatList
        data={list.items}
        keyExtractor={(item) => item.id}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={state.isListPending} onRefresh={() => void getBoxAsync(box)} />}
        onEndReached={() => void loadMoreAsync(box)}
        onEndReachedThreshold={0.4}
        ListHeaderComponent={header}
        ListEmptyComponent={
          <StateView
            state="empty"
            message={box === "inbox" ? "No messages yet. Teachers' replies will appear here." : "You haven't sent any messages yet."}
          />
        }
        ListFooterComponent={state.isLoadingMore ? <ActivityIndicator style={styles.more} color={colors.primary} /> : null}
        renderItem={({ item }) => (
          <MessageRow
            message={item}
            box={box}
            onPress={() => router.push({ pathname: "/(parent)/message/[threadId]", params: { threadId: item.threadId } })}
          />
        )}
      />
    </Screen>
  );
}

function MessageRow({ message, box, onPress }: { message: IMessageSummary; box: MessageBox; onPress: () => void }) {
  const unread = box === "inbox" && !message.isRead;
  const who = box === "inbox" ? message.senderName ?? "Someone at school" : `To ${message.recipientName ?? "a teacher"}`;

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={`${unread ? "Unread. " : ""}${who}. ${message.subject ?? "No subject"}`}
      onPress={onPress}
      style={styles.cardWrapper}
    >
      <Card style={unread ? styles.unreadCard : undefined}>
        <View style={styles.rowTop}>
          <Text style={[styles.who, unread && styles.whoUnread]} numberOfLines={1}>{who}</Text>
          <Text style={styles.time}>{formatMessageTime(message.creationTime)}</Text>
        </View>
        <View style={styles.rowBottom}>
          <Text style={styles.subject} numberOfLines={1}>{message.subject ?? "No subject"}</Text>
          {unread && <Badge label="New" tone="success" />}
          {box === "sent" && message.isRead && <Text style={styles.read}>Read</Text>}
        </View>
      </Card>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  title: { color: colors.text, fontSize: typography.title, fontWeight: "800", marginTop: spacing.lg },
  subtitle: { color: colors.textMuted, fontSize: 14, marginTop: spacing.xs },
  compose: { marginTop: spacing.lg },
  chips: { gap: spacing.sm, marginTop: spacing.lg },
  inlineError: { color: colors.danger, fontSize: 14, marginTop: spacing.md },
  list: { paddingBottom: spacing.xxl },
  cardWrapper: { marginTop: spacing.md },
  unreadCard: { borderColor: colors.primary },
  rowTop: { alignItems: "center", flexDirection: "row", gap: spacing.sm },
  who: { color: colors.text, flex: 1, fontSize: 16, fontWeight: "600" },
  whoUnread: { fontWeight: "800" },
  time: { color: colors.textMuted, fontSize: 13 },
  rowBottom: { alignItems: "center", flexDirection: "row", gap: spacing.sm, marginTop: spacing.xs },
  subject: { color: colors.textMuted, flex: 1, fontSize: 14 },
  read: { color: colors.success, fontSize: 12, fontWeight: "700" },
  more: { marginVertical: spacing.lg },
});
