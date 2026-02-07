import { createAction } from "redux-actions";
import { IAnnouncementReadStateContext } from "./context";
import { IAnnouncementRead, IListResult } from "../shared/interfaces";

export enum AnnouncementReadActionEnums {
  markAsReadPending = "MARK_AS_READ_PENDING",
  markAsReadSuccess = "MARK_AS_READ_SUCCESS",
  markAsReadError = "MARK_AS_READ_ERROR",

  getByAnnouncementPending = "GET_BY_ANNOUNCEMENT_PENDING",
  getByAnnouncementSuccess = "GET_BY_ANNOUNCEMENT_SUCCESS",
  getByAnnouncementError = "GET_BY_ANNOUNCEMENT_ERROR",

  getUnreadCountPending = "GET_UNREAD_COUNT_PENDING",
  getUnreadCountSuccess = "GET_UNREAD_COUNT_SUCCESS",
  getUnreadCountError = "GET_UNREAD_COUNT_ERROR",
}

// Mark As Read Actions
export const markAsReadPending = createAction<IAnnouncementReadStateContext>(
  AnnouncementReadActionEnums.markAsReadPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const markAsReadSuccess = createAction<IAnnouncementReadStateContext>(
  AnnouncementReadActionEnums.markAsReadSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const markAsReadError = createAction<IAnnouncementReadStateContext>(
  AnnouncementReadActionEnums.markAsReadError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Announcement Actions
export const getByAnnouncementPending = createAction<IAnnouncementReadStateContext>(
  AnnouncementReadActionEnums.getByAnnouncementPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByAnnouncementSuccess = createAction<
  IAnnouncementReadStateContext,
  IListResult<IAnnouncementRead>
>(
  AnnouncementReadActionEnums.getByAnnouncementSuccess,
  (result: IListResult<IAnnouncementRead>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    announcementReads: result.items,
  })
);

export const getByAnnouncementError = createAction<IAnnouncementReadStateContext>(
  AnnouncementReadActionEnums.getByAnnouncementError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Unread Count Actions
export const getUnreadCountPending = createAction<IAnnouncementReadStateContext>(
  AnnouncementReadActionEnums.getUnreadCountPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getUnreadCountSuccess = createAction<
  IAnnouncementReadStateContext,
  number
>(
  AnnouncementReadActionEnums.getUnreadCountSuccess,
  (unreadCount: number) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    unreadCount,
  })
);

export const getUnreadCountError = createAction<IAnnouncementReadStateContext>(
  AnnouncementReadActionEnums.getUnreadCountError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
