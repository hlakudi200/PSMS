"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  DocumentActionContext,
  DocumentStateContext,
} from "./context";
import {
  ICreateDocument,
  IUpdateDocument,
  IGetDocumentsInput,
} from "../shared/interfaces";
import { DocumentReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getDocumentPending,
  getDocumentSuccess,
  getDocumentError,
  getAllDocumentsPending,
  getAllDocumentsSuccess,
  getAllDocumentsError,
  createDocumentPending,
  createDocumentSuccess,
  createDocumentError,
  updateDocumentPending,
  updateDocumentSuccess,
  updateDocumentError,
  deleteDocumentPending,
  deleteDocumentSuccess,
  deleteDocumentError,
  publishDocumentPending,
  publishDocumentSuccess,
  publishDocumentError,
  unpublishDocumentPending,
  unpublishDocumentSuccess,
  unpublishDocumentError,
  recordDownloadPending,
  recordDownloadSuccess,
  recordDownloadError,
} from "./actions";

export const DocumentProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(DocumentReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getDocumentPending());
    const endpoint = `/api/services/app/Document/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getDocumentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getDocumentError());
      });
  };

  const getAllAsync = async (input?: IGetDocumentsInput) => {
    dispatch(getAllDocumentsPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.documentType !== undefined) params.append('DocumentType', input.documentType.toString());
    if (input?.targetAudience !== undefined) params.append('TargetAudience', input.targetAudience.toString());
    if (input?.category) params.append('Category', input.category);
    if (input?.isPublished !== undefined) params.append('IsPublished', input.isPublished.toString());
    if (input?.academicYearId) params.append('AcademicYearId', input.academicYearId);
    if (input?.search) params.append('Search', input.search);

    const endpoint = `/api/services/app/Document/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllDocumentsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllDocumentsError());
      });
  };

  const createAsync = async (input: ICreateDocument) => {
    dispatch(createDocumentPending());
    const endpoint = `/api/services/app/Document/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createDocumentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createDocumentError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateDocument) => {
    dispatch(updateDocumentPending());
    const endpoint = `/api/services/app/Document/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateDocumentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateDocumentError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteDocumentPending());
    const endpoint = `/api/services/app/Document/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteDocumentSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteDocumentError());
      });
  };

  const publishAsync = async (id: string) => {
    dispatch(publishDocumentPending());
    const endpoint = `/api/services/app/Document/Publish`;
    await instance
      .post(endpoint, { id })
      .then((response) => {
        dispatch(publishDocumentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(publishDocumentError());
      });
  };

  const unpublishAsync = async (id: string) => {
    dispatch(unpublishDocumentPending());
    const endpoint = `/api/services/app/Document/Unpublish`;
    await instance
      .post(endpoint, { id })
      .then((response) => {
        dispatch(unpublishDocumentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(unpublishDocumentError());
      });
  };

  const recordDownloadAsync = async (id: string) => {
    dispatch(recordDownloadPending());
    const endpoint = `/api/services/app/Document/RecordDownload`;
    await instance
      .post(endpoint, { id })
      .then((response) => {
        dispatch(recordDownloadSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(recordDownloadError());
      });
  };

  return (
    <DocumentStateContext.Provider value={state}>
      <DocumentActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          publishAsync,
          unpublishAsync,
          recordDownloadAsync,
        }}
      >
        {children}
      </DocumentActionContext.Provider>
    </DocumentStateContext.Provider>
  );
};

export const useDocumentState = () => {
  const context = useContext(DocumentStateContext);
  if (!context) {
    throw new Error("useDocumentState must be used within a DocumentProvider");
  }
  return context;
};

export const useDocumentActions = () => {
  const context = useContext(DocumentActionContext);
  if (!context) {
    throw new Error("useDocumentActions must be used within a DocumentProvider");
  }
  return context;
};
