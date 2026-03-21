'use client'
import { createContext } from "react";
import type {
  IWorkflowDelegation,
  ICreateWorkflowDelegation,
  IGetWorkflowDelegationsInput,
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";

export interface IWorkflowDelegationStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  delegation?: IWorkflowDelegation;
  delegations?: IWorkflowDelegation[];
  totalCount?: number;
}

export interface IWorkflowDelegationActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetWorkflowDelegationsInput) => void;
  getMyDelegationsAsync: (input?: IPagedAndSortedResultRequest) => void;
  createAsync: (input: ICreateWorkflowDelegation) => void;
  revokeAsync: (id: string) => void;
}

export const INITIAL_STATE: IWorkflowDelegationStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const WorkflowDelegationStateContext =
  createContext<IWorkflowDelegationStateContext>(INITIAL_STATE);

export const WorkflowDelegationActionContext = createContext<
  IWorkflowDelegationActionContext | undefined
>(undefined);
