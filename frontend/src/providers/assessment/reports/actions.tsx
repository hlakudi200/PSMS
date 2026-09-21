import { createAction } from "redux-actions";
import { IReportStateContext } from "./context";
import {
  IReport,
  IReportList,
  IPagedResult,
  IBulkGenerateReportsResult,
} from "../shared/interfaces";

export enum ReportActionEnums {
  getReportPending = "GET_REPORT_PENDING",
  getReportSuccess = "GET_REPORT_SUCCESS",
  getReportError = "GET_REPORT_ERROR",

  getAllReportsPending = "GET_ALL_REPORTS_PENDING",
  getAllReportsSuccess = "GET_ALL_REPORTS_SUCCESS",
  getAllReportsError = "GET_ALL_REPORTS_ERROR",

  getByStudentTermPending = "GET_REPORT_BY_STUDENT_TERM_PENDING",
  getByStudentTermSuccess = "GET_REPORT_BY_STUDENT_TERM_SUCCESS",
  getByStudentTermError = "GET_REPORT_BY_STUDENT_TERM_ERROR",

  generateReportPending = "GENERATE_REPORT_PENDING",
  generateReportSuccess = "GENERATE_REPORT_SUCCESS",
  generateReportError = "GENERATE_REPORT_ERROR",

  submitForApprovalPending = "SUBMIT_FOR_APPROVAL_PENDING",
  submitForApprovalSuccess = "SUBMIT_FOR_APPROVAL_SUCCESS",
  submitForApprovalError = "SUBMIT_FOR_APPROVAL_ERROR",

  approveReportPending = "APPROVE_REPORT_PENDING",
  approveReportSuccess = "APPROVE_REPORT_SUCCESS",
  approveReportError = "APPROVE_REPORT_ERROR",

  publishReportPending = "PUBLISH_REPORT_PENDING",
  publishReportSuccess = "PUBLISH_REPORT_SUCCESS",
  publishReportError = "PUBLISH_REPORT_ERROR",

  addTeacherCommentPending = "ADD_TEACHER_COMMENT_PENDING",
  addTeacherCommentSuccess = "ADD_TEACHER_COMMENT_SUCCESS",
  addTeacherCommentError = "ADD_TEACHER_COMMENT_ERROR",

  addPrincipalCommentPending = "ADD_PRINCIPAL_COMMENT_PENDING",
  addPrincipalCommentSuccess = "ADD_PRINCIPAL_COMMENT_SUCCESS",
  addPrincipalCommentError = "ADD_PRINCIPAL_COMMENT_ERROR",

  acknowledgeByParentPending = "ACKNOWLEDGE_BY_PARENT_PENDING",
  acknowledgeByParentSuccess = "ACKNOWLEDGE_BY_PARENT_SUCCESS",
  acknowledgeByParentError = "ACKNOWLEDGE_BY_PARENT_ERROR",

  recordPromotionPending = "RECORD_PROMOTION_PENDING",
  recordPromotionSuccess = "RECORD_PROMOTION_SUCCESS",
  recordPromotionError = "RECORD_PROMOTION_ERROR",

  deleteReportPending = "DELETE_REPORT_PENDING",
  deleteReportSuccess = "DELETE_REPORT_SUCCESS",
  deleteReportError = "DELETE_REPORT_ERROR",

  generatePdfPending = "GENERATE_PDF_PENDING",
  generatePdfSuccess = "GENERATE_PDF_SUCCESS",
  generatePdfError = "GENERATE_PDF_ERROR",

  bulkGeneratePdfsPending = "BULK_GENERATE_PDFS_PENDING",
  bulkGeneratePdfsSuccess = "BULK_GENERATE_PDFS_SUCCESS",
  bulkGeneratePdfsError = "BULK_GENERATE_PDFS_ERROR",

  previewBulkGeneratePending = "PREVIEW_BULK_GENERATE_PENDING",
  previewBulkGenerateSuccess = "PREVIEW_BULK_GENERATE_SUCCESS",
  previewBulkGenerateError = "PREVIEW_BULK_GENERATE_ERROR",

  bulkGenerateReportsPending = "BULK_GENERATE_REPORTS_PENDING",
  bulkGenerateReportsSuccess = "BULK_GENERATE_REPORTS_SUCCESS",
  bulkGenerateReportsError = "BULK_GENERATE_REPORTS_ERROR",
}

// Get Single Report Actions
export const getReportPending = createAction<IReportStateContext>(
  ReportActionEnums.getReportPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getReportSuccess = createAction<IReportStateContext, IReport>(
  ReportActionEnums.getReportSuccess,
  (report: IReport) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    report,
  })
);

export const getReportError = createAction<IReportStateContext>(
  ReportActionEnums.getReportError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All Reports Actions
export const getAllReportsPending = createAction<IReportStateContext>(
  ReportActionEnums.getAllReportsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllReportsSuccess = createAction<
  IReportStateContext,
  IPagedResult<IReportList>
>(
  ReportActionEnums.getAllReportsSuccess,
  (result: IPagedResult<IReportList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    reports: result.items,
    totalCount: result.totalCount,
  })
);

export const getAllReportsError = createAction<IReportStateContext>(
  ReportActionEnums.getAllReportsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Student Term Actions
export const getByStudentTermPending = createAction<IReportStateContext>(
  ReportActionEnums.getByStudentTermPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByStudentTermSuccess = createAction<IReportStateContext, IReport>(
  ReportActionEnums.getByStudentTermSuccess,
  (report: IReport) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    report,
  })
);

export const getByStudentTermError = createAction<IReportStateContext>(
  ReportActionEnums.getByStudentTermError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Generate Report Actions
export const generateReportPending = createAction<IReportStateContext>(
  ReportActionEnums.generateReportPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const generateReportSuccess = createAction<IReportStateContext, IReport>(
  ReportActionEnums.generateReportSuccess,
  (report: IReport) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    report,
  })
);

export const generateReportError = createAction<IReportStateContext>(
  ReportActionEnums.generateReportError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Submit For Approval Actions
export const submitForApprovalPending = createAction<IReportStateContext>(
  ReportActionEnums.submitForApprovalPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const submitForApprovalSuccess = createAction<IReportStateContext, IReport>(
  ReportActionEnums.submitForApprovalSuccess,
  (report: IReport) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    report,
  })
);

export const submitForApprovalError = createAction<IReportStateContext>(
  ReportActionEnums.submitForApprovalError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Approve Report Actions
export const approveReportPending = createAction<IReportStateContext>(
  ReportActionEnums.approveReportPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const approveReportSuccess = createAction<IReportStateContext, IReport>(
  ReportActionEnums.approveReportSuccess,
  (report: IReport) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    report,
  })
);

export const approveReportError = createAction<IReportStateContext>(
  ReportActionEnums.approveReportError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Publish Report Actions
export const publishReportPending = createAction<IReportStateContext>(
  ReportActionEnums.publishReportPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const publishReportSuccess = createAction<IReportStateContext, IReport>(
  ReportActionEnums.publishReportSuccess,
  (report: IReport) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    report,
  })
);

export const publishReportError = createAction<IReportStateContext>(
  ReportActionEnums.publishReportError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Add Teacher Comment Actions
export const addTeacherCommentPending = createAction<IReportStateContext>(
  ReportActionEnums.addTeacherCommentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const addTeacherCommentSuccess = createAction<IReportStateContext, IReport>(
  ReportActionEnums.addTeacherCommentSuccess,
  (report: IReport) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    report,
  })
);

export const addTeacherCommentError = createAction<IReportStateContext>(
  ReportActionEnums.addTeacherCommentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Add Principal Comment Actions
export const addPrincipalCommentPending = createAction<IReportStateContext>(
  ReportActionEnums.addPrincipalCommentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const addPrincipalCommentSuccess = createAction<IReportStateContext, IReport>(
  ReportActionEnums.addPrincipalCommentSuccess,
  (report: IReport) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    report,
  })
);

export const addPrincipalCommentError = createAction<IReportStateContext>(
  ReportActionEnums.addPrincipalCommentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Acknowledge By Parent Actions
export const acknowledgeByParentPending = createAction<IReportStateContext>(
  ReportActionEnums.acknowledgeByParentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const acknowledgeByParentSuccess = createAction<IReportStateContext, IReport>(
  ReportActionEnums.acknowledgeByParentSuccess,
  (report: IReport) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    report,
  })
);

export const acknowledgeByParentError = createAction<IReportStateContext>(
  ReportActionEnums.acknowledgeByParentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Record Promotion Actions
export const recordPromotionPending = createAction<IReportStateContext>(
  ReportActionEnums.recordPromotionPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const recordPromotionSuccess = createAction<IReportStateContext, IReport>(
  ReportActionEnums.recordPromotionSuccess,
  (report: IReport) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    report,
  })
);

export const recordPromotionError = createAction<IReportStateContext>(
  ReportActionEnums.recordPromotionError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Report Actions
export const deleteReportPending = createAction<IReportStateContext>(
  ReportActionEnums.deleteReportPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteReportSuccess = createAction<IReportStateContext>(
  ReportActionEnums.deleteReportSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteReportError = createAction<IReportStateContext>(
  ReportActionEnums.deleteReportError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Generate PDF Actions
export const generatePdfPending = createAction<IReportStateContext>(
  ReportActionEnums.generatePdfPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const generatePdfSuccess = createAction<IReportStateContext>(
  ReportActionEnums.generatePdfSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);

export const generatePdfError = createAction<IReportStateContext>(
  ReportActionEnums.generatePdfError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Bulk Generate PDFs Actions
export const bulkGeneratePdfsPending = createAction<IReportStateContext>(
  ReportActionEnums.bulkGeneratePdfsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const bulkGeneratePdfsSuccess = createAction<IReportStateContext>(
  ReportActionEnums.bulkGeneratePdfsSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);

export const bulkGeneratePdfsError = createAction<IReportStateContext>(
  ReportActionEnums.bulkGeneratePdfsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// RC-01: bulk generation. Both the preview and the run land their result in
// state so the screen can render the per-learner outcome list.
export const previewBulkGeneratePending = createAction<IReportStateContext>(
  ReportActionEnums.previewBulkGeneratePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const previewBulkGenerateSuccess = createAction<
  IReportStateContext,
  IBulkGenerateReportsResult
>(ReportActionEnums.previewBulkGenerateSuccess, (bulkPreview) => ({
  isPending: false,
  isSuccess: true,
  isError: false,
  bulkPreview,
}));

export const previewBulkGenerateError = createAction<IReportStateContext>(
  ReportActionEnums.previewBulkGenerateError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const bulkGenerateReportsPending = createAction<IReportStateContext>(
  ReportActionEnums.bulkGenerateReportsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const bulkGenerateReportsSuccess = createAction<
  IReportStateContext,
  IBulkGenerateReportsResult
>(ReportActionEnums.bulkGenerateReportsSuccess, (bulkResult) => ({
  isPending: false,
  isSuccess: true,
  isError: false,
  bulkResult,
}));

export const bulkGenerateReportsError = createAction<IReportStateContext>(
  ReportActionEnums.bulkGenerateReportsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
