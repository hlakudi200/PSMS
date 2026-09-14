import { ActivityIndicator, StyleSheet, Text, View } from "react-native";

export function AuthLoadingScreen() {
  return <View style={styles.container}>
    <View style={styles.mark}><Text style={styles.markText}>PSMS</Text></View>
    <ActivityIndicator size="large" color="#0066cc" />
    <Text style={styles.message}>Restoring your secure session…</Text>
  </View>;
}

const styles = StyleSheet.create({
  container: { flex: 1, alignItems: "center", justifyContent: "center", backgroundColor: "#f5f7fa", gap: 18 },
  mark: { width: 78, height: 78, alignItems: "center", justifyContent: "center", borderRadius: 18, backgroundColor: "#003d73" },
  markText: { color: "#fff", fontWeight: "800", fontSize: 21, letterSpacing: 1.5 },
  message: { color: "#595959", fontSize: 15 },
});
