import { StyleSheet, Text, View } from "react-native";
import { colors, radius, spacing } from "../theme";

export type BadgeTone = "neutral" | "success" | "warning" | "danger" | "muted";

const TONES: Record<BadgeTone, { background: string; text: string }> = {
  neutral: { background: colors.primaryLight, text: colors.primaryDark },
  success: { background: colors.successBackground, text: colors.success },
  warning: { background: colors.warningBackground, text: colors.warning },
  danger: { background: colors.dangerBackground, text: colors.danger },
  muted: { background: colors.mutedBackground, text: colors.textMuted },
};

export function Badge({ label, tone = "neutral" }: { label: string; tone?: BadgeTone }) {
  const t = TONES[tone];
  return (
    <View style={[styles.badge, { backgroundColor: t.background }]}>
      <Text style={[styles.label, { color: t.text }]}>{label}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  badge: { alignSelf: "flex-start", borderRadius: radius.pill, paddingHorizontal: spacing.sm, paddingVertical: spacing.xs },
  label: { fontSize: 12, fontWeight: "700" },
});
