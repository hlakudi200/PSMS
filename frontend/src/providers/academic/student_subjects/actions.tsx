import { createAction } from "redux-actions";
import { IStudentSubjectStateContext } from "./context";
import { IStudentSubject, IListResult } from "../shared/interfaces";

export enum StudentSubjectActionEnums {
  getByStudentPending = "GET_STUDENT_SUBJECTS_BY_STUDENT_PENDING",
  getByStudentSuccess = "GET_STUDENT_SUBJECTS_BY_STUDENT_SUCCESS",
  getByStudentError = "GET_STUDENT_SUBJECTS_BY_STUDENT_ERROR",

  getBySubjectPending = "GET_STUDENT_SUBJECTS_BY_SUBJECT_PENDING",
  getBySubjectSuccess = "GET_STUDENT_SUBJECTS_BY_SUBJECT_SUCCESS",
  getBySubjectError = "GET_STUDENT_SUBJECTS_BY_SUBJECT_ERROR",

  getByStudentAndYearPending = "GET_STUDENT_SUBJECTS_BY_STUDENT_AND_YEAR_PENDING",
  getByStudentAndYearSuccess = "GET_STUDENT_SUBJECTS_BY_STUDENT_AND_YEAR_SUCCESS",
  getByStudentAndYearError = "GET_STUDENT_SUBJECTS_BY_STUDENT_AND_YEAR_ERROR",

  enrollPending = "ENROLL_STUDENT_SUBJECT_PENDING",
  enrollSuccess = "ENROLL_STUDENT_SUBJECT_SUCCESS",
  enrollError = "ENROLL_STUDENT_SUBJECT_ERROR",

  unenrollPending = "UNENROLL_STUDENT_SUBJECT_PENDING",
  unenrollSuccess = "UNENROLL_STUDENT_SUBJECT_SUCCESS",
  unenrollError = "UNENROLL_STUDENT_SUBJECT_ERROR",
}

// Get By Student Actions
export const getByStudentPending = createAction<IStudentSubjectStateContext>(
  StudentSubjectActionEnums.getByStudentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByStudentSuccess = createAction<
  IStudentSubjectStateContext,
  IListResult<IStudentSubject>
>(
  StudentSubjectActionEnums.getByStudentSuccess,
  (result: IListResult<IStudentSubject>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentSubjects: result.items,
  })
);

export const getByStudentError = createAction<IStudentSubjectStateContext>(
  StudentSubjectActionEnums.getByStudentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Subject Actions
export const getBySubjectPending = createAction<IStudentSubjectStateContext>(
  StudentSubjectActionEnums.getBySubjectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getBySubjectSuccess = createAction<
  IStudentSubjectStateContext,
  IListResult<IStudentSubject>
>(
  StudentSubjectActionEnums.getBySubjectSuccess,
  (result: IListResult<IStudentSubject>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentSubjects: result.items,
  })
);

export const getBySubjectError = createAction<IStudentSubjectStateContext>(
  StudentSubjectActionEnums.getBySubjectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Student And Year Actions
export const getByStudentAndYearPending = createAction<IStudentSubjectStateContext>(
  StudentSubjectActionEnums.getByStudentAndYearPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByStudentAndYearSuccess = createAction<
  IStudentSubjectStateContext,
  IListResult<IStudentSubject>
>(
  StudentSubjectActionEnums.getByStudentAndYearSuccess,
  (result: IListResult<IStudentSubject>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentSubjects: result.items,
  })
);

export const getByStudentAndYearError = createAction<IStudentSubjectStateContext>(
  StudentSubjectActionEnums.getByStudentAndYearError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Enroll Actions
export const enrollPending = createAction<IStudentSubjectStateContext>(
  StudentSubjectActionEnums.enrollPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const enrollSuccess = createAction<IStudentSubjectStateContext, IStudentSubject>(
  StudentSubjectActionEnums.enrollSuccess,
  (studentSubject: IStudentSubject) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentSubject,
  })
);

export const enrollError = createAction<IStudentSubjectStateContext>(
  StudentSubjectActionEnums.enrollError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Unenroll Actions
export const unenrollPending = createAction<IStudentSubjectStateContext>(
  StudentSubjectActionEnums.unenrollPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const unenrollSuccess = createAction<IStudentSubjectStateContext>(
  StudentSubjectActionEnums.unenrollSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const unenrollError = createAction<IStudentSubjectStateContext>(
  StudentSubjectActionEnums.unenrollError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
