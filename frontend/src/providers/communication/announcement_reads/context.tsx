'use client'
import { createContext } from "react";
import { IAnnouncementRead } from "../shared/interfaces";

export interface IAnnouncementReadStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  announcementReads?: IAnnouncementRead[];
  unreadCount?: number;
}

export interface IAnnouncementReadActionContext {
  markAsReadAsync: (announcementId: string) => void;
  getByAnnouncementAsync: (announcementId: string) => void;
  getUnreadCountAsync: () => void;
}

export const INITIAL_STATE: IAnnouncementReadStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const AnnouncementReadStateContext =
  createContext<IAnnouncementReadStateContext>(INITIAL_STATE);

export const AnnouncementReadActionContext = createContext<
  IAnnouncementReadActionContext | undefined
>(undefined);
