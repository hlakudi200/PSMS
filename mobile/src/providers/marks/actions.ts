import { createAction } from "redux-actions";
import type { IAssessmentDetail, IMark, IMarksStateContext, ISubjectTrend, ITerm } from "./context";

type MarksPatch = Partial<IMarksStateContext>;

export enum MarksActionEnums {
  getTermPending = "MARKS_GET_TERM_PENDING",
  getTermSuccess = "MARKS_GET_TERM_SUCCESS",
  getTermError = "MARKS_GET_TERM_ERROR",
  getMarksPending = "MARKS_GET_MARKS_PENDING",
  getMarksSuccess = "MARKS_GET_MARKS_SUCCESS",
  getMarksError = "MARKS_GET_MARKS_ERROR",
  getClassAssessmentsSuccess = "MARKS_GET_CLASS_ASSESSMENTS_SUCCESS",
  getTrendPending = "MARKS_GET_TREND_PENDING",
  getTrendSuccess = "MARKS_GET_TREND_SUCCESS",
  getTrendError = "MARKS_GET_TREND_ERROR",
  /** Payload is the raw IAssessmentDetail; the reducer merges it into assessmentDetailsById. */
  cacheAssessmentDetail = "MARKS_CACHE_ASSESSMENT_DETAIL",
  /** Payload is the raw IMark; the reducer upserts it into marks[] by assessmentId. */
  mergeMark = "MARKS_MERGE_MARK",
}

export const getTermPending = createAction<MarksPatch>(MarksActionEnums.getTermPending, () => ({
  isTermPending: true, isTermError: false,
}));
export const getTermSuccess = createAction<MarksPatch, { currentTerm: ITerm; terms: ITerm[] }>(
  MarksActionEnums.getTermSuccess,
  ({ currentTerm, terms }) => ({ isTermPending: false, isTermError: false, currentTerm, terms })
);
export const getTermError = createAction<MarksPatch>(MarksActionEnums.getTermError, () => ({
  isTermPending: false, isTermError: true,
}));

export const getMarksPending = createAction<MarksPatch>(MarksActionEnums.getMarksPending, () => ({
  isMarksPending: true, isMarksError: false,
}));
export const getMarksSuccess = createAction<MarksPatch, { marks: IMark[]; termId: string; studentId: string }>(
  MarksActionEnums.getMarksSuccess,
  ({ marks, termId, studentId }) => ({
    isMarksPending: false, isMarksError: false, marks, marksTermId: termId, marksStudentId: studentId,
  })
);
export const getMarksError = createAction<MarksPatch>(MarksActionEnums.getMarksError, () => ({
  isMarksPending: false, isMarksError: true,
}));

export const getClassAssessmentsSuccess = createAction<MarksPatch, { classAssessments: IAssessmentDetail[]; key: string }>(
  MarksActionEnums.getClassAssessmentsSuccess,
  ({ classAssessments, key }) => ({ classAssessments, classAssessmentsKey: key })
);

export const getTrendPending = createAction<MarksPatch>(MarksActionEnums.getTrendPending, () => ({
  isTrendPending: true,
}));
export const getTrendSuccess = createAction<MarksPatch, ISubjectTrend>(
  MarksActionEnums.getTrendSuccess,
  (subjectTrend) => ({ isTrendPending: false, subjectTrend })
);
export const getTrendError = createAction<MarksPatch>(MarksActionEnums.getTrendError, () => ({
  isTrendPending: false,
}));

export const cacheAssessmentDetail = createAction<IAssessmentDetail, IAssessmentDetail>(
  MarksActionEnums.cacheAssessmentDetail,
  (detail) => detail
);

export const mergeMark = createAction<IMark, IMark>(MarksActionEnums.mergeMark, (mark) => mark);
