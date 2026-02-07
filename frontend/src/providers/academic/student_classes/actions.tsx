import { createAction } from "redux-actions";
import { IStudentClassStateContext } from "./context";
import { IStudentClass, IStudentClassList, IListResult } from "../shared/interfaces";

export enum StudentClassActionEnums {
  getStudentClassPending = "GET_STUDENT_CLASS_PENDING",
  getStudentClassSuccess = "GET_STUDENT_CLASS_SUCCESS",
  getStudentClassError = "GET_STUDENT_CLASS_ERROR",

  getByStudentPending = "GET_STUDENT_CLASSES_BY_STUDENT_PENDING",
  getByStudentSuccess = "GET_STUDENT_CLASSES_BY_STUDENT_SUCCESS",
  getByStudentError = "GET_STUDENT_CLASSES_BY_STUDENT_ERROR",

  getByClassPending = "GET_STUDENT_CLASSES_BY_CLASS_PENDING",
  getByClassSuccess = "GET_STUDENT_CLASSES_BY_CLASS_SUCCESS",
  getByClassError = "GET_STUDENT_CLASSES_BY_CLASS_ERROR",

  getCurrentByStudentPending = "GET_CURRENT_STUDENT_CLASS_PENDING",
  getCurrentByStudentSuccess = "GET_CURRENT_STUDENT_CLASS_SUCCESS",
  getCurrentByStudentError = "GET_CURRENT_STUDENT_CLASS_ERROR",

  enrollPending = "ENROLL_STUDENT_CLASS_PENDING",
  enrollSuccess = "ENROLL_STUDENT_CLASS_SUCCESS",
  enrollError = "ENROLL_STUDENT_CLASS_ERROR",

  updatePending = "UPDATE_STUDENT_CLASS_PENDING",
  updateSuccess = "UPDATE_STUDENT_CLASS_SUCCESS",
  updateError = "UPDATE_STUDENT_CLASS_ERROR",

  endEnrollmentPending = "END_ENROLLMENT_PENDING",
  endEnrollmentSuccess = "END_ENROLLMENT_SUCCESS",
  endEnrollmentError = "END_ENROLLMENT_ERROR",

  setAsCurrentPending = "SET_AS_CURRENT_PENDING",
  setAsCurrentSuccess = "SET_AS_CURRENT_SUCCESS",
  setAsCurrentError = "SET_AS_CURRENT_ERROR",

  deleteStudentClassPending = "DELETE_STUDENT_CLASS_PENDING",
  deleteStudentClassSuccess = "DELETE_STUDENT_CLASS_SUCCESS",
  deleteStudentClassError = "DELETE_STUDENT_CLASS_ERROR",
}

// Get Single StudentClass Actions
export const getStudentClassPending = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.getStudentClassPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getStudentClassSuccess = createAction<IStudentClassStateContext, IStudentClass>(
  StudentClassActionEnums.getStudentClassSuccess,
  (studentClass: IStudentClass) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentClass,
  })
);

export const getStudentClassError = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.getStudentClassError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Student Actions
export const getByStudentPending = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.getByStudentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByStudentSuccess = createAction<
  IStudentClassStateContext,
  IListResult<IStudentClassList>
>(
  StudentClassActionEnums.getByStudentSuccess,
  (result: IListResult<IStudentClassList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentClasses: result.items,
  })
);

export const getByStudentError = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.getByStudentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Class Actions
export const getByClassPending = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.getByClassPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByClassSuccess = createAction<
  IStudentClassStateContext,
  IListResult<IStudentClassList>
>(
  StudentClassActionEnums.getByClassSuccess,
  (result: IListResult<IStudentClassList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentClasses: result.items,
  })
);

export const getByClassError = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.getByClassError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Current By Student Actions
export const getCurrentByStudentPending = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.getCurrentByStudentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getCurrentByStudentSuccess = createAction<IStudentClassStateContext, IStudentClass>(
  StudentClassActionEnums.getCurrentByStudentSuccess,
  (studentClass: IStudentClass) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentClass,
  })
);

export const getCurrentByStudentError = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.getCurrentByStudentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Enroll Actions
export const enrollPending = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.enrollPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const enrollSuccess = createAction<IStudentClassStateContext, IStudentClass>(
  StudentClassActionEnums.enrollSuccess,
  (studentClass: IStudentClass) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentClass,
  })
);

export const enrollError = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.enrollError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Actions
export const updatePending = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.updatePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateSuccess = createAction<IStudentClassStateContext, IStudentClass>(
  StudentClassActionEnums.updateSuccess,
  (studentClass: IStudentClass) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentClass,
  })
);

export const updateError = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.updateError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// End Enrollment Actions
export const endEnrollmentPending = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.endEnrollmentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const endEnrollmentSuccess = createAction<IStudentClassStateContext, IStudentClass>(
  StudentClassActionEnums.endEnrollmentSuccess,
  (studentClass: IStudentClass) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentClass,
  })
);

export const endEnrollmentError = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.endEnrollmentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Set As Current Actions
export const setAsCurrentPending = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.setAsCurrentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const setAsCurrentSuccess = createAction<IStudentClassStateContext, IStudentClass>(
  StudentClassActionEnums.setAsCurrentSuccess,
  (studentClass: IStudentClass) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentClass,
  })
);

export const setAsCurrentError = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.setAsCurrentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Actions
export const deleteStudentClassPending = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.deleteStudentClassPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteStudentClassSuccess = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.deleteStudentClassSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteStudentClassError = createAction<IStudentClassStateContext>(
  StudentClassActionEnums.deleteStudentClassError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
