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
  IRequestDocumentUploadUrl,
  IFileUploadTicket,
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

  // SF-03 Step 1: ask the server for a one-time signed upload URL into the
  // private "documents" bucket (server validates the application + extension).
  const requestUploadUrlAsync = async (
    input: IRequestDocumentUploadUrl
  ): Promise<IFileUploadTicket> => {
    const endpoint = `/api/services/app/ApplicationDocument/RequestUploadUrl`;
    const response = await instance.post(endpoint, input);
    return response.data.result as IFileUploadTicket;
  };

  // SF-03 Step 2: PUT the bytes straight to storage. Uses fetch (not the axios
  // instance) so no Authorization header is sent to Supabase and the bytes
  // never touch our server. Throws on non-2xx so the caller can surface it.
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

  // SF-03 Step 3: record the uploaded object key. The server re-validates the
  // key, HEADs the real size/type, and keeps the key (downloads are signed).
  const uploadAsync = async (input: IUploadDocument) => {
    dispatch(uploadDocumentPending());
    const endpoint = `/api/services/app/ApplicationDocument/Upload`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(uploadDocumentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(uploadDocumentError());
        throw error;
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

  // Returns a short-lived signed URL for the private document so the caller can
  // open/stream it. Also mirrored into state for any consumer reading it there.
  const getDownloadUrlAsync = async (id: string): Promise<string | undefined> => {
    dispatch(getDownloadUrlPending());
    const endpoint = `/api/services/app/ApplicationDocument/GetDownloadUrl?id=${id}`;
    try {
      const response = await instance.get(endpoint);
      const url = response.data.result as string;
      dispatch(getDownloadUrlSuccess(url));
      return url;
    } catch (error) {
      console.error(error);
      dispatch(getDownloadUrlError());
      return undefined;
    }
  };

  return (
    <ApplicationDocumentStateContext.Provider value={state}>
      <ApplicationDocumentActionContext.Provider
        value={{
          getAsync,
          getAllByApplicationAsync,
          getRequiredDocumentsStatusAsync,
          requestUploadUrlAsync,
          uploadFileToStorageAsync,
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
