'use client'
import { createContext } from "react";
import {
  IAdmissionAssessment,
  IAssessor,
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
  /** Only ever the signed-in person's own; being named the assessor is the authority. */
  getMineAsync: (includePast?: boolean) => Promise<IAdmissionAssessment[]>;
  getAssessorsAsync: () => Promise<IAssessor[]>;
  scheduleAsync: (input: IScheduleAssessment) => Promise<IAdmissionAssessment>;
  recordResultsAsync: (id: string, input: IRecordAssessmentResults) => Promise<void>;
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
