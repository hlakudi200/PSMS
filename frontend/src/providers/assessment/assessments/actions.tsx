import { createAction } from "redux-actions";
import { IAssessmentStateContext } from "./context";
import { IAssessment, IAssessmentList, IPagedResult } from "../shared/interfaces";

export enum AssessmentActionEnums {
  getAssessmentPending = "GET_ASSESSMENT_PENDING",
  getAssessmentSuccess = "GET_ASSESSMENT_SUCCESS",
  getAssessmentError = "GET_ASSESSMENT_ERROR",

  getAllAssessmentsPending = "GET_ALL_ASSESSMENTS_PENDING",
  getAllAssessmentsSuccess = "GET_ALL_ASSESSMENTS_SUCCESS",
  getAllAssessmentsError = "GET_ALL_ASSESSMENTS_ERROR",

  createAssessmentPending = "CREATE_ASSESSMENT_PENDING",
  createAssessmentSuccess = "CREATE_ASSESSMENT_SUCCESS",
  createAssessmentError = "CREATE_ASSESSMENT_ERROR",

  updateAssessmentPending = "UPDATE_ASSESSMENT_PENDING",
  updateAssessmentSuccess = "UPDATE_ASSESSMENT_SUCCESS",
  updateAssessmentError = "UPDATE_ASSESSMENT_ERROR",

  deleteAssessmentPending = "DELETE_ASSESSMENT_PENDING",
  deleteAssessmentSuccess = "DELETE_ASSESSMENT_SUCCESS",
  deleteAssessmentError = "DELETE_ASSESSMENT_ERROR",

  publishAssessmentPending = "PUBLISH_ASSESSMENT_PENDING",
  publishAssessmentSuccess = "PUBLISH_ASSESSMENT_SUCCESS",
  publishAssessmentError = "PUBLISH_ASSESSMENT_ERROR",

  unpublishAssessmentPending = "UNPUBLISH_ASSESSMENT_PENDING",
  unpublishAssessmentSuccess = "UNPUBLISH_ASSESSMENT_SUCCESS",
  unpublishAssessmentError = "UNPUBLISH_ASSESSMENT_ERROR",

  releaseMarksPending = "RELEASE_MARKS_PENDING",
  releaseMarksSuccess = "RELEASE_MARKS_SUCCESS",
  releaseMarksError = "RELEASE_MARKS_ERROR",
}

// Get Single Assessment Actions
export const getAssessmentPending = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.getAssessmentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAssessmentSuccess = createAction<IAssessmentStateContext, IAssessment>(
  AssessmentActionEnums.getAssessmentSuccess,
  (assessment: IAssessment) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    assessment,
  })
);

export const getAssessmentError = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.getAssessmentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All Assessments Actions
export const getAllAssessmentsPending = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.getAllAssessmentsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllAssessmentsSuccess = createAction<
  IAssessmentStateContext,
  IPagedResult<IAssessmentList>
>(
  AssessmentActionEnums.getAllAssessmentsSuccess,
  (result: IPagedResult<IAssessmentList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    assessments: result.items,
    totalCount: result.totalCount,
  })
);

export const getAllAssessmentsError = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.getAllAssessmentsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Assessment Actions
export const createAssessmentPending = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.createAssessmentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createAssessmentSuccess = createAction<IAssessmentStateContext, IAssessment>(
  AssessmentActionEnums.createAssessmentSuccess,
  (assessment: IAssessment) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    assessment,
  })
);

export const createAssessmentError = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.createAssessmentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Assessment Actions
export const updateAssessmentPending = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.updateAssessmentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateAssessmentSuccess = createAction<IAssessmentStateContext, IAssessment>(
  AssessmentActionEnums.updateAssessmentSuccess,
  (assessment: IAssessment) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    assessment,
  })
);

export const updateAssessmentError = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.updateAssessmentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Assessment Actions
export const deleteAssessmentPending = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.deleteAssessmentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteAssessmentSuccess = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.deleteAssessmentSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteAssessmentError = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.deleteAssessmentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Publish Assessment Actions
export const publishAssessmentPending = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.publishAssessmentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const publishAssessmentSuccess = createAction<IAssessmentStateContext, IAssessment>(
  AssessmentActionEnums.publishAssessmentSuccess,
  (assessment: IAssessment) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    assessment,
  })
);

export const publishAssessmentError = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.publishAssessmentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Unpublish Assessment Actions
export const unpublishAssessmentPending = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.unpublishAssessmentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const unpublishAssessmentSuccess = createAction<IAssessmentStateContext, IAssessment>(
  AssessmentActionEnums.unpublishAssessmentSuccess,
  (assessment: IAssessment) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    assessment,
  })
);

export const unpublishAssessmentError = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.unpublishAssessmentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Release Marks Actions
export const releaseMarksPending = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.releaseMarksPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const releaseMarksSuccess = createAction<IAssessmentStateContext, IAssessment>(
  AssessmentActionEnums.releaseMarksSuccess,
  (assessment: IAssessment) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    assessment,
  })
);

export const releaseMarksError = createAction<IAssessmentStateContext>(
  AssessmentActionEnums.releaseMarksError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
