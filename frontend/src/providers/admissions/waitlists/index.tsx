"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  WaitlistActionContext,
  WaitlistStateContext,
} from "./context";
import {
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";
import { WaitlistReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getWaitlistPending,
  getWaitlistSuccess,
  getWaitlistError,
  getByApplicationPending,
  getByApplicationSuccess,
  getByApplicationError,
  getByGradePending,
  getByGradeSuccess,
  getByGradeError,
  getAllWaitlistPending,
  getAllWaitlistSuccess,
  getAllWaitlistError,
  addToWaitlistPending,
  addToWaitlistSuccess,
  addToWaitlistError,
  offerPositionPending,
  offerPositionSuccess,
  offerPositionError,
  acceptOfferPending,
  acceptOfferSuccess,
  acceptOfferError,
  declineOfferPending,
  declineOfferSuccess,
  declineOfferError,
  withdrawPending,
  withdrawSuccess,
  withdrawError,
  getPositionPending,
  getPositionSuccess,
  getPositionError,
} from "./actions";

export const WaitlistProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(WaitlistReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getWaitlistPending());
    const endpoint = `/api/services/app/Waitlist/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getWaitlistSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getWaitlistError());
        throw error;
      });
  };

  const getByApplicationAsync = async (applicationId: string) => {
    dispatch(getByApplicationPending());
    const endpoint = `/api/services/app/Waitlist/GetByApplication?applicationId=${applicationId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByApplicationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByApplicationError());
        throw error;
      });
  };

  const getByGradeAsync = async (gradeId: string) => {
    dispatch(getByGradePending());
    const endpoint = `/api/services/app/Waitlist/GetByGrade?gradeId=${gradeId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByGradeSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByGradeError());
        throw error;
      });
  };

  const getAllAsync = async (input: IPagedAndSortedResultRequest) => {
    dispatch(getAllWaitlistPending());
    const params = new URLSearchParams();
    if (input.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input.skipCount !== undefined) params.append('SkipCount', input.skipCount.toString());
    if (input.sorting) params.append('Sorting', input.sorting);
    const endpoint = `/api/services/app/Waitlist/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllWaitlistSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllWaitlistError());
        throw error;
      });
  };

  const addToWaitlistAsync = async (applicationId: string, notes?: string) => {
    dispatch(addToWaitlistPending());
    const params = new URLSearchParams();
    params.append('applicationId', applicationId);
    if (notes) params.append('notes', notes);
    const endpoint = `/api/services/app/Waitlist/AddToWaitlist?${params.toString()}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(addToWaitlistSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(addToWaitlistError());
        throw error;
      });
  };

  const offerPositionAsync = async (id: string, expiryDays: number = 7) => {
    dispatch(offerPositionPending());
    const endpoint = `/api/services/app/Waitlist/OfferPosition?id=${id}&expiryDays=${expiryDays}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(offerPositionSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(offerPositionError());
        throw error;
      });
  };

  const acceptOfferAsync = async (id: string) => {
    dispatch(acceptOfferPending());
    const endpoint = `/api/services/app/Waitlist/AcceptOffer?id=${id}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(acceptOfferSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(acceptOfferError());
        throw error;
      });
  };

  const declineOfferAsync = async (id: string) => {
    dispatch(declineOfferPending());
    const endpoint = `/api/services/app/Waitlist/DeclineOffer?id=${id}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(declineOfferSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(declineOfferError());
        throw error;
      });
  };

  const withdrawAsync = async (id: string) => {
    dispatch(withdrawPending());
    const endpoint = `/api/services/app/Waitlist/Withdraw?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(withdrawSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(withdrawError());
        throw error;
      });
  };

  const getPositionAsync = async (applicationId: string) => {
    dispatch(getPositionPending());
    const endpoint = `/api/services/app/Waitlist/GetPosition?applicationId=${applicationId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getPositionSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getPositionError());
        throw error;
      });
  };

  return (
    <WaitlistStateContext.Provider value={state}>
      <WaitlistActionContext.Provider
        value={{
          getAsync,
          getByApplicationAsync,
          getByGradeAsync,
          getAllAsync,
          addToWaitlistAsync,
          offerPositionAsync,
          acceptOfferAsync,
          declineOfferAsync,
          withdrawAsync,
          getPositionAsync,
        }}
      >
        {children}
      </WaitlistActionContext.Provider>
    </WaitlistStateContext.Provider>
  );
};

export const useWaitlistState = () => {
  const context = useContext(WaitlistStateContext);
  if (!context) {
    throw new Error("useWaitlistState must be used within a WaitlistProvider");
  }
  return context;
};

export const useWaitlistActions = () => {
  const context = useContext(WaitlistActionContext);
  if (!context) {
    throw new Error("useWaitlistActions must be used within a WaitlistProvider");
  }
  return context;
};
