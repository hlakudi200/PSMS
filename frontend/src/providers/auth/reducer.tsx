import { handleActions } from "redux-actions";
import { INITIAL_STATE, IAuthStateContext } from "./context";
import { AuthActionEnums } from "./actions";

// Every action here dispatches a patch, not a whole state — the handlers all
// spread it over the previous state. Saying so lets an action carry only the
// field it changes (hydrationSettled carries just isHydrating) instead of
// having to restate flags it knows nothing about.
export const AuthReducer = handleActions<IAuthStateContext, Partial<IAuthStateContext>>(
  {
    [AuthActionEnums.loginUserPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AuthActionEnums.loginUserSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AuthActionEnums.loginUserError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),

    [AuthActionEnums.getCurrentUserPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AuthActionEnums.getCurrentUserSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AuthActionEnums.getCurrentUserError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),

    [AuthActionEnums.signOutUser]: (state, action) => ({
      ...state,
      ...action.payload,
    }),

    [AuthActionEnums.resetStateFlagsAction]: (state, action) => ({
      ...state,
      ...action.payload,
    }),

    [AuthActionEnums.hydrationSettled]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
