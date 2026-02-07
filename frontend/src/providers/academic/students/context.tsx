'use client'
import { createContext } from "react";
import {
  IStudent,
  ICreateStudent,
  IUpdateStudent,
  IPagedAndSortedResultRequest
} from "../shared/interfaces";

export interface IStudentStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  student?: IStudent;
  students?: IStudent[];
  totalCount?: number;
}

export interface IStudentActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IPagedAndSortedResultRequest) => void;
  createAsync: (input: ICreateStudent) => void;
  updateAsync: (id: string, input: IUpdateStudent) => void;
  deleteAsync: (id: string) => void;
}

export const INITIAL_STATE: IStudentStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const StudentStateContext =
  createContext<IStudentStateContext>(INITIAL_STATE);

export const StudentActionContext = createContext<
  IStudentActionContext | undefined
>(undefined);
