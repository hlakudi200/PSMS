"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  EnrollmentActionContext,
  EnrollmentStateContext,
} from "./context";
import {
  IAcceptOffer,
  IAssignClass,
  ICompleteEnrollment,
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";
import { EnrollmentReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getByApplicationPending,
  getByApplicationSuccess,
  getByApplicationError,
  getPendingEnrollmentsPending,
  getPendingEnrollmentsSuccess,
  getPendingEnrollmentsError,
  getAvailableClassesPending,
  getAvailableClassesSuccess,
  getAvailableClassesError,
  acceptOfferPending,
  acceptOfferSuccess,
  acceptOfferError,
  assignClassPending,
  assignClassSuccess,
  assignClassError,
  completeEnrollmentPending,
  completeEnrollmentSuccess,
  completeEnrollmentError,
} from "./actions";

export const EnrollmentProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(EnrollmentReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getByApplicationAsync = async (applicationId: string) => {
    dispatch(getByApplicationPending());
    const endpoint = `/api/services/app/Enrollment/GetByApplication?applicationId=${applicationId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByApplicationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByApplicationError());
      });
  };

  const getPendingEnrollmentsAsync = async (input: IPagedAndSortedResultRequest) => {
    dispatch(getPendingEnrollmentsPending());
    const params = new URLSearchParams();
    if (input.maxResultCount !== undefined) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input.skipCount !== undefined) params.append('SkipCount', input.skipCount.toString());
    if (input.sorting) params.append('Sorting', input.sorting);
    const endpoint = `/api/services/app/Enrollment/GetPendingEnrollments?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getPendingEnrollmentsSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getPendingEnrollmentsError());
      });
  };

  const getAvailableClassesAsync = async (gradeId: string) => {
    dispatch(getAvailableClassesPending());
    const endpoint = `/api/services/app/Enrollment/GetAvailableClasses?gradeId=${gradeId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAvailableClassesSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAvailableClassesError());
      });
  };

  const acceptOfferAsync = async (input: IAcceptOffer) => {
    dispatch(acceptOfferPending());
    const endpoint = `/api/services/app/Enrollment/AcceptOffer`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(acceptOfferSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(acceptOfferError());
      });
  };

  const assignClassAsync = async (input: IAssignClass) => {
    dispatch(assignClassPending());
    const endpoint = `/api/services/app/Enrollment/AssignClass`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(assignClassSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(assignClassError());
      });
  };

  const completeEnrollmentAsync = async (input: ICompleteEnrollment) => {
    dispatch(completeEnrollmentPending());
    const endpoint = `/api/services/app/Enrollment/CompleteEnrollment`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(completeEnrollmentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(completeEnrollmentError());
      });
  };

  return (
    <EnrollmentStateContext.Provider value={state}>
      <EnrollmentActionContext.Provider
        value={{
          getByApplicationAsync,
          getPendingEnrollmentsAsync,
          getAvailableClassesAsync,
          acceptOfferAsync,
          assignClassAsync,
          completeEnrollmentAsync,
        }}
      >
        {children}
      </EnrollmentActionContext.Provider>
    </EnrollmentStateContext.Provider>
  );
};

export const useEnrollmentState = () => {
  const context = useContext(EnrollmentStateContext);
  if (!context) {
    throw new Error("useEnrollmentState must be used within an EnrollmentProvider");
  }
  return context;
};

export const useEnrollmentActions = () => {
  const context = useContext(EnrollmentActionContext);
  if (!context) {
    throw new Error("useEnrollmentActions must be used within an EnrollmentProvider");
  }
  return context;
};
