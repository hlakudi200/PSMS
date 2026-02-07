import { createAction } from "redux-actions";
import { IGradeSubjectStateContext } from "./context";
import { IGradeSubject, ISubject, IListResult } from "../shared/interfaces";

export enum GradeSubjectActionEnums {
  getByGradePending = "GET_GRADE_SUBJECTS_BY_GRADE_PENDING",
  getByGradeSuccess = "GET_GRADE_SUBJECTS_BY_GRADE_SUCCESS",
  getByGradeError = "GET_GRADE_SUBJECTS_BY_GRADE_ERROR",

  getBySubjectPending = "GET_GRADE_SUBJECTS_BY_SUBJECT_PENDING",
  getBySubjectSuccess = "GET_GRADE_SUBJECTS_BY_SUBJECT_SUCCESS",
  getBySubjectError = "GET_GRADE_SUBJECTS_BY_SUBJECT_ERROR",

  assignPending = "ASSIGN_GRADE_SUBJECT_PENDING",
  assignSuccess = "ASSIGN_GRADE_SUBJECT_SUCCESS",
  assignError = "ASSIGN_GRADE_SUBJECT_ERROR",

  unassignPending = "UNASSIGN_GRADE_SUBJECT_PENDING",
  unassignSuccess = "UNASSIGN_GRADE_SUBJECT_SUCCESS",
  unassignError = "UNASSIGN_GRADE_SUBJECT_ERROR",

  getUnassignedSubjectsPending = "GET_UNASSIGNED_SUBJECTS_PENDING",
  getUnassignedSubjectsSuccess = "GET_UNASSIGNED_SUBJECTS_SUCCESS",
  getUnassignedSubjectsError = "GET_UNASSIGNED_SUBJECTS_ERROR",
}

// Get By Grade Actions
export const getByGradePending = createAction<IGradeSubjectStateContext>(
  GradeSubjectActionEnums.getByGradePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByGradeSuccess = createAction<
  IGradeSubjectStateContext,
  IListResult<IGradeSubject>
>(
  GradeSubjectActionEnums.getByGradeSuccess,
  (result: IListResult<IGradeSubject>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    gradeSubjects: result.items,
  })
);

export const getByGradeError = createAction<IGradeSubjectStateContext>(
  GradeSubjectActionEnums.getByGradeError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Subject Actions
export const getBySubjectPending = createAction<IGradeSubjectStateContext>(
  GradeSubjectActionEnums.getBySubjectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getBySubjectSuccess = createAction<
  IGradeSubjectStateContext,
  IListResult<IGradeSubject>
>(
  GradeSubjectActionEnums.getBySubjectSuccess,
  (result: IListResult<IGradeSubject>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    gradeSubjects: result.items,
  })
);

export const getBySubjectError = createAction<IGradeSubjectStateContext>(
  GradeSubjectActionEnums.getBySubjectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Assign Actions
export const assignPending = createAction<IGradeSubjectStateContext>(
  GradeSubjectActionEnums.assignPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const assignSuccess = createAction<IGradeSubjectStateContext, IGradeSubject>(
  GradeSubjectActionEnums.assignSuccess,
  (gradeSubject: IGradeSubject) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    gradeSubject,
  })
);

export const assignError = createAction<IGradeSubjectStateContext>(
  GradeSubjectActionEnums.assignError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Unassign Actions
export const unassignPending = createAction<IGradeSubjectStateContext>(
  GradeSubjectActionEnums.unassignPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const unassignSuccess = createAction<IGradeSubjectStateContext>(
  GradeSubjectActionEnums.unassignSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const unassignError = createAction<IGradeSubjectStateContext>(
  GradeSubjectActionEnums.unassignError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Unassigned Subjects Actions
export const getUnassignedSubjectsPending = createAction<IGradeSubjectStateContext>(
  GradeSubjectActionEnums.getUnassignedSubjectsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getUnassignedSubjectsSuccess = createAction<
  IGradeSubjectStateContext,
  IListResult<ISubject>
>(
  GradeSubjectActionEnums.getUnassignedSubjectsSuccess,
  (result: IListResult<ISubject>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    unassignedSubjects: result.items,
  })
);

export const getUnassignedSubjectsError = createAction<IGradeSubjectStateContext>(
  GradeSubjectActionEnums.getUnassignedSubjectsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
