'use client'
import { createContext } from "react";
import {
  IMessage,
  IMessageList,
  ICreateMessage,
  IGetMessagesInput,
} from "../shared/interfaces";

export interface IMessageStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  message?: IMessage;
  messages?: IMessageList[];
  threadMessages?: IMessage[];
  totalCount?: number;
}

export interface IMessageActionContext {
  getAsync: (id: string) => void;
  getInboxAsync: (input?: IGetMessagesInput) => void;
  getSentAsync: (input?: IGetMessagesInput) => void;
  getThreadAsync: (threadId: string) => void;
  sendAsync: (input: ICreateMessage) => void;
  markAsReadAsync: (id: string) => void;
  deleteAsync: (id: string) => void;
}

export const INITIAL_STATE: IMessageStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const MessageStateContext =
  createContext<IMessageStateContext>(INITIAL_STATE);

export const MessageActionContext = createContext<
  IMessageActionContext | undefined
>(undefined);
