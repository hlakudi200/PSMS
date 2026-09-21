import type { ReactNode } from "react";
import { Pressable, StyleSheet, Text } from "react-native";
import { colors, radius, spacing } from "../theme";

export function Button({ children, onPress, disabled = false }: { children: ReactNode; onPress: () => void; disabled?: boolean }) {
  return <Pressable accessibilityRole="button" disabled={disabled} onPress={onPress} style={[styles.button, disabled && styles.disabled]}><Text style={styles.text}>{children}</Text></Pressable>;
}
const styles = StyleSheet.create({ button: { alignItems: "center", backgroundColor: colors.primary, borderRadius: radius.sm, padding: spacing.lg }, disabled: { opacity: 0.6 }, text: { color: colors.surface, fontSize: 16, fontWeight: "800" } });
