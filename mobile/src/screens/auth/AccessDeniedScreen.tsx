import { Pressable, StyleSheet, Text, View } from "react-native";

export function AccessDeniedScreen({ onBack }: { onBack: () => void }) {
  return <View style={styles.container}><View style={styles.icon}><Text style={styles.iconText}>!</Text></View><Text style={styles.title}>Mobile access unavailable</Text><Text style={styles.description}>PSMS Mobile is available to Students and Parents. Please use the web application for this account.</Text><Pressable accessibilityRole="button" onPress={onBack} style={styles.button}><Text style={styles.buttonText}>Back to sign in</Text></Pressable></View>;
}

const styles = StyleSheet.create({
  container: { flex: 1, alignItems: "center", backgroundColor: "#f5f7fa", justifyContent: "center", padding: 28 }, icon: { alignItems: "center", backgroundColor: "#fff1f0", borderRadius: 36, height: 72, justifyContent: "center", width: 72 }, iconText: { color: "#cf1322", fontSize: 38, fontWeight: "800" }, title: { color: "#262626", fontSize: 25, fontWeight: "800", marginTop: 24, textAlign: "center" }, description: { color: "#595959", fontSize: 16, lineHeight: 24, marginTop: 12, textAlign: "center" }, button: { backgroundColor: "#0066cc", borderRadius: 7, marginTop: 30, paddingHorizontal: 24, paddingVertical: 14 }, buttonText: { color: "#fff", fontSize: 15, fontWeight: "800" },
});
