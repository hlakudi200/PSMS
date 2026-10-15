"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  EnrollmentActionContext,
  EnrollmentStateContext,
} from "./context";
import {
  IAcceptOffer,
  IDeclineOffer,
  IAssignClass,
  IClassAvailability,
  ICompleteEnrollment,
  IEnrollment,
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
  declineOfferPending,
  declineOfferSuccess,
  declineOfferError,
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

  const getAvailableClassesAsync = async (gradeId: string): Promise<IClassAvailability[]> => {
    dispatch(getAvailableClassesPending());
    const endpoint = `/api/services/app/Enrollment/GetAvailableClasses?gradeId=${gradeId}`;
    return instance
      .get(endpoint)
      .then((response) => {
        const classes: IClassAvailability[] = response.data.result ?? [];
        dispatch(getAvailableClassesSuccess(classes));
        return classes;
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAvailableClassesError());
        throw error;
      });
  };

  /**
   * Accepting a place swallowed its own failures, so an expired offer looked
   * exactly like an accepted one: the screen simply did not change.
   */
  const acceptOfferAsync = async (input: IAcceptOffer) => {
    dispatch(acceptOfferPending());
    const endpoint = `/api/services/app/Enrollment/AcceptOffer`;
    return instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(acceptOfferSuccess(response.data.result));
        return response.data.result;
      })
      .catch((error) => {
        console.error(error);
        dispatch(acceptOfferError());
        throw error;
      });
  };

  /**
   * Turning a place down ends the application, so the caller is told when it
   * fails rather than left looking at a screen that did not change.
   */
  const declineOfferAsync = async (input: IDeclineOffer) => {
    dispatch(declineOfferPending());
    const endpoint = `/api/services/app/Enrollment/DeclineOffer`;
    return instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(declineOfferSuccess(response.data.result));
        return response.data.result;
      })
      .catch((error) => {
        console.error(error);
        dispatch(declineOfferError());
        throw error;
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

  /**
   * Returns the finished enrolment, which carries the learner's new sign-in
   * once and once only — the server withholds it on every later read, so
   * whatever calls this has to show it there and then.
   */
  const completeEnrollmentAsync = async (input: ICompleteEnrollment): Promise<IEnrollment> => {
    dispatch(completeEnrollmentPending());
    const endpoint = `/api/services/app/Enrollment/CompleteEnrollment`;
    return instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(completeEnrollmentSuccess(response.data.result));
        return response.data.result as IEnrollment;
      })
      .catch((error) => {
        console.error(error);
        dispatch(completeEnrollmentError());
        // The refusals matter here: a full class, a full intake, an offer that
        // was never accepted. The screen has to be able to say which.
        throw error;
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
          declineOfferAsync,
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
