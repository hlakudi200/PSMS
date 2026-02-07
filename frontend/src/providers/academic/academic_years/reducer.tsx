import { handleActions } from "redux-actions";
import { INITIAL_STATE, IAcademicYearStateContext } from "./context";
import { AcademicYearActionEnums } from "./actions";

export const AcademicYearReducer = handleActions<
  IAcademicYearStateContext,
  IAcademicYearStateContext
>(
  {
    [AcademicYearActionEnums.getAcademicYearsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.getAcademicYearsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.getAcademicYearsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.getAcademicYearPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.getAcademicYearSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.getAcademicYearError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.getCurrentAcademicYearPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.getCurrentAcademicYearSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.getCurrentAcademicYearError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.createAcademicYearPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.createAcademicYearSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.createAcademicYearError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.updateAcademicYearPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.updateAcademicYearSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.updateAcademicYearError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.deleteAcademicYearPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.deleteAcademicYearSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.deleteAcademicYearError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.setAsCurrentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.setAsCurrentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AcademicYearActionEnums.setAsCurrentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
