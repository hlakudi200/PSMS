"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  ApplicantParentActionContext,
  ApplicantParentStateContext,
} from "./context";
import {
  ICreateApplicantParent,
  IUpdateApplicantParent,
} from "../shared/interfaces";
import { ApplicantParentReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getApplicantParentPending,
  getApplicantParentSuccess,
  getApplicantParentError,
  getAllByApplicationPending,
  getAllByApplicationSuccess,
  getAllByApplicationError,
  createApplicantParentPending,
  createApplicantParentSuccess,
  createApplicantParentError,
  updateApplicantParentPending,
  updateApplicantParentSuccess,
  updateApplicantParentError,
  deleteApplicantParentPending,
  deleteApplicantParentSuccess,
  deleteApplicantParentError,
  setAsPrimaryContactPending,
  setAsPrimaryContactSuccess,
  setAsPrimaryContactError,
  setAsFinanciallyResponsiblePending,
  setAsFinanciallyResponsibleSuccess,
  setAsFinanciallyResponsibleError,
} from "./actions";

export const ApplicantParentProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(ApplicantParentReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getApplicantParentPending());
    const endpoint = `/api/services/app/ApplicantParent/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getApplicantParentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getApplicantParentError());
      });
  };

  const getAllByApplicationAsync = async (applicationId: string) => {
    dispatch(getAllByApplicationPending());
    const endpoint = `/api/services/app/ApplicantParent/GetAllByApplication?applicationId=${applicationId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllByApplicationSuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllByApplicationError());
      });
  };

  const createAsync = async (input: ICreateApplicantParent) => {
    dispatch(createApplicantParentPending());
    const endpoint = `/api/services/app/ApplicantParent/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createApplicantParentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createApplicantParentError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateApplicantParent) => {
    dispatch(updateApplicantParentPending());
    const endpoint = `/api/services/app/ApplicantParent/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateApplicantParentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateApplicantParentError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteApplicantParentPending());
    const endpoint = `/api/services/app/ApplicantParent/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteApplicantParentSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteApplicantParentError());
      });
  };

  const setAsPrimaryContactAsync = async (id: string) => {
    dispatch(setAsPrimaryContactPending());
    const endpoint = `/api/services/app/ApplicantParent/SetAsPrimaryContact?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(setAsPrimaryContactSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(setAsPrimaryContactError());
      });
  };

  const setAsFinanciallyResponsibleAsync = async (id: string) => {
    dispatch(setAsFinanciallyResponsiblePending());
    const endpoint = `/api/services/app/ApplicantParent/SetAsFinanciallyResponsible?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(setAsFinanciallyResponsibleSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(setAsFinanciallyResponsibleError());
      });
  };

  return (
    <ApplicantParentStateContext.Provider value={state}>
      <ApplicantParentActionContext.Provider
        value={{
          getAsync,
          getAllByApplicationAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          setAsPrimaryContactAsync,
          setAsFinanciallyResponsibleAsync,
        }}
      >
        {children}
      </ApplicantParentActionContext.Provider>
    </ApplicantParentStateContext.Provider>
  );
};

export const useApplicantParentState = () => {
  const context = useContext(ApplicantParentStateContext);
  if (!context) {
    throw new Error("useApplicantParentState must be used within an ApplicantParentProvider");
  }
  return context;
};

export const useApplicantParentActions = () => {
  const context = useContext(ApplicantParentActionContext);
  if (!context) {
    throw new Error("useApplicantParentActions must be used within an ApplicantParentProvider");
  }
  return context;
};
