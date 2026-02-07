import { createAction } from "redux-actions";
import { IMedicalInfoStateContext } from "./context";
import { IMedicalInfo } from "../shared/interfaces";

export enum MedicalInfoActionEnums {
  getByStudentPending = "GET_MEDICAL_INFO_BY_STUDENT_PENDING",
  getByStudentSuccess = "GET_MEDICAL_INFO_BY_STUDENT_SUCCESS",
  getByStudentError = "GET_MEDICAL_INFO_BY_STUDENT_ERROR",

  createOrUpdatePending = "CREATE_OR_UPDATE_MEDICAL_INFO_PENDING",
  createOrUpdateSuccess = "CREATE_OR_UPDATE_MEDICAL_INFO_SUCCESS",
  createOrUpdateError = "CREATE_OR_UPDATE_MEDICAL_INFO_ERROR",

  deletePending = "DELETE_MEDICAL_INFO_PENDING",
  deleteSuccess = "DELETE_MEDICAL_INFO_SUCCESS",
  deleteError = "DELETE_MEDICAL_INFO_ERROR",
}

// Get By Student Actions
export const getByStudentPending = createAction<IMedicalInfoStateContext>(
  MedicalInfoActionEnums.getByStudentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByStudentSuccess = createAction<IMedicalInfoStateContext, IMedicalInfo>(
  MedicalInfoActionEnums.getByStudentSuccess,
  (medicalInfo: IMedicalInfo) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    medicalInfo,
  })
);

export const getByStudentError = createAction<IMedicalInfoStateContext>(
  MedicalInfoActionEnums.getByStudentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Or Update Actions
export const createOrUpdatePending = createAction<IMedicalInfoStateContext>(
  MedicalInfoActionEnums.createOrUpdatePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createOrUpdateSuccess = createAction<IMedicalInfoStateContext, IMedicalInfo>(
  MedicalInfoActionEnums.createOrUpdateSuccess,
  (medicalInfo: IMedicalInfo) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    medicalInfo,
  })
);

export const createOrUpdateError = createAction<IMedicalInfoStateContext>(
  MedicalInfoActionEnums.createOrUpdateError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Actions
export const deletePending = createAction<IMedicalInfoStateContext>(
  MedicalInfoActionEnums.deletePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteSuccess = createAction<IMedicalInfoStateContext>(
  MedicalInfoActionEnums.deleteSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteError = createAction<IMedicalInfoStateContext>(
  MedicalInfoActionEnums.deleteError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
