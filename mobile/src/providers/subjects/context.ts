import { createContext } from "react";

export interface IMySubject {
  subjectId: string;
  subjectName: string;
  subjectCode?: string;
  teacherName?: string;
  className?: string;
}

export interface ISubjectsStateContext {
  isPending: boolean;
  isError: boolean;
  subjects?: IMySubject[];
}

export interface ISubjectsActionContext {
  /** Loads the student's subject enrollments for the current academic year, joined with the class's teacher assignments. */
  getMySubjectsAsync: (studentId: string, classId: string) => Promise<void>;
}

export const INITIAL_STATE: ISubjectsStateContext = {
  isPending: false,
  isError: false,
};

export const SubjectsStateContext = createContext<ISubjectsStateContext>(INITIAL_STATE);
export const SubjectsActionContext = createContext<ISubjectsActionContext | undefined>(undefined);
