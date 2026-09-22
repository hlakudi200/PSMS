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
  /** The caller's own Student.Id (Guid) — resolved once for a Student-role session; every student screen needs this to scope its own reads. */
  currentStudentId?: string;
  /** The caller's own CurrentClassId (Guid) — for class-scoped reads (e.g. this term's assessment list). */
  currentClassId?: string;
  /** True once the Student/GetActiveStudents lookup behind currentStudentId/currentClassId has failed — lets screens stop showing a loading spinner and show a real error state instead. */
  currentStudentIdError?: boolean;
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
