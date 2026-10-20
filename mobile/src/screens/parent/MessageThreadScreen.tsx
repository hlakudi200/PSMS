import { useEffect, useRef, useState } from "react";
import { KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, TextInput, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { Button, StateView } from "../../components";
import { colors, radius, spacing, typography } from "../../theme";
import { useAuthState } from "../../providers/auth";
import { useMessagesActions, useMessagesState } from "../../providers/messages";
import type { IMessage } from "../../providers/messages/context";
import { formatMessageTime, MAX_CONTENT_LENGTH, replySchema } from "../../utils/message-forms";

export function MessageThreadScreen({ threadId }: { threadId?: string }) {
  const { currentUser } = useAuthState();
  const { thread, threadId: loadedThreadId, isThreadError, isSending, sendErrorMessage } = useMessagesState();
  const { openThreadAsync, replyAsync, clearSendError } = useMessagesActions();
  const [reply, setReply] = useState("");
  const [validationMessage, setValidationMessage] = useState<string>();
  const scrollRef = useRef<ScrollView>(null);
  const userId = currentUser?.id;

  useEffect(() => {
    if (threadId && userId != null) void openThreadAsync(threadId, userId);
  }, [threadId, userId, openThreadAsync]);

  useEffect(() => { clearSendError(); }, [threadId, clearSendError]);

  const messages = loadedThreadId === threadId ? thread : undefined;

  if (!threadId || userId == null || (isThreadError && !messages)) {
    return (
      <SafeAreaView style={styles.safe}>
        <View style={styles.centred}>
          <StateView state="error" message="Couldn't open this conversation." />
          {!!threadId && userId != null && <Button onPress={() => void openThreadAsync(threadId, userId)}>Retry</Button>}
        </View>
      </SafeAreaView>
    );
  }
  if (!messages) {
    return <SafeAreaView style={styles.safe}><StateView state="loading" message="Loading conversation…" /></SafeAreaView>;
  }
  if (messages.length === 0) {
    return <SafeAreaView style={styles.safe}><StateView state="empty" message="This conversation is empty." /></SafeAreaView>;
  }

  const first = messages[0];
  const other = first.senderUserId === userId ? first.recipientName : first.senderName;

  const send = async () => {
    const parsed = replySchema.safeParse({ content: reply });
    if (!parsed.success) {
      setValidationMessage(parsed.error.issues[0]?.message);
      return;
    }
    setValidationMessage(undefined);
    if (await replyAsync(parsed.data.content, userId)) {
      setReply("");
      requestAnimationFrame(() => scrollRef.current?.scrollToEnd({ animated: true }));
    }
  };

  const error = validationMessage || sendErrorMessage;

  return (
    <SafeAreaView style={styles.safe} edges={["top", "bottom"]}>
      <KeyboardAvoidingView style={styles.safe} behavior={Platform.OS === "ios" ? "padding" : undefined}>
        <View style={styles.header}>
          <Text style={styles.title} numberOfLines={2} accessibilityRole="header">{first.subject ?? "Conversation"}</Text>
          {!!other && <Text style={styles.with}>With {other}</Text>}
        </View>

        <ScrollView
          ref={scrollRef}
          contentContainerStyle={styles.messages}
          onContentSizeChange={() => scrollRef.current?.scrollToEnd({ animated: false })}
        >
          {messages.map((message) => (
            <Bubble key={message.id} message={message} mine={message.senderUserId === userId} />
          ))}
        </ScrollView>

        <View style={styles.composer}>
          {!!error && <Text accessibilityRole="alert" style={styles.error}>{error}</Text>}
          <TextInput
            value={reply}
            onChangeText={(value) => { setReply(value); setValidationMessage(undefined); }}
            placeholder={other ? `Reply to ${other}` : "Write a reply"}
            multiline
            maxLength={MAX_CONTENT_LENGTH}
            editable={!isSending}
            accessibilityLabel="Reply"
            style={styles.input}
          />
          <Button disabled={isSending || !reply.trim()} onPress={() => void send()}>
            {isSending ? "Sending…" : "Send reply"}
          </Button>
        </View>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

function Bubble({ message, mine }: { message: IMessage; mine: boolean }) {
  return (
    <View style={[styles.bubbleRow, mine && styles.bubbleRowMine]}>
      <View style={[styles.bubble, mine ? styles.bubbleMine : styles.bubbleTheirs]}>
        {!mine && !!message.senderName && <Text style={styles.sender}>{message.senderName}</Text>}
        <Text style={[styles.content, mine && styles.contentMine]} selectable>{message.content}</Text>
        <Text style={[styles.meta, mine && styles.metaMine]}>
          {formatMessageTime(message.creationTime)}{mine && message.isRead ? " · Read" : ""}
        </Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: colors.background },
  centred: { flex: 1, justifyContent: "center", padding: spacing.xxl },
  header: { borderBottomColor: colors.border, borderBottomWidth: 1, paddingHorizontal: spacing.xxl, paddingVertical: spacing.lg },
  title: { color: colors.text, fontSize: typography.body + 4, fontWeight: "800" },
  with: { color: colors.textMuted, fontSize: 14, marginTop: spacing.xs },
  messages: { gap: spacing.md, padding: spacing.lg },
  bubbleRow: { flexDirection: "row" },
  bubbleRowMine: { justifyContent: "flex-end" },
  bubble: { borderRadius: radius.md, maxWidth: "85%", padding: spacing.md },
  bubbleMine: { backgroundColor: colors.primary },
  bubbleTheirs: { backgroundColor: colors.surface, borderColor: colors.border, borderWidth: 1 },
  sender: { color: colors.primaryDark, fontSize: 12, fontWeight: "800", marginBottom: spacing.xs },
  content: { color: colors.text, fontSize: 15, lineHeight: 21 },
  contentMine: { color: colors.surface },
  meta: { color: colors.textMuted, fontSize: 11, marginTop: spacing.xs },
  metaMine: { color: colors.heroText },
  composer: {
    backgroundColor: colors.surface, borderTopColor: colors.border, borderTopWidth: 1, gap: spacing.sm, padding: spacing.lg,
  },
  error: { color: colors.danger, fontSize: 14 },
  input: {
    backgroundColor: colors.surface, borderColor: colors.border, borderRadius: radius.sm, borderWidth: 1, color: colors.text,
    fontSize: 16, maxHeight: 140, minHeight: 48, paddingHorizontal: spacing.md, paddingVertical: spacing.sm,
  },
});
