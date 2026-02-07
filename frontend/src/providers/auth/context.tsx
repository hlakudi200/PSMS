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
};

export const AuthStateContext = createContext<IAuthStateContext>(INITIAL_STATE);

export const AuthActionContext = createContext<IAuthActionContext | undefined>(
  undefined
);
