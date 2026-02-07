'use client'
import { createContext } from "react";
import {
  INotification,
  INotificationList,
  ICreateNotification,
  IGetNotificationsInput
} from "../shared/interfaces";

export interface INotificationStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  notification?: INotification;
  notifications?: INotificationList[];
  totalCount?: number;
  unreadCount?: number;
}

export interface INotificationActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetNotificationsInput) => void;
  getUnreadCountAsync: () => void;
  markAsReadAsync: (id: string) => void;
  markAllAsReadAsync: () => void;
  createAsync: (input: ICreateNotification) => void;
}

export const INITIAL_STATE: INotificationStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const NotificationStateContext =
  createContext<INotificationStateContext>(INITIAL_STATE);

export const NotificationActionContext = createContext<
  INotificationActionContext | undefined
>(undefined);
