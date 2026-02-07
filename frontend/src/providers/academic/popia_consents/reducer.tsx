import { handleActions } from "redux-actions";
import { INITIAL_STATE, IPOPIAConsentStateContext } from "./context";
import { POPIAConsentActionEnums } from "./actions";

export const POPIAConsentReducer = handleActions<
  IPOPIAConsentStateContext,
  IPOPIAConsentStateContext
>(
  {
    [POPIAConsentActionEnums.getByStudentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [POPIAConsentActionEnums.getByStudentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [POPIAConsentActionEnums.getByStudentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [POPIAConsentActionEnums.getAllPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [POPIAConsentActionEnums.getAllSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [POPIAConsentActionEnums.getAllError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [POPIAConsentActionEnums.createPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [POPIAConsentActionEnums.createSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [POPIAConsentActionEnums.createError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [POPIAConsentActionEnums.updatePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [POPIAConsentActionEnums.updateSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [POPIAConsentActionEnums.updateError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [POPIAConsentActionEnums.revokePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [POPIAConsentActionEnums.revokeSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [POPIAConsentActionEnums.revokeError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
