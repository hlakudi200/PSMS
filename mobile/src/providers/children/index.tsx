import { useCallback, useContext, useMemo, useReducer, useRef } from "react";
import type { AxiosInstance } from "axios";
import { getAxiosInstance } from "../../utils/axios-instance";
import { ChildrenReducer } from "./reducer";
import {
  INITIAL_STATE, ChildrenActionContext, ChildrenStateContext,
  type IChildReportSummary, type IChildSummary, type IChildrenActionContext,
} from "./context";
import { getError, getPending, getSuccess, selectChild as selectChildAction } from "./actions";

const REPORT_STATUS_PUBLISHED = 5;

interface ITermRange {
  startDate: string;
  endDate: string;
}

const loadCurrentTermRange = async (instance: AxiosInstance): Promise<ITermRange | undefined> => {
  try {
    const { data } = await instance.get("/api/services/app/Term/GetCurrent");
    return { startDate: data.result.startDate, endDate: data.result.endDate };
  } catch {
    return undefined;
  }
};

const loadAttendance = async (instance: AxiosInstance, studentId: string, term: ITermRange) => {
  try {
    const { data } = await instance.get("/api/services/app/Attendance/GetStudentSummary", {
      params: { studentId, startDate: term.startDate, endDate: term.endDate },
    });
    // AttendancePercentage is a non-nullable decimal the backend computes as 0
    // when no register has been captured yet, which would read on the card as
    // total absence. Only treat it as a real figure once there are days behind it.
    const totalDays: number = data.result.totalDays ?? 0;
    if (totalDays <= 0) return undefined;
    return {
      attendancePercentage: data.result.attendancePercentage as number,
      attendanceDaysPresent: data.result.presentCount as number,
      attendanceTotalDays: totalDays,
    };
  } catch {
    return undefined;
  }
};

const loadLatestPublishedReport = async (
  instance: AxiosInstance, studentId: string
): Promise<IChildReportSummary | undefined> => {
  try {
    const { data } = await instance.get("/api/services/app/Report/GetAll", {
      params: {
        StudentId: studentId,
        Status: REPORT_STATUS_PUBLISHED,
        MaxResultCount: 1,
        Sorting: "PublishedDate DESC",
      },
    });
    const latest = (data.result.items as any[])[0];
    if (!latest) return undefined;
    return {
      id: latest.id,
      termName: latest.termName ?? undefined,
      publishedDate: latest.publishedDate ?? undefined,
      overallPercentage: latest.overallPercentage ?? undefined,
    };
  } catch {
    return undefined;
  }
};

const loadUnreadNotifications = async (instance: AxiosInstance): Promise<number | undefined> => {
  try {
    const { data } = await instance.get("/api/services/app/Notification/GetUnreadCount");
    return data.result ?? undefined;
  } catch {
    return undefined;
  }
};

export const ChildrenProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(ChildrenReducer, INITIAL_STATE);
  const instance = useRef(getAxiosInstance()).current;
  const selectedChildIdRef = useRef(state.selectedChildId);
  selectedChildIdRef.current = state.selectedChildId;
  const isLoadingRef = useRef(false);

  const getMyChildrenAsync = useCallback(async () => {
    // The load fans out over several requests, so a second pull-to-refresh
    // landing mid-flight could otherwise overwrite newer state with older.
    if (isLoadingRef.current) return;
    isLoadingRef.current = true;
    dispatch(getPending());
    try {
      // Student/GetAll resolves a parent caller to their own children
      // server-side (MOB-BE-04) and already carries each child's name,
      // admission number, grade and class — so it replaces a per-child
      // Student/Get fan-out. None of these three depend on each other.
      const [childrenResponse, term, unreadNotifications] = await Promise.all([
        instance.get("/api/services/app/Student/GetAll"),
        loadCurrentTermRange(instance),
        loadUnreadNotifications(instance),
      ]);

      // Every per-child metric below is best-effort: a child still lists with
      // its name and class when a summary call fails or has no data yet.
      const myChildren: IChildSummary[] = await Promise.all(
        (childrenResponse.data.result.items as any[]).map(async (student) => {
          const [attendance, latestReport] = await Promise.all([
            term ? loadAttendance(instance, student.id, term) : Promise.resolve(undefined),
            loadLatestPublishedReport(instance, student.id),
          ]);
          return {
            studentId: student.id,
            studentName: student.fullName,
            admissionNumber: student.admissionNumber ?? undefined,
            className: student.currentClassName ?? undefined,
            gradeName: student.currentGradeName ?? undefined,
            attendancePercentage: attendance?.attendancePercentage,
            attendanceDaysPresent: attendance?.attendanceDaysPresent,
            attendanceTotalDays: attendance?.attendanceTotalDays,
            latestReport,
          };
        })
      );

      // Keep the parent's current selection across a refresh; fall back to the
      // first child on first load, or if that child is no longer linked.
      const previous = selectedChildIdRef.current;
      const selectedChildId = myChildren.some((child) => child.studentId === previous)
        ? previous
        : myChildren[0]?.studentId;

      dispatch(getSuccess({ myChildren, selectedChildId, unreadNotifications }));
    } catch {
      dispatch(getError());
    } finally {
      isLoadingRef.current = false;
    }
  }, [instance]);

  const selectChild = useCallback((studentId: string) => {
    dispatch(selectChildAction(studentId));
  }, []);

  const actions = useMemo<IChildrenActionContext>(
    () => ({ getMyChildrenAsync, selectChild }),
    [getMyChildrenAsync, selectChild]
  );

  return (
    <ChildrenStateContext.Provider value={state}>
      <ChildrenActionContext.Provider value={actions}>{children}</ChildrenActionContext.Provider>
    </ChildrenStateContext.Provider>
  );
};

export const useChildrenState = () => useContext(ChildrenStateContext);
export const useChildrenActions = () => {
  const context = useContext(ChildrenActionContext);
  if (!context) throw new Error("useChildrenActions must be used within a ChildrenProvider");
  return context;
};
