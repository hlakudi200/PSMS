'use client'
import { createContext } from "react";
import type { IPrincipalDashboardSummary } from "../shared/interfaces";

export interface IPrincipalDashboardStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  summary?: IPrincipalDashboardSummary;
}

export interface IPrincipalDashboardActionContext {
  getSummaryAsync: (academicYearId?: string) => void;
}

export const INITIAL_STATE: IPrincipalDashboardStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const PrincipalDashboardStateContext =
  createContext<IPrincipalDashboardStateContext>(INITIAL_STATE);

export const PrincipalDashboardActionContext = createContext<
  IPrincipalDashboardActionContext | undefined
>(undefined);
