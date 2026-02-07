'use client'
import { createContext } from "react";
import {
  IExtramuralActivity,
  IExtramuralActivityList,
  ICreateExtramuralActivity,
  IUpdateExtramuralActivity,
  IGetExtramuralActivitiesInput,
} from "../shared/interfaces";

export interface IExtramuralActivityStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  extramuralActivity?: IExtramuralActivity;
  extramuralActivities?: IExtramuralActivityList[];
  totalCount?: number;
}

export interface IExtramuralActivityActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetExtramuralActivitiesInput) => void;
  getByAcademicYearAsync: (academicYearId: string) => void;
  createAsync: (input: ICreateExtramuralActivity) => void;
  updateAsync: (id: string, input: IUpdateExtramuralActivity) => void;
  deleteAsync: (id: string) => void;
  activateAsync: (id: string) => void;
  deactivateAsync: (id: string) => void;
  openRegistrationAsync: (id: string) => void;
  closeRegistrationAsync: (id: string) => void;
}

export const INITIAL_STATE: IExtramuralActivityStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const ExtramuralActivityStateContext =
  createContext<IExtramuralActivityStateContext>(INITIAL_STATE);

export const ExtramuralActivityActionContext = createContext<
  IExtramuralActivityActionContext | undefined
>(undefined);
