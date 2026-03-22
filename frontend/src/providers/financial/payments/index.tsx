"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  PaymentActionContext,
  PaymentStateContext,
} from "./context";
import {
  ICreatePayment,
  IUpdatePayment,
  IGetPaymentsInput,
} from "../shared/interfaces";
import { PaymentReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
    getPaymentPending,
    getPaymentSuccess,
    getPaymentError,
    getAllPaymentsPending,
    getAllPaymentsSuccess,
    getAllPaymentsError,
    getByStudentPending,
    getByStudentSuccess,
    getByStudentError,
    getByParentPending,
    getByParentSuccess,
    getByParentError,
    createPaymentPending,
    createPaymentSuccess,
    createPaymentError,
    updatePaymentPending,
    updatePaymentSuccess,
    updatePaymentError,
    completePaymentPending,
    completePaymentSuccess,
    completePaymentError,
    failPaymentPending,
    failPaymentSuccess,
    failPaymentError,
    refundPaymentPending,
    refundPaymentSuccess,
    refundPaymentError,
    cancelPaymentPending,
    cancelPaymentSuccess,
    cancelPaymentError,
    getByReceiptNumberPending,
    getByReceiptNumberSuccess,
    getByReceiptNumberError,
} from "./actions";

export const PaymentProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(PaymentReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getPaymentPending());
    const endpoint = `/api/services/app/Payment/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getPaymentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getPaymentError());
      });
  };

  const getAllAsync = async (input?: IGetPaymentsInput) => {
    dispatch(getAllPaymentsPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.studentId) params.append('StudentId', input.studentId);
    if (input?.parentId) params.append('ParentId', input.parentId);
    if (input?.status) params.append('Status', input.status.toString());
    if (input?.paymentMethod) params.append('PaymentMethod', input.paymentMethod.toString());
    if (input?.fromDate) params.append('FromDate', input.fromDate);
    if (input?.toDate) params.append('ToDate', input.toDate);
    if (input?.receiptNumber) params.append('ReceiptNumber', input.receiptNumber);
    if (input?.studentName) params.append('StudentName', input.studentName);
    
    const endpoint = `/api/services/app/Payment/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllPaymentsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllPaymentsError());
      });
  };

  const getByStudentAsync = async (studentId: string) => {
    dispatch(getByStudentPending());
    const endpoint = `/api/services/app/Payment/GetByStudent?studentId=${studentId}`;
    await instance
        .get(endpoint)
        .then((response) => {
            dispatch(getByStudentSuccess({
                items: response.data.result.items
            }));
        })
        .catch((error) => {
            console.error(error);
            dispatch(getByStudentError());
        });
    };

    const getByParentAsync = async (parentId: string) => {
    dispatch(getByParentPending());
    const endpoint = `/api/services/app/Payment/GetByParent?parentId=${parentId}`;
    await instance
        .get(endpoint)
        .then((response) => {
            dispatch(getByParentSuccess({
                items: response.data.result.items
            }));
        })
        .catch((error) => {
            console.error(error);
            dispatch(getByParentError());
        });
    };

  const createAsync = async (input: ICreatePayment) => {
    dispatch(createPaymentPending());
    const endpoint = `/api/services/app/Payment/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createPaymentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createPaymentError());
      });
  };

  const updateAsync = async (id: string, input: IUpdatePayment) => {
    dispatch(updatePaymentPending());
    const endpoint = `/api/services/app/Payment/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updatePaymentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updatePaymentError());
      });
  };

  const completeAsync = async (id: string) => {
    dispatch(completePaymentPending());
    const endpoint = `/api/services/app/Payment/Complete?id=${id}`;
    await instance
        .post(endpoint)
        .then((response) => {
            dispatch(completePaymentSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(completePaymentError());
        });
    };

    const failAsync = async (id: string) => {
    dispatch(failPaymentPending());
    const endpoint = `/api/services/app/Payment/Fail?id=${id}`;
    await instance
        .post(endpoint)
        .then((response) => {
            dispatch(failPaymentSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(failPaymentError());
        });
    };

    const refundAsync = async (id: string) => {
    dispatch(refundPaymentPending());
    const endpoint = `/api/services/app/Payment/Refund?id=${id}`;
    await instance
        .post(endpoint)
        .then((response) => {
            dispatch(refundPaymentSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(refundPaymentError());
        });
    };

    const cancelAsync = async (id: string) => {
    dispatch(cancelPaymentPending());
    const endpoint = `/api/services/app/Payment/Cancel?id=${id}`;
    await instance
        .post(endpoint)
        .then((response) => {
            dispatch(cancelPaymentSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(cancelPaymentError());
        });
    };

    const getByReceiptNumberAsync = async (receiptNumber: string) => {
    dispatch(getByReceiptNumberPending());
    const endpoint = `/api/services/app/Payment/GetByReceiptNumber?receiptNumber=${receiptNumber}`;
    await instance
        .get(endpoint)
        .then((response) => {
            dispatch(getByReceiptNumberSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(getByReceiptNumberError());
        });
    };

  return (
    <PaymentStateContext.Provider value={state}>
      <PaymentActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getByStudentAsync,
          getByParentAsync,
          createAsync,
          updateAsync,
          completeAsync,
          failAsync,
          refundAsync,
          cancelAsync,
          getByReceiptNumberAsync,
        }}
      >
        {children}
      </PaymentActionContext.Provider>
    </PaymentStateContext.Provider>
  );
};

export const usePaymentState = () => {
  const context = useContext(PaymentStateContext);
  if (!context) {
    throw new Error("usePaymentState must be used within a PaymentProvider");
  }
  return context;
};

export const usePaymentActions = () => {
  const context = useContext(PaymentActionContext);
  if (!context) {
    throw new Error("usePaymentActions must be used within a PaymentProvider");
  }
  return context;
};
