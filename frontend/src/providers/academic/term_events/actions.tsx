import { createAction } from "redux-actions";
import { ITermEventStateContext } from "./context";
import { ITermEvent, IListResult } from "../shared/interfaces";

export enum TermEventActionEnums {
  getTermEventPending = "GET_TERM_EVENT_PENDING",
  getTermEventSuccess = "GET_TERM_EVENT_SUCCESS",
  getTermEventError = "GET_TERM_EVENT_ERROR",

  getByTermPending = "GET_TERM_EVENTS_BY_TERM_PENDING",
  getByTermSuccess = "GET_TERM_EVENTS_BY_TERM_SUCCESS",
  getByTermError = "GET_TERM_EVENTS_BY_TERM_ERROR",

  getByDateRangePending = "GET_TERM_EVENTS_BY_DATE_RANGE_PENDING",
  getByDateRangeSuccess = "GET_TERM_EVENTS_BY_DATE_RANGE_SUCCESS",
  getByDateRangeError = "GET_TERM_EVENTS_BY_DATE_RANGE_ERROR",

  createTermEventPending = "CREATE_TERM_EVENT_PENDING",
  createTermEventSuccess = "CREATE_TERM_EVENT_SUCCESS",
  createTermEventError = "CREATE_TERM_EVENT_ERROR",

  updateTermEventPending = "UPDATE_TERM_EVENT_PENDING",
  updateTermEventSuccess = "UPDATE_TERM_EVENT_SUCCESS",
  updateTermEventError = "UPDATE_TERM_EVENT_ERROR",

  deleteTermEventPending = "DELETE_TERM_EVENT_PENDING",
  deleteTermEventSuccess = "DELETE_TERM_EVENT_SUCCESS",
  deleteTermEventError = "DELETE_TERM_EVENT_ERROR",
}

// Get Single TermEvent Actions
export const getTermEventPending = createAction<ITermEventStateContext>(
  TermEventActionEnums.getTermEventPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getTermEventSuccess = createAction<ITermEventStateContext, ITermEvent>(
  TermEventActionEnums.getTermEventSuccess,
  (termEvent: ITermEvent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    termEvent,
  })
);

export const getTermEventError = createAction<ITermEventStateContext>(
  TermEventActionEnums.getTermEventError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Term Actions
export const getByTermPending = createAction<ITermEventStateContext>(
  TermEventActionEnums.getByTermPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByTermSuccess = createAction<
  ITermEventStateContext,
  IListResult<ITermEvent>
>(
  TermEventActionEnums.getByTermSuccess,
  (result: IListResult<ITermEvent>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    termEvents: result.items,
  })
);

export const getByTermError = createAction<ITermEventStateContext>(
  TermEventActionEnums.getByTermError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Date Range Actions
export const getByDateRangePending = createAction<ITermEventStateContext>(
  TermEventActionEnums.getByDateRangePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByDateRangeSuccess = createAction<
  ITermEventStateContext,
  IListResult<ITermEvent>
>(
  TermEventActionEnums.getByDateRangeSuccess,
  (result: IListResult<ITermEvent>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    termEvents: result.items,
  })
);

export const getByDateRangeError = createAction<ITermEventStateContext>(
  TermEventActionEnums.getByDateRangeError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create TermEvent Actions
export const createTermEventPending = createAction<ITermEventStateContext>(
  TermEventActionEnums.createTermEventPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createTermEventSuccess = createAction<ITermEventStateContext, ITermEvent>(
  TermEventActionEnums.createTermEventSuccess,
  (termEvent: ITermEvent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    termEvent,
  })
);

export const createTermEventError = createAction<ITermEventStateContext>(
  TermEventActionEnums.createTermEventError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update TermEvent Actions
export const updateTermEventPending = createAction<ITermEventStateContext>(
  TermEventActionEnums.updateTermEventPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateTermEventSuccess = createAction<ITermEventStateContext, ITermEvent>(
  TermEventActionEnums.updateTermEventSuccess,
  (termEvent: ITermEvent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    termEvent,
  })
);

export const updateTermEventError = createAction<ITermEventStateContext>(
  TermEventActionEnums.updateTermEventError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete TermEvent Actions
export const deleteTermEventPending = createAction<ITermEventStateContext>(
  TermEventActionEnums.deleteTermEventPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteTermEventSuccess = createAction<ITermEventStateContext>(
  TermEventActionEnums.deleteTermEventSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteTermEventError = createAction<ITermEventStateContext>(
  TermEventActionEnums.deleteTermEventError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
