import { createAction } from "redux-actions";
import { IAdmissionInterviewStateContext } from "./context";
import { IAdmissionInterview, ITimeSlot, IPagedResult } from "../shared/interfaces";

export enum AdmissionInterviewActionEnums {
  // Get
  getInterviewPending = "GET_INTERVIEW_PENDING",
  getInterviewSuccess = "GET_INTERVIEW_SUCCESS",
  getInterviewError = "GET_INTERVIEW_ERROR",

  // GetByApplication
  getInterviewByApplicationPending = "GET_INTERVIEW_BY_APPLICATION_PENDING",
  getInterviewByApplicationSuccess = "GET_INTERVIEW_BY_APPLICATION_SUCCESS",
  getInterviewByApplicationError = "GET_INTERVIEW_BY_APPLICATION_ERROR",

  // GetAll
  getAllInterviewsPending = "GET_ALL_INTERVIEWS_PENDING",
  getAllInterviewsSuccess = "GET_ALL_INTERVIEWS_SUCCESS",
  getAllInterviewsError = "GET_ALL_INTERVIEWS_ERROR",

  // Schedule
  scheduleInterviewPending = "SCHEDULE_INTERVIEW_PENDING",
  scheduleInterviewSuccess = "SCHEDULE_INTERVIEW_SUCCESS",
  scheduleInterviewError = "SCHEDULE_INTERVIEW_ERROR",

  // Reschedule
  rescheduleInterviewPending = "RESCHEDULE_INTERVIEW_PENDING",
  rescheduleInterviewSuccess = "RESCHEDULE_INTERVIEW_SUCCESS",
  rescheduleInterviewError = "RESCHEDULE_INTERVIEW_ERROR",

  // Cancel
  cancelInterviewPending = "CANCEL_INTERVIEW_PENDING",
  cancelInterviewSuccess = "CANCEL_INTERVIEW_SUCCESS",
  cancelInterviewError = "CANCEL_INTERVIEW_ERROR",

  // Complete
  completeInterviewPending = "COMPLETE_INTERVIEW_PENDING",
  completeInterviewSuccess = "COMPLETE_INTERVIEW_SUCCESS",
  completeInterviewError = "COMPLETE_INTERVIEW_ERROR",

  // MarkNoShow
  markNoShowPending = "MARK_NO_SHOW_PENDING",
  markNoShowSuccess = "MARK_NO_SHOW_SUCCESS",
  markNoShowError = "MARK_NO_SHOW_ERROR",

  // GetAvailableTimeSlots
  getAvailableTimeSlotsPending = "GET_AVAILABLE_TIME_SLOTS_PENDING",
  getAvailableTimeSlotsSuccess = "GET_AVAILABLE_TIME_SLOTS_SUCCESS",
  getAvailableTimeSlotsError = "GET_AVAILABLE_TIME_SLOTS_ERROR",
}

// Get Actions
export const getInterviewPending = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.getInterviewPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getInterviewSuccess = createAction<IAdmissionInterviewStateContext, IAdmissionInterview>(
  AdmissionInterviewActionEnums.getInterviewSuccess,
  (interview: IAdmissionInterview) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    interview,
  })
);

export const getInterviewError = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.getInterviewError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetByApplication Actions
export const getInterviewByApplicationPending = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.getInterviewByApplicationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getInterviewByApplicationSuccess = createAction<IAdmissionInterviewStateContext, IAdmissionInterview>(
  AdmissionInterviewActionEnums.getInterviewByApplicationSuccess,
  (interview: IAdmissionInterview) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    interview,
  })
);

export const getInterviewByApplicationError = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.getInterviewByApplicationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetAll Actions
export const getAllInterviewsPending = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.getAllInterviewsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllInterviewsSuccess = createAction<
  IAdmissionInterviewStateContext,
  IPagedResult<IAdmissionInterview>
>(
  AdmissionInterviewActionEnums.getAllInterviewsSuccess,
  (result: IPagedResult<IAdmissionInterview>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    interviews: result.items,
    totalCount: result.totalCount,
  })
);

export const getAllInterviewsError = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.getAllInterviewsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Schedule Actions
export const scheduleInterviewPending = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.scheduleInterviewPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const scheduleInterviewSuccess = createAction<IAdmissionInterviewStateContext, IAdmissionInterview>(
  AdmissionInterviewActionEnums.scheduleInterviewSuccess,
  (interview: IAdmissionInterview) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    interview,
  })
);

export const scheduleInterviewError = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.scheduleInterviewError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Reschedule Actions
export const rescheduleInterviewPending = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.rescheduleInterviewPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const rescheduleInterviewSuccess = createAction<IAdmissionInterviewStateContext, IAdmissionInterview>(
  AdmissionInterviewActionEnums.rescheduleInterviewSuccess,
  (interview: IAdmissionInterview) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    interview,
  })
);

export const rescheduleInterviewError = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.rescheduleInterviewError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Cancel Actions
export const cancelInterviewPending = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.cancelInterviewPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const cancelInterviewSuccess = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.cancelInterviewSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const cancelInterviewError = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.cancelInterviewError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Complete Actions
export const completeInterviewPending = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.completeInterviewPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const completeInterviewSuccess = createAction<IAdmissionInterviewStateContext, IAdmissionInterview>(
  AdmissionInterviewActionEnums.completeInterviewSuccess,
  (interview: IAdmissionInterview) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    interview,
  })
);

export const completeInterviewError = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.completeInterviewError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// MarkNoShow Actions
export const markNoShowPending = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.markNoShowPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const markNoShowSuccess = createAction<IAdmissionInterviewStateContext, IAdmissionInterview>(
  AdmissionInterviewActionEnums.markNoShowSuccess,
  (interview: IAdmissionInterview) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    interview,
  })
);

export const markNoShowError = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.markNoShowError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetAvailableTimeSlots Actions
export const getAvailableTimeSlotsPending = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.getAvailableTimeSlotsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAvailableTimeSlotsSuccess = createAction<IAdmissionInterviewStateContext, ITimeSlot[]>(
  AdmissionInterviewActionEnums.getAvailableTimeSlotsSuccess,
  (timeSlots: ITimeSlot[]) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    timeSlots,
  })
);

export const getAvailableTimeSlotsError = createAction<IAdmissionInterviewStateContext>(
  AdmissionInterviewActionEnums.getAvailableTimeSlotsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
