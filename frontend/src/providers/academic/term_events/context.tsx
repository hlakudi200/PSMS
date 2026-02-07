'use client'
import { createContext } from "react";
import {
  ITermEvent,
  ICreateTermEvent,
  IUpdateTermEvent
} from "../shared/interfaces";

export interface ITermEventStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  termEvent?: ITermEvent;
  termEvents?: ITermEvent[];
}

export interface ITermEventActionContext {
  getAsync: (id: string) => void;
  getByTermAsync: (termId: string) => void;
  getByDateRangeAsync: (startDate: string, endDate: string) => void;
  createAsync: (input: ICreateTermEvent) => void;
  updateAsync: (id: string, input: IUpdateTermEvent) => void;
  deleteAsync: (id: string) => void;
}

export const INITIAL_STATE: ITermEventStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const TermEventStateContext =
  createContext<ITermEventStateContext>(INITIAL_STATE);

export const TermEventActionContext = createContext<
  ITermEventActionContext | undefined
>(undefined);
