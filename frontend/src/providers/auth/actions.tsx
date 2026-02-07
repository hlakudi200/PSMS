import { createAction } from "redux-actions";
import { IAuthStateContext, IUser, ITenant } from "./context";

export enum AuthActionEnums {
  loginUserPending = "LOGIN_USER_PENDING",
  loginUserSuccess = "LOGIN_USER_SUCCESS",
  loginUserError = "LOGIN_USER_ERROR",

  getCurrentUserPending = "GET_CURRENT_USER_PENDING",
  getCurrentUserSuccess = "GET_CURRENT_USER_SUCCESS",
  getCurrentUserError = "GET_CURRENT_USER_ERROR",

  signOutUser = "SIGN_OUT_USER",
  resetStateFlagsAction = "RESET_STATE_FLAGS",
}

// Login actions
export const loginUserPending = createAction<IAuthStateContext>(
  AuthActionEnums.loginUserPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const loginUserSuccess = createAction<
  IAuthStateContext,
  { jwtToken: string; currentRole: string }
>(
  AuthActionEnums.loginUserSuccess,
  (payload: { jwtToken: string; currentRole: string }) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    jwtToken: payload.jwtToken,
    currentRole: payload.currentRole,
  })
);

export const loginUserError = createAction<IAuthStateContext>(
  AuthActionEnums.loginUserError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get current user actions
export const getCurrentUserPending = createAction<IAuthStateContext>(
  AuthActionEnums.getCurrentUserPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getCurrentUserSuccess = createAction<
  IAuthStateContext,
  { currentUser: IUser; currentTenant?: ITenant }
>(
  AuthActionEnums.getCurrentUserSuccess,
  (payload: { currentUser: IUser; currentTenant?: ITenant }) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    currentUser: payload.currentUser,
    currentTenant: payload.currentTenant,
  })
);

export const getCurrentUserError = createAction<IAuthStateContext>(
  AuthActionEnums.getCurrentUserError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Sign out
export const signOutUser = createAction<IAuthStateContext>(
  AuthActionEnums.signOutUser,
  () => ({
    isPending: false,
    isSuccess: false,
    isError: false,
    currentUser: undefined,
    currentTenant: undefined,
    jwtToken: undefined,
    currentRole: undefined,
  })
);

// Reset flags
export const resetStateFlagsAction = createAction<IAuthStateContext>(
  AuthActionEnums.resetStateFlagsAction,
  () => ({ isPending: false, isSuccess: false, isError: false })
);
