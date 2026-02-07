import { createAction } from "redux-actions";
import { IPaymentStateContext } from "./context";
import { IPayment, IPaymentList, IPagedResult, IListResult } from "../shared/interfaces";

export enum PaymentActionEnums {
    getPaymentPending = "GET_PAYMENT_PENDING",
    getPaymentSuccess = "GET_PAYMENT_SUCCESS",
    getPaymentError = "GET_PAYMENT_ERROR",

    getAllPaymentsPending = "GET_ALL_PAYMENTS_PENDING",
    getAllPaymentsSuccess = "GET_ALL_PAYMENTS_SUCCESS",
    getAllPaymentsError = "GET_ALL_PAYMENTS_ERROR",

    getByStudentPending = "GET_PAYMENTS_BY_STUDENT_PENDING",
    getByStudentSuccess = "GET_PAYMENTS_BY_STUDENT_SUCCESS",
    getByStudentError = "GET_PAYMENTS_BY_STUDENT_ERROR",

    getByParentPending = "GET_PAYMENTS_BY_PARENT_PENDING",
    getByParentSuccess = "GET_PAYMENTS_BY_PARENT_SUCCESS",
    getByParentError = "GET_PAYMENTS_BY_PARENT_ERROR",

    createPaymentPending = "CREATE_PAYMENT_PENDING",
    createPaymentSuccess = "CREATE_PAYMENT_SUCCESS",
    createPaymentError = "CREATE_PAYMENT_ERROR",

    updatePaymentPending = "UPDATE_PAYMENT_PENDING",
    updatePaymentSuccess = "UPDATE_PAYMENT_SUCCESS",
    updatePaymentError = "UPDATE_PAYMENT_ERROR",

    completePaymentPending = "COMPLETE_PAYMENT_PENDING",
    completePaymentSuccess = "COMPLETE_PAYMENT_SUCCESS",
    completePaymentError = "COMPLETE_PAYMENT_ERROR",

    failPaymentPending = "FAIL_PAYMENT_PENDING",
    failPaymentSuccess = "FAIL_PAYMENT_SUCCESS",
    failPaymentError = "FAIL_PAYMENT_ERROR",

    refundPaymentPending = "REFUND_PAYMENT_PENDING",
    refundPaymentSuccess = "REFUND_PAYMENT_SUCCESS",
    refundPaymentError = "REFUND_PAYMENT_ERROR",
    
    cancelPaymentPending = "CANCEL_PAYMENT_PENDING",
    cancelPaymentSuccess = "CANCEL_PAYMENT_SUCCESS",
    cancelPaymentError = "CANCEL_PAYMENT_ERROR",

    getByReceiptNumberPending = "GET_BY_RECEIPT_NUMBER_PENDING",
    getByReceiptNumberSuccess = "GET_BY_RECEIPT_NUMBER_SUCCESS",
    getByReceiptNumberError = "GET_BY_RECEIPT_NUMBER_ERROR",
}

// Get Single Payment Actions
export const getPaymentPending = createAction<IPaymentStateContext>(
    PaymentActionEnums.getPaymentPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getPaymentSuccess = createAction<IPaymentStateContext, IPayment>(
    PaymentActionEnums.getPaymentSuccess,
    (payment: IPayment) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        payment,
    })
);

export const getPaymentError = createAction<IPaymentStateContext>(
    PaymentActionEnums.getPaymentError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All Payments Actions
export const getAllPaymentsPending = createAction<IPaymentStateContext>(
    PaymentActionEnums.getAllPaymentsPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllPaymentsSuccess = createAction<
    IPaymentStateContext,
    IPagedResult<IPaymentList>
>(
    PaymentActionEnums.getAllPaymentsSuccess,
    (result: IPagedResult<IPaymentList>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        payments: result.items,
        totalCount: result.totalCount,
    })
);

export const getAllPaymentsError = createAction<IPaymentStateContext>(
    PaymentActionEnums.getAllPaymentsError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Student Actions
export const getByStudentPending = createAction<IPaymentStateContext>(
    PaymentActionEnums.getByStudentPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByStudentSuccess = createAction<
    IPaymentStateContext,
    IListResult<IPaymentList>
>(
    PaymentActionEnums.getByStudentSuccess,
    (result: IListResult<IPaymentList>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        payments: result.items,
    })
);

export const getByStudentError = createAction<IPaymentStateContext>(
    PaymentActionEnums.getByStudentError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Parent Actions
export const getByParentPending = createAction<IPaymentStateContext>(
    PaymentActionEnums.getByParentPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByParentSuccess = createAction<
    IPaymentStateContext,
    IListResult<IPaymentList>
>(
    PaymentActionEnums.getByParentSuccess,
    (result: IListResult<IPaymentList>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        payments: result.items,
    })
);

export const getByParentError = createAction<IPaymentStateContext>(
    PaymentActionEnums.getByParentError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Payment Actions
export const createPaymentPending = createAction<IPaymentStateContext>(
    PaymentActionEnums.createPaymentPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createPaymentSuccess = createAction<IPaymentStateContext, IPayment>(
    PaymentActionEnums.createPaymentSuccess,
    (payment: IPayment) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        payment,
    })
);

export const createPaymentError = createAction<IPaymentStateContext>(
    PaymentActionEnums.createPaymentError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Payment Actions
export const updatePaymentPending = createAction<IPaymentStateContext>(
    PaymentActionEnums.updatePaymentPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updatePaymentSuccess = createAction<IPaymentStateContext, IPayment>(
    PaymentActionEnums.updatePaymentSuccess,
    (payment: IPayment) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        payment,
    })
);

export const updatePaymentError = createAction<IPaymentStateContext>(
    PaymentActionEnums.updatePaymentError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Complete Payment Actions
export const completePaymentPending = createAction<IPaymentStateContext>(
    PaymentActionEnums.completePaymentPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const completePaymentSuccess = createAction<IPaymentStateContext, IPayment>(
    PaymentActionEnums.completePaymentSuccess,
    (payment: IPayment) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        payment,
    })
);

export const completePaymentError = createAction<IPaymentStateContext>(
    PaymentActionEnums.completePaymentError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Fail Payment Actions
export const failPaymentPending = createAction<IPaymentStateContext>(
    PaymentActionEnums.failPaymentPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const failPaymentSuccess = createAction<IPaymentStateContext, IPayment>(
    PaymentActionEnums.failPaymentSuccess,
    (payment: IPayment) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        payment,
    })
);

export const failPaymentError = createAction<IPaymentStateContext>(
    PaymentActionEnums.failPaymentError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Refund Payment Actions
export const refundPaymentPending = createAction<IPaymentStateContext>(
    PaymentActionEnums.refundPaymentPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const refundPaymentSuccess = createAction<IPaymentStateContext, IPayment>(
    PaymentActionEnums.refundPaymentSuccess,
    (payment: IPayment) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        payment,
    })
);

export const refundPaymentError = createAction<IPaymentStateContext>(
    PaymentActionEnums.refundPaymentError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Cancel Payment Actions
export const cancelPaymentPending = createAction<IPaymentStateContext>(
    PaymentActionEnums.cancelPaymentPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const cancelPaymentSuccess = createAction<IPaymentStateContext, IPayment>(
    PaymentActionEnums.cancelPaymentSuccess,
    (payment: IPayment) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        payment,
    })
);

export const cancelPaymentError = createAction<IPaymentStateContext>(
    PaymentActionEnums.cancelPaymentError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Receipt Number Actions
export const getByReceiptNumberPending = createAction<IPaymentStateContext>(
    PaymentActionEnums.getByReceiptNumberPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByReceiptNumberSuccess = createAction<IPaymentStateContext, IPayment>(
    PaymentActionEnums.getByReceiptNumberSuccess,
    (payment: IPayment) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        payment,
    })
);

export const getByReceiptNumberError = createAction<IPaymentStateContext>(
    PaymentActionEnums.getByReceiptNumberError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);
