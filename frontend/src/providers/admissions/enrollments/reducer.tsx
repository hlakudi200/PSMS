import { handleActions } from "redux-actions";
import { INITIAL_STATE, IEnrollmentStateContext } from "./context";
import { EnrollmentActionEnums } from "./actions";

export const EnrollmentReducer = handleActions<
  IEnrollmentStateContext,
  IEnrollmentStateContext
>(
  {
    [EnrollmentActionEnums.getByApplicationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.getByApplicationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.getByApplicationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.getPendingEnrollmentsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.getPendingEnrollmentsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.getPendingEnrollmentsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.getAvailableClassesPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.getAvailableClassesSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.getAvailableClassesError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.acceptOfferPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.acceptOfferSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.acceptOfferError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.assignClassPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.assignClassSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.assignClassError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.completeEnrollmentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.completeEnrollmentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EnrollmentActionEnums.completeEnrollmentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
