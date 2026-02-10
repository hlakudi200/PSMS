import axios from "axios";

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

  return instance;
};
