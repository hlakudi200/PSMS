import { handleActions } from "redux-actions";
import { INITIAL_STATE, IStudentExtramuralStateContext } from "./context";
import { StudentExtramuralActionEnums } from "./actions";

export const StudentExtramuralReducer = handleActions<
  IStudentExtramuralStateContext,
  IStudentExtramuralStateContext
>(
  {
    [StudentExtramuralActionEnums.getStudentExtramuralPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.getStudentExtramuralSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.getStudentExtramuralError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.getStudentExtramuralsPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.getStudentExtramuralsSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.getStudentExtramuralsError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.getByActivityPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.getByActivitySuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.getByActivityError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.getByStudentPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.getByStudentSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.getByStudentError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.createStudentExtramuralPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.createStudentExtramuralSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.createStudentExtramuralError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.updateStudentExtramuralPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.updateStudentExtramuralSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.updateStudentExtramuralError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.deleteStudentExtramuralPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.deleteStudentExtramuralSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.deleteStudentExtramuralError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.suspendPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.suspendSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.suspendError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.reactivatePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.reactivateSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.reactivateError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.terminatePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.terminateSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.terminateError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.signConsentFormPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.signConsentFormSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentExtramuralActionEnums.signConsentFormError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
