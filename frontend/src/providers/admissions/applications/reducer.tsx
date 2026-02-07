import { handleActions } from "redux-actions";
import { INITIAL_STATE, IApplicationStateContext } from "./context";
import { ApplicationActionEnums } from "./actions";

export const ApplicationReducer = handleActions<
  IApplicationStateContext,
  IApplicationStateContext
>(
  {
    [ApplicationActionEnums.getApplicationsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.getApplicationsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.getApplicationsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.getApplicationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.getApplicationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.getApplicationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.getApplicationByNumberPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.getApplicationByNumberSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.getApplicationByNumberError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.createApplicationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.createApplicationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.createApplicationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.updateApplicationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.updateApplicationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.updateApplicationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.deleteApplicationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.deleteApplicationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.deleteApplicationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.submitApplicationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.submitApplicationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.submitApplicationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.withdrawApplicationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.withdrawApplicationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.withdrawApplicationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.getStatisticsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.getStatisticsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationActionEnums.getStatisticsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
