'use client'
import { createContext } from "react";
import {
  IApplicantParent,
  ICreateApplicantParent,
  IUpdateApplicantParent,
} from "../shared/interfaces";

export interface IApplicantParentStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  applicantParent?: IApplicantParent;
  applicantParents?: IApplicantParent[];
}

export interface IApplicantParentActionContext {
  getAsync: (id: string) => void;
  getAllByApplicationAsync: (applicationId: string) => void;
  createAsync: (input: ICreateApplicantParent) => void;
  updateAsync: (id: string, input: IUpdateApplicantParent) => void;
  deleteAsync: (id: string) => void;
  setAsPrimaryContactAsync: (id: string) => void;
  setAsFinanciallyResponsibleAsync: (id: string) => void;
}

export const INITIAL_STATE: IApplicantParentStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const ApplicantParentStateContext =
  createContext<IApplicantParentStateContext>(INITIAL_STATE);

export const ApplicantParentActionContext = createContext<
  IApplicantParentActionContext | undefined
>(undefined);
