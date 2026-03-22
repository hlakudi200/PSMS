import { handleActions } from "redux-actions";
import { INITIAL_STATE, IFieldTripStateContext } from "./context";
import { FieldTripActionEnums } from "./actions";

export const FieldTripReducer = handleActions<
  IFieldTripStateContext,
  IFieldTripStateContext
>(
  {
    [FieldTripActionEnums.getTripPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.getTripSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.getTripError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.getTripsPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.getTripsSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.getTripsError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.createTripPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.createTripSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.createTripError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.updateTripPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.updateTripSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.updateTripError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.submitPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.submitSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.submitError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.approvePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.approveSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.approveError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.rejectPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.rejectSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.rejectError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.cancelPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.cancelSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.cancelError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.completePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.completeSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [FieldTripActionEnums.completeError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
