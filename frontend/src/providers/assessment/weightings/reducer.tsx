import { handleActions } from "redux-actions";
import { INITIAL_STATE, IAssessmentWeightingStateContext } from "./context";
import { AssessmentWeightingActionEnums } from "./actions";

const merge = (
  state: IAssessmentWeightingStateContext,
  action: { payload: IAssessmentWeightingStateContext }
) => ({ ...state, ...action.payload });

export const AssessmentWeightingReducer = handleActions<
  IAssessmentWeightingStateContext,
  IAssessmentWeightingStateContext
>(
  {
    [AssessmentWeightingActionEnums.getAllPending]: merge,
    [AssessmentWeightingActionEnums.getAllSuccess]: merge,
    [AssessmentWeightingActionEnums.getAllError]: merge,
    [AssessmentWeightingActionEnums.updatePending]: merge,
    [AssessmentWeightingActionEnums.updateSuccess]: merge,
    [AssessmentWeightingActionEnums.updateError]: merge,
    [AssessmentWeightingActionEnums.resetPending]: merge,
    [AssessmentWeightingActionEnums.resetSuccess]: merge,
    [AssessmentWeightingActionEnums.resetError]: merge,
  },
  INITIAL_STATE
);
