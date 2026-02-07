'use client'
import { createContext } from "react";
import {
  IReportSubject,
  IRecordReportSubjectMarks,
  IBulkRecordReportSubjectMarks,
} from "../shared/interfaces";

export interface IReportSubjectStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  reportSubject?: IReportSubject;
  reportSubjects?: IReportSubject[];
}

export interface IReportSubjectActionContext {
  getAsync: (id: string) => void;
  getByReportAsync: (reportId: string) => void;
  recordMarksAsync: (input: IRecordReportSubjectMarks) => void;
  bulkRecordMarksAsync: (input: IBulkRecordReportSubjectMarks) => void;
  addTeacherCommentAsync: (id: string, comment: string) => void;
}

export const INITIAL_STATE: IReportSubjectStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const ReportSubjectStateContext =
  createContext<IReportSubjectStateContext>(INITIAL_STATE);

export const ReportSubjectActionContext = createContext<
  IReportSubjectActionContext | undefined
>(undefined);
