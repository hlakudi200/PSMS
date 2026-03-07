'use client'
import { createContext } from "react";
import {
  ITerm,
  ICreateTerm,
  IUpdateTerm,
  IPagedAndSortedResultRequest
} from "../shared/interfaces";

export interface ITermStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  term?: ITerm;
  terms?: ITerm[];
  totalCount?: number;
}

export interface ITermActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IPagedAndSortedResultRequest) => void;
  getByAcademicYearAsync: (academicYearId: string) => void;
  getCurrentAsync: () => void;
  createAsync: (input: ICreateTerm) => void;
  updateAsync: (id: string, input: IUpdateTerm) => void;
  deleteAsync: (id: string) => void;
  setAsCurrentAsync: (id: string) => void;
}

export const INITIAL_STATE: ITermStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const TermStateContext =
  createContext<ITermStateContext>(INITIAL_STATE);

export const TermActionContext = createContext<
  ITermActionContext | undefined
>(undefined);
