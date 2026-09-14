import * as SecureStore from "expo-secure-store";

const ACCESS_TOKEN_KEY = "psms.accessToken";
const ENCRYPTED_ACCESS_TOKEN_KEY = "psms.encryptedAccessToken";
const TENANT_ID_KEY = "psms.tenantId";

export interface IMobileSession {
  accessToken: string;
  encryptedAccessToken?: string;
  tenantId?: string;
}

export const getSession = async (): Promise<IMobileSession | undefined> => {
  const [accessToken, encryptedAccessToken, tenantId] = await Promise.all([
    SecureStore.getItemAsync(ACCESS_TOKEN_KEY),
    SecureStore.getItemAsync(ENCRYPTED_ACCESS_TOKEN_KEY),
    SecureStore.getItemAsync(TENANT_ID_KEY),
  ]);

  return accessToken
    ? { accessToken, encryptedAccessToken: encryptedAccessToken ?? undefined, tenantId: tenantId ?? undefined }
    : undefined;
};

export const saveSession = async (session: IMobileSession): Promise<void> => {
  const writes: Promise<void>[] = [SecureStore.setItemAsync(ACCESS_TOKEN_KEY, session.accessToken)];

  if (session.encryptedAccessToken) {
    writes.push(SecureStore.setItemAsync(ENCRYPTED_ACCESS_TOKEN_KEY, session.encryptedAccessToken));
  }
  if (session.tenantId) {
    writes.push(SecureStore.setItemAsync(TENANT_ID_KEY, session.tenantId));
  }

  await Promise.all(writes);
};

export const clearSession = async (): Promise<void> => {
  await Promise.all([
    SecureStore.deleteItemAsync(ACCESS_TOKEN_KEY),
    SecureStore.deleteItemAsync(ENCRYPTED_ACCESS_TOKEN_KEY),
    SecureStore.deleteItemAsync(TENANT_ID_KEY),
  ]);
};
