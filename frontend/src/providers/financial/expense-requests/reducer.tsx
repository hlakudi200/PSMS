import { handleActions } from "redux-actions";
import { INITIAL_STATE, IExpenseRequestStateContext } from "./context";
import { ExpenseRequestActionEnums } from "./actions";

export const ExpenseRequestReducer = handleActions<
  IExpenseRequestStateContext,
  IExpenseRequestStateContext
>(
  {
    [ExpenseRequestActionEnums.getExpensePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.getExpenseSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.getExpenseError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.getExpensesPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.getExpensesSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.getExpensesError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.createExpensePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.createExpenseSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.createExpenseError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.updateExpensePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.updateExpenseSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.updateExpenseError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.submitPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.submitSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.submitError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.approvePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.approveSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.approveError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.rejectPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.rejectSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.rejectError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.cancelPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.cancelSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.cancelError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.markAsPaidPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.markAsPaidSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [ExpenseRequestActionEnums.markAsPaidError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
