import { createAction } from "redux-actions";
import { IFieldTripStateContext, IFieldTrip } from "./context";

export enum FieldTripActionEnums {
  getTripPending = "GET_FIELD_TRIP_PENDING",
  getTripSuccess = "GET_FIELD_TRIP_SUCCESS",
  getTripError = "GET_FIELD_TRIP_ERROR",

  getTripsPending = "GET_FIELD_TRIPS_PENDING",
  getTripsSuccess = "GET_FIELD_TRIPS_SUCCESS",
  getTripsError = "GET_FIELD_TRIPS_ERROR",

  createTripPending = "CREATE_FIELD_TRIP_PENDING",
  createTripSuccess = "CREATE_FIELD_TRIP_SUCCESS",
  createTripError = "CREATE_FIELD_TRIP_ERROR",

  updateTripPending = "UPDATE_FIELD_TRIP_PENDING",
  updateTripSuccess = "UPDATE_FIELD_TRIP_SUCCESS",
  updateTripError = "UPDATE_FIELD_TRIP_ERROR",

  submitPending = "SUBMIT_FIELD_TRIP_PENDING",
  submitSuccess = "SUBMIT_FIELD_TRIP_SUCCESS",
  submitError = "SUBMIT_FIELD_TRIP_ERROR",

  approvePending = "APPROVE_FIELD_TRIP_PENDING",
  approveSuccess = "APPROVE_FIELD_TRIP_SUCCESS",
  approveError = "APPROVE_FIELD_TRIP_ERROR",

  rejectPending = "REJECT_FIELD_TRIP_PENDING",
  rejectSuccess = "REJECT_FIELD_TRIP_SUCCESS",
  rejectError = "REJECT_FIELD_TRIP_ERROR",

  cancelPending = "CANCEL_FIELD_TRIP_PENDING",
  cancelSuccess = "CANCEL_FIELD_TRIP_SUCCESS",
  cancelError = "CANCEL_FIELD_TRIP_ERROR",

  completePending = "COMPLETE_FIELD_TRIP_PENDING",
  completeSuccess = "COMPLETE_FIELD_TRIP_SUCCESS",
  completeError = "COMPLETE_FIELD_TRIP_ERROR",
}

// Get Single
export const getTripPending = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.getTripPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getTripSuccess = createAction<IFieldTripStateContext, IFieldTrip>(
  FieldTripActionEnums.getTripSuccess,
  (fieldTrip: IFieldTrip) => ({
    isPending: false, isSuccess: true, isError: false, fieldTrip,
  })
);
export const getTripError = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.getTripError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All
export const getTripsPending = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.getTripsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getTripsSuccess = createAction<IFieldTripStateContext, { items: IFieldTrip[]; totalCount: number }>(
  FieldTripActionEnums.getTripsSuccess,
  (result) => ({
    isPending: false, isSuccess: true, isError: false,
    fieldTrips: result.items, totalCount: result.totalCount,
  })
);
export const getTripsError = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.getTripsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create
export const createTripPending = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.createTripPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const createTripSuccess = createAction<IFieldTripStateContext, IFieldTrip>(
  FieldTripActionEnums.createTripSuccess,
  (fieldTrip: IFieldTrip) => ({
    isPending: false, isSuccess: true, isError: false, fieldTrip,
  })
);
export const createTripError = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.createTripError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update
export const updateTripPending = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.updateTripPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const updateTripSuccess = createAction<IFieldTripStateContext, IFieldTrip>(
  FieldTripActionEnums.updateTripSuccess,
  (fieldTrip: IFieldTrip) => ({
    isPending: false, isSuccess: true, isError: false, fieldTrip,
  })
);
export const updateTripError = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.updateTripError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Submit
export const submitPending = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.submitPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const submitSuccess = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.submitSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const submitError = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.submitError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Approve
export const approvePending = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.approvePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const approveSuccess = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.approveSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const approveError = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.approveError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Reject
export const rejectPending = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.rejectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const rejectSuccess = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.rejectSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const rejectError = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.rejectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Cancel
export const cancelPending = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.cancelPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const cancelSuccess = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.cancelSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const cancelError = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.cancelError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Complete
export const completePending = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.completePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const completeSuccess = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.completeSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const completeError = createAction<IFieldTripStateContext>(
  FieldTripActionEnums.completeError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
