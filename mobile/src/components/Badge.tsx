import { StyleSheet, Text, View } from "react-native";
import { colors, radius, spacing } from "../theme";

export function Badge({ label }: { label: string }) { return <View style={styles.badge}><Text style={styles.label}>{label}</Text></View>; }
const styles = StyleSheet.create({ badge: { alignSelf: "flex-start", backgroundColor: colors.primaryLight, borderRadius: radius.pill, paddingHorizontal: spacing.sm, paddingVertical: spacing.xs }, label: { color: colors.primaryDark, fontSize: 12, fontWeight: "700" } });
