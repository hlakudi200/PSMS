import { useCallback, useContext, useMemo, useReducer, useRef } from "react";
import { getApiErrorMessage, getAxiosInstance } from "../../utils/axios-instance";
import { ReportCardsReducer } from "./reducer";
import {
  INITIAL_STATE, ReportCardsActionContext, ReportCardsStateContext,
  type IReportCard, type IReportCardSummary, type IReportCardsActionContext,
} from "./context";
import {
  acknowledgeError, acknowledgePending, cacheReport, getAllError, getAllPending, getAllSuccess,
  getError, getPending, pdfError, pdfPending, pdfSuccess,
} from "./actions";

const ENDPOINT = "/api/services/app/Report";
const REPORT_STATUS_PUBLISHED = 5;

const optionalNumber = (value: unknown) => (value == null ? undefined : Number(value));

const mapSummary = (dto: any): IReportCardSummary => ({
  id: dto.id,
  studentId: dto.studentId,
  reportType: dto.reportType,
  termName: dto.termName ?? undefined,
  academicYearName: dto.academicYearName ?? undefined,
  overallPercentage: optionalNumber(dto.overallPercentage),
  overallAchievementLevel: dto.overallAchievementLevel ?? undefined,
  classPosition: dto.classPosition ?? undefined,
  publishedDate: dto.publishedDate ?? undefined,
  reportsPercentages: dto.reportsPercentages !== false,
  hasPdf: !!dto.hasPdf,
});

const mapReport = (dto: any): IReportCard => ({
  ...mapSummary(dto),
  reportCardNumber: dto.reportCardNumber ?? undefined,
  className: dto.className ?? undefined,
  gradeName: dto.gradeName ?? undefined,
  totalStudentsInClass: dto.totalStudentsInClass ?? undefined,
  daysPresent: dto.daysPresent ?? 0,
  daysAbsent: dto.daysAbsent ?? 0,
  daysLate: dto.daysLate ?? 0,
  daysInTerm: dto.daysInTerm ?? undefined,
  conductRating: dto.conductRating ?? undefined,
  diligenceRating: dto.diligenceRating ?? undefined,
  behaviourComments: dto.behaviourComments || undefined,
  teacherComment: dto.teacherComment || undefined,
  principalComment: dto.principalComment || undefined,
  promotionDecision: dto.promotionDecision ?? undefined,
  promotedToGradeName: dto.promotedToGradeName ?? undefined,
  parentComment: dto.parentComment || undefined,
  parentAcknowledgedDate: dto.parentAcknowledgedDate ?? undefined,
  subjects: ((dto.subjectReports ?? []) as any[]).map((subject) => ({
    id: subject.id,
    subjectName: subject.subjectName,
    teacherName: subject.teacherName ?? undefined,
    finalMark: optionalNumber(subject.finalMark),
    achievementLevel: subject.achievementLevel ?? undefined,
    teacherComment: subject.teacherComment || undefined,
    awaitsExternalExamination: !!subject.awaitsExternalExamination,
  })),
});

export const ReportCardsProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(ReportCardsReducer, INITIAL_STATE);
  const instance = useRef(getAxiosInstance()).current;
  // A parent can switch child before the last list lands; only the latest may write.
  const latestListRequestRef = useRef(0);

  const getReportsAsync = useCallback(async (studentId: string) => {
    const requestId = ++latestListRequestRef.current;
    dispatch(getAllPending());
    try {
      // The backend already limits a family to published cards; the filter says so plainly.
      const { data } = await instance.get(`${ENDPOINT}/GetAll`, {
        params: {
          StudentId: studentId,
          Status: REPORT_STATUS_PUBLISHED,
          Sorting: "PublishedDate DESC",
          MaxResultCount: 100,
        },
      });
      if (requestId !== latestListRequestRef.current) return;
      dispatch(getAllSuccess({ reports: (data.result.items as any[]).map(mapSummary), studentId }));
    } catch {
      if (requestId === latestListRequestRef.current) dispatch(getAllError());
    }
  }, [instance]);

  const getReportAsync = useCallback(async (id: string) => {
    dispatch(getPending());
    try {
      const { data } = await instance.get(`${ENDPOINT}/Get`, { params: { id } });
      dispatch(cacheReport(mapReport(data.result)));
    } catch {
      dispatch(getError());
    }
  }, [instance]);

  const acknowledgeAsync = useCallback(async (id: string, comment?: string) => {
    dispatch(acknowledgePending());
    try {
      // ABP's dynamic API: the id travels in the query, the DTO in the body.
      const { data } = await instance.post(`${ENDPOINT}/AcknowledgeByParent`, { comment }, { params: { id } });
      dispatch(cacheReport(mapReport(data.result)));
      return true;
    } catch (error) {
      dispatch(acknowledgeError(getApiErrorMessage(error, "Couldn't record your acknowledgement.")));
      return false;
    }
  }, [instance]);

  const getPdfUrlAsync = useCallback(async (id: string) => {
    dispatch(pdfPending());
    try {
      const { data } = await instance.get(`${ENDPOINT}/GetReportPdfUrl`, { params: { id } });
      dispatch(pdfSuccess());
      // A signed link that expires in minutes — hand it straight back, never keep it.
      return data.result as string;
    } catch (error) {
      dispatch(pdfError(getApiErrorMessage(error, "Couldn't open the report card PDF.")));
      return undefined;
    }
  }, [instance]);

  const actions = useMemo<IReportCardsActionContext>(
    () => ({ getReportsAsync, getReportAsync, acknowledgeAsync, getPdfUrlAsync }),
    [getReportsAsync, getReportAsync, acknowledgeAsync, getPdfUrlAsync]
  );

  return (
    <ReportCardsStateContext.Provider value={state}>
      <ReportCardsActionContext.Provider value={actions}>{children}</ReportCardsActionContext.Provider>
    </ReportCardsStateContext.Provider>
  );
};

export const useReportCardsState = () => useContext(ReportCardsStateContext);
export const useReportCardsActions = () => {
  const context = useContext(ReportCardsActionContext);
  if (!context) throw new Error("useReportCardsActions must be used within a ReportCardsProvider");
  return context;
};
