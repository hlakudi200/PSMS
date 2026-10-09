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
  /** How much is owed, what state it is in, and whether there is anything to click. */
  getCheckoutAsync: (applicationId: string) => Promise<IFeeCheckout>;
  /** Settles it through the stand-in gateway. No money moves. */
  simulatePaymentAsync: (applicationId: string) => Promise<void>;
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


/** Mirrors ApplicationFeeCheckoutDto. */
export interface IFeeCheckout {
  applicationId: string;
  applicationNumber: string;
  feeRequired: boolean;
  amount: number;
  currency: string;
  status: number;
  paymentReference?: string;
  receiptNumber?: string;
  paymentDate?: string;
  awaitingPayment: boolean;
  gatewayMode: number;
  /** True while the deployment uses the stand-in gateway. The screen must say so. */
  isSimulated: boolean;
}
