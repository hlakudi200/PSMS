import { createAction } from "redux-actions";
import type { IMessage, IMessageList, IMessageRecipient, IMessagesStateContext } from "./context";

type MessagesPatch = Partial<IMessagesStateContext>;

export enum MessagesActionEnums {
  listPending = "MESSAGES_LIST_PENDING",
  listSuccess = "MESSAGES_LIST_SUCCESS",
  listError = "MESSAGES_LIST_ERROR",
  loadMorePending = "MESSAGES_LOAD_MORE_PENDING",
  loadMoreDone = "MESSAGES_LOAD_MORE_DONE",
  threadPending = "MESSAGES_THREAD_PENDING",
  threadSuccess = "MESSAGES_THREAD_SUCCESS",
  threadError = "MESSAGES_THREAD_ERROR",
  threadRead = "MESSAGES_THREAD_READ",
  recipientsPending = "MESSAGES_RECIPIENTS_PENDING",
  recipientsSuccess = "MESSAGES_RECIPIENTS_SUCCESS",
  recipientsError = "MESSAGES_RECIPIENTS_ERROR",
  sendPending = "MESSAGES_SEND_PENDING",
  sendSuccess = "MESSAGES_SEND_SUCCESS",
  sendError = "MESSAGES_SEND_ERROR",
  unreadCountSuccess = "MESSAGES_UNREAD_COUNT_SUCCESS",
}

/** Inbox or sent, keyed so one patch shape serves both. */
type BoxPatch = { inbox?: IMessageList; sent?: IMessageList };

export const listPending = createAction<MessagesPatch>(MessagesActionEnums.listPending, () => ({
  isListPending: true, isListError: false,
}));

export const listSuccess = createAction<MessagesPatch, BoxPatch>(
  MessagesActionEnums.listSuccess,
  (box) => ({ isListPending: false, isListError: false, ...box })
);

export const listError = createAction<MessagesPatch>(MessagesActionEnums.listError, () => ({
  isListPending: false, isListError: true,
}));

export const loadMorePending = createAction<MessagesPatch>(MessagesActionEnums.loadMorePending, () => ({
  isLoadingMore: true,
}));

/** A failed next page keeps what has loaded; reaching the end again retries. */
export const loadMoreDone = createAction<MessagesPatch, BoxPatch>(
  MessagesActionEnums.loadMoreDone,
  (box) => ({ isLoadingMore: false, ...box })
);

export const threadPending = createAction<MessagesPatch, string>(MessagesActionEnums.threadPending, (threadId) => ({
  isThreadPending: true, isThreadError: false, threadId,
}));

export const threadSuccess = createAction<MessagesPatch, IMessage[]>(
  MessagesActionEnums.threadSuccess,
  (thread) => ({ isThreadPending: false, isThreadError: false, thread })
);

export const threadError = createAction<MessagesPatch>(MessagesActionEnums.threadError, () => ({
  isThreadPending: false, isThreadError: true,
}));

/** The conversation, the inbox rows and the count move together once the server has recorded the reads. */
export const threadRead = createAction<
  MessagesPatch,
  { thread: IMessage[]; inbox?: IMessageList; unreadCount?: number }
>(
  MessagesActionEnums.threadRead,
  ({ thread, inbox, unreadCount }) => ({ thread, ...(inbox && { inbox }), ...(unreadCount != null && { unreadCount }) })
);

export const unreadCountSuccess = createAction<MessagesPatch, number>(
  MessagesActionEnums.unreadCountSuccess,
  (unreadCount) => ({ unreadCount })
);

export const recipientsPending = createAction<MessagesPatch>(MessagesActionEnums.recipientsPending, () => ({
  isRecipientsPending: true, isRecipientsError: false,
}));

export const recipientsSuccess = createAction<MessagesPatch, IMessageRecipient[]>(
  MessagesActionEnums.recipientsSuccess,
  (recipients) => ({ isRecipientsPending: false, isRecipientsError: false, recipients })
);

export const recipientsError = createAction<MessagesPatch>(MessagesActionEnums.recipientsError, () => ({
  isRecipientsPending: false, isRecipientsError: true,
}));

export const sendPending = createAction<MessagesPatch>(MessagesActionEnums.sendPending, () => ({
  isSending: true, sendErrorMessage: undefined,
}));

export const sendSuccess = createAction<MessagesPatch, { thread?: IMessage[] }>(
  MessagesActionEnums.sendSuccess,
  ({ thread }) => ({ isSending: false, ...(thread && { thread }) })
);

export const sendError = createAction<MessagesPatch, string | undefined>(
  MessagesActionEnums.sendError,
  (message) => ({ isSending: false, sendErrorMessage: message })
);
