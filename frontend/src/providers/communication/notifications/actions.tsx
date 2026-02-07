import { createAction } from "redux-actions";
import { INotificationStateContext } from "./context";
import { INotification, INotificationList, IPagedResult } from "../shared/interfaces";

export enum NotificationActionEnums {
  getNotificationPending = "GET_NOTIFICATION_PENDING",
  getNotificationSuccess = "GET_NOTIFICATION_SUCCESS",
  getNotificationError = "GET_NOTIFICATION_ERROR",

  getAllNotificationsPending = "GET_ALL_NOTIFICATIONS_PENDING",
  getAllNotificationsSuccess = "GET_ALL_NOTIFICATIONS_SUCCESS",
  getAllNotificationsError = "GET_ALL_NOTIFICATIONS_ERROR",

  getUnreadCountPending = "GET_UNREAD_COUNT_PENDING",
  getUnreadCountSuccess = "GET_UNREAD_COUNT_SUCCESS",
  getUnreadCountError = "GET_UNREAD_COUNT_ERROR",

  markAsReadPending = "MARK_AS_READ_PENDING",
  markAsReadSuccess = "MARK_AS_READ_SUCCESS",
  markAsReadError = "MARK_AS_READ_ERROR",

  markAllAsReadPending = "MARK_ALL_AS_READ_PENDING",
  markAllAsReadSuccess = "MARK_ALL_AS_READ_SUCCESS",
  markAllAsReadError = "MARK_ALL_AS_READ_ERROR",

  createNotificationPending = "CREATE_NOTIFICATION_PENDING",
  createNotificationSuccess = "CREATE_NOTIFICATION_SUCCESS",
  createNotificationError = "CREATE_NOTIFICATION_ERROR",
}

// Get Single Notification Actions
export const getNotificationPending = createAction<INotificationStateContext>(
  NotificationActionEnums.getNotificationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getNotificationSuccess = createAction<INotificationStateContext, INotification>(
  NotificationActionEnums.getNotificationSuccess,
  (notification: INotification) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    notification,
  })
);

export const getNotificationError = createAction<INotificationStateContext>(
  NotificationActionEnums.getNotificationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All Notifications Actions
export const getAllNotificationsPending = createAction<INotificationStateContext>(
  NotificationActionEnums.getAllNotificationsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllNotificationsSuccess = createAction<
  INotificationStateContext,
  IPagedResult<INotificationList>
>(
  NotificationActionEnums.getAllNotificationsSuccess,
  (result: IPagedResult<INotificationList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    notifications: result.items,
    totalCount: result.totalCount,
  })
);

export const getAllNotificationsError = createAction<INotificationStateContext>(
  NotificationActionEnums.getAllNotificationsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Unread Count Actions
export const getUnreadCountPending = createAction<INotificationStateContext>(
  NotificationActionEnums.getUnreadCountPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getUnreadCountSuccess = createAction<INotificationStateContext, number>(
  NotificationActionEnums.getUnreadCountSuccess,
  (unreadCount: number) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    unreadCount,
  })
);

export const getUnreadCountError = createAction<INotificationStateContext>(
  NotificationActionEnums.getUnreadCountError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Mark As Read Actions
export const markAsReadPending = createAction<INotificationStateContext>(
  NotificationActionEnums.markAsReadPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const markAsReadSuccess = createAction<INotificationStateContext>(
  NotificationActionEnums.markAsReadSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const markAsReadError = createAction<INotificationStateContext>(
  NotificationActionEnums.markAsReadError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Mark All As Read Actions
export const markAllAsReadPending = createAction<INotificationStateContext>(
  NotificationActionEnums.markAllAsReadPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const markAllAsReadSuccess = createAction<INotificationStateContext>(
  NotificationActionEnums.markAllAsReadSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const markAllAsReadError = createAction<INotificationStateContext>(
  NotificationActionEnums.markAllAsReadError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Notification Actions
export const createNotificationPending = createAction<INotificationStateContext>(
  NotificationActionEnums.createNotificationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createNotificationSuccess = createAction<INotificationStateContext, INotification>(
  NotificationActionEnums.createNotificationSuccess,
  (notification: INotification) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    notification,
  })
);

export const createNotificationError = createAction<INotificationStateContext>(
  NotificationActionEnums.createNotificationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
