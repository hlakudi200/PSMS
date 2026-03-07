"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  TermActionContext,
  TermStateContext,
} from "./context";
import {
  ITerm,
  ICreateTerm,
  IUpdateTerm,
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";
import { TermReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getTermsError,
  getTermsPending,
  getTermsSuccess,
  getTermError,
  getTermPending,
  getTermSuccess,
  getCurrentTermPending,
  getCurrentTermSuccess,
  getCurrentTermError,
  createTermPending,
  createTermError,
  updateTermSuccess,
  createTermSuccess,
  updateTermPending,
  updateTermError,
  deleteTermPending,
  deleteTermSuccess,
  deleteTermError,
  setAsCurrentPending,
  setAsCurrentSuccess,
  setAsCurrentError,
} from "./actions";

export const TermProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(TermReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getTermPending());
    const endpoint = `/api/services/app/Term/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getTermSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getTermError());
      });
  };

  const getAllAsync = async (input?: IPagedAndSortedResultRequest) => {
    dispatch(getTermsPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);

    const endpoint = `/api/services/app/Term/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getTermsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getTermsError());
      });
  };

  const getByAcademicYearAsync = async (academicYearId: string) => {
    dispatch(getTermsPending());
    const endpoint = `/api/services/app/Term/GetByAcademicYear?academicYearId=${academicYearId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        const items = response.data.result.items ?? response.data.result;
        dispatch(getTermsSuccess({
          items: Array.isArray(items) ? items : [],
          totalCount: Array.isArray(items) ? items.length : 0,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getTermsError());
      });
  };

  const getCurrentAsync = async () => {
    dispatch(getCurrentTermPending());
    const endpoint = `/api/services/app/Term/GetCurrent`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getCurrentTermSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getCurrentTermError());
      });
  };

  const createAsync = async (input: ICreateTerm) => {
    dispatch(createTermPending());
    const endpoint = `/api/services/app/Term/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createTermSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createTermError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateTerm) => {
    dispatch(updateTermPending());
    const endpoint = `/api/services/app/Term/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateTermSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateTermError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteTermPending());
    const endpoint = `/api/services/app/Term/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteTermSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteTermError());
      });
  };

  const setAsCurrentAsync = async (id: string) => {
    dispatch(setAsCurrentPending());
    const endpoint = `/api/services/app/Term/SetAsCurrent?id=${id}`;
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
    <TermStateContext.Provider value={state}>
      <TermActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getByAcademicYearAsync,
          getCurrentAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          setAsCurrentAsync,
        }}
      >
        {children}
      </TermActionContext.Provider>
    </TermStateContext.Provider>
  );
};

export const useTermState = () => {
  const context = useContext(TermStateContext);
  if (!context) {
    throw new Error("useTermState must be used within a TermProvider");
  }
  return context;
};

export const useTermActions = () => {
  const context = useContext(TermActionContext);
  if (!context) {
    throw new Error("useTermActions must be used within a TermProvider");
  }
  return context;
};
