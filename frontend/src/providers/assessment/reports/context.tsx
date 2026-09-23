'use client'
import { createContext } from "react";
import {
  IReport,
  IReportList,
  IGenerateReport,
  IGetReportsInput,
  IReportComment,
  IBulkGenerateReportPdfsInput,
  IBulkGenerateReports,
  IBulkGenerateReportsResult,
  IPromotionAdvice,
  IRecordPromotion,
  IRecordConduct,
} from "../shared/interfaces";

export interface IReportStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  report?: IReport;
  reports?: IReportList[];
  totalCount?: number;
  /** RC-01: the last bulk preview, and the last bulk run's per-learner result. */
  bulkPreview?: IBulkGenerateReportsResult;
  bulkResult?: IBulkGenerateReportsResult;
  /** RC-16: the last promotion evaluation loaded, for the decision screen. */
  promotionAdvice?: IPromotionAdvice;
}

export interface IReportActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetReportsInput) => void;
  getByStudentTermAsync: (studentId: string, termId: string, reportType: number) => void;
  generateAsync: (input: IGenerateReport) => void;
  submitForApprovalAsync: (id: string) => void;
  publishAsync: (id: string) => void;
  addTeacherCommentAsync: (id: string, input: IReportComment) => void;
  addPrincipalCommentAsync: (id: string, input: IReportComment) => void;
  acknowledgeByParentAsync: (id: string, input: IReportComment) => void;
  recordPromotionAsync: (id: string, decision: number, promotedToGradeId?: string) => void;
  /** RC-16: what the national requirements make of this learner's year. */
  getPromotionAdviceAsync: (id: string) => void;
  /** RC-16: records the decision, with the destination grade and a reason. */
  recordPromotionDecisionAsync: (input: IRecordPromotion) => void;
  /** RC-17: records the conduct and diligence ratings (RE-002). */
  recordConductAsync: (input: IRecordConduct) => void;
  /** RC-17: the class teacher signs the card off (RE-003). */
  signAsTeacherAsync: (id: string) => void;
  /** RC-17: the principal signs the card off (RE-003). */
  signAsPrincipalAsync: (id: string) => void;
  deleteAsync: (id: string) => void;
  generatePdfAsync: (id: string) => void;
  /**
   * Returns a short-lived signed link. Deliberately returns the value instead
   * of putting it in state: it expires in minutes and must not be cached or
   * re-rendered from (RC-04).
   */
  getPdfUrlAsync: (id: string) => Promise<string | undefined>;
  bulkGeneratePdfsAsync: (input: IBulkGenerateReportPdfsInput) => void;
  previewBulkGenerateAsync: (input: IBulkGenerateReports) => void;
  bulkGenerateAsync: (input: IBulkGenerateReports) => void;
}

export const INITIAL_STATE: IReportStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const ReportStateContext =
  createContext<IReportStateContext>(INITIAL_STATE);

export const ReportActionContext = createContext<
  IReportActionContext | undefined
>(undefined);
