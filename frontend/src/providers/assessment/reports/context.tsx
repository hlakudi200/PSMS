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
}

export interface IReportActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetReportsInput) => void;
  getByStudentTermAsync: (studentId: string, termId: string, reportType: number) => void;
  generateAsync: (input: IGenerateReport) => void;
  submitForApprovalAsync: (id: string) => void;
  approveAsync: (id: string) => void;
  publishAsync: (id: string) => void;
  addTeacherCommentAsync: (id: string, input: IReportComment) => void;
  addPrincipalCommentAsync: (id: string, input: IReportComment) => void;
  acknowledgeByParentAsync: (id: string, input: IReportComment) => void;
  recordPromotionAsync: (id: string, decision: number, promotedToGradeId?: string) => void;
  deleteAsync: (id: string) => void;
  generatePdfAsync: (id: string) => void;
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
