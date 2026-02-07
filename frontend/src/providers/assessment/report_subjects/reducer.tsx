import { handleActions } from "redux-actions";
import { INITIAL_STATE, IReportSubjectStateContext } from "./context";
import { ReportSubjectActionEnums } from "./actions";

export const ReportSubjectReducer = handleActions<
  IReportSubjectStateContext,
  IReportSubjectStateContext
>(
  {
    [ReportSubjectActionEnums.getReportSubjectPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportSubjectActionEnums.getReportSubjectSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportSubjectActionEnums.getReportSubjectError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ReportSubjectActionEnums.getByReportPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [ReportSubjectActionEnums.getByReportSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [ReportSubjectActionEnums.getByReportError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [ReportSubjectActionEnums.recordMarksPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [ReportSubjectActionEnums.recordMarksSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [ReportSubjectActionEnums.recordMarksError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [ReportSubjectActionEnums.bulkRecordMarksPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [ReportSubjectActionEnums.bulkRecordMarksSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [ReportSubjectActionEnums.bulkRecordMarksError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [ReportSubjectActionEnums.addTeacherCommentPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [ReportSubjectActionEnums.addTeacherCommentSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [ReportSubjectActionEnums.addTeacherCommentError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
  },
  INITIAL_STATE
);
