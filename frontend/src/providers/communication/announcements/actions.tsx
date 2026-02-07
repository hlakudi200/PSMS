import { createAction } from "redux-actions";
import { IAnnouncementStateContext } from "./context";
import { IAnnouncement, IAnnouncementList, IPagedResult } from "../shared/interfaces";

export enum AnnouncementActionEnums {
  getAnnouncementPending = "GET_ANNOUNCEMENT_PENDING",
  getAnnouncementSuccess = "GET_ANNOUNCEMENT_SUCCESS",
  getAnnouncementError = "GET_ANNOUNCEMENT_ERROR",

  getAllAnnouncementsPending = "GET_ALL_ANNOUNCEMENTS_PENDING",
  getAllAnnouncementsSuccess = "GET_ALL_ANNOUNCEMENTS_SUCCESS",
  getAllAnnouncementsError = "GET_ALL_ANNOUNCEMENTS_ERROR",

  createAnnouncementPending = "CREATE_ANNOUNCEMENT_PENDING",
  createAnnouncementSuccess = "CREATE_ANNOUNCEMENT_SUCCESS",
  createAnnouncementError = "CREATE_ANNOUNCEMENT_ERROR",

  updateAnnouncementPending = "UPDATE_ANNOUNCEMENT_PENDING",
  updateAnnouncementSuccess = "UPDATE_ANNOUNCEMENT_SUCCESS",
  updateAnnouncementError = "UPDATE_ANNOUNCEMENT_ERROR",

  deleteAnnouncementPending = "DELETE_ANNOUNCEMENT_PENDING",
  deleteAnnouncementSuccess = "DELETE_ANNOUNCEMENT_SUCCESS",
  deleteAnnouncementError = "DELETE_ANNOUNCEMENT_ERROR",

  publishAnnouncementPending = "PUBLISH_ANNOUNCEMENT_PENDING",
  publishAnnouncementSuccess = "PUBLISH_ANNOUNCEMENT_SUCCESS",
  publishAnnouncementError = "PUBLISH_ANNOUNCEMENT_ERROR",

  unpublishAnnouncementPending = "UNPUBLISH_ANNOUNCEMENT_PENDING",
  unpublishAnnouncementSuccess = "UNPUBLISH_ANNOUNCEMENT_SUCCESS",
  unpublishAnnouncementError = "UNPUBLISH_ANNOUNCEMENT_ERROR",

  pinAnnouncementPending = "PIN_ANNOUNCEMENT_PENDING",
  pinAnnouncementSuccess = "PIN_ANNOUNCEMENT_SUCCESS",
  pinAnnouncementError = "PIN_ANNOUNCEMENT_ERROR",

  unpinAnnouncementPending = "UNPIN_ANNOUNCEMENT_PENDING",
  unpinAnnouncementSuccess = "UNPIN_ANNOUNCEMENT_SUCCESS",
  unpinAnnouncementError = "UNPIN_ANNOUNCEMENT_ERROR",
}

// Get Single Announcement Actions
export const getAnnouncementPending = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.getAnnouncementPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAnnouncementSuccess = createAction<IAnnouncementStateContext, IAnnouncement>(
  AnnouncementActionEnums.getAnnouncementSuccess,
  (announcement: IAnnouncement) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    announcement,
  })
);

export const getAnnouncementError = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.getAnnouncementError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All Announcements Actions
export const getAllAnnouncementsPending = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.getAllAnnouncementsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllAnnouncementsSuccess = createAction<
  IAnnouncementStateContext,
  IPagedResult<IAnnouncementList>
>(
  AnnouncementActionEnums.getAllAnnouncementsSuccess,
  (result: IPagedResult<IAnnouncementList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    announcements: result.items,
    totalCount: result.totalCount,
  })
);

export const getAllAnnouncementsError = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.getAllAnnouncementsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Announcement Actions
export const createAnnouncementPending = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.createAnnouncementPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createAnnouncementSuccess = createAction<IAnnouncementStateContext, IAnnouncement>(
  AnnouncementActionEnums.createAnnouncementSuccess,
  (announcement: IAnnouncement) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    announcement,
  })
);

export const createAnnouncementError = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.createAnnouncementError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Announcement Actions
export const updateAnnouncementPending = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.updateAnnouncementPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateAnnouncementSuccess = createAction<IAnnouncementStateContext, IAnnouncement>(
  AnnouncementActionEnums.updateAnnouncementSuccess,
  (announcement: IAnnouncement) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    announcement,
  })
);

export const updateAnnouncementError = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.updateAnnouncementError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Announcement Actions
export const deleteAnnouncementPending = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.deleteAnnouncementPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteAnnouncementSuccess = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.deleteAnnouncementSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteAnnouncementError = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.deleteAnnouncementError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Publish Announcement Actions
export const publishAnnouncementPending = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.publishAnnouncementPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const publishAnnouncementSuccess = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.publishAnnouncementSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const publishAnnouncementError = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.publishAnnouncementError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Unpublish Announcement Actions
export const unpublishAnnouncementPending = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.unpublishAnnouncementPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const unpublishAnnouncementSuccess = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.unpublishAnnouncementSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const unpublishAnnouncementError = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.unpublishAnnouncementError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Pin Announcement Actions
export const pinAnnouncementPending = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.pinAnnouncementPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const pinAnnouncementSuccess = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.pinAnnouncementSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const pinAnnouncementError = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.pinAnnouncementError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Unpin Announcement Actions
export const unpinAnnouncementPending = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.unpinAnnouncementPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const unpinAnnouncementSuccess = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.unpinAnnouncementSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const unpinAnnouncementError = createAction<IAnnouncementStateContext>(
  AnnouncementActionEnums.unpinAnnouncementError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
