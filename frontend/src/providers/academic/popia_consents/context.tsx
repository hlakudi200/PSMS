'use client'
import { createContext } from "react";
import {
  IPOPIAConsent,
  ICreatePOPIAConsent,
  IUpdatePOPIAConsent,
  IGetPOPIAConsentsInput
} from "../shared/interfaces";

export interface IPOPIAConsentStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  popiaConsent?: IPOPIAConsent;
  popiaConsents?: IPOPIAConsent[];
  totalCount?: number;
}

export interface IPOPIAConsentActionContext {
  getByStudentAsync: (studentId: string) => void;
  getAllAsync: (input?: IGetPOPIAConsentsInput) => void;
  createAsync: (input: ICreatePOPIAConsent) => void;
  updateAsync: (id: string, input: IUpdatePOPIAConsent) => void;
  revokeAsync: (id: string) => void;
}

export const INITIAL_STATE: IPOPIAConsentStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const POPIAConsentStateContext =
  createContext<IPOPIAConsentStateContext>(INITIAL_STATE);

export const POPIAConsentActionContext = createContext<
  IPOPIAConsentActionContext | undefined
>(undefined);
