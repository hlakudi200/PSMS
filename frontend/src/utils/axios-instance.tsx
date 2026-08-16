import axios from "axios";
import { Modal } from "antd";

const baseURL = process.env.NEXT_PUBLIC_BASE_URL;

/**
 * Per-request opt-out of the error modal below, for calls whose failure the
 * caller handles itself and that the user never asked for. Without it a
 * background fetch (e.g. the login page's branding lookup, which fires as the
 * user types) throws a blocking dialog over the screen on every failure.
 *
 * Set it as a normal request-config field:
 *   instance.get(url, { suppressErrorModal: true })
 */
declare module "axios" {
  export interface AxiosRequestConfig {
    suppressErrorModal?: boolean;
  }
}

export const getAxiosInstance = () => {
  const instance = axios.create({
    baseURL: `${baseURL}`,
    headers: {
      "Content-Type": "application/json;charset=utf-8",
    },
  });

  instance.interceptors.request.use((config) => {
    config.headers = config.headers ?? {};

    // ABP multi-tenancy: set tenant ID on every request
    const tenantId = sessionStorage.getItem("tenantId");
    if (tenantId) {
      config.headers["Abp-TenantId"] = tenantId;
    }

    const token = sessionStorage.getItem("accessToken");
    if (token) {
      config.headers["Authorization"] = `Bearer ${token}`;
    }
    return config;
  });

  instance.interceptors.response.use(
    (response) => response,
    (error) => {
      // Callers that handle their own failures opt out of every dialog below.
      const silent = Boolean(
        axios.isAxiosError(error) && error.config?.suppressErrorModal
      );

      if (!axios.isAxiosError(error) || !error.response) {
        if (!silent) {
          Modal.error({
            title: "Network Error",
            content: "Unable to connect to the server. Please check your internet connection.",
          });
        }
        return Promise.reject(error);
      }

      const { status, data } = error.response;

      // Handle 401 Unauthorized — redirect to login
      if (status === 401) {
        sessionStorage.removeItem("accessToken");
        sessionStorage.removeItem("tenantId");
        if (typeof window !== "undefined" && !window.location.pathname.includes("/auth/login")) {
          window.location.href = "/auth/login";
        }
        return Promise.reject(error);
      }

      // 401 is handled above (it redirects regardless of `silent`, since a
      // dead session must not be swallowed). Everything past here is purely
      // a dialog, so an opted-out caller stops now.
      if (silent) {
        return Promise.reject(error);
      }

      // Parse ABP error response: { success: false, error: { code, message, details, validationErrors } }
      const abpError = data?.error;

      if (abpError) {
        if (abpError.validationErrors && abpError.validationErrors.length > 0) {
          const validationMessages = abpError.validationErrors
            .map((ve: { message: string }) => ve.message)
            .join("\n");

          Modal.error({
            title: "Validation Error",
            content: validationMessages,
          });
        } else {
          Modal.error({
            title: abpError.message || "Request Failed",
            content: abpError.details || undefined,
          });
        }
      } else {
        const fallbackMessages: Record<number, string> = {
          403: "You do not have permission to perform this action.",
          404: "The requested resource was not found.",
          500: "An internal server error occurred. Please try again later.",
        };

        Modal.error({
          title: `Error ${status}`,
          content: fallbackMessages[status] || "An unexpected error occurred.",
        });
      }

      return Promise.reject(error);
    }
  );

  return instance;
};
