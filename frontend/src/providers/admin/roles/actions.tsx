import { createAction } from "redux-actions";
import { IRoleStateContext } from "./context";
import { IAdminRole, IPermissionDto, IRoleForEdit } from "../shared/interfaces";
import { IPagedResult } from "../../shared/interfaces";

export enum RoleActionEnums {
  getRolePending = "GET_ROLE_PENDING",
  getRoleSuccess = "GET_ROLE_SUCCESS",
  getRoleError = "GET_ROLE_ERROR",

  getRolesPending = "GET_ROLES_PENDING",
  getRolesSuccess = "GET_ROLES_SUCCESS",
  getRolesError = "GET_ROLES_ERROR",

  createRolePending = "CREATE_ROLE_PENDING",
  createRoleSuccess = "CREATE_ROLE_SUCCESS",
  createRoleError = "CREATE_ROLE_ERROR",

  updateRolePending = "UPDATE_ROLE_PENDING",
  updateRoleSuccess = "UPDATE_ROLE_SUCCESS",
  updateRoleError = "UPDATE_ROLE_ERROR",

  deleteRolePending = "DELETE_ROLE_PENDING",
  deleteRoleSuccess = "DELETE_ROLE_SUCCESS",
  deleteRoleError = "DELETE_ROLE_ERROR",

  getAllPermissionsPending = "GET_ALL_PERMISSIONS_PENDING",
  getAllPermissionsSuccess = "GET_ALL_PERMISSIONS_SUCCESS",
  getAllPermissionsError = "GET_ALL_PERMISSIONS_ERROR",

  getRoleForEditPending = "GET_ROLE_FOR_EDIT_PENDING",
  getRoleForEditSuccess = "GET_ROLE_FOR_EDIT_SUCCESS",
  getRoleForEditError = "GET_ROLE_FOR_EDIT_ERROR",
}

// Get Single Role
export const getRolePending = createAction<IRoleStateContext>(
  RoleActionEnums.getRolePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getRoleSuccess = createAction<IRoleStateContext, IAdminRole>(
  RoleActionEnums.getRoleSuccess,
  (role: IAdminRole) => ({
    isPending: false, isSuccess: true, isError: false, role,
  })
);
export const getRoleError = createAction<IRoleStateContext>(
  RoleActionEnums.getRoleError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All Roles
export const getRolesPending = createAction<IRoleStateContext>(
  RoleActionEnums.getRolesPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getRolesSuccess = createAction<IRoleStateContext, IPagedResult<IAdminRole>>(
  RoleActionEnums.getRolesSuccess,
  (result: IPagedResult<IAdminRole>) => ({
    isPending: false, isSuccess: true, isError: false,
    roles: result.items, totalCount: result.totalCount,
  })
);
export const getRolesError = createAction<IRoleStateContext>(
  RoleActionEnums.getRolesError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Role
export const createRolePending = createAction<IRoleStateContext>(
  RoleActionEnums.createRolePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const createRoleSuccess = createAction<IRoleStateContext, IAdminRole>(
  RoleActionEnums.createRoleSuccess,
  (role: IAdminRole) => ({
    isPending: false, isSuccess: true, isError: false, role,
  })
);
export const createRoleError = createAction<IRoleStateContext>(
  RoleActionEnums.createRoleError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Role
export const updateRolePending = createAction<IRoleStateContext>(
  RoleActionEnums.updateRolePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const updateRoleSuccess = createAction<IRoleStateContext, IAdminRole>(
  RoleActionEnums.updateRoleSuccess,
  (role: IAdminRole) => ({
    isPending: false, isSuccess: true, isError: false, role,
  })
);
export const updateRoleError = createAction<IRoleStateContext>(
  RoleActionEnums.updateRoleError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Role
export const deleteRolePending = createAction<IRoleStateContext>(
  RoleActionEnums.deleteRolePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const deleteRoleSuccess = createAction<IRoleStateContext>(
  RoleActionEnums.deleteRoleSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const deleteRoleError = createAction<IRoleStateContext>(
  RoleActionEnums.deleteRoleError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All Permissions
export const getAllPermissionsPending = createAction<IRoleStateContext>(
  RoleActionEnums.getAllPermissionsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getAllPermissionsSuccess = createAction<IRoleStateContext, IPermissionDto[]>(
  RoleActionEnums.getAllPermissionsSuccess,
  (allPermissions: IPermissionDto[]) => ({
    isPending: false, isSuccess: true, isError: false, allPermissions,
  })
);
export const getAllPermissionsError = createAction<IRoleStateContext>(
  RoleActionEnums.getAllPermissionsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Role For Edit
export const getRoleForEditPending = createAction<IRoleStateContext>(
  RoleActionEnums.getRoleForEditPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getRoleForEditSuccess = createAction<IRoleStateContext, IRoleForEdit>(
  RoleActionEnums.getRoleForEditSuccess,
  (roleForEdit: IRoleForEdit) => ({
    isPending: false, isSuccess: true, isError: false, roleForEdit,
  })
);
export const getRoleForEditError = createAction<IRoleStateContext>(
  RoleActionEnums.getRoleForEditError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
