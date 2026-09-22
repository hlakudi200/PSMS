import axios, { type AxiosInstance, type AxiosResponse } from "axios";
import { addNetworkEntry, isDevInspectorEnabled, nextInspectorId, stringifyPayload, updateNetworkEntry } from "./store";

declare module "axios" {
  interface InternalAxiosRequestConfig {
    inspectorId?: string;
    inspectorStartedAt?: number;
  }
}

const SENSITIVE_HEADERS = ["authorization", "cookie", "set-cookie"];
const SENSITIVE_BODY_KEYS = /"(password|currentPassword|newPassword|accessToken|encryptedAccessToken|refreshToken)"\s*:\s*"[^"]*"/gi;

const toHeaderRecord = (headers: unknown): Record<string, string> => {
  const plain = headers && typeof (headers as { toJSON?: () => unknown }).toJSON === "function"
    ? (headers as { toJSON: () => Record<string, unknown> }).toJSON()
    : (headers as Record<string, unknown> | undefined) ?? {};

  return Object.fromEntries(
    Object.entries(plain)
      .filter(([, value]) => value !== undefined && value !== null && typeof value !== "object")
      .map(([key, value]) => {
        const text = String(value);
        if (!SENSITIVE_HEADERS.includes(key.toLowerCase())) return [key, text];
        return [key, text.length > 16 ? `${text.slice(0, 12)}…(redacted)` : "(redacted)"];
      })
  );
};

const redactBody = (body?: string) => body?.replace(SENSITIVE_BODY_KEYS, (_match, key: string) => `"${key}": "(redacted)"`);

const buildUrl = (baseURL: string | undefined, url: string | undefined, params: unknown) => {
  const path = url ?? "";
  const full = /^https?:\/\//i.test(path) ? path : `${(baseURL ?? "").replace(/\/$/, "")}/${path.replace(/^\//, "")}`;
  if (!params || typeof params !== "object") return full;
  const query = Object.entries(params as Record<string, unknown>)
    .filter(([, value]) => value !== undefined && value !== null)
    .map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`)
    .join("&");
  return query ? `${full}${full.includes("?") ? "&" : "?"}${query}` : full;
};

/**
 * Records every request made through the instance. Call this BEFORE adding the
 * app's own interceptors: axios runs request interceptors last-registered-first
 * (so we see the final auth headers) and response interceptors in registration
 * order (so we see the raw response before 401 handling signs the user out).
 */
export const attachNetworkLogger = (instance: AxiosInstance) => {
  if (!isDevInspectorEnabled) return;

  instance.interceptors.request.use((config) => {
    const id = nextInspectorId();
    config.inspectorId = id;
    config.inspectorStartedAt = Date.now();
    addNetworkEntry({
      id,
      method: (config.method ?? "get").toUpperCase(),
      url: buildUrl(config.baseURL, config.url, config.params),
      startedAt: config.inspectorStartedAt,
      state: "pending",
      requestHeaders: toHeaderRecord(config.headers),
      requestBody: redactBody(stringifyPayload(config.data)),
    });
    return config;
  });

  const finish = (response: AxiosResponse | undefined, id: string | undefined, startedAt: number | undefined, errorMessage?: string) => {
    if (!id) return;
    updateNetworkEntry(id, {
      state: errorMessage ? "error" : "success",
      status: response?.status,
      durationMs: startedAt ? Date.now() - startedAt : undefined,
      responseHeaders: response ? toHeaderRecord(response.headers) : undefined,
      responseBody: redactBody(stringifyPayload(response?.data)),
      errorMessage,
    });
  };

  instance.interceptors.response.use(
    (response) => {
      finish(response, response.config.inspectorId, response.config.inspectorStartedAt);
      return response;
    },
    (error) => {
      if (axios.isAxiosError(error)) {
        // No response usually means the device can't reach the API (wrong base URL, localhost, cleartext blocked).
        const message = error.response ? error.message : `${error.code ?? "NETWORK"}: ${error.message} — the device could not reach the server`;
        finish(error.response, error.config?.inspectorId, error.config?.inspectorStartedAt, message);
      }
      return Promise.reject(error);
    }
  );
};
