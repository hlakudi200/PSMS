'use client'
import { createContext } from "react";
import {
  IStudentParent,
  ILinkStudentParent
} from "../shared/interfaces";

export interface IStudentParentStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  studentParent?: IStudentParent;
  studentParents?: IStudentParent[];
}

export interface IStudentParentActionContext {
  getByStudentAsync: (studentId: string) => void;
  getByParentAsync: (parentId: string) => void;
  linkAsync: (input: ILinkStudentParent) => void;
  updateLinkAsync: (id: string, input: ILinkStudentParent) => void;
  unlinkAsync: (id: string) => void;
}

export const INITIAL_STATE: IStudentParentStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const StudentParentStateContext =
  createContext<IStudentParentStateContext>(INITIAL_STATE);

export const StudentParentActionContext = createContext<
  IStudentParentActionContext | undefined
>(undefined);
