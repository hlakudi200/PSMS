'use client'
import { createContext } from "react";
import {
  ITeacher,
  ICreateTeacher,
  IUpdateTeacher,
  IPagedAndSortedResultRequest
} from "../shared/interfaces";

export interface ITeacherStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  teacher?: ITeacher;
  teachers?: ITeacher[];
  totalCount?: number;
}

export interface ITeacherActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IPagedAndSortedResultRequest) => void;
  createAsync: (input: ICreateTeacher) => void;
  updateAsync: (id: string, input: IUpdateTeacher) => void;
  deleteAsync: (id: string) => void;
  activateAsync: (id: string) => void;
  deactivateAsync: (id: string) => void;
}

export const INITIAL_STATE: ITeacherStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const TeacherStateContext =
  createContext<ITeacherStateContext>(INITIAL_STATE);

export const TeacherActionContext = createContext<
  ITeacherActionContext | undefined
>(undefined);
