'use client'
import { createContext } from "react";
import {
  ISubject,
  ICreateSubject,
  IUpdateSubject,
  IPagedAndSortedResultRequest
} from "../shared/interfaces";

export interface ISubjectStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  subject?: ISubject;
  subjects?: ISubject[];
  totalCount?: number;
}

export interface ISubjectActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IPagedAndSortedResultRequest) => void;
  createAsync: (input: ICreateSubject) => void;
  updateAsync: (id: string, input: IUpdateSubject) => void;
  deleteAsync: (id: string) => void;
  activateAsync: (id: string) => void;
  deactivateAsync: (id: string) => void;
}

export const INITIAL_STATE: ISubjectStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const SubjectStateContext =
  createContext<ISubjectStateContext>(INITIAL_STATE);

export const SubjectActionContext = createContext<
  ISubjectActionContext | undefined
>(undefined);
