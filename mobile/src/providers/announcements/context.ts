import { createContext } from "react";
import type { AnnouncementAudience, AnnouncementPriority, AnnouncementType } from "../../utils/announcement-labels";

export interface IAnnouncementSummary {
  id: string;
  title: string;
  type: AnnouncementType;
  priority: AnnouncementPriority;
  targetAudience: AnnouncementAudience;
  publishDate: string;
  expiryDate?: string;
  isPinned: boolean;
  isReadByMe: boolean;
}

export interface IAnnouncement extends IAnnouncementSummary {
  content: string;
  attachmentUrl?: string;
}

export interface IAnnouncementsStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  /** Pinned first, then newest. Only what the server says is addressed to this family. */
  announcements?: IAnnouncementSummary[];
  totalCount?: number;
  isLoadingMore: boolean;
  /** Unread announcements, for Home and the tab badge. Undefined until it first loads. */
  unreadCount?: number;
  detail?: IAnnouncement;
  isDetailPending: boolean;
  isDetailError: boolean;
}

export interface IAnnouncementsActionContext {
  /** The first page of the feed, replacing what was there. */
  getAnnouncementsAsync: () => Promise<void>;
  /** The next page, appended. Does nothing when everything has loaded. */
  loadMoreAsync: () => Promise<void>;
  /** Loads one announcement and, if it is unread, marks it read. */
  openAnnouncementAsync: (id: string) => Promise<void>;
  getUnreadCountAsync: () => Promise<void>;
}

export const INITIAL_STATE: IAnnouncementsStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
  isLoadingMore: false,
  isDetailPending: false,
  isDetailError: false,
};

export const AnnouncementsStateContext = createContext<IAnnouncementsStateContext>(INITIAL_STATE);
export const AnnouncementsActionContext = createContext<IAnnouncementsActionContext | undefined>(undefined);
