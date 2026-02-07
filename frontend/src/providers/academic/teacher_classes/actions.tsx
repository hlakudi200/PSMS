import { createAction } from "redux-actions";
import { ITeacherClassStateContext } from "./context";
import { ITeacherClass, IListResult } from "../shared/interfaces";

export enum TeacherClassActionEnums {
  getByTeacherPending = "GET_TEACHER_CLASSES_BY_TEACHER_PENDING",
  getByTeacherSuccess = "GET_TEACHER_CLASSES_BY_TEACHER_SUCCESS",
  getByTeacherError = "GET_TEACHER_CLASSES_BY_TEACHER_ERROR",

  getByClassPending = "GET_TEACHER_CLASSES_BY_CLASS_PENDING",
  getByClassSuccess = "GET_TEACHER_CLASSES_BY_CLASS_SUCCESS",
  getByClassError = "GET_TEACHER_CLASSES_BY_CLASS_ERROR",

  assignPending = "ASSIGN_TEACHER_CLASS_PENDING",
  assignSuccess = "ASSIGN_TEACHER_CLASS_SUCCESS",
  assignError = "ASSIGN_TEACHER_CLASS_ERROR",

  unassignPending = "UNASSIGN_TEACHER_CLASS_PENDING",
  unassignSuccess = "UNASSIGN_TEACHER_CLASS_SUCCESS",
  unassignError = "UNASSIGN_TEACHER_CLASS_ERROR",
}

// Get By Teacher Actions
export const getByTeacherPending = createAction<ITeacherClassStateContext>(
  TeacherClassActionEnums.getByTeacherPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByTeacherSuccess = createAction<
  ITeacherClassStateContext,
  IListResult<ITeacherClass>
>(
  TeacherClassActionEnums.getByTeacherSuccess,
  (result: IListResult<ITeacherClass>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    teacherClasses: result.items,
  })
);

export const getByTeacherError = createAction<ITeacherClassStateContext>(
  TeacherClassActionEnums.getByTeacherError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Class Actions
export const getByClassPending = createAction<ITeacherClassStateContext>(
  TeacherClassActionEnums.getByClassPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByClassSuccess = createAction<
  ITeacherClassStateContext,
  IListResult<ITeacherClass>
>(
  TeacherClassActionEnums.getByClassSuccess,
  (result: IListResult<ITeacherClass>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    teacherClasses: result.items,
  })
);

export const getByClassError = createAction<ITeacherClassStateContext>(
  TeacherClassActionEnums.getByClassError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Assign Actions
export const assignPending = createAction<ITeacherClassStateContext>(
  TeacherClassActionEnums.assignPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const assignSuccess = createAction<ITeacherClassStateContext, ITeacherClass>(
  TeacherClassActionEnums.assignSuccess,
  (teacherClass: ITeacherClass) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    teacherClass,
  })
);

export const assignError = createAction<ITeacherClassStateContext>(
  TeacherClassActionEnums.assignError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Unassign Actions
export const unassignPending = createAction<ITeacherClassStateContext>(
  TeacherClassActionEnums.unassignPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const unassignSuccess = createAction<ITeacherClassStateContext>(
  TeacherClassActionEnums.unassignSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const unassignError = createAction<ITeacherClassStateContext>(
  TeacherClassActionEnums.unassignError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
