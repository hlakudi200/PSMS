"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  NotificationActionContext,
  NotificationStateContext,
} from "./context";
import {
  ICreateNotification,
  IGetNotificationsInput,
} from "../shared/interfaces";
import { NotificationReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getNotificationPending,
  getNotificationSuccess,
  getNotificationError,
  getAllNotificationsPending,
  getAllNotificationsSuccess,
  getAllNotificationsError,
  getUnreadCountPending,
  getUnreadCountSuccess,
  getUnreadCountError,
  markAsReadPending,
  markAsReadSuccess,
  markAsReadError,
  markAllAsReadPending,
  markAllAsReadSuccess,
  markAllAsReadError,
  createNotificationPending,
  createNotificationSuccess,
  createNotificationError,
} from "./actions";

export const NotificationProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(NotificationReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getNotificationPending());
    const endpoint = `/api/services/app/Notification/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getNotificationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getNotificationError());
      });
  };

  const getAllAsync = async (input?: IGetNotificationsInput) => {
    dispatch(getAllNotificationsPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.type !== undefined) params.append('Type', input.type.toString());
    if (input?.priority !== undefined) params.append('Priority', input.priority.toString());
    if (input?.isRead !== undefined) params.append('IsRead', input.isRead.toString());
    if (input?.search) params.append('Search', input.search);

    const endpoint = `/api/services/app/Notification/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllNotificationsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllNotificationsError());
      });
  };

  const getUnreadCountAsync = async () => {
    dispatch(getUnreadCountPending());
    const endpoint = `/api/services/app/Notification/GetUnreadCount`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getUnreadCountSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getUnreadCountError());
      });
  };

  const markAsReadAsync = async (id: string) => {
    dispatch(markAsReadPending());
    const endpoint = `/api/services/app/Notification/MarkAsRead`;
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

  const markAllAsReadAsync = async () => {
    dispatch(markAllAsReadPending());
    const endpoint = `/api/services/app/Notification/MarkAllAsRead`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(markAllAsReadSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(markAllAsReadError());
      });
  };

  const createAsync = async (input: ICreateNotification) => {
    dispatch(createNotificationPending());
    const endpoint = `/api/services/app/Notification/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createNotificationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createNotificationError());
      });
  };

  return (
    <NotificationStateContext.Provider value={state}>
      <NotificationActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getUnreadCountAsync,
          markAsReadAsync,
          markAllAsReadAsync,
          createAsync,
        }}
      >
        {children}
      </NotificationActionContext.Provider>
    </NotificationStateContext.Provider>
  );
};

export const useNotificationState = () => {
  const context = useContext(NotificationStateContext);
  if (!context) {
    throw new Error("useNotificationState must be used within a NotificationProvider");
  }
  return context;
};

export const useNotificationActions = () => {
  const context = useContext(NotificationActionContext);
  if (!context) {
    throw new Error("useNotificationActions must be used within a NotificationProvider");
  }
  return context;
};
