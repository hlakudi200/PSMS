import { IPagedAndSortedResultRequest } from "../../shared/interfaces";

// User DTOs
export interface IAdminUser {
  id: number;
  userName: string;
  name: string;
  surname: string;
  emailAddress: string;
  isActive: boolean;
  fullName?: string;
  roleNames: string[];
  lastLoginTime?: string;
  creationTime: string;
}

export interface ICreateUser {
  userName: string;
  name: string;
  surname: string;
  emailAddress: string;
  isActive: boolean;
  roleNames: string[];
  password: string;
}

export interface IUpdateUser {
  userName: string;
  name: string;
  surname: string;
  emailAddress: string;
  isActive: boolean;
  roleNames: string[];
}

export interface IGetUsersInput extends IPagedAndSortedResultRequest {
  keyword?: string;
  isActive?: boolean;
}

export interface IResetPasswordInput {
  adminPassword: string;
  userId: number;
  newPassword: string;
}

// Role DTOs
export interface IAdminRole {
  id: number;
  name: string;
  displayName: string;
  normalizedName?: string;
  description?: string;
  isStatic: boolean;
  isDefault: boolean;
  creationTime: string;
  grantedPermissions?: string[];
}

export interface ICreateRole {
  name: string;
  displayName: string;
  description?: string;
  grantedPermissions: string[];
}

export interface IUpdateRole {
  name: string;
  displayName: string;
  description?: string;
  grantedPermissions: string[];
}

export interface IRoleForEdit {
  role: IAdminRole;
  permissions: IPermissionDto[];
  grantedPermissionNames: string[];
}

export interface IPermissionDto {
  name: string;
  displayName: string;
  description?: string;
  parentName?: string;
}

export interface IGetRolesInput extends IPagedAndSortedResultRequest {
  keyword?: string;
}
