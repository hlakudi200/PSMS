import { handleActions } from "redux-actions";
import { INITIAL_STATE, IReportStateContext } from "./context";
import { ReportActionEnums } from "./actions";

export const ReportReducer = handleActions<
  IReportStateContext,
  IReportStateContext
>(
  {
    [ReportActionEnums.getReportPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.getReportSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.getReportError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.getAllReportsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.getAllReportsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.getAllReportsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.getByStudentTermPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.getByStudentTermSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.getByStudentTermError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.generateReportPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.generateReportSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.generateReportError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.submitForApprovalPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.submitForApprovalSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.submitForApprovalError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.publishReportPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.publishReportSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.publishReportError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.addTeacherCommentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.addTeacherCommentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.addTeacherCommentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.addPrincipalCommentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.addPrincipalCommentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.addPrincipalCommentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.acknowledgeByParentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.acknowledgeByParentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.acknowledgeByParentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.getPromotionAdvicePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.getPromotionAdviceSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.getPromotionAdviceError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.recordPromotionPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.recordPromotionSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.recordPromotionError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.deleteReportPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.deleteReportSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.deleteReportError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.generatePdfPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.generatePdfSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.generatePdfError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.bulkGeneratePdfsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.bulkGeneratePdfsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.bulkGeneratePdfsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.previewBulkGeneratePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.previewBulkGenerateSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.previewBulkGenerateError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.bulkGenerateReportsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.bulkGenerateReportsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportActionEnums.bulkGenerateReportsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
