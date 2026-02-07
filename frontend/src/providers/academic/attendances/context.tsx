'use client'
import { createContext } from "react";
import {
  IAttendance,
  IAttendanceList,
  IAttendanceSummary,
  ICaptureAttendance,
  IBulkCaptureAttendance,
  IUpdateAttendance,
  IGetAttendanceInput
} from "../shared/interfaces";

export interface IAttendanceStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  attendance?: IAttendance;
  attendances?: IAttendanceList[];
  attendanceSummary?: IAttendanceSummary;
  attendanceSummaries?: IAttendanceSummary[];
  totalCount?: number;
}

export interface IAttendanceActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetAttendanceInput) => void;
  getByStudentAsync: (studentId: string, startDate?: string, endDate?: string) => void;
  getByClassAndDateAsync: (classId: string, date: string) => void;
  captureAsync: (input: ICaptureAttendance) => void;
  bulkCaptureAsync: (input: IBulkCaptureAttendance) => void;
  updateAsync: (id: string, input: IUpdateAttendance) => void;
  deleteAsync: (id: string) => void;
  getStudentSummaryAsync: (studentId: string, startDate: string, endDate: string) => void;
  getClassSummaryAsync: (classId: string, startDate: string, endDate: string) => void;
}

export const INITIAL_STATE: IAttendanceStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const AttendanceStateContext =
  createContext<IAttendanceStateContext>(INITIAL_STATE);

export const AttendanceActionContext = createContext<
  IAttendanceActionContext | undefined
>(undefined);
