"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  PaymentAllocationActionContext,
  PaymentAllocationStateContext,
} from "./context";
import {
  ICreatePaymentAllocation,
  IBulkAllocatePayment,
  IUpdatePaymentAllocation,
} from "../shared/interfaces";
import { PaymentAllocationReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
    getPaymentAllocationPending,
    getPaymentAllocationSuccess,
    getPaymentAllocationError,
    getByPaymentPending,
    getByPaymentSuccess,
    getByPaymentError,
    getByStudentFeePending,
    getByStudentFeeSuccess,
    getByStudentFeeError,
    createPaymentAllocationPending,
    createPaymentAllocationSuccess,
    createPaymentAllocationError,
    bulkAllocatePending,
    bulkAllocateSuccess,
    bulkAllocateError,
    updatePaymentAllocationPending,
    updatePaymentAllocationSuccess,
    updatePaymentAllocationError,
    deletePaymentAllocationPending,
    deletePaymentAllocationSuccess,
    deletePaymentAllocationError,
} from "./actions";

export const PaymentAllocationProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(PaymentAllocationReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getPaymentAllocationPending());
    const endpoint = `/api/services/app/PaymentAllocation/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getPaymentAllocationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getPaymentAllocationError());
      });
  };

  const getByPaymentAsync = async (paymentId: string) => {
    dispatch(getByPaymentPending());
    const endpoint = `/api/services/app/PaymentAllocation/GetByPayment?paymentId=${paymentId}`;
    await instance
        .get(endpoint)
        .then((response) => {
            dispatch(getByPaymentSuccess({
                items: response.data.result.items
            }));
        })
        .catch((error) => {
            console.error(error);
            dispatch(getByPaymentError());
        });
    };

    const getByStudentFeeAsync = async (studentFeeId: string) => {
    dispatch(getByStudentFeePending());
    const endpoint = `/api/services/app/PaymentAllocation/GetByStudentFee?studentFeeId=${studentFeeId}`;
    await instance
        .get(endpoint)
        .then((response) => {
            dispatch(getByStudentFeeSuccess({
                items: response.data.result.items
            }));
        })
        .catch((error) => {
            console.error(error);
            dispatch(getByStudentFeeError());
        });
    };

  const createAsync = async (input: ICreatePaymentAllocation) => {
    dispatch(createPaymentAllocationPending());
    const endpoint = `/api/services/app/PaymentAllocation/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createPaymentAllocationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createPaymentAllocationError());
      });
  };

  const bulkAllocateAsync = async (input: IBulkAllocatePayment) => {
    dispatch(bulkAllocatePending());
    const endpoint = `/api/services/app/PaymentAllocation/BulkAllocate`;
    await instance
        .post(endpoint, input)
        .then((response) => {
            dispatch(bulkAllocateSuccess({
                items: response.data.result.items
            }));
        })
        .catch((error) => {
            console.error(error);
            dispatch(bulkAllocateError());
        });
    };

  const updateAsync = async (id: string, input: IUpdatePaymentAllocation) => {
    dispatch(updatePaymentAllocationPending());
    const endpoint = `/api/services/app/PaymentAllocation/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updatePaymentAllocationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updatePaymentAllocationError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deletePaymentAllocationPending());
    const endpoint = `/api/services/app/PaymentAllocation/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deletePaymentAllocationSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deletePaymentAllocationError());
      });
  };

  return (
    <PaymentAllocationStateContext.Provider value={state}>
      <PaymentAllocationActionContext.Provider
        value={{
          getAsync,
          getByPaymentAsync,
          getByStudentFeeAsync,
          createAsync,
          bulkAllocateAsync,
          updateAsync,
          deleteAsync,
        }}
      >
        {children}
      </PaymentAllocationActionContext.Provider>
    </PaymentAllocationStateContext.Provider>
  );
};

export const usePaymentAllocationState = () => {
  const context = useContext(PaymentAllocationStateContext);
  if (!context) {
    throw new Error("usePaymentAllocationState must be used within a PaymentAllocationProvider");
  }
  return context;
};

export const usePaymentAllocationActions = () => {
  const context = useContext(PaymentAllocationActionContext);
  if (!context) {
    throw new Error("usePaymentAllocationActions must be used within a PaymentAllocationProvider");
  }
  return context;
};
