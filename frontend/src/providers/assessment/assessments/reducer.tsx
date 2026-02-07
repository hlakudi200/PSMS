import { handleActions } from "redux-actions";
import { INITIAL_STATE, IAssessmentStateContext } from "./context";
import { AssessmentActionEnums } from "./actions";

export const AssessmentReducer = handleActions<
  IAssessmentStateContext,
  IAssessmentStateContext
>(
  {
    [AssessmentActionEnums.getAssessmentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.getAssessmentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.getAssessmentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.getAllAssessmentsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.getAllAssessmentsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.getAllAssessmentsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.createAssessmentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.createAssessmentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.createAssessmentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.updateAssessmentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.updateAssessmentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.updateAssessmentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.deleteAssessmentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.deleteAssessmentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.deleteAssessmentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.publishAssessmentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.publishAssessmentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.publishAssessmentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.unpublishAssessmentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.unpublishAssessmentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.unpublishAssessmentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.releaseMarksPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.releaseMarksSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentActionEnums.releaseMarksError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
