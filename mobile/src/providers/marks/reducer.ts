import { handleActions } from "redux-actions";
import { MarksActionEnums } from "./actions";
import { INITIAL_STATE, type IAssessmentDetail, type IMark, type IMarksStateContext } from "./context";

const shallowMergeKeys = Object.values(MarksActionEnums).filter(
  (k) => k !== MarksActionEnums.cacheAssessmentDetail && k !== MarksActionEnums.mergeMark
);

const shallowMergeHandlers = Object.fromEntries(
  shallowMergeKeys.map((action) => [
    action,
    (state: IMarksStateContext, a: { payload: Partial<IMarksStateContext> }) => ({ ...state, ...a.payload }),
  ])
);

export const MarksReducer = handleActions<IMarksStateContext, any>(
  {
    ...shallowMergeHandlers,
    [MarksActionEnums.cacheAssessmentDetail]: (state, action: { payload: IAssessmentDetail }) => ({
      ...state,
      assessmentDetailsById: { ...state.assessmentDetailsById, [action.payload.id]: action.payload },
    }),
    [MarksActionEnums.mergeMark]: (state, action: { payload: IMark }) => {
      const existing = state.marks ?? [];
      const index = existing.findIndex((m) => m.assessmentId === action.payload.assessmentId);
      const marks = index >= 0
        ? existing.map((m, i) => (i === index ? action.payload : m))
        : [...existing, action.payload];
      return { ...state, marks };
    },
  },
  INITIAL_STATE
);
