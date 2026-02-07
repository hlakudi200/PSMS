'use client'
import { createContext } from "react";
import {
  IPayment,
  IPaymentList,
  ICreatePayment,
  IUpdatePayment,
  IGetPaymentsInput
} from "../shared/interfaces";

export interface IPaymentStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  payment?: IPayment;
  payments?: IPaymentList[];
  totalCount?: number;
}

export interface IPaymentActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetPaymentsInput) => void;
  getByStudentAsync: (studentId: string) => void;
  getByParentAsync: (parentId: string) => void;
  createAsync: (input: ICreatePayment) => void;
  updateAsync: (id: string, input: IUpdatePayment) => void;
  completeAsync: (id: string) => void;
  failAsync: (id: string) => void;
  refundAsync: (id: string) => void;
  cancelAsync: (id: string) => void;
  getByReceiptNumberAsync: (receiptNumber: string) => void;
}

export const INITIAL_STATE: IPaymentStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const PaymentStateContext =
  createContext<IPaymentStateContext>(INITIAL_STATE);

export const PaymentActionContext = createContext<
  IPaymentActionContext | undefined
>(undefined);
