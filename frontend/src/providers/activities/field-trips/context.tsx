'use client'
import { createContext } from "react";

export interface IFieldTrip {
  id: string;
  tripName: string;
  academicYearId: string;
  organizingTeacherId: string;
  organizingTeacherName: string;
  destination: string;
  tripDate: string;
  estimatedCost: number;
  approvedBudget?: number;
  numberOfStudents: number;
  numberOfChaperones: number;
  status: number;
  rejectionReason?: string;
  cancellationReason?: string;
  creationTime: string;
}

export interface IFieldTripStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  fieldTrip?: IFieldTrip;
  fieldTrips?: IFieldTrip[];
  totalCount?: number;
}

export interface IFieldTripActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: Record<string, unknown>) => void;
  createAsync: (input: Record<string, unknown>) => void;
  updateAsync: (id: string, input: Record<string, unknown>) => void;
  submitAsync: (id: string) => void;
  approveAsync: (id: string, budget: number) => void;
  rejectAsync: (id: string, reason: string) => void;
  cancelAsync: (id: string, reason: string) => void;
  completeAsync: (id: string) => void;
}

export const INITIAL_STATE: IFieldTripStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const FieldTripStateContext =
  createContext<IFieldTripStateContext>(INITIAL_STATE);

export const FieldTripActionContext = createContext<
  IFieldTripActionContext | undefined
>(undefined);
