'use client'
import { createContext } from "react";
import {
  IClass,
  ICreateClass,
  IUpdateClass,
  IPagedAndSortedResultRequest
} from "../shared/interfaces";

export interface IClassStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  class?: IClass;
  classes?: IClass[];
  totalCount?: number;
}

export interface IClassActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IPagedAndSortedResultRequest) => void;
  createAsync: (input: ICreateClass) => void;
  updateAsync: (id: string, input: IUpdateClass) => void;
  deleteAsync: (id: string) => void;
  getActiveClassesAsync: () => void;
  getClassesByGradeAsync: (gradeId: string) => void;
  getClassesByAcademicYearAsync: (academicYearId: string) => void;
  assignClassTeacherAsync: (id: string, teacherId: string) => void;
  removeClassTeacherAsync: (id: string) => void;
  activateAsync: (id: string) => void;
  deactivateAsync: (id: string) => void;
}

export const INITIAL_STATE: IClassStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const ClassStateContext =
  createContext<IClassStateContext>(INITIAL_STATE);

export const ClassActionContext = createContext<
  IClassActionContext | undefined
>(undefined);
