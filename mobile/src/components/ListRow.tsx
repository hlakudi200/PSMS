import type { ReactNode } from "react";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { colors, spacing } from "../theme";

export function ListRow({ title, detail, onPress, accessory }: { title: string; detail?: string; onPress?: () => void; accessory?: ReactNode }) {
  return <Pressable accessibilityRole={onPress ? "button" : undefined} disabled={!onPress} onPress={onPress} style={styles.row}><View style={styles.copy}><Text style={styles.title}>{title}</Text>{detail && <Text style={styles.detail}>{detail}</Text>}</View>{accessory}</Pressable>;
}
const styles = StyleSheet.create({ row: { alignItems: "center", borderBottomColor: colors.border, borderBottomWidth: 1, flexDirection: "row", minHeight: 60, paddingVertical: spacing.sm }, copy: { flex: 1 }, title: { color: colors.text, fontSize: 16, fontWeight: "700" }, detail: { color: colors.textMuted, fontSize: 14, marginTop: spacing.xs } });
