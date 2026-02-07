'use client'
import { createContext } from "react";
import {
  ITimetableSlot,
  ITimetableSlotList,
  ICreateTimetableSlot,
  IUpdateTimetableSlot
} from "../shared/interfaces";

export interface ITimetableSlotStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  timetableSlot?: ITimetableSlot;
  timetableSlots?: ITimetableSlotList[];
}

export interface ITimetableSlotActionContext {
  getAsync: (id: string) => void;
  getByTimetableAsync: (timetableId: string) => void;
  getByDayAsync: (timetableId: string, day: number) => void;
  getByTeacherAsync: (teacherId: string) => void;
  createAsync: (input: ICreateTimetableSlot) => void;
  updateAsync: (id: string, input: IUpdateTimetableSlot) => void;
  deleteAsync: (id: string) => void;
  bulkCreateAsync: (timetableId: string, input: ICreateTimetableSlot[]) => void;
}

export const INITIAL_STATE: ITimetableSlotStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const TimetableSlotStateContext =
  createContext<ITimetableSlotStateContext>(INITIAL_STATE);

export const TimetableSlotActionContext = createContext<
  ITimetableSlotActionContext | undefined
>(undefined);
