import { handleActions } from "redux-actions";
import { INITIAL_STATE, IUserStateContext } from "./context";
import { UserActionEnums } from "./actions";

export const UserReducer = handleActions<
  IUserStateContext,
  IUserStateContext
>(
  {
    [UserActionEnums.getUserPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.getUserSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.getUserError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.getUsersPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.getUsersSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.getUsersError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.createUserPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.createUserSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.createUserError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.updateUserPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.updateUserSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.updateUserError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.deleteUserPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.deleteUserSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.deleteUserError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.activateUserPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.activateUserSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.activateUserError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.deactivateUserPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.deactivateUserSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.deactivateUserError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.resetPasswordPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.resetPasswordSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.resetPasswordError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.getRolesPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.getRolesSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [UserActionEnums.getRolesError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
