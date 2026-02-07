'use client'
import { createContext } from "react";
import {
  IStudentAfterCare,
  IStudentAfterCareList,
  ICreateStudentAfterCare,
  IUpdateStudentAfterCare,
  IGetStudentAfterCaresInput
} from "../shared/interfaces";

export interface IStudentAfterCareStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  studentAfterCare?: IStudentAfterCare;
  studentAfterCares?: IStudentAfterCareList[];
  totalCount?: number;
}

export interface IStudentAfterCareActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetStudentAfterCaresInput) => void;
  getByAfterCareAsync: (afterCareId: string) => void;
  getByStudentAsync: (studentId: string) => void;
  createAsync: (input: ICreateStudentAfterCare) => void;
  updateAsync: (id: string, input: IUpdateStudentAfterCare) => void;
  deleteAsync: (id: string) => void;
  suspendAsync: (id: string) => void;
  reactivateAsync: (id: string) => void;
  terminateAsync: (id: string) => void;
}

export const INITIAL_STATE: IStudentAfterCareStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const StudentAfterCareStateContext =
  createContext<IStudentAfterCareStateContext>(INITIAL_STATE);

export const StudentAfterCareActionContext = createContext<
  IStudentAfterCareActionContext | undefined
>(undefined);
