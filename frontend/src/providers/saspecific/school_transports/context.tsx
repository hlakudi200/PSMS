'use client'
import { createContext } from "react";
import {
  ISchoolTransport,
  ISchoolTransportList,
  ICreateSchoolTransport,
  IUpdateSchoolTransport,
  IGetSchoolTransportsInput
} from "../shared/interfaces";

export interface ISchoolTransportStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  schoolTransport?: ISchoolTransport;
  schoolTransports?: ISchoolTransportList[];
  totalCount?: number;
}

export interface ISchoolTransportActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetSchoolTransportsInput) => void;
  createAsync: (input: ICreateSchoolTransport) => void;
  updateAsync: (id: string, input: IUpdateSchoolTransport) => void;
  deleteAsync: (id: string) => void;
  activateAsync: (id: string) => void;
  deactivateAsync: (id: string) => void;
}

export const INITIAL_STATE: ISchoolTransportStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const SchoolTransportStateContext =
  createContext<ISchoolTransportStateContext>(INITIAL_STATE);

export const SchoolTransportActionContext = createContext<
  ISchoolTransportActionContext | undefined
>(undefined);
