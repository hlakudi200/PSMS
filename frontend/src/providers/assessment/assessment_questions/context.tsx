'use client'
import { createContext } from "react";
import {
  IAssessmentQuestion,
  IAssessmentQuestionList,
  ICreateAssessmentQuestion,
  IUpdateAssessmentQuestion,
  IReorderQuestions,
} from "../shared/interfaces";

export interface IAssessmentQuestionStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  question?: IAssessmentQuestion;
  questions?: IAssessmentQuestionList[];
}

export interface IAssessmentQuestionActionContext {
  getAsync: (id: string) => void;
  getByAssessmentAsync: (assessmentId: string) => void;
  createAsync: (input: ICreateAssessmentQuestion) => void;
  updateAsync: (id: string, input: IUpdateAssessmentQuestion) => void;
  deleteAsync: (id: string) => void;
  reorderAsync: (input: IReorderQuestions) => void;
}

export const INITIAL_STATE: IAssessmentQuestionStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const AssessmentQuestionStateContext =
  createContext<IAssessmentQuestionStateContext>(INITIAL_STATE);

export const AssessmentQuestionActionContext = createContext<
  IAssessmentQuestionActionContext | undefined
>(undefined);
