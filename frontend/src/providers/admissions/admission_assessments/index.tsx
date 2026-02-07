"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  AdmissionAssessmentActionContext,
  AdmissionAssessmentStateContext,
} from "./context";
import {
  IScheduleAssessment,
  IRecordAssessmentResults,
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";
import { AdmissionAssessmentReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getAssessmentPending,
  getAssessmentSuccess,
  getAssessmentError,
  getAssessmentByApplicationPending,
  getAssessmentByApplicationSuccess,
  getAssessmentByApplicationError,
  getAssessmentsPending,
  getAssessmentsSuccess,
  getAssessmentsError,
  scheduleAssessmentPending,
  scheduleAssessmentSuccess,
  scheduleAssessmentError,
  recordResultsPending,
  recordResultsSuccess,
  recordResultsError,
  cancelAssessmentPending,
  cancelAssessmentSuccess,
  cancelAssessmentError,
} from "./actions";

export const AdmissionAssessmentProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(AdmissionAssessmentReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getAssessmentPending());
    const endpoint = `/api/services/app/AdmissionAssessment/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAssessmentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAssessmentError());
      });
  };

  const getByApplicationAsync = async (applicationId: string) => {
    dispatch(getAssessmentByApplicationPending());
    const endpoint = `/api/services/app/AdmissionAssessment/GetByApplication?applicationId=${applicationId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAssessmentByApplicationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAssessmentByApplicationError());
      });
  };

  const getAllAsync = async (input?: IPagedAndSortedResultRequest) => {
    dispatch(getAssessmentsPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);

    const endpoint = `/api/services/app/AdmissionAssessment/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAssessmentsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAssessmentsError());
      });
  };

  const scheduleAsync = async (input: IScheduleAssessment) => {
    dispatch(scheduleAssessmentPending());
    const endpoint = `/api/services/app/AdmissionAssessment/Schedule`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(scheduleAssessmentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(scheduleAssessmentError());
      });
  };

  const recordResultsAsync = async (id: string, input: IRecordAssessmentResults) => {
    dispatch(recordResultsPending());
    const endpoint = `/api/services/app/AdmissionAssessment/RecordResults?id=${id}`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(recordResultsSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(recordResultsError());
      });
  };

  const cancelAsync = async (id: string, reason: string) => {
    dispatch(cancelAssessmentPending());
    const endpoint = `/api/services/app/AdmissionAssessment/Cancel?id=${id}&reason=${encodeURIComponent(reason)}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(cancelAssessmentSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(cancelAssessmentError());
      });
  };

  return (
    <AdmissionAssessmentStateContext.Provider value={state}>
      <AdmissionAssessmentActionContext.Provider
        value={{
          getAsync,
          getByApplicationAsync,
          getAllAsync,
          scheduleAsync,
          recordResultsAsync,
          cancelAsync,
        }}
      >
        {children}
      </AdmissionAssessmentActionContext.Provider>
    </AdmissionAssessmentStateContext.Provider>
  );
};

export const useAdmissionAssessmentState = () => {
  const context = useContext(AdmissionAssessmentStateContext);
  if (!context) {
    throw new Error("useAdmissionAssessmentState must be used within an AdmissionAssessmentProvider");
  }
  return context;
};

export const useAdmissionAssessmentActions = () => {
  const context = useContext(AdmissionAssessmentActionContext);
  if (!context) {
    throw new Error("useAdmissionAssessmentActions must be used within an AdmissionAssessmentProvider");
  }
  return context;
};
