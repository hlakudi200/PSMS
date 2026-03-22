import { createAction } from "redux-actions";
import { IStudentTransferStateContext, IStudentTransfer } from "./context";

export enum StudentTransferActionEnums {
  getTransferPending = "GET_STUDENT_TRANSFER_PENDING",
  getTransferSuccess = "GET_STUDENT_TRANSFER_SUCCESS",
  getTransferError = "GET_STUDENT_TRANSFER_ERROR",

  getTransfersPending = "GET_STUDENT_TRANSFERS_PENDING",
  getTransfersSuccess = "GET_STUDENT_TRANSFERS_SUCCESS",
  getTransfersError = "GET_STUDENT_TRANSFERS_ERROR",

  createTransferPending = "CREATE_STUDENT_TRANSFER_PENDING",
  createTransferSuccess = "CREATE_STUDENT_TRANSFER_SUCCESS",
  createTransferError = "CREATE_STUDENT_TRANSFER_ERROR",

  updateTransferPending = "UPDATE_STUDENT_TRANSFER_PENDING",
  updateTransferSuccess = "UPDATE_STUDENT_TRANSFER_SUCCESS",
  updateTransferError = "UPDATE_STUDENT_TRANSFER_ERROR",

  submitPending = "SUBMIT_STUDENT_TRANSFER_PENDING",
  submitSuccess = "SUBMIT_STUDENT_TRANSFER_SUCCESS",
  submitError = "SUBMIT_STUDENT_TRANSFER_ERROR",

  approvePending = "APPROVE_STUDENT_TRANSFER_PENDING",
  approveSuccess = "APPROVE_STUDENT_TRANSFER_SUCCESS",
  approveError = "APPROVE_STUDENT_TRANSFER_ERROR",

  rejectPending = "REJECT_STUDENT_TRANSFER_PENDING",
  rejectSuccess = "REJECT_STUDENT_TRANSFER_SUCCESS",
  rejectError = "REJECT_STUDENT_TRANSFER_ERROR",

  completePending = "COMPLETE_STUDENT_TRANSFER_PENDING",
  completeSuccess = "COMPLETE_STUDENT_TRANSFER_SUCCESS",
  completeError = "COMPLETE_STUDENT_TRANSFER_ERROR",

  cancelPending = "CANCEL_STUDENT_TRANSFER_PENDING",
  cancelSuccess = "CANCEL_STUDENT_TRANSFER_SUCCESS",
  cancelError = "CANCEL_STUDENT_TRANSFER_ERROR",
}

// Get Single
export const getTransferPending = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.getTransferPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getTransferSuccess = createAction<IStudentTransferStateContext, IStudentTransfer>(
  StudentTransferActionEnums.getTransferSuccess,
  (studentTransfer: IStudentTransfer) => ({
    isPending: false, isSuccess: true, isError: false, studentTransfer,
  })
);
export const getTransferError = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.getTransferError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All
export const getTransfersPending = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.getTransfersPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getTransfersSuccess = createAction<IStudentTransferStateContext, { items: IStudentTransfer[]; totalCount: number }>(
  StudentTransferActionEnums.getTransfersSuccess,
  (result) => ({
    isPending: false, isSuccess: true, isError: false,
    studentTransfers: result.items, totalCount: result.totalCount,
  })
);
export const getTransfersError = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.getTransfersError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create
export const createTransferPending = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.createTransferPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const createTransferSuccess = createAction<IStudentTransferStateContext, IStudentTransfer>(
  StudentTransferActionEnums.createTransferSuccess,
  (studentTransfer: IStudentTransfer) => ({
    isPending: false, isSuccess: true, isError: false, studentTransfer,
  })
);
export const createTransferError = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.createTransferError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update
export const updateTransferPending = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.updateTransferPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const updateTransferSuccess = createAction<IStudentTransferStateContext, IStudentTransfer>(
  StudentTransferActionEnums.updateTransferSuccess,
  (studentTransfer: IStudentTransfer) => ({
    isPending: false, isSuccess: true, isError: false, studentTransfer,
  })
);
export const updateTransferError = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.updateTransferError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Submit
export const submitPending = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.submitPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const submitSuccess = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.submitSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const submitError = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.submitError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Approve
export const approvePending = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.approvePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const approveSuccess = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.approveSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const approveError = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.approveError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Reject
export const rejectPending = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.rejectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const rejectSuccess = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.rejectSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const rejectError = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.rejectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Complete
export const completePending = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.completePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const completeSuccess = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.completeSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const completeError = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.completeError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Cancel
export const cancelPending = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.cancelPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const cancelSuccess = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.cancelSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const cancelError = createAction<IStudentTransferStateContext>(
  StudentTransferActionEnums.cancelError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
