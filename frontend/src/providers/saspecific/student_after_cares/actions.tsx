import { createAction } from "redux-actions";
import { IStudentAfterCareStateContext } from "./context";
import { IStudentAfterCare, IStudentAfterCareList, IPagedResult, IListResult } from "../shared/interfaces";

export enum StudentAfterCareActionEnums {
  getStudentAfterCarePending = "GET_STUDENT_AFTER_CARE_PENDING",
  getStudentAfterCareSuccess = "GET_STUDENT_AFTER_CARE_SUCCESS",
  getStudentAfterCareError = "GET_STUDENT_AFTER_CARE_ERROR",

  getAllStudentAfterCaresPending = "GET_ALL_STUDENT_AFTER_CARES_PENDING",
  getAllStudentAfterCaresSuccess = "GET_ALL_STUDENT_AFTER_CARES_SUCCESS",
  getAllStudentAfterCaresError = "GET_ALL_STUDENT_AFTER_CARES_ERROR",

  getByAfterCarePending = "GET_STUDENT_AFTER_CARES_BY_AFTER_CARE_PENDING",
  getByAfterCareSuccess = "GET_STUDENT_AFTER_CARES_BY_AFTER_CARE_SUCCESS",
  getByAfterCareError = "GET_STUDENT_AFTER_CARES_BY_AFTER_CARE_ERROR",

  getByStudentPending = "GET_STUDENT_AFTER_CARES_BY_STUDENT_PENDING",
  getByStudentSuccess = "GET_STUDENT_AFTER_CARES_BY_STUDENT_SUCCESS",
  getByStudentError = "GET_STUDENT_AFTER_CARES_BY_STUDENT_ERROR",

  createStudentAfterCarePending = "CREATE_STUDENT_AFTER_CARE_PENDING",
  createStudentAfterCareSuccess = "CREATE_STUDENT_AFTER_CARE_SUCCESS",
  createStudentAfterCareError = "CREATE_STUDENT_AFTER_CARE_ERROR",

  updateStudentAfterCarePending = "UPDATE_STUDENT_AFTER_CARE_PENDING",
  updateStudentAfterCareSuccess = "UPDATE_STUDENT_AFTER_CARE_SUCCESS",
  updateStudentAfterCareError = "UPDATE_STUDENT_AFTER_CARE_ERROR",

  deleteStudentAfterCarePending = "DELETE_STUDENT_AFTER_CARE_PENDING",
  deleteStudentAfterCareSuccess = "DELETE_STUDENT_AFTER_CARE_SUCCESS",
  deleteStudentAfterCareError = "DELETE_STUDENT_AFTER_CARE_ERROR",

  suspendStudentAfterCarePending = "SUSPEND_STUDENT_AFTER_CARE_PENDING",
  suspendStudentAfterCareSuccess = "SUSPEND_STUDENT_AFTER_CARE_SUCCESS",
  suspendStudentAfterCareError = "SUSPEND_STUDENT_AFTER_CARE_ERROR",

  reactivateStudentAfterCarePending = "REACTIVATE_STUDENT_AFTER_CARE_PENDING",
  reactivateStudentAfterCareSuccess = "REACTIVATE_STUDENT_AFTER_CARE_SUCCESS",
  reactivateStudentAfterCareError = "REACTIVATE_STUDENT_AFTER_CARE_ERROR",

  terminateStudentAfterCarePending = "TERMINATE_STUDENT_AFTER_CARE_PENDING",
  terminateStudentAfterCareSuccess = "TERMINATE_STUDENT_AFTER_CARE_SUCCESS",
  terminateStudentAfterCareError = "TERMINATE_STUDENT_AFTER_CARE_ERROR",
}

// Get Single StudentAfterCare Actions
export const getStudentAfterCarePending = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.getStudentAfterCarePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getStudentAfterCareSuccess = createAction<IStudentAfterCareStateContext, IStudentAfterCare>(
  StudentAfterCareActionEnums.getStudentAfterCareSuccess,
  (studentAfterCare: IStudentAfterCare) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentAfterCare,
  })
);

export const getStudentAfterCareError = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.getStudentAfterCareError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All StudentAfterCares Actions
export const getAllStudentAfterCaresPending = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.getAllStudentAfterCaresPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllStudentAfterCaresSuccess = createAction<
  IStudentAfterCareStateContext,
  IPagedResult<IStudentAfterCareList>
>(
  StudentAfterCareActionEnums.getAllStudentAfterCaresSuccess,
  (result: IPagedResult<IStudentAfterCareList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentAfterCares: result.items,
    totalCount: result.totalCount,
  })
);

export const getAllStudentAfterCaresError = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.getAllStudentAfterCaresError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By AfterCare Actions
export const getByAfterCarePending = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.getByAfterCarePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByAfterCareSuccess = createAction<
  IStudentAfterCareStateContext,
  IListResult<IStudentAfterCareList>
>(
  StudentAfterCareActionEnums.getByAfterCareSuccess,
  (result: IListResult<IStudentAfterCareList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentAfterCares: result.items,
  })
);

export const getByAfterCareError = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.getByAfterCareError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Student Actions
export const getByStudentPending = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.getByStudentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByStudentSuccess = createAction<
  IStudentAfterCareStateContext,
  IListResult<IStudentAfterCareList>
>(
  StudentAfterCareActionEnums.getByStudentSuccess,
  (result: IListResult<IStudentAfterCareList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentAfterCares: result.items,
  })
);

export const getByStudentError = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.getByStudentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create StudentAfterCare Actions
export const createStudentAfterCarePending = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.createStudentAfterCarePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createStudentAfterCareSuccess = createAction<IStudentAfterCareStateContext, IStudentAfterCare>(
  StudentAfterCareActionEnums.createStudentAfterCareSuccess,
  (studentAfterCare: IStudentAfterCare) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentAfterCare,
  })
);

export const createStudentAfterCareError = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.createStudentAfterCareError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update StudentAfterCare Actions
export const updateStudentAfterCarePending = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.updateStudentAfterCarePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateStudentAfterCareSuccess = createAction<IStudentAfterCareStateContext, IStudentAfterCare>(
  StudentAfterCareActionEnums.updateStudentAfterCareSuccess,
  (studentAfterCare: IStudentAfterCare) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentAfterCare,
  })
);

export const updateStudentAfterCareError = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.updateStudentAfterCareError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete StudentAfterCare Actions
export const deleteStudentAfterCarePending = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.deleteStudentAfterCarePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteStudentAfterCareSuccess = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.deleteStudentAfterCareSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteStudentAfterCareError = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.deleteStudentAfterCareError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Suspend StudentAfterCare Actions
export const suspendStudentAfterCarePending = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.suspendStudentAfterCarePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const suspendStudentAfterCareSuccess = createAction<IStudentAfterCareStateContext, IStudentAfterCare>(
  StudentAfterCareActionEnums.suspendStudentAfterCareSuccess,
  (studentAfterCare: IStudentAfterCare) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentAfterCare,
  })
);

export const suspendStudentAfterCareError = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.suspendStudentAfterCareError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Reactivate StudentAfterCare Actions
export const reactivateStudentAfterCarePending = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.reactivateStudentAfterCarePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const reactivateStudentAfterCareSuccess = createAction<IStudentAfterCareStateContext, IStudentAfterCare>(
  StudentAfterCareActionEnums.reactivateStudentAfterCareSuccess,
  (studentAfterCare: IStudentAfterCare) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentAfterCare,
  })
);

export const reactivateStudentAfterCareError = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.reactivateStudentAfterCareError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Terminate StudentAfterCare Actions
export const terminateStudentAfterCarePending = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.terminateStudentAfterCarePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const terminateStudentAfterCareSuccess = createAction<IStudentAfterCareStateContext, IStudentAfterCare>(
  StudentAfterCareActionEnums.terminateStudentAfterCareSuccess,
  (studentAfterCare: IStudentAfterCare) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentAfterCare,
  })
);

export const terminateStudentAfterCareError = createAction<IStudentAfterCareStateContext>(
  StudentAfterCareActionEnums.terminateStudentAfterCareError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
