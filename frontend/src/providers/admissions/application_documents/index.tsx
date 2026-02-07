"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  ApplicationDocumentActionContext,
  ApplicationDocumentStateContext,
} from "./context";
import {
  IUploadDocument,
  IVerifyDocument,
} from "../shared/interfaces";
import { ApplicationDocumentReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getDocumentPending,
  getDocumentSuccess,
  getDocumentError,
  getAllByApplicationPending,
  getAllByApplicationSuccess,
  getAllByApplicationError,
  getRequiredDocumentsStatusPending,
  getRequiredDocumentsStatusSuccess,
  getRequiredDocumentsStatusError,
  uploadDocumentPending,
  uploadDocumentSuccess,
  uploadDocumentError,
  deleteDocumentPending,
  deleteDocumentSuccess,
  deleteDocumentError,
  verifyDocumentPending,
  verifyDocumentSuccess,
  verifyDocumentError,
  rejectDocumentPending,
  rejectDocumentSuccess,
  rejectDocumentError,
  getDownloadUrlPending,
  getDownloadUrlSuccess,
  getDownloadUrlError,
} from "./actions";

export const ApplicationDocumentProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(ApplicationDocumentReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getDocumentPending());
    const endpoint = `/api/services/app/ApplicationDocument/Get?id=${id}`;
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

  const getAllByApplicationAsync = async (applicationId: string) => {
    dispatch(getAllByApplicationPending());
    const endpoint = `/api/services/app/ApplicationDocument/GetAllByApplication?applicationId=${applicationId}`;
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

  const getRequiredDocumentsStatusAsync = async (applicationId: string) => {
    dispatch(getRequiredDocumentsStatusPending());
    const endpoint = `/api/services/app/ApplicationDocument/GetRequiredDocumentsStatus?applicationId=${applicationId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getRequiredDocumentsStatusSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getRequiredDocumentsStatusError());
      });
  };

  const uploadAsync = async (input: IUploadDocument) => {
    dispatch(uploadDocumentPending());
    const endpoint = `/api/services/app/ApplicationDocument/Upload`;

    const formData = new FormData();
    formData.append('applicationId', input.applicationId);
    formData.append('category', input.category);
    formData.append('file', input.file);
    if (input.description) {
      formData.append('description', input.description);
    }

    await instance
      .post(endpoint, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((response) => {
        dispatch(uploadDocumentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(uploadDocumentError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteDocumentPending());
    const endpoint = `/api/services/app/ApplicationDocument/Delete?id=${id}`;
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

  const verifyAsync = async (id: string, input: IVerifyDocument) => {
    dispatch(verifyDocumentPending());
    const endpoint = `/api/services/app/ApplicationDocument/Verify?id=${id}`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(verifyDocumentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(verifyDocumentError());
      });
  };

  const rejectAsync = async (id: string, reason: string) => {
    dispatch(rejectDocumentPending());
    const endpoint = `/api/services/app/ApplicationDocument/Reject?id=${id}&reason=${encodeURIComponent(reason)}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(rejectDocumentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(rejectDocumentError());
      });
  };

  const getDownloadUrlAsync = async (id: string) => {
    dispatch(getDownloadUrlPending());
    const endpoint = `/api/services/app/ApplicationDocument/GetDownloadUrl?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getDownloadUrlSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getDownloadUrlError());
      });
  };

  return (
    <ApplicationDocumentStateContext.Provider value={state}>
      <ApplicationDocumentActionContext.Provider
        value={{
          getAsync,
          getAllByApplicationAsync,
          getRequiredDocumentsStatusAsync,
          uploadAsync,
          deleteAsync,
          verifyAsync,
          rejectAsync,
          getDownloadUrlAsync,
        }}
      >
        {children}
      </ApplicationDocumentActionContext.Provider>
    </ApplicationDocumentStateContext.Provider>
  );
};

export const useApplicationDocumentState = () => {
  const context = useContext(ApplicationDocumentStateContext);
  if (!context) {
    throw new Error("useApplicationDocumentState must be used within an ApplicationDocumentProvider");
  }
  return context;
};

export const useApplicationDocumentActions = () => {
  const context = useContext(ApplicationDocumentActionContext);
  if (!context) {
    throw new Error("useApplicationDocumentActions must be used within an ApplicationDocumentProvider");
  }
  return context;
};
