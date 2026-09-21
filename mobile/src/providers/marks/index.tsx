import { useCallback, useContext, useMemo, useReducer, useRef } from "react";
import { getAxiosInstance } from "../../utils/axios-instance";
import { MarksReducer } from "./reducer";
import {
  INITIAL_STATE, MarksActionContext, MarksStateContext,
  type IAssessmentDetail, type IMark, type IMarksActionContext, type ITerm,
} from "./context";
import {
  cacheAssessmentDetail, getClassAssessmentsSuccess, getMarksError, getMarksPending, getMarksSuccess,
  getTermError, getTermPending, getTermSuccess, mergeMark,
} from "./actions";

const mapTerm = (dto: any): ITerm => ({
  id: dto.id,
  academicYearId: dto.academicYearId,
  academicYearName: dto.academicYearName,
  termNumber: dto.termNumber,
  termName: dto.termName,
  startDate: dto.startDate,
  endDate: dto.endDate,
  isCurrent: dto.isCurrent,
});

const mapMark = (dto: any): IMark => ({
  id: dto.id,
  assessmentId: dto.assessmentId,
  rawMark: dto.rawMark ?? undefined,
  percentage: dto.percentage ?? undefined,
  achievementLevel: dto.achievementLevel ?? undefined,
  status: dto.status,
  wasAbsent: dto.wasAbsent,
  isReassessment: dto.isReassessment,
  feedback: dto.feedback ?? undefined,
  assessmentName: dto.assessmentName,
});

const mapAssessmentDetail = (dto: any): IAssessmentDetail => ({
  id: dto.id,
  name: dto.name,
  description: dto.description ?? undefined,
  assessmentType: dto.assessmentType,
  maxMarks: dto.maxMarks,
  weight: dto.weight,
  passPercentage: dto.passPercentage,
  scheduledDate: dto.scheduledDate ?? undefined,
  dueDate: dto.dueDate ?? undefined,
  instructions: dto.instructions ?? undefined,
  isPublished: dto.isPublished,
  marksReleased: dto.marksReleased,
  className: dto.className,
  subjectName: dto.subjectName,
  termName: dto.termName,
});

export const MarksProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(MarksReducer, INITIAL_STATE);
  const instance = useRef(getAxiosInstance()).current;
  const assessmentDetailsRef = useRef(state.assessmentDetailsById);
  assessmentDetailsRef.current = state.assessmentDetailsById;
  const marksRef = useRef(state.marks);
  marksRef.current = state.marks;

  const getAssessmentDetailAsync = useCallback(async (assessmentId: string) => {
    const cached = assessmentDetailsRef.current[assessmentId];
    if (cached) return cached;
    try {
      const { data } = await instance.get("/api/services/app/Assessment/Get", { params: { id: assessmentId } });
      const detail = mapAssessmentDetail(data.result);
      dispatch(cacheAssessmentDetail(detail));
      return detail;
    } catch {
      return undefined;
    }
  }, [instance]);

  const getCurrentTermAsync = useCallback(async () => {
    dispatch(getTermPending());
    try {
      const { data: currentData } = await instance.get("/api/services/app/Term/GetCurrent");
      const currentTerm = mapTerm(currentData.result);
      const { data: yearData } = await instance.get("/api/services/app/Term/GetByAcademicYear", {
        params: { academicYearId: currentTerm.academicYearId },
      });
      const terms = (yearData.result.items as any[]).map(mapTerm).sort((a, b) => a.termNumber - b.termNumber);
      dispatch(getTermSuccess({ currentTerm, terms }));
    } catch {
      dispatch(getTermError());
    }
  }, [instance]);

  const getMarksByStudentAsync = useCallback(async (studentId: string, termId: string) => {
    dispatch(getMarksPending());
    try {
      const { data } = await instance.get("/api/services/app/Mark/GetByStudent", {
        params: { studentId, termId },
      });
      const marks = (data.result.items as any[]).map(mapMark);
      dispatch(getMarksSuccess({ marks, termId }));

      const uniqueAssessmentIds = [...new Set(marks.map((m) => m.assessmentId))];
      await Promise.all(uniqueAssessmentIds.map((id) => getAssessmentDetailAsync(id)));
    } catch {
      dispatch(getMarksError());
    }
  }, [instance, getAssessmentDetailAsync]);

  const getClassAssessmentsAsync = useCallback(async (classId: string, termId: string) => {
    try {
      const { data } = await instance.get("/api/services/app/Assessment/GetAll", {
        params: { ClassId: classId, TermId: termId, IsPublished: true, MaxResultCount: 200 },
      });
      const items: IAssessmentDetail[] = (data.result.items as any[]).map((dto) => ({
        id: dto.id,
        name: dto.name,
        assessmentType: dto.assessmentType,
        maxMarks: dto.maxMarks,
        weight: dto.weight,
        passPercentage: dto.passPercentage,
        scheduledDate: dto.scheduledDate ?? undefined,
        dueDate: dto.dueDate ?? undefined,
        isPublished: dto.isPublished,
        marksReleased: dto.marksReleased,
        className: dto.className,
        subjectName: dto.subjectName,
        termName: dto.termName,
      }));
      dispatch(getClassAssessmentsSuccess(items));
    } catch {
      // Non-fatal: assessments the teacher hasn't captured a mark for yet just won't show
      // as placeholders; everything already-marked is unaffected.
    }
  }, [instance]);

  const ensureMarkForAssessmentAsync = useCallback(async (assessmentId: string) => {
    const existing = marksRef.current?.find((m) => m.assessmentId === assessmentId);
    if (existing) return existing;
    try {
      const { data } = await instance.get("/api/services/app/Mark/GetByAssessment", { params: { assessmentId } });
      const items = (data.result.items as any[]).map(mapMark);
      const mine = items[0];
      if (mine) dispatch(mergeMark(mine));
      return mine;
    } catch {
      return undefined;
    }
  }, [instance]);

  const actions = useMemo<IMarksActionContext>(() => ({
    getCurrentTermAsync, getMarksByStudentAsync, getClassAssessmentsAsync, getAssessmentDetailAsync, ensureMarkForAssessmentAsync,
  }), [getCurrentTermAsync, getMarksByStudentAsync, getClassAssessmentsAsync, getAssessmentDetailAsync, ensureMarkForAssessmentAsync]);

  return (
    <MarksStateContext.Provider value={state}>
      <MarksActionContext.Provider value={actions}>{children}</MarksActionContext.Provider>
    </MarksStateContext.Provider>
  );
};

export const useMarksState = () => useContext(MarksStateContext);
export const useMarksActions = () => {
  const context = useContext(MarksActionContext);
  if (!context) throw new Error("useMarksActions must be used within a MarksProvider");
  return context;
};
