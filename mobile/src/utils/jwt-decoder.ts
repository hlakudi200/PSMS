import { jwtDecode } from "jwt-decode";

export enum AbpTokenProperties {
  nameidentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
  name = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
  emailaddress = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
  role = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
}

type IDecodedToken = Record<string, string | string[] | number | undefined>;

export const MOBILE_ROLES = ["student", "parent"] as const;
export type MobileRole = (typeof MOBILE_ROLES)[number];

export const getRoles = (accessToken: string): string[] => {
  try {
    const role = jwtDecode<IDecodedToken>(accessToken)[AbpTokenProperties.role];
    return (Array.isArray(role) ? role : role ? [role] : [])
      .map((value) => String(value).toLowerCase());
  } catch {
    return [];
  }
};

export const getMobileRole = (accessToken: string, roleNames: string[] = []): MobileRole | undefined => {
  const roles = [...getRoles(accessToken), ...roleNames.map((role) => role.toLowerCase())];
  return MOBILE_ROLES.find((role) => roles.includes(role));
};
