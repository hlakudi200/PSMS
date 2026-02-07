import { createAction } from "redux-actions";
import { IPOPIAConsentStateContext } from "./context";
import { IPOPIAConsent, IPagedResult } from "../shared/interfaces";

export enum POPIAConsentActionEnums {
  getByStudentPending = "GET_POPIA_CONSENT_BY_STUDENT_PENDING",
  getByStudentSuccess = "GET_POPIA_CONSENT_BY_STUDENT_SUCCESS",
  getByStudentError = "GET_POPIA_CONSENT_BY_STUDENT_ERROR",

  getAllPending = "GET_ALL_POPIA_CONSENTS_PENDING",
  getAllSuccess = "GET_ALL_POPIA_CONSENTS_SUCCESS",
  getAllError = "GET_ALL_POPIA_CONSENTS_ERROR",

  createPending = "CREATE_POPIA_CONSENT_PENDING",
  createSuccess = "CREATE_POPIA_CONSENT_SUCCESS",
  createError = "CREATE_POPIA_CONSENT_ERROR",

  updatePending = "UPDATE_POPIA_CONSENT_PENDING",
  updateSuccess = "UPDATE_POPIA_CONSENT_SUCCESS",
  updateError = "UPDATE_POPIA_CONSENT_ERROR",

  revokePending = "REVOKE_POPIA_CONSENT_PENDING",
  revokeSuccess = "REVOKE_POPIA_CONSENT_SUCCESS",
  revokeError = "REVOKE_POPIA_CONSENT_ERROR",
}

// Get By Student Actions
export const getByStudentPending = createAction<IPOPIAConsentStateContext>(
  POPIAConsentActionEnums.getByStudentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByStudentSuccess = createAction<IPOPIAConsentStateContext, IPOPIAConsent>(
  POPIAConsentActionEnums.getByStudentSuccess,
  (popiaConsent: IPOPIAConsent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    popiaConsent,
  })
);

export const getByStudentError = createAction<IPOPIAConsentStateContext>(
  POPIAConsentActionEnums.getByStudentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All Actions
export const getAllPending = createAction<IPOPIAConsentStateContext>(
  POPIAConsentActionEnums.getAllPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllSuccess = createAction<
  IPOPIAConsentStateContext,
  IPagedResult<IPOPIAConsent>
>(
  POPIAConsentActionEnums.getAllSuccess,
  (result: IPagedResult<IPOPIAConsent>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    popiaConsents: result.items,
    totalCount: result.totalCount,
  })
);

export const getAllError = createAction<IPOPIAConsentStateContext>(
  POPIAConsentActionEnums.getAllError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Actions
export const createPending = createAction<IPOPIAConsentStateContext>(
  POPIAConsentActionEnums.createPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createSuccess = createAction<IPOPIAConsentStateContext, IPOPIAConsent>(
  POPIAConsentActionEnums.createSuccess,
  (popiaConsent: IPOPIAConsent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    popiaConsent,
  })
);

export const createError = createAction<IPOPIAConsentStateContext>(
  POPIAConsentActionEnums.createError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Actions
export const updatePending = createAction<IPOPIAConsentStateContext>(
  POPIAConsentActionEnums.updatePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateSuccess = createAction<IPOPIAConsentStateContext, IPOPIAConsent>(
  POPIAConsentActionEnums.updateSuccess,
  (popiaConsent: IPOPIAConsent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    popiaConsent,
  })
);

export const updateError = createAction<IPOPIAConsentStateContext>(
  POPIAConsentActionEnums.updateError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Revoke Actions
export const revokePending = createAction<IPOPIAConsentStateContext>(
  POPIAConsentActionEnums.revokePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const revokeSuccess = createAction<IPOPIAConsentStateContext>(
  POPIAConsentActionEnums.revokeSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const revokeError = createAction<IPOPIAConsentStateContext>(
  POPIAConsentActionEnums.revokeError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
