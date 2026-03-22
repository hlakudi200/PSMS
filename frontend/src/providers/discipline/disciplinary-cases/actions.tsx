import { createAction } from "redux-actions";
import { IDisciplinaryCaseStateContext, IDisciplinaryCase } from "./context";

export enum DisciplinaryCaseActionEnums {
  getCasePending = "GET_DISCIPLINARY_CASE_PENDING",
  getCaseSuccess = "GET_DISCIPLINARY_CASE_SUCCESS",
  getCaseError = "GET_DISCIPLINARY_CASE_ERROR",

  getCasesPending = "GET_DISCIPLINARY_CASES_PENDING",
  getCasesSuccess = "GET_DISCIPLINARY_CASES_SUCCESS",
  getCasesError = "GET_DISCIPLINARY_CASES_ERROR",

  createCasePending = "CREATE_DISCIPLINARY_CASE_PENDING",
  createCaseSuccess = "CREATE_DISCIPLINARY_CASE_SUCCESS",
  createCaseError = "CREATE_DISCIPLINARY_CASE_ERROR",

  updateCasePending = "UPDATE_DISCIPLINARY_CASE_PENDING",
  updateCaseSuccess = "UPDATE_DISCIPLINARY_CASE_SUCCESS",
  updateCaseError = "UPDATE_DISCIPLINARY_CASE_ERROR",

  deleteCasePending = "DELETE_DISCIPLINARY_CASE_PENDING",
  deleteCaseSuccess = "DELETE_DISCIPLINARY_CASE_SUCCESS",
  deleteCaseError = "DELETE_DISCIPLINARY_CASE_ERROR",

  submitPending = "SUBMIT_DISCIPLINARY_CASE_PENDING",
  submitSuccess = "SUBMIT_DISCIPLINARY_CASE_SUCCESS",
  submitError = "SUBMIT_DISCIPLINARY_CASE_ERROR",

  startInvestigationPending = "START_INVESTIGATION_PENDING",
  startInvestigationSuccess = "START_INVESTIGATION_SUCCESS",
  startInvestigationError = "START_INVESTIGATION_ERROR",

  scheduleHearingPending = "SCHEDULE_HEARING_PENDING",
  scheduleHearingSuccess = "SCHEDULE_HEARING_SUCCESS",
  scheduleHearingError = "SCHEDULE_HEARING_ERROR",

  recordOutcomePending = "RECORD_OUTCOME_PENDING",
  recordOutcomeSuccess = "RECORD_OUTCOME_SUCCESS",
  recordOutcomeError = "RECORD_OUTCOME_ERROR",

  resolvePending = "RESOLVE_DISCIPLINARY_CASE_PENDING",
  resolveSuccess = "RESOLVE_DISCIPLINARY_CASE_SUCCESS",
  resolveError = "RESOLVE_DISCIPLINARY_CASE_ERROR",

  cancelPending = "CANCEL_DISCIPLINARY_CASE_PENDING",
  cancelSuccess = "CANCEL_DISCIPLINARY_CASE_SUCCESS",
  cancelError = "CANCEL_DISCIPLINARY_CASE_ERROR",
}

// Get Single
export const getCasePending = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.getCasePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getCaseSuccess = createAction<IDisciplinaryCaseStateContext, IDisciplinaryCase>(
  DisciplinaryCaseActionEnums.getCaseSuccess,
  (disciplinaryCase: IDisciplinaryCase) => ({
    isPending: false, isSuccess: true, isError: false, disciplinaryCase,
  })
);
export const getCaseError = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.getCaseError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All
export const getCasesPending = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.getCasesPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getCasesSuccess = createAction<IDisciplinaryCaseStateContext, { items: IDisciplinaryCase[]; totalCount: number }>(
  DisciplinaryCaseActionEnums.getCasesSuccess,
  (result) => ({
    isPending: false, isSuccess: true, isError: false,
    disciplinaryCases: result.items, totalCount: result.totalCount,
  })
);
export const getCasesError = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.getCasesError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create
export const createCasePending = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.createCasePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const createCaseSuccess = createAction<IDisciplinaryCaseStateContext, IDisciplinaryCase>(
  DisciplinaryCaseActionEnums.createCaseSuccess,
  (disciplinaryCase: IDisciplinaryCase) => ({
    isPending: false, isSuccess: true, isError: false, disciplinaryCase,
  })
);
export const createCaseError = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.createCaseError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update
export const updateCasePending = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.updateCasePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const updateCaseSuccess = createAction<IDisciplinaryCaseStateContext, IDisciplinaryCase>(
  DisciplinaryCaseActionEnums.updateCaseSuccess,
  (disciplinaryCase: IDisciplinaryCase) => ({
    isPending: false, isSuccess: true, isError: false, disciplinaryCase,
  })
);
export const updateCaseError = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.updateCaseError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete
export const deleteCasePending = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.deleteCasePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const deleteCaseSuccess = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.deleteCaseSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const deleteCaseError = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.deleteCaseError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Submit
export const submitPending = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.submitPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const submitSuccess = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.submitSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const submitError = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.submitError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Start Investigation
export const startInvestigationPending = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.startInvestigationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const startInvestigationSuccess = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.startInvestigationSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const startInvestigationError = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.startInvestigationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Schedule Hearing
export const scheduleHearingPending = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.scheduleHearingPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const scheduleHearingSuccess = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.scheduleHearingSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const scheduleHearingError = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.scheduleHearingError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Record Outcome
export const recordOutcomePending = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.recordOutcomePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const recordOutcomeSuccess = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.recordOutcomeSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const recordOutcomeError = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.recordOutcomeError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Resolve
export const resolvePending = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.resolvePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const resolveSuccess = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.resolveSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const resolveError = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.resolveError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Cancel
export const cancelPending = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.cancelPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const cancelSuccess = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.cancelSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const cancelError = createAction<IDisciplinaryCaseStateContext>(
  DisciplinaryCaseActionEnums.cancelError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
