'use client'
import { createContext } from "react";
import {
  IEnrollment,
  IAcceptOffer,
  IDeclineOffer,
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
  getAvailableClassesAsync: (gradeId: string) => Promise<IClassAvailability[]>;
  acceptOfferAsync: (input: IAcceptOffer) => Promise<IEnrollment>;
  declineOfferAsync: (input: IDeclineOffer) => Promise<IEnrollment>;
  assignClassAsync: (input: IAssignClass) => void;
  completeEnrollmentAsync: (input: ICompleteEnrollment) => Promise<IEnrollment>;
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
