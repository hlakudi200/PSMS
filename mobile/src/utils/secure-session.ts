import { Platform } from "react-native";
import * as SecureStore from "expo-secure-store";

const ACCESS_TOKEN_KEY = "psms.accessToken";
const ENCRYPTED_ACCESS_TOKEN_KEY = "psms.encryptedAccessToken";
const TENANT_ID_KEY = "psms.tenantId";

export interface IMobileSession {
  accessToken: string;
  encryptedAccessToken?: string;
  tenantId?: string;
}

// expo-secure-store has no web implementation (it's a native-only keychain/keystore
// wrapper) and throws if called on web. Fall back to localStorage there — fine for
// local web-preview development, since the app's real target is native.
const isWeb = Platform.OS === "web";

const getItem = async (key: string): Promise<string | null> => {
  if (isWeb) {
    try { return typeof window === "undefined" ? null : window.localStorage.getItem(key); }
    catch { return null; }
  }
  return SecureStore.getItemAsync(key);
};

const setItem = async (key: string, value: string): Promise<void> => {
  if (isWeb) {
    try { if (typeof window !== "undefined") window.localStorage.setItem(key, value); }
    catch { /* ignore */ }
    return;
  }
  await SecureStore.setItemAsync(key, value);
};

const deleteItem = async (key: string): Promise<void> => {
  if (isWeb) {
    try { if (typeof window !== "undefined") window.localStorage.removeItem(key); }
    catch { /* ignore */ }
    return;
  }
  await SecureStore.deleteItemAsync(key);
};

export const getSession = async (): Promise<IMobileSession | undefined> => {
  const [accessToken, encryptedAccessToken, tenantId] = await Promise.all([
    getItem(ACCESS_TOKEN_KEY),
    getItem(ENCRYPTED_ACCESS_TOKEN_KEY),
    getItem(TENANT_ID_KEY),
  ]);

  return accessToken
    ? { accessToken, encryptedAccessToken: encryptedAccessToken ?? undefined, tenantId: tenantId ?? undefined }
    : undefined;
};

export const saveSession = async (session: IMobileSession): Promise<void> => {
  const writes: Promise<void>[] = [setItem(ACCESS_TOKEN_KEY, session.accessToken)];

  if (session.encryptedAccessToken) {
    writes.push(setItem(ENCRYPTED_ACCESS_TOKEN_KEY, session.encryptedAccessToken));
  }
  if (session.tenantId) {
    writes.push(setItem(TENANT_ID_KEY, session.tenantId));
  }

  await Promise.all(writes);
};

export const clearSession = async (): Promise<void> => {
  await Promise.all([
    deleteItem(ACCESS_TOKEN_KEY),
    deleteItem(ENCRYPTED_ACCESS_TOKEN_KEY),
    deleteItem(TENANT_ID_KEY),
  ]);
};
