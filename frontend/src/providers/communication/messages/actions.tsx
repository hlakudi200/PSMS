import { createAction } from "redux-actions";
import { IMessageStateContext } from "./context";
import { IMessage, IMessageList, IPagedResult, IListResult } from "../shared/interfaces";

export enum MessageActionEnums {
  getMessagePending = "GET_MESSAGE_PENDING",
  getMessageSuccess = "GET_MESSAGE_SUCCESS",
  getMessageError = "GET_MESSAGE_ERROR",

  getInboxPending = "GET_INBOX_PENDING",
  getInboxSuccess = "GET_INBOX_SUCCESS",
  getInboxError = "GET_INBOX_ERROR",

  getSentPending = "GET_SENT_PENDING",
  getSentSuccess = "GET_SENT_SUCCESS",
  getSentError = "GET_SENT_ERROR",

  getThreadPending = "GET_THREAD_PENDING",
  getThreadSuccess = "GET_THREAD_SUCCESS",
  getThreadError = "GET_THREAD_ERROR",

  sendMessagePending = "SEND_MESSAGE_PENDING",
  sendMessageSuccess = "SEND_MESSAGE_SUCCESS",
  sendMessageError = "SEND_MESSAGE_ERROR",

  markAsReadPending = "MARK_AS_READ_PENDING",
  markAsReadSuccess = "MARK_AS_READ_SUCCESS",
  markAsReadError = "MARK_AS_READ_ERROR",

  deleteMessagePending = "DELETE_MESSAGE_PENDING",
  deleteMessageSuccess = "DELETE_MESSAGE_SUCCESS",
  deleteMessageError = "DELETE_MESSAGE_ERROR",
}

// Get Single Message Actions
export const getMessagePending = createAction<IMessageStateContext>(
  MessageActionEnums.getMessagePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getMessageSuccess = createAction<IMessageStateContext, IMessage>(
  MessageActionEnums.getMessageSuccess,
  (message: IMessage) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    message,
  })
);

export const getMessageError = createAction<IMessageStateContext>(
  MessageActionEnums.getMessageError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Inbox Actions
export const getInboxPending = createAction<IMessageStateContext>(
  MessageActionEnums.getInboxPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getInboxSuccess = createAction<
  IMessageStateContext,
  IPagedResult<IMessageList>
>(
  MessageActionEnums.getInboxSuccess,
  (result: IPagedResult<IMessageList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    messages: result.items,
    totalCount: result.totalCount,
  })
);

export const getInboxError = createAction<IMessageStateContext>(
  MessageActionEnums.getInboxError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Sent Actions
export const getSentPending = createAction<IMessageStateContext>(
  MessageActionEnums.getSentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getSentSuccess = createAction<
  IMessageStateContext,
  IPagedResult<IMessageList>
>(
  MessageActionEnums.getSentSuccess,
  (result: IPagedResult<IMessageList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    messages: result.items,
    totalCount: result.totalCount,
  })
);

export const getSentError = createAction<IMessageStateContext>(
  MessageActionEnums.getSentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Thread Actions
export const getThreadPending = createAction<IMessageStateContext>(
  MessageActionEnums.getThreadPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getThreadSuccess = createAction<
  IMessageStateContext,
  IListResult<IMessage>
>(
  MessageActionEnums.getThreadSuccess,
  (result: IListResult<IMessage>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    threadMessages: result.items,
  })
);

export const getThreadError = createAction<IMessageStateContext>(
  MessageActionEnums.getThreadError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Send Message Actions
export const sendMessagePending = createAction<IMessageStateContext>(
  MessageActionEnums.sendMessagePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const sendMessageSuccess = createAction<IMessageStateContext, IMessage>(
  MessageActionEnums.sendMessageSuccess,
  (message: IMessage) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    message,
  })
);

export const sendMessageError = createAction<IMessageStateContext>(
  MessageActionEnums.sendMessageError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Mark As Read Actions
export const markAsReadPending = createAction<IMessageStateContext>(
  MessageActionEnums.markAsReadPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const markAsReadSuccess = createAction<IMessageStateContext>(
  MessageActionEnums.markAsReadSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const markAsReadError = createAction<IMessageStateContext>(
  MessageActionEnums.markAsReadError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Message Actions
export const deleteMessagePending = createAction<IMessageStateContext>(
  MessageActionEnums.deleteMessagePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteMessageSuccess = createAction<IMessageStateContext>(
  MessageActionEnums.deleteMessageSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteMessageError = createAction<IMessageStateContext>(
  MessageActionEnums.deleteMessageError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
