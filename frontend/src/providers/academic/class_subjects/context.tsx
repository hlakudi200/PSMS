'use client'
import { createContext } from "react";
import {
  IClassSubject,
  IClassSubjectList,
  ICreateClassSubject,
  IUpdateClassSubject,
  IGetClassSubjectsInput
} from "../shared/interfaces";

export interface IClassSubjectStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  classSubject?: IClassSubject;
  classSubjects?: IClassSubjectList[];
  totalCount?: number;
}

export interface IClassSubjectActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetClassSubjectsInput) => void;
  getByClassAsync: (classId: string) => void;
  getByTeacherAsync: (teacherId: string) => void;
  createAsync: (input: ICreateClassSubject) => void;
  updateAsync: (id: string, input: IUpdateClassSubject) => void;
  deleteAsync: (id: string) => void;
  assignTeacherAsync: (id: string, teacherId: string) => void;
  removeTeacherAsync: (id: string) => void;
}

export const INITIAL_STATE: IClassSubjectStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const ClassSubjectStateContext =
  createContext<IClassSubjectStateContext>(INITIAL_STATE);

export const ClassSubjectActionContext = createContext<
  IClassSubjectActionContext | undefined
>(undefined);
