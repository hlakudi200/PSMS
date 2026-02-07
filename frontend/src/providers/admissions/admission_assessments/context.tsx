'use client'
import { createContext } from "react";
import {
  IAdmissionAssessment,
  IScheduleAssessment,
  IRecordAssessmentResults,
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";

export interface IAdmissionAssessmentStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  assessment?: IAdmissionAssessment;
  assessments?: IAdmissionAssessment[];
  totalCount?: number;
}

export interface IAdmissionAssessmentActionContext {
  getAsync: (id: string) => void;
  getByApplicationAsync: (applicationId: string) => void;
  getAllAsync: (input?: IPagedAndSortedResultRequest) => void;
  scheduleAsync: (input: IScheduleAssessment) => void;
  recordResultsAsync: (id: string, input: IRecordAssessmentResults) => void;
  cancelAsync: (id: string, reason: string) => void;
}

export const INITIAL_STATE: IAdmissionAssessmentStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const AdmissionAssessmentStateContext =
  createContext<IAdmissionAssessmentStateContext>(INITIAL_STATE);

export const AdmissionAssessmentActionContext = createContext<
  IAdmissionAssessmentActionContext | undefined
>(undefined);
