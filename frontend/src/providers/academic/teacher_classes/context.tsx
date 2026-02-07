'use client'
import { createContext } from "react";
import {
  ITeacherClass,
  IAssignTeacherClass
} from "../shared/interfaces";

export interface ITeacherClassStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  teacherClass?: ITeacherClass;
  teacherClasses?: ITeacherClass[];
}

export interface ITeacherClassActionContext {
  getByTeacherAsync: (teacherId: string) => void;
  getByClassAsync: (classId: string) => void;
  assignAsync: (input: IAssignTeacherClass) => void;
  unassignAsync: (id: string) => void;
}

export const INITIAL_STATE: ITeacherClassStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const TeacherClassStateContext =
  createContext<ITeacherClassStateContext>(INITIAL_STATE);

export const TeacherClassActionContext = createContext<
  ITeacherClassActionContext | undefined
>(undefined);
