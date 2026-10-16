import { useCallback, useContext, useMemo, useReducer, useRef } from "react";
import { getAxiosInstance } from "../../utils/axios-instance";
import { AttendanceReducer } from "./reducer";
import {
  INITIAL_STATE, AttendanceActionContext, AttendanceStateContext,
  type IAttendanceActionContext, type IAttendanceRange, type IAttendanceRecord, type IAttendanceSummary,
} from "./context";
import { getError, getPending, getSuccess } from "./actions";

const mapSummary = (dto: any): IAttendanceSummary => ({
  totalDays: dto.totalDays ?? 0,
  presentCount: dto.presentCount ?? 0,
  absentCount: dto.absentCount ?? 0,
  lateCount: dto.lateCount ?? 0,
  excusedCount: dto.excusedCount ?? 0,
  sickLeaveCount: dto.sickLeaveCount ?? 0,
  attendancePercentage: Number(dto.attendancePercentage ?? 0),
});

const mapRecord = (dto: any): IAttendanceRecord => ({
  id: dto.id,
  attendanceDate: dto.attendanceDate,
  status: dto.status,
  notes: dto.notes || undefined,
  className: dto.className ?? undefined,
});

export const AttendanceProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(AttendanceReducer, INITIAL_STATE);
  const instance = useRef(getAxiosInstance()).current;
  // A parent can switch child or range before the last load lands; only the latest may write.
  const latestRequestRef = useRef(0);

  const getAttendanceAsync = useCallback(async (studentId: string, range: IAttendanceRange) => {
    const requestId = ++latestRequestRef.current;
    dispatch(getPending());
    try {
      const params = { studentId, startDate: range.startDate, endDate: range.endDate };
      // Sequential, not parallel: two requests is all this screen needs, and the shared
      // database pool is small enough that a burst from every open screen adds up.
      const { data: summaryData } = await instance.get("/api/services/app/Attendance/GetStudentSummary", { params });
      const { data: recordsData } = await instance.get("/api/services/app/Attendance/GetByStudent", { params });
      if (requestId !== latestRequestRef.current) return;
      dispatch(getSuccess({
        summary: mapSummary(summaryData.result),
        records: (recordsData.result.items as any[]).map(mapRecord),
        studentId,
        range,
      }));
    } catch {
      if (requestId === latestRequestRef.current) dispatch(getError());
    }
  }, [instance]);

  const actions = useMemo<IAttendanceActionContext>(() => ({ getAttendanceAsync }), [getAttendanceAsync]);

  return (
    <AttendanceStateContext.Provider value={state}>
      <AttendanceActionContext.Provider value={actions}>{children}</AttendanceActionContext.Provider>
    </AttendanceStateContext.Provider>
  );
};

export const useAttendanceState = () => useContext(AttendanceStateContext);
export const useAttendanceActions = () => {
  const context = useContext(AttendanceActionContext);
  if (!context) throw new Error("useAttendanceActions must be used within an AttendanceProvider");
  return context;
};
