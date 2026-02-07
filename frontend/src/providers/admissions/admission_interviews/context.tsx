'use client'
import { createContext } from "react";
import {
  IAdmissionInterview,
  IScheduleInterview,
  IRescheduleInterview,
  ICompleteInterview,
  ITimeSlot,
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";

export interface IAdmissionInterviewStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  interview?: IAdmissionInterview;
  interviews?: IAdmissionInterview[];
  totalCount?: number;
  timeSlots?: ITimeSlot[];
}

export interface IAdmissionInterviewActionContext {
  getAsync: (id: string) => void;
  getByApplicationAsync: (applicationId: string) => void;
  getAllAsync: (input: IPagedAndSortedResultRequest) => void;
  scheduleAsync: (input: IScheduleInterview) => void;
  rescheduleAsync: (id: string, input: IRescheduleInterview) => void;
  cancelAsync: (id: string, reason: string) => void;
  completeAsync: (id: string, input: ICompleteInterview) => void;
  markNoShowAsync: (id: string) => void;
  getAvailableTimeSlotsAsync: (date: string, interviewerUserId: number) => void;
}

export const INITIAL_STATE: IAdmissionInterviewStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const AdmissionInterviewStateContext =
  createContext<IAdmissionInterviewStateContext>(INITIAL_STATE);

export const AdmissionInterviewActionContext = createContext<
  IAdmissionInterviewActionContext | undefined
>(undefined);
