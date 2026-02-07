import { createAction } from "redux-actions";
import { IAcademicYearStateContext } from "./context";
import { IAcademicYear, IPagedResult } from "../shared/interfaces";

export enum AcademicYearActionEnums {
  getAcademicYearsPending = "GET_ACADEMIC_YEARS_PENDING",
  getAcademicYearsSuccess = "GET_ACADEMIC_YEARS_SUCCESS",
  getAcademicYearsError = "GET_ACADEMIC_YEARS_ERROR",

  getAcademicYearPending = "GET_ACADEMIC_YEAR_PENDING",
  getAcademicYearSuccess = "GET_ACADEMIC_YEAR_SUCCESS",
  getAcademicYearError = "GET_ACADEMIC_YEAR_ERROR",

  getCurrentAcademicYearPending = "GET_CURRENT_ACADEMIC_YEAR_PENDING",
  getCurrentAcademicYearSuccess = "GET_CURRENT_ACADEMIC_YEAR_SUCCESS",
  getCurrentAcademicYearError = "GET_CURRENT_ACADEMIC_YEAR_ERROR",

  createAcademicYearPending = "CREATE_ACADEMIC_YEAR_PENDING",
  createAcademicYearSuccess = "CREATE_ACADEMIC_YEAR_SUCCESS",
  createAcademicYearError = "CREATE_ACADEMIC_YEAR_ERROR",

  updateAcademicYearPending = "UPDATE_ACADEMIC_YEAR_PENDING",
  updateAcademicYearSuccess = "UPDATE_ACADEMIC_YEAR_SUCCESS",
  updateAcademicYearError = "UPDATE_ACADEMIC_YEAR_ERROR",

  deleteAcademicYearPending = "DELETE_ACADEMIC_YEAR_PENDING",
  deleteAcademicYearSuccess = "DELETE_ACADEMIC_YEAR_SUCCESS",
  deleteAcademicYearError = "DELETE_ACADEMIC_YEAR_ERROR",

  setAsCurrentPending = "SET_AS_CURRENT_PENDING",
  setAsCurrentSuccess = "SET_AS_CURRENT_SUCCESS",
  setAsCurrentError = "SET_AS_CURRENT_ERROR",
}

// Get All Academic Years Actions
export const getAcademicYearsPending = createAction<IAcademicYearStateContext>(
  AcademicYearActionEnums.getAcademicYearsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAcademicYearsSuccess = createAction<
  IAcademicYearStateContext,
  IPagedResult<IAcademicYear>
>(
  AcademicYearActionEnums.getAcademicYearsSuccess,
  (result: IPagedResult<IAcademicYear>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    academicYears: result.items,
    totalCount: result.totalCount,
  })
);

export const getAcademicYearsError = createAction<IAcademicYearStateContext>(
  AcademicYearActionEnums.getAcademicYearsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Single Academic Year Actions
export const getAcademicYearPending = createAction<IAcademicYearStateContext>(
  AcademicYearActionEnums.getAcademicYearPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAcademicYearSuccess = createAction<
  IAcademicYearStateContext,
  IAcademicYear
>(
  AcademicYearActionEnums.getAcademicYearSuccess,
  (academicYear: IAcademicYear) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    academicYear,
  })
);

export const getAcademicYearError = createAction<IAcademicYearStateContext>(
  AcademicYearActionEnums.getAcademicYearError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Current Academic Year Actions
export const getCurrentAcademicYearPending =
  createAction<IAcademicYearStateContext>(
    AcademicYearActionEnums.getCurrentAcademicYearPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
  );

export const getCurrentAcademicYearSuccess = createAction<
  IAcademicYearStateContext,
  IAcademicYear
>(
  AcademicYearActionEnums.getCurrentAcademicYearSuccess,
  (academicYear: IAcademicYear) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    academicYear,
  })
);

export const getCurrentAcademicYearError =
  createAction<IAcademicYearStateContext>(
    AcademicYearActionEnums.getCurrentAcademicYearError,
    () => ({ isPending: false, isSuccess: false, isError: true })
  );

// Create Academic Year Actions
export const createAcademicYearPending = createAction<IAcademicYearStateContext>(
  AcademicYearActionEnums.createAcademicYearPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createAcademicYearSuccess = createAction<
  IAcademicYearStateContext,
  IAcademicYear
>(
  AcademicYearActionEnums.createAcademicYearSuccess,
  (academicYear: IAcademicYear) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    academicYear,
  })
);

export const createAcademicYearError = createAction<IAcademicYearStateContext>(
  AcademicYearActionEnums.createAcademicYearError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Academic Year Actions
export const updateAcademicYearPending = createAction<IAcademicYearStateContext>(
  AcademicYearActionEnums.updateAcademicYearPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateAcademicYearSuccess = createAction<
  IAcademicYearStateContext,
  IAcademicYear
>(
  AcademicYearActionEnums.updateAcademicYearSuccess,
  (academicYear: IAcademicYear) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    academicYear,
  })
);

export const updateAcademicYearError = createAction<IAcademicYearStateContext>(
  AcademicYearActionEnums.updateAcademicYearError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Academic Year Actions
export const deleteAcademicYearPending = createAction<IAcademicYearStateContext>(
  AcademicYearActionEnums.deleteAcademicYearPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteAcademicYearSuccess = createAction<IAcademicYearStateContext>(
  AcademicYearActionEnums.deleteAcademicYearSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteAcademicYearError = createAction<IAcademicYearStateContext>(
  AcademicYearActionEnums.deleteAcademicYearError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Set As Current Actions
export const setAsCurrentPending = createAction<IAcademicYearStateContext>(
  AcademicYearActionEnums.setAsCurrentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const setAsCurrentSuccess = createAction<
  IAcademicYearStateContext,
  IAcademicYear
>(
  AcademicYearActionEnums.setAsCurrentSuccess,
  (academicYear: IAcademicYear) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    academicYear,
  })
);

export const setAsCurrentError = createAction<IAcademicYearStateContext>(
  AcademicYearActionEnums.setAsCurrentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
