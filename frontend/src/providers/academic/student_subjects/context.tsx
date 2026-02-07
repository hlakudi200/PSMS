'use client'
import { createContext } from "react";
import {
  IStudentSubject,
  IEnrollStudentSubject
} from "../shared/interfaces";

export interface IStudentSubjectStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  studentSubject?: IStudentSubject;
  studentSubjects?: IStudentSubject[];
}

export interface IStudentSubjectActionContext {
  getByStudentAsync: (studentId: string) => void;
  getBySubjectAsync: (subjectId: string) => void;
  getByStudentAndYearAsync: (studentId: string, academicYearId: string) => void;
  enrollAsync: (input: IEnrollStudentSubject) => void;
  unenrollAsync: (id: string) => void;
}

export const INITIAL_STATE: IStudentSubjectStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const StudentSubjectStateContext =
  createContext<IStudentSubjectStateContext>(INITIAL_STATE);

export const StudentSubjectActionContext = createContext<
  IStudentSubjectActionContext | undefined
>(undefined);
