'use client'
import { createContext } from "react";
import {
  IGradeSubject,
  IAssignSubjectToGrade,
  ISubject
} from "../shared/interfaces";

export interface IGradeSubjectStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  gradeSubject?: IGradeSubject;
  gradeSubjects?: IGradeSubject[];
  unassignedSubjects?: ISubject[];
}

export interface IGradeSubjectActionContext {
  getByGradeAsync: (gradeId: string) => void;
  getBySubjectAsync: (subjectId: string) => void;
  assignAsync: (input: IAssignSubjectToGrade) => void;
  unassignAsync: (id: string) => void;
  getUnassignedSubjectsForGradeAsync: (gradeId: string) => void;
}

export const INITIAL_STATE: IGradeSubjectStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const GradeSubjectStateContext =
  createContext<IGradeSubjectStateContext>(INITIAL_STATE);

export const GradeSubjectActionContext = createContext<
  IGradeSubjectActionContext | undefined
>(undefined);
