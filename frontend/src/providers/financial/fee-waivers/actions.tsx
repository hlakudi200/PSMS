import { createAction } from "redux-actions";
import { IFeeWaiverStateContext, IFeeWaiver } from "./context";

export enum FeeWaiverActionEnums {
  getFeeWaiverPending = "GET_FEE_WAIVER_PENDING",
  getFeeWaiverSuccess = "GET_FEE_WAIVER_SUCCESS",
  getFeeWaiverError = "GET_FEE_WAIVER_ERROR",

  getFeeWaiversPending = "GET_FEE_WAIVERS_PENDING",
  getFeeWaiversSuccess = "GET_FEE_WAIVERS_SUCCESS",
  getFeeWaiversError = "GET_FEE_WAIVERS_ERROR",

  createFeeWaiverPending = "CREATE_FEE_WAIVER_PENDING",
  createFeeWaiverSuccess = "CREATE_FEE_WAIVER_SUCCESS",
  createFeeWaiverError = "CREATE_FEE_WAIVER_ERROR",

  updateFeeWaiverPending = "UPDATE_FEE_WAIVER_PENDING",
  updateFeeWaiverSuccess = "UPDATE_FEE_WAIVER_SUCCESS",
  updateFeeWaiverError = "UPDATE_FEE_WAIVER_ERROR",

  deleteFeeWaiverPending = "DELETE_FEE_WAIVER_PENDING",
  deleteFeeWaiverSuccess = "DELETE_FEE_WAIVER_SUCCESS",
  deleteFeeWaiverError = "DELETE_FEE_WAIVER_ERROR",

  submitPending = "SUBMIT_FEE_WAIVER_PENDING",
  submitSuccess = "SUBMIT_FEE_WAIVER_SUCCESS",
  submitError = "SUBMIT_FEE_WAIVER_ERROR",

  approvePending = "APPROVE_FEE_WAIVER_PENDING",
  approveSuccess = "APPROVE_FEE_WAIVER_SUCCESS",
  approveError = "APPROVE_FEE_WAIVER_ERROR",

  rejectPending = "REJECT_FEE_WAIVER_PENDING",
  rejectSuccess = "REJECT_FEE_WAIVER_SUCCESS",
  rejectError = "REJECT_FEE_WAIVER_ERROR",
}

// Get Single
export const getFeeWaiverPending = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.getFeeWaiverPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getFeeWaiverSuccess = createAction<IFeeWaiverStateContext, IFeeWaiver>(
  FeeWaiverActionEnums.getFeeWaiverSuccess,
  (feeWaiver: IFeeWaiver) => ({
    isPending: false, isSuccess: true, isError: false, feeWaiver,
  })
);
export const getFeeWaiverError = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.getFeeWaiverError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All
export const getFeeWaiversPending = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.getFeeWaiversPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getFeeWaiversSuccess = createAction<IFeeWaiverStateContext, { items: IFeeWaiver[]; totalCount: number }>(
  FeeWaiverActionEnums.getFeeWaiversSuccess,
  (result) => ({
    isPending: false, isSuccess: true, isError: false,
    feeWaivers: result.items, totalCount: result.totalCount,
  })
);
export const getFeeWaiversError = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.getFeeWaiversError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create
export const createFeeWaiverPending = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.createFeeWaiverPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const createFeeWaiverSuccess = createAction<IFeeWaiverStateContext, IFeeWaiver>(
  FeeWaiverActionEnums.createFeeWaiverSuccess,
  (feeWaiver: IFeeWaiver) => ({
    isPending: false, isSuccess: true, isError: false, feeWaiver,
  })
);
export const createFeeWaiverError = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.createFeeWaiverError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update
export const updateFeeWaiverPending = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.updateFeeWaiverPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const updateFeeWaiverSuccess = createAction<IFeeWaiverStateContext, IFeeWaiver>(
  FeeWaiverActionEnums.updateFeeWaiverSuccess,
  (feeWaiver: IFeeWaiver) => ({
    isPending: false, isSuccess: true, isError: false, feeWaiver,
  })
);
export const updateFeeWaiverError = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.updateFeeWaiverError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete
export const deleteFeeWaiverPending = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.deleteFeeWaiverPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const deleteFeeWaiverSuccess = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.deleteFeeWaiverSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const deleteFeeWaiverError = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.deleteFeeWaiverError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Submit
export const submitPending = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.submitPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const submitSuccess = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.submitSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const submitError = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.submitError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Approve
export const approvePending = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.approvePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const approveSuccess = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.approveSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const approveError = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.approveError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Reject
export const rejectPending = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.rejectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const rejectSuccess = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.rejectSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const rejectError = createAction<IFeeWaiverStateContext>(
  FeeWaiverActionEnums.rejectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
