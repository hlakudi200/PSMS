import { handleActions } from "redux-actions";
import { INITIAL_STATE, IApplicationDocumentStateContext } from "./context";
import { ApplicationDocumentActionEnums } from "./actions";

export const ApplicationDocumentReducer = handleActions<
  IApplicationDocumentStateContext,
  IApplicationDocumentStateContext
>(
  {
    [ApplicationDocumentActionEnums.getDocumentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.getDocumentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.getDocumentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.getAllByApplicationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.getAllByApplicationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.getAllByApplicationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.getRequiredDocumentsStatusPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.getRequiredDocumentsStatusSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.getRequiredDocumentsStatusError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.uploadDocumentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.uploadDocumentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.uploadDocumentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.deleteDocumentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.deleteDocumentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.deleteDocumentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.verifyDocumentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.verifyDocumentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.verifyDocumentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.rejectDocumentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.rejectDocumentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.rejectDocumentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.getDownloadUrlPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.getDownloadUrlSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ApplicationDocumentActionEnums.getDownloadUrlError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
