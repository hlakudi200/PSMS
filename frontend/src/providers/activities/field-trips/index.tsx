"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import { buildQueryParams } from "@/utils/query-params";
import {
  INITIAL_STATE,
  FieldTripActionContext,
  FieldTripStateContext,
} from "./context";
import { FieldTripReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getTripPending, getTripSuccess, getTripError,
  getTripsPending, getTripsSuccess, getTripsError,
  createTripPending, createTripSuccess, createTripError,
  updateTripPending, updateTripSuccess, updateTripError,
  submitPending, submitSuccess, submitError,
  approvePending, approveSuccess, approveError,
  rejectPending, rejectSuccess, rejectError,
  cancelPending, cancelSuccess, cancelError,
  completePending, completeSuccess, completeError,
} from "./actions";

export const FieldTripProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(FieldTripReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getTripPending());
    const endpoint = `/api/services/app/FieldTrip/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getTripSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getTripError());
        throw error;
      });
  };

  const getAllAsync = async (input?: Record<string, unknown>) => {
    dispatch(getTripsPending());
    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/FieldTrip/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getTripsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getTripsError());
        throw error;
      });
  };

  const createAsync = async (input: Record<string, unknown>) => {
    dispatch(createTripPending());
    const endpoint = `/api/services/app/FieldTrip/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createTripSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createTripError());
        throw error;
      });
  };

  const updateAsync = async (id: string, input: Record<string, unknown>) => {
    dispatch(updateTripPending());
    const endpoint = `/api/services/app/FieldTrip/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateTripSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateTripError());
        throw error;
      });
  };

  const submitAsync = async (id: string) => {
    dispatch(submitPending());
    const endpoint = `/api/services/app/FieldTrip/Submit?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => { dispatch(submitSuccess()); })
      .catch((error) => { console.error(error); dispatch(submitError()); });
  };

  const approveAsync = async (id: string, budget: number) => {
    dispatch(approvePending());
    const endpoint = `/api/services/app/FieldTrip/Approve`;
    await instance
      .post(endpoint, { id, approvedBudget: budget })
      .then(() => { dispatch(approveSuccess()); })
      .catch((error) => { console.error(error); dispatch(approveError()); });
  };

  const rejectAsync = async (id: string, reason: string) => {
    dispatch(rejectPending());
    const endpoint = `/api/services/app/FieldTrip/Reject`;
    await instance
      .post(endpoint, { id, reason })
      .then(() => { dispatch(rejectSuccess()); })
      .catch((error) => { console.error(error); dispatch(rejectError()); });
  };

  const cancelAsync = async (id: string, reason: string) => {
    dispatch(cancelPending());
    const endpoint = `/api/services/app/FieldTrip/Cancel`;
    await instance
      .post(endpoint, { id, reason })
      .then(() => { dispatch(cancelSuccess()); })
      .catch((error) => { console.error(error); dispatch(cancelError()); });
  };

  const completeAsync = async (id: string) => {
    dispatch(completePending());
    const endpoint = `/api/services/app/FieldTrip/Complete?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => { dispatch(completeSuccess()); })
      .catch((error) => { console.error(error); dispatch(completeError()); });
  };

  return (
    <FieldTripStateContext.Provider value={state}>
      <FieldTripActionContext.Provider
        value={{
          getAsync, getAllAsync, createAsync, updateAsync,
          submitAsync, approveAsync, rejectAsync, cancelAsync, completeAsync,
        }}
      >
        {children}
      </FieldTripActionContext.Provider>
    </FieldTripStateContext.Provider>
  );
};

export const useFieldTripState = () => {
  const context = useContext(FieldTripStateContext);
  if (!context) {
    throw new Error("useFieldTripState must be used within a FieldTripProvider");
  }
  return context;
};

export const useFieldTripActions = () => {
  const context = useContext(FieldTripActionContext);
  if (!context) {
    throw new Error("useFieldTripActions must be used within a FieldTripProvider");
  }
  return context;
};
