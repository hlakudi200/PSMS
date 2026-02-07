import { createAction } from "redux-actions";
import { IStudentStateContext } from "./context";
import { IStudent, IPagedResult } from "../shared/interfaces";

export enum StudentActionEnums {
  getStudentsPending = "GET_STUDENTS_PENDING",
  getStudentsSuccess = "GET_STUDENTS_SUCCESS",
  getStudentsError = "GET_STUDENTS_ERROR",

  getStudentPending = "GET_STUDENT_PENDING",
  getStudentSuccess = "GET_STUDENT_SUCCESS",
  getStudentError = "GET_STUDENT_ERROR",

  createStudentPending = "CREATE_STUDENT_PENDING",
  createStudentSuccess = "CREATE_STUDENT_SUCCESS",
  createStudentError = "CREATE_STUDENT_ERROR",

  updateStudentPending = "UPDATE_STUDENT_PENDING",
  updateStudentSuccess = "UPDATE_STUDENT_SUCCESS",
  updateStudentError = "UPDATE_STUDENT_ERROR",

  deleteStudentPending = "DELETE_STUDENT_PENDING",
  deleteStudentSuccess = "DELETE_STUDENT_SUCCESS",
  deleteStudentError = "DELETE_STUDENT_ERROR",
}

// Get All Students Actions
export const getStudentsPending = createAction<IStudentStateContext>(
  StudentActionEnums.getStudentsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getStudentsSuccess = createAction<
  IStudentStateContext,
  IPagedResult<IStudent>
>(
  StudentActionEnums.getStudentsSuccess,
  (result: IPagedResult<IStudent>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    students: result.items,
    totalCount: result.totalCount,
  })
);

export const getStudentsError = createAction<IStudentStateContext>(
  StudentActionEnums.getStudentsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Single Student Actions
export const getStudentPending = createAction<IStudentStateContext>(
  StudentActionEnums.getStudentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getStudentSuccess = createAction<IStudentStateContext, IStudent>(
  StudentActionEnums.getStudentSuccess,
  (student: IStudent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    student,
  })
);

export const getStudentError = createAction<IStudentStateContext>(
  StudentActionEnums.getStudentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Student Actions
export const createStudentPending = createAction<IStudentStateContext>(
  StudentActionEnums.createStudentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createStudentSuccess = createAction<IStudentStateContext, IStudent>(
  StudentActionEnums.createStudentSuccess,
  (student: IStudent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    student,
  })
);

export const createStudentError = createAction<IStudentStateContext>(
  StudentActionEnums.createStudentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Student Actions
export const updateStudentPending = createAction<IStudentStateContext>(
  StudentActionEnums.updateStudentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateStudentSuccess = createAction<IStudentStateContext, IStudent>(
  StudentActionEnums.updateStudentSuccess,
  (student: IStudent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    student,
  })
);

export const updateStudentError = createAction<IStudentStateContext>(
  StudentActionEnums.updateStudentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Student Actions
export const deleteStudentPending = createAction<IStudentStateContext>(
  StudentActionEnums.deleteStudentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteStudentSuccess = createAction<IStudentStateContext>(
  StudentActionEnums.deleteStudentSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteStudentError = createAction<IStudentStateContext>(
  StudentActionEnums.deleteStudentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
