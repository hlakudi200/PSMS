'use client'
import { createContext } from "react";
import {
  IStudentExtramural,
  IStudentExtramuralList,
  ICreateStudentExtramural,
  IUpdateStudentExtramural,
  IGetStudentExtramuralsInput
} from "../shared/interfaces";

export interface IStudentExtramuralStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  studentExtramural?: IStudentExtramural;
  studentExtramurals?: IStudentExtramuralList[];
  totalCount?: number;
}

export interface IStudentExtramuralActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetStudentExtramuralsInput) => void;
  getByActivityAsync: (activityId: string) => void;
  getByStudentAsync: (studentId: string) => void;
  createAsync: (input: ICreateStudentExtramural) => void;
  updateAsync: (id: string, input: IUpdateStudentExtramural) => void;
  deleteAsync: (id: string) => void;
  suspendAsync: (id: string) => void;
  reactivateAsync: (id: string) => void;
  terminateAsync: (id: string) => void;
  signConsentFormAsync: (id: string) => void;
}

export const INITIAL_STATE: IStudentExtramuralStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const StudentExtramuralStateContext =
  createContext<IStudentExtramuralStateContext>(INITIAL_STATE);

export const StudentExtramuralActionContext = createContext<
  IStudentExtramuralActionContext | undefined
>(undefined);
