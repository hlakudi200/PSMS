import { handleActions } from "redux-actions";
import { INITIAL_STATE, IPrincipalDashboardStateContext } from "./context";
import { PrincipalDashboardActionEnums } from "./actions";

export const PrincipalDashboardReducer = handleActions<
  IPrincipalDashboardStateContext,
  IPrincipalDashboardStateContext
>(
  {
    [PrincipalDashboardActionEnums.getSummaryPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [PrincipalDashboardActionEnums.getSummarySuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [PrincipalDashboardActionEnums.getSummaryError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
