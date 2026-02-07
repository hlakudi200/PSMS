import { createAction } from "redux-actions";
import { IApplicantParentStateContext } from "./context";
import { IApplicantParent, IListResult } from "../shared/interfaces";

export enum ApplicantParentActionEnums {
  // Get
  getApplicantParentPending = "GET_APPLICANT_PARENT_PENDING",
  getApplicantParentSuccess = "GET_APPLICANT_PARENT_SUCCESS",
  getApplicantParentError = "GET_APPLICANT_PARENT_ERROR",

  // GetAllByApplication
  getAllByApplicationPending = "GET_ALL_BY_APPLICATION_PENDING",
  getAllByApplicationSuccess = "GET_ALL_BY_APPLICATION_SUCCESS",
  getAllByApplicationError = "GET_ALL_BY_APPLICATION_ERROR",

  // Create
  createApplicantParentPending = "CREATE_APPLICANT_PARENT_PENDING",
  createApplicantParentSuccess = "CREATE_APPLICANT_PARENT_SUCCESS",
  createApplicantParentError = "CREATE_APPLICANT_PARENT_ERROR",

  // Update
  updateApplicantParentPending = "UPDATE_APPLICANT_PARENT_PENDING",
  updateApplicantParentSuccess = "UPDATE_APPLICANT_PARENT_SUCCESS",
  updateApplicantParentError = "UPDATE_APPLICANT_PARENT_ERROR",

  // Delete
  deleteApplicantParentPending = "DELETE_APPLICANT_PARENT_PENDING",
  deleteApplicantParentSuccess = "DELETE_APPLICANT_PARENT_SUCCESS",
  deleteApplicantParentError = "DELETE_APPLICANT_PARENT_ERROR",

  // SetAsPrimaryContact
  setAsPrimaryContactPending = "SET_AS_PRIMARY_CONTACT_PENDING",
  setAsPrimaryContactSuccess = "SET_AS_PRIMARY_CONTACT_SUCCESS",
  setAsPrimaryContactError = "SET_AS_PRIMARY_CONTACT_ERROR",

  // SetAsFinanciallyResponsible
  setAsFinanciallyResponsiblePending = "SET_AS_FINANCIALLY_RESPONSIBLE_PENDING",
  setAsFinanciallyResponsibleSuccess = "SET_AS_FINANCIALLY_RESPONSIBLE_SUCCESS",
  setAsFinanciallyResponsibleError = "SET_AS_FINANCIALLY_RESPONSIBLE_ERROR",
}

// Get Actions
export const getApplicantParentPending = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.getApplicantParentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getApplicantParentSuccess = createAction<IApplicantParentStateContext, IApplicantParent>(
  ApplicantParentActionEnums.getApplicantParentSuccess,
  (applicantParent: IApplicantParent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    applicantParent,
  })
);

export const getApplicantParentError = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.getApplicantParentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetAllByApplication Actions
export const getAllByApplicationPending = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.getAllByApplicationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllByApplicationSuccess = createAction<
  IApplicantParentStateContext,
  IListResult<IApplicantParent>
>(
  ApplicantParentActionEnums.getAllByApplicationSuccess,
  (result: IListResult<IApplicantParent>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    applicantParents: result.items,
  })
);

export const getAllByApplicationError = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.getAllByApplicationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Actions
export const createApplicantParentPending = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.createApplicantParentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createApplicantParentSuccess = createAction<IApplicantParentStateContext, IApplicantParent>(
  ApplicantParentActionEnums.createApplicantParentSuccess,
  (applicantParent: IApplicantParent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    applicantParent,
  })
);

export const createApplicantParentError = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.createApplicantParentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Actions
export const updateApplicantParentPending = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.updateApplicantParentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateApplicantParentSuccess = createAction<IApplicantParentStateContext, IApplicantParent>(
  ApplicantParentActionEnums.updateApplicantParentSuccess,
  (applicantParent: IApplicantParent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    applicantParent,
  })
);

export const updateApplicantParentError = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.updateApplicantParentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Actions
export const deleteApplicantParentPending = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.deleteApplicantParentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteApplicantParentSuccess = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.deleteApplicantParentSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteApplicantParentError = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.deleteApplicantParentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// SetAsPrimaryContact Actions
export const setAsPrimaryContactPending = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.setAsPrimaryContactPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const setAsPrimaryContactSuccess = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.setAsPrimaryContactSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const setAsPrimaryContactError = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.setAsPrimaryContactError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// SetAsFinanciallyResponsible Actions
export const setAsFinanciallyResponsiblePending = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.setAsFinanciallyResponsiblePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const setAsFinanciallyResponsibleSuccess = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.setAsFinanciallyResponsibleSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const setAsFinanciallyResponsibleError = createAction<IApplicantParentStateContext>(
  ApplicantParentActionEnums.setAsFinanciallyResponsibleError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
