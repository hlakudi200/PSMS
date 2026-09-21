import { ActivityIndicator, StyleSheet, Text, View } from "react-native";
import { colors, spacing } from "../theme";

export function StateView({ state, message }: { state: "loading" | "error" | "empty"; message: string }) {
  return <View style={styles.container}>{state === "loading" && <ActivityIndicator color={colors.primary} />}{state !== "loading" && <Text style={state === "error" ? styles.error : styles.message}>{message}</Text>}</View>;
}
const styles = StyleSheet.create({ container: { alignItems: "center", justifyContent: "center", minHeight: 180, padding: spacing.xl }, message: { color: colors.textMuted, fontSize: 16, textAlign: "center" }, error: { color: colors.danger, fontSize: 16, textAlign: "center" } });
