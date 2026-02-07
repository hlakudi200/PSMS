"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  MessageActionContext,
  MessageStateContext,
} from "./context";
import {
  ICreateMessage,
  IGetMessagesInput,
} from "../shared/interfaces";
import { MessageReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getMessagePending,
  getMessageSuccess,
  getMessageError,
  getInboxPending,
  getInboxSuccess,
  getInboxError,
  getSentPending,
  getSentSuccess,
  getSentError,
  getThreadPending,
  getThreadSuccess,
  getThreadError,
  sendMessagePending,
  sendMessageSuccess,
  sendMessageError,
  markAsReadPending,
  markAsReadSuccess,
  markAsReadError,
  deleteMessagePending,
  deleteMessageSuccess,
  deleteMessageError,
} from "./actions";

export const MessageProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(MessageReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getMessagePending());
    const endpoint = `/api/services/app/Message/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getMessageSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getMessageError());
      });
  };

  const getInboxAsync = async (input?: IGetMessagesInput) => {
    dispatch(getInboxPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.isRead !== undefined) params.append('IsRead', input.isRead.toString());
    if (input?.search) params.append('Search', input.search);

    const endpoint = `/api/services/app/Message/GetInbox?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getInboxSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getInboxError());
      });
  };

  const getSentAsync = async (input?: IGetMessagesInput) => {
    dispatch(getSentPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.isRead !== undefined) params.append('IsRead', input.isRead.toString());
    if (input?.search) params.append('Search', input.search);

    const endpoint = `/api/services/app/Message/GetSent?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getSentSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getSentError());
      });
  };

  const getThreadAsync = async (threadId: string) => {
    dispatch(getThreadPending());
    const endpoint = `/api/services/app/Message/GetThread?threadId=${threadId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getThreadSuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getThreadError());
      });
  };

  const sendAsync = async (input: ICreateMessage) => {
    dispatch(sendMessagePending());
    const endpoint = `/api/services/app/Message/Send`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(sendMessageSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(sendMessageError());
      });
  };

  const markAsReadAsync = async (id: string) => {
    dispatch(markAsReadPending());
    const endpoint = `/api/services/app/Message/MarkAsRead`;
    await instance
      .post(endpoint, { id })
      .then(() => {
        dispatch(markAsReadSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(markAsReadError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteMessagePending());
    const endpoint = `/api/services/app/Message/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteMessageSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteMessageError());
      });
  };

  return (
    <MessageStateContext.Provider value={state}>
      <MessageActionContext.Provider
        value={{
          getAsync,
          getInboxAsync,
          getSentAsync,
          getThreadAsync,
          sendAsync,
          markAsReadAsync,
          deleteAsync,
        }}
      >
        {children}
      </MessageActionContext.Provider>
    </MessageStateContext.Provider>
  );
};

export const useMessageState = () => {
  const context = useContext(MessageStateContext);
  if (!context) {
    throw new Error("useMessageState must be used within a MessageProvider");
  }
  return context;
};

export const useMessageActions = () => {
  const context = useContext(MessageActionContext);
  if (!context) {
    throw new Error("useMessageActions must be used within a MessageProvider");
  }
  return context;
};
