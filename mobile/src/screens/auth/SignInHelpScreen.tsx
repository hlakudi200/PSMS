import { Pressable, StyleSheet, Text, View } from "react-native";

export function SignInHelpScreen({ onBack }: { onBack: () => void }) {
  return <View style={styles.container}><Text style={styles.overline}>ACCOUNT HELP</Text><Text style={styles.title}>Need help signing in?</Text><Text style={styles.description}>Contact your school office if you do not know your school name, username, or password. They can verify your account and reset access when needed.</Text><View style={styles.notice}><Text style={styles.noticeTitle}>For your security</Text><Text style={styles.noticeCopy}>Passwords are not reset from the mobile app. Your school manages account recovery.</Text></View><Pressable accessibilityRole="button" onPress={onBack} style={styles.button}><Text style={styles.buttonText}>Back to sign in</Text></Pressable></View>;
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#f5f7fa", justifyContent: "center", padding: 28 }, overline: { color: "#0066cc", fontSize: 12, fontWeight: "800", letterSpacing: 1.3 }, title: { color: "#262626", fontSize: 29, fontWeight: "800", lineHeight: 36, marginTop: 10 }, description: { color: "#595959", fontSize: 16, lineHeight: 24, marginTop: 14 }, notice: { backgroundColor: "#e6f4ff", borderColor: "#91caff", borderRadius: 8, borderWidth: 1, marginTop: 24, padding: 16 }, noticeTitle: { color: "#003d73", fontSize: 15, fontWeight: "800" }, noticeCopy: { color: "#434343", fontSize: 14, lineHeight: 21, marginTop: 5 }, button: { alignItems: "center", backgroundColor: "#0066cc", borderRadius: 7, marginTop: 28, padding: 14 }, buttonText: { color: "#fff", fontSize: 16, fontWeight: "800" },
});
