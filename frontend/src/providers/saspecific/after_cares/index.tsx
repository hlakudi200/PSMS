"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  AfterCareActionContext,
  AfterCareStateContext,
} from "./context";
import {
  ICreateAfterCare,
  IUpdateAfterCare,
  IGetAfterCaresInput,
} from "../shared/interfaces";
import { AfterCareReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getAfterCarePending,
  getAfterCareSuccess,
  getAfterCareError,
  getAfterCaresPending,
  getAfterCaresSuccess,
  getAfterCaresError,
  getByAcademicYearPending,
  getByAcademicYearSuccess,
  getByAcademicYearError,
  createAfterCarePending,
  createAfterCareSuccess,
  createAfterCareError,
  updateAfterCarePending,
  updateAfterCareSuccess,
  updateAfterCareError,
  deleteAfterCarePending,
  deleteAfterCareSuccess,
  deleteAfterCareError,
  activatePending,
  activateSuccess,
  activateError,
  deactivatePending,
  deactivateSuccess,
  deactivateError,
} from "./actions";

export const AfterCareProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(AfterCareReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getAfterCarePending());
    const endpoint = `/api/services/app/AfterCare/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAfterCareSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAfterCareError());
      });
  };

  const getAllAsync = async (input?: IGetAfterCaresInput) => {
    dispatch(getAfterCaresPending());
    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append("MaxResultCount", input.maxResultCount.toString());
    if (input?.skipCount) params.append("SkipCount", input.skipCount.toString());
    if (input?.sorting) params.append("Sorting", input.sorting);
    if (input?.academicYearId) params.append("AcademicYearId", input.academicYearId);
    if (input?.afterCareType !== undefined) params.append("AfterCareType", input.afterCareType.toString());
    if (input?.isActive !== undefined) params.append("IsActive", input.isActive.toString());
    if (input?.programName) params.append("ProgramName", input.programName);
    const endpoint = `/api/services/app/AfterCare/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAfterCaresSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAfterCaresError());
      });
  };

  const getByAcademicYearAsync = async (academicYearId: string) => {
    dispatch(getByAcademicYearPending());
    const endpoint = `/api/services/app/AfterCare/GetByAcademicYear?academicYearId=${academicYearId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByAcademicYearSuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByAcademicYearError());
      });
  };

  const createAsync = async (input: ICreateAfterCare) => {
    dispatch(createAfterCarePending());
    const endpoint = `/api/services/app/AfterCare/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createAfterCareSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createAfterCareError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateAfterCare) => {
    dispatch(updateAfterCarePending());
    const endpoint = `/api/services/app/AfterCare/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateAfterCareSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateAfterCareError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteAfterCarePending());
    const endpoint = `/api/services/app/AfterCare/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteAfterCareSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteAfterCareError());
      });
  };

  const activateAsync = async (id: string) => {
    dispatch(activatePending());
    const endpoint = `/api/services/app/AfterCare/Activate`;
    await instance
      .post(endpoint, { id })
      .then(() => {
        dispatch(activateSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(activateError());
      });
  };

  const deactivateAsync = async (id: string) => {
    dispatch(deactivatePending());
    const endpoint = `/api/services/app/AfterCare/Deactivate`;
    await instance
      .post(endpoint, { id })
      .then(() => {
        dispatch(deactivateSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deactivateError());
      });
  };

  return (
    <AfterCareStateContext.Provider value={state}>
      <AfterCareActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getByAcademicYearAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          activateAsync,
          deactivateAsync,
        }}
      >
        {children}
      </AfterCareActionContext.Provider>
    </AfterCareStateContext.Provider>
  );
};

export const useAfterCareState = () => {
  const context = useContext(AfterCareStateContext);
  if (!context) {
    throw new Error("useAfterCareState must be used within an AfterCareProvider");
  }
  return context;
};

export const useAfterCareActions = () => {
  const context = useContext(AfterCareActionContext);
  if (!context) {
    throw new Error("useAfterCareActions must be used within an AfterCareProvider");
  }
  return context;
};
