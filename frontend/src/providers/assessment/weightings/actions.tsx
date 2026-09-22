import { createAction } from "redux-actions";
import { IAssessmentWeighting, IAssessmentWeightingStateContext } from "./context";

export enum AssessmentWeightingActionEnums {
  getAllPending = "GET_ALL_ASSESSMENT_WEIGHTINGS_PENDING",
  getAllSuccess = "GET_ALL_ASSESSMENT_WEIGHTINGS_SUCCESS",
  getAllError = "GET_ALL_ASSESSMENT_WEIGHTINGS_ERROR",

  updatePending = "UPDATE_ASSESSMENT_WEIGHTINGS_PENDING",
  updateSuccess = "UPDATE_ASSESSMENT_WEIGHTINGS_SUCCESS",
  updateError = "UPDATE_ASSESSMENT_WEIGHTINGS_ERROR",

  resetPending = "RESET_ASSESSMENT_WEIGHTINGS_PENDING",
  resetSuccess = "RESET_ASSESSMENT_WEIGHTINGS_SUCCESS",
  resetError = "RESET_ASSESSMENT_WEIGHTINGS_ERROR",
}

const pendingAction = (type: AssessmentWeightingActionEnums) =>
  createAction<IAssessmentWeightingStateContext>(type, () => ({
    isPending: true,
    isSuccess: false,
    isError: false,
  }));

const successAction = (type: AssessmentWeightingActionEnums) =>
  createAction<IAssessmentWeightingStateContext, IAssessmentWeighting[]>(
    type,
    (weightings) => ({ isPending: false, isSuccess: true, isError: false, weightings })
  );

const errorAction = (type: AssessmentWeightingActionEnums) =>
  createAction<IAssessmentWeightingStateContext>(type, () => ({
    isPending: false,
    isSuccess: false,
    isError: true,
  }));

export const getAllPending = pendingAction(AssessmentWeightingActionEnums.getAllPending);
export const getAllSuccess = successAction(AssessmentWeightingActionEnums.getAllSuccess);
export const getAllError = errorAction(AssessmentWeightingActionEnums.getAllError);

export const updatePending = pendingAction(AssessmentWeightingActionEnums.updatePending);
export const updateSuccess = successAction(AssessmentWeightingActionEnums.updateSuccess);
export const updateError = errorAction(AssessmentWeightingActionEnums.updateError);

export const resetPending = pendingAction(AssessmentWeightingActionEnums.resetPending);
export const resetSuccess = successAction(AssessmentWeightingActionEnums.resetSuccess);
export const resetError = errorAction(AssessmentWeightingActionEnums.resetError);
