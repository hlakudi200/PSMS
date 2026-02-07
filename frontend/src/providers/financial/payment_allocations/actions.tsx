import { createAction } from "redux-actions";
import { IPaymentAllocationStateContext } from "./context";
import { IPaymentAllocation, IPaymentAllocationList, IListResult } from "../shared/interfaces";

export enum PaymentAllocationActionEnums {
    getPaymentAllocationPending = "GET_PAYMENT_ALLOCATION_PENDING",
    getPaymentAllocationSuccess = "GET_PAYMENT_ALLOCATION_SUCCESS",
    getPaymentAllocationError = "GET_PAYMENT_ALLOCATION_ERROR",

    getByPaymentPending = "GET_PAYMENT_ALLOCATIONS_BY_PAYMENT_PENDING",
    getByPaymentSuccess = "GET_PAYMENT_ALLOCATIONS_BY_PAYMENT_SUCCESS",
    getByPaymentError = "GET_PAYMENT_ALLOCATIONS_BY_PAYMENT_ERROR",

    getByStudentFeePending = "GET_PAYMENT_ALLOCATIONS_BY_STUDENT_FEE_PENDING",
    getByStudentFeeSuccess = "GET_PAYMENT_ALLOCATIONS_BY_STUDENT_FEE_SUCCESS",
    getByStudentFeeError = "GET_PAYMENT_ALLOCATIONS_BY_STUDENT_FEE_ERROR",

    createPaymentAllocationPending = "CREATE_PAYMENT_ALLOCATION_PENDING",
    createPaymentAllocationSuccess = "CREATE_PAYMENT_ALLOCATION_SUCCESS",
    createPaymentAllocationError = "CREATE_PAYMENT_ALLOCATION_ERROR",

    bulkAllocatePending = "BULK_ALLOCATE_PAYMENT_PENDING",
    bulkAllocateSuccess = "BULK_ALLOCATE_PAYMENT_SUCCESS",
    bulkAllocateError = "BULK_ALLOCATE_PAYMENT_ERROR",

    updatePaymentAllocationPending = "UPDATE_PAYMENT_ALLOCATION_PENDING",
    updatePaymentAllocationSuccess = "UPDATE_PAYMENT_ALLOCATION_SUCCESS",
    updatePaymentAllocationError = "UPDATE_PAYMENT_ALLOCATION_ERROR",

    deletePaymentAllocationPending = "DELETE_PAYMENT_ALLOCATION_PENDING",
    deletePaymentAllocationSuccess = "DELETE_PAYMENT_ALLOCATION_SUCCESS",
    deletePaymentAllocationError = "DELETE_PAYMENT_ALLOCATION_ERROR",
}

// Get Single PaymentAllocation Actions
export const getPaymentAllocationPending = createAction<IPaymentAllocationStateContext>(
    PaymentAllocationActionEnums.getPaymentAllocationPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getPaymentAllocationSuccess = createAction<IPaymentAllocationStateContext, IPaymentAllocation>(
    PaymentAllocationActionEnums.getPaymentAllocationSuccess,
    (paymentAllocation: IPaymentAllocation) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        paymentAllocation,
    })
);

export const getPaymentAllocationError = createAction<IPaymentAllocationStateContext>(
    PaymentAllocationActionEnums.getPaymentAllocationError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Payment Actions
export const getByPaymentPending = createAction<IPaymentAllocationStateContext>(
    PaymentAllocationActionEnums.getByPaymentPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByPaymentSuccess = createAction<
    IPaymentAllocationStateContext,
    IListResult<IPaymentAllocationList>
>(
    PaymentAllocationActionEnums.getByPaymentSuccess,
    (result: IListResult<IPaymentAllocationList>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        paymentAllocations: result.items,
    })
);

export const getByPaymentError = createAction<IPaymentAllocationStateContext>(
    PaymentAllocationActionEnums.getByPaymentError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By StudentFee Actions
export const getByStudentFeePending = createAction<IPaymentAllocationStateContext>(
    PaymentAllocationActionEnums.getByStudentFeePending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByStudentFeeSuccess = createAction<
    IPaymentAllocationStateContext,
    IListResult<IPaymentAllocationList>
>(
    PaymentAllocationActionEnums.getByStudentFeeSuccess,
    (result: IListResult<IPaymentAllocationList>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        paymentAllocations: result.items,
    })
);

export const getByStudentFeeError = createAction<IPaymentAllocationStateContext>(
    PaymentAllocationActionEnums.getByStudentFeeError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create PaymentAllocation Actions
export const createPaymentAllocationPending = createAction<IPaymentAllocationStateContext>(
    PaymentAllocationActionEnums.createPaymentAllocationPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createPaymentAllocationSuccess = createAction<IPaymentAllocationStateContext, IPaymentAllocation>(
    PaymentAllocationActionEnums.createPaymentAllocationSuccess,
    (paymentAllocation: IPaymentAllocation) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        paymentAllocation,
    })
);

export const createPaymentAllocationError = createAction<IPaymentAllocationStateContext>(
    PaymentAllocationActionEnums.createPaymentAllocationError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Bulk Allocate Actions
export const bulkAllocatePending = createAction<IPaymentAllocationStateContext>(
    PaymentAllocationActionEnums.bulkAllocatePending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const bulkAllocateSuccess = createAction<
    IPaymentAllocationStateContext,
    IListResult<IPaymentAllocation>
>(
    PaymentAllocationActionEnums.bulkAllocateSuccess,
    (result: IListResult<IPaymentAllocation>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
    })
);

export const bulkAllocateError = createAction<IPaymentAllocationStateContext>(
    PaymentAllocationActionEnums.bulkAllocateError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update PaymentAllocation Actions
export const updatePaymentAllocationPending = createAction<IPaymentAllocationStateContext>(
    PaymentAllocationActionEnums.updatePaymentAllocationPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updatePaymentAllocationSuccess = createAction<IPaymentAllocationStateContext, IPaymentAllocation>(
    PaymentAllocationActionEnums.updatePaymentAllocationSuccess,
    (paymentAllocation: IPaymentAllocation) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        paymentAllocation,
    })
);

export const updatePaymentAllocationError = createAction<IPaymentAllocationStateContext>(
    PaymentAllocationActionEnums.updatePaymentAllocationError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete PaymentAllocation Actions
export const deletePaymentAllocationPending = createAction<IPaymentAllocationStateContext>(
    PaymentAllocationActionEnums.deletePaymentAllocationPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deletePaymentAllocationSuccess = createAction<IPaymentAllocationStateContext>(
    PaymentAllocationActionEnums.deletePaymentAllocationSuccess,
    () => ({
        isPending: false,
        isSuccess: true,
        isError: false,
    })
);

export const deletePaymentAllocationError = createAction<IPaymentAllocationStateContext>(
    PaymentAllocationActionEnums.deletePaymentAllocationError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);
