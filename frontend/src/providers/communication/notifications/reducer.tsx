import { handleActions } from "redux-actions";
import { INITIAL_STATE, INotificationStateContext } from "./context";
import { NotificationActionEnums } from "./actions";

export const NotificationReducer = handleActions<
  INotificationStateContext,
  INotificationStateContext
>(
  {
    [NotificationActionEnums.getNotificationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.getNotificationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.getNotificationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.getAllNotificationsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.getAllNotificationsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.getAllNotificationsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.getUnreadCountPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.getUnreadCountSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.getUnreadCountError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.markAsReadPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.markAsReadSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.markAsReadError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.markAllAsReadPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.markAllAsReadSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.markAllAsReadError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.createNotificationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.createNotificationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [NotificationActionEnums.createNotificationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
