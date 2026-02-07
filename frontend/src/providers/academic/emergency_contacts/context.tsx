'use client'
import { createContext } from "react";
import { IEmergencyContact, ICreateEmergencyContact, IUpdateEmergencyContact } from "../shared/interfaces";

export interface IEmergencyContactStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  emergencyContact?: IEmergencyContact;
  emergencyContacts?: IEmergencyContact[];
}

export interface IEmergencyContactActionContext {
  getAsync: (id: string) => void;
  getByStudentAsync: (studentId: string) => void;
  createAsync: (input: ICreateEmergencyContact) => void;
  updateAsync: (id: string, input: IUpdateEmergencyContact) => void;
  deleteAsync: (id: string) => void;
}

export const INITIAL_STATE: IEmergencyContactStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const EmergencyContactStateContext = createContext<IEmergencyContactStateContext>(INITIAL_STATE);
export const EmergencyContactActionContext = createContext<IEmergencyContactActionContext | undefined>(undefined);
