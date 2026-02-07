import { createAction } from "redux-actions";
import { IAdmissionSettingsStateContext } from "./context";
import { IAdmissionSettings, ICapacityStatus, IListResult } from "../shared/interfaces";

export enum AdmissionSettingsActionEnums {
  // Get
  getAdmissionSettingsPending = "GET_ADMISSION_SETTINGS_PENDING",
  getAdmissionSettingsSuccess = "GET_ADMISSION_SETTINGS_SUCCESS",
  getAdmissionSettingsError = "GET_ADMISSION_SETTINGS_ERROR",

  // GetByGrade
  getAdmissionSettingsByGradePending = "GET_ADMISSION_SETTINGS_BY_GRADE_PENDING",
  getAdmissionSettingsByGradeSuccess = "GET_ADMISSION_SETTINGS_BY_GRADE_SUCCESS",
  getAdmissionSettingsByGradeError = "GET_ADMISSION_SETTINGS_BY_GRADE_ERROR",

  // GetAllByAcademicYear
  getAllByAcademicYearPending = "GET_ALL_BY_ACADEMIC_YEAR_PENDING",
  getAllByAcademicYearSuccess = "GET_ALL_BY_ACADEMIC_YEAR_SUCCESS",
  getAllByAcademicYearError = "GET_ALL_BY_ACADEMIC_YEAR_ERROR",

  // Create
  createAdmissionSettingsPending = "CREATE_ADMISSION_SETTINGS_PENDING",
  createAdmissionSettingsSuccess = "CREATE_ADMISSION_SETTINGS_SUCCESS",
  createAdmissionSettingsError = "CREATE_ADMISSION_SETTINGS_ERROR",

  // Update
  updateAdmissionSettingsPending = "UPDATE_ADMISSION_SETTINGS_PENDING",
  updateAdmissionSettingsSuccess = "UPDATE_ADMISSION_SETTINGS_SUCCESS",
  updateAdmissionSettingsError = "UPDATE_ADMISSION_SETTINGS_ERROR",

  // Delete
  deleteAdmissionSettingsPending = "DELETE_ADMISSION_SETTINGS_PENDING",
  deleteAdmissionSettingsSuccess = "DELETE_ADMISSION_SETTINGS_SUCCESS",
  deleteAdmissionSettingsError = "DELETE_ADMISSION_SETTINGS_ERROR",

  // GetCapacityStatus
  getCapacityStatusPending = "GET_CAPACITY_STATUS_PENDING",
  getCapacityStatusSuccess = "GET_CAPACITY_STATUS_SUCCESS",
  getCapacityStatusError = "GET_CAPACITY_STATUS_ERROR",

  // OpenApplications
  openApplicationsPending = "OPEN_APPLICATIONS_PENDING",
  openApplicationsSuccess = "OPEN_APPLICATIONS_SUCCESS",
  openApplicationsError = "OPEN_APPLICATIONS_ERROR",

  // CloseApplications
  closeApplicationsPending = "CLOSE_APPLICATIONS_PENDING",
  closeApplicationsSuccess = "CLOSE_APPLICATIONS_SUCCESS",
  closeApplicationsError = "CLOSE_APPLICATIONS_ERROR",

  // UpdateCapacity
  updateCapacityPending = "UPDATE_CAPACITY_PENDING",
  updateCapacitySuccess = "UPDATE_CAPACITY_SUCCESS",
  updateCapacityError = "UPDATE_CAPACITY_ERROR",
}

// Get Actions
export const getAdmissionSettingsPending = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.getAdmissionSettingsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAdmissionSettingsSuccess = createAction<IAdmissionSettingsStateContext, IAdmissionSettings>(
  AdmissionSettingsActionEnums.getAdmissionSettingsSuccess,
  (admissionSettings: IAdmissionSettings) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    admissionSettings,
  })
);

export const getAdmissionSettingsError = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.getAdmissionSettingsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetByGrade Actions
export const getAdmissionSettingsByGradePending = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.getAdmissionSettingsByGradePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAdmissionSettingsByGradeSuccess = createAction<IAdmissionSettingsStateContext, IAdmissionSettings>(
  AdmissionSettingsActionEnums.getAdmissionSettingsByGradeSuccess,
  (admissionSettings: IAdmissionSettings) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    admissionSettings,
  })
);

export const getAdmissionSettingsByGradeError = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.getAdmissionSettingsByGradeError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetAllByAcademicYear Actions
export const getAllByAcademicYearPending = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.getAllByAcademicYearPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllByAcademicYearSuccess = createAction<
  IAdmissionSettingsStateContext,
  IListResult<IAdmissionSettings>
>(
  AdmissionSettingsActionEnums.getAllByAcademicYearSuccess,
  (result: IListResult<IAdmissionSettings>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    admissionSettingsList: result.items,
  })
);

export const getAllByAcademicYearError = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.getAllByAcademicYearError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Actions
export const createAdmissionSettingsPending = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.createAdmissionSettingsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createAdmissionSettingsSuccess = createAction<IAdmissionSettingsStateContext, IAdmissionSettings>(
  AdmissionSettingsActionEnums.createAdmissionSettingsSuccess,
  (admissionSettings: IAdmissionSettings) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    admissionSettings,
  })
);

export const createAdmissionSettingsError = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.createAdmissionSettingsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Actions
export const updateAdmissionSettingsPending = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.updateAdmissionSettingsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateAdmissionSettingsSuccess = createAction<IAdmissionSettingsStateContext, IAdmissionSettings>(
  AdmissionSettingsActionEnums.updateAdmissionSettingsSuccess,
  (admissionSettings: IAdmissionSettings) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    admissionSettings,
  })
);

export const updateAdmissionSettingsError = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.updateAdmissionSettingsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Actions
export const deleteAdmissionSettingsPending = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.deleteAdmissionSettingsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteAdmissionSettingsSuccess = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.deleteAdmissionSettingsSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteAdmissionSettingsError = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.deleteAdmissionSettingsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// GetCapacityStatus Actions
export const getCapacityStatusPending = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.getCapacityStatusPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getCapacityStatusSuccess = createAction<IAdmissionSettingsStateContext, ICapacityStatus>(
  AdmissionSettingsActionEnums.getCapacityStatusSuccess,
  (capacityStatus: ICapacityStatus) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    capacityStatus,
  })
);

export const getCapacityStatusError = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.getCapacityStatusError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// OpenApplications Actions
export const openApplicationsPending = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.openApplicationsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const openApplicationsSuccess = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.openApplicationsSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const openApplicationsError = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.openApplicationsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// CloseApplications Actions
export const closeApplicationsPending = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.closeApplicationsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const closeApplicationsSuccess = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.closeApplicationsSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const closeApplicationsError = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.closeApplicationsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// UpdateCapacity Actions
export const updateCapacityPending = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.updateCapacityPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateCapacitySuccess = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.updateCapacitySuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const updateCapacityError = createAction<IAdmissionSettingsStateContext>(
  AdmissionSettingsActionEnums.updateCapacityError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
