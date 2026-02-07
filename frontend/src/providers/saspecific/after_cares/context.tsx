'use client'
import { createContext } from "react";
import {
  IAfterCare,
  IAfterCareList,
  ICreateAfterCare,
  IUpdateAfterCare,
  IGetAfterCaresInput
} from "../shared/interfaces";

export interface IAfterCareStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  afterCare?: IAfterCare;
  afterCares?: IAfterCareList[];
  totalCount?: number;
}

export interface IAfterCareActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetAfterCaresInput) => void;
  getByAcademicYearAsync: (academicYearId: string) => void;
  createAsync: (input: ICreateAfterCare) => void;
  updateAsync: (id: string, input: IUpdateAfterCare) => void;
  deleteAsync: (id: string) => void;
  activateAsync: (id: string) => void;
  deactivateAsync: (id: string) => void;
}

export const INITIAL_STATE: IAfterCareStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const AfterCareStateContext =
  createContext<IAfterCareStateContext>(INITIAL_STATE);

export const AfterCareActionContext = createContext<
  IAfterCareActionContext | undefined
>(undefined);
