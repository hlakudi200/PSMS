import { handleActions } from "redux-actions";
import { INITIAL_STATE, IPaymentAllocationStateContext } from "./context";
import { PaymentAllocationActionEnums } from "./actions";

export const PaymentAllocationReducer = handleActions<
  IPaymentAllocationStateContext,
  IPaymentAllocationStateContext
>(
  {
    [PaymentAllocationActionEnums.getPaymentAllocationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [PaymentAllocationActionEnums.getPaymentAllocationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [PaymentAllocationActionEnums.getPaymentAllocationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [PaymentAllocationActionEnums.getByPaymentPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.getByPaymentSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.getByPaymentError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.getByStudentFeePending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.getByStudentFeeSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.getByStudentFeeError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.createPaymentAllocationPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.createPaymentAllocationSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.createPaymentAllocationError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.bulkAllocatePending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.bulkAllocateSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.bulkAllocateError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.updatePaymentAllocationPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.updatePaymentAllocationSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.updatePaymentAllocationError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.deletePaymentAllocationPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.deletePaymentAllocationSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentAllocationActionEnums.deletePaymentAllocationError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
  },
  INITIAL_STATE
);
