import axios from "axios";
import { Modal } from "antd";

const baseURL = process.env.NEXT_PUBLIC_BASE_URL;

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
      if (!axios.isAxiosError(error) || !error.response) {
        Modal.error({
          title: "Network Error",
          content: "Unable to connect to the server. Please check your internet connection.",
        });
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
