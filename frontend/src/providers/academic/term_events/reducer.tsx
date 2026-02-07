import { handleActions } from "redux-actions";
import { INITIAL_STATE, ITermEventStateContext } from "./context";
import { TermEventActionEnums } from "./actions";

export const TermEventReducer = handleActions<
  ITermEventStateContext,
  ITermEventStateContext
>(
  {
    [TermEventActionEnums.getTermEventPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.getTermEventSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.getTermEventError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.getByTermPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.getByTermSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.getByTermError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.getByDateRangePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.getByDateRangeSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.getByDateRangeError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.createTermEventPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.createTermEventSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.createTermEventError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.updateTermEventPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.updateTermEventSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.updateTermEventError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.deleteTermEventPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.deleteTermEventSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermEventActionEnums.deleteTermEventError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
