'use client'
import { createContext } from "react";
import {
  IStudentTransport,
  IStudentTransportList,
  ICreateStudentTransport,
  IUpdateStudentTransport,
  IGetStudentTransportsInput
} from "../shared/interfaces";

export interface IStudentTransportStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  studentTransport?: IStudentTransport;
  studentTransports?: IStudentTransportList[];
  totalCount?: number;
}

export interface IStudentTransportActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetStudentTransportsInput) => void;
  getByTransportAsync: (transportId: string) => void;
  getByStudentAsync: (studentId: string) => void;
  createAsync: (input: ICreateStudentTransport) => void;
  updateAsync: (id: string, input: IUpdateStudentTransport) => void;
  deleteAsync: (id: string) => void;
  suspendAsync: (id: string) => void;
  reactivateAsync: (id: string) => void;
  terminateAsync: (id: string) => void;
}

export const INITIAL_STATE: IStudentTransportStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const StudentTransportStateContext =
  createContext<IStudentTransportStateContext>(INITIAL_STATE);

export const StudentTransportActionContext = createContext<
  IStudentTransportActionContext | undefined
>(undefined);
