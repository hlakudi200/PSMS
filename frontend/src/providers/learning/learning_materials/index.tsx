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
import { IUploadLearningMaterial } from "./context";
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
    uploadLearningMaterialPending,
    uploadLearningMaterialSuccess,
    uploadLearningMaterialError,
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
        throw error;
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
        throw error;
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
            throw error;
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
        throw error;
      });
  };

  const uploadAsync = async (input: IUploadLearningMaterial) => {
    dispatch(uploadLearningMaterialPending());
    const endpoint = `/api/services/app/LearningMaterial/Upload`;

    // ABP MVC binds the IFormFile from a multipart "File" field; the
    // sibling primitive fields are bound directly off the form. Sending
    // null/undefined values would break the binder, so we only set the
    // ones the caller actually provided.
    const formData = new FormData();
    formData.append('ClassSubjectId', input.classSubjectId);
    formData.append('Title', input.title);
    formData.append('MaterialType', input.materialType.toString());
    if (input.termId) formData.append('TermId', input.termId);
    if (input.description) formData.append('Description', input.description);
    if (input.externalLink) formData.append('ExternalLink', input.externalLink);
    if (input.displayOrder != null)
      formData.append('DisplayOrder', input.displayOrder.toString());
    if (input.file) formData.append('File', input.file);

    await instance
      .post(endpoint, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((response) => {
        dispatch(uploadLearningMaterialSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(uploadLearningMaterialError());
        throw error;
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
        throw error;
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
        throw error;
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
            throw error;
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
            throw error;
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
            throw error;
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
          uploadAsync,
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
