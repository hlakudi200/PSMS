"use client";

import { createContext } from "react";

export interface IUser {
  id?: number;
  userName: string;
  name: string;
  surname: string;
  emailAddress: string;
  isActive?: boolean;
  roleNames?: string[];
  tenantId?: number;
}

export interface ILoginData {
  userNameOrEmailAddress: string;
  password: string;
  tenancyName?: string;
}

export interface ITenant {
  id: number;
  tenancyName: string;
  name: string;
  isActive: boolean;
}

export interface IAuthStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  currentUser?: IUser;
  currentTenant?: ITenant;
  jwtToken?: string;
  currentRole?: string;
  /**
   * True until the provider has looked in sessionStorage for an existing
   * session. Nothing may conclude the visitor is signed out before this is
   * false: on a browser refresh the provider starts from INITIAL_STATE and only
   * reads the token in a mount effect, and effects run children first — so a
   * route guard that redirected on `!jwtToken` fired before the token was ever
   * read, and every refresh bounced the user to the login screen.
   *
   * Only an explicit `false` means "we looked". Undefined is treated as still
   * hydrating, so a payload that omits the flag can never release a guard.
   */
  isHydrating?: boolean;
}

export interface IAuthActionContext {
  loginUser: (loginData: ILoginData) => Promise<void>;
  getCurrentUser: (jwtToken: string) => Promise<void>;
  signOut: () => void;
  resetStateFlags: () => void;
}

export const INITIAL_STATE: IAuthStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
  currentUser: undefined,
  currentTenant: undefined,
  jwtToken: undefined,
  currentRole: undefined,
  isHydrating: true,
};

export const AuthStateContext = createContext<IAuthStateContext>(INITIAL_STATE);

export const AuthActionContext = createContext<IAuthActionContext | undefined>(
  undefined
);
