"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import { buildQueryParams } from "@/utils/query-params";
import {
  INITIAL_STATE,
  StaffLeaveRequestActionContext,
  StaffLeaveRequestStateContext,
} from "./context";
import { StaffLeaveReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getLeavePending, getLeaveSuccess, getLeaveError,
  getLeavesPending, getLeavesSuccess, getLeavesError,
  createLeavePending, createLeaveSuccess, createLeaveError,
  updateLeavePending, updateLeaveSuccess, updateLeaveError,
  submitPending, submitSuccess, submitError,
  approvePending, approveSuccess, approveError,
  rejectPending, rejectSuccess, rejectError,
  cancelPending, cancelSuccess, cancelError,
} from "./actions";

export const StaffLeaveRequestProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(StaffLeaveReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getLeavePending());
    const endpoint = `/api/services/app/StaffLeaveRequest/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getLeaveSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getLeaveError());
      });
  };

  const getAllAsync = async (input?: Record<string, unknown>) => {
    dispatch(getLeavesPending());
    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/StaffLeaveRequest/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getLeavesSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getLeavesError());
      });
  };

  const createAsync = async (input: Record<string, unknown>) => {
    dispatch(createLeavePending());
    const endpoint = `/api/services/app/StaffLeaveRequest/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createLeaveSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createLeaveError());
      });
  };

  const updateAsync = async (id: string, input: Record<string, unknown>) => {
    dispatch(updateLeavePending());
    const endpoint = `/api/services/app/StaffLeaveRequest/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateLeaveSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateLeaveError());
      });
  };

  const submitAsync = async (id: string) => {
    dispatch(submitPending());
    const endpoint = `/api/services/app/StaffLeaveRequest/Submit?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => { dispatch(submitSuccess()); })
      .catch((error) => { console.error(error); dispatch(submitError()); });
  };

  const approveAsync = async (id: string) => {
    dispatch(approvePending());
    const endpoint = `/api/services/app/StaffLeaveRequest/Approve?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => { dispatch(approveSuccess()); })
      .catch((error) => { console.error(error); dispatch(approveError()); });
  };

  const rejectAsync = async (id: string, reason: string) => {
    dispatch(rejectPending());
    const endpoint = `/api/services/app/StaffLeaveRequest/Reject`;
    await instance
      .post(endpoint, { id, reason })
      .then(() => { dispatch(rejectSuccess()); })
      .catch((error) => { console.error(error); dispatch(rejectError()); });
  };

  const cancelAsync = async (id: string) => {
    dispatch(cancelPending());
    const endpoint = `/api/services/app/StaffLeaveRequest/Cancel?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => { dispatch(cancelSuccess()); })
      .catch((error) => { console.error(error); dispatch(cancelError()); });
  };

  return (
    <StaffLeaveRequestStateContext.Provider value={state}>
      <StaffLeaveRequestActionContext.Provider
        value={{
          getAsync, getAllAsync, createAsync, updateAsync,
          submitAsync, approveAsync, rejectAsync, cancelAsync,
        }}
      >
        {children}
      </StaffLeaveRequestActionContext.Provider>
    </StaffLeaveRequestStateContext.Provider>
  );
};

export const useStaffLeaveRequestState = () => {
  const context = useContext(StaffLeaveRequestStateContext);
  if (!context) {
    throw new Error("useStaffLeaveRequestState must be used within a StaffLeaveRequestProvider");
  }
  return context;
};

export const useStaffLeaveRequestActions = () => {
  const context = useContext(StaffLeaveRequestActionContext);
  if (!context) {
    throw new Error("useStaffLeaveRequestActions must be used within a StaffLeaveRequestProvider");
  }
  return context;
};
