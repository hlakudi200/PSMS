import { handleActions } from "redux-actions";
import { INITIAL_STATE, IStudentStateContext } from "./context";
import { StudentActionEnums } from "./actions";

export const StudentReducer = handleActions<
  IStudentStateContext,
  IStudentStateContext
>(
  {
    [StudentActionEnums.getStudentsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentActionEnums.getStudentsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentActionEnums.getStudentsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentActionEnums.getStudentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentActionEnums.getStudentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentActionEnums.getStudentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentActionEnums.createStudentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentActionEnums.createStudentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentActionEnums.createStudentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentActionEnums.updateStudentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentActionEnums.updateStudentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentActionEnums.updateStudentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentActionEnums.deleteStudentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentActionEnums.deleteStudentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentActionEnums.deleteStudentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
