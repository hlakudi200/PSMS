'use client'
import { createContext } from "react";
import {
  IMark,
  IMarkList,
  IRecordMark,
  IBulkRecordMarks,
  IGetMarksInput,
} from "../shared/interfaces";

export interface IMarkStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  mark?: IMark;
  marks?: IMarkList[];
  totalCount?: number;
}

export interface IMarkActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetMarksInput) => void;
  getByAssessmentAsync: (assessmentId: string) => void;
  getByStudentAsync: (studentId: string, termId?: string) => void;
  recordMarkAsync: (input: IRecordMark) => void;
  bulkRecordMarksAsync: (input: IBulkRecordMarks) => void;
  updateMarkAsync: (id: string, input: IRecordMark) => void;
  markAsAbsentAsync: (id: string) => void;
  applyModerationAsync: (id: string, adjustment: number) => void;
  unlockMarkAsync: (id: string) => void;
  deleteAsync: (id: string) => void;
}

export const INITIAL_STATE: IMarkStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const MarkStateContext =
  createContext<IMarkStateContext>(INITIAL_STATE);

export const MarkActionContext = createContext<
  IMarkActionContext | undefined
>(undefined);
