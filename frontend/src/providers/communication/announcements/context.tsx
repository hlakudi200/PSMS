'use client'
import { createContext } from "react";
import {
  IAnnouncement,
  IAnnouncementList,
  ICreateAnnouncement,
  IUpdateAnnouncement,
  IGetAnnouncementsInput
} from "../shared/interfaces";

export interface IAnnouncementStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  announcement?: IAnnouncement;
  announcements?: IAnnouncementList[];
  totalCount?: number;
}

export interface IAnnouncementActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetAnnouncementsInput) => void;
  createAsync: (input: ICreateAnnouncement) => void;
  updateAsync: (id: string, input: IUpdateAnnouncement) => void;
  deleteAsync: (id: string) => void;
  publishAsync: (id: string) => void;
  unpublishAsync: (id: string) => void;
  pinAsync: (id: string) => void;
  unpinAsync: (id: string) => void;
}

export const INITIAL_STATE: IAnnouncementStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const AnnouncementStateContext =
  createContext<IAnnouncementStateContext>(INITIAL_STATE);

export const AnnouncementActionContext = createContext<
  IAnnouncementActionContext | undefined
>(undefined);
