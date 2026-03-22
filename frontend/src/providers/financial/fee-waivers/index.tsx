"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import { buildQueryParams } from "@/utils/query-params";
import {
  INITIAL_STATE,
  FeeWaiverActionContext,
  FeeWaiverStateContext,
} from "./context";
import { FeeWaiverReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getFeeWaiverPending,
  getFeeWaiverSuccess,
  getFeeWaiverError,
  getFeeWaiversPending,
  getFeeWaiversSuccess,
  getFeeWaiversError,
  createFeeWaiverPending,
  createFeeWaiverSuccess,
  createFeeWaiverError,
  updateFeeWaiverPending,
  updateFeeWaiverSuccess,
  updateFeeWaiverError,
  deleteFeeWaiverPending,
  deleteFeeWaiverSuccess,
  deleteFeeWaiverError,
  submitPending,
  submitSuccess,
  submitError,
  approvePending,
  approveSuccess,
  approveError,
  rejectPending,
  rejectSuccess,
  rejectError,
} from "./actions";

export const FeeWaiverProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(FeeWaiverReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getFeeWaiverPending());
    const endpoint = `/api/services/app/FeeWaiver/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getFeeWaiverSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getFeeWaiverError());
      });
  };

  const getAllAsync = async (input?: Record<string, unknown>) => {
    dispatch(getFeeWaiversPending());
    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/FeeWaiver/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getFeeWaiversSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getFeeWaiversError());
      });
  };

  const createAsync = async (input: Record<string, unknown>) => {
    dispatch(createFeeWaiverPending());
    const endpoint = `/api/services/app/FeeWaiver/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createFeeWaiverSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createFeeWaiverError());
      });
  };

  const updateAsync = async (id: string, input: Record<string, unknown>) => {
    dispatch(updateFeeWaiverPending());
    const endpoint = `/api/services/app/FeeWaiver/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateFeeWaiverSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateFeeWaiverError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteFeeWaiverPending());
    const endpoint = `/api/services/app/FeeWaiver/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteFeeWaiverSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteFeeWaiverError());
      });
  };

  const submitAsync = async (id: string) => {
    dispatch(submitPending());
    const endpoint = `/api/services/app/FeeWaiver/Submit?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(submitSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(submitError());
      });
  };

  const approveAsync = async (id: string, amount: number, notes: string) => {
    dispatch(approvePending());
    const endpoint = `/api/services/app/FeeWaiver/Approve`;
    await instance
      .post(endpoint, { id, approvedAmount: amount, notes })
      .then(() => {
        dispatch(approveSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(approveError());
      });
  };

  const rejectAsync = async (id: string, notes: string) => {
    dispatch(rejectPending());
    const endpoint = `/api/services/app/FeeWaiver/Reject`;
    await instance
      .post(endpoint, { id, notes })
      .then(() => {
        dispatch(rejectSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(rejectError());
      });
  };

  return (
    <FeeWaiverStateContext.Provider value={state}>
      <FeeWaiverActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          submitAsync,
          approveAsync,
          rejectAsync,
        }}
      >
        {children}
      </FeeWaiverActionContext.Provider>
    </FeeWaiverStateContext.Provider>
  );
};

export const useFeeWaiverState = () => {
  const context = useContext(FeeWaiverStateContext);
  if (!context) {
    throw new Error("useFeeWaiverState must be used within a FeeWaiverProvider");
  }
  return context;
};

export const useFeeWaiverActions = () => {
  const context = useContext(FeeWaiverActionContext);
  if (!context) {
    throw new Error("useFeeWaiverActions must be used within a FeeWaiverProvider");
  }
  return context;
};
