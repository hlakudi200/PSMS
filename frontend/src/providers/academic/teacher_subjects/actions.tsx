import { createAction } from "redux-actions";
import { ITeacherSubjectStateContext } from "./context";
import { ITeacherSubject, IListResult } from "../shared/interfaces";

export enum TeacherSubjectActionEnums {
  getByTeacherPending = "GET_TEACHER_SUBJECTS_BY_TEACHER_PENDING",
  getByTeacherSuccess = "GET_TEACHER_SUBJECTS_BY_TEACHER_SUCCESS",
  getByTeacherError = "GET_TEACHER_SUBJECTS_BY_TEACHER_ERROR",

  getBySubjectPending = "GET_TEACHER_SUBJECTS_BY_SUBJECT_PENDING",
  getBySubjectSuccess = "GET_TEACHER_SUBJECTS_BY_SUBJECT_SUCCESS",
  getBySubjectError = "GET_TEACHER_SUBJECTS_BY_SUBJECT_ERROR",

  getByGradePending = "GET_TEACHER_SUBJECTS_BY_GRADE_PENDING",
  getByGradeSuccess = "GET_TEACHER_SUBJECTS_BY_GRADE_SUCCESS",
  getByGradeError = "GET_TEACHER_SUBJECTS_BY_GRADE_ERROR",

  assignPending = "ASSIGN_TEACHER_SUBJECT_PENDING",
  assignSuccess = "ASSIGN_TEACHER_SUBJECT_SUCCESS",
  assignError = "ASSIGN_TEACHER_SUBJECT_ERROR",

  unassignPending = "UNASSIGN_TEACHER_SUBJECT_PENDING",
  unassignSuccess = "UNASSIGN_TEACHER_SUBJECT_SUCCESS",
  unassignError = "UNASSIGN_TEACHER_SUBJECT_ERROR",
}

// Get By Teacher Actions
export const getByTeacherPending = createAction<ITeacherSubjectStateContext>(
  TeacherSubjectActionEnums.getByTeacherPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByTeacherSuccess = createAction<
  ITeacherSubjectStateContext,
  IListResult<ITeacherSubject>
>(
  TeacherSubjectActionEnums.getByTeacherSuccess,
  (result: IListResult<ITeacherSubject>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    teacherSubjects: result.items,
  })
);

export const getByTeacherError = createAction<ITeacherSubjectStateContext>(
  TeacherSubjectActionEnums.getByTeacherError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Subject Actions
export const getBySubjectPending = createAction<ITeacherSubjectStateContext>(
  TeacherSubjectActionEnums.getBySubjectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getBySubjectSuccess = createAction<
  ITeacherSubjectStateContext,
  IListResult<ITeacherSubject>
>(
  TeacherSubjectActionEnums.getBySubjectSuccess,
  (result: IListResult<ITeacherSubject>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    teacherSubjects: result.items,
  })
);

export const getBySubjectError = createAction<ITeacherSubjectStateContext>(
  TeacherSubjectActionEnums.getBySubjectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Grade Actions
export const getByGradePending = createAction<ITeacherSubjectStateContext>(
  TeacherSubjectActionEnums.getByGradePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByGradeSuccess = createAction<
  ITeacherSubjectStateContext,
  IListResult<ITeacherSubject>
>(
  TeacherSubjectActionEnums.getByGradeSuccess,
  (result: IListResult<ITeacherSubject>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    teacherSubjects: result.items,
  })
);

export const getByGradeError = createAction<ITeacherSubjectStateContext>(
  TeacherSubjectActionEnums.getByGradeError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Assign Actions
export const assignPending = createAction<ITeacherSubjectStateContext>(
  TeacherSubjectActionEnums.assignPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const assignSuccess = createAction<ITeacherSubjectStateContext, ITeacherSubject>(
  TeacherSubjectActionEnums.assignSuccess,
  (teacherSubject: ITeacherSubject) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    teacherSubject,
  })
);

export const assignError = createAction<ITeacherSubjectStateContext>(
  TeacherSubjectActionEnums.assignError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Unassign Actions
export const unassignPending = createAction<ITeacherSubjectStateContext>(
  TeacherSubjectActionEnums.unassignPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const unassignSuccess = createAction<ITeacherSubjectStateContext>(
  TeacherSubjectActionEnums.unassignSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const unassignError = createAction<ITeacherSubjectStateContext>(
  TeacherSubjectActionEnums.unassignError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
