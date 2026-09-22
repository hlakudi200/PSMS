import { useMemo, useState, useSyncExternalStore } from "react";
import { FlatList, Modal, Pressable, SafeAreaView, ScrollView, Share, StyleSheet, Text, TextInput, View } from "react-native";
import { colors, radius, spacing } from "../../theme";
import {
  clearInspector, getInspectorSnapshot, isDevInspectorEnabled, subscribeInspector,
  type LogEntry, type LogLevel, type NetworkEntry,
} from "../../utils/dev-inspector";

type Tab = "network" | "logs";

const MONO = { fontFamily: "monospace" } as const;

const LOG_COLORS: Record<LogLevel, string> = {
  log: colors.text, info: colors.primary, debug: colors.textMuted, warn: colors.warning, error: colors.danger,
};

const time = (at: number) => new Date(at).toTimeString().slice(0, 8);

const statusColor = (entry: NetworkEntry) => {
  if (entry.state === "pending") return colors.textMuted;
  if (entry.state === "error" || (entry.status ?? 0) >= 400) return colors.danger;
  return colors.success;
};

const shortUrl = (url: string) => url.replace(/^https?:\/\/[^/]+/i, "") || url;

const describeEntry = (entry: NetworkEntry) => [
  `${entry.method} ${entry.url}`,
  `Status: ${entry.status ?? entry.state}${entry.durationMs !== undefined ? ` (${entry.durationMs} ms)` : ""}`,
  entry.errorMessage && `Error: ${entry.errorMessage}`,
  entry.requestHeaders && `\nRequest headers:\n${JSON.stringify(entry.requestHeaders, null, 2)}`,
  entry.requestBody && `\nRequest body:\n${entry.requestBody}`,
  entry.responseBody && `\nResponse body:\n${entry.responseBody}`,
].filter(Boolean).join("\n");

/**
 * Floating on-device inspector (network + console) for testing on physical
 * devices through Expo Go. Renders nothing unless __DEV__ or
 * EXPO_PUBLIC_ENABLE_DEV_INSPECTOR=true.
 */
export function DevInspector() {
  if (!isDevInspectorEnabled) return null;
  return <DevInspectorPanel />;
}

function DevInspectorPanel() {
  const snapshot = useSyncExternalStore(subscribeInspector, getInspectorSnapshot);
  const [open, setOpen] = useState(false);
  const [tab, setTab] = useState<Tab>("network");
  const [filter, setFilter] = useState("");
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const errorCount = useMemo(
    () => snapshot.network.filter((entry) => entry.state === "error" || (entry.status ?? 0) >= 400).length
      + snapshot.logs.filter((entry) => entry.level === "error").length,
    [snapshot]
  );

  const query = filter.trim().toLowerCase();
  const network = query ? snapshot.network.filter((e) => `${e.method} ${e.url} ${e.status ?? ""}`.toLowerCase().includes(query)) : snapshot.network;
  const logs = query ? snapshot.logs.filter((e) => `${e.level} ${e.message}`.toLowerCase().includes(query)) : snapshot.logs;
  const selected = snapshot.network.find((entry) => entry.id === selectedId);

  return (
    <>
      <Pressable
        accessibilityRole="button"
        accessibilityLabel="Open developer inspector"
        onPress={() => setOpen(true)}
        style={[styles.fab, errorCount > 0 && styles.fabError]}
      >
        <Text style={styles.fabText}>DEV</Text>
        {errorCount > 0 && <Text style={styles.fabCount}>{errorCount}</Text>}
      </Pressable>

      <Modal visible={open} animationType="slide" onRequestClose={() => (selected ? setSelectedId(null) : setOpen(false))}>
        <SafeAreaView style={styles.safe}>
          {selected ? (
            <NetworkDetail entry={selected} onBack={() => setSelectedId(null)} />
          ) : (
            <>
              <View style={styles.header}>
                <Text style={styles.title}>Dev inspector</Text>
                <HeaderButton label="Clear" onPress={() => clearInspector(tab === "network" ? "network" : "logs")} />
                <HeaderButton label="Close" onPress={() => setOpen(false)} />
              </View>
              <View style={styles.tabs}>
                <TabButton label={`Network (${snapshot.network.length})`} active={tab === "network"} onPress={() => setTab("network")} />
                <TabButton label={`Console (${snapshot.logs.length})`} active={tab === "logs"} onPress={() => setTab("logs")} />
              </View>
              <TextInput
                accessibilityLabel="Filter entries"
                placeholder={tab === "network" ? "Filter by URL, method or status" : "Filter by text or level"}
                value={filter}
                onChangeText={setFilter}
                autoCapitalize="none"
                autoCorrect={false}
                style={styles.filter}
              />
              {tab === "network" ? (
                <FlatList
                  data={network}
                  keyExtractor={(item) => item.id}
                  ListEmptyComponent={<Text style={styles.empty}>No requests yet.</Text>}
                  renderItem={({ item }) => <NetworkRow entry={item} onPress={() => setSelectedId(item.id)} />}
                />
              ) : (
                <FlatList
                  data={logs}
                  keyExtractor={(item) => item.id}
                  ListEmptyComponent={<Text style={styles.empty}>No console output yet.</Text>}
                  renderItem={({ item }) => <LogRow entry={item} />}
                />
              )}
            </>
          )}
        </SafeAreaView>
      </Modal>
    </>
  );
}

function NetworkRow({ entry, onPress }: { entry: NetworkEntry; onPress: () => void }) {
  return (
    <Pressable accessibilityRole="button" onPress={onPress} style={styles.row}>
      <View style={styles.rowTop}>
        <Text style={[styles.method, MONO]}>{entry.method}</Text>
        <Text style={[styles.status, { color: statusColor(entry) }]}>{entry.status ?? (entry.state === "pending" ? "…" : "ERR")}</Text>
        <Text style={styles.meta}>{entry.durationMs !== undefined ? `${entry.durationMs} ms · ` : ""}{time(entry.startedAt)}</Text>
      </View>
      <Text style={[styles.url, MONO]} numberOfLines={2}>{shortUrl(entry.url)}</Text>
      {entry.errorMessage && <Text style={styles.errorText} numberOfLines={2}>{entry.errorMessage}</Text>}
    </Pressable>
  );
}

function LogRow({ entry }: { entry: LogEntry }) {
  const [expanded, setExpanded] = useState(false);
  return (
    <Pressable onPress={() => setExpanded((value) => !value)} style={styles.row}>
      <Text style={styles.meta}>{time(entry.at)} · {entry.level.toUpperCase()}</Text>
      <Text style={[styles.logText, MONO, { color: LOG_COLORS[entry.level] }]} numberOfLines={expanded ? undefined : 4}>{entry.message}</Text>
    </Pressable>
  );
}

function NetworkDetail({ entry, onBack }: { entry: NetworkEntry; onBack: () => void }) {
  return (
    <>
      <View style={styles.header}>
        <HeaderButton label="‹ Back" onPress={onBack} />
        <Text style={styles.title} numberOfLines={1}>{entry.method} {entry.status ?? entry.state}</Text>
        <HeaderButton label="Share" onPress={() => { void Share.share({ message: describeEntry(entry) }); }} />
      </View>
      <ScrollView contentContainerStyle={styles.detail}>
        <Text selectable style={[styles.url, MONO]}>{entry.url}</Text>
        <Text style={styles.meta}>
          {time(entry.startedAt)}{entry.durationMs !== undefined ? ` · ${entry.durationMs} ms` : ""}
        </Text>
        {entry.errorMessage && <Text selectable style={styles.errorText}>{entry.errorMessage}</Text>}
        <Section title="Request headers" body={entry.requestHeaders && JSON.stringify(entry.requestHeaders, null, 2)} />
        <Section title="Request body" body={entry.requestBody} />
        <Section title="Response headers" body={entry.responseHeaders && JSON.stringify(entry.responseHeaders, null, 2)} />
        <Section title="Response body" body={entry.responseBody ?? (entry.state === "pending" ? "Waiting for response…" : undefined)} />
      </ScrollView>
    </>
  );
}

function Section({ title, body }: { title: string; body?: string }) {
  return (
    <View style={styles.section}>
      <Text style={styles.sectionTitle}>{title}</Text>
      <Text selectable style={[styles.code, MONO]}>{body ?? "—"}</Text>
    </View>
  );
}

function HeaderButton({ label, onPress }: { label: string; onPress: () => void }) {
  return (
    <Pressable accessibilityRole="button" onPress={onPress} style={styles.headerButton} hitSlop={8}>
      <Text style={styles.headerButtonText}>{label}</Text>
    </Pressable>
  );
}

function TabButton({ label, active, onPress }: { label: string; active: boolean; onPress: () => void }) {
  return (
    <Pressable accessibilityRole="tab" accessibilityState={{ selected: active }} onPress={onPress} style={[styles.tab, active && styles.tabActive]}>
      <Text style={[styles.tabText, active && styles.tabTextActive]}>{label}</Text>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  fab: {
    position: "absolute", right: spacing.lg, bottom: 96, minWidth: 52, height: 52, borderRadius: radius.pill,
    backgroundColor: colors.primaryDark, alignItems: "center", justifyContent: "center", paddingHorizontal: spacing.sm,
    elevation: 6, shadowColor: "#000", shadowOpacity: 0.25, shadowRadius: 6, shadowOffset: { width: 0, height: 3 }, opacity: 0.9,
  },
  fabError: { backgroundColor: colors.danger },
  fabText: { color: colors.surface, fontWeight: "800", fontSize: 12 },
  fabCount: { color: colors.surface, fontSize: 11, fontWeight: "700" },
  safe: { flex: 1, backgroundColor: colors.background },
  header: {
    flexDirection: "row", alignItems: "center", gap: spacing.sm, paddingHorizontal: spacing.lg, paddingVertical: spacing.md,
    backgroundColor: colors.surface, borderBottomWidth: StyleSheet.hairlineWidth, borderBottomColor: colors.border,
  },
  title: { flex: 1, fontSize: 18, fontWeight: "800", color: colors.text },
  headerButton: { paddingHorizontal: spacing.sm, paddingVertical: spacing.sm },
  headerButtonText: { color: colors.primary, fontWeight: "700", fontSize: 15 },
  tabs: { flexDirection: "row", backgroundColor: colors.surface },
  tab: { flex: 1, alignItems: "center", paddingVertical: spacing.md, borderBottomWidth: 2, borderBottomColor: "transparent" },
  tabActive: { borderBottomColor: colors.primary },
  tabText: { color: colors.textMuted, fontWeight: "600" },
  tabTextActive: { color: colors.primary },
  filter: {
    margin: spacing.md, paddingHorizontal: spacing.md, paddingVertical: spacing.sm, borderRadius: radius.sm,
    borderWidth: 1, borderColor: colors.border, backgroundColor: colors.surface, color: colors.text,
  },
  empty: { textAlign: "center", color: colors.textMuted, padding: spacing.xl },
  row: {
    backgroundColor: colors.surface, paddingHorizontal: spacing.lg, paddingVertical: spacing.md,
    borderBottomWidth: StyleSheet.hairlineWidth, borderBottomColor: colors.border, gap: spacing.xs,
  },
  rowTop: { flexDirection: "row", alignItems: "center", gap: spacing.sm },
  method: { fontWeight: "700", color: colors.text },
  status: { fontWeight: "800" },
  meta: { color: colors.textMuted, fontSize: 12, marginLeft: "auto" },
  url: { color: colors.text, fontSize: 13 },
  errorText: { color: colors.danger, fontSize: 13 },
  logText: { fontSize: 12 },
  detail: { padding: spacing.lg, gap: spacing.md },
  section: { gap: spacing.xs },
  sectionTitle: { fontWeight: "800", color: colors.text },
  code: {
    fontSize: 12, color: colors.text, backgroundColor: colors.surface, padding: spacing.md,
    borderRadius: radius.sm, borderWidth: StyleSheet.hairlineWidth, borderColor: colors.border,
  },
});
