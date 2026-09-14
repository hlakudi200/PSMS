import { createContext } from "react";
import type { MobileRole } from "../../utils/jwt-decoder";

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
  isBootstrapping: boolean;
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  isAccessDenied?: boolean;
  errorMessage?: string;
  currentUser?: IUser;
  currentTenant?: ITenant;
  jwtToken?: string;
  currentRole?: MobileRole;
}

export interface IAuthActionContext {
  loginUser: (loginData: ILoginData) => Promise<void>;
  getCurrentUser: (jwtToken: string) => Promise<void>;
  signOut: () => Promise<void>;
  resetStateFlags: () => void;
}

export const INITIAL_STATE: IAuthStateContext = {
  isBootstrapping: true,
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const AuthStateContext = createContext<IAuthStateContext>(INITIAL_STATE);
export const AuthActionContext = createContext<IAuthActionContext | undefined>(undefined);
