'use client'
import { createContext } from "react";

export interface IFeeWaiver {
  id: string;
  studentId: string;
  studentName: string;
  academicYearId: string;
  academicYearName: string;
  waiverType: number;
  requestedAmount: number;
  approvedAmount?: number;
  reason: string;
  status: number;
  notes?: string;
  creationTime: string;
}

export interface IFeeWaiverStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  feeWaiver?: IFeeWaiver;
  feeWaivers?: IFeeWaiver[];
  totalCount?: number;
}

export interface IFeeWaiverActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: Record<string, unknown>) => void;
  createAsync: (input: Record<string, unknown>) => void;
  updateAsync: (id: string, input: Record<string, unknown>) => void;
  deleteAsync: (id: string) => void;
  submitAsync: (id: string) => void;
  approveAsync: (id: string, amount: number, notes: string) => void;
  rejectAsync: (id: string, notes: string) => void;
}

export const INITIAL_STATE: IFeeWaiverStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const FeeWaiverStateContext =
  createContext<IFeeWaiverStateContext>(INITIAL_STATE);

export const FeeWaiverActionContext = createContext<
  IFeeWaiverActionContext | undefined
>(undefined);
