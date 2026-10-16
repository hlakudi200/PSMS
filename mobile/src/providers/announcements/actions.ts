import { createAction } from "redux-actions";
import type { IAnnouncement, IAnnouncementsStateContext, IAnnouncementSummary } from "./context";

type AnnouncementsPatch = Partial<IAnnouncementsStateContext>;

export enum AnnouncementsActionEnums {
  getPending = "ANNOUNCEMENTS_GET_PENDING",
  getSuccess = "ANNOUNCEMENTS_GET_SUCCESS",
  getError = "ANNOUNCEMENTS_GET_ERROR",
  loadMorePending = "ANNOUNCEMENTS_LOAD_MORE_PENDING",
  loadMoreDone = "ANNOUNCEMENTS_LOAD_MORE_DONE",
  detailPending = "ANNOUNCEMENTS_DETAIL_PENDING",
  detailSuccess = "ANNOUNCEMENTS_DETAIL_SUCCESS",
  detailError = "ANNOUNCEMENTS_DETAIL_ERROR",
  markedRead = "ANNOUNCEMENTS_MARKED_READ",
  unreadCountSuccess = "ANNOUNCEMENTS_UNREAD_COUNT_SUCCESS",
}

export const getPending = createAction<AnnouncementsPatch>(AnnouncementsActionEnums.getPending, () => ({
  isPending: true, isSuccess: false, isError: false,
}));

export const getSuccess = createAction<AnnouncementsPatch, { announcements: IAnnouncementSummary[]; totalCount: number }>(
  AnnouncementsActionEnums.getSuccess,
  ({ announcements, totalCount }) => ({ isPending: false, isSuccess: true, isError: false, announcements, totalCount })
);

export const getError = createAction<AnnouncementsPatch>(AnnouncementsActionEnums.getError, () => ({
  isPending: false, isSuccess: false, isError: true,
}));

export const loadMorePending = createAction<AnnouncementsPatch>(AnnouncementsActionEnums.loadMorePending, () => ({
  isLoadingMore: true,
}));

/** A failed next page keeps what has loaded; scrolling to the end again retries. */
export const loadMoreDone = createAction<AnnouncementsPatch, { announcements?: IAnnouncementSummary[]; totalCount?: number }>(
  AnnouncementsActionEnums.loadMoreDone,
  ({ announcements, totalCount }) => ({
    isLoadingMore: false,
    ...(announcements && { announcements }),
    ...(totalCount != null && { totalCount }),
  })
);

export const detailPending = createAction<AnnouncementsPatch>(AnnouncementsActionEnums.detailPending, () => ({
  isDetailPending: true, isDetailError: false,
}));

export const detailSuccess = createAction<AnnouncementsPatch, IAnnouncement>(
  AnnouncementsActionEnums.detailSuccess,
  (detail) => ({ isDetailPending: false, isDetailError: false, detail })
);

export const detailError = createAction<AnnouncementsPatch>(AnnouncementsActionEnums.detailError, () => ({
  isDetailPending: false, isDetailError: true,
}));

/** The detail, the feed row and the count all move together once the server has recorded the read. */
export const markedRead = createAction<
  AnnouncementsPatch,
  { detail?: IAnnouncement; announcements?: IAnnouncementSummary[]; unreadCount?: number }
>(AnnouncementsActionEnums.markedRead, (patch) => patch);

export const unreadCountSuccess = createAction<AnnouncementsPatch, number>(
  AnnouncementsActionEnums.unreadCountSuccess,
  (unreadCount) => ({ unreadCount })
);
