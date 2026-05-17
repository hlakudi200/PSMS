"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import { buildQueryParams } from "@/utils/query-params";
import {
  INITIAL_STATE,
  StudentTransferActionContext,
  StudentTransferStateContext,
} from "./context";
import { StudentTransferReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getTransferPending, getTransferSuccess, getTransferError,
  getTransfersPending, getTransfersSuccess, getTransfersError,
  createTransferPending, createTransferSuccess, createTransferError,
  updateTransferPending, updateTransferSuccess, updateTransferError,
  submitPending, submitSuccess, submitError,
  approvePending, approveSuccess, approveError,
  rejectPending, rejectSuccess, rejectError,
  completePending, completeSuccess, completeError,
  cancelPending, cancelSuccess, cancelError,
} from "./actions";

export const StudentTransferProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(StudentTransferReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getTransferPending());
    const endpoint = `/api/services/app/StudentTransfer/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getTransferSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getTransferError());
        throw error;
      });
  };

  const getAllAsync = async (input?: Record<string, unknown>) => {
    dispatch(getTransfersPending());
    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/StudentTransfer/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getTransfersSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getTransfersError());
        throw error;
      });
  };

  const createAsync = async (input: Record<string, unknown>) => {
    dispatch(createTransferPending());
    const endpoint = `/api/services/app/StudentTransfer/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createTransferSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createTransferError());
        throw error;
      });
  };

  const updateAsync = async (id: string, input: Record<string, unknown>) => {
    dispatch(updateTransferPending());
    const endpoint = `/api/services/app/StudentTransfer/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateTransferSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateTransferError());
        throw error;
      });
  };

  const submitAsync = async (id: string) => {
    dispatch(submitPending());
    const endpoint = `/api/services/app/StudentTransfer/Submit?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => { dispatch(submitSuccess()); })
      .catch((error) => { console.error(error); dispatch(submitError()); });
  };

  const approveAsync = async (id: string) => {
    dispatch(approvePending());
    const endpoint = `/api/services/app/StudentTransfer/Approve?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => { dispatch(approveSuccess()); })
      .catch((error) => { console.error(error); dispatch(approveError()); });
  };

  const rejectAsync = async (id: string, notes: string) => {
    dispatch(rejectPending());
    const endpoint = `/api/services/app/StudentTransfer/Reject`;
    await instance
      .post(endpoint, { id, notes })
      .then(() => { dispatch(rejectSuccess()); })
      .catch((error) => { console.error(error); dispatch(rejectError()); });
  };

  const completeAsync = async (id: string, certificateUrl?: string) => {
    dispatch(completePending());
    const endpoint = `/api/services/app/StudentTransfer/Complete`;
    await instance
      .post(endpoint, { id, certificateUrl })
      .then(() => { dispatch(completeSuccess()); })
      .catch((error) => { console.error(error); dispatch(completeError()); });
  };

  const cancelAsync = async (id: string) => {
    dispatch(cancelPending());
    const endpoint = `/api/services/app/StudentTransfer/Cancel?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => { dispatch(cancelSuccess()); })
      .catch((error) => { console.error(error); dispatch(cancelError()); });
  };

  return (
    <StudentTransferStateContext.Provider value={state}>
      <StudentTransferActionContext.Provider
        value={{
          getAsync, getAllAsync, createAsync, updateAsync,
          submitAsync, approveAsync, rejectAsync, completeAsync, cancelAsync,
        }}
      >
        {children}
      </StudentTransferActionContext.Provider>
    </StudentTransferStateContext.Provider>
  );
};

export const useStudentTransferState = () => {
  const context = useContext(StudentTransferStateContext);
  if (!context) {
    throw new Error("useStudentTransferState must be used within a StudentTransferProvider");
  }
  return context;
};

export const useStudentTransferActions = () => {
  const context = useContext(StudentTransferActionContext);
  if (!context) {
    throw new Error("useStudentTransferActions must be used within a StudentTransferProvider");
  }
  return context;
};
