'use client'
import { createContext } from "react";
import type {
  IWorkflowStep,
  ICreateWorkflowStep,
  IUpdateWorkflowStep,
  IReorderWorkflowSteps,
} from "../shared/interfaces";

export interface IWorkflowStepStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  step?: IWorkflowStep;
  steps?: IWorkflowStep[];
}

export interface IWorkflowStepActionContext {
  getAsync: (id: string) => void;
  getByDefinitionAsync: (definitionId: string) => void;
  createAsync: (input: ICreateWorkflowStep) => void;
  updateAsync: (id: string, input: IUpdateWorkflowStep) => void;
  deleteAsync: (id: string) => void;
  reorderAsync: (input: IReorderWorkflowSteps) => void;
}

export const INITIAL_STATE: IWorkflowStepStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const WorkflowStepStateContext =
  createContext<IWorkflowStepStateContext>(INITIAL_STATE);

export const WorkflowStepActionContext = createContext<
  IWorkflowStepActionContext | undefined
>(undefined);
