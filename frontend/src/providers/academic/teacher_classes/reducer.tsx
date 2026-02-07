import { handleActions } from "redux-actions";
import { INITIAL_STATE, ITeacherClassStateContext } from "./context";
import { TeacherClassActionEnums } from "./actions";

export const TeacherClassReducer = handleActions<
  ITeacherClassStateContext,
  ITeacherClassStateContext
>(
  {
    [TeacherClassActionEnums.getByTeacherPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherClassActionEnums.getByTeacherSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherClassActionEnums.getByTeacherError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherClassActionEnums.getByClassPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherClassActionEnums.getByClassSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherClassActionEnums.getByClassError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherClassActionEnums.assignPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherClassActionEnums.assignSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherClassActionEnums.assignError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherClassActionEnums.unassignPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherClassActionEnums.unassignSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherClassActionEnums.unassignError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
