'use client'
import { createContext } from "react";
import {
  IStudentFee,
  IStudentFeeList,
  ICreateStudentFee,
  IBulkCreateStudentFees,
  IUpdateStudentFee,
  IGetStudentFeesInput
} from "../shared/interfaces";

export interface IStudentFeeStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  studentFee?: IStudentFee;
  studentFees?: IStudentFeeList[];
  totalCount?: number;
}

export interface IStudentFeeActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetStudentFeesInput) => void;
  getByStudentAsync: (studentId: string) => void;
  createAsync: (input: ICreateStudentFee) => void;
  bulkCreateAsync: (input: IBulkCreateStudentFees) => void;
  updateAsync: (id: string, input: IUpdateStudentFee) => void;
  deleteAsync: (id: string) => void;
  applyDiscountAsync: (id: string, discountAmount: number) => void;
  waiveAsync: (id: string) => void;
  cancelAsync: (id: string) => void;
  checkOverdueFeesAsync: () => void;
}

export const INITIAL_STATE: IStudentFeeStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const StudentFeeStateContext =
  createContext<IStudentFeeStateContext>(INITIAL_STATE);

export const StudentFeeActionContext = createContext<
  IStudentFeeActionContext | undefined
>(undefined);
