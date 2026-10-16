import { createAction } from "redux-actions";
import type { IReportCard, IReportCardSummary, IReportCardsStateContext } from "./context";

type ReportCardsPatch = Partial<IReportCardsStateContext>;

export enum ReportCardsActionEnums {
  getAllPending = "REPORT_CARDS_GET_ALL_PENDING",
  getAllSuccess = "REPORT_CARDS_GET_ALL_SUCCESS",
  getAllError = "REPORT_CARDS_GET_ALL_ERROR",
  getPending = "REPORT_CARDS_GET_PENDING",
  getError = "REPORT_CARDS_GET_ERROR",
  /** Payload is an IReportCard; the reducer merges it into reportsById. */
  cacheReport = "REPORT_CARDS_CACHE_REPORT",
  acknowledgePending = "REPORT_CARDS_ACKNOWLEDGE_PENDING",
  acknowledgeError = "REPORT_CARDS_ACKNOWLEDGE_ERROR",
  pdfPending = "REPORT_CARDS_PDF_PENDING",
  pdfSuccess = "REPORT_CARDS_PDF_SUCCESS",
  pdfError = "REPORT_CARDS_PDF_ERROR",
}

export const getAllPending = createAction<ReportCardsPatch>(ReportCardsActionEnums.getAllPending, () => ({
  isPending: true, isSuccess: false, isError: false,
}));
export const getAllSuccess = createAction<ReportCardsPatch, { reports: IReportCardSummary[]; studentId: string }>(
  ReportCardsActionEnums.getAllSuccess,
  ({ reports, studentId }) => ({ isPending: false, isSuccess: true, isError: false, reports, reportsStudentId: studentId })
);
export const getAllError = createAction<ReportCardsPatch>(ReportCardsActionEnums.getAllError, () => ({
  isPending: false, isSuccess: false, isError: true,
}));

export const getPending = createAction<ReportCardsPatch>(ReportCardsActionEnums.getPending, () => ({
  isDetailPending: true, isDetailError: false,
}));
export const getError = createAction<ReportCardsPatch>(ReportCardsActionEnums.getError, () => ({
  isDetailPending: false, isDetailError: true,
}));
export const cacheReport = createAction<IReportCard, IReportCard>(ReportCardsActionEnums.cacheReport, (report) => report);

export const acknowledgePending = createAction<ReportCardsPatch>(ReportCardsActionEnums.acknowledgePending, () => ({
  isAcknowledgePending: true, acknowledgeErrorMessage: undefined,
}));
export const acknowledgeError = createAction<ReportCardsPatch, string>(
  ReportCardsActionEnums.acknowledgeError,
  (acknowledgeErrorMessage) => ({ isAcknowledgePending: false, acknowledgeErrorMessage })
);

export const pdfPending = createAction<ReportCardsPatch>(ReportCardsActionEnums.pdfPending, () => ({
  isPdfPending: true, pdfErrorMessage: undefined,
}));
export const pdfSuccess = createAction<ReportCardsPatch>(ReportCardsActionEnums.pdfSuccess, () => ({
  isPdfPending: false,
}));
export const pdfError = createAction<ReportCardsPatch, string>(
  ReportCardsActionEnums.pdfError,
  (pdfErrorMessage) => ({ isPdfPending: false, pdfErrorMessage })
);
