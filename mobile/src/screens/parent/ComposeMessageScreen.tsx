import { useEffect, useState } from "react";
import { KeyboardAvoidingView, Platform, Pressable, ScrollView, StyleSheet, Text, TextInput, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { router } from "expo-router";
import { Badge, Button, Card, StateView } from "../../components";
import { colors, radius, spacing, typography } from "../../theme";
import { useMessagesActions, useMessagesState } from "../../providers/messages";
import { composeSchema, MAX_CONTENT_LENGTH, MAX_SUBJECT_LENGTH } from "../../utils/message-forms";

export function ComposeMessageScreen() {
  const { recipients, isRecipientsError, isSending, sendErrorMessage } = useMessagesState();
  const { getRecipientsAsync, sendAsync, clearSendError } = useMessagesActions();
  const [recipientUserId, setRecipientUserId] = useState<number>();
  const [subject, setSubject] = useState("");
  const [content, setContent] = useState("");
  const [validationMessage, setValidationMessage] = useState<string>();

  useEffect(() => {
    clearSendError();
    void getRecipientsAsync();
  }, [getRecipientsAsync, clearSendError]);

  if (isRecipientsError && !recipients) {
    return (
      <SafeAreaView style={styles.safe}>
        <View style={styles.centred}>
          <StateView state="error" message="Couldn't load your children's teachers." />
          <Button onPress={() => void getRecipientsAsync()}>Retry</Button>
        </View>
      </SafeAreaView>
    );
  }
  if (!recipients) {
    return <SafeAreaView style={styles.safe}><StateView state="loading" message="Loading your children's teachers…" /></SafeAreaView>;
  }
  if (recipients.length === 0) {
    return (
      <SafeAreaView style={styles.safe}>
        <StateView
          state="empty"
          message="No teachers are linked to your children's classes yet. Your school office can help."
        />
      </SafeAreaView>
    );
  }

  const submit = async () => {
    const parsed = composeSchema.safeParse({ recipientUserId, subject, content });
    if (!parsed.success) {
      setValidationMessage(parsed.error.issues[0]?.message);
      return;
    }
    setValidationMessage(undefined);
    const threadId = await sendAsync(parsed.data);
    if (threadId) router.replace({ pathname: "/(parent)/message/[threadId]", params: { threadId } });
  };

  const error = validationMessage || sendErrorMessage;

  return (
    <SafeAreaView style={styles.safe}>
      <KeyboardAvoidingView style={styles.safe} behavior={Platform.OS === "ios" ? "padding" : undefined}>
        <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
          <Badge label="NEW MESSAGE" />
          <Text style={styles.title}>Write to a teacher</Text>

          <Text style={styles.label}>To</Text>
          {recipients.map((recipient) => {
            const selected = recipient.userId === recipientUserId;
            return (
              <Pressable
                key={recipient.userId}
                accessibilityRole="radio"
                accessibilityState={{ selected }}
                accessibilityLabel={`${recipient.name}, ${recipient.roles.join("; ")}`}
                onPress={() => { setRecipientUserId(recipient.userId); setValidationMessage(undefined); }}
                style={styles.recipientWrapper}
              >
                <Card style={selected ? styles.selected : undefined}>
                  <View style={styles.recipientTop}>
                    <Text style={styles.name}>{recipient.name}</Text>
                    {selected && <Badge label="Selected" tone="success" />}
                  </View>
                  {recipient.roles.map((role) => <Text key={role} style={styles.role}>{role}</Text>)}
                </Card>
              </Pressable>
            );
          })}

          <Text style={styles.label}>Subject (optional)</Text>
          <TextInput
            value={subject}
            onChangeText={setSubject}
            placeholder="e.g. Homework this week"
            maxLength={MAX_SUBJECT_LENGTH}
            editable={!isSending}
            accessibilityLabel="Subject"
            style={styles.input}
          />

          <Text style={styles.label}>Message</Text>
          <TextInput
            value={content}
            onChangeText={(value) => { setContent(value); setValidationMessage(undefined); }}
            placeholder="Write your message"
            multiline
            textAlignVertical="top"
            maxLength={MAX_CONTENT_LENGTH}
            editable={!isSending}
            accessibilityLabel="Message"
            style={[styles.input, styles.body]}
          />

          {!!error && <Text accessibilityRole="alert" style={styles.error}>{error}</Text>}
          <View style={styles.send}>
            <Button disabled={isSending} onPress={() => void submit()}>{isSending ? "Sending…" : "Send"}</Button>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: colors.background },
  centred: { flex: 1, justifyContent: "center", padding: spacing.xxl },
  content: { padding: spacing.xxl, paddingBottom: spacing.xxl * 2 },
  title: { color: colors.text, fontSize: typography.heading, fontWeight: "800", marginTop: spacing.lg },
  label: { color: colors.text, fontSize: 13, fontWeight: "700", marginBottom: spacing.xs, marginTop: spacing.xl },
  recipientWrapper: { marginTop: spacing.sm },
  selected: { borderColor: colors.primary, borderWidth: 2 },
  recipientTop: { alignItems: "center", flexDirection: "row", gap: spacing.sm, justifyContent: "space-between" },
  name: { color: colors.text, flex: 1, fontSize: 16, fontWeight: "800" },
  role: { color: colors.textMuted, fontSize: 13, marginTop: spacing.xs },
  input: {
    backgroundColor: colors.surface, borderColor: colors.border, borderRadius: radius.sm, borderWidth: 1,
    color: colors.text, fontSize: 16, paddingHorizontal: spacing.md, paddingVertical: spacing.md,
  },
  body: { minHeight: 160 },
  error: { color: colors.danger, fontSize: 14, marginTop: spacing.md },
  send: { marginTop: spacing.lg },
});
