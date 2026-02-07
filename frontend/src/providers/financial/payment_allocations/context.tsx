'use client'
import { createContext } from "react";
import {
  IPaymentAllocation,
  IPaymentAllocationList,
  ICreatePaymentAllocation,
  IBulkAllocatePayment,
  IUpdatePaymentAllocation
} from "../shared/interfaces";

export interface IPaymentAllocationStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  paymentAllocation?: IPaymentAllocation;
  paymentAllocations?: IPaymentAllocationList[];
}

export interface IPaymentAllocationActionContext {
  getAsync: (id: string) => void;
  getByPaymentAsync: (paymentId: string) => void;
  getByStudentFeeAsync: (studentFeeId: string) => void;
  createAsync: (input: ICreatePaymentAllocation) => void;
  bulkAllocateAsync: (input: IBulkAllocatePayment) => void;
  updateAsync: (id: string, input: IUpdatePaymentAllocation) => void;
  deleteAsync: (id: string) => void;
}

export const INITIAL_STATE: IPaymentAllocationStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const PaymentAllocationStateContext =
  createContext<IPaymentAllocationStateContext>(INITIAL_STATE);

export const PaymentAllocationActionContext = createContext<
  IPaymentAllocationActionContext | undefined
>(undefined);
