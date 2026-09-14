import { handleActions } from "redux-actions";
import { INITIAL_STATE, ITimetableStateContext } from "./context";
import { TimetableActionEnums } from "./actions";

export const TimetableReducer = handleActions<
  ITimetableStateContext,
  ITimetableStateContext
>(
  {
    [TimetableActionEnums.getTimetablesPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.getTimetablesSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.getTimetablesError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.getTimetablePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.getTimetableSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.getTimetableError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.getByClassPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.getByClassSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.getByClassError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.createTimetablePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.createTimetableSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.createTimetableError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.updateTimetablePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.updateTimetableSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.updateTimetableError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.deleteTimetablePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.deleteTimetableSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.deleteTimetableError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.activateTimetablePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.activateTimetableSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.activateTimetableError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.deactivateTimetablePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.deactivateTimetableSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.deactivateTimetableError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.generateTimetablesPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.generateTimetablesSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TimetableActionEnums.generateTimetablesError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
