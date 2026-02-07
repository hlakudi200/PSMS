import { handleActions } from "redux-actions";
import { INITIAL_STATE, IApplicationFeeStateContext } from "./context";
import { ApplicationFeeActionEnums } from "./actions";

export const ApplicationFeeReducer = handleActions<
  IApplicationFeeStateContext,
  IApplicationFeeStateContext
>(
  {
    [ApplicationFeeActionEnums.getByApplicationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationFeeActionEnums.getByApplicationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationFeeActionEnums.getByApplicationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationFeeActionEnums.recordPaymentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationFeeActionEnums.recordPaymentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationFeeActionEnums.recordPaymentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationFeeActionEnums.processPaymentCallbackPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationFeeActionEnums.processPaymentCallbackSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationFeeActionEnums.processPaymentCallbackError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationFeeActionEnums.getPaymentStatusPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationFeeActionEnums.getPaymentStatusSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationFeeActionEnums.getPaymentStatusError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
