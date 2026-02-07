'use client'
import { createContext } from "react";
import {
  IGrade,
  IGradeList,
  ICreateGrade,
  IUpdateGrade,
  IPagedAndSortedResultRequest
} from "../shared/interfaces";

export interface IGradeStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  grade?: IGrade;
  grades?: IGradeList[];
  activeGrades?: IGradeList[];
  totalCount?: number;
}

export interface IGradeActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IPagedAndSortedResultRequest) => void;
  createAsync: (input: ICreateGrade) => void;
  updateAsync: (id: string, input: IUpdateGrade) => void;
  deleteAsync: (id: string) => void;
  getActiveGradesAsync: () => void;
  getByPhaseAsync: (phase: number) => void;
  activateAsync: (id: string) => void;
  deactivateAsync: (id: string) => void;
}

export const INITIAL_STATE: IGradeStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const GradeStateContext =
  createContext<IGradeStateContext>(INITIAL_STATE);

export const GradeActionContext = createContext<
  IGradeActionContext | undefined
>(undefined);
