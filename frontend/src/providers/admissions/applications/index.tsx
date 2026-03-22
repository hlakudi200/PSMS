"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  ApplicationActionContext,
  ApplicationStateContext,
} from "./context";
import {
  ICreateApplication,
  IUpdateApplication,
  IGetApplicationsInput,
} from "../shared/interfaces";
import { ApplicationReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getApplicationsPending,
  getApplicationsSuccess,
  getApplicationsError,
  getApplicationPending,
  getApplicationSuccess,
  getApplicationError,
  getApplicationByNumberPending,
  getApplicationByNumberSuccess,
  getApplicationByNumberError,
  createApplicationPending,
  createApplicationSuccess,
  createApplicationError,
  updateApplicationPending,
  updateApplicationSuccess,
  updateApplicationError,
  deleteApplicationPending,
  deleteApplicationSuccess,
  deleteApplicationError,
  submitApplicationPending,
  submitApplicationSuccess,
  submitApplicationError,
  withdrawApplicationPending,
  withdrawApplicationSuccess,
  withdrawApplicationError,
  getStatisticsPending,
  getStatisticsSuccess,
  getStatisticsError,
} from "./actions";

export const ApplicationProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(ApplicationReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getApplicationPending());
    const endpoint = `/api/services/app/Application/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getApplicationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getApplicationError());
      });
  };

  const getByApplicationNumberAsync = async (applicationNumber: string) => {
    dispatch(getApplicationByNumberPending());
    const endpoint = `/api/services/app/Application/GetByApplicationNumber?applicationNumber=${applicationNumber}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getApplicationByNumberSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getApplicationByNumberError());
      });
  };

  const getAllAsync = async (input?: IGetApplicationsInput) => {
    dispatch(getApplicationsPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.applicationNumber) params.append('ApplicationNumber', input.applicationNumber);
    if (input?.applicantName) params.append('ApplicantName', input.applicantName);
    if (input?.status) params.append('Status', input.status);
    if (input?.academicYearId) params.append('AcademicYearId', input.academicYearId);
    if (input?.gradeId) params.append('GradeId', input.gradeId);
    if (input?.submittedDateFrom) params.append('SubmittedDateFrom', input.submittedDateFrom);
    if (input?.submittedDateTo) params.append('SubmittedDateTo', input.submittedDateTo);
    if (input?.isFeePaid !== undefined) params.append('IsFeePaid', input.isFeePaid.toString());
    if (input?.isOnWaitlist !== undefined) params.append('IsOnWaitlist', input.isOnWaitlist.toString());
    if (input?.hasExpiredOffer !== undefined) params.append('HasExpiredOffer', input.hasExpiredOffer.toString());

    const endpoint = `/api/services/app/Application/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getApplicationsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getApplicationsError());
      });
  };

  const createAsync = async (input: ICreateApplication) => {
    dispatch(createApplicationPending());
    const endpoint = `/api/services/app/Application/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createApplicationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createApplicationError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateApplication) => {
    dispatch(updateApplicationPending());
    const endpoint = `/api/services/app/Application/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateApplicationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateApplicationError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteApplicationPending());
    const endpoint = `/api/services/app/Application/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteApplicationSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteApplicationError());
      });
  };

  const submitAsync = async (id: string) => {
    dispatch(submitApplicationPending());
    const endpoint = `/api/services/app/Application/Submit?id=${id}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(submitApplicationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(submitApplicationError());
      });
  };

  const withdrawAsync = async (id: string, reason?: string) => {
    dispatch(withdrawApplicationPending());
    let endpoint = `/api/services/app/Application/Withdraw?id=${id}`;
    if (reason) endpoint += `&reason=${encodeURIComponent(reason)}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(withdrawApplicationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(withdrawApplicationError());
      });
  };

  const getStatisticsAsync = async (academicYearId: string, gradeId?: string) => {
    dispatch(getStatisticsPending());
    let endpoint = `/api/services/app/Application/GetStatistics?academicYearId=${academicYearId}`;
    if (gradeId) endpoint += `&gradeId=${gradeId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getStatisticsSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getStatisticsError());
      });
  };

  return (
    <ApplicationStateContext.Provider value={state}>
      <ApplicationActionContext.Provider
        value={{
          getAsync,
          getByApplicationNumberAsync,
          getAllAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          submitAsync,
          withdrawAsync,
          getStatisticsAsync,
        }}
      >
        {children}
      </ApplicationActionContext.Provider>
    </ApplicationStateContext.Provider>
  );
};

export const useApplicationState = () => {
  const context = useContext(ApplicationStateContext);
  if (!context) {
    throw new Error("useApplicationState must be used within an ApplicationProvider");
  }
  return context;
};

export const useApplicationActions = () => {
  const context = useContext(ApplicationActionContext);
  if (!context) {
    throw new Error("useApplicationActions must be used within an ApplicationProvider");
  }
  return context;
};
