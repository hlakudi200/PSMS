import { createAction } from "redux-actions";
import { ITimetableSlotStateContext } from "./context";
import { ITimetableSlot, ITimetableSlotList, IListResult } from "../shared/interfaces";

export enum TimetableSlotActionEnums {
  getTimetableSlotPending = "GET_TIMETABLE_SLOT_PENDING",
  getTimetableSlotSuccess = "GET_TIMETABLE_SLOT_SUCCESS",
  getTimetableSlotError = "GET_TIMETABLE_SLOT_ERROR",

  getByTimetablePending = "GET_TIMETABLE_SLOTS_BY_TIMETABLE_PENDING",
  getByTimetableSuccess = "GET_TIMETABLE_SLOTS_BY_TIMETABLE_SUCCESS",
  getByTimetableError = "GET_TIMETABLE_SLOTS_BY_TIMETABLE_ERROR",

  getByDayPending = "GET_TIMETABLE_SLOTS_BY_DAY_PENDING",
  getByDaySuccess = "GET_TIMETABLE_SLOTS_BY_DAY_SUCCESS",
  getByDayError = "GET_TIMETABLE_SLOTS_BY_DAY_ERROR",

  getByTeacherPending = "GET_TIMETABLE_SLOTS_BY_TEACHER_PENDING",
  getByTeacherSuccess = "GET_TIMETABLE_SLOTS_BY_TEACHER_SUCCESS",
  getByTeacherError = "GET_TIMETABLE_SLOTS_BY_TEACHER_ERROR",

  createTimetableSlotPending = "CREATE_TIMETABLE_SLOT_PENDING",
  createTimetableSlotSuccess = "CREATE_TIMETABLE_SLOT_SUCCESS",
  createTimetableSlotError = "CREATE_TIMETABLE_SLOT_ERROR",

  updateTimetableSlotPending = "UPDATE_TIMETABLE_SLOT_PENDING",
  updateTimetableSlotSuccess = "UPDATE_TIMETABLE_SLOT_SUCCESS",
  updateTimetableSlotError = "UPDATE_TIMETABLE_SLOT_ERROR",

  deleteTimetableSlotPending = "DELETE_TIMETABLE_SLOT_PENDING",
  deleteTimetableSlotSuccess = "DELETE_TIMETABLE_SLOT_SUCCESS",
  deleteTimetableSlotError = "DELETE_TIMETABLE_SLOT_ERROR",

  bulkCreateTimetableSlotPending = "BULK_CREATE_TIMETABLE_SLOT_PENDING",
  bulkCreateTimetableSlotSuccess = "BULK_CREATE_TIMETABLE_SLOT_SUCCESS",
  bulkCreateTimetableSlotError = "BULK_CREATE_TIMETABLE_SLOT_ERROR",
}

// Get Single TimetableSlot Actions
export const getTimetableSlotPending = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.getTimetableSlotPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getTimetableSlotSuccess = createAction<ITimetableSlotStateContext, ITimetableSlot>(
  TimetableSlotActionEnums.getTimetableSlotSuccess,
  (timetableSlot: ITimetableSlot) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    timetableSlot,
  })
);

export const getTimetableSlotError = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.getTimetableSlotError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Timetable Actions
export const getByTimetablePending = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.getByTimetablePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByTimetableSuccess = createAction<
  ITimetableSlotStateContext,
  IListResult<ITimetableSlotList>
>(
  TimetableSlotActionEnums.getByTimetableSuccess,
  (result: IListResult<ITimetableSlotList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    timetableSlots: result.items,
  })
);

export const getByTimetableError = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.getByTimetableError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Day Actions
export const getByDayPending = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.getByDayPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByDaySuccess = createAction<
  ITimetableSlotStateContext,
  IListResult<ITimetableSlotList>
>(
  TimetableSlotActionEnums.getByDaySuccess,
  (result: IListResult<ITimetableSlotList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    timetableSlots: result.items,
  })
);

export const getByDayError = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.getByDayError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Teacher Actions
export const getByTeacherPending = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.getByTeacherPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByTeacherSuccess = createAction<
  ITimetableSlotStateContext,
  IListResult<ITimetableSlotList>
>(
  TimetableSlotActionEnums.getByTeacherSuccess,
  (result: IListResult<ITimetableSlotList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    timetableSlots: result.items,
  })
);

export const getByTeacherError = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.getByTeacherError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create TimetableSlot Actions
export const createTimetableSlotPending = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.createTimetableSlotPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createTimetableSlotSuccess = createAction<ITimetableSlotStateContext, ITimetableSlot>(
  TimetableSlotActionEnums.createTimetableSlotSuccess,
  (timetableSlot: ITimetableSlot) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    timetableSlot,
  })
);

export const createTimetableSlotError = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.createTimetableSlotError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update TimetableSlot Actions
export const updateTimetableSlotPending = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.updateTimetableSlotPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateTimetableSlotSuccess = createAction<ITimetableSlotStateContext, ITimetableSlot>(
  TimetableSlotActionEnums.updateTimetableSlotSuccess,
  (timetableSlot: ITimetableSlot) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    timetableSlot,
  })
);

export const updateTimetableSlotError = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.updateTimetableSlotError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete TimetableSlot Actions
export const deleteTimetableSlotPending = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.deleteTimetableSlotPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteTimetableSlotSuccess = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.deleteTimetableSlotSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteTimetableSlotError = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.deleteTimetableSlotError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Bulk Create TimetableSlot Actions
export const bulkCreateTimetableSlotPending = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.bulkCreateTimetableSlotPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const bulkCreateTimetableSlotSuccess = createAction<
  ITimetableSlotStateContext,
  IListResult<ITimetableSlot>
>(
  TimetableSlotActionEnums.bulkCreateTimetableSlotSuccess,
  (result: IListResult<ITimetableSlot>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const bulkCreateTimetableSlotError = createAction<ITimetableSlotStateContext>(
  TimetableSlotActionEnums.bulkCreateTimetableSlotError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
