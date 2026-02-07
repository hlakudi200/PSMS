import { handleActions } from "redux-actions";
import { INITIAL_STATE, IGradeSubjectStateContext } from "./context";
import { GradeSubjectActionEnums } from "./actions";

export const GradeSubjectReducer = handleActions<
  IGradeSubjectStateContext,
  IGradeSubjectStateContext
>(
  {
    [GradeSubjectActionEnums.getByGradePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [GradeSubjectActionEnums.getByGradeSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [GradeSubjectActionEnums.getByGradeError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [GradeSubjectActionEnums.getBySubjectPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [GradeSubjectActionEnums.getBySubjectSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [GradeSubjectActionEnums.getBySubjectError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [GradeSubjectActionEnums.assignPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [GradeSubjectActionEnums.assignSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [GradeSubjectActionEnums.assignError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [GradeSubjectActionEnums.unassignPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [GradeSubjectActionEnums.unassignSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [GradeSubjectActionEnums.unassignError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [GradeSubjectActionEnums.getUnassignedSubjectsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [GradeSubjectActionEnums.getUnassignedSubjectsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [GradeSubjectActionEnums.getUnassignedSubjectsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
