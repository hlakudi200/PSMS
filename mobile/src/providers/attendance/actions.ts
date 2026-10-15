import { createAction } from "redux-actions";
import type { IAttendanceRange, IAttendanceRecord, IAttendanceStateContext, IAttendanceSummary } from "./context";

type AttendancePatch = Partial<IAttendanceStateContext>;

export enum AttendanceActionEnums {
  getPending = "ATTENDANCE_GET_PENDING",
  getSuccess = "ATTENDANCE_GET_SUCCESS",
  getError = "ATTENDANCE_GET_ERROR",
}

export const getPending = createAction<AttendancePatch>(AttendanceActionEnums.getPending, () => ({
  isPending: true, isSuccess: false, isError: false,
}));

export const getSuccess = createAction<
  AttendancePatch,
  { summary: IAttendanceSummary; records: IAttendanceRecord[]; studentId: string; range: IAttendanceRange }
>(
  AttendanceActionEnums.getSuccess,
  ({ summary, records, studentId, range }) => ({
    isPending: false, isSuccess: true, isError: false, summary, records, studentId, range,
  })
);

export const getError = createAction<AttendancePatch>(AttendanceActionEnums.getError, () => ({
  isPending: false, isSuccess: false, isError: true,
}));
