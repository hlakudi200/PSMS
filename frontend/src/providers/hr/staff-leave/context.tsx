'use client'
import { createContext } from "react";

export interface IStaffLeaveRequest {
  id: string;
  leaveNumber: string;
  userId: number;
  userName: string;
  leaveType: number;
  startDate: string;
  endDate: string;
  totalDays: number;
  reason: string;
  status: number;
  rejectionReason?: string;
  creationTime: string;
}

export interface IStaffLeaveRequestStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  staffLeaveRequest?: IStaffLeaveRequest;
  staffLeaveRequests?: IStaffLeaveRequest[];
  totalCount?: number;
}

export interface IStaffLeaveRequestActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: Record<string, unknown>) => void;
  createAsync: (input: Record<string, unknown>) => void;
  updateAsync: (id: string, input: Record<string, unknown>) => void;
  submitAsync: (id: string) => void;
  approveAsync: (id: string) => void;
  rejectAsync: (id: string, reason: string) => void;
  cancelAsync: (id: string) => void;
}

export const INITIAL_STATE: IStaffLeaveRequestStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const StaffLeaveRequestStateContext =
  createContext<IStaffLeaveRequestStateContext>(INITIAL_STATE);

export const StaffLeaveRequestActionContext = createContext<
  IStaffLeaveRequestActionContext | undefined
>(undefined);
