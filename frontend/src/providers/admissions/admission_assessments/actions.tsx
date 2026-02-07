import { createAction } from "redux-actions";
import { IAdmissionAssessmentStateContext } from "./context";
import { IAdmissionAssessment, IPagedResult } from "../shared/interfaces";

export enum AdmissionAssessmentActionEnums {
  // Get
  getAssessmentPending = "GET_ADMISSION_ASSESSMENT_PENDING",
  getAssessmentSuccess = "GET_ADMISSION_ASSESSMENT_SUCCESS",
  getAssessmentError = "GET_ADMISSION_ASSESSMENT_ERROR",

  // GetByApplication
  getAssessmentByApplicationPending = "GET_ADMISSION_ASSESSMENT_BY_APPLICATION_PENDING",
  getAssessmentByApplicationSuccess = "GET_ADMISSION_ASSESSMENT_BY_APPLICATION_SUCCESS",
  getAssessmentByApplicationError = "GET_ADMISSION_ASSESSMENT_BY_APPLICATION_ERROR",

  // GetAll
  getAssessmentsPending = "GET_ADMISSION_ASSESSMENTS_PENDING",
  getAssessmentsSuccess = "GET_ADMISSION_ASSESSMENTS_SUCCESS",
  getAssessmentsError = "GET_ADMISSION_ASSESSMENTS_ERROR",

  // Schedule
  scheduleAssessmentPending = "SCHEDULE_ADMISSION_ASSESSMENT_PENDING",
  scheduleAssessmentSuccess = "SCHEDULE_ADMISSION_ASSESSMENT_SUCCESS",
  scheduleAssessmentError = "SCHEDULE_ADMISSION_ASSESSMENT_ERROR",

  // RecordResults
  recordResultsPending = "RECORD_ADMISSION_ASSESSMENT_RESULTS_PENDING",
  recordResultsSuccess = "RECORD_ADMISSION_ASSESSMENT_RESULTS_SUCCESS",
  recordResultsError = "RECORD_ADMISSION_ASSESSMENT_RESULTS_ERROR",

  // Cancel
  cancelAssessmentPending = "CANCEL_ADMISSION_ASSESSMENT_PENDING",
  cancelAssessmentSuccess = "CANCEL_ADMISSION_ASSESSMENT_SUCCESS",
  cancelAssessmentError = "CANCEL_ADMISSION_ASSESSMENT_ERROR",
}

// Get Actions
export const getAssessmentPending = createAction<IAdmissionAssessmentStateContext>(
  AdmissionAssessmentActionEnums.getAssessmentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAssessmentSuccess = createAction<IAdmissionAssessmentStateContext, IAdmissionAssessment>(
  AdmissionAssessmentActionEnums.getAssessmentSuccess,
  (assessment: IAdmissionAssessment) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    assessment,
  })
);

export const getAssessmentError = createAction<IAdmissionAssessmentStateContext>(
  AdmissionAssessmentActionEnums.getAssessmentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetByApplication Actions
export const getAssessmentByApplicationPending = createAction<IAdmissionAssessmentStateContext>(
  AdmissionAssessmentActionEnums.getAssessmentByApplicationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAssessmentByApplicationSuccess = createAction<IAdmissionAssessmentStateContext, IAdmissionAssessment>(
  AdmissionAssessmentActionEnums.getAssessmentByApplicationSuccess,
  (assessment: IAdmissionAssessment) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    assessment,
  })
);

export const getAssessmentByApplicationError = createAction<IAdmissionAssessmentStateContext>(
  AdmissionAssessmentActionEnums.getAssessmentByApplicationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetAll Actions
export const getAssessmentsPending = createAction<IAdmissionAssessmentStateContext>(
  AdmissionAssessmentActionEnums.getAssessmentsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAssessmentsSuccess = createAction<
  IAdmissionAssessmentStateContext,
  IPagedResult<IAdmissionAssessment>
>(
  AdmissionAssessmentActionEnums.getAssessmentsSuccess,
  (result: IPagedResult<IAdmissionAssessment>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    assessments: result.items,
    totalCount: result.totalCount,
  })
);

export const getAssessmentsError = createAction<IAdmissionAssessmentStateContext>(
  AdmissionAssessmentActionEnums.getAssessmentsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Schedule Actions
export const scheduleAssessmentPending = createAction<IAdmissionAssessmentStateContext>(
  AdmissionAssessmentActionEnums.scheduleAssessmentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const scheduleAssessmentSuccess = createAction<IAdmissionAssessmentStateContext, IAdmissionAssessment>(
  AdmissionAssessmentActionEnums.scheduleAssessmentSuccess,
  (assessment: IAdmissionAssessment) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    assessment,
  })
);

export const scheduleAssessmentError = createAction<IAdmissionAssessmentStateContext>(
  AdmissionAssessmentActionEnums.scheduleAssessmentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// RecordResults Actions
export const recordResultsPending = createAction<IAdmissionAssessmentStateContext>(
  AdmissionAssessmentActionEnums.recordResultsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const recordResultsSuccess = createAction<IAdmissionAssessmentStateContext, IAdmissionAssessment>(
  AdmissionAssessmentActionEnums.recordResultsSuccess,
  (assessment: IAdmissionAssessment) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    assessment,
  })
);

export const recordResultsError = createAction<IAdmissionAssessmentStateContext>(
  AdmissionAssessmentActionEnums.recordResultsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Cancel Actions
export const cancelAssessmentPending = createAction<IAdmissionAssessmentStateContext>(
  AdmissionAssessmentActionEnums.cancelAssessmentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const cancelAssessmentSuccess = createAction<IAdmissionAssessmentStateContext>(
  AdmissionAssessmentActionEnums.cancelAssessmentSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const cancelAssessmentError = createAction<IAdmissionAssessmentStateContext>(
  AdmissionAssessmentActionEnums.cancelAssessmentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
