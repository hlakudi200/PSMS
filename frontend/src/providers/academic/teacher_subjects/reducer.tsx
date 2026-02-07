import { handleActions } from "redux-actions";
import { INITIAL_STATE, ITeacherSubjectStateContext } from "./context";
import { TeacherSubjectActionEnums } from "./actions";

export const TeacherSubjectReducer = handleActions<
  ITeacherSubjectStateContext,
  ITeacherSubjectStateContext
>(
  {
    [TeacherSubjectActionEnums.getByTeacherPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherSubjectActionEnums.getByTeacherSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherSubjectActionEnums.getByTeacherError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherSubjectActionEnums.getBySubjectPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherSubjectActionEnums.getBySubjectSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherSubjectActionEnums.getBySubjectError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherSubjectActionEnums.getByGradePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherSubjectActionEnums.getByGradeSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherSubjectActionEnums.getByGradeError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherSubjectActionEnums.assignPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherSubjectActionEnums.assignSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherSubjectActionEnums.assignError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherSubjectActionEnums.unassignPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherSubjectActionEnums.unassignSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherSubjectActionEnums.unassignError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
