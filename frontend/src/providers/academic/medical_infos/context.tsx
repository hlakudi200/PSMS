'use client'
import { createContext } from "react";
import {
  IMedicalInfo,
  ICreateUpdateMedicalInfo
} from "../shared/interfaces";

export interface IMedicalInfoStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  medicalInfo?: IMedicalInfo;
}

export interface IMedicalInfoActionContext {
  getByStudentAsync: (studentId: string) => void;
  createOrUpdateAsync: (input: ICreateUpdateMedicalInfo) => void;
  deleteAsync: (studentId: string) => void;
}

export const INITIAL_STATE: IMedicalInfoStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const MedicalInfoStateContext =
  createContext<IMedicalInfoStateContext>(INITIAL_STATE);

export const MedicalInfoActionContext = createContext<
  IMedicalInfoActionContext | undefined
>(undefined);
