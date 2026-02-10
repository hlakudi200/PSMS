import { createAction } from "redux-actions";
import { IUserStateContext, IAdminRoleOption } from "./context";
import { IAdminUser } from "../shared/interfaces";
import { IPagedResult } from "../../shared/interfaces";

export enum UserActionEnums {
  getUserPending = "GET_USER_PENDING",
  getUserSuccess = "GET_USER_SUCCESS",
  getUserError = "GET_USER_ERROR",

  getUsersPending = "GET_USERS_PENDING",
  getUsersSuccess = "GET_USERS_SUCCESS",
  getUsersError = "GET_USERS_ERROR",

  createUserPending = "CREATE_USER_PENDING",
  createUserSuccess = "CREATE_USER_SUCCESS",
  createUserError = "CREATE_USER_ERROR",

  updateUserPending = "UPDATE_USER_PENDING",
  updateUserSuccess = "UPDATE_USER_SUCCESS",
  updateUserError = "UPDATE_USER_ERROR",

  deleteUserPending = "DELETE_USER_PENDING",
  deleteUserSuccess = "DELETE_USER_SUCCESS",
  deleteUserError = "DELETE_USER_ERROR",

  activateUserPending = "ACTIVATE_USER_PENDING",
  activateUserSuccess = "ACTIVATE_USER_SUCCESS",
  activateUserError = "ACTIVATE_USER_ERROR",

  deactivateUserPending = "DEACTIVATE_USER_PENDING",
  deactivateUserSuccess = "DEACTIVATE_USER_SUCCESS",
  deactivateUserError = "DEACTIVATE_USER_ERROR",

  resetPasswordPending = "RESET_PASSWORD_PENDING",
  resetPasswordSuccess = "RESET_PASSWORD_SUCCESS",
  resetPasswordError = "RESET_PASSWORD_ERROR",

  getRolesPending = "GET_ROLES_PENDING",
  getRolesSuccess = "GET_ROLES_SUCCESS",
  getRolesError = "GET_ROLES_ERROR",
}

// Get Single User
export const getUserPending = createAction<IUserStateContext>(
  UserActionEnums.getUserPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getUserSuccess = createAction<IUserStateContext, IAdminUser>(
  UserActionEnums.getUserSuccess,
  (user: IAdminUser) => ({
    isPending: false, isSuccess: true, isError: false, user,
  })
);
export const getUserError = createAction<IUserStateContext>(
  UserActionEnums.getUserError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All Users
export const getUsersPending = createAction<IUserStateContext>(
  UserActionEnums.getUsersPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getUsersSuccess = createAction<IUserStateContext, IPagedResult<IAdminUser>>(
  UserActionEnums.getUsersSuccess,
  (result: IPagedResult<IAdminUser>) => ({
    isPending: false, isSuccess: true, isError: false,
    users: result.items, totalCount: result.totalCount,
  })
);
export const getUsersError = createAction<IUserStateContext>(
  UserActionEnums.getUsersError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create User
export const createUserPending = createAction<IUserStateContext>(
  UserActionEnums.createUserPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const createUserSuccess = createAction<IUserStateContext, IAdminUser>(
  UserActionEnums.createUserSuccess,
  (user: IAdminUser) => ({
    isPending: false, isSuccess: true, isError: false, user,
  })
);
export const createUserError = createAction<IUserStateContext>(
  UserActionEnums.createUserError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update User
export const updateUserPending = createAction<IUserStateContext>(
  UserActionEnums.updateUserPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const updateUserSuccess = createAction<IUserStateContext, IAdminUser>(
  UserActionEnums.updateUserSuccess,
  (user: IAdminUser) => ({
    isPending: false, isSuccess: true, isError: false, user,
  })
);
export const updateUserError = createAction<IUserStateContext>(
  UserActionEnums.updateUserError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete User
export const deleteUserPending = createAction<IUserStateContext>(
  UserActionEnums.deleteUserPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const deleteUserSuccess = createAction<IUserStateContext>(
  UserActionEnums.deleteUserSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const deleteUserError = createAction<IUserStateContext>(
  UserActionEnums.deleteUserError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Activate User
export const activateUserPending = createAction<IUserStateContext>(
  UserActionEnums.activateUserPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const activateUserSuccess = createAction<IUserStateContext>(
  UserActionEnums.activateUserSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const activateUserError = createAction<IUserStateContext>(
  UserActionEnums.activateUserError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Deactivate User
export const deactivateUserPending = createAction<IUserStateContext>(
  UserActionEnums.deactivateUserPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const deactivateUserSuccess = createAction<IUserStateContext>(
  UserActionEnums.deactivateUserSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const deactivateUserError = createAction<IUserStateContext>(
  UserActionEnums.deactivateUserError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Reset Password
export const resetPasswordPending = createAction<IUserStateContext>(
  UserActionEnums.resetPasswordPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const resetPasswordSuccess = createAction<IUserStateContext>(
  UserActionEnums.resetPasswordSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const resetPasswordError = createAction<IUserStateContext>(
  UserActionEnums.resetPasswordError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Roles
export const getRolesPending = createAction<IUserStateContext>(
  UserActionEnums.getRolesPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getRolesSuccess = createAction<IUserStateContext, IAdminRoleOption[]>(
  UserActionEnums.getRolesSuccess,
  (roles: IAdminRoleOption[]) => ({
    isPending: false, isSuccess: true, isError: false, roles,
  })
);
export const getRolesError = createAction<IUserStateContext>(
  UserActionEnums.getRolesError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
