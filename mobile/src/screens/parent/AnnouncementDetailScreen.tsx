import { useEffect } from "react";
import { Linking, ScrollView, StyleSheet, Text, View } from "react-native";
import { Badge, Button, Card, Screen, StateView } from "../../components";
import { colors, spacing, typography } from "../../theme";
import { useAnnouncementsActions, useAnnouncementsState } from "../../providers/announcements";
import { formatDate } from "../../utils/assessment-labels";
import {
  ANNOUNCEMENT_TYPE_LABELS, isWebLink, PARENT_AUDIENCE_LABELS, PRIORITY_BADGES,
} from "../../utils/announcement-labels";

export function AnnouncementDetailScreen({ announcementId }: { announcementId?: string }) {
  const { detail, isDetailError } = useAnnouncementsState();
  const { openAnnouncementAsync } = useAnnouncementsActions();

  // Opening it is what marks it read.
  useEffect(() => {
    if (announcementId) void openAnnouncementAsync(announcementId);
  }, [announcementId, openAnnouncementAsync]);

  const announcement = detail?.id === announcementId ? detail : undefined;

  if (!announcementId || (isDetailError && !announcement)) {
    return (
      <Screen>
        <StateView state="error" message="Couldn't open this announcement. It may have been taken down." />
        {!!announcementId && <Button onPress={() => void openAnnouncementAsync(announcementId)}>Retry</Button>}
      </Screen>
    );
  }
  if (!announcement) {
    return <Screen><StateView state="loading" message="Loading announcement…" /></Screen>;
  }

  const priority = PRIORITY_BADGES[announcement.priority];
  const audience = PARENT_AUDIENCE_LABELS[announcement.targetAudience];
  const attachment = isWebLink(announcement.attachmentUrl) ? announcement.attachmentUrl : undefined;

  return (
    <Screen>
      <ScrollView contentContainerStyle={styles.content}>
        <View style={styles.badges}>
          <Badge label={ANNOUNCEMENT_TYPE_LABELS[announcement.type].toUpperCase()} />
          {announcement.isPinned && <Badge label="Pinned" tone="neutral" />}
          {priority && <Badge label={priority.label} tone={priority.tone} />}
        </View>
        <Text style={styles.title} accessibilityRole="header">{announcement.title}</Text>
        <Text style={styles.meta}>
          {[audience && `For ${audience.charAt(0).toLowerCase()}${audience.slice(1)}`, formatDate(announcement.publishDate)]
            .filter(Boolean)
            .join(" · ")}
        </Text>
        {!!announcement.expiryDate && (
          <Text style={styles.meta}>Showing until {formatDate(announcement.expiryDate)}</Text>
        )}

        <Card style={styles.body}>
          <Text style={styles.bodyText} selectable>{announcement.content}</Text>
        </Card>

        {attachment && (
          <View style={styles.attachment}>
            <Button onPress={() => void Linking.openURL(attachment)}>Open attachment</Button>
          </View>
        )}
      </ScrollView>
    </Screen>
  );
}

const styles = StyleSheet.create({
  content: { paddingBottom: spacing.xxl },
  badges: { flexDirection: "row", flexWrap: "wrap", gap: spacing.xs },
  title: { color: colors.text, fontSize: typography.heading, fontWeight: "800", marginTop: spacing.lg },
  meta: { color: colors.textMuted, fontSize: 14, marginTop: spacing.xs },
  body: { marginTop: spacing.lg },
  bodyText: { color: colors.text, fontSize: typography.body, lineHeight: 24 },
  attachment: { marginTop: spacing.lg },
});
