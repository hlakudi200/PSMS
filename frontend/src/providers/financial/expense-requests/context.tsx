'use client'
import { createContext } from "react";

export interface IExpenseRequest {
  id: string;
  requestNumber: string;
  academicYearId: string;
  requestedByName: string;
  category: number;
  description: string;
  amount: number;
  approvedAmount?: number;
  priority: number;
  status: number;
  rejectionReason?: string;
  paymentReference?: string;
  requiredByDate?: string;
  creationTime: string;
}

export interface IExpenseRequestStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  expenseRequest?: IExpenseRequest;
  expenseRequests?: IExpenseRequest[];
  totalCount?: number;
}

export interface IExpenseRequestActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: Record<string, unknown>) => void;
  createAsync: (input: Record<string, unknown>) => void;
  updateAsync: (id: string, input: Record<string, unknown>) => void;
  submitAsync: (id: string) => void;
  approveAsync: (id: string, amount: number) => void;
  rejectAsync: (id: string, reason: string) => void;
  cancelAsync: (id: string) => void;
  markAsPaidAsync: (id: string, reference: string) => void;
}

export const INITIAL_STATE: IExpenseRequestStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const ExpenseRequestStateContext =
  createContext<IExpenseRequestStateContext>(INITIAL_STATE);

export const ExpenseRequestActionContext = createContext<
  IExpenseRequestActionContext | undefined
>(undefined);
