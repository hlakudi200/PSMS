import { handleActions } from "redux-actions";
import { INITIAL_STATE, IFeeWaiverStateContext } from "./context";
import { FeeWaiverActionEnums } from "./actions";

export const FeeWaiverReducer = handleActions<
  IFeeWaiverStateContext,
  IFeeWaiverStateContext
>(
  {
    [FeeWaiverActionEnums.getFeeWaiverPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.getFeeWaiverSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.getFeeWaiverError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.getFeeWaiversPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.getFeeWaiversSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.getFeeWaiversError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.createFeeWaiverPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.createFeeWaiverSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.createFeeWaiverError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.updateFeeWaiverPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.updateFeeWaiverSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.updateFeeWaiverError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.deleteFeeWaiverPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.deleteFeeWaiverSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.deleteFeeWaiverError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.submitPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.submitSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.submitError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.approvePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.approveSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.approveError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.rejectPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.rejectSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FeeWaiverActionEnums.rejectError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
