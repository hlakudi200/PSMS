"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import { buildQueryParams } from "@/utils/query-params";
import {
  INITIAL_STATE,
  ExpenseRequestActionContext,
  ExpenseRequestStateContext,
} from "./context";
import { ExpenseRequestReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getExpensePending, getExpenseSuccess, getExpenseError,
  getExpensesPending, getExpensesSuccess, getExpensesError,
  createExpensePending, createExpenseSuccess, createExpenseError,
  updateExpensePending, updateExpenseSuccess, updateExpenseError,
  submitPending, submitSuccess, submitError,
  approvePending, approveSuccess, approveError,
  rejectPending, rejectSuccess, rejectError,
  cancelPending, cancelSuccess, cancelError,
  markAsPaidPending, markAsPaidSuccess, markAsPaidError,
} from "./actions";

export const ExpenseRequestProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(ExpenseRequestReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getExpensePending());
    const endpoint = `/api/services/app/ExpenseRequest/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getExpenseSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getExpenseError());
      });
  };

  const getAllAsync = async (input?: Record<string, unknown>) => {
    dispatch(getExpensesPending());
    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/ExpenseRequest/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getExpensesSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getExpensesError());
      });
  };

  const createAsync = async (input: Record<string, unknown>) => {
    dispatch(createExpensePending());
    const endpoint = `/api/services/app/ExpenseRequest/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createExpenseSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createExpenseError());
      });
  };

  const updateAsync = async (id: string, input: Record<string, unknown>) => {
    dispatch(updateExpensePending());
    const endpoint = `/api/services/app/ExpenseRequest/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateExpenseSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateExpenseError());
      });
  };

  const submitAsync = async (id: string) => {
    dispatch(submitPending());
    const endpoint = `/api/services/app/ExpenseRequest/Submit?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => { dispatch(submitSuccess()); })
      .catch((error) => { console.error(error); dispatch(submitError()); });
  };

  const approveAsync = async (id: string, amount: number) => {
    dispatch(approvePending());
    const endpoint = `/api/services/app/ExpenseRequest/Approve`;
    await instance
      .post(endpoint, { id, approvedAmount: amount })
      .then(() => { dispatch(approveSuccess()); })
      .catch((error) => { console.error(error); dispatch(approveError()); });
  };

  const rejectAsync = async (id: string, reason: string) => {
    dispatch(rejectPending());
    const endpoint = `/api/services/app/ExpenseRequest/Reject`;
    await instance
      .post(endpoint, { id, reason })
      .then(() => { dispatch(rejectSuccess()); })
      .catch((error) => { console.error(error); dispatch(rejectError()); });
  };

  const cancelAsync = async (id: string) => {
    dispatch(cancelPending());
    const endpoint = `/api/services/app/ExpenseRequest/Cancel?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => { dispatch(cancelSuccess()); })
      .catch((error) => { console.error(error); dispatch(cancelError()); });
  };

  const markAsPaidAsync = async (id: string, reference: string) => {
    dispatch(markAsPaidPending());
    const endpoint = `/api/services/app/ExpenseRequest/MarkAsPaid`;
    await instance
      .post(endpoint, { id, paymentReference: reference })
      .then(() => { dispatch(markAsPaidSuccess()); })
      .catch((error) => { console.error(error); dispatch(markAsPaidError()); });
  };

  return (
    <ExpenseRequestStateContext.Provider value={state}>
      <ExpenseRequestActionContext.Provider
        value={{
          getAsync, getAllAsync, createAsync, updateAsync,
          submitAsync, approveAsync, rejectAsync, cancelAsync, markAsPaidAsync,
        }}
      >
        {children}
      </ExpenseRequestActionContext.Provider>
    </ExpenseRequestStateContext.Provider>
  );
};

export const useExpenseRequestState = () => {
  const context = useContext(ExpenseRequestStateContext);
  if (!context) {
    throw new Error("useExpenseRequestState must be used within an ExpenseRequestProvider");
  }
  return context;
};

export const useExpenseRequestActions = () => {
  const context = useContext(ExpenseRequestActionContext);
  if (!context) {
    throw new Error("useExpenseRequestActions must be used within an ExpenseRequestProvider");
  }
  return context;
};
