import { useEffect, useState } from "react";
import {
  KeyboardAvoidingView, Platform, RefreshControl, ScrollView, StyleSheet, Switch, Text, TextInput, View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { Badge, Button, Card, StateView } from "../../components";
import { colors, radius, spacing, typography } from "../../theme";
import {
  useNotificationSettingsActions, useNotificationSettingsState,
} from "../../providers/notification-settings";
import type { IMyContact } from "../../providers/notification-settings/context";
import { formatDate } from "../../utils/assessment-labels";
import {
  CATEGORIES, CATEGORY_LABELS, CHANNEL_LABELS, contactSchema, EXTERNAL_CHANNELS,
  type ExternalChannel,
} from "../../utils/notification-labels";

export function NotificationSettingsScreen() {
  const { isPending, isError, consents, contact, saveErrorMessage } = useNotificationSettingsState();
  const { loadAsync } = useNotificationSettingsActions();

  useEffect(() => { void loadAsync(); }, [loadAsync]);

  if (isError && !consents) {
    return (
      <SafeAreaView style={styles.safe}>
        <View style={styles.centred}>
          <StateView state="error" message="Couldn't load your notification settings." />
          <Button onPress={() => void loadAsync()}>Retry</Button>
        </View>
      </SafeAreaView>
    );
  }
  if (!consents || !contact) {
    return <SafeAreaView style={styles.safe}><StateView state="loading" message="Loading your settings…" /></SafeAreaView>;
  }

  return (
    <SafeAreaView style={styles.safe}>
      <KeyboardAvoidingView style={styles.safe} behavior={Platform.OS === "ios" ? "padding" : undefined}>
        <ScrollView
          contentContainerStyle={styles.content}
          keyboardShouldPersistTaps="handled"
          refreshControl={<RefreshControl refreshing={isPending} onRefresh={() => void loadAsync()} />}
        >
          <Badge label="NOTIFICATIONS" />
          <Text style={styles.title}>Notifications</Text>
          <Text style={styles.intro}>
            Everything the school sends arrives in the app&apos;s inbox. Choose which other ways the school
            may reach you, and what about.
          </Text>
          {isError && <Text style={styles.inlineError}>Couldn&apos;t refresh — showing what loaded earlier.</Text>}
          {!!saveErrorMessage && <Text accessibilityRole="alert" style={styles.inlineError}>{saveErrorMessage}</Text>}

          {EXTERNAL_CHANNELS.map((channel) => (
            <ChannelCard key={channel} channel={channel} contact={contact} />
          ))}

          <Text style={styles.consentNote}>
            Turning a channel on is your consent for the school to use it, and you can withdraw it here at any
            time. Once a channel is off, nothing is sent on it. Until you choose, the school may still use it for
            messages about your own application.
          </Text>

          <ContactCard contact={contact} />
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

/** What stops a channel reaching this parent even with consent. */
function missingContact(channel: ExternalChannel, contact: IMyContact): string | undefined {
  switch (channel) {
    case 2: return contact.email ? undefined : "There's no email address on your account.";
    case 3: return contact.mobileNumber ? undefined : "Add a mobile number below to get SMS.";
    case 4: return contact.deviceCount > 0 ? undefined : "No phone is set up for push notifications yet.";
    case 5: return contact.whatsAppNumber ? undefined : "Add a WhatsApp number below to get WhatsApp messages.";
  }
}

function ChannelCard({ channel, contact }: { channel: ExternalChannel; contact: IMyContact }) {
  const { consents, preferences, savingKey } = useNotificationSettingsState();
  const { setConsentAsync, setPreferenceAsync } = useNotificationSettingsActions();
  const consent = consents?.find((c) => c.channel === channel);
  const isOn = consent?.isGranted === true;
  const isSaving = !!savingKey;
  const label = CHANNEL_LABELS[channel];
  const missing = missingContact(channel, contact);

  const status = isOn
    ? consent?.grantedDate ? `On since ${formatDate(consent.grantedDate)}` : "On"
    : consent?.revokedDate ? `Turned off on ${formatDate(consent.revokedDate)}` : "Not turned on";

  return (
    <Card style={styles.card}>
      <View style={styles.row}>
        <View style={styles.rowCopy}>
          <Text style={styles.channelTitle}>{label}</Text>
          <Text style={styles.meta}>{status}</Text>
        </View>
        <Switch
          accessibilityLabel={`${label}: ${isOn ? "on" : "off"}`}
          value={isOn}
          disabled={isSaving}
          onValueChange={(value) => void setConsentAsync(channel, value)}
          trackColor={{ true: colors.primary, false: colors.border }}
        />
      </View>

      {isOn && !!missing && (
        <View style={styles.warning}><Text style={styles.warningText}>{missing}</Text></View>
      )}

      {isOn && (
        <View style={styles.categories}>
          <Text style={styles.categoriesTitle}>Send me</Text>
          {CATEGORIES.map((category) => {
            // No row means it is sent: only an opt-out is stored.
            const enabled = preferences?.find((p) => p.channel === channel && p.category === category)?.isEnabled ?? true;
            const { title, detail } = CATEGORY_LABELS[category];
            return (
              <View key={category} style={styles.categoryRow}>
                <View style={styles.rowCopy}>
                  <Text style={styles.categoryTitle}>{title}</Text>
                  <Text style={styles.meta}>{detail}</Text>
                </View>
                <Switch
                  accessibilityLabel={`${title} by ${label}: ${enabled ? "on" : "off"}`}
                  value={enabled}
                  disabled={isSaving}
                  onValueChange={(value) => void setPreferenceAsync(channel, category, value)}
                  trackColor={{ true: colors.primary, false: colors.border }}
                />
              </View>
            );
          })}
        </View>
      )}
    </Card>
  );
}

function ContactCard({ contact }: { contact: IMyContact }) {
  const { isContactSaving, contactErrorMessage } = useNotificationSettingsState();
  const { updateContactAsync } = useNotificationSettingsActions();
  const [mobileNumber, setMobileNumber] = useState(contact.mobileNumber ?? "");
  const [whatsAppNumber, setWhatsAppNumber] = useState(contact.whatsAppNumber ?? "");
  const [validationMessage, setValidationMessage] = useState<string>();
  const [saved, setSaved] = useState(false);

  const isDirty = mobileNumber.trim() !== (contact.mobileNumber ?? "")
    || whatsAppNumber.trim() !== (contact.whatsAppNumber ?? "");

  const submit = async () => {
    const parsed = contactSchema.safeParse({ mobileNumber, whatsAppNumber });
    if (!parsed.success) {
      setValidationMessage(parsed.error.issues[0]?.message);
      return;
    }
    setValidationMessage(undefined);
    setSaved(false);
    if (await updateContactAsync(parsed.data)) {
      setMobileNumber(parsed.data.mobileNumber);
      setWhatsAppNumber(parsed.data.whatsAppNumber);
      setSaved(true);
    }
  };

  const message = validationMessage || contactErrorMessage;

  return (
    <Card style={styles.card}>
      <Text style={styles.channelTitle}>Contact details</Text>
      <Text style={styles.meta}>Where the school reaches you outside the app.</Text>

      <Text style={styles.label}>Email</Text>
      <Text style={styles.readOnly}>{contact.email ?? "None on your account"}</Text>
      <Text style={styles.meta}>This is your sign-in. Your school office can change it.</Text>

      <Text style={styles.label}>Mobile number (SMS)</Text>
      <TextInput
        value={mobileNumber}
        onChangeText={(value) => { setMobileNumber(value); setSaved(false); }}
        placeholder="e.g. 082 123 4567"
        keyboardType="phone-pad"
        autoComplete="tel"
        editable={!isContactSaving}
        accessibilityLabel="Mobile number"
        style={styles.input}
      />

      <Text style={styles.label}>WhatsApp number</Text>
      <TextInput
        value={whatsAppNumber}
        onChangeText={(value) => { setWhatsAppNumber(value); setSaved(false); }}
        placeholder="e.g. 082 123 4567"
        keyboardType="phone-pad"
        editable={!isContactSaving}
        accessibilityLabel="WhatsApp number"
        style={styles.input}
      />

      {!!message && <Text accessibilityRole="alert" style={styles.inlineError}>{message}</Text>}
      {saved && !isDirty && <Text style={styles.saved}>Saved.</Text>}

      <View style={styles.saveButton}>
        <Button disabled={isContactSaving || !isDirty} onPress={() => void submit()}>
          {isContactSaving ? "Saving…" : "Save contact details"}
        </Button>
      </View>
    </Card>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: colors.background },
  centred: { flex: 1, justifyContent: "center", padding: spacing.xxl },
  content: { padding: spacing.xxl, paddingBottom: spacing.xxl * 2 },
  title: { color: colors.text, fontSize: typography.title, fontWeight: "800", marginTop: spacing.lg },
  intro: { color: colors.textMuted, fontSize: 15, lineHeight: 22, marginTop: spacing.sm },
  inlineError: { color: colors.danger, fontSize: 14, marginTop: spacing.md },
  card: { marginTop: spacing.lg },
  row: { alignItems: "center", flexDirection: "row", gap: spacing.md },
  rowCopy: { flex: 1 },
  channelTitle: { color: colors.text, fontSize: 17, fontWeight: "800" },
  meta: { color: colors.textMuted, fontSize: 13, marginTop: spacing.xs },
  warning: {
    backgroundColor: colors.warningBackground, borderRadius: radius.sm, marginTop: spacing.md, padding: spacing.md,
  },
  warningText: { color: colors.warning, fontSize: 13, fontWeight: "700" },
  categories: { borderTopColor: colors.border, borderTopWidth: 1, marginTop: spacing.md, paddingTop: spacing.sm },
  categoriesTitle: { color: colors.textMuted, fontSize: 12, fontWeight: "800", letterSpacing: 1, marginTop: spacing.xs },
  categoryRow: { alignItems: "center", flexDirection: "row", gap: spacing.md, minHeight: 56, paddingVertical: spacing.xs },
  categoryTitle: { color: colors.text, fontSize: 15, fontWeight: "700" },
  consentNote: { color: colors.textMuted, fontSize: 13, lineHeight: 19, marginTop: spacing.lg },
  label: { color: colors.text, fontSize: 13, fontWeight: "700", marginBottom: spacing.xs, marginTop: spacing.lg },
  readOnly: { color: colors.text, fontSize: 16 },
  input: {
    backgroundColor: colors.surface, borderColor: colors.border, borderRadius: radius.sm, borderWidth: 1,
    color: colors.text, fontSize: 16, paddingHorizontal: spacing.md, paddingVertical: spacing.md,
  },
  saved: { color: colors.success, fontSize: 14, fontWeight: "700", marginTop: spacing.md },
  saveButton: { marginTop: spacing.lg },
});
