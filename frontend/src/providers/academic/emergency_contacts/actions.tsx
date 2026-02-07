import { createAction } from "redux-actions";
import { IEmergencyContactStateContext } from "./context";
import { IEmergencyContact, IListResult } from "../shared/interfaces";

export enum EmergencyContactActionEnums {
  getEmergencyContactPending = "GET_EMERGENCY_CONTACT_PENDING",
  getEmergencyContactSuccess = "GET_EMERGENCY_CONTACT_SUCCESS",
  getEmergencyContactError = "GET_EMERGENCY_CONTACT_ERROR",

  getByStudentPending = "GET_EMERGENCY_CONTACTS_BY_STUDENT_PENDING",
  getByStudentSuccess = "GET_EMERGENCY_CONTACTS_BY_STUDENT_SUCCESS",
  getByStudentError = "GET_EMERGENCY_CONTACTS_BY_STUDENT_ERROR",

  createEmergencyContactPending = "CREATE_EMERGENCY_CONTACT_PENDING",
  createEmergencyContactSuccess = "CREATE_EMERGENCY_CONTACT_SUCCESS",
  createEmergencyContactError = "CREATE_EMERGENCY_CONTACT_ERROR",

  updateEmergencyContactPending = "UPDATE_EMERGENCY_CONTACT_PENDING",
  updateEmergencyContactSuccess = "UPDATE_EMERGENCY_CONTACT_SUCCESS",
  updateEmergencyContactError = "UPDATE_EMERGENCY_CONTACT_ERROR",

  deleteEmergencyContactPending = "DELETE_EMERGENCY_CONTACT_PENDING",
  deleteEmergencyContactSuccess = "DELETE_EMERGENCY_CONTACT_SUCCESS",
  deleteEmergencyContactError = "DELETE_EMERGENCY_CONTACT_ERROR",
}

// Get Single EmergencyContact Actions
export const getEmergencyContactPending = createAction<IEmergencyContactStateContext>(
  EmergencyContactActionEnums.getEmergencyContactPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getEmergencyContactSuccess = createAction<IEmergencyContactStateContext, IEmergencyContact>(
  EmergencyContactActionEnums.getEmergencyContactSuccess,
  (emergencyContact: IEmergencyContact) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    emergencyContact,
  })
);

export const getEmergencyContactError = createAction<IEmergencyContactStateContext>(
  EmergencyContactActionEnums.getEmergencyContactError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Student Actions
export const getByStudentPending = createAction<IEmergencyContactStateContext>(
  EmergencyContactActionEnums.getByStudentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByStudentSuccess = createAction<
  IEmergencyContactStateContext,
  IListResult<IEmergencyContact>
>(
  EmergencyContactActionEnums.getByStudentSuccess,
  (result: IListResult<IEmergencyContact>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    emergencyContacts: result.items,
  })
);

export const getByStudentError = createAction<IEmergencyContactStateContext>(
  EmergencyContactActionEnums.getByStudentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create EmergencyContact Actions
export const createEmergencyContactPending = createAction<IEmergencyContactStateContext>(
  EmergencyContactActionEnums.createEmergencyContactPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createEmergencyContactSuccess = createAction<IEmergencyContactStateContext, IEmergencyContact>(
  EmergencyContactActionEnums.createEmergencyContactSuccess,
  (emergencyContact: IEmergencyContact) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    emergencyContact,
  })
);

export const createEmergencyContactError = createAction<IEmergencyContactStateContext>(
  EmergencyContactActionEnums.createEmergencyContactError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update EmergencyContact Actions
export const updateEmergencyContactPending = createAction<IEmergencyContactStateContext>(
  EmergencyContactActionEnums.updateEmergencyContactPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateEmergencyContactSuccess = createAction<IEmergencyContactStateContext, IEmergencyContact>(
  EmergencyContactActionEnums.updateEmergencyContactSuccess,
  (emergencyContact: IEmergencyContact) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    emergencyContact,
  })
);

export const updateEmergencyContactError = createAction<IEmergencyContactStateContext>(
  EmergencyContactActionEnums.updateEmergencyContactError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete EmergencyContact Actions
export const deleteEmergencyContactPending = createAction<IEmergencyContactStateContext>(
  EmergencyContactActionEnums.deleteEmergencyContactPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteEmergencyContactSuccess = createAction<IEmergencyContactStateContext>(
  EmergencyContactActionEnums.deleteEmergencyContactSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteEmergencyContactError = createAction<IEmergencyContactStateContext>(
  EmergencyContactActionEnums.deleteEmergencyContactError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
