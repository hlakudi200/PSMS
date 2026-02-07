import { handleActions } from "redux-actions";
import { INITIAL_STATE, IAssessmentQuestionStateContext } from "./context";
import { AssessmentQuestionActionEnums } from "./actions";

export const AssessmentQuestionReducer = handleActions<
  IAssessmentQuestionStateContext,
  IAssessmentQuestionStateContext
>(
  {
    [AssessmentQuestionActionEnums.getQuestionPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.getQuestionSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.getQuestionError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.getByAssessmentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.getByAssessmentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.getByAssessmentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.createQuestionPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.createQuestionSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.createQuestionError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.updateQuestionPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.updateQuestionSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.updateQuestionError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.deleteQuestionPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.deleteQuestionSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.deleteQuestionError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.reorderPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.reorderSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AssessmentQuestionActionEnums.reorderError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
