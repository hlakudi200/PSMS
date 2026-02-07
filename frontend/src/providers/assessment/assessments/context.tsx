'use client'
import { createContext } from "react";
import {
  IAssessment,
  IAssessmentList,
  ICreateAssessment,
  IUpdateAssessment,
  IGetAssessmentsInput,
} from "../shared/interfaces";

export interface IAssessmentStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  assessment?: IAssessment;
  assessments?: IAssessmentList[];
  totalCount?: number;
}

export interface IAssessmentActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetAssessmentsInput) => void;
  createAsync: (input: ICreateAssessment) => void;
  updateAsync: (id: string, input: IUpdateAssessment) => void;
  deleteAsync: (id: string) => void;
  publishAsync: (id: string) => void;
  unpublishAsync: (id: string) => void;
  releaseMarksAsync: (id: string) => void;
}

export const INITIAL_STATE: IAssessmentStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const AssessmentStateContext =
  createContext<IAssessmentStateContext>(INITIAL_STATE);

export const AssessmentActionContext = createContext<
  IAssessmentActionContext | undefined
>(undefined);
