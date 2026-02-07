import { handleActions } from "redux-actions";
import { INITIAL_STATE, ITermStateContext } from "./context";
import { TermActionEnums } from "./actions";

export const TermReducer = handleActions<ITermStateContext, ITermStateContext>(
  {
    [TermActionEnums.getTermsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.getTermsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.getTermsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.getTermPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.getTermSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.getTermError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.getCurrentTermPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.getCurrentTermSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.getCurrentTermError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.createTermPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.createTermSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.createTermError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.updateTermPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.updateTermSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.updateTermError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.deleteTermPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.deleteTermSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.deleteTermError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.setAsCurrentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.setAsCurrentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TermActionEnums.setAsCurrentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
