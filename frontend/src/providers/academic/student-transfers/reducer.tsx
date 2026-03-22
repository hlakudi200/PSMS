import { handleActions } from "redux-actions";
import { INITIAL_STATE, IStudentTransferStateContext } from "./context";
import { StudentTransferActionEnums } from "./actions";

export const StudentTransferReducer = handleActions<
  IStudentTransferStateContext,
  IStudentTransferStateContext
>(
  {
    [StudentTransferActionEnums.getTransferPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.getTransferSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.getTransferError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.getTransfersPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.getTransfersSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.getTransfersError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.createTransferPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.createTransferSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.createTransferError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.updateTransferPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.updateTransferSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.updateTransferError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.submitPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.submitSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.submitError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.approvePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.approveSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.approveError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.rejectPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.rejectSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.rejectError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.completePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.completeSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.completeError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.cancelPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.cancelSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransferActionEnums.cancelError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
