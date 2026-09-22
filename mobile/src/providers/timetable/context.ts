import { createContext } from "react";

export interface ITimetableSlot {
  id: string;
  /** 0 = Sunday … 6 = Saturday, matching the backend's DayOfWeek. */
  dayOfWeek: number;
  periodNumber: number;
  startTime: string;
  endTime: string;
  subjectName?: string;
  teacherName?: string;
  roomNumber?: string;
}

export interface ITimetableStateContext {
  isPending: boolean;
  isError: boolean;
  timetableId?: string;
  /** undefined = not loaded yet; [] = loaded, either no active timetable or an active timetable with no slots. */
  slots?: ITimetableSlot[];
}

export interface ITimetableActionContext {
  /** Loads the active timetable for the student's class, plus all of its slots. */
  getForClassAsync: (classId: string) => Promise<void>;
}

export const INITIAL_STATE: ITimetableStateContext = {
  isPending: false,
  isError: false,
};

export const TimetableStateContext = createContext<ITimetableStateContext>(INITIAL_STATE);
export const TimetableActionContext = createContext<ITimetableActionContext | undefined>(undefined);
