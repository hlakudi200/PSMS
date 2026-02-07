import { createAction } from "redux-actions";
import { IStudentFeeStateContext } from "./context";
import { IStudentFee, IStudentFeeList, IPagedResult, IListResult } from "../shared/interfaces";

export enum StudentFeeActionEnums {
    getStudentFeePending = "GET_STUDENT_FEE_PENDING",
    getStudentFeeSuccess = "GET_STUDENT_FEE_SUCCESS",
    getStudentFeeError = "GET_STUDENT_FEE_ERROR",

    getAllStudentFeesPending = "GET_ALL_STUDENT_FEES_PENDING",
    getAllStudentFeesSuccess = "GET_ALL_STUDENT_FEES_SUCCESS",
    getAllStudentFeesError = "GET_ALL_STUDENT_FEES_ERROR",

    getByStudentPending = "GET_STUDENT_FEES_BY_STUDENT_PENDING",
    getByStudentSuccess = "GET_STUDENT_FEES_BY_STUDENT_SUCCESS",
    getByStudentError = "GET_STUDENT_FEES_BY_STUDENT_ERROR",

    createStudentFeePending = "CREATE_STUDENT_FEE_PENDING",
    createStudentFeeSuccess = "CREATE_STUDENT_FEE_SUCCESS",
    createStudentFeeError = "CREATE_STUDENT_FEE_ERROR",

    bulkCreatePending = "BULK_CREATE_STUDENT_FEES_PENDING",
    bulkCreateSuccess = "BULK_CREATE_STUDENT_FEES_SUCCESS",
    bulkCreateError = "BULK_CREATE_STUDENT_FEES_ERROR",

    updateStudentFeePending = "UPDATE_STUDENT_FEE_PENDING",
    updateStudentFeeSuccess = "UPDATE_STUDENT_FEE_SUCCESS",
    updateStudentFeeError = "UPDATE_STUDENT_FEE_ERROR",

    deleteStudentFeePending = "DELETE_STUDENT_FEE_PENDING",
    deleteStudentFeeSuccess = "DELETE_STUDENT_FEE_SUCCESS",
    deleteStudentFeeError = "DELETE_STUDENT_FEE_ERROR",

    applyDiscountPending = "APPLY_DISCOUNT_PENDING",
    applyDiscountSuccess = "APPLY_DISCOUNT_SUCCESS",
    applyDiscountError = "APPLY_DISCOUNT_ERROR",

    waiveFeePending = "WAIVE_FEE_PENDING",
    waiveFeeSuccess = "WAIVE_FEE_SUCCESS",
    waiveFeeError = "WAIVE_FEE_ERROR",

    cancelFeePending = "CANCEL_FEE_PENDING",
    cancelFeeSuccess = "CANCEL_FEE_SUCCESS",
    cancelFeeError = "CANCEL_FEE_ERROR",

    checkOverdueFeesPending = "CHECK_OVERDUE_FEES_PENDING",
    checkOverdueFeesSuccess = "CHECK_OVERDUE_FEES_SUCCESS",
    checkOverdueFeesError = "CHECK_OVERDUE_FEES_ERROR",
}

// Get Single StudentFee Actions
export const getStudentFeePending = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.getStudentFeePending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getStudentFeeSuccess = createAction<IStudentFeeStateContext, IStudentFee>(
    StudentFeeActionEnums.getStudentFeeSuccess,
    (studentFee: IStudentFee) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        studentFee,
    })
);

export const getStudentFeeError = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.getStudentFeeError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All StudentFees Actions
export const getAllStudentFeesPending = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.getAllStudentFeesPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllStudentFeesSuccess = createAction<
    IStudentFeeStateContext,
    IPagedResult<IStudentFeeList>
>(
    StudentFeeActionEnums.getAllStudentFeesSuccess,
    (result: IPagedResult<IStudentFeeList>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        studentFees: result.items,
        totalCount: result.totalCount,
    })
);

export const getAllStudentFeesError = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.getAllStudentFeesError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Student Actions
export const getByStudentPending = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.getByStudentPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByStudentSuccess = createAction<
    IStudentFeeStateContext,
    IListResult<IStudentFeeList>
>(
    StudentFeeActionEnums.getByStudentSuccess,
    (result: IListResult<IStudentFeeList>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        studentFees: result.items,
    })
);

export const getByStudentError = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.getByStudentError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create StudentFee Actions
export const createStudentFeePending = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.createStudentFeePending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createStudentFeeSuccess = createAction<IStudentFeeStateContext, IStudentFee>(
    StudentFeeActionEnums.createStudentFeeSuccess,
    (studentFee: IStudentFee) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        studentFee,
    })
);

export const createStudentFeeError = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.createStudentFeeError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Bulk Create Actions
export const bulkCreatePending = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.bulkCreatePending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const bulkCreateSuccess = createAction<
    IStudentFeeStateContext,
    IListResult<IStudentFee>
>(
    StudentFeeActionEnums.bulkCreateSuccess,
    (result: IListResult<IStudentFee>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
    })
);

export const bulkCreateError = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.bulkCreateError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update StudentFee Actions
export const updateStudentFeePending = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.updateStudentFeePending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateStudentFeeSuccess = createAction<IStudentFeeStateContext, IStudentFee>(
    StudentFeeActionEnums.updateStudentFeeSuccess,
    (studentFee: IStudentFee) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        studentFee,
    })
);

export const updateStudentFeeError = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.updateStudentFeeError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete StudentFee Actions
export const deleteStudentFeePending = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.deleteStudentFeePending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteStudentFeeSuccess = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.deleteStudentFeeSuccess,
    () => ({
        isPending: false,
        isSuccess: true,
        isError: false,
    })
);

export const deleteStudentFeeError = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.deleteStudentFeeError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Apply Discount Actions
export const applyDiscountPending = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.applyDiscountPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const applyDiscountSuccess = createAction<IStudentFeeStateContext, IStudentFee>(
    StudentFeeActionEnums.applyDiscountSuccess,
    (studentFee: IStudentFee) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        studentFee,
    })
);

export const applyDiscountError = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.applyDiscountError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Waive Fee Actions
export const waiveFeePending = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.waiveFeePending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const waiveFeeSuccess = createAction<IStudentFeeStateContext, IStudentFee>(
    StudentFeeActionEnums.waiveFeeSuccess,
    (studentFee: IStudentFee) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        studentFee,
    })
);

export const waiveFeeError = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.waiveFeeError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Cancel Fee Actions
export const cancelFeePending = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.cancelFeePending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const cancelFeeSuccess = createAction<IStudentFeeStateContext, IStudentFee>(
    StudentFeeActionEnums.cancelFeeSuccess,
    (studentFee: IStudentFee) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        studentFee,
    })
);

export const cancelFeeError = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.cancelFeeError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Check Overdue Fees Actions
export const checkOverdueFeesPending = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.checkOverdueFeesPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const checkOverdueFeesSuccess = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.checkOverdueFeesSuccess,
    () => ({
        isPending: false,
        isSuccess: true,
        isError: false,
    })
);

export const checkOverdueFeesError = createAction<IStudentFeeStateContext>(
    StudentFeeActionEnums.checkOverdueFeesError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);
