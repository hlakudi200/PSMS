import { createAction } from "redux-actions";
import { IStaffLeaveRequestStateContext, IStaffLeaveRequest } from "./context";

export enum StaffLeaveActionEnums {
  getLeavePending = "GET_STAFF_LEAVE_PENDING",
  getLeaveSuccess = "GET_STAFF_LEAVE_SUCCESS",
  getLeaveError = "GET_STAFF_LEAVE_ERROR",

  getLeavesPending = "GET_STAFF_LEAVES_PENDING",
  getLeavesSuccess = "GET_STAFF_LEAVES_SUCCESS",
  getLeavesError = "GET_STAFF_LEAVES_ERROR",

  createLeavePending = "CREATE_STAFF_LEAVE_PENDING",
  createLeaveSuccess = "CREATE_STAFF_LEAVE_SUCCESS",
  createLeaveError = "CREATE_STAFF_LEAVE_ERROR",

  updateLeavePending = "UPDATE_STAFF_LEAVE_PENDING",
  updateLeaveSuccess = "UPDATE_STAFF_LEAVE_SUCCESS",
  updateLeaveError = "UPDATE_STAFF_LEAVE_ERROR",

  submitPending = "SUBMIT_STAFF_LEAVE_PENDING",
  submitSuccess = "SUBMIT_STAFF_LEAVE_SUCCESS",
  submitError = "SUBMIT_STAFF_LEAVE_ERROR",

  approvePending = "APPROVE_STAFF_LEAVE_PENDING",
  approveSuccess = "APPROVE_STAFF_LEAVE_SUCCESS",
  approveError = "APPROVE_STAFF_LEAVE_ERROR",

  rejectPending = "REJECT_STAFF_LEAVE_PENDING",
  rejectSuccess = "REJECT_STAFF_LEAVE_SUCCESS",
  rejectError = "REJECT_STAFF_LEAVE_ERROR",

  cancelPending = "CANCEL_STAFF_LEAVE_PENDING",
  cancelSuccess = "CANCEL_STAFF_LEAVE_SUCCESS",
  cancelError = "CANCEL_STAFF_LEAVE_ERROR",
}

// Get Single
export const getLeavePending = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.getLeavePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getLeaveSuccess = createAction<IStaffLeaveRequestStateContext, IStaffLeaveRequest>(
  StaffLeaveActionEnums.getLeaveSuccess,
  (staffLeaveRequest: IStaffLeaveRequest) => ({
    isPending: false, isSuccess: true, isError: false, staffLeaveRequest,
  })
);
export const getLeaveError = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.getLeaveError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All
export const getLeavesPending = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.getLeavesPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getLeavesSuccess = createAction<IStaffLeaveRequestStateContext, { items: IStaffLeaveRequest[]; totalCount: number }>(
  StaffLeaveActionEnums.getLeavesSuccess,
  (result) => ({
    isPending: false, isSuccess: true, isError: false,
    staffLeaveRequests: result.items, totalCount: result.totalCount,
  })
);
export const getLeavesError = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.getLeavesError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create
export const createLeavePending = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.createLeavePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const createLeaveSuccess = createAction<IStaffLeaveRequestStateContext, IStaffLeaveRequest>(
  StaffLeaveActionEnums.createLeaveSuccess,
  (staffLeaveRequest: IStaffLeaveRequest) => ({
    isPending: false, isSuccess: true, isError: false, staffLeaveRequest,
  })
);
export const createLeaveError = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.createLeaveError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update
export const updateLeavePending = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.updateLeavePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const updateLeaveSuccess = createAction<IStaffLeaveRequestStateContext, IStaffLeaveRequest>(
  StaffLeaveActionEnums.updateLeaveSuccess,
  (staffLeaveRequest: IStaffLeaveRequest) => ({
    isPending: false, isSuccess: true, isError: false, staffLeaveRequest,
  })
);
export const updateLeaveError = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.updateLeaveError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Submit
export const submitPending = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.submitPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const submitSuccess = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.submitSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const submitError = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.submitError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Approve
export const approvePending = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.approvePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const approveSuccess = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.approveSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const approveError = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.approveError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Reject
export const rejectPending = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.rejectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const rejectSuccess = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.rejectSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const rejectError = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.rejectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Cancel
export const cancelPending = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.cancelPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const cancelSuccess = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.cancelSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const cancelError = createAction<IStaffLeaveRequestStateContext>(
  StaffLeaveActionEnums.cancelError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
