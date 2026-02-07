import { handleActions } from "redux-actions";
import { INITIAL_STATE, IGradeStateContext } from "./context";
import { GradeActionEnums } from "./actions";

export const GradeReducer = handleActions<
  IGradeStateContext,
  IGradeStateContext
>(
  {
    [GradeActionEnums.getGradePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.getGradeSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.getGradeError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.getGradesPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.getGradesSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.getGradesError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.createGradePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.createGradeSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.createGradeError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.updateGradePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.updateGradeSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.updateGradeError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.deleteGradePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.deleteGradeSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.deleteGradeError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.getActiveGradesPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.getActiveGradesSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.getActiveGradesError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.getByPhasePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.getByPhaseSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.getByPhaseError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.activatePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.activateSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.activateError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.deactivatePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.deactivateSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [GradeActionEnums.deactivateError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
