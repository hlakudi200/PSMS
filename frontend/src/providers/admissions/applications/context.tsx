'use client'
import { createContext } from "react";
import {
  IApplication,
  IApplicationList,
  ICreateApplication,
  IUpdateApplication,
  IGetApplicationsInput,
  IApplicationStatistics,
} from "../shared/interfaces";

export interface IApplicationStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  application?: IApplication;
  applications?: IApplicationList[];
  totalCount?: number;
  statistics?: IApplicationStatistics;
}

export interface IApplicationActionContext {
  getAsync: (id: string) => void;
  getByApplicationNumberAsync: (applicationNumber: string) => void;
  getAllAsync: (input?: IGetApplicationsInput) => void;
  createAsync: (input: ICreateApplication) => void;
  updateAsync: (id: string, input: IUpdateApplication) => void;
  deleteAsync: (id: string) => void;
  submitAsync: (id: string) => void;
  withdrawAsync: (id: string, reason?: string) => void;
  getStatisticsAsync: (academicYearId: string, gradeId?: string) => void;
}

export const INITIAL_STATE: IApplicationStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const ApplicationStateContext =
  createContext<IApplicationStateContext>(INITIAL_STATE);

export const ApplicationActionContext = createContext<
  IApplicationActionContext | undefined
>(undefined);
