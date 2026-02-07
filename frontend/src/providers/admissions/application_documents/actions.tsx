import { createAction } from "redux-actions";
import { IApplicationDocumentStateContext } from "./context";
import { IApplicationDocument, IListResult, IRequiredDocumentsStatus } from "../shared/interfaces";

export enum ApplicationDocumentActionEnums {
  // Get
  getDocumentPending = "GET_DOCUMENT_PENDING",
  getDocumentSuccess = "GET_DOCUMENT_SUCCESS",
  getDocumentError = "GET_DOCUMENT_ERROR",

  // GetAllByApplication
  getAllByApplicationPending = "GET_ALL_BY_APPLICATION_PENDING",
  getAllByApplicationSuccess = "GET_ALL_BY_APPLICATION_SUCCESS",
  getAllByApplicationError = "GET_ALL_BY_APPLICATION_ERROR",

  // GetRequiredDocumentsStatus
  getRequiredDocumentsStatusPending = "GET_REQUIRED_DOCUMENTS_STATUS_PENDING",
  getRequiredDocumentsStatusSuccess = "GET_REQUIRED_DOCUMENTS_STATUS_SUCCESS",
  getRequiredDocumentsStatusError = "GET_REQUIRED_DOCUMENTS_STATUS_ERROR",

  // Upload
  uploadDocumentPending = "UPLOAD_DOCUMENT_PENDING",
  uploadDocumentSuccess = "UPLOAD_DOCUMENT_SUCCESS",
  uploadDocumentError = "UPLOAD_DOCUMENT_ERROR",

  // Delete
  deleteDocumentPending = "DELETE_DOCUMENT_PENDING",
  deleteDocumentSuccess = "DELETE_DOCUMENT_SUCCESS",
  deleteDocumentError = "DELETE_DOCUMENT_ERROR",

  // Verify
  verifyDocumentPending = "VERIFY_DOCUMENT_PENDING",
  verifyDocumentSuccess = "VERIFY_DOCUMENT_SUCCESS",
  verifyDocumentError = "VERIFY_DOCUMENT_ERROR",

  // Reject
  rejectDocumentPending = "REJECT_DOCUMENT_PENDING",
  rejectDocumentSuccess = "REJECT_DOCUMENT_SUCCESS",
  rejectDocumentError = "REJECT_DOCUMENT_ERROR",

  // GetDownloadUrl
  getDownloadUrlPending = "GET_DOWNLOAD_URL_PENDING",
  getDownloadUrlSuccess = "GET_DOWNLOAD_URL_SUCCESS",
  getDownloadUrlError = "GET_DOWNLOAD_URL_ERROR",
}

// Get Actions
export const getDocumentPending = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.getDocumentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getDocumentSuccess = createAction<IApplicationDocumentStateContext, IApplicationDocument>(
  ApplicationDocumentActionEnums.getDocumentSuccess,
  (document: IApplicationDocument) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    document,
  })
);

export const getDocumentError = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.getDocumentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetAllByApplication Actions
export const getAllByApplicationPending = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.getAllByApplicationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllByApplicationSuccess = createAction<
  IApplicationDocumentStateContext,
  IListResult<IApplicationDocument>
>(
  ApplicationDocumentActionEnums.getAllByApplicationSuccess,
  (result: IListResult<IApplicationDocument>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    documents: result.items,
  })
);

export const getAllByApplicationError = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.getAllByApplicationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetRequiredDocumentsStatus Actions
export const getRequiredDocumentsStatusPending = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.getRequiredDocumentsStatusPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getRequiredDocumentsStatusSuccess = createAction<IApplicationDocumentStateContext, IRequiredDocumentsStatus>(
  ApplicationDocumentActionEnums.getRequiredDocumentsStatusSuccess,
  (requiredDocumentsStatus: IRequiredDocumentsStatus) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    requiredDocumentsStatus,
  })
);

export const getRequiredDocumentsStatusError = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.getRequiredDocumentsStatusError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Upload Actions
export const uploadDocumentPending = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.uploadDocumentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const uploadDocumentSuccess = createAction<IApplicationDocumentStateContext, IApplicationDocument>(
  ApplicationDocumentActionEnums.uploadDocumentSuccess,
  (document: IApplicationDocument) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    document,
  })
);

export const uploadDocumentError = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.uploadDocumentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Actions
export const deleteDocumentPending = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.deleteDocumentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteDocumentSuccess = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.deleteDocumentSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteDocumentError = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.deleteDocumentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Verify Actions
export const verifyDocumentPending = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.verifyDocumentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const verifyDocumentSuccess = createAction<IApplicationDocumentStateContext, IApplicationDocument>(
  ApplicationDocumentActionEnums.verifyDocumentSuccess,
  (document: IApplicationDocument) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    document,
  })
);

export const verifyDocumentError = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.verifyDocumentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Reject Actions
export const rejectDocumentPending = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.rejectDocumentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const rejectDocumentSuccess = createAction<IApplicationDocumentStateContext, IApplicationDocument>(
  ApplicationDocumentActionEnums.rejectDocumentSuccess,
  (document: IApplicationDocument) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    document,
  })
);

export const rejectDocumentError = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.rejectDocumentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetDownloadUrl Actions
export const getDownloadUrlPending = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.getDownloadUrlPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getDownloadUrlSuccess = createAction<IApplicationDocumentStateContext, string>(
  ApplicationDocumentActionEnums.getDownloadUrlSuccess,
  (downloadUrl: string) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    downloadUrl,
  })
);

export const getDownloadUrlError = createAction<IApplicationDocumentStateContext>(
  ApplicationDocumentActionEnums.getDownloadUrlError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
