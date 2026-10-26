import { useCallback, useMemo } from "react";
import { ActivityIndicator, Pressable, RefreshControl, SectionList, StyleSheet, Text, View } from "react-native";
import { router, useFocusEffect } from "expo-router";
import { Badge, Button, Card, Screen, StateView } from "../../components";
import { colors, spacing, typography } from "../../theme";
import { useNotificationsActions, useNotificationsState } from "../../providers/notifications";
import type { INotification } from "../../providers/notifications/context";
import { formatTime, groupByDay, NOTIFICATION_TYPE_LABELS, notificationTarget } from "../../utils/notification-centre";

export function NotificationCentreScreen() {
  const { notifications, isPending, isError, isLoadingMore, unreadCount, isMarkingAll, actionErrorMessage } =
    useNotificationsState();
  const { getNotificationsAsync, loadMoreAsync, markAsReadAsync, markAllAsReadAsync } = useNotificationsActions();

  // Again on returning from a report card, so anything new since is there.
  useFocusEffect(useCallback(() => { void getNotificationsAsync(); }, [getNotificationsAsync]));

  const sections = useMemo(() => groupByDay(notifications ?? []), [notifications]);

  const open = (notification: INotification) => {
    void markAsReadAsync(notification.id);
    const target = notificationTarget(notification.entityType, notification.entityId);
    if (target) router.push(target);
  };

  const header = (
    <View>
      <Badge label="NOTIFICATIONS" />
      <Text style={styles.title}>Notifications</Text>
      <Text style={styles.subtitle}>{unreadCount ? `${unreadCount} unread` : "You're up to date"}</Text>
      <View style={styles.actions}>
        {!!unreadCount && (
          <View style={styles.action}>
            <Button disabled={isMarkingAll} onPress={() => void markAllAsReadAsync()}>
              {isMarkingAll ? "Marking…" : "Mark all as read"}
            </Button>
          </View>
        )}
        <Pressable
          accessibilityRole="link"
          onPress={() => router.push("/(parent)/notifications")}
          style={styles.settings}
        >
          <Text style={styles.settingsText}>Notification settings</Text>
        </Pressable>
      </View>
      {!!actionErrorMessage && <Text accessibilityRole="alert" style={styles.inlineError}>{actionErrorMessage}</Text>}
      {isError && !!notifications && (
        <Text style={styles.inlineError}>Couldn&apos;t refresh — showing what loaded earlier.</Text>
      )}
    </View>
  );

  if (!notifications) {
    return (
      <Screen>
        {header}
        {isError ? (
          <>
            <StateView state="error" message="Couldn't load your notifications." />
            <Button onPress={() => void getNotificationsAsync()}>Retry</Button>
          </>
        ) : (
          <StateView state="loading" message="Loading notifications…" />
        )}
      </Screen>
    );
  }

  return (
    <Screen>
      <SectionList
        sections={sections}
        keyExtractor={(item) => item.id}
        contentContainerStyle={styles.list}
        stickySectionHeadersEnabled={false}
        refreshControl={<RefreshControl refreshing={isPending} onRefresh={() => void getNotificationsAsync()} />}
        onEndReached={() => void loadMoreAsync()}
        onEndReachedThreshold={0.4}
        ListHeaderComponent={header}
        ListEmptyComponent={
          <StateView state="empty" message="No notifications yet. The school's news about your children will appear here." />
        }
        ListFooterComponent={isLoadingMore ? <ActivityIndicator style={styles.more} color={colors.primary} /> : null}
        renderSectionHeader={({ section }) => (
          <Text style={styles.day} accessibilityRole="header">{section.title}</Text>
        )}
        renderItem={({ item }) => <NotificationRow notification={item} onPress={() => open(item)} />}
      />
    </Screen>
  );
}

function NotificationRow({ notification, onPress }: { notification: INotification; onPress: () => void }) {
  const unread = !notification.isRead;
  const opens = !!notificationTarget(notification.entityType, notification.entityId);
  const urgent = notification.priority >= 3;

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={`${unread ? "Unread. " : ""}${notification.title}. ${notification.message}`}
      accessibilityHint={opens ? "Opens what it is about" : unread ? "Marks it as read" : undefined}
      onPress={onPress}
      style={styles.cardWrapper}
    >
      <Card style={unread ? styles.unreadCard : undefined}>
        <View style={styles.rowTop}>
          <Text style={styles.type}>{NOTIFICATION_TYPE_LABELS[notification.type] ?? "Notification"}</Text>
          <Text style={styles.time}>{formatTime(notification.creationTime)}</Text>
        </View>
        <Text style={[styles.rowTitle, unread && styles.rowTitleUnread]}>{notification.title}</Text>
        <Text style={styles.message}>{notification.message}</Text>
        {(unread || urgent || opens) && (
          <View style={styles.badges}>
            {unread && <Badge label="New" tone="success" />}
            {urgent && <Badge label="Important" tone="warning" />}
            {opens && <Text style={styles.opens}>Open ›</Text>}
          </View>
        )}
      </Card>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  title: { color: colors.text, fontSize: typography.title, fontWeight: "800", marginTop: spacing.lg },
  subtitle: { color: colors.textMuted, fontSize: 14, marginTop: spacing.xs },
  actions: { alignItems: "center", flexDirection: "row", flexWrap: "wrap", gap: spacing.md, marginTop: spacing.lg },
  action: { flexGrow: 1 },
  settings: { paddingVertical: spacing.sm },
  settingsText: { color: colors.primary, fontSize: 14, fontWeight: "700" },
  inlineError: { color: colors.danger, fontSize: 14, marginTop: spacing.md },
  list: { paddingBottom: spacing.xxl },
  day: { color: colors.textMuted, fontSize: 12, fontWeight: "800", letterSpacing: 1, marginTop: spacing.xl, textTransform: "uppercase" },
  cardWrapper: { marginTop: spacing.sm },
  unreadCard: { borderColor: colors.primary },
  rowTop: { alignItems: "center", flexDirection: "row", gap: spacing.sm, justifyContent: "space-between" },
  type: { color: colors.primaryDark, fontSize: 12, fontWeight: "800" },
  time: { color: colors.textMuted, fontSize: 12 },
  rowTitle: { color: colors.text, fontSize: 16, fontWeight: "600", marginTop: spacing.xs },
  rowTitleUnread: { fontWeight: "800" },
  message: { color: colors.textMuted, fontSize: 14, lineHeight: 20, marginTop: spacing.xs },
  badges: { alignItems: "center", flexDirection: "row", gap: spacing.sm, marginTop: spacing.sm },
  opens: { color: colors.primary, fontSize: 13, fontWeight: "800", marginLeft: "auto" },
  more: { marginVertical: spacing.lg },
});
