'use client'
import { createContext } from "react";
import type {
  IWorkflowInstance,
  IWorkflowInstanceList,
  IWorkflowTransition,
  IStartWorkflow,
  IAdvanceWorkflow,
  IBatchAdvance,
  IBatchAdvanceResult,
  IGetWorkflowInstancesInput,
  IPagedAndSortedResultRequest,
  IWorkflowEntitySummary,
} from "../shared/interfaces";

export interface IWorkflowInstanceStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  instance?: IWorkflowInstance;
  instances?: IWorkflowInstanceList[];
  totalCount?: number;
  history?: IWorkflowTransition[];
  batchResult?: IBatchAdvanceResult;
}

export interface IWorkflowInstanceActionContext {
  getAsync: (id: string) => void;
  getByEntityAsync: (entityType: number, entityId: string) => void;
  getAllAsync: (input?: IGetWorkflowInstancesInput) => void;
  startAsync: (input: IStartWorkflow) => void;
  advanceAsync: (id: string, input: IAdvanceWorkflow) => void;
  cancelAsync: (id: string, comment?: string) => void;
  recallAsync: (id: string, comment?: string) => void;
  batchAdvanceAsync: (input: IBatchAdvance) => void;
  getHistoryAsync: (instanceId: string) => void;
  getPendingForRoleAsync: (roleName: string, input?: IPagedAndSortedResultRequest) => void;
  getMyPendingAsync: (input?: IPagedAndSortedResultRequest) => void;
  getOverdueAsync: (input?: IPagedAndSortedResultRequest) => void;
  getEntitySummaryAsync: (instanceId: string) => Promise<IWorkflowEntitySummary | undefined>;
}

export const INITIAL_STATE: IWorkflowInstanceStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const WorkflowInstanceStateContext =
  createContext<IWorkflowInstanceStateContext>(INITIAL_STATE);

export const WorkflowInstanceActionContext = createContext<
  IWorkflowInstanceActionContext | undefined
>(undefined);
