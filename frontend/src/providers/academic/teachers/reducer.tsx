import { handleActions } from "redux-actions";
import { INITIAL_STATE, ITeacherStateContext } from "./context";
import { TeacherActionEnums } from "./actions";

export const TeacherReducer = handleActions<
  ITeacherStateContext,
  ITeacherStateContext
>(
  {
    [TeacherActionEnums.getTeachersPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.getTeachersSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.getTeachersError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.getTeacherPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.getTeacherSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.getTeacherError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.getCurrentTeacherPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.getCurrentTeacherSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.getCurrentTeacherError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.createTeacherPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.createTeacherSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.createTeacherError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.updateTeacherPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.updateTeacherSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.updateTeacherError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.deleteTeacherPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.deleteTeacherSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.deleteTeacherError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.activateTeacherPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.activateTeacherSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.activateTeacherError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.deactivateTeacherPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.deactivateTeacherSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [TeacherActionEnums.deactivateTeacherError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
