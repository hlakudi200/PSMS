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

const loadStudentDetail = async (instance: AxiosInstance, studentId: string) => {
  try {
    const { data } = await instance.get("/api/services/app/Student/Get", { params: { id: studentId } });
    return {
      className: data.result.currentClassName ?? undefined,
      gradeName: data.result.currentGradeName ?? undefined,
    };
  } catch {
    return undefined;
  }
};

const loadAttendance = async (instance: AxiosInstance, studentId: string, term: ITermRange) => {
  try {
    const { data } = await instance.get("/api/services/app/Attendance/GetStudentSummary", {
      params: { studentId, startDate: term.startDate, endDate: term.endDate },
    });
    return {
      attendancePercentage: data.result.attendancePercentage ?? undefined,
      attendanceDaysPresent: data.result.presentCount ?? undefined,
      attendanceTotalDays: data.result.totalDays ?? undefined,
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

const loadUnreadCount = async (instance: AxiosInstance, url: string): Promise<number | undefined> => {
  try {
    const { data } = await instance.get(url);
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

  const getMyChildrenAsync = useCallback(async () => {
    dispatch(getPending());
    try {
      const { data } = await instance.get("/api/services/app/StudentParent/GetMyChildren");
      const links = data.result.items as any[];

      // Every per-child metric below is best-effort: a child still lists with
      // its name and class when a summary call fails or has no data yet.
      const term = await loadCurrentTermRange(instance);

      const myChildren: IChildSummary[] = await Promise.all(
        links.map(async (link) => {
          const [detail, attendance, latestReport] = await Promise.all([
            loadStudentDetail(instance, link.studentId),
            term ? loadAttendance(instance, link.studentId, term) : Promise.resolve(undefined),
            loadLatestPublishedReport(instance, link.studentId),
          ]);
          return {
            studentId: link.studentId,
            studentName: link.studentName,
            admissionNumber: link.studentAdmissionNumber ?? undefined,
            isPrimaryContact: !!link.isPrimaryContact,
            className: detail?.className,
            gradeName: detail?.gradeName,
            attendancePercentage: attendance?.attendancePercentage,
            attendanceDaysPresent: attendance?.attendanceDaysPresent,
            attendanceTotalDays: attendance?.attendanceTotalDays,
            latestReport,
          };
        })
      );

      const [unreadAnnouncements, unreadNotifications] = await Promise.all([
        loadUnreadCount(instance, "/api/services/app/AnnouncementRead/GetUnreadCount"),
        loadUnreadCount(instance, "/api/services/app/Notification/GetUnreadCount"),
      ]);

      // Keep the parent's current selection across a refresh; fall back to the
      // first child on first load, or if that child is no longer linked.
      const previous = selectedChildIdRef.current;
      const selectedChildId = myChildren.some((child) => child.studentId === previous)
        ? previous
        : myChildren[0]?.studentId;

      dispatch(getSuccess({ myChildren, selectedChildId, unreadAnnouncements, unreadNotifications }));
    } catch {
      dispatch(getError());
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
