'use client'
import { createContext } from "react";
import type {
  ICloneWorkflowDefinition,
  IWorkflowDefinition,
  IWorkflowDefinitionList,
  ICreateWorkflowDefinition,
  IUpdateWorkflowDefinition,
  IGetWorkflowDefinitionsInput,
} from "../shared/interfaces";

export interface IWorkflowDefinitionStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  definition?: IWorkflowDefinition;
  definitions?: IWorkflowDefinitionList[];
  totalCount?: number;
}

export interface IWorkflowDefinitionActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetWorkflowDefinitionsInput) => void;
  createAsync: (input: ICreateWorkflowDefinition) => void;
  updateAsync: (id: string, input: IUpdateWorkflowDefinition) => void;
  deleteAsync: (id: string) => void;
  activateAsync: (id: string) => void;
  deactivateAsync: (id: string) => void;
  cloneAsync: (id: string, input?: ICloneWorkflowDefinition) => Promise<IWorkflowDefinition | undefined>;
}

export const INITIAL_STATE: IWorkflowDefinitionStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const WorkflowDefinitionStateContext =
  createContext<IWorkflowDefinitionStateContext>(INITIAL_STATE);

export const WorkflowDefinitionActionContext = createContext<
  IWorkflowDefinitionActionContext | undefined
>(undefined);
