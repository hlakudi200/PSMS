'use client'
import { createContext } from "react";
import {
  IApplicationFee,
  IPaymentResult,
  IRecordPayment,
  IPaymentCallback,
} from "../shared/interfaces";

export interface IApplicationFeeStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  applicationFee?: IApplicationFee;
  paymentResult?: IPaymentResult;
}

export interface IApplicationFeeActionContext {
  getByApplicationAsync: (applicationId: string) => void;
  recordPaymentAsync: (applicationId: string, input: IRecordPayment) => void;
  processPaymentCallbackAsync: (input: IPaymentCallback) => void;
  getPaymentStatusAsync: (applicationId: string) => void;
}

export const INITIAL_STATE: IApplicationFeeStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const ApplicationFeeStateContext =
  createContext<IApplicationFeeStateContext>(INITIAL_STATE);

export const ApplicationFeeActionContext = createContext<
  IApplicationFeeActionContext | undefined
>(undefined);
