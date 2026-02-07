import { handleActions } from "redux-actions";
import { INITIAL_STATE, IAfterCareStateContext } from "./context";
import { AfterCareActionEnums } from "./actions";

export const AfterCareReducer = handleActions<
  IAfterCareStateContext,
  IAfterCareStateContext
>(
  {
    [AfterCareActionEnums.getAfterCarePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.getAfterCareSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.getAfterCareError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.getAfterCaresPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.getAfterCaresSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.getAfterCaresError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.getByAcademicYearPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.getByAcademicYearSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.getByAcademicYearError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.createAfterCarePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.createAfterCareSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.createAfterCareError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.updateAfterCarePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.updateAfterCareSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.updateAfterCareError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.deleteAfterCarePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.deleteAfterCareSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.deleteAfterCareError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.activatePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.activateSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.activateError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.deactivatePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.deactivateSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [AfterCareActionEnums.deactivateError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
