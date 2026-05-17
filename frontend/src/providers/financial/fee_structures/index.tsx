"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  FeeStructureActionContext,
  FeeStructureStateContext,
} from "./context";
import {
  ICreateFeeStructure,
  IUpdateFeeStructure,
  IGetFeeStructuresInput,
} from "../shared/interfaces";
import { FeeStructureReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
    getFeeStructurePending,
    getFeeStructureSuccess,
    getFeeStructureError,
    getAllFeeStructuresPending,
    getAllFeeStructuresSuccess,
    getAllFeeStructuresError,
    getByGradeAndYearPending,
    getByGradeAndYearSuccess,
    getByGradeAndYearError,
    createFeeStructurePending,
    createFeeStructureSuccess,
    createFeeStructureError,
    updateFeeStructurePending,
    updateFeeStructureSuccess,
    updateFeeStructureError,
    deleteFeeStructurePending,
    deleteFeeStructureSuccess,
    deleteFeeStructureError,
    activateFeeStructurePending,
    activateFeeStructureSuccess,
    activateFeeStructureError,
    deactivateFeeStructurePending,
    deactivateFeeStructureSuccess,
    deactivateFeeStructureError,
} from "./actions";

export const FeeStructureProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(FeeStructureReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getFeeStructurePending());
    const endpoint = `/api/services/app/FeeStructure/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getFeeStructureSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getFeeStructureError());
        throw error;
      });
  };

  const getAllAsync = async (input?: IGetFeeStructuresInput) => {
    dispatch(getAllFeeStructuresPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.gradeId) params.append('GradeId', input.gradeId);
    if (input?.academicYearId) params.append('AcademicYearId', input.academicYearId);
    if (input?.feeType) params.append('FeeType', input.feeType.toString());
    if (input?.isActive !== undefined) params.append('IsActive', input.isActive.toString());
    if (input?.feeName) params.append('FeeName', input.feeName);
    
    const endpoint = `/api/services/app/FeeStructure/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllFeeStructuresSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllFeeStructuresError());
        throw error;
      });
  };

  const getByGradeAndYearAsync = async (gradeId: string, academicYearId: string) => {
    dispatch(getByGradeAndYearPending());
    const endpoint = `/api/services/app/FeeStructure/GetByGradeAndYear?gradeId=${gradeId}&academicYearId=${academicYearId}`;
    await instance
        .get(endpoint)
        .then((response) => {
            dispatch(getByGradeAndYearSuccess({
                items: response.data.result.items
            }));
        })
        .catch((error) => {
            console.error(error);
            dispatch(getByGradeAndYearError());
        });
    };

  const createAsync = async (input: ICreateFeeStructure) => {
    dispatch(createFeeStructurePending());
    const endpoint = `/api/services/app/FeeStructure/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createFeeStructureSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createFeeStructureError());
        throw error;
      });
  };

  const updateAsync = async (id: string, input: IUpdateFeeStructure) => {
    dispatch(updateFeeStructurePending());
    const endpoint = `/api/services/app/FeeStructure/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateFeeStructureSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateFeeStructureError());
        throw error;
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteFeeStructurePending());
    const endpoint = `/api/services/app/FeeStructure/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteFeeStructureSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteFeeStructureError());
        throw error;
      });
  };

  const activateAsync = async (id: string) => {
    dispatch(activateFeeStructurePending());
    const endpoint = `/api/services/app/FeeStructure/Activate?id=${id}`;
    await instance
        .post(endpoint)
        .then((response) => {
            dispatch(activateFeeStructureSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(activateFeeStructureError());
        });
    };

    const deactivateAsync = async (id: string) => {
    dispatch(deactivateFeeStructurePending());
    const endpoint = `/api/services/app/FeeStructure/Deactivate?id=${id}`;
    await instance
        .post(endpoint)
        .then((response) => {
            dispatch(deactivateFeeStructureSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(deactivateFeeStructureError());
        });
    };

  return (
    <FeeStructureStateContext.Provider value={state}>
      <FeeStructureActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getByGradeAndYearAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          activateAsync,
          deactivateAsync,
        }}
      >
        {children}
      </FeeStructureActionContext.Provider>
    </FeeStructureStateContext.Provider>
  );
};

export const useFeeStructureState = () => {
  const context = useContext(FeeStructureStateContext);
  if (!context) {
    throw new Error("useFeeStructureState must be used within a FeeStructureProvider");
  }
  return context;
};

export const useFeeStructureActions = () => {
  const context = useContext(FeeStructureActionContext);
  if (!context) {
    throw new Error("useFeeStructureActions must be used within a FeeStructureProvider");
  }
  return context;
};
