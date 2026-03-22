'use client'
import { createContext } from "react";

export interface IStudentTransfer {
  id: string;
  transferNumber: string;
  studentId: string;
  studentName: string;
  academicYearId: string;
  transferType: number;
  reason: string;
  status: number;
  requestedDate: string;
  effectiveDate?: string;
  fromSchoolName?: string;
  toSchoolName?: string;
  notes?: string;
  creationTime: string;
}

export interface IStudentTransferStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  studentTransfer?: IStudentTransfer;
  studentTransfers?: IStudentTransfer[];
  totalCount?: number;
}

export interface IStudentTransferActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: Record<string, unknown>) => void;
  createAsync: (input: Record<string, unknown>) => void;
  updateAsync: (id: string, input: Record<string, unknown>) => void;
  submitAsync: (id: string) => void;
  approveAsync: (id: string) => void;
  rejectAsync: (id: string, notes: string) => void;
  completeAsync: (id: string, certificateUrl?: string) => void;
  cancelAsync: (id: string) => void;
}

export const INITIAL_STATE: IStudentTransferStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const StudentTransferStateContext =
  createContext<IStudentTransferStateContext>(INITIAL_STATE);

export const StudentTransferActionContext = createContext<
  IStudentTransferActionContext | undefined
>(undefined);
