import { createAction } from "redux-actions";
import { IAfterCareStateContext } from "./context";
import { IAfterCare, IAfterCareList, IPagedResult, IListResult } from "../shared/interfaces";

export enum AfterCareActionEnums {
  getAfterCarePending = "GET_AFTERCARE_PENDING",
  getAfterCareSuccess = "GET_AFTERCARE_SUCCESS",
  getAfterCareError = "GET_AFTERCARE_ERROR",

  getAfterCaresPending = "GET_AFTERCARES_PENDING",
  getAfterCaresSuccess = "GET_AFTERCARES_SUCCESS",
  getAfterCaresError = "GET_AFTERCARES_ERROR",

  getByAcademicYearPending = "GET_AFTERCARES_BY_ACADEMIC_YEAR_PENDING",
  getByAcademicYearSuccess = "GET_AFTERCARES_BY_ACADEMIC_YEAR_SUCCESS",
  getByAcademicYearError = "GET_AFTERCARES_BY_ACADEMIC_YEAR_ERROR",

  createAfterCarePending = "CREATE_AFTERCARE_PENDING",
  createAfterCareSuccess = "CREATE_AFTERCARE_SUCCESS",
  createAfterCareError = "CREATE_AFTERCARE_ERROR",

  updateAfterCarePending = "UPDATE_AFTERCARE_PENDING",
  updateAfterCareSuccess = "UPDATE_AFTERCARE_SUCCESS",
  updateAfterCareError = "UPDATE_AFTERCARE_ERROR",

  deleteAfterCarePending = "DELETE_AFTERCARE_PENDING",
  deleteAfterCareSuccess = "DELETE_AFTERCARE_SUCCESS",
  deleteAfterCareError = "DELETE_AFTERCARE_ERROR",

  activatePending = "ACTIVATE_AFTERCARE_PENDING",
  activateSuccess = "ACTIVATE_AFTERCARE_SUCCESS",
  activateError = "ACTIVATE_AFTERCARE_ERROR",

  deactivatePending = "DEACTIVATE_AFTERCARE_PENDING",
  deactivateSuccess = "DEACTIVATE_AFTERCARE_SUCCESS",
  deactivateError = "DEACTIVATE_AFTERCARE_ERROR",
}

// Get Single AfterCare
export const getAfterCarePending = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.getAfterCarePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getAfterCareSuccess = createAction<IAfterCareStateContext, IAfterCare>(
  AfterCareActionEnums.getAfterCareSuccess,
  (afterCare: IAfterCare) => ({
    isPending: false, isSuccess: true, isError: false, afterCare,
  })
);
export const getAfterCareError = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.getAfterCareError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All AfterCares (paged)
export const getAfterCaresPending = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.getAfterCaresPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getAfterCaresSuccess = createAction<IAfterCareStateContext, IPagedResult<IAfterCareList>>(
  AfterCareActionEnums.getAfterCaresSuccess,
  (result: IPagedResult<IAfterCareList>) => ({
    isPending: false, isSuccess: true, isError: false,
    afterCares: result.items, totalCount: result.totalCount,
  })
);
export const getAfterCaresError = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.getAfterCaresError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Academic Year
export const getByAcademicYearPending = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.getByAcademicYearPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getByAcademicYearSuccess = createAction<IAfterCareStateContext, IListResult<IAfterCareList>>(
  AfterCareActionEnums.getByAcademicYearSuccess,
  (result: IListResult<IAfterCareList>) => ({
    isPending: false, isSuccess: true, isError: false,
    afterCares: result.items,
  })
);
export const getByAcademicYearError = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.getByAcademicYearError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create AfterCare
export const createAfterCarePending = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.createAfterCarePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const createAfterCareSuccess = createAction<IAfterCareStateContext, IAfterCare>(
  AfterCareActionEnums.createAfterCareSuccess,
  (afterCare: IAfterCare) => ({
    isPending: false, isSuccess: true, isError: false, afterCare,
  })
);
export const createAfterCareError = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.createAfterCareError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update AfterCare
export const updateAfterCarePending = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.updateAfterCarePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const updateAfterCareSuccess = createAction<IAfterCareStateContext, IAfterCare>(
  AfterCareActionEnums.updateAfterCareSuccess,
  (afterCare: IAfterCare) => ({
    isPending: false, isSuccess: true, isError: false, afterCare,
  })
);
export const updateAfterCareError = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.updateAfterCareError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete AfterCare
export const deleteAfterCarePending = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.deleteAfterCarePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const deleteAfterCareSuccess = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.deleteAfterCareSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const deleteAfterCareError = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.deleteAfterCareError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Activate AfterCare
export const activatePending = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.activatePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const activateSuccess = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.activateSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const activateError = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.activateError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Deactivate AfterCare
export const deactivatePending = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.deactivatePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const deactivateSuccess = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.deactivateSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const deactivateError = createAction<IAfterCareStateContext>(
  AfterCareActionEnums.deactivateError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
