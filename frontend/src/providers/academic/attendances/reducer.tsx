import { handleActions } from "redux-actions";
import { INITIAL_STATE, IAttendanceStateContext } from "./context";
import { AttendanceActionEnums } from "./actions";

export const AttendanceReducer = handleActions<
  IAttendanceStateContext,
  IAttendanceStateContext
>(
  {
    [AttendanceActionEnums.getAttendancesPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getAttendancesSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getAttendancesError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getAttendancePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getAttendanceSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getAttendanceError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getByStudentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getByStudentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getByStudentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getByClassAndDatePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getByClassAndDateSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getByClassAndDateError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.captureAttendancePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.captureAttendanceSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.captureAttendanceError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.bulkCaptureAttendancePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.bulkCaptureAttendanceSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.bulkCaptureAttendanceError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.updateAttendancePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.updateAttendanceSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.updateAttendanceError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.deleteAttendancePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.deleteAttendanceSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.deleteAttendanceError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getStudentSummaryPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getStudentSummarySuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getStudentSummaryError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getClassSummaryPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getClassSummarySuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AttendanceActionEnums.getClassSummaryError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
