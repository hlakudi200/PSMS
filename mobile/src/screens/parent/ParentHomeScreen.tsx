import { Pressable, StyleSheet, Text, View } from "react-native";
import { useAuthActions, useAuthState } from "../../providers/auth";

export function ParentHomeScreen() {
  const { currentUser } = useAuthState();
  const { signOut } = useAuthActions();
  return <View style={styles.container}><Text style={styles.overline}>PARENT PORTAL</Text><Text style={styles.title}>Welcome{currentUser?.name ? `, ${currentUser.name}` : ""}</Text><Text style={styles.copy}>Your family dashboard will appear here.</Text><Pressable accessibilityRole="button" onPress={() => void signOut()} style={styles.signOut}><Text style={styles.signOutText}>Sign out</Text></Pressable></View>;
}

const styles = StyleSheet.create({
  container: { flex: 1, justifyContent: "center", padding: 28 }, overline: { color: "#0066cc", fontSize: 12, fontWeight: "800", letterSpacing: 1.3 }, title: { color: "#262626", fontSize: 30, fontWeight: "800", marginTop: 10 }, copy: { color: "#595959", fontSize: 16, marginTop: 10 }, signOut: { alignSelf: "flex-start", marginTop: 28, paddingVertical: 8 }, signOutText: { color: "#0066cc", fontSize: 16, fontWeight: "800" },
});
