import { useCallback, useContext, useMemo, useReducer, useRef } from "react";
import { getAxiosInstance } from "../../utils/axios-instance";
import { SubjectsReducer } from "./reducer";
import {
  INITIAL_STATE, SubjectsActionContext, SubjectsStateContext,
  type IMySubject, type ISubjectsActionContext,
} from "./context";
import { getError, getPending, getSuccess } from "./actions";

export const SubjectsProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(SubjectsReducer, INITIAL_STATE);
  const instance = useRef(getAxiosInstance()).current;

  const getMySubjectsAsync = useCallback(async (studentId: string, classId: string) => {
    dispatch(getPending());
    try {
      const { data: yearData } = await instance.get("/api/services/app/AcademicYear/GetCurrent");
      const academicYearId: string = yearData.result.id;

      const [enrollResponse, classSubjectResponse] = await Promise.all([
        instance.get("/api/services/app/StudentSubject/GetByStudentAndYear", {
          params: { studentId, academicYearId },
        }),
        instance.get("/api/services/app/ClassSubject/GetByClass", { params: { classId } }),
      ]);

      const teachingBySubjectId = new Map<string, { teacherName?: string; className?: string }>();
      for (const cs of classSubjectResponse.data.result.items as any[]) {
        teachingBySubjectId.set(cs.subjectId, {
          teacherName: cs.teacherName ?? undefined,
          className: cs.className ?? undefined,
        });
      }

      const subjects: IMySubject[] = (enrollResponse.data.result.items as any[])
        .filter((e) => e.isActive)
        .map((e) => {
          const teaching = teachingBySubjectId.get(e.subjectId);
          return {
            subjectId: e.subjectId,
            subjectName: e.subjectName,
            subjectCode: e.subjectCode ?? undefined,
            teacherName: teaching?.teacherName,
            className: teaching?.className,
          };
        })
        .sort((a, b) => a.subjectName.localeCompare(b.subjectName));

      dispatch(getSuccess(subjects));
    } catch {
      dispatch(getError());
    }
  }, [instance]);

  const actions = useMemo<ISubjectsActionContext>(() => ({ getMySubjectsAsync }), [getMySubjectsAsync]);

  return (
    <SubjectsStateContext.Provider value={state}>
      <SubjectsActionContext.Provider value={actions}>{children}</SubjectsActionContext.Provider>
    </SubjectsStateContext.Provider>
  );
};

export const useSubjectsState = () => useContext(SubjectsStateContext);
export const useSubjectsActions = () => {
  const context = useContext(SubjectsActionContext);
  if (!context) throw new Error("useSubjectsActions must be used within a SubjectsProvider");
  return context;
};
