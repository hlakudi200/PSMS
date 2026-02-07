import { handleActions } from "redux-actions";
import { INITIAL_STATE, IApplicantParentStateContext } from "./context";
import { ApplicantParentActionEnums } from "./actions";

export const ApplicantParentReducer = handleActions<
  IApplicantParentStateContext,
  IApplicantParentStateContext
>(
  {
    [ApplicantParentActionEnums.getApplicantParentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.getApplicantParentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.getApplicantParentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.getAllByApplicationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.getAllByApplicationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.getAllByApplicationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.createApplicantParentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.createApplicantParentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.createApplicantParentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.updateApplicantParentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.updateApplicantParentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.updateApplicantParentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.deleteApplicantParentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.deleteApplicantParentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.deleteApplicantParentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.setAsPrimaryContactPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.setAsPrimaryContactSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.setAsPrimaryContactError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.setAsFinanciallyResponsiblePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.setAsFinanciallyResponsibleSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicantParentActionEnums.setAsFinanciallyResponsibleError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
