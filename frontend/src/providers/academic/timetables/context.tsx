'use client'
import { createContext } from "react";
import {
  ITimetable,
  ITimetableList,
  ICreateTimetable,
  IUpdateTimetable,
  IPagedAndSortedResultRequest
} from "../shared/interfaces";

export interface ITimetableStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  timetable?: ITimetable;
  timetables?: ITimetableList[];
  totalCount?: number;
}

export interface ITimetableActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IPagedAndSortedResultRequest) => void;
  getByClassAsync: (classId: string) => void;
  createAsync: (input: ICreateTimetable) => void;
  updateAsync: (id: string, input: IUpdateTimetable) => void;
  deleteAsync: (id: string) => void;
  activateAsync: (id: string) => void;
  deactivateAsync: (id: string) => void;
}

export const INITIAL_STATE: ITimetableStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const TimetableStateContext =
  createContext<ITimetableStateContext>(INITIAL_STATE);

export const TimetableActionContext = createContext<
  ITimetableActionContext | undefined
>(undefined);
