import { createAction } from "redux-actions";
import { IWaitlistStateContext } from "./context";
import { IWaitlist, IWaitlistPosition, IPagedResult } from "../shared/interfaces";

export enum WaitlistActionEnums {
  // Get
  getWaitlistPending = "GET_WAITLIST_PENDING",
  getWaitlistSuccess = "GET_WAITLIST_SUCCESS",
  getWaitlistError = "GET_WAITLIST_ERROR",

  // GetByApplication
  getByApplicationPending = "GET_BY_APPLICATION_PENDING",
  getByApplicationSuccess = "GET_BY_APPLICATION_SUCCESS",
  getByApplicationError = "GET_BY_APPLICATION_ERROR",

  // GetByGrade
  getByGradePending = "GET_BY_GRADE_PENDING",
  getByGradeSuccess = "GET_BY_GRADE_SUCCESS",
  getByGradeError = "GET_BY_GRADE_ERROR",

  // GetAll
  getAllWaitlistPending = "GET_ALL_WAITLIST_PENDING",
  getAllWaitlistSuccess = "GET_ALL_WAITLIST_SUCCESS",
  getAllWaitlistError = "GET_ALL_WAITLIST_ERROR",

  // AddToWaitlist
  addToWaitlistPending = "ADD_TO_WAITLIST_PENDING",
  addToWaitlistSuccess = "ADD_TO_WAITLIST_SUCCESS",
  addToWaitlistError = "ADD_TO_WAITLIST_ERROR",

  // OfferPosition
  offerPositionPending = "OFFER_POSITION_PENDING",
  offerPositionSuccess = "OFFER_POSITION_SUCCESS",
  offerPositionError = "OFFER_POSITION_ERROR",

  // AcceptOffer
  acceptOfferPending = "ACCEPT_OFFER_PENDING",
  acceptOfferSuccess = "ACCEPT_OFFER_SUCCESS",
  acceptOfferError = "ACCEPT_OFFER_ERROR",

  // DeclineOffer
  declineOfferPending = "DECLINE_OFFER_PENDING",
  declineOfferSuccess = "DECLINE_OFFER_SUCCESS",
  declineOfferError = "DECLINE_OFFER_ERROR",

  // Withdraw
  withdrawPending = "WITHDRAW_PENDING",
  withdrawSuccess = "WITHDRAW_SUCCESS",
  withdrawError = "WITHDRAW_ERROR",

  // GetPosition
  getPositionPending = "GET_POSITION_PENDING",
  getPositionSuccess = "GET_POSITION_SUCCESS",
  getPositionError = "GET_POSITION_ERROR",
}

// Get Actions
export const getWaitlistPending = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.getWaitlistPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getWaitlistSuccess = createAction<IWaitlistStateContext, IWaitlist>(
  WaitlistActionEnums.getWaitlistSuccess,
  (waitlistEntry: IWaitlist) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    waitlistEntry,
  })
);

export const getWaitlistError = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.getWaitlistError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetByApplication Actions
export const getByApplicationPending = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.getByApplicationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByApplicationSuccess = createAction<IWaitlistStateContext, IWaitlist>(
  WaitlistActionEnums.getByApplicationSuccess,
  (waitlistEntry: IWaitlist) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    waitlistEntry,
  })
);

export const getByApplicationError = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.getByApplicationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetByGrade Actions
export const getByGradePending = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.getByGradePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByGradeSuccess = createAction<IWaitlistStateContext, IWaitlist[]>(
  WaitlistActionEnums.getByGradeSuccess,
  (waitlistEntries: IWaitlist[]) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    waitlistEntries,
  })
);

export const getByGradeError = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.getByGradeError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetAll Actions
export const getAllWaitlistPending = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.getAllWaitlistPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllWaitlistSuccess = createAction<
  IWaitlistStateContext,
  IPagedResult<IWaitlist>
>(
  WaitlistActionEnums.getAllWaitlistSuccess,
  (result: IPagedResult<IWaitlist>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    waitlistEntries: result.items,
    totalCount: result.totalCount,
  })
);

export const getAllWaitlistError = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.getAllWaitlistError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// AddToWaitlist Actions
export const addToWaitlistPending = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.addToWaitlistPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const addToWaitlistSuccess = createAction<IWaitlistStateContext, IWaitlist>(
  WaitlistActionEnums.addToWaitlistSuccess,
  (waitlistEntry: IWaitlist) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    waitlistEntry,
  })
);

export const addToWaitlistError = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.addToWaitlistError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// OfferPosition Actions
export const offerPositionPending = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.offerPositionPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const offerPositionSuccess = createAction<IWaitlistStateContext, IWaitlist>(
  WaitlistActionEnums.offerPositionSuccess,
  (waitlistEntry: IWaitlist) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    waitlistEntry,
  })
);

export const offerPositionError = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.offerPositionError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// AcceptOffer Actions
export const acceptOfferPending = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.acceptOfferPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const acceptOfferSuccess = createAction<IWaitlistStateContext, IWaitlist>(
  WaitlistActionEnums.acceptOfferSuccess,
  (waitlistEntry: IWaitlist) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    waitlistEntry,
  })
);

export const acceptOfferError = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.acceptOfferError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// DeclineOffer Actions
export const declineOfferPending = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.declineOfferPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const declineOfferSuccess = createAction<IWaitlistStateContext, IWaitlist>(
  WaitlistActionEnums.declineOfferSuccess,
  (waitlistEntry: IWaitlist) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    waitlistEntry,
  })
);

export const declineOfferError = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.declineOfferError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Withdraw Actions
export const withdrawPending = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.withdrawPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const withdrawSuccess = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.withdrawSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const withdrawError = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.withdrawError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetPosition Actions
export const getPositionPending = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.getPositionPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getPositionSuccess = createAction<IWaitlistStateContext, IWaitlistPosition>(
  WaitlistActionEnums.getPositionSuccess,
  (waitlistPosition: IWaitlistPosition) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    waitlistPosition,
  })
);

export const getPositionError = createAction<IWaitlistStateContext>(
  WaitlistActionEnums.getPositionError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
