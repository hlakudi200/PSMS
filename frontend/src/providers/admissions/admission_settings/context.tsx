'use client'
import { createContext } from "react";
import {
  IAdmissionSettings,
  ICreateAdmissionSettings,
  IUpdateAdmissionSettings,
  ICapacityStatus,
} from "../shared/interfaces";

export interface IAdmissionSettingsStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  admissionSettings?: IAdmissionSettings;
  admissionSettingsList?: IAdmissionSettings[];
  capacityStatus?: ICapacityStatus;
}

export interface IAdmissionSettingsActionContext {
  getAsync: (id: string) => void;
  getByGradeAsync: (academicYearId: string, gradeId: string) => void;
  /** Every row, narrowed by year and grade when given. */
  getAllAsync: (academicYearId?: string, gradeId?: string) => void;
  /** What a prospective parent may apply for, and on what terms. */
  getOpenIntakesAsync: () => Promise<IOpenIntake[]>;
  getAllByAcademicYearAsync: (academicYearId: string) => void;
  createAsync: (input: ICreateAdmissionSettings) => void;
  updateAsync: (id: string, input: IUpdateAdmissionSettings) => void;
  deleteAsync: (id: string) => void;
  getCapacityStatusAsync: (academicYearId: string, gradeId: string) => void;
  openApplicationsAsync: (academicYearId: string, gradeId?: string) => void;
  closeApplicationsAsync: (academicYearId: string, gradeId?: string) => void;
  updateCapacityAsync: (academicYearId: string, gradeId: string, newCapacity: number) => void;
}

export const INITIAL_STATE: IAdmissionSettingsStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const AdmissionSettingsStateContext =
  createContext<IAdmissionSettingsStateContext>(INITIAL_STATE);

export const AdmissionSettingsActionContext = createContext<
  IAdmissionSettingsActionContext | undefined
>(undefined);


/**
 * A year and grade a prospective parent may apply for, with the terms attached.
 * Mirrors OpenIntakeDto.
 */
export interface IOpenIntake {
  academicYearId: string;
  academicYearName: string;
  gradeId?: string;
  gradeName?: string;
  applicationCloseDate?: string;
  feeRequired: boolean;
  feeAmount: number;
  isInterviewRequired: boolean;
  isAssessmentRequired: boolean;
  minimumAge?: number;
  maximumAge?: number;
  requiredDocuments?: string;
  availableSpots?: number;
}
