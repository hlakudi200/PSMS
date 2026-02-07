import { handleActions } from "redux-actions";
import { INITIAL_STATE, IStudentParentStateContext } from "./context";
import { StudentParentActionEnums } from "./actions";

export const StudentParentReducer = handleActions<
  IStudentParentStateContext,
  IStudentParentStateContext
>(
  {
    [StudentParentActionEnums.getByStudentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentParentActionEnums.getByStudentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentParentActionEnums.getByStudentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentParentActionEnums.getByParentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentParentActionEnums.getByParentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentParentActionEnums.getByParentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentParentActionEnums.linkPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentParentActionEnums.linkSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentParentActionEnums.linkError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentParentActionEnums.updateLinkPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentParentActionEnums.updateLinkSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentParentActionEnums.updateLinkError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentParentActionEnums.unlinkPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentParentActionEnums.unlinkSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentParentActionEnums.unlinkError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
