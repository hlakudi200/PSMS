import { createAction } from "redux-actions";
import { ITimetableStateContext } from "./context";
import {
  ITimetable,
  ITimetableList,
  IPagedResult,
  IGenerateTimetablesResult,
} from "../shared/interfaces";

export enum TimetableActionEnums {
  getTimetablesPending = "GET_TIMETABLES_PENDING",
  getTimetablesSuccess = "GET_TIMETABLES_SUCCESS",
  getTimetablesError = "GET_TIMETABLES_ERROR",

  getTimetablePending = "GET_TIMETABLE_PENDING",
  getTimetableSuccess = "GET_TIMETABLE_SUCCESS",
  getTimetableError = "GET_TIMETABLE_ERROR",

  getByClassPending = "GET_TIMETABLE_BY_CLASS_PENDING",
  getByClassSuccess = "GET_TIMETABLE_BY_CLASS_SUCCESS",
  getByClassError = "GET_TIMETABLE_BY_CLASS_ERROR",

  createTimetablePending = "CREATE_TIMETABLE_PENDING",
  createTimetableSuccess = "CREATE_TIMETABLE_SUCCESS",
  createTimetableError = "CREATE_TIMETABLE_ERROR",

  updateTimetablePending = "UPDATE_TIMETABLE_PENDING",
  updateTimetableSuccess = "UPDATE_TIMETABLE_SUCCESS",
  updateTimetableError = "UPDATE_TIMETABLE_ERROR",

  deleteTimetablePending = "DELETE_TIMETABLE_PENDING",
  deleteTimetableSuccess = "DELETE_TIMETABLE_SUCCESS",
  deleteTimetableError = "DELETE_TIMETABLE_ERROR",

  activateTimetablePending = "ACTIVATE_TIMETABLE_PENDING",
  activateTimetableSuccess = "ACTIVATE_TIMETABLE_SUCCESS",
  activateTimetableError = "ACTIVATE_TIMETABLE_ERROR",

  deactivateTimetablePending = "DEACTIVATE_TIMETABLE_PENDING",
  deactivateTimetableSuccess = "DEACTIVATE_TIMETABLE_SUCCESS",
  deactivateTimetableError = "DEACTIVATE_TIMETABLE_ERROR",

  generateTimetablesPending = "GENERATE_TIMETABLES_PENDING",
  generateTimetablesSuccess = "GENERATE_TIMETABLES_SUCCESS",
  generateTimetablesError = "GENERATE_TIMETABLES_ERROR",
}

// Get All Timetables Actions
export const getTimetablesPending = createAction<ITimetableStateContext>(
  TimetableActionEnums.getTimetablesPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getTimetablesSuccess = createAction<
  ITimetableStateContext,
  IPagedResult<ITimetableList>
>(
  TimetableActionEnums.getTimetablesSuccess,
  (result: IPagedResult<ITimetableList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    timetables: result.items,
    totalCount: result.totalCount,
  })
);

export const getTimetablesError = createAction<ITimetableStateContext>(
  TimetableActionEnums.getTimetablesError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Single Timetable Actions
export const getTimetablePending = createAction<ITimetableStateContext>(
  TimetableActionEnums.getTimetablePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getTimetableSuccess = createAction<ITimetableStateContext, ITimetable>(
  TimetableActionEnums.getTimetableSuccess,
  (timetable: ITimetable) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    timetable,
  })
);

export const getTimetableError = createAction<ITimetableStateContext>(
  TimetableActionEnums.getTimetableError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Class Actions
export const getByClassPending = createAction<ITimetableStateContext>(
  TimetableActionEnums.getByClassPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByClassSuccess = createAction<ITimetableStateContext, ITimetable>(
  TimetableActionEnums.getByClassSuccess,
  (timetable: ITimetable) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    timetable,
  })
);

export const getByClassError = createAction<ITimetableStateContext>(
  TimetableActionEnums.getByClassError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Timetable Actions
export const createTimetablePending = createAction<ITimetableStateContext>(
  TimetableActionEnums.createTimetablePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createTimetableSuccess = createAction<ITimetableStateContext, ITimetable>(
  TimetableActionEnums.createTimetableSuccess,
  (timetable: ITimetable) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    timetable,
  })
);

export const createTimetableError = createAction<ITimetableStateContext>(
  TimetableActionEnums.createTimetableError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Timetable Actions
export const updateTimetablePending = createAction<ITimetableStateContext>(
  TimetableActionEnums.updateTimetablePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateTimetableSuccess = createAction<ITimetableStateContext, ITimetable>(
  TimetableActionEnums.updateTimetableSuccess,
  (timetable: ITimetable) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    timetable,
  })
);

export const updateTimetableError = createAction<ITimetableStateContext>(
  TimetableActionEnums.updateTimetableError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Timetable Actions
export const deleteTimetablePending = createAction<ITimetableStateContext>(
  TimetableActionEnums.deleteTimetablePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteTimetableSuccess = createAction<ITimetableStateContext>(
  TimetableActionEnums.deleteTimetableSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteTimetableError = createAction<ITimetableStateContext>(
  TimetableActionEnums.deleteTimetableError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Activate Timetable Actions
export const activateTimetablePending = createAction<ITimetableStateContext>(
  TimetableActionEnums.activateTimetablePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const activateTimetableSuccess = createAction<ITimetableStateContext, ITimetable>(
  TimetableActionEnums.activateTimetableSuccess,
  (timetable: ITimetable) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    timetable,
  })
);

export const activateTimetableError = createAction<ITimetableStateContext>(
  TimetableActionEnums.activateTimetableError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Deactivate Timetable Actions
export const deactivateTimetablePending = createAction<ITimetableStateContext>(
  TimetableActionEnums.deactivateTimetablePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deactivateTimetableSuccess = createAction<ITimetableStateContext, ITimetable>(
  TimetableActionEnums.deactivateTimetableSuccess,
  (timetable: ITimetable) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    timetable,
  })
);

export const deactivateTimetableError = createAction<ITimetableStateContext>(
  TimetableActionEnums.deactivateTimetableError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Generate Timetables Actions
export const generateTimetablesPending = createAction<ITimetableStateContext>(
  TimetableActionEnums.generateTimetablesPending,
  () => ({ isPending: false, isSuccess: false, isError: false, isGenerating: true })
);

export const generateTimetablesSuccess = createAction<
  ITimetableStateContext,
  IGenerateTimetablesResult
>(
  TimetableActionEnums.generateTimetablesSuccess,
  (result: IGenerateTimetablesResult) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    isGenerating: false,
    generationResult: result,
  })
);

export const generateTimetablesError = createAction<ITimetableStateContext>(
  TimetableActionEnums.generateTimetablesError,
  () => ({ isPending: false, isSuccess: false, isError: true, isGenerating: false })
);
