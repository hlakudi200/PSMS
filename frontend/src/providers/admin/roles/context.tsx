'use client';
import { createContext } from "react";
import {
  IAdminRole,
  ICreateRole,
  IUpdateRole,
  IGetRolesInput,
  IPermissionDto,
  IRoleForEdit,
} from "../shared/interfaces";

export interface IRoleStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  role?: IAdminRole;
  roles?: IAdminRole[];
  totalCount?: number;
  allPermissions?: IPermissionDto[];
  roleForEdit?: IRoleForEdit;
}

export interface IRoleActionContext {
  getAsync: (id: number) => void;
  getAllAsync: (input?: IGetRolesInput) => void;
  createAsync: (input: ICreateRole) => void;
  updateAsync: (id: number, input: IUpdateRole) => void;
  deleteAsync: (id: number) => void;
  getAllPermissionsAsync: () => void;
  getRoleForEditAsync: (id: number) => void;
}

export const INITIAL_STATE: IRoleStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const RoleStateContext =
  createContext<IRoleStateContext>(INITIAL_STATE);

export const RoleActionContext = createContext<
  IRoleActionContext | undefined
>(undefined);
