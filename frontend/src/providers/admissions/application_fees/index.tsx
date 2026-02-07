"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  ApplicationFeeActionContext,
  ApplicationFeeStateContext,
} from "./context";
import {
  IRecordPayment,
  IPaymentCallback,
} from "../shared/interfaces";
import { ApplicationFeeReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getByApplicationPending,
  getByApplicationSuccess,
  getByApplicationError,
  recordPaymentPending,
  recordPaymentSuccess,
  recordPaymentError,
  processPaymentCallbackPending,
  processPaymentCallbackSuccess,
  processPaymentCallbackError,
  getPaymentStatusPending,
  getPaymentStatusSuccess,
  getPaymentStatusError,
} from "./actions";

export const ApplicationFeeProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(ApplicationFeeReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getByApplicationAsync = async (applicationId: string) => {
    dispatch(getByApplicationPending());
    const endpoint = `/api/services/app/ApplicationFee/GetByApplication?applicationId=${applicationId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByApplicationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByApplicationError());
      });
  };

  const recordPaymentAsync = async (applicationId: string, input: IRecordPayment) => {
    dispatch(recordPaymentPending());
    const endpoint = `/api/services/app/ApplicationFee/RecordPayment?applicationId=${applicationId}`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(recordPaymentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(recordPaymentError());
      });
  };

  const processPaymentCallbackAsync = async (input: IPaymentCallback) => {
    dispatch(processPaymentCallbackPending());
    const endpoint = `/api/services/app/ApplicationFee/ProcessPaymentCallback`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(processPaymentCallbackSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(processPaymentCallbackError());
      });
  };

  const getPaymentStatusAsync = async (applicationId: string) => {
    dispatch(getPaymentStatusPending());
    const endpoint = `/api/services/app/ApplicationFee/GetPaymentStatus?applicationId=${applicationId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getPaymentStatusSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getPaymentStatusError());
      });
  };

  return (
    <ApplicationFeeStateContext.Provider value={state}>
      <ApplicationFeeActionContext.Provider
        value={{
          getByApplicationAsync,
          recordPaymentAsync,
          processPaymentCallbackAsync,
          getPaymentStatusAsync,
        }}
      >
        {children}
      </ApplicationFeeActionContext.Provider>
    </ApplicationFeeStateContext.Provider>
  );
};

export const useApplicationFeeState = () => {
  const context = useContext(ApplicationFeeStateContext);
  if (!context) {
    throw new Error("useApplicationFeeState must be used within an ApplicationFeeProvider");
  }
  return context;
};

export const useApplicationFeeActions = () => {
  const context = useContext(ApplicationFeeActionContext);
  if (!context) {
    throw new Error("useApplicationFeeActions must be used within an ApplicationFeeProvider");
  }
  return context;
};
