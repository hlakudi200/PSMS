import { handleActions } from "redux-actions";
import { INITIAL_STATE, IDisciplinaryCaseStateContext } from "./context";
import { DisciplinaryCaseActionEnums } from "./actions";

export const DisciplinaryCaseReducer = handleActions<
  IDisciplinaryCaseStateContext,
  IDisciplinaryCaseStateContext
>(
  {
    [DisciplinaryCaseActionEnums.getCasePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.getCaseSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.getCaseError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.getCasesPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.getCasesSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.getCasesError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.createCasePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.createCaseSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.createCaseError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.updateCasePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.updateCaseSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.updateCaseError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.deleteCasePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.deleteCaseSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.deleteCaseError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.submitPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.submitSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.submitError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.startInvestigationPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.startInvestigationSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.startInvestigationError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.scheduleHearingPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.scheduleHearingSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.scheduleHearingError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.recordOutcomePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.recordOutcomeSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.recordOutcomeError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.resolvePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.resolveSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.resolveError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.cancelPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.cancelSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [DisciplinaryCaseActionEnums.cancelError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
