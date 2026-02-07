'use client'
import { createContext } from "react";
import {
  IAcademicYear,
  ICreateAcademicYear,
  IUpdateAcademicYear,
  IPagedAndSortedResultRequest
} from "../shared/interfaces";

export interface IAcademicYearStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  academicYear?: IAcademicYear;
  academicYears?: IAcademicYear[];
  totalCount?: number;
}

export interface IAcademicYearActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IPagedAndSortedResultRequest) => void;
  getCurrentAsync: () => void;
  createAsync: (input: ICreateAcademicYear) => void;
  updateAsync: (id: string, input: IUpdateAcademicYear) => void;
  deleteAsync: (id: string) => void;
  setAsCurrentAsync: (id: string) => void;
}

export const INITIAL_STATE: IAcademicYearStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const AcademicYearStateContext =
  createContext<IAcademicYearStateContext>(INITIAL_STATE);

export const AcademicYearActionContext = createContext<
  IAcademicYearActionContext | undefined
>(undefined);
