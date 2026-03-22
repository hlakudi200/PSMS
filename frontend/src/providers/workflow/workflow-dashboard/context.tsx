'use client'
import { createContext } from "react";
import type {
  IWorkflowDashboard,
  IWorkflowActivity,
} from "../shared/interfaces";

export interface IWorkflowDashboardStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  dashboard?: IWorkflowDashboard;
  recentActivity?: IWorkflowActivity[];
}

export interface IWorkflowDashboardActionContext {
  getDashboardAsync: () => void;
  getRecentActivityAsync: (count?: number) => void;
}

export const INITIAL_STATE: IWorkflowDashboardStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const WorkflowDashboardStateContext =
  createContext<IWorkflowDashboardStateContext>(INITIAL_STATE);

export const WorkflowDashboardActionContext = createContext<
  IWorkflowDashboardActionContext | undefined
>(undefined);
