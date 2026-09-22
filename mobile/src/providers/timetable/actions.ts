import { createAction } from "redux-actions";
import type { ITimetableSlot, ITimetableStateContext } from "./context";

type TimetablePatch = Partial<ITimetableStateContext>;

export enum TimetableActionEnums {
  getPending = "TIMETABLE_GET_PENDING",
  getSuccess = "TIMETABLE_GET_SUCCESS",
  getError = "TIMETABLE_GET_ERROR",
}

export const getPending = createAction<TimetablePatch>(TimetableActionEnums.getPending, () => ({
  isPending: true, isError: false,
}));

export const getSuccess = createAction<TimetablePatch, { timetableId?: string; slots: ITimetableSlot[] }>(
  TimetableActionEnums.getSuccess,
  ({ timetableId, slots }) => ({ isPending: false, isError: false, timetableId, slots })
);

export const getError = createAction<TimetablePatch>(TimetableActionEnums.getError, () => ({
  isPending: false, isError: true,
}));
