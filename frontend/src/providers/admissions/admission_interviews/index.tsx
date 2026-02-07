"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  AdmissionInterviewActionContext,
  AdmissionInterviewStateContext,
} from "./context";
import {
  IScheduleInterview,
  IRescheduleInterview,
  ICompleteInterview,
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";
import { AdmissionInterviewReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getInterviewPending,
  getInterviewSuccess,
  getInterviewError,
  getInterviewByApplicationPending,
  getInterviewByApplicationSuccess,
  getInterviewByApplicationError,
  getAllInterviewsPending,
  getAllInterviewsSuccess,
  getAllInterviewsError,
  scheduleInterviewPending,
  scheduleInterviewSuccess,
  scheduleInterviewError,
  rescheduleInterviewPending,
  rescheduleInterviewSuccess,
  rescheduleInterviewError,
  cancelInterviewPending,
  cancelInterviewSuccess,
  cancelInterviewError,
  completeInterviewPending,
  completeInterviewSuccess,
  completeInterviewError,
  markNoShowPending,
  markNoShowSuccess,
  markNoShowError,
  getAvailableTimeSlotsPending,
  getAvailableTimeSlotsSuccess,
  getAvailableTimeSlotsError,
} from "./actions";

export const AdmissionInterviewProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(AdmissionInterviewReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getInterviewPending());
    const endpoint = `/api/services/app/AdmissionInterview/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getInterviewSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getInterviewError());
      });
  };

  const getByApplicationAsync = async (applicationId: string) => {
    dispatch(getInterviewByApplicationPending());
    const endpoint = `/api/services/app/AdmissionInterview/GetByApplication?applicationId=${applicationId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getInterviewByApplicationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getInterviewByApplicationError());
      });
  };

  const getAllAsync = async (input: IPagedAndSortedResultRequest) => {
    dispatch(getAllInterviewsPending());
    const params = new URLSearchParams();
    if (input.maxResultCount !== undefined) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input.skipCount !== undefined) params.append('SkipCount', input.skipCount.toString());
    if (input.sorting) params.append('Sorting', input.sorting);
    const endpoint = `/api/services/app/AdmissionInterview/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllInterviewsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllInterviewsError());
      });
  };

  const scheduleAsync = async (input: IScheduleInterview) => {
    dispatch(scheduleInterviewPending());
    const endpoint = `/api/services/app/AdmissionInterview/Schedule`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(scheduleInterviewSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(scheduleInterviewError());
      });
  };

  const rescheduleAsync = async (id: string, input: IRescheduleInterview) => {
    dispatch(rescheduleInterviewPending());
    const endpoint = `/api/services/app/AdmissionInterview/Reschedule?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(rescheduleInterviewSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(rescheduleInterviewError());
      });
  };

  const cancelAsync = async (id: string, reason: string) => {
    dispatch(cancelInterviewPending());
    const endpoint = `/api/services/app/AdmissionInterview/Cancel?id=${id}&reason=${encodeURIComponent(reason)}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(cancelInterviewSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(cancelInterviewError());
      });
  };

  const completeAsync = async (id: string, input: ICompleteInterview) => {
    dispatch(completeInterviewPending());
    const endpoint = `/api/services/app/AdmissionInterview/Complete?id=${id}`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(completeInterviewSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(completeInterviewError());
      });
  };

  const markNoShowAsync = async (id: string) => {
    dispatch(markNoShowPending());
    const endpoint = `/api/services/app/AdmissionInterview/MarkNoShow?id=${id}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(markNoShowSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(markNoShowError());
      });
  };

  const getAvailableTimeSlotsAsync = async (date: string, interviewerUserId: number) => {
    dispatch(getAvailableTimeSlotsPending());
    const endpoint = `/api/services/app/AdmissionInterview/GetAvailableTimeSlots?date=${date}&interviewerUserId=${interviewerUserId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAvailableTimeSlotsSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAvailableTimeSlotsError());
      });
  };

  return (
    <AdmissionInterviewStateContext.Provider value={state}>
      <AdmissionInterviewActionContext.Provider
        value={{
          getAsync,
          getByApplicationAsync,
          getAllAsync,
          scheduleAsync,
          rescheduleAsync,
          cancelAsync,
          completeAsync,
          markNoShowAsync,
          getAvailableTimeSlotsAsync,
        }}
      >
        {children}
      </AdmissionInterviewActionContext.Provider>
    </AdmissionInterviewStateContext.Provider>
  );
};

export const useAdmissionInterviewState = () => {
  const context = useContext(AdmissionInterviewStateContext);
  if (!context) {
    throw new Error("useAdmissionInterviewState must be used within an AdmissionInterviewProvider");
  }
  return context;
};

export const useAdmissionInterviewActions = () => {
  const context = useContext(AdmissionInterviewActionContext);
  if (!context) {
    throw new Error("useAdmissionInterviewActions must be used within an AdmissionInterviewProvider");
  }
  return context;
};
