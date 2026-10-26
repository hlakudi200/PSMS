import { createAction } from "redux-actions";
import type { INotification, INotificationsStateContext } from "./context";

type NotificationsPatch = Partial<INotificationsStateContext>;

export enum NotificationsActionEnums {
  getPending = "NOTIFICATIONS_GET_PENDING",
  getSuccess = "NOTIFICATIONS_GET_SUCCESS",
  getError = "NOTIFICATIONS_GET_ERROR",
  loadMorePending = "NOTIFICATIONS_LOAD_MORE_PENDING",
  loadMoreDone = "NOTIFICATIONS_LOAD_MORE_DONE",
  readChanged = "NOTIFICATIONS_READ_CHANGED",
  markAllPending = "NOTIFICATIONS_MARK_ALL_PENDING",
  markAllDone = "NOTIFICATIONS_MARK_ALL_DONE",
  unreadCountSuccess = "NOTIFICATIONS_UNREAD_COUNT_SUCCESS",
}

export const getPending = createAction<NotificationsPatch>(NotificationsActionEnums.getPending, () => ({
  isPending: true, isSuccess: false, isError: false,
}));

export const getSuccess = createAction<NotificationsPatch, { notifications: INotification[]; totalCount: number }>(
  NotificationsActionEnums.getSuccess,
  ({ notifications, totalCount }) => ({ isPending: false, isSuccess: true, isError: false, notifications, totalCount })
);

export const getError = createAction<NotificationsPatch>(NotificationsActionEnums.getError, () => ({
  isPending: false, isSuccess: false, isError: true,
}));

export const loadMorePending = createAction<NotificationsPatch>(NotificationsActionEnums.loadMorePending, () => ({
  isLoadingMore: true,
}));

/** A failed next page keeps what has loaded; reaching the end again retries. */
export const loadMoreDone = createAction<NotificationsPatch, { notifications?: INotification[]; totalCount?: number }>(
  NotificationsActionEnums.loadMoreDone,
  ({ notifications, totalCount }) => ({
    isLoadingMore: false,
    ...(notifications && { notifications }),
    ...(totalCount != null && { totalCount }),
  })
);

/** One row and the count, moved together: optimistically, or back again on a refusal. */
export const readChanged = createAction<
  NotificationsPatch,
  { notifications?: INotification[]; unreadCount?: number; actionErrorMessage?: string }
>(NotificationsActionEnums.readChanged, ({ notifications, unreadCount, actionErrorMessage }) => ({
  actionErrorMessage,
  ...(notifications && { notifications }),
  ...(unreadCount != null && { unreadCount }),
}));

export const markAllPending = createAction<NotificationsPatch>(NotificationsActionEnums.markAllPending, () => ({
  isMarkingAll: true, actionErrorMessage: undefined,
}));

export const markAllDone = createAction<
  NotificationsPatch,
  { notifications?: INotification[]; unreadCount?: number; actionErrorMessage?: string }
>(NotificationsActionEnums.markAllDone, ({ notifications, unreadCount, actionErrorMessage }) => ({
  isMarkingAll: false,
  actionErrorMessage,
  ...(notifications && { notifications }),
  ...(unreadCount != null && { unreadCount }),
}));

export const unreadCountSuccess = createAction<NotificationsPatch, number>(
  NotificationsActionEnums.unreadCountSuccess,
  (unreadCount) => ({ unreadCount })
);
