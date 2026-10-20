import { createContext } from "react";
import type { NotificationType } from "../../utils/notification-centre";

/** Mirrors the backend NotificationPriority. */
export type NotificationPriority = 1 | 2 | 3 | 4; // Low | Normal | High | Critical

export interface INotification {
  id: string;
  title: string;
  message: string;
  type: NotificationType;
  priority: NotificationPriority;
  /** What it is about, for opening it in the app (#201). */
  entityType?: string;
  entityId?: string;
  isRead: boolean;
  creationTime: string;
}

export interface INotificationsStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  /** Newest first. Only the caller's own: the server scopes every read to the signed-in user. */
  notifications?: INotification[];
  totalCount?: number;
  isLoadingMore: boolean;
  /** For Home and the tab badge. Undefined until it first loads. */
  unreadCount?: number;
  isMarkingAll: boolean;
  actionErrorMessage?: string;
}

export interface INotificationsActionContext {
  /** The first page, replacing what was there, then the unread count. */
  getNotificationsAsync: () => Promise<void>;
  /** The next page, appended. Does nothing when everything has loaded. */
  loadMoreAsync: () => Promise<void>;
  /** Marks one read straight away, and puts it back if the server refuses. */
  markAsReadAsync: (id: string) => Promise<void>;
  markAllAsReadAsync: () => Promise<void>;
  getUnreadCountAsync: () => Promise<void>;
}

export const INITIAL_STATE: INotificationsStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
  isLoadingMore: false,
  isMarkingAll: false,
};

export const NotificationsStateContext = createContext<INotificationsStateContext>(INITIAL_STATE);
export const NotificationsActionContext = createContext<INotificationsActionContext | undefined>(undefined);
