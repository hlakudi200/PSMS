import { handleActions } from "redux-actions";
import { INITIAL_STATE, IAdmissionInterviewStateContext } from "./context";
import { AdmissionInterviewActionEnums } from "./actions";

export const AdmissionInterviewReducer = handleActions<
  IAdmissionInterviewStateContext,
  IAdmissionInterviewStateContext
>(
  {
    [AdmissionInterviewActionEnums.getInterviewPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.getInterviewSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.getInterviewError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.getInterviewByApplicationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.getInterviewByApplicationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.getInterviewByApplicationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.getAllInterviewsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.getAllInterviewsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.getAllInterviewsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.scheduleInterviewPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.scheduleInterviewSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.scheduleInterviewError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.rescheduleInterviewPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.rescheduleInterviewSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.rescheduleInterviewError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.cancelInterviewPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.cancelInterviewSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.cancelInterviewError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.completeInterviewPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.completeInterviewSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.completeInterviewError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.markNoShowPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.markNoShowSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.markNoShowError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.getAvailableTimeSlotsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.getAvailableTimeSlotsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionInterviewActionEnums.getAvailableTimeSlotsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
