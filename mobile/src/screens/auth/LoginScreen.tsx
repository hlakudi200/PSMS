import { useState } from "react";
import { KeyboardAvoidingView, Platform, Pressable, ScrollView, StyleSheet, Text, TextInput, View } from "react-native";
import { z } from "zod";
import { useAuthActions, useAuthState } from "../../providers/auth";

const loginSchema = z.object({
  tenancyName: z.string().trim().min(1, "Enter your school name."),
  userNameOrEmailAddress: z.string().trim().min(1, "Enter your username or email."),
  password: z.string().min(1, "Enter your password."),
});

export function LoginScreen({ onNeedHelp }: { onNeedHelp: () => void }) {
  const { isPending, isError, errorMessage } = useAuthState();
  const { loginUser, resetStateFlags } = useAuthActions();
  const [tenancyName, setTenancyName] = useState("Default");
  const [userNameOrEmailAddress, setUserNameOrEmailAddress] = useState("");
  const [password, setPassword] = useState("");
  const [validationMessage, setValidationMessage] = useState<string>();

  const submit = async () => {
    resetStateFlags();
    const parsed = loginSchema.safeParse({ tenancyName, userNameOrEmailAddress, password });
    if (!parsed.success) return setValidationMessage(parsed.error.issues[0]?.message);
    setValidationMessage(undefined);
    await loginUser(parsed.data);
  };

  return <KeyboardAvoidingView style={styles.container} behavior={Platform.OS === "ios" ? "padding" : undefined}>
    <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">
      <View style={styles.hero}><Text style={styles.heroMark}>PSMS</Text><Text style={styles.heroTitle}>Your school, in your pocket.</Text><Text style={styles.heroCopy}>Stay connected to learning, progress, and school life.</Text></View>
      <View style={styles.card}>
        <Text style={styles.overline}>STUDENT & PARENT PORTAL</Text><Text style={styles.title}>Welcome back</Text><Text style={styles.subtitle}>Sign in to continue to PSMS.</Text>
        <Text style={styles.label}>School name</Text><TextInput value={tenancyName} onChangeText={setTenancyName} placeholder="School name" autoCapitalize="none" editable={!isPending} style={styles.input} />
        <Text style={styles.label}>Username or email</Text><TextInput value={userNameOrEmailAddress} onChangeText={setUserNameOrEmailAddress} placeholder="Username or email" autoCapitalize="none" autoComplete="username" editable={!isPending} style={styles.input} />
        <Text style={styles.label}>Password</Text><TextInput value={password} onChangeText={setPassword} placeholder="Password" secureTextEntry autoComplete="current-password" editable={!isPending} style={styles.input} onSubmitEditing={() => void submit()} />
        {(validationMessage || (isError && errorMessage)) && <Text accessibilityRole="alert" style={styles.error}>{validationMessage || errorMessage}</Text>}
        <Pressable accessibilityRole="button" disabled={isPending} onPress={() => void submit()} style={[styles.button, isPending && styles.disabled]}><Text style={styles.buttonText}>{isPending ? "Signing in…" : "Sign in"}</Text></Pressable>
        <Pressable accessibilityRole="button" onPress={onNeedHelp} style={styles.helpButton}><Text style={styles.helpText}>Need help signing in?</Text></Pressable>
      </View>
      <Text style={styles.footer}>Private School Management System</Text>
    </ScrollView>
  </KeyboardAvoidingView>;
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: "#f5f7fa" }, scroll: { flexGrow: 1 },
  hero: { backgroundColor: "#003d73", paddingHorizontal: 28, paddingTop: 52, paddingBottom: 42 }, heroMark: { color: "#9fd4ff", fontSize: 14, fontWeight: "800", letterSpacing: 2 }, heroTitle: { color: "#fff", fontSize: 30, fontWeight: "800", lineHeight: 37, marginTop: 12, maxWidth: 290 }, heroCopy: { color: "#d6eaff", fontSize: 15, lineHeight: 22, marginTop: 10, maxWidth: 300 },
  card: { backgroundColor: "#fff", borderRadius: 16, elevation: 2, marginHorizontal: 20, marginTop: -18, padding: 24, shadowColor: "#001529", shadowOpacity: 0.12, shadowOffset: { width: 0, height: 3 }, shadowRadius: 12 }, overline: { color: "#0066cc", fontSize: 11, fontWeight: "800", letterSpacing: 1.2 }, title: { color: "#262626", fontSize: 26, fontWeight: "800", marginTop: 7 }, subtitle: { color: "#737373", fontSize: 15, marginTop: 5, marginBottom: 24 }, label: { color: "#434343", fontSize: 13, fontWeight: "700", marginBottom: 7, marginTop: 14 }, input: { backgroundColor: "#fff", borderColor: "#cbd5e1", borderWidth: 1, borderRadius: 7, color: "#262626", fontSize: 16, paddingHorizontal: 13, paddingVertical: 12 }, error: { color: "#cf1322", fontSize: 13, lineHeight: 19, marginTop: 16 }, button: { alignItems: "center", backgroundColor: "#0066cc", borderRadius: 7, marginTop: 24, paddingVertical: 14 }, disabled: { opacity: 0.6 }, buttonText: { color: "#fff", fontSize: 16, fontWeight: "800" }, helpButton: { alignItems: "center", marginTop: 20, padding: 6 }, helpText: { color: "#0066cc", fontSize: 14, fontWeight: "700" }, footer: { color: "#8c8c8c", fontSize: 12, marginVertical: 28, textAlign: "center" },
});
