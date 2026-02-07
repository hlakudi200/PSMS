import { createAction } from "redux-actions";
import { ITermStateContext } from "./context";
import { ITerm, IPagedResult } from "../shared/interfaces";

export enum TermActionEnums {
  getTermsPending = "GET_TERMS_PENDING",
  getTermsSuccess = "GET_TERMS_SUCCESS",
  getTermsError = "GET_TERMS_ERROR",

  getTermPending = "GET_TERM_PENDING",
  getTermSuccess = "GET_TERM_SUCCESS",
  getTermError = "GET_TERM_ERROR",

  getCurrentTermPending = "GET_CURRENT_TERM_PENDING",
  getCurrentTermSuccess = "GET_CURRENT_TERM_SUCCESS",
  getCurrentTermError = "GET_CURRENT_TERM_ERROR",

  createTermPending = "CREATE_TERM_PENDING",
  createTermSuccess = "CREATE_TERM_SUCCESS",
  createTermError = "CREATE_TERM_ERROR",

  updateTermPending = "UPDATE_TERM_PENDING",
  updateTermSuccess = "UPDATE_TERM_SUCCESS",
  updateTermError = "UPDATE_TERM_ERROR",

  deleteTermPending = "DELETE_TERM_PENDING",
  deleteTermSuccess = "DELETE_TERM_SUCCESS",
  deleteTermError = "DELETE_TERM_ERROR",

  setAsCurrentPending = "SET_TERM_AS_CURRENT_PENDING",
  setAsCurrentSuccess = "SET_TERM_AS_CURRENT_SUCCESS",
  setAsCurrentError = "SET_TERM_AS_CURRENT_ERROR",
}

// Get All Terms Actions
export const getTermsPending = createAction<ITermStateContext>(
  TermActionEnums.getTermsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getTermsSuccess = createAction<
  ITermStateContext,
  IPagedResult<ITerm>
>(
  TermActionEnums.getTermsSuccess,
  (result: IPagedResult<ITerm>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    terms: result.items,
    totalCount: result.totalCount,
  })
);

export const getTermsError = createAction<ITermStateContext>(
  TermActionEnums.getTermsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Single Term Actions
export const getTermPending = createAction<ITermStateContext>(
  TermActionEnums.getTermPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getTermSuccess = createAction<ITermStateContext, ITerm>(
  TermActionEnums.getTermSuccess,
  (term: ITerm) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    term,
  })
);

export const getTermError = createAction<ITermStateContext>(
  TermActionEnums.getTermError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Current Term Actions
export const getCurrentTermPending = createAction<ITermStateContext>(
  TermActionEnums.getCurrentTermPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getCurrentTermSuccess = createAction<ITermStateContext, ITerm>(
  TermActionEnums.getCurrentTermSuccess,
  (term: ITerm) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    term,
  })
);

export const getCurrentTermError = createAction<ITermStateContext>(
  TermActionEnums.getCurrentTermError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Term Actions
export const createTermPending = createAction<ITermStateContext>(
  TermActionEnums.createTermPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createTermSuccess = createAction<ITermStateContext, ITerm>(
  TermActionEnums.createTermSuccess,
  (term: ITerm) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    term,
  })
);

export const createTermError = createAction<ITermStateContext>(
  TermActionEnums.createTermError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Term Actions
export const updateTermPending = createAction<ITermStateContext>(
  TermActionEnums.updateTermPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateTermSuccess = createAction<ITermStateContext, ITerm>(
  TermActionEnums.updateTermSuccess,
  (term: ITerm) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    term,
  })
);

export const updateTermError = createAction<ITermStateContext>(
  TermActionEnums.updateTermError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Term Actions
export const deleteTermPending = createAction<ITermStateContext>(
  TermActionEnums.deleteTermPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteTermSuccess = createAction<ITermStateContext>(
  TermActionEnums.deleteTermSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteTermError = createAction<ITermStateContext>(
  TermActionEnums.deleteTermError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Set As Current Actions
export const setAsCurrentPending = createAction<ITermStateContext>(
  TermActionEnums.setAsCurrentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const setAsCurrentSuccess = createAction<ITermStateContext, ITerm>(
  TermActionEnums.setAsCurrentSuccess,
  (term: ITerm) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    term,
  })
);

export const setAsCurrentError = createAction<ITermStateContext>(
  TermActionEnums.setAsCurrentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
