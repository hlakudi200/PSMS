'use client';
import { createContext } from "react";
import {
  IAdminUser,
  ICreateUser,
  IUpdateUser,
  IGetUsersInput,
  IResetPasswordInput,
} from "../shared/interfaces";

export interface IAdminRoleOption {
  name: string;
  displayName: string;
}

export interface IUserStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  user?: IAdminUser;
  users?: IAdminUser[];
  totalCount?: number;
  roles?: IAdminRoleOption[];
}

export interface IUserActionContext {
  getAsync: (id: number) => void;
  getAllAsync: (input?: IGetUsersInput) => void;
  createAsync: (input: ICreateUser) => void;
  updateAsync: (id: number, input: IUpdateUser) => void;
  deleteAsync: (id: number) => void;
  activateAsync: (id: number) => void;
  deactivateAsync: (id: number) => void;
  resetPasswordAsync: (input: IResetPasswordInput) => void;
  getRolesAsync: () => void;
}

export const INITIAL_STATE: IUserStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const UserStateContext =
  createContext<IUserStateContext>(INITIAL_STATE);

export const UserActionContext = createContext<
  IUserActionContext | undefined
>(undefined);
