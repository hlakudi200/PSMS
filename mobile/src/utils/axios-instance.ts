import axios from "axios";
import { attachNetworkLogger } from "./dev-inspector";
import { clearSession, getSession } from "./secure-session";

const baseURL = process.env.EXPO_PUBLIC_API_BASE_URL ?? "http://localhost:21021";

export const getAxiosInstance = (onUnauthorized?: () => void) => {
  const instance = axios.create({
    baseURL,
    headers: { "Content-Type": "application/json;charset=utf-8" },
  });

  // Must be attached first so it sees final request headers and raw responses.
  attachNetworkLogger(instance);

  instance.interceptors.request.use(async (config) => {
    const session = await getSession();
    config.headers = config.headers ?? {};

    if (session?.tenantId) config.headers["Abp-TenantId"] = session.tenantId;
    if (session?.accessToken) config.headers.Authorization = `Bearer ${session.accessToken}`;

    return config;
  });

  instance.interceptors.response.use(
    (response) => response,
    async (error) => {
      if (axios.isAxiosError(error) && error.response?.status === 401) {
        await clearSession();
        onUnauthorized?.();
      }
      return Promise.reject(error);
    }
  );

  return instance;
};

export const getApiErrorMessage = (error: unknown, fallback: string): string => {
  if (!axios.isAxiosError(error)) return fallback;
  const abpError = error.response?.data?.error;
  const validationMessages = abpError?.validationErrors?.map((item: { message: string }) => item.message);
  return validationMessages?.join("\n") || abpError?.message || fallback;
};
