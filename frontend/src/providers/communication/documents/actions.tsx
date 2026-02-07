import { createAction } from "redux-actions";
import { IDocumentStateContext } from "./context";
import { IDocument, IDocumentList, IPagedResult } from "../shared/interfaces";

export enum DocumentActionEnums {
  getDocumentPending = "GET_DOCUMENT_PENDING",
  getDocumentSuccess = "GET_DOCUMENT_SUCCESS",
  getDocumentError = "GET_DOCUMENT_ERROR",

  getAllDocumentsPending = "GET_ALL_DOCUMENTS_PENDING",
  getAllDocumentsSuccess = "GET_ALL_DOCUMENTS_SUCCESS",
  getAllDocumentsError = "GET_ALL_DOCUMENTS_ERROR",

  createDocumentPending = "CREATE_DOCUMENT_PENDING",
  createDocumentSuccess = "CREATE_DOCUMENT_SUCCESS",
  createDocumentError = "CREATE_DOCUMENT_ERROR",

  updateDocumentPending = "UPDATE_DOCUMENT_PENDING",
  updateDocumentSuccess = "UPDATE_DOCUMENT_SUCCESS",
  updateDocumentError = "UPDATE_DOCUMENT_ERROR",

  deleteDocumentPending = "DELETE_DOCUMENT_PENDING",
  deleteDocumentSuccess = "DELETE_DOCUMENT_SUCCESS",
  deleteDocumentError = "DELETE_DOCUMENT_ERROR",

  publishDocumentPending = "PUBLISH_DOCUMENT_PENDING",
  publishDocumentSuccess = "PUBLISH_DOCUMENT_SUCCESS",
  publishDocumentError = "PUBLISH_DOCUMENT_ERROR",

  unpublishDocumentPending = "UNPUBLISH_DOCUMENT_PENDING",
  unpublishDocumentSuccess = "UNPUBLISH_DOCUMENT_SUCCESS",
  unpublishDocumentError = "UNPUBLISH_DOCUMENT_ERROR",

  recordDownloadPending = "RECORD_DOWNLOAD_PENDING",
  recordDownloadSuccess = "RECORD_DOWNLOAD_SUCCESS",
  recordDownloadError = "RECORD_DOWNLOAD_ERROR",
}

// Get Single Document Actions
export const getDocumentPending = createAction<IDocumentStateContext>(
  DocumentActionEnums.getDocumentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getDocumentSuccess = createAction<IDocumentStateContext, IDocument>(
  DocumentActionEnums.getDocumentSuccess,
  (document: IDocument) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    document,
  })
);

export const getDocumentError = createAction<IDocumentStateContext>(
  DocumentActionEnums.getDocumentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All Documents Actions
export const getAllDocumentsPending = createAction<IDocumentStateContext>(
  DocumentActionEnums.getAllDocumentsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllDocumentsSuccess = createAction<
  IDocumentStateContext,
  IPagedResult<IDocumentList>
>(
  DocumentActionEnums.getAllDocumentsSuccess,
  (result: IPagedResult<IDocumentList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    documents: result.items,
    totalCount: result.totalCount,
  })
);

export const getAllDocumentsError = createAction<IDocumentStateContext>(
  DocumentActionEnums.getAllDocumentsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Document Actions
export const createDocumentPending = createAction<IDocumentStateContext>(
  DocumentActionEnums.createDocumentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createDocumentSuccess = createAction<IDocumentStateContext, IDocument>(
  DocumentActionEnums.createDocumentSuccess,
  (document: IDocument) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    document,
  })
);

export const createDocumentError = createAction<IDocumentStateContext>(
  DocumentActionEnums.createDocumentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Document Actions
export const updateDocumentPending = createAction<IDocumentStateContext>(
  DocumentActionEnums.updateDocumentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateDocumentSuccess = createAction<IDocumentStateContext, IDocument>(
  DocumentActionEnums.updateDocumentSuccess,
  (document: IDocument) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    document,
  })
);

export const updateDocumentError = createAction<IDocumentStateContext>(
  DocumentActionEnums.updateDocumentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Document Actions
export const deleteDocumentPending = createAction<IDocumentStateContext>(
  DocumentActionEnums.deleteDocumentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteDocumentSuccess = createAction<IDocumentStateContext>(
  DocumentActionEnums.deleteDocumentSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteDocumentError = createAction<IDocumentStateContext>(
  DocumentActionEnums.deleteDocumentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Publish Document Actions
export const publishDocumentPending = createAction<IDocumentStateContext>(
  DocumentActionEnums.publishDocumentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const publishDocumentSuccess = createAction<IDocumentStateContext, IDocument>(
  DocumentActionEnums.publishDocumentSuccess,
  (document: IDocument) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    document,
  })
);

export const publishDocumentError = createAction<IDocumentStateContext>(
  DocumentActionEnums.publishDocumentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Unpublish Document Actions
export const unpublishDocumentPending = createAction<IDocumentStateContext>(
  DocumentActionEnums.unpublishDocumentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const unpublishDocumentSuccess = createAction<IDocumentStateContext, IDocument>(
  DocumentActionEnums.unpublishDocumentSuccess,
  (document: IDocument) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    document,
  })
);

export const unpublishDocumentError = createAction<IDocumentStateContext>(
  DocumentActionEnums.unpublishDocumentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Record Download Actions
export const recordDownloadPending = createAction<IDocumentStateContext>(
  DocumentActionEnums.recordDownloadPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const recordDownloadSuccess = createAction<IDocumentStateContext, IDocument>(
  DocumentActionEnums.recordDownloadSuccess,
  (document: IDocument) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    document,
  })
);

export const recordDownloadError = createAction<IDocumentStateContext>(
  DocumentActionEnums.recordDownloadError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
