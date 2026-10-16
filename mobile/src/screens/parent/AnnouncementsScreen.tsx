import { useEffect, type ReactNode } from "react";
import { ActivityIndicator, FlatList, Pressable, RefreshControl, StyleSheet, Text, View } from "react-native";
import { router } from "expo-router";
import { Badge, Button, Card, Screen, StateView } from "../../components";
import { colors, spacing, typography } from "../../theme";
import { useAnnouncementsActions, useAnnouncementsState } from "../../providers/announcements";
import type { IAnnouncementSummary } from "../../providers/announcements/context";
import { formatDate } from "../../utils/assessment-labels";
import {
  ANNOUNCEMENT_TYPE_LABELS, PARENT_AUDIENCE_LABELS, PRIORITY_BADGES,
} from "../../utils/announcement-labels";

/** `switcher` is the Messages tab's Teachers / School news choice, shown in the header. */
export function AnnouncementsScreen({ switcher }: { switcher?: ReactNode }) {
  const { announcements, isPending, isError, isLoadingMore, unreadCount } = useAnnouncementsState();
  const { getAnnouncementsAsync, loadMoreAsync } = useAnnouncementsActions();

  useEffect(() => { void getAnnouncementsAsync(); }, [getAnnouncementsAsync]);

  if (isError && !announcements) {
    return (
      <Screen>
        {switcher}
        <StateView state="error" message="Couldn't load announcements." />
        <Button onPress={() => void getAnnouncementsAsync()}>Retry</Button>
      </Screen>
    );
  }
  if (!announcements) {
    return <Screen>{switcher}<StateView state="loading" message="Loading announcements…" /></Screen>;
  }

  return (
    <Screen>
      <FlatList
        data={announcements}
        keyExtractor={(item) => item.id}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={isPending} onRefresh={() => void getAnnouncementsAsync()} />}
        onEndReached={() => void loadMoreAsync()}
        onEndReachedThreshold={0.4}
        ListHeaderComponent={
          <View>
            <Badge label="SCHOOL NEWS" />
            <Text style={styles.title}>Announcements</Text>
            <Text style={styles.subtitle}>
              {unreadCount ? `${unreadCount} unread` : "You're up to date"}
            </Text>
            {switcher}
            {isError && <Text style={styles.inlineError}>Couldn&apos;t refresh — showing what loaded earlier.</Text>}
          </View>
        }
        ListEmptyComponent={<StateView state="empty" message="There are no announcements for you yet." />}
        ListFooterComponent={isLoadingMore ? <ActivityIndicator style={styles.more} color={colors.primary} /> : null}
        renderItem={({ item }) => (
          <AnnouncementRow
            announcement={item}
            onPress={() => router.push({ pathname: "/(parent)/announcement/[id]", params: { id: item.id } })}
          />
        )}
      />
    </Screen>
  );
}

function AnnouncementRow({ announcement, onPress }: { announcement: IAnnouncementSummary; onPress: () => void }) {
  const unread = !announcement.isReadByMe;
  const priority = PRIORITY_BADGES[announcement.priority];
  const audience = PARENT_AUDIENCE_LABELS[announcement.targetAudience];
  const meta = [ANNOUNCEMENT_TYPE_LABELS[announcement.type], audience, formatDate(announcement.publishDate)]
    .filter(Boolean)
    .join(" · ");

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={`${unread ? "Unread. " : ""}${announcement.isPinned ? "Pinned. " : ""}${announcement.title}`}
      onPress={onPress}
      style={styles.cardWrapper}
    >
      <Card style={unread ? styles.unreadCard : undefined}>
        <View style={styles.badges}>
          {announcement.isPinned && <Badge label="Pinned" tone="neutral" />}
          {priority && <Badge label={priority.label} tone={priority.tone} />}
          {unread && <Badge label="New" tone="success" />}
        </View>
        <Text style={[styles.rowTitle, unread && styles.rowTitleUnread]} numberOfLines={2}>{announcement.title}</Text>
        <Text style={styles.meta}>{meta}</Text>
      </Card>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  list: { paddingBottom: spacing.xxl },
  title: { color: colors.text, fontSize: typography.title, fontWeight: "800", marginTop: spacing.lg },
  subtitle: { color: colors.textMuted, fontSize: 14, marginTop: spacing.xs },
  inlineError: { color: colors.danger, fontSize: 14, marginTop: spacing.md },
  cardWrapper: { marginTop: spacing.md },
  unreadCard: { borderColor: colors.primary },
  badges: { flexDirection: "row", flexWrap: "wrap", gap: spacing.xs },
  rowTitle: { color: colors.text, fontSize: 16, fontWeight: "600", marginTop: spacing.sm },
  rowTitleUnread: { fontWeight: "800" },
  meta: { color: colors.textMuted, fontSize: 13, marginTop: spacing.xs },
  more: { marginVertical: spacing.lg },
});
