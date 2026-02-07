import { handleActions } from "redux-actions";
import { INITIAL_STATE, IPaymentStateContext } from "./context";
import { PaymentActionEnums } from "./actions";

export const PaymentReducer = handleActions<
  IPaymentStateContext,
  IPaymentStateContext
>(
  {
    [PaymentActionEnums.getPaymentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [PaymentActionEnums.getPaymentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [PaymentActionEnums.getPaymentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [PaymentActionEnums.getAllPaymentsPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.getAllPaymentsSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.getAllPaymentsError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.getByStudentPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.getByStudentSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.getByStudentError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.getByParentPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.getByParentSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.getByParentError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.createPaymentPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.createPaymentSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.createPaymentError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.updatePaymentPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.updatePaymentSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.updatePaymentError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.completePaymentPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.completePaymentSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.completePaymentError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.failPaymentPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.failPaymentSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.failPaymentError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.refundPaymentPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.refundPaymentSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.refundPaymentError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.cancelPaymentPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.cancelPaymentSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.cancelPaymentError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.getByReceiptNumberPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.getByReceiptNumberSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [PaymentActionEnums.getByReceiptNumberError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
  },
  INITIAL_STATE
);
