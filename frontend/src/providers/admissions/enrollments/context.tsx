'use client'
import { createContext } from "react";
import {
  IEnrollment,
  IAcceptOffer,
  IAssignClass,
  ICompleteEnrollment,
  IClassAvailability,
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";

export interface IEnrollmentStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  enrollment?: IEnrollment;
  enrollments?: IEnrollment[];
  totalCount?: number;
  availableClasses?: IClassAvailability[];
}

export interface IEnrollmentActionContext {
  getByApplicationAsync: (applicationId: string) => void;
  getPendingEnrollmentsAsync: (input: IPagedAndSortedResultRequest) => void;
  getAvailableClassesAsync: (gradeId: string) => void;
  acceptOfferAsync: (input: IAcceptOffer) => void;
  assignClassAsync: (input: IAssignClass) => void;
  completeEnrollmentAsync: (input: ICompleteEnrollment) => void;
}

export const INITIAL_STATE: IEnrollmentStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const EnrollmentStateContext =
  createContext<IEnrollmentStateContext>(INITIAL_STATE);

export const EnrollmentActionContext = createContext<
  IEnrollmentActionContext | undefined
>(undefined);
