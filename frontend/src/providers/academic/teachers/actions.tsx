import { createAction } from "redux-actions";
import { ITeacherStateContext } from "./context";
import { ITeacher, IPagedResult } from "../shared/interfaces";

export enum TeacherActionEnums {
  getTeachersPending = "GET_TEACHERS_PENDING",
  getTeachersSuccess = "GET_TEACHERS_SUCCESS",
  getTeachersError = "GET_TEACHERS_ERROR",

  getTeacherPending = "GET_TEACHER_PENDING",
  getTeacherSuccess = "GET_TEACHER_SUCCESS",
  getTeacherError = "GET_TEACHER_ERROR",

  getCurrentTeacherPending = "GET_CURRENT_TEACHER_PENDING",
  getCurrentTeacherSuccess = "GET_CURRENT_TEACHER_SUCCESS",
  getCurrentTeacherError = "GET_CURRENT_TEACHER_ERROR",

  createTeacherPending = "CREATE_TEACHER_PENDING",
  createTeacherSuccess = "CREATE_TEACHER_SUCCESS",
  createTeacherError = "CREATE_TEACHER_ERROR",

  updateTeacherPending = "UPDATE_TEACHER_PENDING",
  updateTeacherSuccess = "UPDATE_TEACHER_SUCCESS",
  updateTeacherError = "UPDATE_TEACHER_ERROR",

  deleteTeacherPending = "DELETE_TEACHER_PENDING",
  deleteTeacherSuccess = "DELETE_TEACHER_SUCCESS",
  deleteTeacherError = "DELETE_TEACHER_ERROR",

  activateTeacherPending = "ACTIVATE_TEACHER_PENDING",
  activateTeacherSuccess = "ACTIVATE_TEACHER_SUCCESS",
  activateTeacherError = "ACTIVATE_TEACHER_ERROR",

  deactivateTeacherPending = "DEACTIVATE_TEACHER_PENDING",
  deactivateTeacherSuccess = "DEACTIVATE_TEACHER_SUCCESS",
  deactivateTeacherError = "DEACTIVATE_TEACHER_ERROR",
}

// Get All Teachers Actions
export const getTeachersPending = createAction<ITeacherStateContext>(
  TeacherActionEnums.getTeachersPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getTeachersSuccess = createAction<
  ITeacherStateContext,
  IPagedResult<ITeacher>
>(
  TeacherActionEnums.getTeachersSuccess,
  (result: IPagedResult<ITeacher>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    teachers: result.items,
    totalCount: result.totalCount,
  })
);

export const getTeachersError = createAction<ITeacherStateContext>(
  TeacherActionEnums.getTeachersError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Single Teacher Actions
export const getTeacherPending = createAction<ITeacherStateContext>(
  TeacherActionEnums.getTeacherPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getTeacherSuccess = createAction<ITeacherStateContext, ITeacher>(
  TeacherActionEnums.getTeacherSuccess,
  (teacher: ITeacher) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    teacher,
  })
);

export const getTeacherError = createAction<ITeacherStateContext>(
  TeacherActionEnums.getTeacherError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Teacher By Current User (returns null if no teacher record is linked
// to the active user — see backend Teacher/GetByCurrentUser).
export const getCurrentTeacherPending = createAction<ITeacherStateContext>(
  TeacherActionEnums.getCurrentTeacherPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getCurrentTeacherSuccess = createAction<
  ITeacherStateContext,
  ITeacher | null
>(
  TeacherActionEnums.getCurrentTeacherSuccess,
  (teacher: ITeacher | null) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    teacher: teacher ?? undefined,
  })
);

export const getCurrentTeacherError = createAction<ITeacherStateContext>(
  TeacherActionEnums.getCurrentTeacherError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Teacher Actions
export const createTeacherPending = createAction<ITeacherStateContext>(
  TeacherActionEnums.createTeacherPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createTeacherSuccess = createAction<ITeacherStateContext, ITeacher>(
  TeacherActionEnums.createTeacherSuccess,
  (teacher: ITeacher) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    teacher,
  })
);

export const createTeacherError = createAction<ITeacherStateContext>(
  TeacherActionEnums.createTeacherError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Teacher Actions
export const updateTeacherPending = createAction<ITeacherStateContext>(
  TeacherActionEnums.updateTeacherPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateTeacherSuccess = createAction<ITeacherStateContext, ITeacher>(
  TeacherActionEnums.updateTeacherSuccess,
  (teacher: ITeacher) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    teacher,
  })
);

export const updateTeacherError = createAction<ITeacherStateContext>(
  TeacherActionEnums.updateTeacherError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Teacher Actions
export const deleteTeacherPending = createAction<ITeacherStateContext>(
  TeacherActionEnums.deleteTeacherPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteTeacherSuccess = createAction<ITeacherStateContext>(
  TeacherActionEnums.deleteTeacherSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteTeacherError = createAction<ITeacherStateContext>(
  TeacherActionEnums.deleteTeacherError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Activate Teacher Actions
export const activateTeacherPending = createAction<ITeacherStateContext>(
  TeacherActionEnums.activateTeacherPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const activateTeacherSuccess = createAction<ITeacherStateContext>(
  TeacherActionEnums.activateTeacherSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);

export const activateTeacherError = createAction<ITeacherStateContext>(
  TeacherActionEnums.activateTeacherError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Deactivate Teacher Actions
export const deactivateTeacherPending = createAction<ITeacherStateContext>(
  TeacherActionEnums.deactivateTeacherPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deactivateTeacherSuccess = createAction<ITeacherStateContext>(
  TeacherActionEnums.deactivateTeacherSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);

export const deactivateTeacherError = createAction<ITeacherStateContext>(
  TeacherActionEnums.deactivateTeacherError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
