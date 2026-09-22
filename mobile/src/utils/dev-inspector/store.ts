/**
 * In-memory ring buffer for the on-device dev inspector. Holds the most recent
 * network calls and console lines so testers on a physical device can see what
 * the browser DevTools would normally show. Nothing is persisted.
 */

export const isDevInspectorEnabled =
  __DEV__ || process.env.EXPO_PUBLIC_ENABLE_DEV_INSPECTOR === "true";

const MAX_ENTRIES = 200;
const MAX_BODY_CHARS = 20_000;

export type NetworkEntry = {
  id: string;
  method: string;
  url: string;
  startedAt: number;
  durationMs?: number;
  status?: number;
  state: "pending" | "success" | "error";
  requestHeaders?: Record<string, string>;
  requestBody?: string;
  responseHeaders?: Record<string, string>;
  responseBody?: string;
  errorMessage?: string;
};

export type LogLevel = "log" | "info" | "warn" | "error" | "debug";

export type LogEntry = { id: string; level: LogLevel; message: string; at: number };

export type InspectorSnapshot = { network: NetworkEntry[]; logs: LogEntry[] };

let snapshot: InspectorSnapshot = { network: [], logs: [] };
const listeners = new Set<() => void>();
let counter = 0;

const emit = () => listeners.forEach((listener) => listener());

export const nextInspectorId = () => `${Date.now().toString(36)}-${(counter++).toString(36)}`;

export const subscribeInspector = (listener: () => void) => {
  listeners.add(listener);
  return () => { listeners.delete(listener); };
};

export const getInspectorSnapshot = () => snapshot;

export const addNetworkEntry = (entry: NetworkEntry) => {
  snapshot = { ...snapshot, network: [entry, ...snapshot.network].slice(0, MAX_ENTRIES) };
  emit();
};

export const updateNetworkEntry = (id: string, patch: Partial<NetworkEntry>) => {
  snapshot = { ...snapshot, network: snapshot.network.map((entry) => (entry.id === id ? { ...entry, ...patch } : entry)) };
  emit();
};

export const addLogEntry = (level: LogLevel, message: string) => {
  const entry: LogEntry = { id: nextInspectorId(), level, message: truncate(message), at: Date.now() };
  snapshot = { ...snapshot, logs: [entry, ...snapshot.logs].slice(0, MAX_ENTRIES) };
  emit();
};

export const clearInspector = (section: keyof InspectorSnapshot) => {
  snapshot = { ...snapshot, [section]: [] };
  emit();
};

export const truncate = (value: string) =>
  value.length > MAX_BODY_CHARS ? `${value.slice(0, MAX_BODY_CHARS)}\n… truncated (${value.length} chars)` : value;

/** Pretty-prints JSON (or JSON strings) and falls back to String() for anything else. */
export const stringifyPayload = (value: unknown): string | undefined => {
  if (value === undefined || value === null || value === "") return undefined;
  if (typeof value === "string") {
    try { return truncate(JSON.stringify(JSON.parse(value), null, 2)); } catch { return truncate(value); }
  }
  if (value instanceof Error) return truncate(`${value.name}: ${value.message}${value.stack ? `\n${value.stack}` : ""}`);
  try { return truncate(JSON.stringify(value, null, 2)); } catch { return truncate(String(value)); }
};
