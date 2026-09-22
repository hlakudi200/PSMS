"use client";
import { createContext } from "react";

/** Mirrors backend AssessmentWeightingBand. */
export enum AssessmentWeightingBand {
  Foundation = 1,
  Intermediate = 2,
  Senior = 3,
  Grade10And11 = 4,
  Grade12 = 5,
}

/**
 * One grade band's School-Based Assessment / examination split, with the
 * national default beside it so the UI can show where a school has departed
 * from policy without holding the policy table itself.
 */
export interface IAssessmentWeighting {
  band: AssessmentWeightingBand;
  bandName: string;
  sbaPercentage: number;
  examPercentage: number;
  policySbaPercentage: number;
  policyExamPercentage: number;
  matchesPolicyDefault: boolean;
}

export interface IUpdateAssessmentWeightingItem {
  band: AssessmentWeightingBand;
  sbaPercentage: number;
  examPercentage: number;
}

export interface IAssessmentWeightingStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  weightings?: IAssessmentWeighting[];
}

export interface IAssessmentWeightingActionContext {
  getAllAsync: () => void;
  updateAsync: (weightings: IUpdateAssessmentWeightingItem[]) => void;
  resetToDefaultsAsync: () => void;
}

export const INITIAL_STATE: IAssessmentWeightingStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const AssessmentWeightingStateContext =
  createContext<IAssessmentWeightingStateContext>(INITIAL_STATE);

export const AssessmentWeightingActionContext =
  createContext<IAssessmentWeightingActionContext | undefined>(undefined);
