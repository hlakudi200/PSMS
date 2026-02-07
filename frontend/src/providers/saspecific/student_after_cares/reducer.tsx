import { handleActions } from "redux-actions";
import { INITIAL_STATE, IStudentAfterCareStateContext } from "./context";
import { StudentAfterCareActionEnums } from "./actions";

export const StudentAfterCareReducer = handleActions<
  IStudentAfterCareStateContext,
  IStudentAfterCareStateContext
>(
  {
    [StudentAfterCareActionEnums.getStudentAfterCarePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.getStudentAfterCareSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.getStudentAfterCareError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.getAllStudentAfterCaresPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.getAllStudentAfterCaresSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.getAllStudentAfterCaresError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.getByAfterCarePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.getByAfterCareSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.getByAfterCareError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.getByStudentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.getByStudentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.getByStudentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.createStudentAfterCarePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.createStudentAfterCareSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.createStudentAfterCareError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.updateStudentAfterCarePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.updateStudentAfterCareSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.updateStudentAfterCareError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.deleteStudentAfterCarePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.deleteStudentAfterCareSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.deleteStudentAfterCareError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.suspendStudentAfterCarePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.suspendStudentAfterCareSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.suspendStudentAfterCareError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.reactivateStudentAfterCarePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.reactivateStudentAfterCareSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.reactivateStudentAfterCareError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.terminateStudentAfterCarePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.terminateStudentAfterCareSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentAfterCareActionEnums.terminateStudentAfterCareError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
