import { createAction } from "redux-actions";
import type { IAuthStateContext, ITenant, IUser } from "./context";
import type { MobileRole } from "../../utils/jwt-decoder";

type AuthPatch = Partial<IAuthStateContext>;

export enum AuthActionEnums {
  loginUserPending = "LOGIN_USER_PENDING",
  loginUserSuccess = "LOGIN_USER_SUCCESS",
  loginUserError = "LOGIN_USER_ERROR",
  getCurrentUserPending = "GET_CURRENT_USER_PENDING",
  getCurrentUserSuccess = "GET_CURRENT_USER_SUCCESS",
  getCurrentUserError = "GET_CURRENT_USER_ERROR",
  signOutUser = "SIGN_OUT_USER",
  resetStateFlagsAction = "RESET_STATE_FLAGS",
  bootstrapComplete = "BOOTSTRAP_COMPLETE",
  setCurrentStudentId = "SET_CURRENT_STUDENT_ID",
}

export const loginUserPending = createAction<AuthPatch>(AuthActionEnums.loginUserPending, () => ({
  isPending: true, isSuccess: false, isError: false, errorMessage: undefined,
}));
export const loginUserSuccess = createAction<AuthPatch, { jwtToken: string; currentRole: MobileRole }>(
  AuthActionEnums.loginUserSuccess,
  ({ jwtToken, currentRole }) => ({ isPending: false, isSuccess: true, isError: false, errorMessage: undefined, jwtToken, currentRole })
);
export const loginUserError = createAction<AuthPatch, { errorMessage: string; isAccessDenied?: boolean }>(
  AuthActionEnums.loginUserError,
  ({ errorMessage, isAccessDenied = false }) => ({ isPending: false, isSuccess: false, isError: true, errorMessage, isAccessDenied })
);
export const getCurrentUserPending = createAction<AuthPatch>(AuthActionEnums.getCurrentUserPending, () => ({
  isPending: true, isSuccess: false, isError: false, errorMessage: undefined,
}));
export const getCurrentUserSuccess = createAction<AuthPatch, { currentUser: IUser; currentTenant?: ITenant }>(
  AuthActionEnums.getCurrentUserSuccess,
  ({ currentUser, currentTenant }) => ({ isPending: false, isSuccess: true, isError: false, currentUser, currentTenant })
);
export const getCurrentUserError = createAction<AuthPatch, string>(
  AuthActionEnums.getCurrentUserError,
  (errorMessage) => ({ isPending: false, isSuccess: false, isError: true, errorMessage, isAccessDenied: false })
);
export const setCurrentStudentId = createAction<AuthPatch, { currentStudentId?: string; currentClassId?: string }>(
  AuthActionEnums.setCurrentStudentId,
  ({ currentStudentId, currentClassId }) => ({ currentStudentId, currentClassId })
);
export const signOutUser = createAction<AuthPatch>(AuthActionEnums.signOutUser, () => ({
  ...INITIAL_AUTH_VALUES,
}));
export const resetStateFlagsAction = createAction<AuthPatch>(AuthActionEnums.resetStateFlagsAction, () => ({
  isPending: false, isSuccess: false, isError: false, errorMessage: undefined, isAccessDenied: false,
}));
export const bootstrapComplete = createAction<AuthPatch>(AuthActionEnums.bootstrapComplete, () => ({ isBootstrapping: false }));

const INITIAL_AUTH_VALUES = {
  isBootstrapping: false,
  isPending: false,
  isSuccess: false,
  isError: false,
  isAccessDenied: false,
  errorMessage: undefined,
  currentUser: undefined,
  currentTenant: undefined,
  jwtToken: undefined,
  currentRole: undefined,
  currentStudentId: undefined,
  currentClassId: undefined,
};
