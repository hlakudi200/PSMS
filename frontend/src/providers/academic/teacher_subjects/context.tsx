'use client'
import { createContext } from "react";
import {
  ITeacherSubject,
  IAssignTeacherSubject
} from "../shared/interfaces";

export interface ITeacherSubjectStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  teacherSubject?: ITeacherSubject;
  teacherSubjects?: ITeacherSubject[];
}

export interface ITeacherSubjectActionContext {
  getByTeacherAsync: (teacherId: string) => void;
  getBySubjectAsync: (subjectId: string) => void;
  getByGradeAsync: (gradeId: string) => void;
  assignAsync: (input: IAssignTeacherSubject) => void;
  unassignAsync: (id: string) => void;
}

export const INITIAL_STATE: ITeacherSubjectStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const TeacherSubjectStateContext =
  createContext<ITeacherSubjectStateContext>(INITIAL_STATE);

export const TeacherSubjectActionContext = createContext<
  ITeacherSubjectActionContext | undefined
>(undefined);
