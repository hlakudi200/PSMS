"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  AcademicYearActionContext,
  AcademicYearStateContext,
} from "./context";
import {
  IAcademicYear,
  ICreateAcademicYear,
  IUpdateAcademicYear,
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";
import { AcademicYearReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getAcademicYearsError,
  getAcademicYearsPending,
  getAcademicYearsSuccess,
  getAcademicYearError,
  getAcademicYearPending,
  getAcademicYearSuccess,
  getCurrentAcademicYearPending,
  getCurrentAcademicYearSuccess,
  getCurrentAcademicYearError,
  createAcademicYearPending,
  createAcademicYearError,
  updateAcademicYearSuccess,
  createAcademicYearSuccess,
  updateAcademicYearPending,
  updateAcademicYearError,
  deleteAcademicYearPending,
  deleteAcademicYearSuccess,
  deleteAcademicYearError,
  setAsCurrentPending,
  setAsCurrentSuccess,
  setAsCurrentError,
} from "./actions";

export const AcademicYearProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(AcademicYearReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getAcademicYearPending());
    const endpoint = `/api/services/app/AcademicYear/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAcademicYearSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAcademicYearError());
      });
  };

  const getAllAsync = async (input?: IPagedAndSortedResultRequest) => {
    dispatch(getAcademicYearsPending());

    // Build query parameters
    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);

    const endpoint = `/api/services/app/AcademicYear/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAcademicYearsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAcademicYearsError());
      });
  };

  const getCurrentAsync = async () => {
    dispatch(getCurrentAcademicYearPending());
    const endpoint = `/api/services/app/AcademicYear/GetCurrent`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getCurrentAcademicYearSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getCurrentAcademicYearError());
      });
  };

  const createAsync = async (input: ICreateAcademicYear) => {
    dispatch(createAcademicYearPending());
    const endpoint = `/api/services/app/AcademicYear/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createAcademicYearSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createAcademicYearError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateAcademicYear) => {
    dispatch(updateAcademicYearPending());
    const endpoint = `/api/services/app/AcademicYear/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateAcademicYearSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateAcademicYearError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteAcademicYearPending());
    const endpoint = `/api/services/app/AcademicYear/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteAcademicYearSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteAcademicYearError());
      });
  };

  const setAsCurrentAsync = async (id: string) => {
    dispatch(setAsCurrentPending());
    const endpoint = `/api/services/app/AcademicYear/SetAsCurrent?id=${id}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(setAsCurrentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(setAsCurrentError());
      });
  };

  return (
    <AcademicYearStateContext.Provider value={state}>
      <AcademicYearActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getCurrentAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          setAsCurrentAsync,
        }}
      >
        {children}
      </AcademicYearActionContext.Provider>
    </AcademicYearStateContext.Provider>
  );
};

export const useAcademicYearState = () => {
  const context = useContext(AcademicYearStateContext);
  if (!context) {
    throw new Error(
      "useAcademicYearState must be used within an AcademicYearProvider"
    );
  }
  return context;
};

export const useAcademicYearActions = () => {
  const context = useContext(AcademicYearActionContext);
  if (!context) {
    throw new Error(
      "useAcademicYearActions must be used within an AcademicYearProvider"
    );
  }
  return context;
};
