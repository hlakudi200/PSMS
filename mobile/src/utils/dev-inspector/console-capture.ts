import { addLogEntry, isDevInspectorEnabled, stringifyPayload, type LogLevel } from "./store";

const LEVELS: LogLevel[] = ["log", "info", "warn", "error", "debug"];
let installed = false;

/** Mirrors console output into the inspector while still printing to Metro as usual. */
export const installConsoleCapture = () => {
  if (installed || !isDevInspectorEnabled) return;
  installed = true;

  LEVELS.forEach((level) => {
    const original = console[level].bind(console);
    console[level] = (...args: unknown[]) => {
      original(...args);
      try {
        addLogEntry(level, args.map((arg) => (typeof arg === "string" ? arg : stringifyPayload(arg) ?? String(arg))).join(" "));
      } catch {
        // Never let the inspector break logging.
      }
    };
  });
};
