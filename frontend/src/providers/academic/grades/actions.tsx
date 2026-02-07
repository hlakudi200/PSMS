import { createAction } from "redux-actions";
import { IGradeStateContext } from "./context";
import { IGrade, IGradeList, IPagedResult, IListResult } from "../shared/interfaces";

export enum GradeActionEnums {
  getGradePending = "GET_GRADE_PENDING",
  getGradeSuccess = "GET_GRADE_SUCCESS",
  getGradeError = "GET_GRADE_ERROR",

  getGradesPending = "GET_GRADES_PENDING",
  getGradesSuccess = "GET_GRADES_SUCCESS",
  getGradesError = "GET_GRADES_ERROR",

  createGradePending = "CREATE_GRADE_PENDING",
  createGradeSuccess = "CREATE_GRADE_SUCCESS",
  createGradeError = "CREATE_GRADE_ERROR",

  updateGradePending = "UPDATE_GRADE_PENDING",
  updateGradeSuccess = "UPDATE_GRADE_SUCCESS",
  updateGradeError = "UPDATE_GRADE_ERROR",

  deleteGradePending = "DELETE_GRADE_PENDING",
  deleteGradeSuccess = "DELETE_GRADE_SUCCESS",
  deleteGradeError = "DELETE_GRADE_ERROR",

  getActiveGradesPending = "GET_ACTIVE_GRADES_PENDING",
  getActiveGradesSuccess = "GET_ACTIVE_GRADES_SUCCESS",
  getActiveGradesError = "GET_ACTIVE_GRADES_ERROR",

  getByPhasePending = "GET_BY_PHASE_PENDING",
  getByPhaseSuccess = "GET_BY_PHASE_SUCCESS",
  getByPhaseError = "GET_BY_PHASE_ERROR",

  activatePending = "ACTIVATE_GRADE_PENDING",
  activateSuccess = "ACTIVATE_GRADE_SUCCESS",
  activateError = "ACTIVATE_GRADE_ERROR",

  deactivatePending = "DEACTIVATE_GRADE_PENDING",
  deactivateSuccess = "DEACTIVATE_GRADE_SUCCESS",
  deactivateError = "DEACTIVATE_GRADE_ERROR",
}

// Get Single Grade
export const getGradePending = createAction<IGradeStateContext>(
  GradeActionEnums.getGradePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getGradeSuccess = createAction<IGradeStateContext, IGrade>(
  GradeActionEnums.getGradeSuccess,
  (grade: IGrade) => ({
    isPending: false, isSuccess: true, isError: false, grade,
  })
);
export const getGradeError = createAction<IGradeStateContext>(
  GradeActionEnums.getGradeError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All Grades (paged)
export const getGradesPending = createAction<IGradeStateContext>(
  GradeActionEnums.getGradesPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getGradesSuccess = createAction<IGradeStateContext, IPagedResult<IGradeList>>(
  GradeActionEnums.getGradesSuccess,
  (result: IPagedResult<IGradeList>) => ({
    isPending: false, isSuccess: true, isError: false,
    grades: result.items, totalCount: result.totalCount,
  })
);
export const getGradesError = createAction<IGradeStateContext>(
  GradeActionEnums.getGradesError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Grade
export const createGradePending = createAction<IGradeStateContext>(
  GradeActionEnums.createGradePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const createGradeSuccess = createAction<IGradeStateContext, IGrade>(
  GradeActionEnums.createGradeSuccess,
  (grade: IGrade) => ({
    isPending: false, isSuccess: true, isError: false, grade,
  })
);
export const createGradeError = createAction<IGradeStateContext>(
  GradeActionEnums.createGradeError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Grade
export const updateGradePending = createAction<IGradeStateContext>(
  GradeActionEnums.updateGradePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const updateGradeSuccess = createAction<IGradeStateContext, IGrade>(
  GradeActionEnums.updateGradeSuccess,
  (grade: IGrade) => ({
    isPending: false, isSuccess: true, isError: false, grade,
  })
);
export const updateGradeError = createAction<IGradeStateContext>(
  GradeActionEnums.updateGradeError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Grade
export const deleteGradePending = createAction<IGradeStateContext>(
  GradeActionEnums.deleteGradePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const deleteGradeSuccess = createAction<IGradeStateContext>(
  GradeActionEnums.deleteGradeSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const deleteGradeError = createAction<IGradeStateContext>(
  GradeActionEnums.deleteGradeError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Active Grades
export const getActiveGradesPending = createAction<IGradeStateContext>(
  GradeActionEnums.getActiveGradesPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getActiveGradesSuccess = createAction<IGradeStateContext, IListResult<IGradeList>>(
  GradeActionEnums.getActiveGradesSuccess,
  (result: IListResult<IGradeList>) => ({
    isPending: false, isSuccess: true, isError: false,
    activeGrades: result.items,
  })
);
export const getActiveGradesError = createAction<IGradeStateContext>(
  GradeActionEnums.getActiveGradesError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Phase
export const getByPhasePending = createAction<IGradeStateContext>(
  GradeActionEnums.getByPhasePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getByPhaseSuccess = createAction<IGradeStateContext, IListResult<IGradeList>>(
  GradeActionEnums.getByPhaseSuccess,
  (result: IListResult<IGradeList>) => ({
    isPending: false, isSuccess: true, isError: false,
    grades: result.items,
  })
);
export const getByPhaseError = createAction<IGradeStateContext>(
  GradeActionEnums.getByPhaseError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Activate Grade
export const activatePending = createAction<IGradeStateContext>(
  GradeActionEnums.activatePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const activateSuccess = createAction<IGradeStateContext>(
  GradeActionEnums.activateSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const activateError = createAction<IGradeStateContext>(
  GradeActionEnums.activateError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Deactivate Grade
export const deactivatePending = createAction<IGradeStateContext>(
  GradeActionEnums.deactivatePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const deactivateSuccess = createAction<IGradeStateContext>(
  GradeActionEnums.deactivateSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const deactivateError = createAction<IGradeStateContext>(
  GradeActionEnums.deactivateError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
