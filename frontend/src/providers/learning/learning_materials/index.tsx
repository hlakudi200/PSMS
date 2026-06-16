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
  IUploadNewVersion,
  IFileUploadTicket,
  IRequestMaterialUploadUrl,
  IRequestVersionUploadUrl,
} from "../shared/interfaces";
import { IUploadLearningMaterial } from "./context";
import { LearningMaterialReducer } from "./reducer";
import { useContext, useReducer, useRef } from "react";
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
    // T-T07 versioning actions
    getVersionsPending,
    getVersionsSuccess,
    getVersionsError,
    uploadNewVersionPending,
    uploadNewVersionSuccess,
    uploadNewVersionError,
    restoreVersionPending,
    restoreVersionSuccess,
    restoreVersionError,
} from "./actions";

export const LearningMaterialProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(LearningMaterialReducer, INITIAL_STATE);
  const instance = getAxiosInstance();
  // Monotonic request id used by getAllAsync to ignore stale responses.
  // The library page debounces but still re-fetches when filters change;
  // without this guard a slow earlier response could land after a faster
  // newer one and clobber the list state.
  const getAllRequestIdRef = useRef(0);

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
    const myReqId = ++getAllRequestIdRef.current;
    dispatch(getAllLearningMaterialsPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.classSubjectId) params.append('ClassSubjectId', input.classSubjectId);
    if (input?.termId) params.append('TermId', input.termId);
    if (input?.materialType) params.append('MaterialType', input.materialType.toString());
    if (input?.isPublished !== undefined) params.append('IsPublished', input.isPublished.toString());
    if (input?.keyword) params.append('Keyword', input.keyword);

    const endpoint = `/api/services/app/LearningMaterial/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        // Drop stale responses (anything other than the latest request).
        if (myReqId !== getAllRequestIdRef.current) return;
        dispatch(getAllLearningMaterialsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        // Same staleness guard for error path.
        if (myReqId !== getAllRequestIdRef.current) return;
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

  // Step 1: get a signed upload URL (the file bytes never pass through our
  // server). Returns the ticket so the caller can PUT to it and then create.
  const requestUploadUrlAsync = async (
    input: IRequestMaterialUploadUrl
  ): Promise<IFileUploadTicket> => {
    const endpoint = `/api/services/app/LearningMaterial/RequestUploadUrl`;
    const response = await instance.post(endpoint, input);
    return response.data.result as IFileUploadTicket;
  };

  const requestVersionUploadUrlAsync = async (
    input: IRequestVersionUploadUrl
  ): Promise<IFileUploadTicket> => {
    const endpoint = `/api/services/app/LearningMaterial/RequestVersionUploadUrl`;
    const response = await instance.post(endpoint, input);
    return response.data.result as IFileUploadTicket;
  };

  // Step 2: PUT the file directly to storage. Uses plain fetch (not the API
  // axios instance) — no auth header, the signed URL carries its own token.
  const uploadFileToStorageAsync = async (uploadUrl: string, file: File) => {
    const res = await fetch(uploadUrl, {
      method: 'PUT',
      body: file,
      headers: { 'Content-Type': file.type || 'application/octet-stream' },
    });
    if (!res.ok) {
      throw new Error(`Storage upload failed (${res.status}).`);
    }
  };

  // Step 3: record the material now that its file is in storage.
  const uploadAsync = async (input: IUploadLearningMaterial) => {
    dispatch(uploadLearningMaterialPending());
    const endpoint = `/api/services/app/LearningMaterial/Upload`;
    await instance
      .post(endpoint, input)
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

  // ── T-T07 Versioning ───────────────────────────────────────────────

  const getVersionsAsync = async (learningMaterialId: string) => {
    dispatch(getVersionsPending());
    const endpoint =
      `/api/services/app/LearningMaterial/GetVersions?learningMaterialId=${learningMaterialId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getVersionsSuccess({ items: response.data.result.items }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getVersionsError());
        throw error;
      });
  };

  const uploadNewVersionAsync = async (input: IUploadNewVersion) => {
    dispatch(uploadNewVersionPending());
    const endpoint = `/api/services/app/LearningMaterial/UploadNewVersion`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(uploadNewVersionSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(uploadNewVersionError());
        throw error;
      });
  };

  const restoreVersionAsync = async (
    learningMaterialId: string,
    versionId: string
  ) => {
    dispatch(restoreVersionPending());
    const endpoint = `/api/services/app/LearningMaterial/RestoreVersion?learningMaterialId=${learningMaterialId}&versionId=${versionId}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(restoreVersionSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(restoreVersionError());
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
          requestUploadUrlAsync,
          requestVersionUploadUrlAsync,
          uploadFileToStorageAsync,
          uploadAsync,
          updateAsync,
          deleteAsync,
          publishAsync,
          unpublishAsync,
          incrementViewCountAsync,
          getVersionsAsync,
          uploadNewVersionAsync,
          restoreVersionAsync,
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
