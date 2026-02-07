import { jwtDecode } from "jwt-decode";

export interface IDecodedToken {
  sub: string;
  jti: string;
  iat: string;
  nbf: string;
  exp: string;
  iss: string;
  aud: string;
  [AbpTokenProperties.nameidentifier]: string;
  [AbpTokenProperties.name]: string;
  [AbpTokenProperties.emailaddress]: string;
  [AbpTokenProperties.role]: string;
}

export enum AbpTokenProperties {
  claims = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/",
  nameidentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
  name = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
  emailaddress = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
  role = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
}

export const decodeToken = (accessToken: string): IDecodedToken => {
  return jwtDecode<IDecodedToken>(accessToken);
};

export const getRole = (accessToken: string): string => {
  if (accessToken) {
    const decoded = decodeToken(accessToken);
    return `${decoded[AbpTokenProperties.role]}`.toLowerCase();
  }
  return "";
};

export const getUserId = (accessToken: string): string => {
  if (accessToken) {
    const decoded = decodeToken(accessToken);
    return `${decoded[AbpTokenProperties.nameidentifier]}`;
  }
  return "";
};

export const getUserName = (accessToken: string): string => {
  if (accessToken) {
    const decoded = decodeToken(accessToken);
    return `${decoded[AbpTokenProperties.name]}`;
  }
  return "";
};
