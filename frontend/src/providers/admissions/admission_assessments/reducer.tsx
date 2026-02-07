import { handleActions } from "redux-actions";
import { INITIAL_STATE, IAdmissionAssessmentStateContext } from "./context";
import { AdmissionAssessmentActionEnums } from "./actions";

export const AdmissionAssessmentReducer = handleActions<
  IAdmissionAssessmentStateContext,
  IAdmissionAssessmentStateContext
>(
  {
    [AdmissionAssessmentActionEnums.getAssessmentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.getAssessmentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.getAssessmentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.getAssessmentByApplicationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.getAssessmentByApplicationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.getAssessmentByApplicationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.getAssessmentsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.getAssessmentsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.getAssessmentsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.scheduleAssessmentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.scheduleAssessmentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.scheduleAssessmentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.recordResultsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.recordResultsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.recordResultsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.cancelAssessmentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.cancelAssessmentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionAssessmentActionEnums.cancelAssessmentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
