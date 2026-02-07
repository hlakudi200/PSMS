import { createAction } from "redux-actions";
import { IClassSubjectStateContext } from "./context";
import { IClassSubject, IClassSubjectList, IPagedResult, IListResult } from "../shared/interfaces";

export enum ClassSubjectActionEnums {
  getClassSubjectsPending = "GET_CLASS_SUBJECTS_PENDING",
  getClassSubjectsSuccess = "GET_CLASS_SUBJECTS_SUCCESS",
  getClassSubjectsError = "GET_CLASS_SUBJECTS_ERROR",

  getClassSubjectPending = "GET_CLASS_SUBJECT_PENDING",
  getClassSubjectSuccess = "GET_CLASS_SUBJECT_SUCCESS",
  getClassSubjectError = "GET_CLASS_SUBJECT_ERROR",

  getByClassPending = "GET_CLASS_SUBJECTS_BY_CLASS_PENDING",
  getByClassSuccess = "GET_CLASS_SUBJECTS_BY_CLASS_SUCCESS",
  getByClassError = "GET_CLASS_SUBJECTS_BY_CLASS_ERROR",

  getByTeacherPending = "GET_CLASS_SUBJECTS_BY_TEACHER_PENDING",
  getByTeacherSuccess = "GET_CLASS_SUBJECTS_BY_TEACHER_SUCCESS",
  getByTeacherError = "GET_CLASS_SUBJECTS_BY_TEACHER_ERROR",

  createClassSubjectPending = "CREATE_CLASS_SUBJECT_PENDING",
  createClassSubjectSuccess = "CREATE_CLASS_SUBJECT_SUCCESS",
  createClassSubjectError = "CREATE_CLASS_SUBJECT_ERROR",

  updateClassSubjectPending = "UPDATE_CLASS_SUBJECT_PENDING",
  updateClassSubjectSuccess = "UPDATE_CLASS_SUBJECT_SUCCESS",
  updateClassSubjectError = "UPDATE_CLASS_SUBJECT_ERROR",

  deleteClassSubjectPending = "DELETE_CLASS_SUBJECT_PENDING",
  deleteClassSubjectSuccess = "DELETE_CLASS_SUBJECT_SUCCESS",
  deleteClassSubjectError = "DELETE_CLASS_SUBJECT_ERROR",

  assignTeacherPending = "ASSIGN_TEACHER_PENDING",
  assignTeacherSuccess = "ASSIGN_TEACHER_SUCCESS",
  assignTeacherError = "ASSIGN_TEACHER_ERROR",

  removeTeacherPending = "REMOVE_TEACHER_PENDING",
  removeTeacherSuccess = "REMOVE_TEACHER_SUCCESS",
  removeTeacherError = "REMOVE_TEACHER_ERROR",
}

// Get All ClassSubjects Actions
export const getClassSubjectsPending = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.getClassSubjectsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getClassSubjectsSuccess = createAction<
  IClassSubjectStateContext,
  IPagedResult<IClassSubjectList>
>(
  ClassSubjectActionEnums.getClassSubjectsSuccess,
  (result: IPagedResult<IClassSubjectList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    classSubjects: result.items,
    totalCount: result.totalCount,
  })
);

export const getClassSubjectsError = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.getClassSubjectsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Single ClassSubject Actions
export const getClassSubjectPending = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.getClassSubjectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getClassSubjectSuccess = createAction<IClassSubjectStateContext, IClassSubject>(
  ClassSubjectActionEnums.getClassSubjectSuccess,
  (classSubject: IClassSubject) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    classSubject,
  })
);

export const getClassSubjectError = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.getClassSubjectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Class Actions
export const getByClassPending = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.getByClassPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByClassSuccess = createAction<
  IClassSubjectStateContext,
  IListResult<IClassSubjectList>
>(
  ClassSubjectActionEnums.getByClassSuccess,
  (result: IListResult<IClassSubjectList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    classSubjects: result.items,
  })
);

export const getByClassError = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.getByClassError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Teacher Actions
export const getByTeacherPending = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.getByTeacherPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByTeacherSuccess = createAction<
  IClassSubjectStateContext,
  IListResult<IClassSubjectList>
>(
  ClassSubjectActionEnums.getByTeacherSuccess,
  (result: IListResult<IClassSubjectList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    classSubjects: result.items,
  })
);

export const getByTeacherError = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.getByTeacherError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create ClassSubject Actions
export const createClassSubjectPending = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.createClassSubjectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createClassSubjectSuccess = createAction<IClassSubjectStateContext, IClassSubject>(
  ClassSubjectActionEnums.createClassSubjectSuccess,
  (classSubject: IClassSubject) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    classSubject,
  })
);

export const createClassSubjectError = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.createClassSubjectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update ClassSubject Actions
export const updateClassSubjectPending = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.updateClassSubjectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateClassSubjectSuccess = createAction<IClassSubjectStateContext, IClassSubject>(
  ClassSubjectActionEnums.updateClassSubjectSuccess,
  (classSubject: IClassSubject) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    classSubject,
  })
);

export const updateClassSubjectError = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.updateClassSubjectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete ClassSubject Actions
export const deleteClassSubjectPending = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.deleteClassSubjectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteClassSubjectSuccess = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.deleteClassSubjectSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteClassSubjectError = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.deleteClassSubjectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Assign Teacher Actions
export const assignTeacherPending = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.assignTeacherPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const assignTeacherSuccess = createAction<IClassSubjectStateContext, IClassSubject>(
  ClassSubjectActionEnums.assignTeacherSuccess,
  (classSubject: IClassSubject) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    classSubject,
  })
);

export const assignTeacherError = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.assignTeacherError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Remove Teacher Actions
export const removeTeacherPending = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.removeTeacherPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const removeTeacherSuccess = createAction<IClassSubjectStateContext, IClassSubject>(
  ClassSubjectActionEnums.removeTeacherSuccess,
  (classSubject: IClassSubject) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    classSubject,
  })
);

export const removeTeacherError = createAction<IClassSubjectStateContext>(
  ClassSubjectActionEnums.removeTeacherError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
