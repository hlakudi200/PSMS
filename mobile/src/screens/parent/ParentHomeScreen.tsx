import { useCallback, useEffect } from "react";
import { FlatList, Pressable, RefreshControl, StyleSheet, Text, View } from "react-native";
import { router } from "expo-router";
import { Badge, Button, Card, Screen, StateView } from "../../components";
import { colors, spacing, typography } from "../../theme";
import { useAuthActions, useAuthState } from "../../providers/auth";
import { useChildrenActions, useChildrenState } from "../../providers/children";
import type { IChildSummary } from "../../providers/children/context";
import { formatDate, formatPercentage } from "../../utils/assessment-labels";

export function ParentHomeScreen() {
  const { currentUser } = useAuthState();
  const { signOut } = useAuthActions();
  const { myChildren, selectedChildId, isPending, isError, unreadAnnouncements, unreadNotifications } = useChildrenState();
  const { getMyChildrenAsync, selectChild } = useChildrenActions();

  useEffect(() => { void getMyChildrenAsync(); }, [getMyChildrenAsync]);

  const handleSignOut = useCallback(async () => {
    await signOut();
    router.dismissAll();
    router.replace("/(auth)/login");
  }, [signOut]);

  if (isPending && !myChildren) {
    return <Screen><StateView state="loading" message="Loading your family…" /></Screen>;
  }
  if (isError) {
    return (
      <Screen>
        <StateView state="error" message="Couldn't load your children." />
        <Button onPress={() => void getMyChildrenAsync()}>Retry</Button>
      </Screen>
    );
  }

  return (
    <Screen>
      <FlatList
        data={myChildren}
        keyExtractor={(child) => child.studentId}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={isPending} onRefresh={() => void getMyChildrenAsync()} />}
        ListHeaderComponent={
          <View>
            <Badge label="PARENT PORTAL" />
            <Text style={styles.title}>Welcome{currentUser?.name ? `, ${currentUser.name}` : ""}</Text>
            <UnreadSummary announcements={unreadAnnouncements} notifications={unreadNotifications} />
          </View>
        }
        ListEmptyComponent={
          <StateView state="empty" message="No children are linked to your account yet. Your school office can link them." />
        }
        renderItem={({ item }) => (
          <ChildCard
            child={item}
            isSelected={item.studentId === selectedChildId}
            onPress={() => selectChild(item.studentId)}
          />
        )}
        ListFooterComponent={
          <View style={styles.footer}>
            <Button onPress={() => void handleSignOut()}>Sign out</Button>
          </View>
        }
      />
    </Screen>
  );
}

function UnreadSummary({ announcements, notifications }: { announcements?: number; notifications?: number }) {
  const hasAnnouncements = !!announcements;
  const hasNotifications = !!notifications;
  if (!hasAnnouncements && !hasNotifications) return null;

  return (
    <View style={styles.unreadRow}>
      {hasAnnouncements && <Badge label={`${announcements} unread announcement${announcements === 1 ? "" : "s"}`} tone="warning" />}
      {hasNotifications && <Badge label={`${notifications} unread notification${notifications === 1 ? "" : "s"}`} tone="warning" />}
    </View>
  );
}

function ChildCard({ child, isSelected, onPress }: { child: IChildSummary; isSelected: boolean; onPress: () => void }) {
  const classLine = [child.gradeName, child.className].filter(Boolean).join(" · ");

  return (
    <Pressable accessibilityRole="button" onPress={onPress} style={styles.cardWrapper}>
      <Card style={isSelected && styles.selectedCard}>
        <View style={styles.cardTop}>
          <Text style={styles.childName} numberOfLines={2}>{child.studentName}</Text>
          {isSelected && <Badge label="Selected" tone="success" />}
        </View>
        {!!classLine && <Text style={styles.classLine}>{classLine}</Text>}

        <View style={styles.metric}>
          <Text style={styles.metricLabel}>Attendance this term</Text>
          {child.attendancePercentage != null ? (
            <Text style={styles.metricValue}>
              {formatPercentage(child.attendancePercentage)}
              {child.attendanceTotalDays ? (
                <Text style={styles.metricDetail}>{`  ${child.attendanceDaysPresent ?? 0}/${child.attendanceTotalDays} days`}</Text>
              ) : null}
            </Text>
          ) : (
            <Text style={styles.metricEmpty}>Not available yet</Text>
          )}
        </View>

        <View style={styles.metric}>
          <Text style={styles.metricLabel}>Latest report card</Text>
          {child.latestReport ? (
            <Text style={styles.metricValue}>
              {child.latestReport.termName ?? "Report"}
              {child.latestReport.overallPercentage != null
                ? <Text style={styles.metricDetail}>{`  ${formatPercentage(child.latestReport.overallPercentage)}`}</Text>
                : null}
              {child.latestReport.publishedDate
                ? <Text style={styles.metricDetail}>{`  published ${formatDate(child.latestReport.publishedDate)}`}</Text>
                : null}
            </Text>
          ) : (
            <Text style={styles.metricEmpty}>None published yet</Text>
          )}
        </View>
      </Card>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  title: { color: colors.text, fontSize: typography.title, fontWeight: "800", marginTop: spacing.lg },
  list: { paddingBottom: spacing.xxl },
  unreadRow: { flexDirection: "row", flexWrap: "wrap", gap: spacing.sm, marginTop: spacing.md, marginBottom: spacing.lg },
  cardWrapper: { marginBottom: spacing.md },
  selectedCard: { borderColor: colors.primary, borderWidth: 2 },
  cardTop: { alignItems: "center", flexDirection: "row", gap: spacing.sm, justifyContent: "space-between" },
  childName: { color: colors.text, flex: 1, fontSize: 18, fontWeight: "800" },
  classLine: { color: colors.textMuted, fontSize: 14, marginTop: spacing.xs },
  metric: { marginTop: spacing.md },
  metricLabel: { color: colors.textMuted, fontSize: 13, fontWeight: "700" },
  metricValue: { color: colors.text, fontSize: 16, fontWeight: "700", marginTop: spacing.xs },
  metricDetail: { color: colors.textMuted, fontSize: 13, fontWeight: "400" },
  metricEmpty: { color: colors.textMuted, fontSize: 15, marginTop: spacing.xs },
  footer: { marginTop: spacing.lg },
});
