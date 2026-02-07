import { handleActions } from "redux-actions";
import { INITIAL_STATE, IWaitlistStateContext } from "./context";
import { WaitlistActionEnums } from "./actions";

export const WaitlistReducer = handleActions<
  IWaitlistStateContext,
  IWaitlistStateContext
>(
  {
    [WaitlistActionEnums.getWaitlistPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.getWaitlistSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.getWaitlistError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.getByApplicationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.getByApplicationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.getByApplicationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.getByGradePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.getByGradeSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.getByGradeError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.getAllWaitlistPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.getAllWaitlistSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.getAllWaitlistError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.addToWaitlistPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.addToWaitlistSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.addToWaitlistError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.offerPositionPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.offerPositionSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.offerPositionError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.acceptOfferPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.acceptOfferSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.acceptOfferError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.declineOfferPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.declineOfferSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.declineOfferError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.withdrawPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.withdrawSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.withdrawError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.getPositionPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.getPositionSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [WaitlistActionEnums.getPositionError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
