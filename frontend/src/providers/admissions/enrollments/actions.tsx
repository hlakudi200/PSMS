import { createAction } from "redux-actions";
import { IEnrollmentStateContext } from "./context";
import {
  IEnrollment,
  IClassAvailability,
  IPagedResult,
} from "../shared/interfaces";

export enum EnrollmentActionEnums {
  // GetByApplication
  getByApplicationPending = "GET_ENROLLMENT_BY_APPLICATION_PENDING",
  getByApplicationSuccess = "GET_ENROLLMENT_BY_APPLICATION_SUCCESS",
  getByApplicationError = "GET_ENROLLMENT_BY_APPLICATION_ERROR",

  // GetPendingEnrollments
  getPendingEnrollmentsPending = "GET_PENDING_ENROLLMENTS_PENDING",
  getPendingEnrollmentsSuccess = "GET_PENDING_ENROLLMENTS_SUCCESS",
  getPendingEnrollmentsError = "GET_PENDING_ENROLLMENTS_ERROR",

  // GetAvailableClasses
  getAvailableClassesPending = "GET_AVAILABLE_CLASSES_PENDING",
  getAvailableClassesSuccess = "GET_AVAILABLE_CLASSES_SUCCESS",
  getAvailableClassesError = "GET_AVAILABLE_CLASSES_ERROR",

  // AcceptOffer
  acceptOfferPending = "ACCEPT_OFFER_PENDING",
  acceptOfferSuccess = "ACCEPT_OFFER_SUCCESS",
  acceptOfferError = "ACCEPT_OFFER_ERROR",

  // AssignClass
  assignClassPending = "ASSIGN_CLASS_PENDING",
  assignClassSuccess = "ASSIGN_CLASS_SUCCESS",
  assignClassError = "ASSIGN_CLASS_ERROR",

  // CompleteEnrollment
  completeEnrollmentPending = "COMPLETE_ENROLLMENT_PENDING",
  completeEnrollmentSuccess = "COMPLETE_ENROLLMENT_SUCCESS",
  completeEnrollmentError = "COMPLETE_ENROLLMENT_ERROR",
}

// GetByApplication Actions
export const getByApplicationPending = createAction<IEnrollmentStateContext>(
  EnrollmentActionEnums.getByApplicationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByApplicationSuccess = createAction<IEnrollmentStateContext, IEnrollment>(
  EnrollmentActionEnums.getByApplicationSuccess,
  (enrollment: IEnrollment) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    enrollment,
  })
);

export const getByApplicationError = createAction<IEnrollmentStateContext>(
  EnrollmentActionEnums.getByApplicationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetPendingEnrollments Actions
export const getPendingEnrollmentsPending = createAction<IEnrollmentStateContext>(
  EnrollmentActionEnums.getPendingEnrollmentsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getPendingEnrollmentsSuccess = createAction<
  IEnrollmentStateContext,
  IPagedResult<IEnrollment>
>(
  EnrollmentActionEnums.getPendingEnrollmentsSuccess,
  (result: IPagedResult<IEnrollment>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    enrollments: result.items,
    totalCount: result.totalCount,
  })
);

export const getPendingEnrollmentsError = createAction<IEnrollmentStateContext>(
  EnrollmentActionEnums.getPendingEnrollmentsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetAvailableClasses Actions
export const getAvailableClassesPending = createAction<IEnrollmentStateContext>(
  EnrollmentActionEnums.getAvailableClassesPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAvailableClassesSuccess = createAction<IEnrollmentStateContext, IClassAvailability[]>(
  EnrollmentActionEnums.getAvailableClassesSuccess,
  (availableClasses: IClassAvailability[]) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    availableClasses,
  })
);

export const getAvailableClassesError = createAction<IEnrollmentStateContext>(
  EnrollmentActionEnums.getAvailableClassesError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// AcceptOffer Actions
export const acceptOfferPending = createAction<IEnrollmentStateContext>(
  EnrollmentActionEnums.acceptOfferPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const acceptOfferSuccess = createAction<IEnrollmentStateContext, IEnrollment>(
  EnrollmentActionEnums.acceptOfferSuccess,
  (enrollment: IEnrollment) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    enrollment,
  })
);

export const acceptOfferError = createAction<IEnrollmentStateContext>(
  EnrollmentActionEnums.acceptOfferError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// AssignClass Actions
export const assignClassPending = createAction<IEnrollmentStateContext>(
  EnrollmentActionEnums.assignClassPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const assignClassSuccess = createAction<IEnrollmentStateContext, IEnrollment>(
  EnrollmentActionEnums.assignClassSuccess,
  (enrollment: IEnrollment) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    enrollment,
  })
);

export const assignClassError = createAction<IEnrollmentStateContext>(
  EnrollmentActionEnums.assignClassError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// CompleteEnrollment Actions
export const completeEnrollmentPending = createAction<IEnrollmentStateContext>(
  EnrollmentActionEnums.completeEnrollmentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const completeEnrollmentSuccess = createAction<IEnrollmentStateContext, IEnrollment>(
  EnrollmentActionEnums.completeEnrollmentSuccess,
  (enrollment: IEnrollment) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    enrollment,
  })
);

export const completeEnrollmentError = createAction<IEnrollmentStateContext>(
  EnrollmentActionEnums.completeEnrollmentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
