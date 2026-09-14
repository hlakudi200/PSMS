import { handleActions } from "redux-actions";
import { AuthActionEnums } from "./actions";
import { INITIAL_STATE, type IAuthStateContext } from "./context";

export const AuthReducer = handleActions<IAuthStateContext, Partial<IAuthStateContext>>(
  Object.fromEntries(
    Object.values(AuthActionEnums).map((action) => [action, (state: IAuthStateContext, payload: { payload: Partial<IAuthStateContext> }) => ({
      ...state,
      ...payload.payload,
    })])
  ),
  INITIAL_STATE
);
