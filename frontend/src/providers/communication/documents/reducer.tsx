import { handleActions } from "redux-actions";
import { INITIAL_STATE, IDocumentStateContext } from "./context";
import { DocumentActionEnums } from "./actions";

export const DocumentReducer = handleActions<
  IDocumentStateContext,
  IDocumentStateContext
>(
  {
    [DocumentActionEnums.getDocumentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.getDocumentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.getDocumentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.getAllDocumentsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.getAllDocumentsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.getAllDocumentsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.createDocumentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.createDocumentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.createDocumentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.updateDocumentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.updateDocumentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.updateDocumentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.deleteDocumentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.deleteDocumentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.deleteDocumentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.publishDocumentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.publishDocumentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.publishDocumentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.unpublishDocumentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.unpublishDocumentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.unpublishDocumentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.recordDownloadPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.recordDownloadSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [DocumentActionEnums.recordDownloadError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
