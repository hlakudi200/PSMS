'use client'
import { createContext } from "react";
import {
  IParent,
  ICreateParent,
  IUpdateParent,
  IPagedAndSortedResultRequest
} from "../shared/interfaces";

export interface IParentStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  parent?: IParent;
  parents?: IParent[];
  totalCount?: number;
}

export interface IParentActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IPagedAndSortedResultRequest) => void;
  createAsync: (input: ICreateParent) => void;
  updateAsync: (id: string, input: IUpdateParent) => void;
  deleteAsync: (id: string) => void;
}

export const INITIAL_STATE: IParentStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const ParentStateContext =
  createContext<IParentStateContext>(INITIAL_STATE);

export const ParentActionContext = createContext<
  IParentActionContext | undefined
>(undefined);
