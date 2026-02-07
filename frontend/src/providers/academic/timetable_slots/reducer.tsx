import { handleActions } from "redux-actions";
import { INITIAL_STATE, ITimetableSlotStateContext } from "./context";
import { TimetableSlotActionEnums } from "./actions";

export const TimetableSlotReducer = handleActions<
  ITimetableSlotStateContext,
  ITimetableSlotStateContext
>(
  {
    [TimetableSlotActionEnums.getTimetableSlotPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.getTimetableSlotSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.getTimetableSlotError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.getByTimetablePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.getByTimetableSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.getByTimetableError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.getByDayPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.getByDaySuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.getByDayError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.getByTeacherPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.getByTeacherSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.getByTeacherError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.createTimetableSlotPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.createTimetableSlotSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.createTimetableSlotError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.updateTimetableSlotPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.updateTimetableSlotSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.updateTimetableSlotError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.deleteTimetableSlotPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.deleteTimetableSlotSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.deleteTimetableSlotError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.bulkCreateTimetableSlotPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.bulkCreateTimetableSlotSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableSlotActionEnums.bulkCreateTimetableSlotError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
