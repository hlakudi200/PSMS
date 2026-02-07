import { createAction } from "redux-actions";
import { IClassStateContext } from "./context";
import { IClass, IPagedResult, IListResult } from "../shared/interfaces";

export enum ClassActionEnums {
  getClassesPending = "GET_CLASSES_PENDING",
  getClassesSuccess = "GET_CLASSES_SUCCESS",
  getClassesError = "GET_CLASSES_ERROR",

  getClassPending = "GET_CLASS_PENDING",
  getClassSuccess = "GET_CLASS_SUCCESS",
  getClassError = "GET_CLASS_ERROR",

  createClassPending = "CREATE_CLASS_PENDING",
  createClassSuccess = "CREATE_CLASS_SUCCESS",
  createClassError = "CREATE_CLASS_ERROR",

  updateClassPending = "UPDATE_CLASS_PENDING",
  updateClassSuccess = "UPDATE_CLASS_SUCCESS",
  updateClassError = "UPDATE_CLASS_ERROR",

  deleteClassPending = "DELETE_CLASS_PENDING",
  deleteClassSuccess = "DELETE_CLASS_SUCCESS",
  deleteClassError = "DELETE_CLASS_ERROR",

  getActiveClassesPending = "GET_ACTIVE_CLASSES_PENDING",
  getActiveClassesSuccess = "GET_ACTIVE_CLASSES_SUCCESS",
  getActiveClassesError = "GET_ACTIVE_CLASSES_ERROR",

  getClassesByGradePending = "GET_CLASSES_BY_GRADE_PENDING",
  getClassesByGradeSuccess = "GET_CLASSES_BY_GRADE_SUCCESS",
  getClassesByGradeError = "GET_CLASSES_BY_GRADE_ERROR",

  getClassesByAcademicYearPending = "GET_CLASSES_BY_ACADEMIC_YEAR_PENDING",
  getClassesByAcademicYearSuccess = "GET_CLASSES_BY_ACADEMIC_YEAR_SUCCESS",
  getClassesByAcademicYearError = "GET_CLASSES_BY_ACADEMIC_YEAR_ERROR",

  assignClassTeacherPending = "ASSIGN_CLASS_TEACHER_PENDING",
  assignClassTeacherSuccess = "ASSIGN_CLASS_TEACHER_SUCCESS",
  assignClassTeacherError = "ASSIGN_CLASS_TEACHER_ERROR",

  removeClassTeacherPending = "REMOVE_CLASS_TEACHER_PENDING",
  removeClassTeacherSuccess = "REMOVE_CLASS_TEACHER_SUCCESS",
  removeClassTeacherError = "REMOVE_CLASS_TEACHER_ERROR",

  activateClassPending = "ACTIVATE_CLASS_PENDING",
  activateClassSuccess = "ACTIVATE_CLASS_SUCCESS",
  activateClassError = "ACTIVATE_CLASS_ERROR",

  deactivateClassPending = "DEACTIVATE_CLASS_PENDING",
  deactivateClassSuccess = "DEACTIVATE_CLASS_SUCCESS",
  deactivateClassError = "DEACTIVATE_CLASS_ERROR",
}

// Get All Classes Actions
export const getClassesPending = createAction<IClassStateContext>(
  ClassActionEnums.getClassesPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getClassesSuccess = createAction<
  IClassStateContext,
  IPagedResult<IClass>
>(
  ClassActionEnums.getClassesSuccess,
  (result: IPagedResult<IClass>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    classes: result.items,
    totalCount: result.totalCount,
  })
);

export const getClassesError = createAction<IClassStateContext>(
  ClassActionEnums.getClassesError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Single Class Actions
export const getClassPending = createAction<IClassStateContext>(
  ClassActionEnums.getClassPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getClassSuccess = createAction<IClassStateContext, IClass>(
  ClassActionEnums.getClassSuccess,
  (classItem: IClass) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    class: classItem,
  })
);

export const getClassError = createAction<IClassStateContext>(
  ClassActionEnums.getClassError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Class Actions
export const createClassPending = createAction<IClassStateContext>(
  ClassActionEnums.createClassPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createClassSuccess = createAction<IClassStateContext, IClass>(
  ClassActionEnums.createClassSuccess,
  (classItem: IClass) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    class: classItem,
  })
);

export const createClassError = createAction<IClassStateContext>(
  ClassActionEnums.createClassError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Class Actions
export const updateClassPending = createAction<IClassStateContext>(
  ClassActionEnums.updateClassPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateClassSuccess = createAction<IClassStateContext, IClass>(
  ClassActionEnums.updateClassSuccess,
  (classItem: IClass) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    class: classItem,
  })
);

export const updateClassError = createAction<IClassStateContext>(
  ClassActionEnums.updateClassError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Class Actions
export const deleteClassPending = createAction<IClassStateContext>(
  ClassActionEnums.deleteClassPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteClassSuccess = createAction<IClassStateContext>(
  ClassActionEnums.deleteClassSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteClassError = createAction<IClassStateContext>(
  ClassActionEnums.deleteClassError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Active Classes Actions
export const getActiveClassesPending = createAction<IClassStateContext>(
  ClassActionEnums.getActiveClassesPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getActiveClassesSuccess = createAction<
  IClassStateContext,
  IListResult<IClass>
>(
  ClassActionEnums.getActiveClassesSuccess,
  (result: IListResult<IClass>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    classes: result.items,
  })
);

export const getActiveClassesError = createAction<IClassStateContext>(
  ClassActionEnums.getActiveClassesError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Classes By Grade Actions
export const getClassesByGradePending = createAction<IClassStateContext>(
  ClassActionEnums.getClassesByGradePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getClassesByGradeSuccess = createAction<
  IClassStateContext,
  IListResult<IClass>
>(
  ClassActionEnums.getClassesByGradeSuccess,
  (result: IListResult<IClass>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    classes: result.items,
  })
);

export const getClassesByGradeError = createAction<IClassStateContext>(
  ClassActionEnums.getClassesByGradeError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Classes By Academic Year Actions
export const getClassesByAcademicYearPending = createAction<IClassStateContext>(
  ClassActionEnums.getClassesByAcademicYearPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getClassesByAcademicYearSuccess = createAction<
  IClassStateContext,
  IListResult<IClass>
>(
  ClassActionEnums.getClassesByAcademicYearSuccess,
  (result: IListResult<IClass>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    classes: result.items,
  })
);

export const getClassesByAcademicYearError = createAction<IClassStateContext>(
  ClassActionEnums.getClassesByAcademicYearError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Assign Class Teacher Actions
export const assignClassTeacherPending = createAction<IClassStateContext>(
  ClassActionEnums.assignClassTeacherPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const assignClassTeacherSuccess = createAction<IClassStateContext, IClass>(
  ClassActionEnums.assignClassTeacherSuccess,
  (classItem: IClass) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    class: classItem,
  })
);

export const assignClassTeacherError = createAction<IClassStateContext>(
  ClassActionEnums.assignClassTeacherError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Remove Class Teacher Actions
export const removeClassTeacherPending = createAction<IClassStateContext>(
  ClassActionEnums.removeClassTeacherPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const removeClassTeacherSuccess = createAction<IClassStateContext, IClass>(
  ClassActionEnums.removeClassTeacherSuccess,
  (classItem: IClass) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    class: classItem,
  })
);

export const removeClassTeacherError = createAction<IClassStateContext>(
  ClassActionEnums.removeClassTeacherError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Activate Class Actions
export const activateClassPending = createAction<IClassStateContext>(
  ClassActionEnums.activateClassPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const activateClassSuccess = createAction<IClassStateContext>(
  ClassActionEnums.activateClassSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const activateClassError = createAction<IClassStateContext>(
  ClassActionEnums.activateClassError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Deactivate Class Actions
export const deactivateClassPending = createAction<IClassStateContext>(
  ClassActionEnums.deactivateClassPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deactivateClassSuccess = createAction<IClassStateContext>(
  ClassActionEnums.deactivateClassSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deactivateClassError = createAction<IClassStateContext>(
  ClassActionEnums.deactivateClassError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
