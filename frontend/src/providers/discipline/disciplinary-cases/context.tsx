'use client'
import { createContext } from "react";

export interface IDisciplinaryCase {
  id: string;
  caseNumber: string;
  studentId: string;
  studentName: string;
  academicYearId: string;
  incidentDate: string;
  incidentDescription: string;
  incidentCategory: number;
  severity: number;
  status: number;
  outcome?: string;
  outcomeDescription?: string;
  reportedByName: string;
  hearingDate?: string;
  creationTime: string;
}

export interface IDisciplinaryCaseStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  disciplinaryCase?: IDisciplinaryCase;
  disciplinaryCases?: IDisciplinaryCase[];
  totalCount?: number;
}

export interface IDisciplinaryCaseActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: Record<string, unknown>) => void;
  createAsync: (input: Record<string, unknown>) => void;
  updateAsync: (id: string, input: Record<string, unknown>) => void;
  deleteAsync: (id: string) => void;
  submitAsync: (id: string) => void;
  startInvestigationAsync: (id: string) => void;
  scheduleHearingAsync: (id: string, date: string) => void;
  recordOutcomeAsync: (id: string, outcome: string, description: string) => void;
  resolveAsync: (id: string) => void;
  cancelAsync: (id: string) => void;
}

export const INITIAL_STATE: IDisciplinaryCaseStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const DisciplinaryCaseStateContext =
  createContext<IDisciplinaryCaseStateContext>(INITIAL_STATE);

export const DisciplinaryCaseActionContext = createContext<
  IDisciplinaryCaseActionContext | undefined
>(undefined);
