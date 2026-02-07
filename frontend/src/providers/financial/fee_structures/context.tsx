'use client'
import { createContext } from "react";
import {
  IFeeStructure,
  IFeeStructureList,
  ICreateFeeStructure,
  IUpdateFeeStructure,
  IGetFeeStructuresInput
} from "../shared/interfaces";

export interface IFeeStructureStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  feeStructure?: IFeeStructure;
  feeStructures?: IFeeStructureList[];
  totalCount?: number;
}

export interface IFeeStructureActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetFeeStructuresInput) => void;
  getByGradeAndYearAsync: (gradeId: string, academicYearId: string) => void;
  createAsync: (input: ICreateFeeStructure) => void;
  updateAsync: (id: string, input: IUpdateFeeStructure) => void;
  deleteAsync: (id: string) => void;
  activateAsync: (id: string) => void;
  deactivateAsync: (id: string) => void;
}

export const INITIAL_STATE: IFeeStructureStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const FeeStructureStateContext =
  createContext<IFeeStructureStateContext>(INITIAL_STATE);

export const FeeStructureActionContext = createContext<
  IFeeStructureActionContext | undefined
>(undefined);
