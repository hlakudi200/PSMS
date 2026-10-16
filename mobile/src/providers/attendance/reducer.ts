import { handleActions } from "redux-actions";
import { INITIAL_STATE, type IAttendanceStateContext } from "./context";
import { AttendanceActionEnums } from "./actions";

export const AttendanceReducer = handleActions<IAttendanceStateContext, any>(
  Object.fromEntries(
    Object.values(AttendanceActionEnums).map((action) => [
      action,
      (state: IAttendanceStateContext, a: { payload: Partial<IAttendanceStateContext> }) => ({ ...state, ...a.payload }),
    ])
  ),
  INITIAL_STATE
);
