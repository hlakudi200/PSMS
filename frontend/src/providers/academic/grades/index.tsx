"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  GradeActionContext,
  GradeStateContext,
} from "./context";
import {
  ICreateGrade,
  IUpdateGrade,
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";
import { GradeReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getGradePending,
  getGradeSuccess,
  getGradeError,
  getGradesPending,
  getGradesSuccess,
  getGradesError,
  createGradePending,
  createGradeSuccess,
  createGradeError,
  updateGradePending,
  updateGradeSuccess,
  updateGradeError,
  deleteGradePending,
  deleteGradeSuccess,
  deleteGradeError,
  getActiveGradesPending,
  getActiveGradesSuccess,
  getActiveGradesError,
  getByPhasePending,
  getByPhaseSuccess,
  getByPhaseError,
  activatePending,
  activateSuccess,
  activateError,
  deactivatePending,
  deactivateSuccess,
  deactivateError,
} from "./actions";

export const GradeProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(GradeReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getGradePending());
    const endpoint = `/api/services/app/Grade/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getGradeSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getGradeError());
      });
  };

  const getAllAsync = async (input?: IPagedAndSortedResultRequest) => {
    dispatch(getGradesPending());
    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append("MaxResultCount", input.maxResultCount.toString());
    if (input?.skipCount) params.append("SkipCount", input.skipCount.toString());
    if (input?.sorting) params.append("Sorting", input.sorting);
    const endpoint = `/api/services/app/Grade/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getGradesSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getGradesError());
      });
  };

  const createAsync = async (input: ICreateGrade) => {
    dispatch(createGradePending());
    const endpoint = `/api/services/app/Grade/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createGradeSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createGradeError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateGrade) => {
    dispatch(updateGradePending());
    const endpoint = `/api/services/app/Grade/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateGradeSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateGradeError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteGradePending());
    const endpoint = `/api/services/app/Grade/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteGradeSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteGradeError());
      });
  };

  const getActiveGradesAsync = async () => {
    dispatch(getActiveGradesPending());
    const endpoint = `/api/services/app/Grade/GetActiveGrades`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getActiveGradesSuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getActiveGradesError());
      });
  };

  const getByPhaseAsync = async (phase: number) => {
    dispatch(getByPhasePending());
    const endpoint = `/api/services/app/Grade/GetByPhase?phase=${phase}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByPhaseSuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByPhaseError());
      });
  };

  const activateAsync = async (id: string) => {
    dispatch(activatePending());
    const endpoint = `/api/services/app/Grade/Activate`;
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
    const endpoint = `/api/services/app/Grade/Deactivate`;
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
    <GradeStateContext.Provider value={state}>
      <GradeActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          getActiveGradesAsync,
          getByPhaseAsync,
          activateAsync,
          deactivateAsync,
        }}
      >
        {children}
      </GradeActionContext.Provider>
    </GradeStateContext.Provider>
  );
};

export const useGradeState = () => {
  const context = useContext(GradeStateContext);
  if (!context) {
    throw new Error("useGradeState must be used within a GradeProvider");
  }
  return context;
};

export const useGradeActions = () => {
  const context = useContext(GradeActionContext);
  if (!context) {
    throw new Error("useGradeActions must be used within a GradeProvider");
  }
  return context;
};
