import { createAction } from "redux-actions";
import { IApplicationFeeStateContext } from "./context";
import { IApplicationFee, IPaymentResult } from "../shared/interfaces";

export enum ApplicationFeeActionEnums {
  // GetByApplication
  getByApplicationPending = "GET_APPLICATION_FEE_BY_APPLICATION_PENDING",
  getByApplicationSuccess = "GET_APPLICATION_FEE_BY_APPLICATION_SUCCESS",
  getByApplicationError = "GET_APPLICATION_FEE_BY_APPLICATION_ERROR",

  // RecordPayment
  recordPaymentPending = "RECORD_APPLICATION_FEE_PAYMENT_PENDING",
  recordPaymentSuccess = "RECORD_APPLICATION_FEE_PAYMENT_SUCCESS",
  recordPaymentError = "RECORD_APPLICATION_FEE_PAYMENT_ERROR",

  // ProcessPaymentCallback
  processPaymentCallbackPending = "PROCESS_APPLICATION_FEE_CALLBACK_PENDING",
  processPaymentCallbackSuccess = "PROCESS_APPLICATION_FEE_CALLBACK_SUCCESS",
  processPaymentCallbackError = "PROCESS_APPLICATION_FEE_CALLBACK_ERROR",

  // GetPaymentStatus
  getPaymentStatusPending = "GET_APPLICATION_FEE_PAYMENT_STATUS_PENDING",
  getPaymentStatusSuccess = "GET_APPLICATION_FEE_PAYMENT_STATUS_SUCCESS",
  getPaymentStatusError = "GET_APPLICATION_FEE_PAYMENT_STATUS_ERROR",
}

// GetByApplication Actions
export const getByApplicationPending = createAction<IApplicationFeeStateContext>(
  ApplicationFeeActionEnums.getByApplicationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByApplicationSuccess = createAction<IApplicationFeeStateContext, IApplicationFee>(
  ApplicationFeeActionEnums.getByApplicationSuccess,
  (applicationFee: IApplicationFee) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    applicationFee,
  })
);

export const getByApplicationError = createAction<IApplicationFeeStateContext>(
  ApplicationFeeActionEnums.getByApplicationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// RecordPayment Actions
export const recordPaymentPending = createAction<IApplicationFeeStateContext>(
  ApplicationFeeActionEnums.recordPaymentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const recordPaymentSuccess = createAction<IApplicationFeeStateContext, IPaymentResult>(
  ApplicationFeeActionEnums.recordPaymentSuccess,
  (paymentResult: IPaymentResult) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    paymentResult,
  })
);

export const recordPaymentError = createAction<IApplicationFeeStateContext>(
  ApplicationFeeActionEnums.recordPaymentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// ProcessPaymentCallback Actions
export const processPaymentCallbackPending = createAction<IApplicationFeeStateContext>(
  ApplicationFeeActionEnums.processPaymentCallbackPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const processPaymentCallbackSuccess = createAction<IApplicationFeeStateContext, IPaymentResult>(
  ApplicationFeeActionEnums.processPaymentCallbackSuccess,
  (paymentResult: IPaymentResult) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    paymentResult,
  })
);

export const processPaymentCallbackError = createAction<IApplicationFeeStateContext>(
  ApplicationFeeActionEnums.processPaymentCallbackError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetPaymentStatus Actions
export const getPaymentStatusPending = createAction<IApplicationFeeStateContext>(
  ApplicationFeeActionEnums.getPaymentStatusPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getPaymentStatusSuccess = createAction<IApplicationFeeStateContext, IPaymentResult>(
  ApplicationFeeActionEnums.getPaymentStatusSuccess,
  (paymentResult: IPaymentResult) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    paymentResult,
  })
);

export const getPaymentStatusError = createAction<IApplicationFeeStateContext>(
  ApplicationFeeActionEnums.getPaymentStatusError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
