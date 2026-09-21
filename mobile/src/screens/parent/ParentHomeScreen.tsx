import { StyleSheet, Text } from "react-native";
import { router } from "expo-router";
import { Badge, Button, Card, Screen } from "../../components";
import { colors, spacing, typography } from "../../theme";
import { useAuthActions, useAuthState } from "../../providers/auth";

export function ParentHomeScreen() {
  const { currentUser } = useAuthState();
  const { signOut } = useAuthActions();
  const handleSignOut = async () => { await signOut(); router.dismissAll(); router.replace("/(auth)/login"); };
  return <Screen><Badge label="PARENT PORTAL" /><Text style={styles.title}>Welcome{currentUser?.name ? `, ${currentUser.name}` : ""}</Text><Text style={styles.copy}>Keep up with your children’s school life in one place.</Text><Card><Text style={styles.cardTitle}>Getting started</Text><Text style={styles.cardCopy}>Use the tabs below to manage family information, fees, and messages as features become available.</Text></Card><Button onPress={() => void handleSignOut()}>Sign out</Button></Screen>;
}

const styles = StyleSheet.create({
  title: { color: colors.text, fontSize: typography.title, fontWeight: "800", marginTop: spacing.lg }, copy: { color: colors.textMuted, fontSize: typography.body, lineHeight: 24, marginTop: spacing.sm, marginBottom: spacing.xl }, cardTitle: { color: colors.text, fontSize: 18, fontWeight: "800" }, cardCopy: { color: colors.textMuted, fontSize: 15, lineHeight: 22, marginTop: spacing.sm },
});
