import { handleActions } from "redux-actions";
import { INITIAL_STATE, IMessageStateContext } from "./context";
import { MessageActionEnums } from "./actions";

export const MessageReducer = handleActions<
  IMessageStateContext,
  IMessageStateContext
>(
  {
    [MessageActionEnums.getMessagePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.getMessageSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.getMessageError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.getInboxPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.getInboxSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.getInboxError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.getSentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.getSentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.getSentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.getThreadPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.getThreadSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.getThreadError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.sendMessagePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.sendMessageSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.sendMessageError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.markAsReadPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.markAsReadSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.markAsReadError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.deleteMessagePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.deleteMessageSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MessageActionEnums.deleteMessageError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
