import { createAction } from "redux-actions";
import { IAttendanceStateContext } from "./context";
import { IAttendance, IAttendanceList, IAttendanceSummary, IPagedResult, IListResult } from "../shared/interfaces";

export enum AttendanceActionEnums {
  getAttendancesPending = "GET_ATTENDANCES_PENDING",
  getAttendancesSuccess = "GET_ATTENDANCES_SUCCESS",
  getAttendancesError = "GET_ATTENDANCES_ERROR",

  getAttendancePending = "GET_ATTENDANCE_PENDING",
  getAttendanceSuccess = "GET_ATTENDANCE_SUCCESS",
  getAttendanceError = "GET_ATTENDANCE_ERROR",

  getByStudentPending = "GET_ATTENDANCE_BY_STUDENT_PENDING",
  getByStudentSuccess = "GET_ATTENDANCE_BY_STUDENT_SUCCESS",
  getByStudentError = "GET_ATTENDANCE_BY_STUDENT_ERROR",

  getByClassAndDatePending = "GET_ATTENDANCE_BY_CLASS_AND_DATE_PENDING",
  getByClassAndDateSuccess = "GET_ATTENDANCE_BY_CLASS_AND_DATE_SUCCESS",
  getByClassAndDateError = "GET_ATTENDANCE_BY_CLASS_AND_DATE_ERROR",

  captureAttendancePending = "CAPTURE_ATTENDANCE_PENDING",
  captureAttendanceSuccess = "CAPTURE_ATTENDANCE_SUCCESS",
  captureAttendanceError = "CAPTURE_ATTENDANCE_ERROR",

  bulkCaptureAttendancePending = "BULK_CAPTURE_ATTENDANCE_PENDING",
  bulkCaptureAttendanceSuccess = "BULK_CAPTURE_ATTENDANCE_SUCCESS",
  bulkCaptureAttendanceError = "BULK_CAPTURE_ATTENDANCE_ERROR",

  updateAttendancePending = "UPDATE_ATTENDANCE_PENDING",
  updateAttendanceSuccess = "UPDATE_ATTENDANCE_SUCCESS",
  updateAttendanceError = "UPDATE_ATTENDANCE_ERROR",

  deleteAttendancePending = "DELETE_ATTENDANCE_PENDING",
  deleteAttendanceSuccess = "DELETE_ATTENDANCE_SUCCESS",
  deleteAttendanceError = "DELETE_ATTENDANCE_ERROR",

  getStudentSummaryPending = "GET_STUDENT_SUMMARY_PENDING",
  getStudentSummarySuccess = "GET_STUDENT_SUMMARY_SUCCESS",
  getStudentSummaryError = "GET_STUDENT_SUMMARY_ERROR",

  getClassSummaryPending = "GET_CLASS_SUMMARY_PENDING",
  getClassSummarySuccess = "GET_CLASS_SUMMARY_SUCCESS",
  getClassSummaryError = "GET_CLASS_SUMMARY_ERROR",
}

// Get All Attendances Actions
export const getAttendancesPending = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.getAttendancesPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAttendancesSuccess = createAction<
  IAttendanceStateContext,
  IPagedResult<IAttendanceList>
>(
  AttendanceActionEnums.getAttendancesSuccess,
  (result: IPagedResult<IAttendanceList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    attendances: result.items,
    totalCount: result.totalCount,
  })
);

export const getAttendancesError = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.getAttendancesError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Single Attendance Actions
export const getAttendancePending = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.getAttendancePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAttendanceSuccess = createAction<IAttendanceStateContext, IAttendance>(
  AttendanceActionEnums.getAttendanceSuccess,
  (attendance: IAttendance) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    attendance,
  })
);

export const getAttendanceError = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.getAttendanceError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Student Actions
export const getByStudentPending = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.getByStudentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByStudentSuccess = createAction<
  IAttendanceStateContext,
  IListResult<IAttendanceList>
>(
  AttendanceActionEnums.getByStudentSuccess,
  (result: IListResult<IAttendanceList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    attendances: result.items,
  })
);

export const getByStudentError = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.getByStudentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Class And Date Actions
export const getByClassAndDatePending = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.getByClassAndDatePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByClassAndDateSuccess = createAction<
  IAttendanceStateContext,
  IListResult<IAttendanceList>
>(
  AttendanceActionEnums.getByClassAndDateSuccess,
  (result: IListResult<IAttendanceList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    attendances: result.items,
  })
);

export const getByClassAndDateError = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.getByClassAndDateError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Capture Attendance Actions
export const captureAttendancePending = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.captureAttendancePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const captureAttendanceSuccess = createAction<IAttendanceStateContext, IAttendance>(
  AttendanceActionEnums.captureAttendanceSuccess,
  (attendance: IAttendance) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    attendance,
  })
);

export const captureAttendanceError = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.captureAttendanceError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Bulk Capture Attendance Actions
export const bulkCaptureAttendancePending = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.bulkCaptureAttendancePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const bulkCaptureAttendanceSuccess = createAction<
  IAttendanceStateContext,
  IListResult<IAttendance>
>(
  AttendanceActionEnums.bulkCaptureAttendanceSuccess,
  (result: IListResult<IAttendance>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const bulkCaptureAttendanceError = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.bulkCaptureAttendanceError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Attendance Actions
export const updateAttendancePending = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.updateAttendancePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateAttendanceSuccess = createAction<IAttendanceStateContext, IAttendance>(
  AttendanceActionEnums.updateAttendanceSuccess,
  (attendance: IAttendance) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    attendance,
  })
);

export const updateAttendanceError = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.updateAttendanceError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Attendance Actions
export const deleteAttendancePending = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.deleteAttendancePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteAttendanceSuccess = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.deleteAttendanceSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteAttendanceError = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.deleteAttendanceError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Student Summary Actions
export const getStudentSummaryPending = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.getStudentSummaryPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getStudentSummarySuccess = createAction<IAttendanceStateContext, IAttendanceSummary>(
  AttendanceActionEnums.getStudentSummarySuccess,
  (attendanceSummary: IAttendanceSummary) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    attendanceSummary,
  })
);

export const getStudentSummaryError = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.getStudentSummaryError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Class Summary Actions
export const getClassSummaryPending = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.getClassSummaryPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getClassSummarySuccess = createAction<
  IAttendanceStateContext,
  IListResult<IAttendanceSummary>
>(
  AttendanceActionEnums.getClassSummarySuccess,
  (result: IListResult<IAttendanceSummary>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    attendanceSummaries: result.items,
  })
);

export const getClassSummaryError = createAction<IAttendanceStateContext>(
  AttendanceActionEnums.getClassSummaryError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
