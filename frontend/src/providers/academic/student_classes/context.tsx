'use client'
import { createContext } from "react";
import {
  IStudentClass,
  IStudentClassList,
  ICreateStudentClass,
  IUpdateStudentClass
} from "../shared/interfaces";

export interface IStudentClassStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  studentClass?: IStudentClass;
  studentClasses?: IStudentClassList[];
}

export interface IStudentClassActionContext {
  getAsync: (id: string) => void;
  getByStudentAsync: (studentId: string) => void;
  getByClassAsync: (classId: string) => void;
  getCurrentByStudentAsync: (studentId: string) => void;
  enrollAsync: (input: ICreateStudentClass) => void;
  updateAsync: (id: string, input: IUpdateStudentClass) => void;
  endEnrollmentAsync: (id: string, endDate: string) => void;
  setAsCurrentAsync: (id: string) => void;
  deleteAsync: (id: string) => void;
}

export const INITIAL_STATE: IStudentClassStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const StudentClassStateContext =
  createContext<IStudentClassStateContext>(INITIAL_STATE);

export const StudentClassActionContext = createContext<
  IStudentClassActionContext | undefined
>(undefined);
