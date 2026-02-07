import { createAction } from "redux-actions";
import { IStudentExtramuralStateContext } from "./context";
import { IStudentExtramural, IStudentExtramuralList, IPagedResult, IListResult } from "../shared/interfaces";

export enum StudentExtramuralActionEnums {
  getStudentExtramuralPending = "GET_STUDENT_EXTRAMURAL_PENDING",
  getStudentExtramuralSuccess = "GET_STUDENT_EXTRAMURAL_SUCCESS",
  getStudentExtramuralError = "GET_STUDENT_EXTRAMURAL_ERROR",

  getStudentExtramuralsPending = "GET_STUDENT_EXTRAMURALS_PENDING",
  getStudentExtramuralsSuccess = "GET_STUDENT_EXTRAMURALS_SUCCESS",
  getStudentExtramuralsError = "GET_STUDENT_EXTRAMURALS_ERROR",

  getByActivityPending = "GET_STUDENT_EXTRAMURALS_BY_ACTIVITY_PENDING",
  getByActivitySuccess = "GET_STUDENT_EXTRAMURALS_BY_ACTIVITY_SUCCESS",
  getByActivityError = "GET_STUDENT_EXTRAMURALS_BY_ACTIVITY_ERROR",

  getByStudentPending = "GET_STUDENT_EXTRAMURALS_BY_STUDENT_PENDING",
  getByStudentSuccess = "GET_STUDENT_EXTRAMURALS_BY_STUDENT_SUCCESS",
  getByStudentError = "GET_STUDENT_EXTRAMURALS_BY_STUDENT_ERROR",

  createStudentExtramuralPending = "CREATE_STUDENT_EXTRAMURAL_PENDING",
  createStudentExtramuralSuccess = "CREATE_STUDENT_EXTRAMURAL_SUCCESS",
  createStudentExtramuralError = "CREATE_STUDENT_EXTRAMURAL_ERROR",

  updateStudentExtramuralPending = "UPDATE_STUDENT_EXTRAMURAL_PENDING",
  updateStudentExtramuralSuccess = "UPDATE_STUDENT_EXTRAMURAL_SUCCESS",
  updateStudentExtramuralError = "UPDATE_STUDENT_EXTRAMURAL_ERROR",

  deleteStudentExtramuralPending = "DELETE_STUDENT_EXTRAMURAL_PENDING",
  deleteStudentExtramuralSuccess = "DELETE_STUDENT_EXTRAMURAL_SUCCESS",
  deleteStudentExtramuralError = "DELETE_STUDENT_EXTRAMURAL_ERROR",

  suspendPending = "SUSPEND_STUDENT_EXTRAMURAL_PENDING",
  suspendSuccess = "SUSPEND_STUDENT_EXTRAMURAL_SUCCESS",
  suspendError = "SUSPEND_STUDENT_EXTRAMURAL_ERROR",

  reactivatePending = "REACTIVATE_STUDENT_EXTRAMURAL_PENDING",
  reactivateSuccess = "REACTIVATE_STUDENT_EXTRAMURAL_SUCCESS",
  reactivateError = "REACTIVATE_STUDENT_EXTRAMURAL_ERROR",

  terminatePending = "TERMINATE_STUDENT_EXTRAMURAL_PENDING",
  terminateSuccess = "TERMINATE_STUDENT_EXTRAMURAL_SUCCESS",
  terminateError = "TERMINATE_STUDENT_EXTRAMURAL_ERROR",

  signConsentFormPending = "SIGN_CONSENT_FORM_STUDENT_EXTRAMURAL_PENDING",
  signConsentFormSuccess = "SIGN_CONSENT_FORM_STUDENT_EXTRAMURAL_SUCCESS",
  signConsentFormError = "SIGN_CONSENT_FORM_STUDENT_EXTRAMURAL_ERROR",
}

// Get Single StudentExtramural
export const getStudentExtramuralPending = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.getStudentExtramuralPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getStudentExtramuralSuccess = createAction<IStudentExtramuralStateContext, IStudentExtramural>(
  StudentExtramuralActionEnums.getStudentExtramuralSuccess,
  (studentExtramural: IStudentExtramural) => ({
    isPending: false, isSuccess: true, isError: false, studentExtramural,
  })
);
export const getStudentExtramuralError = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.getStudentExtramuralError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All StudentExtramurals (paged)
export const getStudentExtramuralsPending = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.getStudentExtramuralsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getStudentExtramuralsSuccess = createAction<IStudentExtramuralStateContext, IPagedResult<IStudentExtramuralList>>(
  StudentExtramuralActionEnums.getStudentExtramuralsSuccess,
  (result: IPagedResult<IStudentExtramuralList>) => ({
    isPending: false, isSuccess: true, isError: false,
    studentExtramurals: result.items, totalCount: result.totalCount,
  })
);
export const getStudentExtramuralsError = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.getStudentExtramuralsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Activity
export const getByActivityPending = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.getByActivityPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getByActivitySuccess = createAction<IStudentExtramuralStateContext, IListResult<IStudentExtramuralList>>(
  StudentExtramuralActionEnums.getByActivitySuccess,
  (result: IListResult<IStudentExtramuralList>) => ({
    isPending: false, isSuccess: true, isError: false,
    studentExtramurals: result.items,
  })
);
export const getByActivityError = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.getByActivityError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Student
export const getByStudentPending = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.getByStudentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getByStudentSuccess = createAction<IStudentExtramuralStateContext, IListResult<IStudentExtramuralList>>(
  StudentExtramuralActionEnums.getByStudentSuccess,
  (result: IListResult<IStudentExtramuralList>) => ({
    isPending: false, isSuccess: true, isError: false,
    studentExtramurals: result.items,
  })
);
export const getByStudentError = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.getByStudentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create StudentExtramural
export const createStudentExtramuralPending = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.createStudentExtramuralPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const createStudentExtramuralSuccess = createAction<IStudentExtramuralStateContext, IStudentExtramural>(
  StudentExtramuralActionEnums.createStudentExtramuralSuccess,
  (studentExtramural: IStudentExtramural) => ({
    isPending: false, isSuccess: true, isError: false, studentExtramural,
  })
);
export const createStudentExtramuralError = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.createStudentExtramuralError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update StudentExtramural
export const updateStudentExtramuralPending = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.updateStudentExtramuralPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const updateStudentExtramuralSuccess = createAction<IStudentExtramuralStateContext, IStudentExtramural>(
  StudentExtramuralActionEnums.updateStudentExtramuralSuccess,
  (studentExtramural: IStudentExtramural) => ({
    isPending: false, isSuccess: true, isError: false, studentExtramural,
  })
);
export const updateStudentExtramuralError = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.updateStudentExtramuralError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete StudentExtramural
export const deleteStudentExtramuralPending = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.deleteStudentExtramuralPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const deleteStudentExtramuralSuccess = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.deleteStudentExtramuralSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const deleteStudentExtramuralError = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.deleteStudentExtramuralError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Suspend StudentExtramural
export const suspendPending = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.suspendPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const suspendSuccess = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.suspendSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const suspendError = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.suspendError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Reactivate StudentExtramural
export const reactivatePending = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.reactivatePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const reactivateSuccess = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.reactivateSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const reactivateError = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.reactivateError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Terminate StudentExtramural
export const terminatePending = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.terminatePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const terminateSuccess = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.terminateSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const terminateError = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.terminateError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Sign Consent Form
export const signConsentFormPending = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.signConsentFormPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const signConsentFormSuccess = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.signConsentFormSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const signConsentFormError = createAction<IStudentExtramuralStateContext>(
  StudentExtramuralActionEnums.signConsentFormError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
