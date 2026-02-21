import { createAction } from "redux-actions";
import { ISubjectStateContext } from "./context";
import { ISubject, IPagedResult } from "../shared/interfaces";

export enum SubjectActionEnums {
  getSubjectsPending = "GET_SUBJECTS_PENDING",
  getSubjectsSuccess = "GET_SUBJECTS_SUCCESS",
  getSubjectsError = "GET_SUBJECTS_ERROR",
  getSubjectPending = "GET_SUBJECT_PENDING",
  getSubjectSuccess = "GET_SUBJECT_SUCCESS",
  getSubjectError = "GET_SUBJECT_ERROR",
  createSubjectPending = "CREATE_SUBJECT_PENDING",
  createSubjectSuccess = "CREATE_SUBJECT_SUCCESS",
  createSubjectError = "CREATE_SUBJECT_ERROR",
  updateSubjectPending = "UPDATE_SUBJECT_PENDING",
  updateSubjectSuccess = "UPDATE_SUBJECT_SUCCESS",
  updateSubjectError = "UPDATE_SUBJECT_ERROR",
  deleteSubjectPending = "DELETE_SUBJECT_PENDING",
  deleteSubjectSuccess = "DELETE_SUBJECT_SUCCESS",
  deleteSubjectError = "DELETE_SUBJECT_ERROR",
  activateSubjectPending = "ACTIVATE_SUBJECT_PENDING",
  activateSubjectSuccess = "ACTIVATE_SUBJECT_SUCCESS",
  activateSubjectError = "ACTIVATE_SUBJECT_ERROR",
  deactivateSubjectPending = "DEACTIVATE_SUBJECT_PENDING",
  deactivateSubjectSuccess = "DEACTIVATE_SUBJECT_SUCCESS",
  deactivateSubjectError = "DEACTIVATE_SUBJECT_ERROR",
}

export const getSubjectsPending = createAction<ISubjectStateContext>(
  SubjectActionEnums.getSubjectsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getSubjectsSuccess = createAction<
  ISubjectStateContext,
  IPagedResult<ISubject>
>(
  SubjectActionEnums.getSubjectsSuccess,
  (result: IPagedResult<ISubject>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    subjects: result.items,
    totalCount: result.totalCount,
  })
);

export const getSubjectsError = createAction<ISubjectStateContext>(
  SubjectActionEnums.getSubjectsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const getSubjectPending = createAction<ISubjectStateContext>(
  SubjectActionEnums.getSubjectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getSubjectSuccess = createAction<ISubjectStateContext, ISubject>(
  SubjectActionEnums.getSubjectSuccess,
  (subject: ISubject) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    subject,
  })
);

export const getSubjectError = createAction<ISubjectStateContext>(
  SubjectActionEnums.getSubjectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const createSubjectPending = createAction<ISubjectStateContext>(
  SubjectActionEnums.createSubjectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createSubjectSuccess = createAction<ISubjectStateContext, ISubject>(
  SubjectActionEnums.createSubjectSuccess,
  (subject: ISubject) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    subject,
  })
);

export const createSubjectError = createAction<ISubjectStateContext>(
  SubjectActionEnums.createSubjectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const updateSubjectPending = createAction<ISubjectStateContext>(
  SubjectActionEnums.updateSubjectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateSubjectSuccess = createAction<ISubjectStateContext, ISubject>(
  SubjectActionEnums.updateSubjectSuccess,
  (subject: ISubject) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    subject,
  })
);

export const updateSubjectError = createAction<ISubjectStateContext>(
  SubjectActionEnums.updateSubjectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const deleteSubjectPending = createAction<ISubjectStateContext>(
  SubjectActionEnums.deleteSubjectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteSubjectSuccess = createAction<ISubjectStateContext>(
  SubjectActionEnums.deleteSubjectSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteSubjectError = createAction<ISubjectStateContext>(
  SubjectActionEnums.deleteSubjectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const activateSubjectPending = createAction<ISubjectStateContext>(
  SubjectActionEnums.activateSubjectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const activateSubjectSuccess = createAction<ISubjectStateContext>(
  SubjectActionEnums.activateSubjectSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);

export const activateSubjectError = createAction<ISubjectStateContext>(
  SubjectActionEnums.activateSubjectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const deactivateSubjectPending = createAction<ISubjectStateContext>(
  SubjectActionEnums.deactivateSubjectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deactivateSubjectSuccess = createAction<ISubjectStateContext>(
  SubjectActionEnums.deactivateSubjectSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);

export const deactivateSubjectError = createAction<ISubjectStateContext>(
  SubjectActionEnums.deactivateSubjectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
