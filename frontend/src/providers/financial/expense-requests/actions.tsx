import { createAction } from "redux-actions";
import { IExpenseRequestStateContext, IExpenseRequest } from "./context";

export enum ExpenseRequestActionEnums {
  getExpensePending = "GET_EXPENSE_REQUEST_PENDING",
  getExpenseSuccess = "GET_EXPENSE_REQUEST_SUCCESS",
  getExpenseError = "GET_EXPENSE_REQUEST_ERROR",

  getExpensesPending = "GET_EXPENSE_REQUESTS_PENDING",
  getExpensesSuccess = "GET_EXPENSE_REQUESTS_SUCCESS",
  getExpensesError = "GET_EXPENSE_REQUESTS_ERROR",

  createExpensePending = "CREATE_EXPENSE_REQUEST_PENDING",
  createExpenseSuccess = "CREATE_EXPENSE_REQUEST_SUCCESS",
  createExpenseError = "CREATE_EXPENSE_REQUEST_ERROR",

  updateExpensePending = "UPDATE_EXPENSE_REQUEST_PENDING",
  updateExpenseSuccess = "UPDATE_EXPENSE_REQUEST_SUCCESS",
  updateExpenseError = "UPDATE_EXPENSE_REQUEST_ERROR",

  submitPending = "SUBMIT_EXPENSE_REQUEST_PENDING",
  submitSuccess = "SUBMIT_EXPENSE_REQUEST_SUCCESS",
  submitError = "SUBMIT_EXPENSE_REQUEST_ERROR",

  approvePending = "APPROVE_EXPENSE_REQUEST_PENDING",
  approveSuccess = "APPROVE_EXPENSE_REQUEST_SUCCESS",
  approveError = "APPROVE_EXPENSE_REQUEST_ERROR",

  rejectPending = "REJECT_EXPENSE_REQUEST_PENDING",
  rejectSuccess = "REJECT_EXPENSE_REQUEST_SUCCESS",
  rejectError = "REJECT_EXPENSE_REQUEST_ERROR",

  cancelPending = "CANCEL_EXPENSE_REQUEST_PENDING",
  cancelSuccess = "CANCEL_EXPENSE_REQUEST_SUCCESS",
  cancelError = "CANCEL_EXPENSE_REQUEST_ERROR",

  markAsPaidPending = "MARK_AS_PAID_EXPENSE_REQUEST_PENDING",
  markAsPaidSuccess = "MARK_AS_PAID_EXPENSE_REQUEST_SUCCESS",
  markAsPaidError = "MARK_AS_PAID_EXPENSE_REQUEST_ERROR",
}

// Get Single
export const getExpensePending = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.getExpensePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getExpenseSuccess = createAction<IExpenseRequestStateContext, IExpenseRequest>(
  ExpenseRequestActionEnums.getExpenseSuccess,
  (expenseRequest: IExpenseRequest) => ({
    isPending: false, isSuccess: true, isError: false, expenseRequest,
  })
);
export const getExpenseError = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.getExpenseError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All
export const getExpensesPending = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.getExpensesPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getExpensesSuccess = createAction<IExpenseRequestStateContext, { items: IExpenseRequest[]; totalCount: number }>(
  ExpenseRequestActionEnums.getExpensesSuccess,
  (result) => ({
    isPending: false, isSuccess: true, isError: false,
    expenseRequests: result.items, totalCount: result.totalCount,
  })
);
export const getExpensesError = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.getExpensesError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create
export const createExpensePending = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.createExpensePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const createExpenseSuccess = createAction<IExpenseRequestStateContext, IExpenseRequest>(
  ExpenseRequestActionEnums.createExpenseSuccess,
  (expenseRequest: IExpenseRequest) => ({
    isPending: false, isSuccess: true, isError: false, expenseRequest,
  })
);
export const createExpenseError = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.createExpenseError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update
export const updateExpensePending = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.updateExpensePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const updateExpenseSuccess = createAction<IExpenseRequestStateContext, IExpenseRequest>(
  ExpenseRequestActionEnums.updateExpenseSuccess,
  (expenseRequest: IExpenseRequest) => ({
    isPending: false, isSuccess: true, isError: false, expenseRequest,
  })
);
export const updateExpenseError = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.updateExpenseError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Submit
export const submitPending = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.submitPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const submitSuccess = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.submitSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const submitError = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.submitError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Approve
export const approvePending = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.approvePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const approveSuccess = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.approveSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const approveError = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.approveError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Reject
export const rejectPending = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.rejectPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const rejectSuccess = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.rejectSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const rejectError = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.rejectError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Cancel
export const cancelPending = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.cancelPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const cancelSuccess = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.cancelSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const cancelError = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.cancelError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Mark as Paid
export const markAsPaidPending = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.markAsPaidPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const markAsPaidSuccess = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.markAsPaidSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const markAsPaidError = createAction<IExpenseRequestStateContext>(
  ExpenseRequestActionEnums.markAsPaidError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
