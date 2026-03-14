"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  LearningMaterialActionContext,
  LearningMaterialStateContext,
} from "./context";
import {
  ICreateLearningMaterial,
  IUpdateLearningMaterial,
  IGetLearningMaterialsInput,
} from "../shared/interfaces";
import { LearningMaterialReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
    getLearningMaterialPending,
    getLearningMaterialSuccess,
    getLearningMaterialError,
    getAllLearningMaterialsPending,
    getAllLearningMaterialsSuccess,
    getAllLearningMaterialsError,
    getByClassSubjectPending,
    getByClassSubjectSuccess,
    getByClassSubjectError,
    createLearningMaterialPending,
    createLearningMaterialSuccess,
    createLearningMaterialError,
    updateLearningMaterialPending,
    updateLearningMaterialSuccess,
    updateLearningMaterialError,
    deleteLearningMaterialPending,
    deleteLearningMaterialSuccess,
    deleteLearningMaterialError,
    publishLearningMaterialPending,
    publishLearningMaterialSuccess,
    publishLearningMaterialError,
    unpublishLearningMaterialPending,
    unpublishLearningMaterialSuccess,
    unpublishLearningMaterialError,
    incrementViewCountPending,
    incrementViewCountSuccess,
    incrementViewCountError,
} from "./actions";

export const LearningMaterialProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(LearningMaterialReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getLearningMaterialPending());
    const endpoint = `/api/services/app/LearningMaterial/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getLearningMaterialSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getLearningMaterialError());
      });
  };

  const getAllAsync = async (input?: IGetLearningMaterialsInput) => {
    dispatch(getAllLearningMaterialsPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.classSubjectId) params.append('ClassSubjectId', input.classSubjectId);
    if (input?.termId) params.append('TermId', input.termId);
    if (input?.materialType) params.append('MaterialType', input.materialType.toString());
    if (input?.isPublished !== undefined) params.append('IsPublished', input.isPublished.toString());

    const endpoint = `/api/services/app/LearningMaterial/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllLearningMaterialsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllLearningMaterialsError());
      });
  };

  const getByClassSubjectAsync = async (classSubjectId: string) => {
    dispatch(getByClassSubjectPending());
    const endpoint = `/api/services/app/LearningMaterial/GetByClassSubject?classSubjectId=${classSubjectId}`;
    await instance
        .get(endpoint)
        .then((response) => {
            dispatch(getByClassSubjectSuccess({
                items: response.data.result.items
            }));
        })
        .catch((error) => {
            console.error(error);
            dispatch(getByClassSubjectError());
        });
    };

  const createAsync = async (input: ICreateLearningMaterial) => {
    dispatch(createLearningMaterialPending());
    const endpoint = `/api/services/app/LearningMaterial/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createLearningMaterialSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createLearningMaterialError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateLearningMaterial) => {
    dispatch(updateLearningMaterialPending());
    const endpoint = `/api/services/app/LearningMaterial/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateLearningMaterialSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateLearningMaterialError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteLearningMaterialPending());
    const endpoint = `/api/services/app/LearningMaterial/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteLearningMaterialSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteLearningMaterialError());
      });
  };

  const publishAsync = async (id: string) => {
    dispatch(publishLearningMaterialPending());
    const endpoint = `/api/services/app/LearningMaterial/Publish?id=${id}`;
    await instance
        .post(endpoint)
        .then((response) => {
            dispatch(publishLearningMaterialSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(publishLearningMaterialError());
        });
    };

    const unpublishAsync = async (id: string) => {
    dispatch(unpublishLearningMaterialPending());
    const endpoint = `/api/services/app/LearningMaterial/Unpublish?id=${id}`;
    await instance
        .post(endpoint)
        .then((response) => {
            dispatch(unpublishLearningMaterialSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(unpublishLearningMaterialError());
        });
    };

    const incrementViewCountAsync = async (id: string) => {
    dispatch(incrementViewCountPending());
    const endpoint = `/api/services/app/LearningMaterial/IncrementViewCount?id=${id}`;
    await instance
        .post(endpoint)
        .then(() => {
            dispatch(incrementViewCountSuccess());
        })
        .catch((error) => {
            console.error(error);
            dispatch(incrementViewCountError());
        });
    };

  return (
    <LearningMaterialStateContext.Provider value={state}>
      <LearningMaterialActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getByClassSubjectAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          publishAsync,
          unpublishAsync,
          incrementViewCountAsync,
        }}
      >
        {children}
      </LearningMaterialActionContext.Provider>
    </LearningMaterialStateContext.Provider>
  );
};

export const useLearningMaterialState = () => {
  const context = useContext(LearningMaterialStateContext);
  if (!context) {
    throw new Error("useLearningMaterialState must be used within a LearningMaterialProvider");
  }
  return context;
};

export const useLearningMaterialActions = () => {
  const context = useContext(LearningMaterialActionContext);
  if (!context) {
    throw new Error("useLearningMaterialActions must be used within a LearningMaterialProvider");
  }
  return context;
};
