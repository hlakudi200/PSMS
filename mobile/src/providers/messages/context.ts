import { createContext } from "react";
import type { ComposeForm } from "../../utils/message-forms";

export type MessageBox = "inbox" | "sent";

export interface IMessageSummary {
  id: string;
  threadId: string;
  senderUserId: number;
  recipientUserId: number;
  senderName?: string;
  recipientName?: string;
  subject?: string;
  isRead: boolean;
  creationTime: string;
}

export interface IMessage extends IMessageSummary {
  content: string;
  parentMessageId?: string;
}

/** A teacher of one of the parent's children, from the server (#204). */
export interface IMessageRecipient {
  userId: number;
  name: string;
  /** "Class teacher, 3D (Ayanda)", "Mathematics, 3D (Ayanda)". */
  roles: string[];
}

export interface IMessageList {
  items: IMessageSummary[];
  totalCount: number;
}

export interface IMessagesStateContext {
  /** Per box: undefined until it first loads. */
  inbox?: IMessageList;
  sent?: IMessageList;
  isListPending: boolean;
  isListError: boolean;
  isLoadingMore: boolean;
  /** The open conversation, oldest first. */
  threadId?: string;
  thread?: IMessage[];
  isThreadPending: boolean;
  isThreadError: boolean;
  recipients?: IMessageRecipient[];
  isRecipientsPending: boolean;
  isRecipientsError: boolean;
  isSending: boolean;
  sendErrorMessage?: string;
  /** Unread messages in the inbox, for the tab badge. Undefined until it first loads. */
  unreadCount?: number;
}

export interface IMessagesActionContext {
  getBoxAsync: (box: MessageBox) => Promise<void>;
  loadMoreAsync: (box: MessageBox) => Promise<void>;
  /** Loads a conversation and marks what was sent to the caller as read. */
  openThreadAsync: (threadId: string, currentUserId: number) => Promise<void>;
  getRecipientsAsync: () => Promise<void>;
  /** Resolves the new conversation's thread id, or undefined if the server refused. */
  sendAsync: (form: ComposeForm) => Promise<string | undefined>;
  /** Replies to the other person in the open conversation. Resolves true once sent. */
  replyAsync: (content: string, currentUserId: number) => Promise<boolean>;
  clearSendError: () => void;
  getUnreadCountAsync: () => Promise<void>;
}

export const INITIAL_STATE: IMessagesStateContext = {
  isListPending: false,
  isListError: false,
  isLoadingMore: false,
  isThreadPending: false,
  isThreadError: false,
  isRecipientsPending: false,
  isRecipientsError: false,
  isSending: false,
};

export const MessagesStateContext = createContext<IMessagesStateContext>(INITIAL_STATE);
export const MessagesActionContext = createContext<IMessagesActionContext | undefined>(undefined);
