'use client'
import { createContext } from "react";
import {
  IWaitlist,
  IWaitlistPosition,
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";

export interface IWaitlistStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  waitlistEntry?: IWaitlist;
  waitlistEntries?: IWaitlist[];
  totalCount?: number;
  waitlistPosition?: IWaitlistPosition;
}

export interface IWaitlistActionContext {
  getAsync: (id: string) => void;
  getByApplicationAsync: (applicationId: string) => void;
  getByGradeAsync: (gradeId: string) => void;
  getAllAsync: (input: IPagedAndSortedResultRequest) => void;
  addToWaitlistAsync: (applicationId: string, notes?: string) => void;
  offerPositionAsync: (id: string, expiryDays?: number) => void;
  acceptOfferAsync: (id: string) => void;
  declineOfferAsync: (id: string) => void;
  withdrawAsync: (id: string) => void;
  getPositionAsync: (applicationId: string) => void;
}

export const INITIAL_STATE: IWaitlistStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const WaitlistStateContext =
  createContext<IWaitlistStateContext>(INITIAL_STATE);

export const WaitlistActionContext = createContext<
  IWaitlistActionContext | undefined
>(undefined);
